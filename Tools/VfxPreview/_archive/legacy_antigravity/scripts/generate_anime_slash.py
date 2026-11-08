import math
import os
import random
from PIL import Image, ImageDraw, ImageFilter

OUTPUT_DIR = r"F:\GameProject\Pale Vertica\Tools\VfxPreview"
UNITY_VFX_DIR = r"F:\GameProject\Pale Vertica\Assets\Image\VFX"
FRAMES_DIR = os.path.join(OUTPUT_DIR, "anime_frames")

os.makedirs(FRAMES_DIR, exist_ok=True)
os.makedirs(UNITY_VFX_DIR, exist_ok=True)

FRAME_SIZE = 256
TOTAL_FRAMES = 16
FPS = 30

def bezier_point(p0, p1, p2, p3, t):
    u = 1 - t
    tt = t * t
    uu = u * u
    uuu = uu * u
    ttt = tt * t
    x = uuu * p0[0] + 3 * uu * t * p1[0] + 3 * u * tt * p2[0] + ttt * p3[0]
    y = uuu * p0[1] + 3 * uu * t * p1[1] + 3 * u * tt * p2[1] + ttt * p3[1]
    return (x, y)

def create_anime_slash_frame(frame_idx):
    im = Image.new("RGBA", (FRAME_SIZE, FRAME_SIZE), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    
    # Timing phases:
    # 0..1: Build-up spark
    # 2..5: Swift explosive arc sweep (anticipation -> peak)
    # 6..10: Impact shock + inner tooth dissolve + needle beam
    # 11..15: Breakaway debris dissipation
    
    t = frame_idx / float(TOTAL_FRAMES - 1)
    
    if frame_idx == 0:
        # Initial needle anticipation flash
        cx, cy = 120, 160
        d.line([cx - 20, cy + 20, cx + 25, cy - 25], fill=(255, 255, 255, 200), width=3)
        d.ellipse([cx - 4, cy - 4, cx + 4, cy + 4], fill=(255, 255, 255, 255))
        return im
        
    # Center of arc rotation
    cx, cy = 138, 138
    
    # Growth & Dissolve factors
    if frame_idx <= 6:
        # Growing phase
        growth = min(1.0, (frame_idx - 1) / 4.5)
        dissolve = 0.0
        opacity = min(255, int(growth * 280))
    else:
        # Dissolving phase
        growth = 1.0
        dissolve = (frame_idx - 6) / 9.0
        opacity = max(0, int(255 * (1.0 - dissolve * 1.1)))

    if opacity <= 0:
        return im

    # Geometry definition: Anime Sickle Arc (Ref: Image 1 & 3)
    # Outer radius: 88, Inner radius: 46 (Thick center, tapered tips)
    base_outer_r = 92
    base_inner_r = 50
    
    # Angular span: from start_ang to end_ang
    # Sweeps from ~-40 deg (upper right) counter-clockwise down to -220 deg (lower left)
    total_sweep = 210 * growth
    start_deg = -35 + dissolve * 65 # Tail follows through!
    end_deg = start_deg - total_sweep
    
    num_steps = 48
    outer_pts = []
    inner_pts = []
    
    for i in range(num_steps + 1):
        step_t = i / float(num_steps)
        deg = start_deg + (end_deg - start_deg) * step_t
        rad = math.radians(deg)
        
        # Profile thickness curve: 0 at tail, max at 65% (blade belly), sharp at tip
        belly = math.sin(step_t * math.pi)
        
        # Outer curve: very smooth, slight outward expansion at peak
        out_r = base_outer_r + belly * 6
        ox = cx + math.cos(rad) * out_r
        oy = cy + math.sin(rad) * out_r
        outer_pts.append((ox, oy))
        
        # Inner curve: Anime teeth / jagged wave facets (Ref: Image 3 & 4)
        # 3 distinct teeth / scalloped scoops carved out of inner edge
        tooth_freq = 3.5
        tooth_wave = math.sin(step_t * math.pi * tooth_freq)
        tooth_indent = max(0.0, tooth_wave) * (18 + 12 * dissolve)
        
        # Additional erosion cutout as dissolve increases
        erosion = 0
        if dissolve > 0:
            erosion = math.sin(step_t * math.pi * 7.0) * (14 * dissolve)
            
        in_r = base_inner_r + (1 - belly) * 38 + tooth_indent + erosion
        in_r = min(in_r, out_r - 2) # ensure positive thickness
        ix = cx + math.cos(rad) * in_r
        iy = cy + math.sin(rad) * in_r
        inner_pts.append((ix, iy))
        
    # Build complete polygon: Outer forward -> Tip Hook -> Inner backward -> Tail
    polygon_pts = outer_pts + list(reversed(inner_pts))
    
    # 1. Render Outer Soft Glow (Feathered halo)
    glow_img = Image.new("RGBA", (FRAME_SIZE, FRAME_SIZE), (0, 0, 0, 0))
    gd = ImageDraw.Draw(glow_img)
    gd.polygon(polygon_pts, fill=(200, 200, 200, int(opacity * 0.45)))
    glow_img = glow_img.filter(ImageFilter.GaussianBlur(radius=7))
    im.alpha_composite(glow_img)
    
    # 2. Render Main Body (Solid white core with gray inner grad)
    body_img = Image.new("RGBA", (FRAME_SIZE, FRAME_SIZE), (0, 0, 0, 0))
    bd = ImageDraw.Draw(body_img)
    # Inner body fill (smooth light gray)
    bd.polygon(polygon_pts, fill=(225, 230, 240, int(opacity * 0.85)))
    
    # 3. Render Crisp Outer Blade Rim (Pure White sharp razor edge + tip hook)
    rim_pts = outer_pts
    bd.line(rim_pts, fill=(255, 255, 255, opacity), width=4, joint="curve")
    
    # Hook horn at tip
    if len(outer_pts) > 2:
        tip_p = outer_pts[-1]
        tip_prev = outer_pts[-3]
        horn_dx = tip_p[0] - tip_prev[0]
        horn_dy = tip_p[1] - tip_prev[1]
        bd.line([tip_p, (tip_p[0] + horn_dx * 0.5, tip_p[1] + horn_dy * 0.5)], fill=(255, 255, 255, opacity), width=3)

    im.alpha_composite(body_img)
    
    # 4. Trailing Breakaway Debris / Diamond Crystals (Ref: Image 1 & 3)
    if frame_idx >= 3:
        random.seed(999)
        num_motes = 6
        for m in range(num_motes):
            m_t = (frame_idx - 3) / 12.0
            tail_deg = start_deg + 15 + m * 14
            tail_rad = math.radians(tail_deg)
            dist_drift = (m * 8 + 14) * (1 + m_t * 2.2)
            mx = cx + math.cos(tail_rad) * (base_inner_r + dist_drift)
            my = cy + math.sin(tail_rad) * (base_inner_r + dist_drift)
            
            m_alpha = max(0, int(255 * (1.0 - m_t * 1.2)))
            sz = (4 - m * 0.5) * (1.0 - m_t * 0.4)
            if sz > 0.5 and m_alpha > 0:
                # Diamond shape
                diamond = [(mx, my - sz), (mx + sz * 1.5, my), (mx, my + sz), (mx - sz * 1.5, my)]
                d.polygon(diamond, fill=(255, 255, 255, m_alpha))

    # 5. Impact Needle Starburst & Piercing Beam (Ref: Image 1)
    # Triggers sharply at frame 4..8
    if 3 <= frame_idx <= 9:
        imp_t = (frame_idx - 3) / 6.0
        # Impact point at the blade's cutting apex
        ix, iy = 195, 142
        
        # Long Piercing Needle Beam shooting forward-right
        beam_len = 70 * (1 - imp_t * 0.4)
        beam_alpha = int(255 * (1 - imp_t))
        rad_beam = math.radians(28) # angled needle
        bx1 = ix - math.cos(rad_beam) * (beam_len * 0.3)
        by1 = iy - math.sin(rad_beam) * (beam_len * 0.3)
        bx2 = ix + math.cos(rad_beam) * beam_len
        by2 = iy + math.sin(rad_beam) * beam_len
        
        # Tapered needle line (wide at base, ultra sharp at tip)
        d.line([bx1, by1, bx2, by2], fill=(255, 255, 255, beam_alpha), width=3)
        
        # Cross flare
        flare_sz = 20 * (1 - imp_t)
        d.line([ix - flare_sz, iy + flare_sz * 0.5, ix + flare_sz, iy - flare_sz * 0.5], fill=(255, 255, 255, beam_alpha), width=2)
        d.ellipse([ix - 4, iy - 4, ix + 4, iy + 4], fill=(255, 255, 255, beam_alpha))

    return im

def build_anime_assets():
    print("Generating Anime Stylized Slash Frames...")
    frames = []
    
    for f in range(TOTAL_FRAMES):
        frame_img = create_anime_slash_frame(f)
        frames.append(frame_img)
        frame_path = os.path.join(FRAMES_DIR, f"anime_slash_frame_{f:02d}.png")
        frame_img.save(frame_path)
        
    print("Assembling 4x4 (1024x1024) Anime Sprite Sheet...")
    GRID_COLS = 4
    GRID_ROWS = 4
    SHEET_SIZE = (GRID_COLS * FRAME_SIZE, GRID_ROWS * FRAME_SIZE)
    sheet = Image.new("RGBA", SHEET_SIZE, (0, 0, 0, 0))
    
    for idx, frm in enumerate(frames):
        c = idx % GRID_COLS
        r = idx // GRID_COLS
        sheet.paste(frm, (c * FRAME_SIZE, r * FRAME_SIZE))
        
    # Save Sprite Sheet in Tools and Unity
    out_sheet_tools = os.path.join(OUTPUT_DIR, "VFX_Anime_Slash_Sheet.png")
    out_sheet_unity = os.path.join(UNITY_VFX_DIR, "VFX_Anime_Slash_Sheet.png")
    sheet.save(out_sheet_tools)
    sheet.save(out_sheet_unity)
    print(f"Saved Sprite Sheet -> {out_sheet_unity}")
    
    # Save Animated GIF (Grayscale preview with dark BG)
    gif_frames = []
    for f in frames:
        bg = Image.new("RGBA", (FRAME_SIZE, FRAME_SIZE), (14, 16, 22, 255))
        bg.alpha_composite(f)
        gif_frames.append(bg.convert("RGB"))
        
    gif_path = os.path.join(OUTPUT_DIR, "VFX_Anime_Slash_Preview.gif")
    gif_frames[0].save(
        gif_path,
        save_all=True,
        append_images=gif_frames[1:],
        duration=int(1000 / FPS),
        loop=0
    )
    print(f"Saved Animated GIF -> {gif_path}")
    
    # Generate Stylized Cyan Tint (Ref: Image 1 & 3 Cyan style)
    cyan_frames = []
    for f in frames:
        r, g, b, a = f.split()
        # Electric Cyan: White core stays white, mid tones turn vibrant cyan (40, 210, 255)
        tr = r.point(lambda v: int(v * 0.75 + (255 if v > 200 else 40) * 0.25))
        tg = g.point(lambda v: int(v * 0.85 + (255 if v > 200 else 210) * 0.15))
        tb = b.point(lambda v: int(v * 1.0))
        tinted = Image.merge("RGBA", (tr, tg, tb, a))
        
        bg = Image.new("RGBA", (FRAME_SIZE, FRAME_SIZE), (14, 16, 22, 255))
        bg.alpha_composite(tinted)
        cyan_frames.append(bg.convert("RGB"))
        
    cyan_gif_path = os.path.join(OUTPUT_DIR, "VFX_Anime_Slash_Cyan.gif")
    cyan_frames[0].save(
        cyan_gif_path,
        save_all=True,
        append_images=cyan_frames[1:],
        duration=int(1000 / FPS),
        loop=0
    )
    print(f"Saved Cyan Demo -> {cyan_gif_path}")

if __name__ == "__main__":
    build_anime_assets()
