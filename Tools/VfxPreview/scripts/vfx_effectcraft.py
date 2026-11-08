import json
import math
import os
import random
import shutil
import subprocess
import uuid

import numpy as np
from PIL import Image

CLI_PATH = r"C:\Program Files\EffectCraft\effectcraft-cli.exe"
SCRIPTS_DIR = os.path.dirname(os.path.abspath(__file__))
TOOLS_DIR = os.path.abspath(os.path.join(SCRIPTS_DIR, ".."))
PROJECT_ROOT = os.path.abspath(os.path.join(TOOLS_DIR, "..", ".."))
PROJECTS_DIR = os.path.join(TOOLS_DIR, "projects")
OUTPUT_DIR = os.path.join(TOOLS_DIR, "output")
UNITY_VFX_DIR = os.path.join(PROJECT_ROOT, "Assets", "Image", "VFX", "Weapon")
GRAY_TOLERANCE = 3

SIZE = 256
FPS = 30
FRAMES = 16
GRID = 4
WHITE = [1.0, 1.0, 1.0]


class GrayscaleOnlyRuleError(RuntimeError):
    pass


def ease_out_expo(t):
    return 1.0 if t >= 1 else 1 - 2 ** (-10 * t)


def ease_out_back(t, s=1.7):
    t = t - 1
    return 1 + (s + 1) * t ** 3 + s * t ** 2


def ease_in_out_cubic(t):
    return 4 * t * t * t if t < 0.5 else 1 - (-2 * t + 2) ** 3 / 2


def clamp01(v):
    return max(0.0, min(1.0, v))


def life(f, start, length):
    return clamp01((f - start) / float(length))


def keyed(fn):
    return [fn(f) for f in range(FRAMES)]


def fade(f, start, length, power=1.4):
    if f < start:
        return 0.0
    return round(100.0 * (1 - life(f, start, length)) ** power, 1)


def window(f, born, hold, length, power=1.4):
    if f < born:
        return 0.0
    if f <= born + hold:
        return 100.0
    return fade(f, born + hold, length, power)


def crescent(radius, thick, a0, a1, peak=0.68, bias=0.62, n=40, squash=0.0):
    if a1 - a0 < 1.5:
        a1 = a0 + 1.5
    outer, inner = [], []
    for i in range(n + 1):
        t = i / n
        ang = math.radians(a0 + (a1 - a0) * t)
        r = radius * (1.0 + squash * math.cos(ang))
        w = (t / peak) ** 1.6 if t < peak else 1.0 - ((t - peak) / (1 - peak)) ** 2.2
        w = thick * max(0.0, w)
        ro, ri = r + w * bias, r - w * (1 - bias)
        outer.append([round(ro * math.cos(ang), 2), round(ro * math.sin(ang), 2)])
        inner.append([round(ri * math.cos(ang), 2), round(ri * math.sin(ang), 2)])
    return outer + inner[::-1]


def lance(x_tail, x_head, half_width, y=0.0, head_ratio=0.18, n=10):
    if x_head - x_tail < 1.0:
        x_head = x_tail + 1.0
    length = x_head - x_tail
    top, bottom = [], []
    for i in range(n + 1):
        t = i / n
        x = x_tail + length * t
        knee = 1 - head_ratio
        w = half_width * (t / knee) ** 0.8 if t < knee else half_width * (1 - (t - knee) / head_ratio)
        top.append([round(x, 2), round(y - w, 2)])
        bottom.append([round(x, 2), round(y + w, 2)])
    return top + bottom[::-1]


def star(points, outer, inner, rot_deg=0.0):
    pts = []
    for i in range(points * 2):
        r = outer if i % 2 == 0 else inner
        a = math.radians(rot_deg) + math.pi * i / points
        pts.append([round(r * math.cos(a), 2), round(r * math.sin(a), 2)])
    return pts


def ring_poly(radius, thick, n=48, sx=1.0, sy=1.0):
    outer, inner = [], []
    for i in range(n + 1):
        a = 2 * math.pi * i / n
        outer.append([round((radius + thick * 0.5) * math.cos(a) * sx, 2), round((radius + thick * 0.5) * math.sin(a) * sy, 2)])
        inner.append([round((radius - thick * 0.5) * math.cos(a) * sx, 2), round((radius - thick * 0.5) * math.sin(a) * sy, 2)])
    return outer + inner[::-1]


def diamond(length, width, back=0.3):
    return [[-length, 0], [0, -width], [length * back, 0], [0, width]]


def tone(value):
    if isinstance(value, list):
        return gray(value[0]), [gray(v) for v in value]
    return gray(value), None


def poly_layer(name, paths, opacity, blur=0, add=False, center=None, fx=None, value=1.0):
    color, keys = tone(value)
    return {"kind": "poly", "name": name, "paths": paths, "opacity": opacity, "blur": blur, "add": add, "center": center or [SIZE * 0.5, SIZE * 0.5], "color": color, "colorKeys": keys, "fx": fx or []}


def shape_layer(name, shape, pos, opacity, scale=None, rot=None, blur=0, add=True, fx=None, value=1.0):
    color, keys = tone(value)
    return {"kind": "shape", "name": name, "shape": shape, "pos": pos, "opacity": opacity, "scale": scale, "rot": rot, "blur": blur, "add": add, "color": color, "colorKeys": keys, "fx": fx or []}


def multi_layer(name, shapes, pos, opacity, scale=None, rot=None, fx=None, value=1.0):
    color, keys = tone(value)
    return {"kind": "multi", "name": name, "shapes": shapes, "pos": pos, "opacity": opacity, "scale": scale, "rot": rot, "blur": 0, "add": False, "color": color, "colorKeys": keys, "fx": fx or []}


def effect(name, values=None, keys=None):
    return {"effect": name, "set": values or {}, "keys": keys or {}}


def turbulent(amount, size, seed=0, boil_every=2, speed=40.0):
    return effect("Turbulent Displace", {"Amount": amount, "Size": size, "Random Seed": seed}, {"Evolution": [round(speed * (f - f % boil_every), 2) for f in range(FRAMES)]})


def echo(count=2, step=1.0 / FPS, intensity=0.7, decay=0.55):
    return effect("Echo", {"Echo Time (seconds)": -step, "Number Of Echoes": count, "Starting Intensity": intensity, "Decay": decay, "Echo Operator": 4})


def burst_particles(prefix, count, seed, origin, born, cone_deg, cone_width, speed, life_frames, length, width, spread_born=0):
    rng = random.Random(seed)
    layers = []
    for i in range(count):
        b = born + rng.randint(0, spread_born)
        ang = math.radians(cone_deg + rng.uniform(-cone_width * 0.5, cone_width * 0.5))
        spd = speed * rng.uniform(0.6, 1.0)
        lf = life_frames * rng.uniform(0.75, 1.1)
        ln = length * rng.uniform(0.6, 1.0)
        pos, ops, scl = [], [], []
        for f in range(FRAMES):
            t = life(f, b, lf)
            d = spd * ease_out_expo(t)
            pos.append([round(origin[0] + math.cos(ang) * d, 2), round(origin[1] + math.sin(ang) * d, 2)])
            ops.append(0.0 if f < b else round(100 * (1 - t) ** 1.5, 1))
            s = 0.0 if f < b else round(100 * (1 - t * 0.85), 1)
            scl.append([s, s])
        rot = [round(math.degrees(ang), 2)] * FRAMES
        layers.append(shape_layer("%s_%d" % (prefix, i + 1), diamond(ln, width), pos, ops, scl, rot))
    return layers


def gray(value):
    v = round(float(value), 4)
    return [v, v, v]


def is_gray(color):
    return len(color) >= 3 and abs(color[0] - color[1]) < 1e-6 and abs(color[1] - color[2]) < 1e-6 and 0.0 <= color[0] <= 1.0


def enforce_grayscale_only(layers):
    for layer in layers:
        colors = [layer["color"]] + (layer.get("colorKeys") or [])
        for color in colors:
            if not is_gray([float(c) for c in color]):
                raise GrayscaleOnlyRuleError("VFX must be grayscale (black to white); colour comes from the Unity gradient map. Layer '%s' has %s" % (layer["name"], color))


JSX_RUNTIME = """
(function() {
    var D = __DATA__;
    var comp = app.project.items.addComp(D.comp, D.size, D.size, 1.0, D.frames / D.fps, D.fps);
    comp.bgColor = [0, 0, 0];
    function fillGroup(layer, color) {
        var g = layer.property("Contents").addProperty("ADBE Vector Group").property("Contents");
        return g;
    }
    function addFill(g, color, keys) {
        var c = g.addProperty("ADBE Vector Graphic - Fill").property("Color");
        c.setValue([color[0], color[1], color[2], 1]);
        if (keys) for (var i = 0; i < D.times.length; i++) c.setValueAtTime(D.times[i], [keys[i][0], keys[i][1], keys[i][2], 1]);
    }
    function keyArray(prop, values) {
        if (!values) return;
        for (var i = 0; i < D.times.length; i++) prop.setValueAtTime(D.times[i], values[i]);
    }
    function finish(layer, L) {
        if (L.blur > 0) {
            var b = layer.property("Effects").addProperty("Gaussian Blur");
            b.property("Blurriness").setValue(L.blur);
        }
        for (var e = 0; e < L.fx.length; e++) {
            var F = L.fx[e];
            var inst = layer.property("Effects").addProperty(F.effect);
            for (var k in F.set) inst.property(k).setValue(F.set[k]);
            for (var kk in F.keys) keyArray(inst.property(kk), F.keys[kk]);
        }
        if (L.add) layer.blendingMode = BlendingMode.ADD;
    }
    function addPoly(L) {
        var layer = comp.layers.addShape();
        layer.name = L.name;
        var g = fillGroup(layer);
        var path = g.addProperty("ADBE Vector Shape - Group").property("Path");
        for (var i = 0; i < D.times.length; i++) {
            var s = new Shape();
            s.vertices = L.paths[i];
            s.closed = true;
            path.setValueAtTime(D.times[i], s);
        }
        addFill(g, L.color, L.colorKeys);
        var tr = layer.property("Transform");
        tr.property("Position").setValue(L.center);
        keyArray(tr.property("Opacity"), L.opacity);
        finish(layer, L);
    }
    function addShape(L) {
        var layer = comp.layers.addShape();
        layer.name = L.name;
        var g = fillGroup(layer);
        var s = new Shape();
        s.vertices = L.shape;
        s.closed = true;
        g.addProperty("ADBE Vector Shape - Group").property("Path").setValue(s);
        addFill(g, L.color, L.colorKeys);
        var tr = layer.property("Transform");
        keyArray(tr.property("Position"), L.pos);
        keyArray(tr.property("Opacity"), L.opacity);
        keyArray(tr.property("Scale"), L.scale);
        keyArray(tr.property("Rotation"), L.rot);
        finish(layer, L);
    }
    function addMulti(L) {
        var layer = comp.layers.addShape();
        layer.name = L.name;
        var g = fillGroup(layer);
        for (var j = 0; j < L.shapes.length; j++) {
            var s = new Shape();
            s.vertices = L.shapes[j];
            s.closed = true;
            g.addProperty("ADBE Vector Shape - Group").property("Path").setValue(s);
        }
        addFill(g, L.color, L.colorKeys);
        var tr = layer.property("Transform");
        keyArray(tr.property("Position"), L.pos);
        keyArray(tr.property("Opacity"), L.opacity);
        keyArray(tr.property("Scale"), L.scale);
        keyArray(tr.property("Rotation"), L.rot);
        finish(layer, L);
    }
    for (var i = 0; i < D.layers.length; i++) {
        var K = D.layers[i].kind;
        if (K == "poly") addPoly(D.layers[i]); else if (K == "multi") addMulti(D.layers[i]); else addShape(D.layers[i]);
    }
    return comp.numLayers;
})();
"""


def run(cmd):
    res = subprocess.run(cmd, capture_output=True, text=True, encoding="utf-8")
    if res.returncode != 0:
        raise RuntimeError("%s\n%s\n%s" % (" ".join(cmd), res.stdout, res.stderr))
    return res.stdout


def project_path(set_name, comp_name):
    return os.path.join(PROJECTS_DIR, set_name, comp_name + ".ecproj")


def output_dir(set_name, comp_name):
    return os.path.join(OUTPUT_DIR, set_name, comp_name)


def build(comp_name, layers, set_name, force=False):
    enforce_grayscale_only(layers)
    out_dir = output_dir(set_name, comp_name)
    os.makedirs(out_dir, exist_ok=True)
    data = {"comp": comp_name, "size": SIZE, "fps": FPS, "frames": FRAMES, "times": [round(f / FPS, 4) for f in range(FRAMES)], "layers": layers}
    jsx_path = os.path.join(out_dir, comp_name + ".jsx")
    with open(jsx_path, "w", encoding="utf-8") as f:
        f.write(JSX_RUNTIME.replace("__DATA__", json.dumps(data)))
    project = project_path(set_name, comp_name)
    create_project(jsx_path, project, force)
    return render_project(project, set_name)


def create_project(jsx_path, project, force=False):
    os.makedirs(os.path.dirname(project), exist_ok=True)
    if os.path.exists(project) and not force:
        print("  kept %s (already exists, may hold hand edits; pass --force to regenerate)" % os.path.relpath(project, TOOLS_DIR))
        return False
    if os.path.exists(project):
        os.remove(project)
    run([CLI_PATH, "script", jsx_path, "--empty", "--save-as", project])
    return True


def project_comp(project, comp=None):
    info = json.loads(run([CLI_PATH, "info", "--project", project, "--json"]))
    comps = [i for i in info["project"]["items"] if i["type"] == "Composition"]
    if comp:
        match = [c for c in comps if c["name"] == comp]
        if not match:
            raise RuntimeError("comp '%s' not found in %s (has: %s)" % (comp, project, ", ".join(c["name"] for c in comps)))
        return match[0]
    stem = os.path.splitext(os.path.basename(project))[0]
    match = [c for c in comps if c["name"] == stem]
    if match:
        return match[0]
    active = info["project"].get("activeComp")
    return next((c for c in comps if c["id"] == active), comps[0])


def check_grayscale_pixels(frame, comp_name, index):
    px = np.asarray(frame).astype(np.int16)
    visible = px[..., 3] > 8
    spread = np.maximum(np.abs(px[..., 0] - px[..., 1]), np.abs(px[..., 1] - px[..., 2]))
    bad = visible & (spread > GRAY_TOLERANCE)
    if bad.any():
        y, x = np.argwhere(bad)[0]
        raise GrayscaleOnlyRuleError("%s frame %d has coloured pixels (e.g. rgb %s at %d,%d); VFX must be grayscale, colour comes from the Unity gradient map" % (comp_name, index, tuple(int(c) for c in px[y, x, :3]), x, y))


def render_project(project, set_name=None, comp=None):
    info = project_comp(project, comp)
    name = info["name"]
    set_name = set_name or os.path.basename(os.path.dirname(project))
    out_dir = output_dir(set_name, name)
    frames_dir = os.path.join(out_dir, "frames")
    os.makedirs(frames_dir, exist_ok=True)
    count = max(1, int(round(info["duration"] * info["frameRate"])))
    frames = []
    for f in range(count):
        p = os.path.join(frames_dir, "%s_%02d.png" % (name, f))
        run([CLI_PATH, "render-frame", "--project", project, "--comp", name, "--frame", str(f), "--out", p, "--transparent", "--scale", "1"])
        frame = Image.open(p).convert("RGBA")
        check_grayscale_pixels(frame, name, f)
        frames.append(frame)
    clipped = [i for i, fr in enumerate(frames) if touches_edge(fr)]
    if clipped:
        print("  WARN %s touches the frame edge on frames %s (content may be cropped)" % (name, clipped))
    export_images(name, frames, out_dir)
    return os.path.join(out_dir, name + "_Sheet.png")


def touches_edge(frame, margin=2, threshold=8):
    a = np.asarray(frame)[..., 3]
    return bool(max(a[:margin].max(), a[-margin:].max(), a[:, :margin].max(), a[:, -margin:].max()) > threshold)


def export_images(comp_name, frames, out_dir):
    size = frames[0].size[0]
    grid = max(1, math.ceil(math.sqrt(len(frames))))
    rows = math.ceil(len(frames) / 8)
    sheet = Image.new("RGBA", (grid * size, grid * size), (0, 0, 0, 0))
    strip = Image.new("RGB", (8 * size, rows * size), (12, 14, 18))
    gif = []
    for i, im in enumerate(frames):
        sheet.paste(im, ((i % grid) * size, (i // grid) * size))
        bg = Image.new("RGBA", im.size, (12, 14, 18, 255))
        bg.alpha_composite(im)
        strip.paste(bg.convert("RGB"), ((i % 8) * size, (i // 8) * size))
        gif.append(bg.convert("RGB"))
    sheet.save(os.path.join(out_dir, comp_name + "_Sheet.png"))
    strip.save(os.path.join(out_dir, comp_name + "_Contact.png"))
    gif[0].save(os.path.join(out_dir, comp_name + "_Preview.gif"), save_all=True, append_images=gif[1:], duration=int(1000 / FPS), loop=0)


def export_to_unity(sheet_path, unity_dir=UNITY_VFX_DIR):
    os.makedirs(unity_dir, exist_ok=True)
    name = os.path.splitext(os.path.basename(sheet_path))[0]
    dst = os.path.join(unity_dir, name + ".png")
    shutil.copyfile(sheet_path, dst)
    meta = dst + ".meta"
    if not os.path.exists(meta):
        with open(meta, "w", encoding="utf-8", newline="\n") as f:
            f.write(sliced_meta(name, uuid.uuid4().hex))
    return dst


def read_guid(meta_path):
    with open(meta_path, encoding="utf-8") as f:
        for line in f:
            if line.startswith("guid:"):
                return line.split(":", 1)[1].strip()
    return None


def sliced_meta(name, guid):
    rng = random.Random(guid)
    ids = []
    while len(ids) < FRAMES:
        v = rng.randint(-2 ** 62, 2 ** 62)
        if v and v not in ids:
            ids.append(v)
    table = "".join("  - first:\n      213: %d\n    second: %s_%d\n" % (ids[i], name, i) for i in range(FRAMES))
    sprites = ""
    for i in range(FRAMES):
        sprites += (
            "    - serializedVersion: 2\n      name: %s_%d\n      rect:\n        serializedVersion: 2\n"
            "        x: %d\n        y: %d\n        width: %d\n        height: %d\n"
            "      alignment: 0\n      pivot: {x: 0.5, y: 0.5}\n      border: {x: 0, y: 0, z: 0, w: 0}\n"
            "      customData: \n      outline: []\n      physicsShape: []\n      tessellationDetail: -1\n      bones: []\n"
            "      spriteID: %s0800000000000000\n      internalID: %d\n      vertices: []\n      indices: \n      edges: []\n      weights: []\n"
        ) % (name, i, (i % GRID) * SIZE, (GRID - 1 - i // GRID) * SIZE, SIZE, SIZE, "%016x" % rng.getrandbits(64), ids[i])
    names = "".join("      %s_%d: %d\n" % (name, i, ids[i]) for i in range(FRAMES))
    return META_TEMPLATE.format(guid=guid, table=table, sprites=sprites, names=names, sheet_id="%032x" % rng.getrandbits(128))


META_TEMPLATE = """fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable:
{table}  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 2
  spriteExtrude: 1
  spriteMeshType: 0
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: 100
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 0
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 0
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 1
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  spriteSheet:
    serializedVersion: 2
    sprites:
{sprites}    outline: []
    customData:
    physicsShape: []
    bones: []
    spriteID: {sheet_id}
    internalID: 0
    vertices: []
    indices:
    edges: []
    weights: []
    secondaryTextures: []
    spriteCustomMetadata:
      entries: []
    nameFileIdTable:
{names}  mipmapLimitGroupName:
  pSDRemoveMatte: 0
  userData:
  assetBundleName:
  assetBundleVariant:
"""
