import os
import json
import math
import subprocess
from PIL import Image

CLI_PATH = r"C:\Program Files\EffectCraft\effectcraft-cli.exe"
TOOLS_DIR = r"F:\GameProject\Pale Vertica\Tools\VfxPreview"
out_gif = os.path.join(TOOLS_DIR, "test_forward_2d_slash.gif")
jsx_file = os.path.join(TOOLS_DIR, "test_forward_2d_slash.jsx")
ecproj_file = os.path.join(TOOLS_DIR, "test_forward_2d_slash.ecproj")
frames_dir = os.path.join(TOOLS_DIR, "test_forward_frames")
os.makedirs(frames_dir, exist_ok=True)

def generate_stylized_crescent(cx, cy, base_r, max_thick, start_deg, end_deg, num_pts=44):
    if abs(start_deg - end_deg) < 1.0:
        end_deg = start_deg + 1.5

    outer_pts = []
    inner_pts = []
    s_rad = math.radians(start_deg)
    e_rad = math.radians(end_deg)

    for i in range(num_pts + 1):
        t = i / num_pts
        ang = s_rad + t * (e_rad - s_rad)

        # Dynamic perspective stretch: slightly elliptical along the swing
        elliptical_scale = 1.0 + 0.10 * math.cos(ang - math.radians(45))
        r = base_r * elliptical_scale

        # Stylized thickness profile:
        # Needle tail at t=0, peak thickness at t=0.75, sharp hooked blade head at t=1.0
        if t < 0.75:
            thick = max_thick * ((t / 0.75) ** 0.8)
        else:
            thick = max_thick * (1.0 - ((t - 0.75) / 0.25) ** 1.4)

        ro = r + thick * 0.6
        ri = r - thick * 0.4

        outer_pts.append([round(cx + ro * math.cos(ang), 2), round(cy + ro * math.sin(ang), 2)])
        inner_pts.append([round(cx + ri * math.cos(ang), 2), round(cy + ri * math.sin(ang), 2)])

    return outer_pts + inner_pts[::-1]

# 16-frame forward swing timeline (Head leads, tail catches up)
# Total sweep from -45 deg to +140 deg
FORWARD_TIMELINE = [
    # (tail_deg, head_deg, opacity, thick_mult)
    (-45,  -15,  0.80, 0.70), # 00: Ignition at sword tip
    (-42,   45,  0.95, 0.90), # 01: Fast forward snap
    (-35,  110,  1.00, 1.00), # 02: Explosive expansion
    (-20,  140,  1.00, 1.00), # 03: Full swing apex (maximum blade length)
    ( 10,  142,  1.00, 0.95), # 04: Tail pulls forward
    ( 40,  143,  0.95, 0.88), # 05: Speed ribbons stretching
    ( 70,  143,  0.90, 0.80), # 06: Tail catching up
    ( 92,  143,  0.80, 0.70), # 07: Thinning wisps
    (110,  143,  0.70, 0.60), # 08: Dissipating arc
    (122,  143,  0.55, 0.50), # 09: Fading trail
    (130,  143,  0.40, 0.40), # 10: Fine wispy strands
    (135,  143,  0.28, 0.30), # 11: Faint residue
    (138,  143,  0.18, 0.20), # 12: Whispers of light
    (140,  143,  0.10, 0.15), # 13: Embers vanishing
    (141,  143,  0.04, 0.10), # 14: Final point
    (142,  143,  0.00, 0.05)  # 15: Blank frame for clean loop
]

TOTAL_FRAMES = 16
FPS = 30

main_shapes  = []
outer_shapes = []
inner_shapes = []
frame_meta   = []

for f in range(TOTAL_FRAMES):
    tail_ang, head_ang, op, th = FORWARD_TIMELINE[f]
    t_sec = round(f / FPS, 4)

    # 1. Main Blade (max thickness ~6.8px * th, 15% thickness)
    m_pts = generate_stylized_crescent(0, 0, 85, 6.8 * th, tail_ang, head_ang, 44)

    # 2. Outer Speed Streak (radius +11px, thickness ~2.2px, leading the blade slightly)
    out_tail = tail_ang + (head_ang - tail_ang) * 0.30
    out_head = head_ang + 6
    o_pts = generate_stylized_crescent(0, 0, 96, 2.2 * th, out_tail, out_head, 32)

    # 3. Inner Speed Streak (radius -11px, thickness ~1.8px, trailing the blade inside)
    in_tail = tail_ang + (head_ang - tail_ang) * 0.15
    in_head = head_ang - 8
    i_pts = generate_stylized_crescent(0, 0, 74, 1.8 * th, in_tail, in_head, 32)

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

    var comp = app.project.items.addComp("TestForward2D", compW, compH, 1.0, duration, fps);
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

        // Pure White Solid Fill, Zero Blur, Zero Glow
        var fill = gContents.addProperty("ADBE Vector Graphic - Fill");
        fill.property("Color").setValue([1.0, 1.0, 1.0, 1.0]);

        var transform = layer.property("Transform");
        transform.property("Position").setValue([compW * 0.5, compH * 0.5 - 25]);

        var opProp = transform.property("Opacity");
        for (var i = 0; i < 16; i++) {{
            opProp.setValueAtTime(frameMeta[i].time, frameMeta[i].opacity);
        }}

        return layer;
    }}

    addAnimLayer("01_MainBlade", mainShapes);
    addAnimLayer("02_OuterStreak", outerShapes);
    addAnimLayer("03_InnerStreak", innerShapes);

    return "Forward2DCreated";
}})();
"""

with open(jsx_file, "w", encoding="utf-8") as f:
    f.write(jsx)

subprocess.run([CLI_PATH, "script", jsx_file, "--save-as", ecproj_file], capture_output=True, text=True, encoding="utf-8")

frames = []
for f in range(TOTAL_FRAMES):
    t = f / 30.0
    p = os.path.join(frames_dir, f"frame_{f:02d}.png")
    subprocess.run([CLI_PATH, "render-frame", "--project", ecproj_file, "--comp", "TestForward2D", "--time", str(t), "--out", p, "--transparent"], capture_output=True, text=True, encoding="utf-8")
    im = Image.open(p).convert("RGBA")
    bg = Image.new("RGBA", (256, 256), (12, 14, 18, 255))
    bg.alpha_composite(im)
    frames.append(bg.convert("RGB"))

frames[0].save(out_gif, save_all=True, append_images=frames[1:], duration=int(1000/30), loop=0)
print(f"Saved {out_gif}")
