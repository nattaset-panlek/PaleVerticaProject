import argparse
import json
import math
import os
import random


from vfx_effectcraft import PROJECT_ROOT, create_project, export_to_unity, output_dir, project_path, render_project

SET_NAME = "SlashStylize"
COMP_NAME = "VFX_Slash_Stylize_V2"
OUT_DIR = output_dir(SET_NAME, COMP_NAME)
PROJECT_PATH = project_path(SET_NAME, COMP_NAME)

SIZE = 256
FPS = 30
FRAMES = 16
CENTER = (SIZE * 0.5, SIZE * 0.5 - 18)
RADIUS = 88
HEAD_FROM, HEAD_TO = -50.0, 150.0
ARC_POINTS = 48

WHITE = [1.0, 1.0, 1.0]
PALETTE = {"core": WHITE, "body": WHITE, "glow": WHITE, "spark": WHITE}


class GrayscaleOnlyRuleError(RuntimeError):
    pass


def enforce_grayscale_only(palette):
    for key, color in palette.items():
        c = [float(v) for v in color]
        if not (abs(c[0] - c[1]) < 1e-6 and abs(c[1] - c[2]) < 1e-6 and 0.0 <= c[0] <= 1.0):
            raise GrayscaleOnlyRuleError("VFX must be grayscale (black to white); colour comes from the Unity gradient map. '%s' is %s" % (key, color))
    return palette


def ease_out_expo(t):
    return 1.0 if t >= 1 else 1 - 2 ** (-10 * t)


def ease_in_out_cubic(t):
    return 4 * t * t * t if t < 0.5 else 1 - (-2 * t + 2) ** 3 / 2


def clamp01(v):
    return max(0.0, min(1.0, v))


def head_angle(f):
    return HEAD_FROM + (HEAD_TO - HEAD_FROM) * ease_out_expo(clamp01((f + 0.7) / 4.0))


def tail_angle(f):
    t = ease_in_out_cubic(clamp01((f - 1.5) / 10.0))
    return HEAD_FROM + 4 + (HEAD_TO - 2 - HEAD_FROM - 4) * t


def crescent(radius, thick, a0, a1, peak=0.68, bias=0.62, n=ARC_POINTS):
    if a1 - a0 < 1.5:
        a1 = a0 + 1.5
    outer, inner = [], []
    for i in range(n + 1):
        t = i / n
        ang = math.radians(a0 + (a1 - a0) * t)
        r = radius * (1.0 + 0.09 * math.cos(ang - math.radians(40)))
        if t < peak:
            w = (t / peak) ** 1.6
        else:
            w = 1.0 - ((t - peak) / (1 - peak)) ** 2.2
        w = thick * max(0.0, w)
        ro, ri = r + w * bias, r - w * (1 - bias)
        outer.append([round(ro * math.cos(ang), 2), round(ro * math.sin(ang), 2)])
        inner.append([round(ri * math.cos(ang), 2), round(ri * math.sin(ang), 2)])
    return outer + inner[::-1]


def span_thickness(f, base):
    span = head_angle(f) - tail_angle(f)
    smear = clamp01(span / 150.0)
    fade = 1.0 if f < 7 else max(0.15, 1 - (f - 7) / 8.0)
    return base * (0.45 + 0.75 * smear) * fade


def opacity_curve(f, hold=7, end=FRAMES - 1):
    if f <= hold:
        return 100.0 if f > 0 else 85.0
    return round(100.0 * max(0.0, 1 - ((f - hold) / (end - hold)) ** 1.4), 1)


def blade_keys():
    keys = []
    for f in range(FRAMES):
        keys.append(crescent(RADIUS, span_thickness(f, 15.0), tail_angle(f), head_angle(f), peak=0.78))
    return keys


def glow_keys():
    return [crescent(RADIUS - 2, span_thickness(f, 20.0), tail_angle(f) + 10, head_angle(f) + 2, peak=0.75, bias=0.5) for f in range(FRAMES)]


def core_keys():
    keys = []
    for f in range(FRAMES):
        a1 = head_angle(f) - 2
        a0 = max(tail_angle(f) + (a1 - tail_angle(f)) * 0.35, a1 - 95)
        keys.append(crescent(RADIUS + 2.5, span_thickness(f, 4.2), a0, a1, peak=0.82, bias=0.85))
    return keys


def streak_keys(radius, thick, lag, lead, head_cut):
    keys = []
    for f in range(FRAMES):
        fl = max(0.0, f - lag)
        a1 = head_angle(fl) + lead
        a0 = tail_angle(fl) + (a1 - tail_angle(fl)) * head_cut
        keys.append(crescent(radius, span_thickness(fl, thick), a0, a1, peak=0.72, bias=0.5, n=32))
    return keys


def fragment_layers():
    rng = random.Random(11)
    layers = []
    for i in range(4):
        start = 6 + i * 0.8
        ang = 40 + i * 28 + rng.uniform(-6, 6)
        drift = rng.uniform(10, 18)
        r0 = RADIUS + rng.uniform(-6, 6)
        keys, ops, times = [], [], []
        for f in range(FRAMES):
            local = clamp01((f - start) / 7.0)
            r = r0 + drift * ease_out_expo(local)
            length = 26 * (1 - local) + 4
            thick = 3.4 * (1 - local) + 0.4
            keys.append(crescent(r, thick, ang - length * 0.5 + 10 * local, ang + length * 0.5 + 14 * local, peak=0.55, bias=0.5, n=16))
            op = 0.0 if f < start else 100.0 * (1 - local) ** 1.3
            ops.append(round(op, 1))
            times.append(round(f / FPS, 4))
        layers.append({"name": "06_Fragment_%d" % (i + 1), "paths": keys, "opacity": ops})
    return layers


def spark_layers():
    rng = random.Random(7)
    sparks = []
    for i in range(7):
        birth = rng.choice([1, 2, 2, 3, 3, 4])
        ang = math.radians(head_angle(birth) + rng.uniform(-14, 4))
        r = RADIUS + rng.uniform(-4, 8)
        px, py = r * math.cos(ang), r * math.sin(ang)
        tang = ang + math.pi / 2
        out = ang
        speed = rng.uniform(55, 95)
        dx = math.cos(tang) * 0.75 + math.cos(out) * 0.45
        dy = math.sin(tang) * 0.75 + math.sin(out) * 0.45
        life = rng.uniform(6, 9)
        length = rng.uniform(12, 20)
        rot = math.degrees(math.atan2(dy, dx))
        pos, ops, scl = [], [], []
        for f in range(FRAMES):
            local = clamp01((f - birth) / life)
            d = speed * ease_out_expo(local) * 0.5
            pos.append([round(CENTER[0] + px + dx * d, 2), round(CENTER[1] + py + dy * d, 2)])
            ops.append(0.0 if f < birth else round(100 * (1 - local) ** 1.6, 1))
            s = 0 if f < birth else round(100 * (1 - local * 0.8), 1)
            scl.append([s, s])
        diamond = [[-length, 0], [0, -1.8], [length * 0.3, 0], [0, 1.8]]
        sparks.append({"name": "07_Spark_%d" % (i + 1), "shape": diamond, "rot": round(rot, 2), "pos": pos, "opacity": ops, "scale": scl})
    return sparks


def build_jsx(palette):
    enforce_grayscale_only(palette)
    times = [round(f / FPS, 4) for f in range(FRAMES)]
    main_ops = [opacity_curve(f) for f in range(FRAMES)]
    glow_ops = [round(o * 0.4, 1) for o in main_ops]
    core_ops = [opacity_curve(f, hold=5, end=11) for f in range(FRAMES)]
    streak_ops = [opacity_curve(f, hold=6, end=13) for f in range(FRAMES)]
    path_layers = [
        {"name": "01_Glow", "paths": glow_keys(), "opacity": glow_ops, "color": palette["glow"], "blur": 6, "mode": "add"},
        {"name": "02_InnerStreak", "paths": streak_keys(RADIUS - 15, 3.0, 1.0, -10, 0.25), "opacity": streak_ops, "color": palette["body"], "blur": 0, "mode": "add"},
        {"name": "03_OuterStreak", "paths": streak_keys(RADIUS + 13, 2.6, 0.6, 6, 0.4), "opacity": streak_ops, "color": palette["body"], "blur": 0, "mode": "add"},
        {"name": "04_MainBlade", "paths": blade_keys(), "opacity": main_ops, "color": palette["body"], "blur": 0, "mode": "normal"},
        {"name": "05_CoreEdge", "paths": core_keys(), "opacity": core_ops, "color": palette["core"], "blur": 0, "mode": "normal"},
    ]
    for frag in fragment_layers():
        frag.update({"color": palette["body"], "blur": 0, "mode": "add"})
        path_layers.append(frag)
    data = {"times": times, "pathLayers": path_layers, "sparks": spark_layers(), "spark": palette["spark"], "center": list(CENTER)}
    return """
(function() {
    var D = %s;
    var comp = app.project.items.addComp("%s", %d, %d, 1.0, %d / %d, %d);
    comp.bgColor = [0, 0, 0];
    function addFill(contents, color) {
        var fill = contents.addProperty("ADBE Vector Graphic - Fill");
        fill.property("Color").setValue([color[0], color[1], color[2], 1]);
    }
    function addPathLayer(L) {
        var layer = comp.layers.addShape();
        layer.name = L.name;
        var g = layer.property("Contents").addProperty("ADBE Vector Group").property("Contents");
        var path = g.addProperty("ADBE Vector Shape - Group").property("Path");
        for (var i = 0; i < D.times.length; i++) {
            var s = new Shape();
            s.vertices = L.paths[i];
            s.closed = true;
            path.setValueAtTime(D.times[i], s);
        }
        addFill(g, L.color);
        var tr = layer.property("Transform");
        tr.property("Position").setValue(D.center);
        var op = tr.property("Opacity");
        for (var j = 0; j < D.times.length; j++) op.setValueAtTime(D.times[j], L.opacity[j]);
        if (L.blur > 0) {
            var b = layer.property("Effects").addProperty("Gaussian Blur");
            b.property("Blurriness").setValue(L.blur);
        }
        if (L.mode == "add") layer.blendingMode = BlendingMode.ADD;
        return layer;
    }
    function addSpark(S) {
        var layer = comp.layers.addShape();
        layer.name = S.name;
        var g = layer.property("Contents").addProperty("ADBE Vector Group").property("Contents");
        var s = new Shape();
        s.vertices = S.shape;
        s.closed = true;
        g.addProperty("ADBE Vector Shape - Group").property("Path").setValue(s);
        addFill(g, D.spark);
        var tr = layer.property("Transform");
        tr.property("Rotation").setValue(S.rot);
        for (var i = 0; i < D.times.length; i++) {
            tr.property("Position").setValueAtTime(D.times[i], S.pos[i]);
            tr.property("Opacity").setValueAtTime(D.times[i], S.opacity[i]);
            tr.property("Scale").setValueAtTime(D.times[i], S.scale[i]);
        }
        layer.blendingMode = BlendingMode.ADD;
        return layer;
    }
    for (var a = 0; a < D.pathLayers.length; a++) addPathLayer(D.pathLayers[a]);
    for (var b = 0; b < D.sparks.length; b++) addSpark(D.sparks[b]);
    return comp.numLayers;
})();
""" % (json.dumps(data), COMP_NAME, SIZE, SIZE, FRAMES, FPS, FPS)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--force", action="store_true", help="regenerate the .ecproj even if it exists (discards hand edits)")
    parser.add_argument("--unity", action="store_true", help="copy the sheet into Assets/Image/VFX")
    args = parser.parse_args()
    os.makedirs(OUT_DIR, exist_ok=True)
    jsx_path = os.path.join(OUT_DIR, "slash_stylize_v2.jsx")
    with open(jsx_path, "w", encoding="utf-8") as f:
        f.write(build_jsx(PALETTE))
    create_project(jsx_path, PROJECT_PATH, args.force)
    sheet = render_project(PROJECT_PATH, SET_NAME)
    print("done ->", OUT_DIR)
    if args.unity:
        print("unity", export_to_unity(sheet, os.path.join(PROJECT_ROOT, "Assets", "Image", "VFX")))


if __name__ == "__main__":
    main()
