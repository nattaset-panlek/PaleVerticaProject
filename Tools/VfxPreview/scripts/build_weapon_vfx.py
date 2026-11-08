import argparse
import math
import os
import random

from vfx_effectcraft import (FRAMES, SIZE, TOOLS_DIR, PROJECT_ROOT, burst_particles, build, clamp01, crescent, diamond,
                             ease_in_out_cubic, ease_out_back, ease_out_expo, export_to_unity, fade, keyed, lance, life,
                             poly_layer, ring_poly, shape_layer, star, window)

SET_NAME = "WeaponClean"
C = SIZE * 0.5


def impact_star(name, born, size, points=8, hold=1, length=5, rot0=0.0, spin=20.0, inner=0.28, blur=0):
    shape = star(points, size, size * inner, rot0)
    scale = keyed(lambda f: [0, 0] if f < born else [round(110 * ease_out_back(life(f, born, 2)) * (1 - 0.35 * life(f, born + hold, length)), 1)] * 2)
    rot = keyed(lambda f: round(spin * life(f, born, length + hold), 2))
    return shape_layer(name, shape, [[C, C]] * FRAMES, keyed(lambda f: window(f, born, hold, length)), scale, rot, blur=blur)


def shock_ring(name, born, r0, r1, thick0, length, sx=1.0, sy=1.0, blur=0):
    def path(f):
        t = ease_out_expo(life(f, born, length))
        return ring_poly(r0 + (r1 - r0) * t, max(0.6, thick0 * (1 - t) ** 1.2), 48, sx, sy)
    return poly_layer(name, keyed(path), keyed(lambda f: fade(f, born, length, 1.2) if f >= born else 0.0), blur=blur, add=True)


def glow_disc(name, born, radius, length, blur=10):
    def disc(f):
        r = radius * (0.55 + 0.45 * ease_out_expo(life(f, born, length)))
        return [[round(r * math.cos(2 * math.pi * i / 32), 2), round(r * math.sin(2 * math.pi * i / 32), 2)] for i in range(32)]
    return poly_layer(name, keyed(disc), keyed(lambda f: round(fade(f, born, length) * 0.45, 1) if f >= born else 0.0), blur=blur, add=True)


def speed_lines(prefix, count, seed, travel, born, length, y_spread, line_len):
    rng = random.Random(seed)
    layers = []
    for i in range(count):
        y = rng.uniform(-y_spread, y_spread)
        b = born + rng.uniform(0, 1.0)
        ln = line_len * rng.uniform(0.6, 1.0)
        def path(f, y=y, b=b, ln=ln):
            t = ease_out_expo(life(f, b, length))
            head = -travel + travel * 0.9 * t - abs(y) * 0.4
            tail = head - ln * (1 - t * 0.7)
            return lance(tail, head, 1.4 * (1 - t * 0.6) + 0.3, y, n=6)
        layers.append(poly_layer("%s_%d" % (prefix, i + 1), keyed(path), keyed(lambda f, b=b: window(f, b, 1, length)), add=True))
    return layers


def fist_jab():
    layers = [glow_disc("00_Glow", 1, 46, 6)]
    layers += speed_lines("01_Speed", 6, 3, 100, 0, 5, 22, 70)
    layers.append(shock_ring("02_Ring", 1, 8, 58, 9, 8, sx=0.55, sy=1.0))
    layers.append(impact_star("03_Star", 1, 34, 8, 1, 5, 0, 25))
    layers.append(impact_star("04_StarCore", 1, 18, 4, 1, 4, 45, -15, 0.2))
    layers += burst_particles("05_Debris", 8, 5, [C, C], 2, 0, 110, 70, 9, 10, 1.6, 2)
    return layers


def fist_hook():
    r = 58
    end = -25.0
    cx, cy = C - r * math.cos(math.radians(end)), C - r * math.sin(math.radians(end))
    head = keyed(lambda f: 160 + (end + 360 - 160) * ease_out_expo(life(f, -0.5, 3.5)))
    tail = keyed(lambda f: 165 + (end + 360 - 8 - 165) * ease_in_out_cubic(life(f, 1, 8)))
    arc = keyed(lambda f: crescent(r, 13 * (0.5 + 0.5 * clamp01((head[f] - tail[f]) / 120)), tail[f], head[f], peak=0.78, n=36))
    core = keyed(lambda f: crescent(r + 2, 3.5, max(tail[f], head[f] - 60), head[f] - 1, peak=0.8, bias=0.85, n=24))
    layers = [
        poly_layer("00_ArcGlow", arc, keyed(lambda f: round(window(f, 0, 4, 7) * 0.45, 1)), blur=6, add=True, center=[cx, cy]),
        poly_layer("01_Arc", arc, keyed(lambda f: window(f, 0, 4, 7)), center=[cx, cy]),
        poly_layer("02_ArcCore", core, keyed(lambda f: window(f, 0, 2, 5)), center=[cx, cy]),
        shock_ring("03_Ring", 3, 6, 46, 8, 7),
        impact_star("04_Star", 3, 30, 6, 1, 5, 15, 30),
    ]
    layers += burst_particles("05_Spark", 7, 9, [C, C], 3, 10, 120, 60, 8, 11, 1.5, 1)
    return layers


def fist_smash():
    layers = [glow_disc("00_Glow", 0, 70, 7, 14)]
    layers.append(shock_ring("01_RingWide", 1, 10, 105, 14, 10, 1.0, 0.45, blur=2))
    layers.append(shock_ring("02_RingInner", 2, 6, 70, 8, 8, 1.0, 0.45))
    layers.append(impact_star("03_Star", 0, 52, 10, 2, 6, 0, 12, 0.22))
    rng = random.Random(21)
    for i in range(9):
        ang = rng.uniform(190, 350)
        dist = rng.uniform(45, 85)
        b = rng.randint(1, 3)
        sz = rng.uniform(4, 8)
        def pos(f, ang=ang, dist=dist, b=b):
            t = life(f, b, 10)
            x = C + math.cos(math.radians(ang)) * dist * ease_out_expo(t)
            y = C + 10 + math.sin(math.radians(ang)) * dist * 0.6 * ease_out_expo(t) + 70 * t * t
            return [round(x, 2), round(y, 2)]
        layers.append(shape_layer("04_Rock_%d" % (i + 1), star(3, sz, sz * 0.6, rng.uniform(0, 120)), keyed(pos), keyed(lambda f, b=b: window(f, b, 3, 7)), None, keyed(lambda f, b=b: round(f * 25.0, 1)), add=False))
    return layers


def fist_rush():
    rng = random.Random(4)
    layers = []
    for k in range(4):
        b = k * 2.5
        ox, oy = C + rng.uniform(-38, 38), C + rng.uniform(-30, 30)
        sz = rng.uniform(18, 26)
        shape = star(6, sz, sz * 0.3, rng.uniform(0, 60))
        scale = keyed(lambda f, b=b: [0, 0] if f < b else [round(110 * ease_out_back(life(f, b, 1.5)) * (1 - 0.4 * life(f, b + 1, 4)), 1)] * 2)
        layers.append(shape_layer("0%d_Hit" % k, shape, [[ox, oy]] * FRAMES, keyed(lambda f, b=b: window(f, b, 1, 4)), scale, None))
        ring = keyed(lambda f, b=b, ox=ox, oy=oy: ring_poly(4 + 30 * ease_out_expo(life(f, b, 5)), max(0.6, 5 * (1 - life(f, b, 5))), 32))
        layers.append(poly_layer("0%d_Ring" % k, ring, keyed(lambda f, b=b: fade(f, b, 5, 1.2) if f >= b else 0.0), add=True, center=[ox, oy]))
    layers += speed_lines("05_Speed", 8, 12, 110, 0, 10, 40, 60)
    return layers


def magic_burst():
    layers = [glow_disc("00_Glow", 0, 50, 7, 12)]
    layers.append(shock_ring("01_Ring", 1, 10, 80, 6, 9))
    layers.append(shock_ring("02_Ring2", 3, 6, 60, 3, 9))
    rng = random.Random(2)
    for i in range(10):
        ang = i * 36 + rng.uniform(-8, 8)
        ln = rng.uniform(40, 62)
        b = rng.uniform(0, 1.5)
        def path(f, ang=ang, ln=ln, b=b):
            t = ease_out_expo(life(f, b, 6))
            head = 10 + ln * t
            tail = 10 + ln * t * t * 0.9
            pts = lance(tail, head, 2.4 * (1 - t) + 0.4, 0, n=6)
            a = math.radians(ang)
            return [[round(x * math.cos(a) - y * math.sin(a), 2), round(x * math.sin(a) + y * math.cos(a), 2)] for x, y in pts]
        layers.append(poly_layer("03_Ray_%d" % i, keyed(path), keyed(lambda f, b=b: window(f, b, 1, 6)), add=True))
    layers.append(impact_star("04_Core", 0, 26, 4, 1, 4, 45, 45, 0.18))
    for i in range(7):
        ang = math.radians(rng.uniform(0, 360))
        d = rng.uniform(45, 80)
        b = rng.randint(2, 5)
        sz = rng.uniform(4, 7)
        pos = keyed(lambda f, ang=ang, d=d, b=b: [round(C + math.cos(ang) * d * ease_out_expo(life(f, b, 9)), 2), round(C + math.sin(ang) * d * ease_out_expo(life(f, b, 9)) - 12 * life(f, b, 9), 2)])
        twinkle = keyed(lambda f, b=b, i=i: 0.0 if f < b else round(100 * (1 - life(f, b, 10)) * (0.55 + 0.45 * math.cos((f + i) * 1.7)), 1))
        layers.append(shape_layer("05_Twinkle_%d" % i, star(4, sz, sz * 0.22), pos, twinkle, None, None))
    return layers


def magic_sigil():
    layers = []
    draw = lambda f: ease_in_out_cubic(life(f, 0, 6))
    def arc_ring(r, th, rot0, direction):
        return keyed(lambda f: crescent(r, th, rot0 + direction * 120 * life(f, 0, 15), rot0 + direction * 120 * life(f, 0, 15) + 359 * max(0.01, draw(f)), peak=0.5, bias=0.5, n=72) if direction > 0 else crescent(r, th, rot0 - 120 * life(f, 0, 15) - 359 * max(0.01, draw(f)), rot0 - 120 * life(f, 0, 15), peak=0.5, bias=0.5, n=72))
    scale_out = lambda f: 1.0 + 0.25 * ease_out_expo(life(f, 10, 5))
    op = keyed(lambda f: window(f, 0, 9, 6))
    outer = arc_ring(82, 3.2, -90, 1)
    inner = arc_ring(62, 2.0, 90, -1)
    layers.append(poly_layer("00_Outer", [[[x * scale_out(f), y * scale_out(f)] for x, y in outer[f]] for f in range(FRAMES)], op, add=True))
    layers.append(poly_layer("01_Inner", [[[x * scale_out(f), y * scale_out(f)] for x, y in inner[f]] for f in range(FRAMES)], op, add=True))
    for k in range(2):
        def tri(f, k=k):
            rot = math.radians(-90 + k * 60 + 40 * life(f, 0, 15) * (1 if k == 0 else -1))
            r = 60 * ease_out_back(life(f, 2 + k, 4)) * scale_out(f)
            pts = []
            for j in range(3):
                a0 = rot + j * 2 * math.pi / 3
                a1 = rot + (j + 1) * 2 * math.pi / 3
                x0, y0, x1, y1 = r * math.cos(a0), r * math.sin(a0), r * math.cos(a1), r * math.sin(a1)
                pts.append((x0, y0, x1, y1))
            out = []
            for x0, y0, x1, y1 in pts:
                out.append([round(x0, 2), round(y0, 2)])
                out.append([round(x1, 2), round(y1, 2)])
            inner_pts = [[round(x * 0.9, 2), round(y * 0.9, 2)] for x, y in out]
            return out + inner_pts[::-1]
        layers.append(poly_layer("02_Tri_%d" % k, keyed(tri), keyed(lambda f, k=k: 0.0 if f < 2 + k else window(f, 2 + k, 7 - k, 6)), add=True))
    for i in range(8):
        def rune(f, i=i):
            a = math.radians(i * 45 - 60 * life(f, 0, 15))
            r = 72 * scale_out(f)
            x, y = r * math.cos(a), r * math.sin(a)
            return [round(C + x, 2), round(C + y, 2)]
        layers.append(shape_layer("03_Rune_%d" % i, [[-2.2, -5], [2.2, -5], [2.2, 5], [-2.2, 5]], keyed(rune), keyed(lambda f, i=i: 0.0 if f < 3 + i * 0.4 else window(f, 3 + i * 0.4, 5, 5)), None, keyed(lambda f, i=i: round(i * 45 - 60 * life(f, 0, 15) + 90, 2))))
    layers.append(glow_disc("04_Flash", 9, 60, 6, 12))
    layers.append(impact_star("05_Flash", 9, 40, 4, 1, 4, 45, 30, 0.15))
    return layers


def magic_orb():
    rng = random.Random(8)
    layers = []
    for i in range(14):
        a0 = rng.uniform(0, 360)
        r0 = rng.uniform(70, 105)
        spin = rng.uniform(140, 220)
        sz = rng.uniform(2.5, 5)
        def pos(f, a0=a0, r0=r0, spin=spin):
            t = ease_in_out_cubic(life(f, 0, 7))
            a = math.radians(a0 + spin * t)
            r = r0 * (1 - t) + 4
            return [round(C + r * math.cos(a), 2), round(C + r * math.sin(a), 2)]
        layers.append(shape_layer("00_Mote_%d" % i, star(4, sz, sz * 0.3), keyed(pos), keyed(lambda f: 0.0 if f > 7 else round(40 + 60 * life(f, 0, 6), 1)), None, None))
    def orb(f):
        g = ease_out_expo(life(f, 0, 7)) * (1 - life(f, 8, 2))
        return ring_poly(g * 9 + 1, g * 18 + 2, 32)
    layers.append(poly_layer("01_Orb", keyed(orb), keyed(lambda f: 100.0 if f <= 8 else 0.0), add=True))
    layers.append(poly_layer("01_OrbGlow", keyed(orb), keyed(lambda f: 70.0 if f <= 8 else 0.0), blur=8, add=True))
    layers.append(glow_disc("02_PopGlow", 8, 70, 6, 14))
    layers.append(shock_ring("03_Pop", 8, 10, 90, 10, 7))
    layers.append(impact_star("04_PopStar", 8, 44, 6, 1, 5, 0, 30, 0.18))
    layers += burst_particles("05_Shard", 10, 13, [C, C], 8, 0, 360, 85, 7, 9, 1.6, 0)
    return layers


def magic_shard():
    rng = random.Random(31)
    layers = [glow_disc("00_Glow", 0, 40, 5, 10)]
    for i in range(9):
        ang = i * 40 + rng.uniform(-10, 10)
        ln = rng.uniform(22, 34)
        b = rng.uniform(0, 2)
        d = rng.uniform(55, 80)
        def pos(f, ang=ang, d=d, b=b):
            t = ease_out_expo(life(f, b, 6))
            return [round(C + math.cos(math.radians(ang)) * d * t, 2), round(C + math.sin(math.radians(ang)) * d * t, 2)]
        shape = [[-ln * 0.35, 0], [0, -ln * 0.16], [ln * 0.65, 0], [0, ln * 0.16]]
        scale = keyed(lambda f, b=b: [0, 0] if f < b else [round(100 * ease_out_back(life(f, b, 2)), 1), round(100 * (1 - 0.6 * life(f, b + 4, 6)), 1)])
        layers.append(shape_layer("01_Shard_%d" % i, shape, keyed(pos), keyed(lambda f, b=b: window(f, b, 4, 6)), scale, [round(ang, 2)] * FRAMES, add=False))
    layers.append(shock_ring("02_Ring", 0, 8, 70, 4, 7))
    layers.append(impact_star("03_Core", 0, 30, 6, 1, 5, 0, 60, 0.3))
    return layers


def spear_thrust():
    head = keyed(lambda f: -95 + 135 * ease_out_expo(life(f, -0.4, 3.5)))
    tail = keyed(lambda f: -120 + 150 * ease_in_out_cubic(life(f, 1, 9)))
    width = keyed(lambda f: 7.5 * (0.45 + 0.55 * clamp01((head[f] - tail[f]) / 110.0)))
    layers = [
        poly_layer("00_Glow", keyed(lambda f: lance(tail[f], head[f] + 4, width[f] * 2.2, 0, 0.25)), keyed(lambda f: round(window(f, 0, 3, 7) * 0.45, 1)), blur=6, add=True),
        poly_layer("01_Lance", keyed(lambda f: lance(tail[f], head[f], width[f], 0, 0.22)), keyed(lambda f: window(f, 0, 3, 7))),
        poly_layer("02_SideTop", keyed(lambda f: lance(tail[max(0, f - 1)] + 20, head[max(0, f - 1)] - 18, 1.6, -14, 0.3)), keyed(lambda f: window(f, 0, 2, 6)), add=True),
        poly_layer("03_SideBottom", keyed(lambda f: lance(tail[max(0, f - 1)] + 30, head[max(0, f - 1)] - 26, 1.4, 13, 0.3)), keyed(lambda f: window(f, 0, 2, 6)), add=True),
        shock_ring("04_Pierce", 2, 6, 44, 7, 7, 0.4, 1.0),
        impact_star("05_Tip", 2, 22, 4, 1, 4, 0, 0, 0.16),
    ]
    layers += burst_particles("06_Spark", 7, 17, [C + 30, C], 2, 0, 70, 70, 8, 12, 1.4, 2)
    return layers


RECIPES = {
    "VFX_Fist_Jab": fist_jab,
    "VFX_Fist_Hook": fist_hook,
    "VFX_Fist_Smash": fist_smash,
    "VFX_Fist_Rush": fist_rush,
    "VFX_Magic_Burst": magic_burst,
    "VFX_Magic_Sigil": magic_sigil,
    "VFX_Magic_Orb": magic_orb,
    "VFX_Magic_Shard": magic_shard,
    "VFX_Spear_Thrust": spear_thrust,
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
