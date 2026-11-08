import argparse
import math
import os
import random

from vfx_effectcraft import (FRAMES, SIZE, TOOLS_DIR, PROJECT_ROOT, build, clamp01, crescent, ease_in_out_cubic,
                             ease_out_back, ease_out_expo, echo, export_to_unity, keyed, life, poly_layer, shape_layer, star, turbulent)

SET_NAME = "WeaponArtStylized"
C = SIZE * 0.5
BODY = 0.55


def twos(f):
    return f - (f % 2)


def rotate(pts, deg, dx=0.0, dy=0.0):
    a = math.radians(deg)
    ca, sa = math.cos(a), math.sin(a)
    return [[round(x * ca - y * sa + dx, 2), round(x * sa + y * ca + dy, 2)] for x, y in pts]


def scale_pts(pts, sx, sy=None):
    sy = sx if sy is None else sy
    return [[round(x * sx, 2), round(y * sy, 2)] for x, y in pts]


def brush(centerline, width, peak=0.55, head=1.4, tail=1.0):
    n = len(centerline)
    left, right = [], []
    for i, (x, y) in enumerate(centerline):
        t = i / (n - 1)
        w = (t / peak) ** tail if t < peak else ((1 - t) / (1 - peak)) ** head
        w = width * max(0.0, w) * 0.5
        x0, y0 = centerline[max(0, i - 1)]
        x1, y1 = centerline[min(n - 1, i + 1)]
        dx, dy = x1 - x0, y1 - y0
        ln = math.hypot(dx, dy) or 1.0
        nx, ny = -dy / ln, dx / ln
        left.append([round(x + nx * w, 2), round(y + ny * w, 2)])
        right.append([round(x - nx * w, 2), round(y - ny * w, 2)])
    return left + right[::-1]


def sub_path(path_fn, t0, t1, n=24):
    t1 = max(t1, t0 + 0.01)
    return [path_fn(t0 + (t1 - t0) * i / (n - 1)) for i in range(n)]


def jagged(seed, spikes, r_in, r_min, r_max, erode=0.0, sx=1.0, sy=1.0, rot=0.0, scale=1.0):
    rng = random.Random(seed)
    pts = []
    base = rng.uniform(0, 360)
    for i in range(spikes):
        a = math.radians(base + rot + 360.0 * i / spikes + rng.uniform(-9, 9))
        r = rng.uniform(r_min, r_max)
        valley_r = r_in * (1 - 0.75 * erode)
        half = math.radians(180.0 / spikes) * (1 - 0.7 * erode)
        r = r * (1 - 0.35 * erode)
        va = a + math.radians(180.0 / spikes)
        pts.append([r * math.cos(a), r * math.sin(a)])
        pts.append([valley_r * math.cos(va - half * 0.15), valley_r * math.sin(va - half * 0.15)])
    return [[round(x * sx * scale, 2), round(y * sy * scale, 2)] for x, y in pts]


def burst_layers(prefix, born, size, spikes=11, sx=1.0, sy=1.0, rot=0.0, center=None, hold=2, erode_len=6, seed=1):
    def frame(f, core):
        if f < born:
            return jagged(seed, spikes, 0.1, 0.2, 0.3)
        q = twos(f) if f > born + 1 else f
        pop = ease_out_back(life(f, born, 1.5), 2.2)
        e = ease_in_out_cubic(life(q, born + hold, erode_len))
        boil = seed * 31 + q // 2
        if core:
            return jagged(boil + 7, max(5, spikes - 4), size * 0.12, size * 0.38, size * 0.6, e, sx, sy, rot + 12, pop * (1 - 0.6 * e))
        return jagged(boil, spikes, size * 0.3, size * 0.62, size, e, sx, sy, rot, pop * (1 + 0.1 * e))
    end = born + hold + erode_len
    body_op = keyed(lambda f: 0.0 if f < born or f > end else round(100 * BODY, 1))
    core_op = keyed(lambda f: 0.0 if f < born or f > end - 2 else 100.0)
    c = center or [C, C]
    return [poly_layer(prefix + "_Body", keyed(lambda f: frame(f, False)), body_op, center=c, fx=[turbulent(7, 26, seed)]),
            poly_layer(prefix + "_Core", keyed(lambda f: frame(f, True)), core_op, center=c, fx=[turbulent(4, 20, seed + 1)])]


def chunk_layers(prefix, count, seed, origin, born, cone, width, dist, size, life_frames=8, gravity=0.0):
    rng = random.Random(seed)
    layers = []
    for i in range(count):
        a = math.radians(cone + rng.uniform(-width / 2, width / 2))
        d = dist * rng.uniform(0.6, 1.0)
        s = size * rng.uniform(0.6, 1.0)
        spin = rng.uniform(-60, 60)
        b = born + rng.randint(0, 1)
        tri = [[s, 0], [-s * 0.6, -s * 0.55], [-s * 0.4, s * 0.6]]
        def pos(f, a=a, d=d, b=b):
            t = ease_out_expo(life(twos(f), b, life_frames))
            g = gravity * life(f, b, life_frames) ** 2
            return [round(origin[0] + math.cos(a) * d * t, 2), round(origin[1] + math.sin(a) * d * t + g, 2)]
        scl = keyed(lambda f, b=b: [0, 0] if f < b else [round(100 * (1 - life(twos(f), b, life_frames)) ** 0.8, 1)] * 2)
        op = keyed(lambda f, b=b: 0.0 if f < b or life(f, b, life_frames) >= 1 else 100.0)
        rot = keyed(lambda f, a=a, spin=spin: round(math.degrees(a) + spin * twos(f) * 0.5, 1))
        layers.append(shape_layer("%s_%d" % (prefix, i + 1), tri, keyed(pos), op, scl, rot, add=False))
    return layers


def sparkle(name, pos, born, size, length=8, spin=0.0):
    shape = star(4, size, size * 0.12)
    scl = keyed(lambda f: [0, 0] if f < born else [round(100 * ease_out_back(life(f, born, 2)) * (1 - life(twos(f), born + 2, length - 2)), 1)] * 2)
    op = keyed(lambda f: 0.0 if f < born or life(f, born, length) >= 1 else 100.0)
    rot = keyed(lambda f: round(spin * life(f, born, length), 1))
    return shape_layer(name, shape, [pos] * FRAMES, op, scl, rot, add=False)


def speed_brush(prefix, count, seed, born, length, travel, spread, curve=0.0, body=True):
    rng = random.Random(seed)
    layers = []
    for i in range(count):
        y0 = rng.uniform(-spread, spread)
        w = rng.uniform(3.5, 6.0)
        ln = rng.uniform(55, 85)
        b = born + rng.uniform(0, 0.8)
        def path(f, y0=y0, w=w, ln=ln, b=b):
            t = ease_out_expo(life(f, b, length))
            head = -18 - travel * (1 - t) - abs(y0) * 0.5
            tail = head - ln * (1 - 0.8 * t)
            line = [[tail + (head - tail) * k / 15, y0 * (1 - 0.35 * k / 15) + curve * math.sin(math.pi * k / 15)] for k in range(16)]
            return brush(line, w * (1 - 0.6 * t) + 0.5, 0.75, 1.2, 1.6)
        op = keyed(lambda f, b=b: 0.0 if f < b or life(f, b, length) >= 1 else (100.0 if i % 2 == 0 else round(100 * BODY, 1)))
        layers.append(poly_layer("%s_%d" % (prefix, i + 1), keyed(path), op, fx=[echo(1)]))
    return layers


def arc_segments(prefix, born, r0, r1, count, seed, length=7, thick=7.0, sy=1.0):
    rng = random.Random(seed)
    layers = []
    for i in range(count):
        a0 = 360.0 * i / count + rng.uniform(-15, 15)
        span = rng.uniform(35, 60)
        def path(f, a0=a0, span=span):
            t = ease_out_expo(life(twos(f) if f > born + 1 else f, born, length))
            r = r0 + (r1 - r0) * t
            pts = crescent(r, thick * (1 - t) + 0.8, a0 + 10 * t, a0 + span * (1 - 0.5 * t) + 10 * t, peak=0.5, bias=0.5, n=14)
            return scale_pts(pts, 1.0, sy)
        op = keyed(lambda f: 0.0 if f < born or life(f, born, length) >= 1 else 100.0)
        layers.append(poly_layer("%s_%d" % (prefix, i + 1), keyed(path), op))
    return layers


def fist_jab():
    layers = []
    layers += speed_brush("00_Speed", 4, 3, 0, 4, 60, 20, 4)
    layers += burst_layers("01_Burst", 1, 62, 12, sx=1.35, sy=1.0, seed=5)
    layers += arc_segments("02_Arc", 2, 20, 62, 3, 7, 6, 6.5, 1.0)
    layers += chunk_layers("03_Chunk", 6, 11, [C, C], 3, 0, 120, 70, 6.5, 8)
    layers.append(sparkle("04_Glint", [C + 22, C - 18], 2, 18, 6, 45))
    return layers


def fist_hook():
    r = 60
    end = -20.0
    cx, cy = C - r * math.cos(math.radians(end)), C - r * math.sin(math.radians(end))
    head = keyed(lambda f: 170 + (end + 360 - 170) * ease_out_expo(life(f, -0.6, 3)))
    tail = keyed(lambda f: 175 + (end + 360 - 6 - 175) * ease_in_out_cubic(life(twos(f), 1, 7)))
    body = keyed(lambda f: crescent(r, 20 * (0.45 + 0.55 * clamp01((head[f] - tail[f]) / 120)), tail[f], head[f], peak=0.8, bias=0.55, n=36))
    core = keyed(lambda f: crescent(r + 3, 7 * (0.45 + 0.55 * clamp01((head[f] - tail[f]) / 120)), tail[f] + (head[f] - tail[f]) * 0.25, head[f] - 1, peak=0.82, bias=0.8, n=36))
    op_b = keyed(lambda f: 0.0 if f > 8 else round(100 * BODY, 1))
    op_c = keyed(lambda f: 0.0 if f > 6 else 100.0)
    layers = [poly_layer("00_ArcBody", body, op_b, center=[cx, cy], fx=[echo(2), turbulent(6, 18, 3)]), poly_layer("01_ArcCore", core, op_c, center=[cx, cy], fx=[echo(1)])]
    layers += burst_layers("02_Burst", 3, 54, 10, sx=1.0, sy=1.25, rot=-20, seed=9)
    layers += chunk_layers("03_Chunk", 6, 13, [C, C], 4, 20, 110, 64, 6.0, 8)
    layers.append(sparkle("04_Glint", [C - 16, C - 26], 4, 16, 6, -45))
    return layers


def fist_smash():
    ground = [C, C + 30]
    layers = []
    layers += burst_layers("00_Burst", 0, 96, 14, sx=1.0, sy=0.42, center=ground, hold=2, erode_len=7, seed=21)
    rng = random.Random(22)
    for i in range(7):
        x0 = rng.uniform(-70, 70)
        h = rng.uniform(55, 95) * (1 - abs(x0) / 120)
        sway = rng.uniform(-12, 12)
        b = 1 + rng.randint(0, 1)
        w = rng.uniform(9, 14)
        def path(f, x0=x0, h=h, sway=sway, b=b, w=w):
            t = ease_out_expo(life(twos(f) if f > b + 1 else f, b, 8))
            top = h * (0.25 + 0.75 * t)
            bottom = top * 0.75 * t
            line = [[x0 * (1 + 0.15 * t) + sway * (k / 15) ** 2 * t, -(bottom + (top - bottom) * k / 15)] for k in range(16)]
            return brush(line[::-1], w * (1 - 0.7 * t) + 1, 0.35, 1.2, 1.0)
        op = keyed(lambda f, b=b, i=i: 0.0 if f < b or life(f, b, 8) >= 1 else (100.0 if i % 2 else round(100 * BODY, 1)))
        layers.append(poly_layer("01_Tongue_%d" % i, keyed(path), op, center=ground, fx=[turbulent(22, 18, i)]))
    layers += chunk_layers("02_Rock", 8, 23, ground, 1, -90, 150, 90, 8.0, 10, 60)
    for side in (-1, 1):
        for k in range(3):
            def puff(f, side=side, k=k):
                t = ease_out_expo(life(twos(f), 3 + k, 9))
                r = (10 + 6 * k) * (0.4 + 0.8 * t)
                cxp = side * (40 + 25 * k + 30 * t)
                return [[round(cxp + r * math.cos(2 * math.pi * j / 20), 2), round(-r * 0.6 * abs(math.sin(math.pi * j / 20)) ** 0.7 + (r * 0.25 if j > 10 else 0) - 6 * t, 2)] for j in range(20)]
            op = keyed(lambda f, k=k: 0.0 if f < 3 + k or life(f, 3 + k, 9) >= 1 else round(100 * BODY * (1 - life(twos(f), 3 + k, 9) * 0.5), 1))
            layers.append(poly_layer("03_Dust_%s%d" % ("L" if side < 0 else "R", k), keyed(puff), op, center=[ground[0], ground[1] + 8]))
    return layers


def fist_rush():
    rng = random.Random(4)
    layers = []
    for k in range(4):
        b = k * 2 + 1
        ox, oy = C + rng.uniform(-36, 36), C + rng.uniform(-30, 30)
        layers += burst_layers("0%d_Hit" % k, b, rng.uniform(32, 42), 9, sx=1.3, sy=1.0, rot=rng.uniform(-20, 20), center=[ox, oy], hold=1, erode_len=4, seed=40 + k)
        def dash(f, ox=ox, oy=oy, b=b):
            t = ease_out_expo(life(f, b - 1, 3))
            head = ox - C - 16
            tail = head - 60 * (1 - t) - 8
            line = [[tail + (head - tail) * j / 11, oy - C] for j in range(12)]
            return brush(line, 5 * (1 - t) + 0.5, 0.8, 1.0, 1.5)
        op = keyed(lambda f, b=b: 0.0 if f < b - 1 or f > b + 2 else 100.0)
        layers.append(poly_layer("0%d_Dash" % k, keyed(dash), op))
    return layers


def pinwheel(prefix, born, count, r0, r1, span, thick, spin, seed, length=8):
    layers = []
    rng = random.Random(seed)
    for i in range(count):
        a0 = 360.0 * i / count + rng.uniform(-8, 8)
        def path(f, a0=a0):
            q = f if f <= born + 2 else twos(f)
            t = ease_out_expo(life(q, born, length))
            r = r0 + (r1 - r0) * t
            a = a0 + spin * t
            return crescent(r, thick * (1 - 0.8 * t) + 0.6, a, a + span * (1 - 0.4 * t), peak=0.82, bias=0.7, n=20)
        op = keyed(lambda f, i=i: 0.0 if f < born or life(f, born, length) >= 1 else (100.0 if i % 2 == 0 else round(100 * BODY, 1)))
        layers.append(poly_layer("%s_%d" % (prefix, i + 1), keyed(path), op, fx=[echo(2)]))
    return layers


def magic_burst():
    layers = []
    layers += burst_layers("00_Burst", 0, 58, 10, seed=61, hold=1, erode_len=5)
    layers += pinwheel("01_Blade", 1, 6, 18, 86, 70, 15, 70, 62)
    layers += arc_segments("02_Arc", 2, 30, 100, 4, 63, 8, 5.0)
    layers.append(sparkle("03_Star", [C, C], 0, 70, 6, 0))
    rng = random.Random(64)
    for i in range(6):
        a = math.radians(rng.uniform(0, 360))
        d = rng.uniform(55, 95)
        layers.append(sparkle("04_Twinkle_%d" % i, [round(C + math.cos(a) * d, 1), round(C + math.sin(a) * d, 1)], rng.randint(4, 8), rng.uniform(9, 15), 6, 90))
    return layers


def magic_sigil():
    layers = []
    grow = lambda f: ease_in_out_cubic(life(f, 0, 6))
    out = lambda f: 1.0 + 0.3 * ease_out_expo(life(f, 10, 5))
    for k in range(3):
        a0 = -90 + k * 120
        def path(f, a0=a0):
            spin = 50 * life(f, 0, 15)
            return scale_pts(crescent(82, 6.0, a0 + spin, a0 + spin + 105 * max(0.02, grow(f)), peak=0.5, bias=0.5, n=24), out(f))
        layers.append(poly_layer("00_Ring_%d" % k, keyed(path), keyed(lambda f: 0.0 if f > 13 else 100.0)))
    for k in range(4):
        a0 = k * 90
        def path(f, a0=a0):
            spin = -80 * life(f, 0, 15)
            return scale_pts(crescent(62, 4.0, a0 + spin, a0 + spin + 60 * max(0.02, ease_in_out_cubic(life(f, 2, 5))), peak=0.5, bias=0.5, n=16), out(f))
        layers.append(poly_layer("01_Inner_%d" % k, keyed(path), keyed(lambda f: 0.0 if f < 2 or f > 13 else round(100 * BODY, 1))))
    for i in range(6):
        def pos(f, i=i):
            a = math.radians(i * 60 + 30 + 50 * life(f, 0, 15))
            return [round(C + 72 * out(f) * math.cos(a), 2), round(C + 72 * out(f) * math.sin(a), 2)]
        glyph = [[0, -7], [3.5, 0], [0, 7], [-3.5, 0]]
        scl = keyed(lambda f, i=i: [0, 0] if f < 3 + i * 0.5 else [round(100 * ease_out_back(life(f, 3 + i * 0.5, 2)), 1)] * 2)
        layers.append(shape_layer("02_Glyph_%d" % i, glyph, keyed(pos), keyed(lambda f: 0.0 if f > 12 else 100.0), scl, keyed(lambda f, i=i: round(i * 60 + 120 + 50 * life(f, 0, 15), 1)), add=False))
    layers += burst_layers("03_Flash", 9, 74, 12, seed=71, hold=1, erode_len=5)
    layers.append(sparkle("04_Star", [C, C], 8, 86, 7, 0))
    return layers


def magic_orb():
    layers = []
    rng = random.Random(81)
    for i in range(6):
        a0 = 60.0 * i + rng.uniform(-10, 10)
        def spiral(t, a0=a0):
            r = 108 * (1 - t) + 8
            a = math.radians(a0 + 260 * t)
            return [r * math.cos(a), r * math.sin(a)]
        def path(f):
            h = ease_in_out_cubic(life(f, 0, 7))
            return brush(sub_path(spiral, max(0.0, h - 0.4), h, 20), 7.0, 0.85, 1.0, 1.6)
        op = keyed(lambda f, i=i: 0.0 if f > 7 else (100.0 if i % 2 else round(100 * BODY, 1)))
        layers.append(poly_layer("00_Wisp_%d" % i, keyed(path), op, fx=[echo(2)]))
    def orb(f, scale):
        g = ease_out_back(life(f, 1, 6), 1.4) * scale
        boil = random.Random(90 + twos(f))
        return [[round((g + boil.uniform(-0.6, 0.6)) * math.cos(2 * math.pi * j / 24), 2), round((g + boil.uniform(-0.6, 0.6)) * math.sin(2 * math.pi * j / 24), 2)] for j in range(24)]
    layers.append(poly_layer("01_OrbBody", keyed(lambda f: orb(f, 22)), keyed(lambda f: 0.0 if f < 1 or f > 8 else round(100 * BODY, 1)), fx=[turbulent(14, 10, 5)]))
    layers.append(poly_layer("02_OrbCore", keyed(lambda f: orb(f, 12)), keyed(lambda f: 0.0 if f < 1 or f > 8 else 100.0)))
    layers += burst_layers("03_Pop", 8, 84, 12, seed=83, hold=1, erode_len=5)
    layers += pinwheel("04_Petal", 8, 8, 20, 92, 40, 12, 40, 84, 7)
    layers.append(sparkle("05_Star", [C + 30, C - 30], 9, 16, 6, 45))
    layers.append(sparkle("05_Star2", [C - 34, C + 22], 10, 12, 5, -45))
    return layers


def magic_shard():
    layers = []
    layers += burst_layers("00_Crack", 0, 46, 9, seed=91, hold=1, erode_len=4)
    rng = random.Random(92)
    for i in range(8):
        ang = i * 45 + rng.uniform(-12, 12)
        ln = rng.uniform(26, 40)
        wd = ln * rng.uniform(0.22, 0.3)
        d = rng.uniform(50, 78)
        b = rng.uniform(0, 1.5)
        def pos(f, ang=ang, d=d, b=b):
            t = ease_out_expo(life(f if f <= b + 2 else twos(f), b, 6))
            return [round(C + math.cos(math.radians(ang)) * d * t, 2), round(C + math.sin(math.radians(ang)) * d * t, 2)]
        scl = keyed(lambda f, b=b: [0, 0] if f < b else [round(100 * ease_out_back(life(f, b, 2), 2.0), 1), round(100 * (1 - 0.75 * life(twos(f), b + 5, 6)), 1)])
        op = keyed(lambda f, b=b: 0.0 if f < b or life(f, b + 5, 6) >= 1 else 100.0)
        top = [[-ln * 0.3, 0], [0, -wd], [ln * 0.7, 0]]
        bottom = [[-ln * 0.3, 0], [ln * 0.7, 0], [0, wd]]
        rot = [round(ang, 1)] * FRAMES
        layers.append(shape_layer("01_ShardLit_%d" % i, top, keyed(pos), op, scl, rot, add=False))
        layers.append(shape_layer("02_ShardShade_%d" % i, bottom, keyed(pos), keyed(lambda f, op=op: round(op[f] * BODY, 1)), scl, rot, add=False))
    layers += arc_segments("03_Arc", 1, 24, 84, 5, 93, 6, 4.5)
    return layers


def spear_thrust():
    head = keyed(lambda f: -100 + 140 * ease_out_expo(life(f, -0.5, 3)))
    tail = keyed(lambda f: -125 + 150 * ease_in_out_cubic(life(twos(f), 1, 9)))
    def lance_line(f, offset=0.0):
        return [[tail[f] + (head[f] + offset - tail[f]) * k / 19, 0] for k in range(20)]
    span = lambda f: clamp01((head[f] - tail[f]) / 110.0)
    layers = [
        poly_layer("00_LanceBody", keyed(lambda f: brush(lance_line(f, 4), 22 * (0.4 + 0.6 * span(f)), 0.8, 0.8, 1.6)), keyed(lambda f: 0.0 if f > 9 else round(100 * BODY, 1)), fx=[echo(3)]),
        poly_layer("01_LanceCore", keyed(lambda f: brush(lance_line(f), 9 * (0.4 + 0.6 * span(f)), 0.85, 0.7, 1.8)), keyed(lambda f: 0.0 if f > 7 else 100.0)),
    ]
    for k, phase in enumerate((0.0, math.pi)):
        def path(f, phase=phase):
            q = f if f < 3 else twos(f)
            t = life(q, 0, 9)
            h = head[q] - 6
            tl = tail[q] + 10
            line = [[tl + (h - tl) * j / 23, 15 * (1 - 0.5 * t) * math.sin(j / 23 * 3 * math.pi + phase + 4 * t) * (j / 23)] for j in range(24)]
            return brush(line, 4.5 * (1 - 0.5 * t), 0.7, 1.4, 1.4)
        layers.append(poly_layer("02_Wind_%d" % k, keyed(path), keyed(lambda f, k=k: 0.0 if f > 10 else (100.0 if k == 0 else round(100 * BODY, 1))), fx=[turbulent(6, 14, k)]))
    layers += burst_layers("03_Pierce", 2, 50, 9, sx=1.6, sy=0.8, center=[C + 40, C], seed=101, hold=1, erode_len=5)
    layers += chunk_layers("04_Chunk", 6, 103, [C + 44, C], 3, 0, 80, 60, 5.5, 8)
    return layers


RECIPES = {
    "VFX_Fist_Jab_Art": fist_jab,
    "VFX_Fist_Hook_Art": fist_hook,
    "VFX_Fist_Smash_Art": fist_smash,
    "VFX_Fist_Rush_Art": fist_rush,
    "VFX_Magic_Burst_Art": magic_burst,
    "VFX_Magic_Sigil_Art": magic_sigil,
    "VFX_Magic_Orb_Art": magic_orb,
    "VFX_Magic_Shard_Art": magic_shard,
    "VFX_Spear_Thrust_Art": spear_thrust,
}


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("names", nargs="*", help="recipe names (default: all)")
    parser.add_argument("--force", action="store_true", help="regenerate the .ecproj even if it exists (discards hand edits)")
    parser.add_argument("--unity", action="store_true", help="copy the sheets into Assets/Image/VFX/Weapon")
    args = parser.parse_args()
    for name in args.names or list(RECIPES):
        sheet = build(name, RECIPES[name](), SET_NAME, args.force)
        print("built", name)
        if args.unity:
            print("  unity", export_to_unity(sheet))


if __name__ == "__main__":
    main()
