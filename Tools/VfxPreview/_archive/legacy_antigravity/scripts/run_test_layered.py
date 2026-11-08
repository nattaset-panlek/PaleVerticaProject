import subprocess
import os
import math

cli = r"C:\Program Files\EffectCraft\effectcraft-cli.exe"
out_dir = r"F:\GameProject\Pale Vertica\Tools\VfxPreview"
jsx_file = os.path.join(out_dir, "test_layered_crescent.jsx")
out_png = os.path.join(out_dir, "test_layered_crescent.png")

# Let's create two custom shapes:
# 1. Main outer blade: Outer radius 150, Inner radius 110. Clean outer edge.
# 2. Inner flame wake: Outer radius 125, Inner radius 80. Displaced with turbulent flames!
# 3. Core spine: Radius 130 to 120. Thin hot white line!

def generate_arc_polygon(cx, cy, r_outer, r_inner, start_deg, end_deg, taper_outer=True, taper_inner=True, num_pts=36):
    outer_pts = []
    inner_pts = []
    
    start_rad = math.radians(start_deg)
    end_rad = math.radians(end_deg)
    
    for i in range(num_pts + 1):
        t = i / num_pts
        ang = start_rad + t * (end_rad - start_rad)
        
        # Organic taper
        t_factor = math.sin(math.pi * t)
        t_out = (t_factor ** 0.6) if taper_outer else 1.0
        t_in  = (t_factor ** 0.6) if taper_inner else 1.0
        
        r_mid = (r_outer + r_inner) * 0.5
        half_thick = (r_outer - r_inner) * 0.5
        
        ro = r_mid + half_thick * t_out
        ri = r_mid - half_thick * t_in
        
        xo = cx + ro * math.cos(ang)
        yo = cy + ro * math.sin(ang)
        outer_pts.append([round(xo, 2), round(yo, 2)])
        
        xi = cx + ri * math.cos(ang)
        yi = cy + ri * math.sin(ang)
        inner_pts.append([round(xi, 2), round(yi, 2)])
        
    return outer_pts + inner_pts[::-1]

# In screen space (Y down):
# Angle from 190 deg (bottom left) to 10 deg (bottom right / right) through 90 deg (bottom)
pts_outer_blade = generate_arc_polygon(0, 0, 160, 115, 195, 15, True, True, 40)
pts_inner_flames = generate_arc_polygon(0, 0, 135, 85, 185, 25, True, True, 40)
pts_core_spine   = generate_arc_polygon(0, 0, 142, 126, 190, 20, True, True, 40)
pts_bloom        = generate_arc_polygon(0, 0, 175, 75, 200, 10, True, True, 30)

jsx = f"""
(function() {{
    var comp = app.project.items.addComp("TestLayered", 512, 512, 1.0, 1.0, 30);
    comp.bgColor = [0, 0, 0];
    
    function addShapeLayer(name, vertices, fillColor, opacity, blurVal, glowVal, turbAmount, turbSize) {{
        var layer = comp.layers.addShape();
        layer.name = name;
        
        var contents = layer.property("Contents");
        var group = contents.addProperty("ADBE Vector Group");
        var gContents = group.property("Contents");
        
        var pathProp = gContents.addProperty("ADBE Vector Shape - Group");
        var s = new Shape();
        s.vertices = vertices;
        s.closed = true;
        pathProp.property("Path").setValue(s);
        
        var fill = gContents.addProperty("ADBE Vector Graphic - Fill");
        fill.property("Color").setValue(fillColor);
        
        layer.property("Transform").property("Position").setValue([256, 256]);
        layer.property("Transform").property("Opacity").setValue(opacity);
        
        if (turbAmount > 0) {{
            var turb = layer.property("Effects").addProperty("Turbulent Displace");
            turb.property("Amount").setValue(turbAmount);
            turb.property("Size").setValue(turbSize);
            turb.property("Complexity").setValue(4);
        }}
        
        if (blurVal > 0) {{
            var blur = layer.property("Effects").addProperty("Gaussian Blur");
            blur.property("Blurriness").setValue(blurVal);
        }}
        
        if (glowVal > 0) {{
            var glow = layer.property("Effects").addProperty("Glow");
            glow.property("Glow Radius").setValue(glowVal);
            glow.property("Glow Intensity").setValue(2.2);
        }}
        
        return layer;
    }}
    
    // 1. Wide Volumetric Atmospheric Bloom
    addShapeLayer("01_Bloom", {pts_bloom}, [0.45, 0.45, 0.45, 1], 40, 28, 45, 12, 25);
    
    // 2. Inner Flaming Wake (Turbulent fire tongues pointing inwards)
    addShapeLayer("02_InnerFlames", {pts_inner_flames}, [0.85, 0.85, 0.85, 1], 85, 4, 16, 32, 16);
    
    // 3. Crisp Cutting Blade (Clean outer boundary with slight feathering)
    addShapeLayer("03_BladeBody", {pts_outer_blade}, [0.95, 0.95, 0.95, 1], 90, 2.5, 12, 6, 20);
    
    // 4. White-Hot Energy Core Spine
    addShapeLayer("04_HotCore", {pts_core_spine}, [1.0, 1.0, 1.0, 1], 100, 1.0, 6, 0, 0);
    
    return "Layered Crescent Created";
}})();
"""

with open(jsx_file, "w", encoding="utf-8") as f:
    f.write(jsx)

res = subprocess.run([cli, "script", jsx_file, "--save-as", os.path.join(out_dir, "test_layered.ecproj")], capture_output=True, text=True, encoding="utf-8")
print("Script out:", res.stdout, res.stderr)

res2 = subprocess.run([cli, "render-frame", "--project", os.path.join(out_dir, "test_layered.ecproj"), "--comp", "TestLayered", "--time", "0", "--out", out_png, "--transparent"], capture_output=True, text=True, encoding="utf-8")
print("Render out:", res2.stdout, res2.stderr)
