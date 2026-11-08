import math
import os
import numpy as np
from PIL import Image
from scipy.ndimage import gaussian_filter, zoom

OUTPUT_DIR = r"F:\GameProject\Pale Vertica\Tools\VfxPreview"
UNITY_VFX_DIR = r"F:\GameProject\Pale Vertica\Assets\Image\VFX"
FRAMES_DIR = os.path.join(OUTPUT_DIR, "energy_frames")

os.makedirs(FRAMES_DIR, exist_ok=True)
os.makedirs(UNITY_VFX_DIR, exist_ok=True)

SIZE = 512 # 2x supersampling for subpixel anti-aliasing and smooth HDR bloom
OUT_SIZE = 256
TOTAL_FRAMES = 16
FPS = 30
GRID_COLS = 4
GRID_ROWS = 4

# Precompute coordinate grids
y_grid, x_grid = np.ogrid[:SIZE, :SIZE]
cx, cy = SIZE * 0.5, SIZE * 0.5
dx = x_grid - cx
dy = y_grid - cy
r_grid = np.hypot(dx, dy)
# Theta from 0.0 to 1.0 (wrapping at -pi / +pi)
theta_grid = (np.arctan2(dy, dx) + np.pi) / (2.0 * np.pi)

# Precompute multi-octave organic noise
np.random.seed(345)
n1 = zoom(np.random.randn(16, 32), (SIZE / 16.0, SIZE / 32.0), order=2)[:SIZE, :SIZE]
n2 = zoom(np.random.randn(32, 64), (SIZE / 32.0, SIZE / 64.0), order=2)[:SIZE, :SIZE]
n3 = zoom(np.random.randn(64, 128), (SIZE / 64.0, SIZE / 128.0), order=2)[:SIZE, :SIZE]
noise_field = n1 * 0.5 + n2 * 0.35 + n3 * 0.15
noise_field = (noise_field - noise_field.min()) / (noise_field.max() - noise_field.min() + 1e-5)

def render_vfx_frame(f_idx):
    if f_idx == 0:
        # Initial needle anticipation flash
        t_img = np.zeros((SIZE, SIZE), dtype=np.float32)
        px, py = cx + 80, cy - 80
        dist = np.hypot(x_grid - px, y_grid - py)
        # Sharp cross star
        star_h = np.exp(-np.abs(y_grid - py) / 2.5) * np.exp(-np.abs(x_grid - px) / 45.0)
        star_v = np.exp(-np.abs(x_grid - px) / 2.5) * np.exp(-np.abs(y_grid - py) / 45.0)
        center_core = np.exp(-dist / 8.0) * 2.0
        glow = gaussian_filter(star_h + star_v + center_core, sigma=6.0) * 0.8
        total = np.clip(star_h + star_v + center_core + glow, 0.0, 1.0)
        
        rgba = np.stack([total, total, total, np.clip(total * 1.5, 0.0, 1.0)], axis=-1)
        full_im = Image.fromarray((rgba * 255).astype(np.uint8), mode="RGBA")
        return full_im.resize((OUT_SIZE, OUT_SIZE), Image.LANCZOS)

    # Frame timing
    # 1..6: Expanding slash swing (Anticipation -> Peak)
    # 7..11: Impact needle laser + cellular dissolve
    # 12..15: Dispersing energy motes & dissipation
    
    if f_idx <= 6:
        progress = f_idx / 6.0
        sweep_growth = min(1.0, progress * 1.3)
        dissolve_amount = 0.0
        overall_alpha = min(1.0, progress * 1.8)
    else:
        progress = 1.0
        sweep_growth = 1.0
        dissolve_amount = (f_idx - 6) / 9.0
        overall_alpha = max(0.0, 1.0 - dissolve_amount * 1.15)

    if overall_alpha <= 0.001:
        return Image.new("RGBA", (OUT_SIZE, OUT_SIZE), (0, 0, 0, 0))

    # Polar Noise Sampling with rotational evolution
    u_idx = np.clip(((theta_grid + f_idx * 0.015) % 1.0 * (SIZE - 1)).astype(int), 0, SIZE - 1)
    v_idx = np.clip(((r_grid / (SIZE * 0.5)) * (SIZE - 1)).astype(int), 0, SIZE - 1)
    p_noise = noise_field[v_idx, u_idx]

    # Slash Crescent Geometry in Polar Coordinates
    # Radius centered at 170px, thickness ~50px
    r_center = 175.0
    r_thick = 46.0
    r_diff = np.abs(r_grid - r_center)
    r_norm = np.clip(1.0 - (r_diff / r_thick), 0.0, 1.0)
    radial_energy = np.sin(r_norm * np.pi * 0.5) ** 1.6

    # Angular Sweep: starts from 0.28, sweeps counter-clockwise to ~0.88
    # Head moves forward, tail follows with delay
    max_sweep = 0.55 * sweep_growth
    ang_head = 0.88
    ang_tail = ang_head - max_sweep + (dissolve_amount * 0.25)
    
    ang_diff = np.clip((theta_grid - ang_tail) / (ang_head - ang_tail + 1e-5), 0.0, 1.0)
    # Head has sharp cutoff, tail has soft fade
    ang_envelope = (np.sin(ang_diff * np.pi) ** 1.3) * (ang_diff ** 0.45)

    # Multi-strand energy filaments (Effect texture lines along the curve)
    strands = np.sin(r_grid * 0.32 + p_noise * 14.0) * 0.5 + 0.5
    strands = strands ** 1.8

    # Base Energy Field
    raw_energy = radial_energy * ang_envelope * (0.35 + 0.65 * strands)

    # Cellular / Fluid Dissolve (Ref Image 1 & 2: organic cutouts & wisps)
    # Threshold increases as dissolve proceeds
    threshold = 0.22 + dissolve_amount * 0.45
    feather = 0.16
    dissolved_energy = np.clip((raw_energy * (0.5 + 0.9 * p_noise) - threshold) / feather, 0.0, 1.0) * raw_energy

    # Razor-sharp Leading Outer Rim (Solid pure-white cutting edge)
    razor_dist = np.abs(r_grid - (r_center + 14.0))
    razor_edge = np.clip(1.0 - (razor_dist / 3.5), 0.0, 1.0) ** 2.2 * ang_envelope * (1.0 - dissolve_amount * 0.5)

    # Hot White Core
    core_light = (razor_edge * 1.6 + dissolved_energy * 1.3) * overall_alpha

    # Multi-tier Additive Volumetric Glow & Bloom
    bloom_tight = gaussian_filter(core_light, sigma=4.0) * 1.5
    bloom_wide = gaussian_filter(core_light, sigma=16.0) * 0.75

    total_luminance = core_light * 1.2 + bloom_tight * 0.85 + bloom_wide * 0.45

    # Impact Needle Laser Beam (Shooting through impact sphere - Ref Image 1)
    # Triggers at frames 4..8
    if 3 <= f_idx <= 8:
        imp_t = (f_idx - 3) / 5.0
        # Needle origin at cutting blade head
        head_rad = ang_head * 2.0 * np.pi - np.pi
        px = cx + np.cos(head_rad) * r_center
        py = cy + np.sin(head_rad) * r_center
        
        # Ray direction: pointing forward-right
        dir_x, dir_y = 0.88, 0.47
        rel_x = x_grid - px
        rel_y = y_grid - py
        proj_along = rel_x * dir_x + rel_y * dir_y
        proj_perp = np.abs(-rel_x * dir_y + rel_y * dir_x)
        
        needle_len = 160.0 * (1.0 - imp_t * 0.3)
        ray_mask = (proj_along > -25) & (proj_along < needle_len)
        needle_int = np.exp(-proj_perp / 3.0) * np.exp(-np.maximum(0, proj_along) / 80.0) * ray_mask
        needle_glow = gaussian_filter(needle_int, sigma=5.0) * 0.9
        
        total_luminance += (needle_int * 1.8 + needle_glow) * (1.0 - imp_t * 0.6)

    # Trailing Plasma Motes / Dispersal Sparks
    if f_idx >= 3:
        np.random.seed(888)
        num_motes = 8
        mote_field = np.zeros((SIZE, SIZE), dtype=np.float32)
        for m in range(num_motes):
            m_t = (f_idx - 3) / 12.0
            mote_ang = (ang_tail + 0.05 + m * 0.035) * 2.0 * np.pi - np.pi
            mote_dist = r_center + (m * 8.0 - 15.0) * (1.0 + m_t * 2.0)
            mx = cx + np.cos(mote_ang) * mote_dist
            my = cy + np.sin(mote_ang) * mote_dist
            
            d_mote = np.hypot(x_grid - mx, y_grid - my)
            m_sz = (5.0 - m * 0.4) * (1.0 - m_t * 0.5)
            if m_sz > 0.5 and (1.0 - m_t) > 0:
                mote_field += np.exp(-d_mote / m_sz) * (1.0 - m_t) * 1.4
                
        total_luminance += mote_field

    # Clamp and compose RGBA
    total_luminance = np.clip(total_luminance, 0.0, 1.0)
    alpha = np.clip(total_luminance * 1.35, 0.0, 1.0)
    
    rgba = np.stack([total_luminance, total_luminance, total_luminance, alpha], axis=-1)
    full_im = Image.fromarray((rgba * 255).astype(np.uint8), mode="RGBA")
    
    # Supersampled 2x down to 256x256
    return full_im.resize((OUT_SIZE, OUT_SIZE), Image.LANCZOS)

def generate_vfx_assets():
    print("Synthesizing Volumetric Energy VFX Frames...")
    frames = []
    
    for f in range(TOTAL_FRAMES):
        frame_im = render_vfx_frame(f)
        frames.append(frame_im)
        frame_path = os.path.join(FRAMES_DIR, f"energy_frame_{f:02d}.png")
        frame_im.save(frame_path)
        print(f"Rendered Energy Frame {f:02d}")
        
    print("Assembling 4x4 (1024x1024) Energy Sprite Sheet...")
    sheet = Image.new("RGBA", (GRID_COLS * OUT_SIZE, GRID_ROWS * OUT_SIZE), (0, 0, 0, 0))
    for idx, frm in enumerate(frames):
        c = idx % GRID_COLS
        r = idx // GRID_COLS
        sheet.paste(frm, (c * OUT_SIZE, r * OUT_SIZE))
        
    sheet_tools = os.path.join(OUTPUT_DIR, "VFX_Energy_Slash_Sheet.png")
    sheet_unity = os.path.join(UNITY_VFX_DIR, "VFX_Energy_Slash_Sheet.png")
    sheet.save(sheet_tools)
    sheet.save(sheet_unity)
    print(f"Saved Sprite Sheet -> {sheet_unity}")
    
    # Save Grayscale Base GIF Preview
    gif_frames = []
    for f in frames:
        bg = Image.new("RGBA", (OUT_SIZE, OUT_SIZE), (12, 14, 18, 255))
        bg.alpha_composite(f)
        gif_frames.append(bg.convert("RGB"))
        
    gif_path = os.path.join(OUTPUT_DIR, "VFX_Energy_Slash_Preview.gif")
    gif_frames[0].save(
        gif_path,
        save_all=True,
        append_images=gif_frames[1:],
        duration=int(1000 / FPS),
        loop=0
    )
    print(f"Saved Animated GIF -> {gif_path}")
    
    # Save Electric Cyan Tint (Exact match for Ref Image 1 & 3)
    cyan_frames = []
    for f in frames:
        r_ch, g_ch, b_ch, a_ch = f.split()
        # Tint: Intense HDR Cyan: Core (over 200) remains bright white, outer glows into deep vibrant cyan
        r_tint = r_ch.point(lambda v: int(v * 0.65 + (255 if v > 215 else 30) * 0.35))
        g_tint = g_ch.point(lambda v: int(v * 0.85 + (255 if v > 215 else 190) * 0.15))
        b_tint = b_ch.point(lambda v: int(v * 1.0))
        tinted = Image.merge("RGBA", (r_tint, g_tint, b_tint, a_ch))
        
        bg = Image.new("RGBA", (OUT_SIZE, OUT_SIZE), (12, 14, 18, 255))
        bg.alpha_composite(tinted)
        cyan_frames.append(bg.convert("RGB"))
        
    cyan_gif_path = os.path.join(OUTPUT_DIR, "VFX_Energy_Slash_Cyan.gif")
    cyan_frames[0].save(
        cyan_gif_path,
        save_all=True,
        append_images=cyan_frames[1:],
        duration=int(1000 / FPS),
        loop=0
    )
    print(f"Saved Electric Cyan Demo -> {cyan_gif_path}")

if __name__ == "__main__":
    generate_vfx_assets()
