import os
import math
import subprocess
from PIL import Image

CLI_PATH = r"C:\Program Files\EffectCraft\effectcraft-cli.exe"
TOOLS_DIR = r"F:\GameProject\Pale Vertica\Tools\VfxPreview"
out_png = os.path.join(TOOLS_DIR, "test_sharp_mask.png")
jsx_file = os.path.join(TOOLS_DIR, "test_sharp_mask.jsx")
ecproj_file = os.path.join(TOOLS_DIR, "test_sharp_mask.ecproj")

def generate_arc_polygon(cx, cy, r_outer, r_inner, start_deg, end_deg, taper_factor=0.6, num_pts=36):
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
# 1. Main outer crescent blade (crisp, solid white)
blade_pts = generate_arc_polygon(0, 0, 92, 60, 160, 20, 0.55, 48)
# 2. Inner flame wake (jagged turbulent cuts)
flame_pts = generate_arc_polygon(0, 0, 80, 48, 150, 35, 0.60, 40)
# 3. Razor core (pure white thin spine)
core_pts  = generate_arc_polygon(0, 0, 84, 76, 155, 25, 0.50, 40)

jsx = f"""
(function() {{
    var comp = app.project.items.addComp("TestSharpMask", 256, 256, 1.0, 1.0, 30);
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

        // ZERO Blur, ZERO Glow - pure sharp mask
        if (turbAmt > 0) {{
            var turb = layer.property("Effects").addProperty("Turbulent Displace");
            turb.property("Amount").setValue(turbAmt);
            turb.property("Size").setValue(turbSz);
            turb.property("Complexity").setValue(5);
        }}

        return layer;
    }}

    // Layer 1: Base crescent body (sharp edge, no blur, no glow)
    addSharpLayer("01_BladeBody", {blade_pts}, [0.75, 0.75, 0.75, 1], 100, 0, 0);

    // Layer 2: Inner turbulent flame cuts (eroding the inner boundary into sharp tongues)
    addSharpLayer("02_InnerFlames", {flame_pts}, [0.90, 0.90, 0.90, 1], 100, 30, 14);

    // Layer 3: Razor hot core (crisp white spine)
    addSharpLayer("03_HotCore", {core_pts}, [1.0, 1.0, 1.0, 1], 100, 0, 0);

    return "SharpMaskCreated";
}})();
"""

with open(jsx_file, "w", encoding="utf-8") as f:
    f.write(jsx)

subprocess.run([CLI_PATH, "script", jsx_file, "--save-as", ecproj_file], capture_output=True, text=True, encoding="utf-8")
subprocess.run([CLI_PATH, "render-frame", "--project", ecproj_file, "--comp", "TestSharpMask", "--time", "0", "--out", out_png, "--transparent"], capture_output=True, text=True, encoding="utf-8")
print("Rendered test_sharp_mask.png")
