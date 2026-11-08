import os
import math
from PIL import Image, ImageDraw

def get_arc_pts(cx, cy, r_out, r_in, start_deg, end_deg, num_pts=24):
    out_pts = []
    in_pts = []
    s_rad = math.radians(start_deg)
    e_rad = math.radians(end_deg)
    for i in range(num_pts + 1):
        t = i / num_pts
        ang = s_rad + t * (e_rad - s_rad)
        taper = (math.sin(math.pi * t) ** 0.6)
        r_mid = (r_out + r_in) * 0.5
        thick = (r_out - r_in) * 0.5 * taper
        ro = r_mid + thick
        ri = r_mid - thick
        out_pts.append((cx + ro * math.cos(ang), cy + ro * math.sin(ang)))
        in_pts.append((cx + ri * math.cos(ang), cy + ri * math.sin(ang)))
    return out_pts + in_pts[::-1]

test_frames = []
for f in range(16):
    t = f / 15.0
    im = Image.new("RGB", (256, 256), (15, 15, 20))
    draw = ImageDraw.Draw(im)
    
    if t < 0.25:
        p = (t / 0.25) ** 0.8
        start_ang = 165 - p * 15     # 165 -> 150
        end_ang   = 165 - p * 165    # 165 -> 0
    elif t < 0.65:
        p = (t - 0.25) / 0.40
        start_ang = 150 - p * 140    # 150 -> 10
        end_ang   = 0   - p * 120    # 0   -> -120
    else:
        p = (t - 0.65) / 0.35
        start_ang = 10  - p * 110    # 10  -> -100
        end_ang   = -120 - p * 30    # -120 -> -150
        
    pts = get_arc_pts(128, 128, 85, 55, start_ang, end_ang)
    draw.polygon(pts, fill=(240, 240, 255))
    test_frames.append(im)

test_frames[0].save(
    "F:/GameProject/Pale Vertica/Tools/VfxPreview/test_sweep_motion.gif",
    save_all=True,
    append_images=test_frames[1:],
    duration=50,
    loop=0
)
print("Saved test_sweep_motion.gif")
