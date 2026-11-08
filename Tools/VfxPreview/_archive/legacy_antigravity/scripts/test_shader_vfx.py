import numpy as np
from PIL import Image
from scipy.ndimage import gaussian_filter

SIZE = 256
cx, cy = 128.0, 128.0

y, x = np.ogrid[:SIZE, :SIZE]
dx = x - cx
dy = y - cy
r = np.hypot(dx, dy)
theta = (np.arctan2(dy, dx) + np.pi) / (2 * np.pi) # 0 to 1

# Blade arc center at radius 85, width 22
r_target = 85.0
r_dist = np.abs(r - r_target)
radial_profile = np.clip(1.0 - (r_dist / 22.0), 0.0, 1.0)
radial_profile = np.sin(radial_profile * np.pi * 0.5) ** 2

# Angular sweep: 0.2 to 0.7 (around 180 degrees)
ang_center = 0.45
ang_span = 0.25
ang_dist = (theta - (ang_center - ang_span)) / (ang_span * 2.0)
ang_mask = np.clip(ang_dist, 0.0, 1.0)
# Head is sharp, tail tapers softly
ang_profile = (np.sin(ang_mask * np.pi) ** 1.5) * (ang_mask ** 0.5)

# Generate multi-frequency procedural noise in polar space
np.random.seed(42)
noise_grid = np.random.randn(32, 64)
# Upsample / smooth noise
from scipy.ndimage import zoom
noise = zoom(noise_grid, (SIZE / 32.0, SIZE / 64.0), order=2)[:SIZE, :SIZE]

# Polar noise sample: sample noise along theta and r
u_idx = np.clip((theta * (SIZE - 1)).astype(int), 0, SIZE - 1)
v_idx = np.clip(((r / 130.0) * (SIZE - 1)).astype(int), 0, SIZE - 1)
polar_noise = noise[v_idx, u_idx]
polar_noise = (polar_noise - polar_noise.min()) / (polar_noise.max() - polar_noise.min() + 1e-5)

# Combine into raw energy
energy = radial_profile * ang_profile * (0.4 + 0.6 * polar_noise)

# Hot Core + Inner Glow + Bloom
core = np.clip((energy - 0.3) / 0.4, 0.0, 1.0) ** 2
glow = gaussian_filter(energy, sigma=4.0) * 1.5
bloom = gaussian_filter(energy, sigma=12.0) * 0.8

total_light = np.clip(core * 1.2 + glow * 0.8 + bloom * 0.4, 0.0, 1.0)
alpha = np.clip(total_light * 1.4, 0.0, 1.0)

# RGBA image (Grayscale)
rgb = (total_light * 255).astype(np.uint8)
a = (alpha * 255).astype(np.uint8)
rgba = np.stack([rgb, rgb, rgb, a], axis=-1)

im = Image.fromarray(rgba, mode="RGBA")
im.save(r"F:\GameProject\Pale Vertica\Tools\VfxPreview\test_shader_vfx.png")
print("Shader VFX rendered successfully")
