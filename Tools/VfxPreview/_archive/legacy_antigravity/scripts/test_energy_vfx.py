import math
import numpy as np
from PIL import Image
from scipy.ndimage import gaussian_filter, zoom

SIZE = 512 # Compute at 512 for high-end subpixel fidelity, output at 256
cx, cy = 256.0, 256.0

y, x = np.ogrid[:SIZE, :SIZE]
dx = x - cx
dy = y - cy
r = np.hypot(dx, dy)
# Angle normalized from -pi to +pi -> 0 to 1
theta = (np.arctan2(dy, dx) + np.pi) / (2.0 * np.pi)

# Multi-octave organic noise
np.random.seed(123)
n1 = zoom(np.random.randn(16, 32), (SIZE / 16.0, SIZE / 32.0), order=2)[:SIZE, :SIZE]
n2 = zoom(np.random.randn(32, 64), (SIZE / 32.0, SIZE / 64.0), order=2)[:SIZE, :SIZE]
n3 = zoom(np.random.randn(64, 128), (SIZE / 64.0, SIZE / 128.0), order=2)[:SIZE, :SIZE]
noise_field = n1 * 0.5 + n2 * 0.35 + n3 * 0.15
noise_field = (noise_field - noise_field.min()) / (noise_field.max() - noise_field.min() + 1e-5)

# Polar mapping: U along angle, V along radius
u_idx = np.clip((theta * (SIZE - 1)).astype(int), 0, SIZE - 1)
v_idx = np.clip(((r / (SIZE * 0.5)) * (SIZE - 1)).astype(int), 0, SIZE - 1)
polar_noise = noise_field[v_idx, u_idx]

# Define Sickle Arc in Polar Space
# Radius centered at 170px, thickness 45px
r_center = 170.0
r_thick = 48.0
r_diff = np.abs(r - r_center)
r_norm = np.clip(1.0 - (r_diff / r_thick), 0.0, 1.0)
radial_energy = np.sin(r_norm * np.pi * 0.5) ** 1.8

# Angular span: counter-clockwise sweep around 180 degrees
ang_start = 0.30
ang_end = 0.85
ang_t = np.clip((theta - ang_start) / (ang_end - ang_start), 0.0, 1.0)
# Head is sharp, tail stretches with gentle falloff
ang_envelope = (np.sin(ang_t * np.pi) ** 1.2) * (ang_t ** 0.4)

# Multi-strand energy filaments (Effect texture lines along the curve)
strands = np.sin(r * 0.35 + polar_noise * 12.0) * 0.5 + 0.5
strands = strands ** 2.0

# Base Energy Density
energy = radial_energy * ang_envelope * (0.35 + 0.65 * strands)

# Apply Cellular / Organic Dissolve (Noise erosion)
dissolve_threshold = 0.28
dissolve_mask = np.clip((energy * (0.6 + 0.8 * polar_noise) - dissolve_threshold) / 0.15, 0.0, 1.0)

# Razor Sharp Leading Edge (Pure White Rim)
razor_rim = np.clip(1.0 - np.abs(r - (r_center + 12)) / 4.0, 0.0, 1.0) ** 2.5 * ang_envelope

# Combine Light Layers:
# 1. Razor Core
# 2. Filament Energy Body
# 3. Soft Volumetric Bloom
core_light = razor_rim * 1.5 + dissolve_mask * energy * 1.2
bloom_tight = gaussian_filter(core_light, sigma=4.0) * 1.4
bloom_wide = gaussian_filter(core_light, sigma=18.0) * 0.7

total_luminance = np.clip(core_light * 1.3 + bloom_tight * 0.9 + bloom_wide * 0.5, 0.0, 1.0)

# Add Piercing Needle Ray at the impact focal point
# Ray originates at (cx + 145, cy + 85), angles forward-right
px, py = cx + 130, cy + 90
ray_dir_x, ray_dir_y = 0.88, 0.47
rel_x = x - px
rel_y = y - py
proj_along = rel_x * ray_dir_x + rel_y * ray_dir_y
proj_perp = np.abs(-rel_x * ray_dir_y + rel_y * ray_dir_x)

ray_mask = (proj_along > -20) & (proj_along < 160)
ray_intensity = np.exp(-proj_perp / 3.0) * np.exp(-np.maximum(0, proj_along) / 75.0) * ray_mask
total_luminance = np.clip(total_luminance + ray_intensity * 1.6, 0.0, 1.0)

# Downsample to 256x256 with Lanczos for crystal clear antialiasing
full_rgba = np.stack([total_luminance, total_luminance, total_luminance, np.clip(total_luminance * 1.3, 0.0, 1.0)], axis=-1)
full_img = Image.fromarray((full_rgba * 255).astype(np.uint8), mode="RGBA")
out_img = full_img.resize((256, 256), Image.LANCZOS)
out_img.save(r"F:\GameProject\Pale Vertica\Tools\VfxPreview\test_energy_vfx.png")
print("Genuine Energy VFX rendered successfully!")
