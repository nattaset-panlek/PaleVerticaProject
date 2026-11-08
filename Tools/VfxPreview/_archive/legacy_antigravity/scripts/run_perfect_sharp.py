import os
import math
import subprocess
from PIL import Image

CLI_PATH = r"C:\Program Files\EffectCraft\effectcraft-cli.exe"
TOOLS_DIR = r"F:\GameProject\Pale Vertica\Tools\VfxPreview"
out_png = os.path.join(TOOLS_DIR, "test_perfect_sharp_mask.png")
jsx_file = os.path.join(TOOLS_DIR, "test_perfect_sharp_mask.jsx")
ecproj_file = os.path.join(TOOLS_DIR, "test_perfect_sharp_mask.ecproj")

def generate_arc_polygon(cx, cy, r_outer, r_inner, start_deg, end_deg, taper_factor=0.6, num_pts=48):
    if abs(start_deg - end_deg) < 1.0:
        end_deg = start_deg - 2.0

    outer_pts = []
    inner_pts = []
    start_rad = math.radians(start_deg)
    end_rad = math.radians(end_deg)

    for i in range(num_pts + 1):
        t = i / num_pts
        ang = start_rad + t * (end_rad - start_rad)
        sin_t = math.sin(math.pi * t)
        taper = (sin_t ** taper_factor) if sin_t > 0 else 0.0

        r_mid = (r_outer + r_inner) * 0.5
        half_thick = (r_outer - r_inner) * 0.5 * taper

        ro = r_mid + half_thick
        ri = r_mid - half_thick

        xo = cx + ro * math.cos(ang)
        yo = cy + ro * math.sin(ang)
        outer_pts.append([round(xo, 2), round(yo, 2)])

        xi = cx + ri * math.cos(ang)
        yi = cy + ri * math.sin(ang)
        inner_pts.append([round(xi, 2), round(yi, 2)])

    return outer_pts + inner_pts[::-1]

# Sharp layers:
# 1. Main Razor Crescent Blade: Crisp outer silhouette, high contrast
blade_pts = generate_arc_polygon(0, 0, 94, 62, 165, 20, 0.55, 48)
# 2. Inner Flaming Wake: Modulated by Turbulent Displace for sharp flame teeth
flame_pts = generate_arc_polygon(0, 0, 82, 46, 155, 30, 0.60, 48)
# 3. Concentric Inner Speed Streak (Secondary ribbon for depth / striation)
streak_pts = generate_arc_polygon(0, 0, 68, 52, 145, 45, 0.65, 36)
# 4. White-Hot Razor Core Spine: 100% pure white, razor sharp
core_pts  = generate_arc_polygon(0, 0, 86, 76, 160, 22, 0.50, 48)

jsx = f"""
(function() {{
    var comp = app.project.items.addComp("TestSharp", 256, 256, 1.0, 1.0, 30);
    comp.bgColor = [0, 0, 0];

    function addSharpLayer(name, vertices, fillColor, opacity, turbAmt, turbSz) {{
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
        fill.property("Color").setValue(fillColor);

        var transform = layer.property("Transform");
        transform.property("Position").setValue([128, 128]);
        transform.property("Opacity").setValue(opacity);

        // ZERO Blur, ZERO Glow - Crisp mask only
        if (turbAmt > 0) {{
            var turb = layer.property("Effects").addProperty("Turbulent Displace");
            turb.property("Amount").setValue(turbAmt);
            turb.property("Size").setValue(turbSz);
            turb.property("Complexity").setValue(4);
        }}

        return layer;
    }}

    // 1. Blade Silhouette (Crisp mid-tone base, opacity 65%)
    addSharpLayer("01_BladeBody", {blade_pts}, [0.65, 0.65, 0.65, 1], 100, 0, 0);

    // 2. Inner Turbulent Flame Wake (Sharp teeth cutting into the body, opacity 85%)
    addSharpLayer("02_InnerFlames", {flame_pts}, [0.85, 0.85, 0.85, 1], 100, 28, 14);

    // 3. Secondary Inner Speed Streak (Parallel ribbon, opacity 75%)
    addSharpLayer("03_SpeedStreak", {streak_pts}, [0.75, 0.75, 0.75, 1], 100, 20, 10);

    // 4. Razor Sharp Core Spine (Pure White 100%, no blur, sharp cutting line)
    addSharpLayer("04_HotCore", {core_pts}, [1.0, 1.0, 1.0, 1], 100, 0, 0);

    return "SharpCreated";
}})();
"""

with open(jsx_file, "w", encoding="utf-8") as f:
    f.write(jsx)

subprocess.run([CLI_PATH, "script", jsx_file, "--save-as", ecproj_file], capture_output=True, text=True, encoding="utf-8")
subprocess.run([CLI_PATH, "render-frame", "--project", ecproj_file, "--comp", "TestSharp", "--time", "0", "--out", out_png, "--transparent"], capture_output=True, text=True, encoding="utf-8")
print("Rendered test_perfect_sharp_mask.png")
