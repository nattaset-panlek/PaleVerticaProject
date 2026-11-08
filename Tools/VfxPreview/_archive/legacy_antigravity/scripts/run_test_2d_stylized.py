import os
import math
import subprocess
from PIL import Image

CLI_PATH = r"C:\Program Files\EffectCraft\effectcraft-cli.exe"
TOOLS_DIR = r"F:\GameProject\Pale Vertica\Tools\VfxPreview"
out_png = os.path.join(TOOLS_DIR, "test_2d_stylized.png")
jsx_file = os.path.join(TOOLS_DIR, "test_2d_stylized.jsx")
ecproj_file = os.path.join(TOOLS_DIR, "test_2d_stylized.ecproj")

# 2D Stylized Slash:
# An expressive anime crescent with:
# - Elliptical tension (radius stretches dynamically)
# - Stylized sharp angled head hook
# - Razor-tapered tail
# - 2 detached satellite speed lines (เส้นริ้วลมสปีดไลน์)

def generate_stylized_slash(cx, cy, base_r, max_thick, start_deg, end_deg, num_pts=48):
    outer_pts = []
    inner_pts = []
    s_rad = math.radians(start_deg)
    e_rad = math.radians(end_deg)

    for i in range(num_pts + 1):
        t = i / num_pts
        ang = s_rad + t * (e_rad - s_rad)

        # Dynamic anime curve: slightly elliptical (wider horizontally than vertically)
        elliptical_scale = 1.0 + 0.15 * math.cos(ang)
        r = base_r * elliptical_scale

        # Stylized thickness taper:
        # Tapers to 0 at tail (t=0), reaches max thickness at 75% (near head), and snaps to a sharp beak at head (t=1.0)
        if t < 0.75:
            thick = max_thick * ((t / 0.75) ** 0.8)
        else:
            # Snap head to sharp beak
            thick = max_thick * (1.0 - ((t - 0.75) / 0.25) ** 1.5)

        ro = r + thick * 0.6
        ri = r - thick * 0.4

        xo = cx + ro * math.cos(ang)
        yo = cy + ro * math.sin(ang)
        xi = cx + ri * math.cos(ang)
        yi = cy + ri * math.sin(ang)

        outer_pts.append([round(xo, 2), round(yo, 2)])
        inner_pts.append([round(xi, 2), round(yi, 2)])

    return outer_pts + inner_pts[::-1]

# Generate main stylized blade + 2 detached wind speed streaks
main_blade = generate_stylized_slash(0, 0, 85, 8.0, 170, 15, 48)
# Detached outer speed streak
outer_streak = generate_stylized_slash(0, 0, 96, 2.5, 145, 30, 36)
# Detached inner speed streak
inner_streak = generate_stylized_slash(0, 0, 74, 2.0, 160, 45, 36)

jsx = f"""
(function() {{
    var comp = app.project.items.addComp("Test2DStylized", 256, 256, 1.0, 1.0, 30);
    comp.bgColor = [0, 0, 0];

    function addStylizedLayer(name, vertices) {{
        var layer = comp.layers.addShape();
        layer.name = name;

        var contents = layer.property("Contents");
        var group = contents.addProperty("ADBE Vector Group");
        var gContents = group.property("Contents");

        var pathProp = gContents.addProperty("ADBE Vector Shape - Group").property("Path");
        var s = new Shape();
        s.vertices = vertices;
        s.closed = true;
        pathProp.setValue(s);

        var fill = gContents.addProperty("ADBE Vector Graphic - Fill");
        fill.property("Color").setValue([1.0, 1.0, 1.0, 1.0]); // Pure White

        var transform = layer.property("Transform");
        transform.property("Position").setValue([128, 128]);

        return layer;
    }}

    addStylizedLayer("01_MainBlade", {main_blade});
    addStylizedLayer("02_OuterStreak", {outer_streak});
    addStylizedLayer("03_InnerStreak", {inner_streak});

    return "2DStylizedCreated";
}})();
"""

with open(jsx_file, "w", encoding="utf-8") as f:
    f.write(jsx)

subprocess.run([CLI_PATH, "script", jsx_file, "--save-as", ecproj_file], capture_output=True, text=True, encoding="utf-8")
subprocess.run([CLI_PATH, "render-frame", "--project", ecproj_file, "--comp", "Test2DStylized", "--time", "0", "--out", out_png, "--transparent"], capture_output=True, text=True, encoding="utf-8")
print("Rendered test_2d_stylized.png")
