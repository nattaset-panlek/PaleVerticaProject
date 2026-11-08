import os
import math
import subprocess
from PIL import Image

CLI_PATH = r"C:\Program Files\EffectCraft\effectcraft-cli.exe"
TOOLS_DIR = r"F:\GameProject\Pale Vertica\Tools\VfxPreview"
out_png = os.path.join(TOOLS_DIR, "test_thin_white.png")
jsx_file = os.path.join(TOOLS_DIR, "test_thin_white.jsx")
ecproj_file = os.path.join(TOOLS_DIR, "test_thin_white.ecproj")

def generate_thin_arc_polygon(cx, cy, r_mid, base_thickness, start_deg, end_deg, taper_factor=0.5, num_pts=48):
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

        half_thick = (base_thickness * 0.5) * taper

        ro = r_mid + half_thick
        ri = r_mid - half_thick

        xo = cx + ro * math.cos(ang)
        yo = cy + ro * math.sin(ang)
        outer_pts.append([round(xo, 2), round(yo, 2)])

        xi = cx + ri * math.cos(ang)
        yi = cy + ri * math.sin(ang)
        inner_pts.append([round(xi, 2), round(yo, 2)])

    return outer_pts + inner_pts[::-1]

# Current thickness was ~34px. 15% of 34px = 5.1px!
main_pts = generate_thin_arc_polygon(0, 0, 85, 5.1, 165, 20, 0.5, 48)
# Optional delicate secondary hairline wisp trailing inside: thickness 2.0px
wisp_pts = generate_thin_arc_polygon(0, 0, 78, 2.2, 150, 40, 0.6, 36)

jsx = f"""
(function() {{
    var comp = app.project.items.addComp("TestThinWhite", 256, 256, 1.0, 1.0, 30);
    comp.bgColor = [0, 0, 0];

    function addPureWhiteLayer(name, vertices, opacity, turbAmt, turbSz) {{
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
        fill.property("Color").setValue([1.0, 1.0, 1.0, 1.0]); // PURE WHITE ONLY

        var transform = layer.property("Transform");
        transform.property("Position").setValue([128, 128]);
        transform.property("Opacity").setValue(opacity);

        if (turbAmt > 0) {{
            var turb = layer.property("Effects").addProperty("Turbulent Displace");
            turb.property("Amount").setValue(turbAmt);
            turb.property("Size").setValue(turbSz);
            turb.property("Complexity").setValue(4);
        }}

        return layer;
    }}

    // Main razor-thin slash line (15% thickness, pure white)
    addPureWhiteLayer("01_MainSlash", {main_pts}, 100, 0, 0);

    // Inner delicate trailing wisp (pure white)
    addPureWhiteLayer("02_TrailingWisp", {wisp_pts}, 100, 15, 8);

    return "ThinWhiteCreated";
}})();
"""

with open(jsx_file, "w", encoding="utf-8") as f:
    f.write(jsx)

subprocess.run([CLI_PATH, "script", jsx_file, "--save-as", ecproj_file], capture_output=True, text=True, encoding="utf-8")
subprocess.run([CLI_PATH, "render-frame", "--project", ecproj_file, "--comp", "TestThinWhite", "--time", "0", "--out", out_png, "--transparent"], capture_output=True, text=True, encoding="utf-8")
print("Rendered test_thin_white.png")
