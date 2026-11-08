import subprocess
import os

cli = r"C:\Program Files\EffectCraft\effectcraft-cli.exe"
out_dir = r"F:\GameProject\Pale Vertica\Tools\VfxPreview"
jsx_file = os.path.join(out_dir, "test_shape_anim.jsx")

jsx = f"""
(function() {{
    var comp = app.project.items.addComp("TestAnim", 256, 256, 1.0, 1.0, 30);
    var layer = comp.layers.addShape();
    var contents = layer.property("Contents");
    var group = contents.addProperty("ADBE Vector Group");
    var pathProp = group.property("Contents").addProperty("ADBE Vector Shape - Group").property("Path");
    
    var s1 = new Shape();
    s1.vertices = [[-50, -50], [50, -50], [0, 50]];
    s1.closed = true;
    
    var s2 = new Shape();
    s2.vertices = [[-80, -20], [80, -20], [0, 80]];
    s2.closed = true;
    
    pathProp.setValueAtTime(0.0, s1);
    pathProp.setValueAtTime(0.5, s2);
    
    return "Keyframed successfully";
}})();
"""

with open(jsx_file, "w", encoding="utf-8") as f:
    f.write(jsx)

res = subprocess.run([cli, "script", jsx_file, "--save-as", os.path.join(out_dir, "test_anim.ecproj")], capture_output=True, text=True, encoding="utf-8")
print("Script out:", res.stdout, res.stderr)
