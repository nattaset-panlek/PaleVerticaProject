import os
import json
import math
import subprocess
from PIL import Image

CLI_PATH = r"C:\Program Files\EffectCraft\effectcraft-cli.exe"
TOOLS_DIR = r"F:\GameProject\Pale Vertica\Tools\VfxPreview"
UNITY_VFX_DIR = r"F:\GameProject\Pale Vertica\Assets\Image\VFX"
FRAMES_DIR = os.path.join(TOOLS_DIR, "pure_slash_frames")
PROJECT_PATH = os.path.join(TOOLS_DIR, "pure_slash_mcp.ecproj").replace("\\", "/")

os.makedirs(FRAMES_DIR, exist_ok=True)
os.makedirs(UNITY_VFX_DIR, exist_ok=True)

class EffectCraftMCP:
    def __init__(self, cli_path):
        self.cli_path = cli_path
        self.proc = None
        self.req_id = 0

    def start(self):
        print("Starting EffectCraft MCP Server...")
        self.proc = subprocess.Popen(
            [self.cli_path, "mcp"],
            stdin=subprocess.PIPE,
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            text=True,
            encoding="utf-8"
        )
        res = self.send("initialize", {
            "protocolVersion": "2024-11-05",
            "capabilities": {},
            "clientInfo": {"name": "Stylized2DSlashAgent", "version": "1.0"}
        })
        print("EffectCraft MCP Initialized successfully.")

    def send(self, method, params):
        self.req_id += 1
        msg = {"jsonrpc": "2.0", "id": self.req_id, "method": method, "params": params}
        self.proc.stdin.write(json.dumps(msg) + "\n")
        self.proc.stdin.flush()
        line = self.proc.stdout.readline()
        if not line:
            err = self.proc.stderr.read()
            raise RuntimeError(f"EffectCraft MCP Error: {err}")
        return json.loads(line)

    def call_tool(self, name, args):
        return self.send("tools/call", {"name": name, "arguments": args})

    def close(self):
        if self.proc:
            self.proc.terminate()
            self.proc.wait()
            print("EffectCraft MCP session closed.")

def generate_stylized_crescent(cx, cy, base_r, max_thick, start_deg, end_deg, num_pts=44):
    if abs(start_deg - end_deg) < 1.0:
        end_deg = start_deg + 1.5

    outer_pts = []
    inner_pts = []
    s_rad = math.radians(start_deg)
    e_rad = math.radians(end_deg)

    for i in range(num_pts + 1):
        t = i / num_pts
        ang = s_rad + t * (e_rad - s_rad)

        elliptical_scale = 1.0 + 0.10 * math.cos(ang - math.radians(45))
        r = base_r * elliptical_scale

        if t < 0.75:
            thick = max_thick * ((t / 0.75) ** 0.8)
        else:
            thick = max_thick * (1.0 - ((t - 0.75) / 0.25) ** 1.4)

        ro = r + thick * 0.6
        ri = r - thick * 0.4

        outer_pts.append([round(cx + ro * math.cos(ang), 2), round(cy + ro * math.sin(ang), 2)])
        inner_pts.append([round(cx + ri * math.cos(ang), 2), round(cy + ri * math.sin(ang), 2)])

    return outer_pts + inner_pts[::-1]

# 16-frame 2D Stylized forward swing timeline
FORWARD_TIMELINE = [
    # (tail_deg, head_deg, opacity, thick_mult)
    (-45,  -15,  0.80, 0.70), # 00: Ignition at sword tip
    (-42,   45,  0.95, 0.90), # 01: Fast forward snap
    (-35,  110,  1.00, 1.00), # 02: Explosive expansion
    (-20,  140,  1.00, 1.00), # 03: Full swing apex
    ( 10,  142,  1.00, 0.95), # 04: Tail pulls forward
    ( 40,  143,  0.95, 0.88), # 05: Speed ribbons stretching
    ( 70,  143,  0.90, 0.80), # 06: Tail catching up
    ( 92,  143,  0.80, 0.70), # 07: Thinning wisps
    (110,  143,  0.70, 0.60), # 08: Dissipating arc
    (122,  143,  0.55, 0.50), # 09: Fading trail
    (130,  143,  0.40, 0.40), # 10: Fine wispy strands
    (135,  143,  0.28, 0.30), # 11: Faint residue
    (138,  143,  0.18, 0.20), # 12: Whispers of light
    (140,  143,  0.10, 0.15), # 13: Embers vanishing
    (141,  143,  0.04, 0.10), # 14: Final point
    (142,  143,  0.00, 0.05)  # 15: Blank frame for clean loop
]

def build_stylized_2d_composition(client):
    print("Building 2D Stylized Slash Animation in EffectCraft via MCP...")
    TOTAL_FRAMES = 16
    FPS = 30

    main_shapes  = []
    outer_shapes = []
    inner_shapes = []
    frame_meta   = []

    for f in range(TOTAL_FRAMES):
        tail_ang, head_ang, op, th = FORWARD_TIMELINE[f]
        t_sec = round(f / FPS, 4)

        # 1. Main Blade (max thickness ~6.8px, 15% thickness)
        m_pts = generate_stylized_crescent(0, 0, 85, 6.8 * th, tail_ang, head_ang, 44)

        # 2. Outer Speed Streak (radius +11px, thickness ~2.2px)
        out_tail = tail_ang + (head_ang - tail_ang) * 0.30
        out_head = head_ang + 6
        o_pts = generate_stylized_crescent(0, 0, 96, 2.2 * th, out_tail, out_head, 32)

        # 3. Inner Speed Streak (radius -11px, thickness ~1.8px)
        in_tail = tail_ang + (head_ang - tail_ang) * 0.15
        in_head = head_ang - 8
        i_pts = generate_stylized_crescent(0, 0, 74, 1.8 * th, in_tail, in_head, 32)

        main_shapes.append(m_pts)
        outer_shapes.append(o_pts)
        inner_shapes.append(i_pts)

        frame_meta.append({
            "time": t_sec,
            "opacity": round(op * 100, 1)
        })

    jsx_script = f"""
    (function() {{
        var compW = 256;
        var compH = 256;
        var fps = 30;
        var duration = 16 / fps;

        var comp = app.project.items.addComp("VFX_Pure_Slash", compW, compH, 1.0, duration, fps);
        comp.bgColor = [0, 0, 0];

        var mainShapes  = {json.dumps(main_shapes)};
        var outerShapes = {json.dumps(outer_shapes)};
        var innerShapes = {json.dumps(inner_shapes)};
        var frameMeta   = {json.dumps(frame_meta)};

        function addAnimLayer(name, shapes) {{
            var layer = comp.layers.addShape();
            layer.name = name;

            var contents = layer.property("Contents");
            var group = contents.addProperty("ADBE Vector Group");
            var gContents = group.property("Contents");

            var pathProp = gContents.addProperty("ADBE Vector Shape - Group").property("Path");

            for (var i = 0; i < 16; i++) {{
                var s = new Shape();
                s.vertices = shapes[i];
                s.closed = true;
                pathProp.setValueAtTime(frameMeta[i].time, s);
            }}

            // Pure White Solid Fill, ZERO Blur, ZERO Glow
            var fill = gContents.addProperty("ADBE Vector Graphic - Fill");
            fill.property("Color").setValue([1.0, 1.0, 1.0, 1.0]);

            var transform = layer.property("Transform");
            transform.property("Position").setValue([compW * 0.5, compH * 0.5 - 25]);

            var opProp = transform.property("Opacity");
            for (var i = 0; i < 16; i++) {{
                opProp.setValueAtTime(frameMeta[i].time, frameMeta[i].opacity);
            }}

            return layer;
        }}

        addAnimLayer("01_MainBlade", mainShapes);
        addAnimLayer("02_OuterStreak", outerShapes);
        addAnimLayer("03_InnerStreak", innerShapes);

        return "2D_Stylized_Slash_Created";
    }})();
    """

    res = client.call_tool("run_script", {"code": jsx_script})
    print("Script result:", res.get("result", {}))

    save_res = client.call_tool("save_project", {"path": PROJECT_PATH})
    print("Project saved to:", PROJECT_PATH)

def render_and_generate_assets(client):
    print("Rendering 16 Frames via EffectCraft MCP...")
    frames = []

    for f in range(16):
        t = f / 30.0
        frame_file = os.path.join(FRAMES_DIR, f"pure_slash_{f:02d}.png").replace("\\", "/")
        client.call_tool("render_frame", {
            "comp": "VFX_Pure_Slash",
            "time": t,
            "path": frame_file,
            "transparent": True
        })
        im = Image.open(frame_file).convert("RGBA")
        frames.append(im)
        print(f"Rendered Frame {f:02d} ({t:.3f}s)")

    print("Assembling 1024x1024 (4x4) Sprite Sheet...")
    FRAME_SIZE = 256
    sheet = Image.new("RGBA", (4 * FRAME_SIZE, 4 * FRAME_SIZE), (0, 0, 0, 0))

    for idx, frm in enumerate(frames):
        c = idx % 4
        r = idx // 4
        sheet.paste(frm, (c * FRAME_SIZE, r * FRAME_SIZE))

    sheet_tools = os.path.join(TOOLS_DIR, "VFX_Pure_Slash_Sheet.png")
    sheet_unity = os.path.join(UNITY_VFX_DIR, "VFX_Pure_Slash_Sheet.png")
    sheet.save(sheet_tools)
    sheet.save(sheet_unity)
    print(f"Saved Sprite Sheet -> {sheet_unity}")

    # Generate Pure White Preview GIF
    gif_frames = []
    for frm in frames:
        bg = Image.new("RGBA", (FRAME_SIZE, FRAME_SIZE), (12, 14, 18, 255))
        bg.alpha_composite(frm)
        gif_frames.append(bg.convert("RGB"))

    gif_path = os.path.join(TOOLS_DIR, "VFX_Pure_Slash_Preview.gif")
    gif_frames[0].save(
        gif_path,
        save_all=True,
        append_images=gif_frames[1:],
        duration=int(1000 / 30),
        loop=0
    )
    print(f"Saved {gif_path}")

    # Generate color previews (pure white base tinted for game engine preview)
    def apply_color_tint(hex_rgb, out_filename):
        r_mult, g_mult, b_mult = hex_rgb
        tint_frames = []
        for frm in frames:
            r, g, b, a = frm.split()
            tr = r.point(lambda v: int(v * (r_mult / 255.0)))
            tg = g.point(lambda v: int(v * (g_mult / 255.0)))
            tb = b.point(lambda v: int(v * (b_mult / 255.0)))
            tinted = Image.merge("RGBA", (tr, tg, tb, a))

            bg = Image.new("RGBA", (FRAME_SIZE, FRAME_SIZE), (12, 14, 18, 255))
            bg.alpha_composite(tinted)
            tint_frames.append(bg.convert("RGB"))

        out_path = os.path.join(TOOLS_DIR, out_filename)
        tint_frames[0].save(
            out_path,
            save_all=True,
            append_images=tint_frames[1:],
            duration=int(1000 / 30),
            loop=0
        )
        print(f"Saved {out_filename}")

    apply_color_tint((255, 140, 20), "VFX_Slash_Tint_Fire_Orange.gif")
    apply_color_tint((56, 189, 248), "VFX_Pure_Slash_Cyan.gif")
    apply_color_tint((250, 204, 21), "VFX_Slash_Tint_Holy_Gold.gif")
    apply_color_tint((192, 132, 252), "VFX_Slash_Tint_Void_Purple.gif")

def main():
    client = EffectCraftMCP(CLI_PATH)
    try:
        client.start()
        build_stylized_2d_composition(client)
        render_and_generate_assets(client)
        print("2D Stylized Slash Generation Complete via EffectCraft MCP!")
    finally:
        client.close()

if __name__ == "__main__":
    main()
