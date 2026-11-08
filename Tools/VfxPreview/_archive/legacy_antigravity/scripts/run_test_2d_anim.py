import os
import json
import math
import subprocess
from PIL import Image

CLI_PATH = r"C:\Program Files\EffectCraft\effectcraft-cli.exe"
TOOLS_DIR = r"F:\GameProject\Pale Vertica\Tools\VfxPreview"
out_gif = os.path.join(TOOLS_DIR, "test_2d_stylized_anim.gif")
jsx_file = os.path.join(TOOLS_DIR, "test_2d_stylized_anim.jsx")
ecproj_file = os.path.join(TOOLS_DIR, "test_2d_stylized_anim.ecproj")
frames_dir = os.path.join(TOOLS_DIR, "test_2d_frames")
os.makedirs(frames_dir, exist_ok=True)

def generate_stylized_arc(cx, cy, base_r, max_thick, start_deg, end_deg, num_pts=40):
    if abs(start_deg - end_deg) < 1.0:
        end_deg = start_deg - 2.0

    outer_pts = []
    inner_pts = []
    s_rad = math.radians(start_deg)
    e_rad = math.radians(end_deg)

    for i in range(num_pts + 1):
        t = i / num_pts
        ang = s_rad + t * (e_rad - s_rad)

        # Dynamic anime tension: slightly elliptical sweep
        elliptical_scale = 1.0 + 0.12 * math.cos(ang)
        r = base_r * elliptical_scale

        # Stylized thickness profile:
        # Taper to sharp tail at t=0, peak thickness at t=0.72, and sharp arrow head at t=1.0
        if t < 0.72:
            thick = max_thick * ((t / 0.72) ** 0.75)
        else:
            thick = max_thick * (1.0 - ((t - 0.72) / 0.28) ** 1.3)

        ro = r + thick * 0.6
        ri = r - thick * 0.4

        outer_pts.append([round(cx + ro * math.cos(ang), 2), round(cy + ro * math.sin(ang), 2)])
        inner_pts.append([round(cx + ri * math.cos(ang), 2), round(cy + ri * math.sin(ang), 2)])

    return outer_pts + inner_pts[::-1]

# 16-frame 2D stylized timing curve
SWEEP_TIMELINE = [
    # (start_deg, end_deg, opacity, thick_mult)
    (170, 45,    0.75, 0.70), # 00: Quick anticipation ignition
    (155, -20,   0.90, 0.90), # 01: Fast snap up-right
    (125, -65,   1.00, 1.00), # 02: Full swing expanding
    (85,  -115,  1.00, 1.00), # 03: Peak arc (top & right)
    (40,  -160,  1.00, 0.98), # 04: Maximum blade stretch
    (0,   -180,  1.00, 0.95), # 05: Follow-through leftward
    (-30, -195,  0.95, 0.90), # 06: Tail catching up
    (-65, -205,  0.90, 0.85), # 07: Tail moving forward
    (-95, -215,  0.85, 0.80), # 08: Speed ribbons separating
    (-120,-220,  0.75, 0.70), # 09: Strands thinning
    (-145,-225,  0.65, 0.60), # 10: Wisps eroding
    (-165,-225,  0.50, 0.50), # 11: Wispy tail dissipation
    (-180,-225,  0.38, 0.40), # 12: Faint trail
    (-195,-225,  0.25, 0.30), # 13: Thin fading wisp
    (-205,-225,  0.12, 0.20), # 14: Dissipating embers
    (-215,-225,  0.00, 0.10)  # 15: Completely gone
]

TOTAL_FRAMES = 16
FPS = 30

main_shapes  = []
outer_shapes = []
inner_shapes = []
frame_meta   = []

for f in range(TOTAL_FRAMES):
    s_ang, e_ang, op, th = SWEEP_TIMELINE[f]
    t_sec = round(f / FPS, 4)

    # Main blade (max thickness ~7px)
    m_pts = generate_stylized_arc(0, 0, 85, 7.2 * th, s_ang, e_ang, 40)

    # Detached outer speed streak (offset radius +11px, thickness ~2.2px, lags slightly)
    out_start = min(s_ang + 16, e_ang - 4)
    out_end = e_ang + 12
    o_pts = generate_stylized_arc(0, 0, 96, 2.4 * th, out_start, out_end, 32)

    # Detached inner speed streak (offset radius -11px, thickness ~2.0px)
    in_start = min(s_ang + 10, e_ang - 4)
    in_end = e_ang + 22
    i_pts = generate_stylized_arc(0, 0, 74, 2.0 * th, in_start, in_end, 32)

    main_shapes.append(m_pts)
    outer_shapes.append(o_pts)
    inner_shapes.append(i_pts)

    frame_meta.append({
        "time": t_sec,
        "opacity": round(op * 100, 1)
    })

jsx = f"""
(function() {{
    var compW = 256;
    var compH = 256;
    var fps = 30;
    var duration = 16 / fps;

    var comp = app.project.items.addComp("Test2DAnim", compW, compH, 1.0, duration, fps);
    comp.bgColor = [0, 0, 0];

    var mainShapes  = {json.dumps(main_shapes)};
    var outerShapes = {json.dumps(outer_shapes)};
    var innerShapes = {json.dumps(inner_shapes)};
    var frameMeta   = {json.dumps(frame_meta)};

    function addAnimLayer(name, shapes) {{
        var layer = comp.layers.addShape();
        layer.name = name;

        var contents = layer.property("Contents");
        var group = contents.addProperty("ADBE Vector Group");
        var gContents = group.property("Contents");

        var pathProp = gContents.addProperty("ADBE Vector Shape - Group").property("Path");

        for (var i = 0; i < 16; i++) {{
            var s = new Shape();
            s.vertices = shapes[i];
            s.closed = true;
            pathProp.setValueAtTime(frameMeta[i].time, s);
        }}

        var fill = gContents.addProperty("ADBE Vector Graphic - Fill");
        fill.property("Color").setValue([1.0, 1.0, 1.0, 1.0]); // Pure White

        var transform = layer.property("Transform");
        transform.property("Position").setValue([compW * 0.5, compH * 0.5]);

        var opProp = transform.property("Opacity");
        for (var i = 0; i < 16; i++) {{
            opProp.setValueAtTime(frameMeta[i].time, frameMeta[i].opacity);
        }}

        return layer;
    }}

    addAnimLayer("01_MainBlade", mainShapes);
    addAnimLayer("02_OuterStreak", outerShapes);
    addAnimLayer("03_InnerStreak", innerShapes);

    return "2DAnimCreated";
}})();
"""

with open(jsx_file, "w", encoding="utf-8") as f:
    f.write(jsx)

subprocess.run([CLI_PATH, "script", jsx_file, "--save-as", ecproj_file], capture_output=True, text=True, encoding="utf-8")

# Render 16 frames
frames = []
for f in range(TOTAL_FRAMES):
    t = f / 30.0
    p = os.path.join(frames_dir, f"frame_{f:02d}.png")
    subprocess.run([CLI_PATH, "render-frame", "--project", ecproj_file, "--comp", "Test2DAnim", "--time", str(t), "--out", p, "--transparent"], capture_output=True, text=True, encoding="utf-8")
    im = Image.open(p).convert("RGBA")
    bg = Image.new("RGBA", (256, 256), (12, 14, 18, 255))
    bg.alpha_composite(im)
    frames.append(bg.convert("RGB"))

frames[0].save(out_gif, save_all=True, append_images=frames[1:], duration=int(1000/30), loop=0)
print(f"Saved {out_gif}")
