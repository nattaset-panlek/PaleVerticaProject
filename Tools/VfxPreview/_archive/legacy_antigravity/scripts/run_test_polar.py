import subprocess
import os

cli = r"C:\Program Files\EffectCraft\effectcraft-cli.exe"
out_dir = r"F:\GameProject\Pale Vertica\Tools\VfxPreview"
jsx_file = os.path.join(out_dir, "test_polar.jsx")
out_png = os.path.join(out_dir, "test_polar.png")

jsx = f"""
(function() {{
    var comp = app.project.items.addComp("TestPolar", 512, 512, 1.0, 1.0, 30);
    comp.bgColor = [0, 0, 0];
    
    // Add a solid
    var solid = comp.layers.addSolid([1, 1, 1], "SlashSolid", 512, 512, 1.0);
    
    // Add Turbulent Noise
    var tn = solid.property("Effects").addProperty("Turbulent Noise");
    tn.property("Contrast").setValue(300);
    tn.property("Brightness").setValue(-20);
    tn.property("Transform").property("Scale Width").setValue(400);
    tn.property("Transform").property("Scale Height").setValue(40);
    
    // Add Polar Coordinates
    var polar = solid.property("Effects").addProperty("Polar Coordinates");
    polar.property("Interpolation").setValue(100);
    polar.property("Type of Conversion").setValue(1); // Rect to Polar
    
    // Add Glow
    var glow = solid.property("Effects").addProperty("Glow");
    glow.property("Glow Radius").setValue(25);
    glow.property("Glow Intensity").setValue(2.0);
    
    return "Done";
}})();
"""

with open(jsx_file, "w", encoding="utf-8") as f:
    f.write(jsx)

res = subprocess.run([cli, "script", jsx_file, "--save-as", os.path.join(out_dir, "test_polar.ecproj")], capture_output=True, text=True, encoding="utf-8")
print("Script out:", res.stdout, res.stderr)

res2 = subprocess.run([cli, "render-frame", "--comp", "TestPolar", "--time", "0", "--out", out_png, "--transparent"], capture_output=True, text=True, encoding="utf-8")
print("Render out:", res2.stdout, res2.stderr)
