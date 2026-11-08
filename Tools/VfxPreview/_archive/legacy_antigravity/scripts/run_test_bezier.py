import subprocess
import os

cli = r"C:\Program Files\EffectCraft\effectcraft-cli.exe"
out_dir = r"F:\GameProject\Pale Vertica\Tools\VfxPreview"
jsx_file = os.path.join(out_dir, "test_shape_bezier.jsx")
out_png = os.path.join(out_dir, "test_shape_bezier.png")

jsx = f"""
(function() {{
    var comp = app.project.items.addComp("TestBezier", 512, 512, 1.0, 1.0, 30);
    comp.bgColor = [0, 0, 0];
    
    var layer = comp.layers.addShape();
    var contents = layer.property("Contents");
    var group = contents.addProperty("ADBE Vector Group");
    var gContents = group.property("Contents");
    
    var pathProp = gContents.addProperty("ADBE Vector Shape - Group");
    
    var myShape = new Shape();
    myShape.vertices = [[100, 100], [400, 100], [250, 400]];
    myShape.inTangents = [[0, 0], [-50, -50], [50, 50]];
    myShape.outTangents = [[50, -50], [0, 0], [-50, 50]];
    myShape.closed = true;
    
    pathProp.property("Path").setValue(myShape);
    
    var fill = gContents.addProperty("ADBE Vector Graphic - Fill");
    fill.property("Color").setValue([1, 1, 1, 1]);
    
    return "Shape Created";
}})();
"""

with open(jsx_file, "w", encoding="utf-8") as f:
    f.write(jsx)

res = subprocess.run([cli, "script", jsx_file, "--save-as", os.path.join(out_dir, "test_bezier.ecproj")], capture_output=True, text=True, encoding="utf-8")
print("Script out:", res.stdout, res.stderr)

res2 = subprocess.run([cli, "render-frame", "--project", os.path.join(out_dir, "test_bezier.ecproj"), "--comp", "TestBezier", "--time", "0", "--out", out_png, "--transparent"], capture_output=True, text=True, encoding="utf-8")
print("Render out:", res2.stdout, res2.stderr)
