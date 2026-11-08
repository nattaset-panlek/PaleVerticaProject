import argparse
import math
import os
import random

from vfx_effectcraft import (FPS, FRAMES, SIZE, TOOLS_DIR, PROJECT_ROOT, build, clamp01, crescent, ease_in_out_cubic,
                             ease_out_back, ease_out_expo, export_to_unity, keyed, life, poly_layer, shape_layer)

SET_NAME = "WeaponV2Style"
C = SIZE * 0.5
GLOW = 0.5
BODY = 0.82
CORE = 1.0
TIERS = (("Glow", 2.0, 0.5, GLOW, 6, True, 0.45), ("Body", 1.0, 0.55, BODY, 0, False, 1.0), ("Core", 0.32, 0.85, CORE, 0, False, 1.0))


def fade(f, hold, end, power=1.4):
    if f <= hold:
        return 100.0
    return round(100.0 * max(0.0, 1 - ((f - hold) / float(max(1, end - hold))) ** power), 1)


def window(born, hold, end):
    return keyed(lambda f: 0.0 if f < born else fade(f - born, hold, end - born))


def transform_pts(pts, sy=1.0, rot=0.0):
    a = math.radians(rot)
    ca, sa = math.cos(a), math.sin(a)
    return [[round(x * ca - y * sy * sa, 2), round(x * sa + y * sy * ca, 2)] for x, y in pts]


def arc(radius, thick, a0, a1, peak=0.5, bias=0.5, n=40, sy=1.0, rot=0.0):
    return transform_pts(crescent(radius, thick, a0, a1, peak=peak, bias=bias, n=n), sy, rot)


def circle(r, n=36, sy=1.0, dx=0.0, dy=0.0):
    r = max(0.2, r)
    return [[round(dx + r * math.cos(2 * math.pi * i / n), 2), round(dy + r * sy * math.sin(2 * math.pi * i / n), 2)] for i in range(n)]


def tiered(name, path_fn, opacity, center=None, tiers=TIERS, skip=()):
    layers = []
    for label, mult, bias, value, blur, add, op_scale in tiers:
        if label in skip:
            continue
        ops = [round(o * op_scale, 1) for o in opacity]
        layers.append(poly_layer("%s_%s" % (name, label), keyed(lambda f, m=mult, b=bias: path_fn(f, m, b)), ops, blur=blur, add=add, center=center or [C, C], value=value))
    return layers


def flash(name, born, radius, center=None, hold=1, length=4, sy=1.0):
    def path(f, mult, bias):
        g = ease_out_back(life(f, born, 1.5), 2.0) * (1 - 0.6 * ease_in_out_cubic(life(f, born + hold, length)))
        return circle(radius * g * (0.55 + 0.45 * mult) if mult < 1.5 else radius * g * 1.6, sy=sy)
    return tiered(name, path, window(born, hold, born + hold + length), center)


def ring(name, born, r0, r1, thick, length, center=None, sy=1.0, rot=0.0, gap=0.0, value=BODY):
    def path(f):
        t = ease_out_expo(life(f, born, length))
        r = r0 + (r1 - r0) * t
        w = thick * (1 - t) ** 1.2 + 0.6
        return arc(r, w, rot + gap * 0.5, rot + 360 - gap * 0.5, peak=0.5, bias=0.5, n=64, sy=sy)
    return poly_layer(name, keyed(path), window(born, 1, born + length), center=center or [C, C], value=value)


def shock_arcs(prefix, born, count, dir_deg, span, r0, r1, thick, length, center=None, stagger=1.0, sy=1.0):
    layers = []
    for k in range(count):
        b = born + k * stagger
        scale = 1 - 0.18 * k
        def path(f, mult, bias, b=b, scale=scale):
            t = ease_out_expo(life(f, b, length))
            r = (r0 + (r1 - r0) * t) * scale
            w = thick * scale * (1 - 0.8 * t) * mult + 0.5
            s = span * (0.6 + 0.4 * t)
            return arc(r, w, dir_deg - s / 2, dir_deg + s / 2, peak=0.5, bias=bias, n=36, sy=sy)
        skip = () if k == 0 else ("Core",)
        layers += tiered("%s_%d" % (prefix, k + 1), path, window(b, 1, b + length), center, skip=skip)
    return layers


def sparks(prefix, count, seed, origin, born, dir_deg, spread, speed, life_frames, length, value=CORE):
    rng = random.Random(seed)
    layers = []
    for i in range(count):
        b = born + rng.choice([0, 0, 1, 1, 2])
        ang = math.radians(dir_deg + rng.uniform(-spread / 2, spread / 2))
        spd = speed * rng.uniform(0.6, 1.0)
        lf = life_frames * rng.uniform(0.8, 1.1)
        ln = length * rng.uniform(0.7, 1.0)
        pos, ops, scl = [], [], []
        for f in range(FRAMES):
            t = life(f, b, lf)
            d = spd * ease_out_expo(t)
            pos.append([round(origin[0] + math.cos(ang) * d, 2), round(origin[1] + math.sin(ang) * d, 2)])
            ops.append(0.0 if f < b else round(100 * (1 - t) ** 1.6, 1))
            s = 0 if f < b else round(100 * (1 - t * 0.8), 1)
            scl.append([s, s])
        diamond = [[-ln, 0], [0, -1.8], [ln * 0.3, 0], [0, 1.8]]
        layers.append(shape_layer("%s_%d" % (prefix, i + 1), diamond, pos, ops, scl, [round(math.degrees(ang), 2)] * FRAMES, add=True, value=value))
    return layers


def fragments(prefix, count, seed, center, radius, a_from, a_to, born, drift, length=7, sy=1.0, value=BODY):
    rng = random.Random(seed)
    layers = []
    for i in range(count):
        b = born + i * 0.7
        ang = a_from + (a_to - a_from) * (i + 0.5) / count + rng.uniform(-6, 6)
        r0 = radius + rng.uniform(-6, 6)
        dr = drift * rng.uniform(0.7, 1.2)
        def path(f, b=b, ang=ang, r0=r0, dr=dr):
            t = clamp01((f - b) / float(length))
            r = r0 + dr * ease_out_expo(t)
            ln = 24 * (1 - t) + 4
            th = 3.6 * (1 - t) + 0.4
            return arc(r, th, ang - ln * 0.5 + 10 * t, ang + ln * 0.5 + 14 * t, peak=0.55, bias=0.5, n=16, sy=sy)
        ops = keyed(lambda f, b=b: 0.0 if f < b else round(100 * (1 - clamp01((f - b) / float(length))) ** 1.3, 1))
        layers.append(poly_layer("%s_%d" % (prefix, i + 1), keyed(path), ops, add=True, center=center, value=value))
    return layers


def streaks(prefix, count, seed, born, length, travel, spread, line_len, center=None):
    rng = random.Random(seed)
    layers = []
    for i in range(count):
        y = rng.uniform(-spread, spread)
        b = born + rng.uniform(0, 0.8)
        ln = line_len * rng.uniform(0.7, 1.0)
        w = rng.uniform(2.2, 3.4)
        def path(f, y=y, b=b, ln=ln, w=w):
            t = ease_out_expo(life(f, b, length))
            head = -16 - travel * (1 - t) - abs(y) * 0.6
            tail = head - ln * (1 - 0.75 * t)
            r = 4000.0
            a1 = math.degrees(head / r)
            a0 = math.degrees(tail / r)
            pts = crescent(r, w * (1 - 0.5 * t) + 0.4, a0, a1, peak=0.78, bias=0.5, n=12)
            return [[round(py, 2), round(px - r + y, 2)] for px, py in pts]
        layers.append(poly_layer("%s_%d" % (prefix, i + 1), keyed(path), window(b, 1, b + length), add=True, center=center or [C, C], value=BODY if i % 2 else CORE))
    return layers


def sweep(name, center, radius, head_from, head_to, thick, head_frames=4, tail_delay=1.5, tail_frames=9, peak=0.75, sy=1.0, rot=0.0, end=12):
    head = lambda f: head_from + (head_to - head_from) * ease_out_expo(clamp01((f + 0.7) / head_frames))
    tail = lambda f: head_from + 3 + (head_to - 2 - head_from - 3) * ease_in_out_cubic(clamp01((f - tail_delay) / tail_frames))
    def path(f, mult, bias):
        h, t = head(f), tail(f)
        span = clamp01((h - t) / max(1.0, abs(head_to - head_from) * 0.8))
        w = thick * (0.45 + 0.75 * span) * mult
        a0 = t if mult >= 0.5 else t + (h - t) * 0.4
        return arc(radius + (2 if mult < 0.5 else 0), w, a0, h, peak=peak if mult >= 0.5 else 0.82, bias=bias, n=48, sy=sy, rot=rot)
    return tiered(name, path, window(0, 6, end), center)


def fist_jab():
    layers = []
    layers += streaks("00_Streak", 4, 3, 0, 5, 50, 18, 60)
    layers.append(ring("01_Ring", 2, 8, 60, 6, 8, value=GLOW))
    layers += sparks("02_Spark", 7, 5, [C + 6, C], 2, 0, 110, 70, 8, 12)
    layers += fragments("03_Frag", 3, 6, [C, C], 48, -50, 50, 5, 16)
    return layers


def fist_hook():
    r = 56
    end = -20.0
    impact = [C + 14, C + 6]
    cx, cy = impact[0] - r * math.cos(math.radians(end)), impact[1] - r * math.sin(math.radians(end))
    layers = sweep("00_Sweep", [cx, cy], r, 170.0, 340.0, 14, head_frames=3.5, tail_delay=1.5, tail_frames=7, end=9)
    layers += sparks("01_Spark", 6, 9, impact, 3, -10, 120, 56, 8, 12)
    layers += fragments("02_Frag", 3, 10, [cx, cy], r, 220, 320, 6, 14)
    return layers


def fist_smash():
    ground = [C, C + 34]
    layers = []
    layers.append(ring("00_Ground", 1, 10, 112, 10, 10, center=ground, sy=0.36, value=GLOW))
    layers.append(ring("01_Ground2", 3, 6, 80, 6, 8, center=ground, sy=0.36, value=BODY))
    for k, (a0, a1) in enumerate(((195, 255), (285, 345))):
        def path(f, mult, bias, a0=a0, a1=a1, k=k):
            t = ease_out_expo(life(f, 1, 6))
            r = 18 + 62 * t
            w = 15 * (1 - 0.75 * t) * mult + 0.5
            if k == 0:
                return arc(r, w, a0 + 10 * (1 - t), a1, peak=0.35, bias=bias, n=32)
            return arc(r, w, a0, a1 - 10 * (1 - t), peak=0.65, bias=bias, n=32)
        layers += tiered("02_Splash_%d" % k, path, window(1, 2, 9), ground)
    layers += flash("03_Flash", 0, 26, center=[ground[0], ground[1] - 6], hold=2, length=4, sy=0.7)
    layers += sparks("04_Spark", 8, 21, ground, 1, -90, 150, 95, 9, 14)
    layers += fragments("05_Frag", 4, 22, ground, 70, 200, 340, 4, 20)
    return layers


def magic_burst():
    layers = []
    layers += flash("00_Core", 0, 24, hold=2, length=5)
    layers.append(ring("01_Ring", 2, 14, 96, 7, 9, value=GLOW))
    layers.append(ring("02_Ring2", 4, 10, 66, 4, 8, gap=40, rot=-90, value=BODY))
    for k in range(4):
        a0 = k * 90
        def path(f, mult, bias, a0=a0):
            t = ease_out_expo(life(f, 1, 9))
            r = 18 + 52 * t
            a = a0 + 150 * t
            w = 19 * (1 - 0.7 * t) * mult + 0.5
            return arc(r, w, a, a + 85 * (0.5 + 0.5 * t), peak=0.8, bias=bias, n=32)
        layers += tiered("03_Blade_%d" % k, path, window(1, 4, 11), skip=() if k % 2 == 0 else ("Core",))
    layers += sparks("04_Spark", 8, 31, [C, C], 2, 0, 360, 80, 8, 10)
    layers += fragments("05_Frag", 4, 32, [C, C], 64, 0, 360, 6, 20)
    return layers


def magic_orb():
    layers = []
    for k in range(3):
        a0 = k * 120
        def path(f, mult, bias, a0=a0):
            t = ease_in_out_cubic(life(f, 0, 7))
            r = 96 * (1 - t) + 14
            a = a0 + 220 * t
            w = (9 + 6 * t) * mult * (1 - 0.3 * t) + 0.5
            return arc(r, w, a - 110 * (1 - 0.5 * t), a, peak=0.85, bias=bias, n=36)
        layers += tiered("00_Tail_%d" % k, path, keyed(lambda f: 100.0 if f <= 7 else 0.0), skip=() if k == 0 else ("Core",))
    def orb(f, mult, bias):
        g = ease_out_back(life(f, 1, 6), 1.3) * (1 if f <= 7 else 0.02)
        return circle(g * 13 * (0.5 + 0.5 * mult) if mult < 1.5 else g * 22)
    layers += tiered("01_Orb", orb, keyed(lambda f: 100.0 if 1 <= f <= 7 else 0.0))
    layers += flash("02_Pop", 8, 30, hold=1, length=5)
    layers.append(ring("03_PopRing", 8, 14, 100, 8, 7, value=GLOW))
    for k in range(6):
        a0 = k * 60
        def path(f, mult, bias, a0=a0):
            t = ease_out_expo(life(f, 8, 6))
            r = 20 + 66 * t
            w = 11 * (1 - 0.8 * t) * mult + 0.5
            return arc(r, w, a0 + 20 * t, a0 + 40 + 20 * t, peak=0.75, bias=bias, n=20)
        layers += tiered("04_Petal_%d" % k, path, window(8, 1, 14), skip=("Core",) if k % 2 else ())
    layers += sparks("05_Spark", 7, 41, [C, C], 8, 0, 360, 80, 7, 10)
    return layers


def magic_sigil():
    layers = []
    def ring_draw(name, radius, thick, start, direction, born, draw_frames, spin):
        def path(f, mult, bias):
            d = ease_in_out_cubic(life(f, born, draw_frames))
            s = spin * life(f, 0, 15)
            pulse = 1 + 0.18 * ease_out_expo(life(f, 10, 5))
            if direction > 0:
                a0, a1 = start + s, start + s + 359 * max(0.02, d)
            else:
                a0, a1 = start - s - 359 * max(0.02, d), start - s
            return arc(radius * pulse, thick * mult + 0.3, a0, a1, peak=0.88 if direction > 0 else 0.12, bias=bias, n=72)
        return tiered(name, path, keyed(lambda f: 0.0 if f < born else fade(f, 10, 15)))
    layers += ring_draw("00_Outer", 82, 5.5, -90, 1, 0, 6, 60)
    layers += ring_draw("01_Inner", 58, 3.5, 90, -1, 2, 5, 80)
    for i in range(4):
        def pos(f, i=i):
            a = math.radians(i * 90 + 45 - 80 * life(f, 0, 15))
            r = 70 * (1 + 0.18 * ease_out_expo(life(f, 10, 5)))
            return [round(C + r * math.cos(a), 2), round(C + r * math.sin(a), 2)]
        scl = keyed(lambda f, i=i: [0, 0] if f < 3 + i * 0.6 else [round(100 * ease_out_back(life(f, 3 + i * 0.6, 2)), 1)] * 2)
        layers.append(shape_layer("02_Dot_%d" % i, circle(4.5, 16), keyed(pos), keyed(lambda f: 0.0 if f < 3 else fade(f, 10, 14)), scl, None, add=False, value=CORE))
    layers += flash("03_Flash", 9, 30, hold=1, length=5)
    layers.append(ring("04_Pulse", 10, 30, 112, 6, 6, value=GLOW))
    layers += sparks("05_Spark", 6, 51, [C, C], 10, 0, 360, 70, 6, 9)
    return layers


def line_blade(x_tail, x_head, thick, bias, y=0.0, peak=0.8, n=24):
    x_head = max(x_tail + 1.0, x_head)
    top, bottom = [], []
    for i in range(n + 1):
        t = i / n
        w = (t / peak) ** 1.6 if t < peak else 1.0 - ((t - peak) / (1 - peak)) ** 2.2
        w = thick * max(0.0, w)
        x = x_tail + (x_head - x_tail) * t
        top.append([round(x, 2), round(y - w * bias, 2)])
        bottom.append([round(x, 2), round(y + w * (1 - bias), 2)])
    return top + bottom[::-1]


def spear_thrust():
    head = lambda f: -88 + 168 * ease_out_expo(clamp01((f + 0.7) / 4.0))
    tail = lambda f: -84 + 160 * ease_in_out_cubic(clamp01((f - 1.5) / 10.0))
    def span(f):
        return clamp01((head(f) - tail(f)) / 140.0)
    def blade(f, mult, bias):
        h, t = head(f), tail(f)
        w = 12 * (0.45 + 0.75 * span(f)) * mult + 0.3
        if mult < 0.5:
            t = t + (h - t) * 0.4
            return line_blade(t, h - 2, w, 0.85, y=-1.5, peak=0.82)
        return line_blade(t, h, w, bias, peak=0.78)
    layers = tiered("00_Blade", blade, window(0, 6, 14))
    for k, (y, lag, lead, cut) in enumerate(((-14, 1.0, -12, 0.25), (13, 0.6, -4, 0.4))):
        def side(f, y=y, lag=lag, lead=lead, cut=cut):
            fl = max(0.0, f - lag)
            h = head(fl) + lead
            t = tail(fl) + (h - tail(fl)) * cut
            return line_blade(t, h, 3.0 * (0.45 + 0.75 * span(fl)), 0.5, y=y, peak=0.72)
        layers.append(poly_layer("01_Streak_%d" % k, keyed(side), window(0, 6, 13), add=True, value=BODY))
    rng = random.Random(61)
    for i in range(4):
        b = 6 + i * 0.8
        x0 = rng.uniform(-28, 52)
        y0 = rng.choice([-1, 1]) * rng.uniform(4, 9)
        dy = (1 if y0 > 0 else -1) * rng.uniform(10, 16)
        def frag(f, b=b, x0=x0, y0=y0, dy=dy):
            t = clamp01((f - b) / 7.0)
            ln = 26 * (1 - t) + 4
            x = x0 + 10 * t
            return line_blade(x - ln * 0.5, x + ln * 0.5, 3.4 * (1 - t) + 0.4, 0.5, y=y0 + dy * ease_out_expo(t), peak=0.55)
        ops = keyed(lambda f, b=b: 0.0 if f < b else round(100 * (1 - clamp01((f - b) / 7.0)) ** 1.3, 1))
        layers.append(poly_layer("02_Frag_%d" % (i + 1), keyed(frag), ops, add=True, value=BODY))
    layers += sparks("03_Spark_Up", 3, 62, [C + 52, C - 4], 2, -110, 50, 34, 8, 11)
    layers += sparks("04_Spark_Down", 3, 63, [C + 52, C + 4], 2, 110, 50, 34, 8, 11)
    return layers


RECIPES = {
    "VFX_Fist_Jab_V2": fist_jab,
    "VFX_Fist_Hook_V2": fist_hook,
    "VFX_Fist_Smash_V2": fist_smash,
    "VFX_Magic_Burst_V2": magic_burst,
    "VFX_Magic_Orb_V2": magic_orb,
    "VFX_Magic_Sigil_V2": magic_sigil,
    "VFX_Spear_Thrust_V2": spear_thrust,
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
