import subprocess
import os
import math

cli = r"C:\Program Files\EffectCraft\effectcraft-cli.exe"
out_dir = r"F:\GameProject\Pale Vertica\Tools\VfxPreview"
jsx_file = os.path.join(out_dir, "test_crescent.jsx")
out_png = os.path.join(out_dir, "test_crescent.png")

def generate_crescent_points(cx, cy, r_outer, r_inner, start_deg, end_deg, num_pts=36):
    outer_pts = []
    inner_pts = []
    
    start_rad = math.radians(start_deg)
    end_rad = math.radians(end_deg)
    
    for i in range(num_pts + 1):
        t = i / num_pts
        ang = start_rad + t * (end_rad - start_rad)
        
        # Taper factor: 0 at ends, 1 in the middle
        # Asymmetric taper: head is slightly thicker and tapers faster, tail is elongated
        taper = math.sin(math.pi * t) ** 0.65
        
        thickness = (r_outer - r_inner) * taper
        r_mid = (r_outer + r_inner) * 0.5
        
        ro = r_mid + thickness * 0.5
        ri = r_mid - thickness * 0.5
        
        xo = cx + ro * math.cos(ang)
        yo = cy + ro * math.sin(ang)
        outer_pts.append([round(xo, 2), round(yo, 2)])
        
        xi = cx + ri * math.cos(ang)
        yi = cy + ri * math.sin(ang)
        inner_pts.append([round(xi, 2), round(yi, 2)])
        
    pts = outer_pts + inner_pts[::-1]
    return pts

# cx=0, cy=0 so it centers properly on the layer
pts_bloom = generate_crescent_points(0, 0, 160, 90, 210, -30, 36)
pts_main  = generate_crescent_points(0, 0, 150, 100, 205, -25, 36)
pts_core  = generate_crescent_points(0, 0, 140, 115, 200, -20, 36)

jsx = f"""
(function() {{
    var comp = app.project.items.addComp("TestCrescent", 512, 512, 1.0, 1.0, 30);
    comp.bgColor = [0, 0, 0];
    
    function addCrescentLayer(name, vertices, fillColor, opacity, blurVal, glowVal, turbAmount, turbSize) {{
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
    
    var ptsBloom = {pts_bloom};
    var ptsMain = {pts_main};
    var ptsCore = {pts_core};
    
    // 1. Wide Ambient Volumetric Bloom
    addCrescentLayer("01_Bloom", ptsBloom, [0.5, 0.5, 0.5, 1], 40, 24, 35, 16, 30);
    
    // 2. Plasma Body with Turbulent flame displacement
    addCrescentLayer("02_Plasma", ptsMain, [0.85, 0.85, 0.85, 1], 80, 6, 18, 24, 18);
    
    // 3. Hot White Core Spine (razor sharp light emission)
    addCrescentLayer("03_HotCore", ptsCore, [1.0, 1.0, 1.0, 1], 100, 1.5, 8, 5, 12);
    
    return "Crescent Created";
}})();
"""

with open(jsx_file, "w", encoding="utf-8") as f:
    f.write(jsx)

res = subprocess.run([cli, "script", jsx_file, "--save-as", os.path.join(out_dir, "test_crescent.ecproj")], capture_output=True, text=True, encoding="utf-8")
print("Script out:", res.stdout, res.stderr)

res2 = subprocess.run([cli, "render-frame", "--project", os.path.join(out_dir, "test_crescent.ecproj"), "--comp", "TestCrescent", "--time", "0", "--out", out_png, "--transparent"], capture_output=True, text=True, encoding="utf-8")
print("Render out:", res2.stdout, res2.stderr)
