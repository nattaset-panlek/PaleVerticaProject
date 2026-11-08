import os
import json
import subprocess
from PIL import Image

CLI_PATH = r"C:\Program Files\EffectCraft\effectcraft-cli.exe"
OUTPUT_DIR = r"F:\GameProject\Pale Vertica\Tools\VfxPreview"
UNITY_VFX_DIR = r"F:\GameProject\Pale Vertica\Assets\Image\VFX"
FRAMES_DIR = os.path.join(OUTPUT_DIR, "pure_slash_frames")
PROJECT_PATH = os.path.join(OUTPUT_DIR, "pure_slash_mcp.ecproj").replace("\\", "/")

os.makedirs(FRAMES_DIR, exist_ok=True)
os.makedirs(UNITY_VFX_DIR, exist_ok=True)

class EffectCraftMCPClient:
    def __init__(self, cli_path):
        self.cli_path = cli_path
        self.process = None
        self.req_id = 0

    def start(self):
        print("Launching EffectCraft MCP Server...")
        self.process = subprocess.Popen(
            [self.cli_path, "mcp"],
            stdin=subprocess.PIPE,
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            text=True,
            encoding="utf-8"
        )
        # MCP Handshake
        init_res = self.send("initialize", {
            "protocolVersion": "2024-11-05",
            "capabilities": {},
            "clientInfo": {"name": "AntigravityAgent", "version": "1.0"}
        })
        print("MCP Initialized successfully:", init_res.get("result", {}).get("instructions", "")[:60] + "...")

    def send(self, method, params):
        self.req_id += 1
        msg = {
            "jsonrpc": "2.0",
            "id": self.req_id,
            "method": method,
            "params": params
        }
        self.process.stdin.write(json.dumps(msg) + "\n")
        self.process.stdin.flush()
        line = self.process.stdout.readline()
        if not line:
            err = self.process.stderr.read()
            raise RuntimeError(f"MCP server died: {err}")
        return json.loads(line)

    def call_tool(self, tool_name, arguments):
        res = self.send("tools/call", {
            "name": tool_name,
            "arguments": arguments
        })
        return res

    def close(self):
        if self.process:
            self.process.terminate()
            self.process.wait()
            print("EffectCraft MCP session closed.")

def build_pure_slash_vfx(client):
    print("Building Pure Attack Slash VFX (No hit blink / No impact flash)...")
    
    # ExtendScript code executed via MCP `run_script`
    jsx_code = """
    (function() {
        var compW = 256;
        var compH = 256;
        var fps = 30;
        var duration = 16 / fps; // 16 frames = 0.533s

        var comp = app.project.items.addComp("VFX_Pure_Slash", compW, compH, 1.0, duration, fps);
        comp.bgColor = [0, 0, 0];

        // Helper to construct curved energy sweep arcs
        function addEnergyArc(name, radius, strokeWidth, blurRadius, glowRadius, glowIntensity, startT, peakT, endT, startAng, sweepAng, rotOffset, opacityPeak) {
            var layer = comp.layers.addShape();
            layer.name = name;

            var contents = layer.property("Contents");
            var group = contents.addProperty("ADBE Vector Group");
            group.name = "ArcGroup";
            var gContents = group.property("Contents");

            var ellipse = gContents.addProperty("ADBE Vector Shape - Ellipse");
            ellipse.property("Size").setValue([radius * 2, radius * 2]);
            ellipse.property("Position").setValue([0, 0]);

            var stroke = gContents.addProperty("ADBE Vector Graphic - Stroke");
            stroke.property("Color").setValue([1, 1, 1, 1]); // Pure White
            stroke.property("Stroke Width").setValue(strokeWidth);
            stroke.property("Line Cap").setValue(2); // Round Cap
            stroke.property("Line Join").setValue(2); // Round Join

            var trim = gContents.addProperty("ADBE Vector Filter - Trim");
            trim.property("Offset").setValue(startAng);

            // Animate head and tail of slash
            var endProp = trim.property("End");
            endProp.setValueAtTime(startT, 0);
            endProp.setValueAtTime(peakT, sweepAng);
            endProp.setValueAtTime(endT, sweepAng);

            var startProp = trim.property("Start");
            startProp.setValueAtTime(startT, 0);
            startProp.setValueAtTime(peakT * 0.9, 0);
            startProp.setValueAtTime(endT, sweepAng);

            var transform = layer.property("Transform");
            transform.property("Position").setValue([compW * 0.5 + 5, compH * 0.5]);
            transform.property("Rotation").setValue(rotOffset);

            var opProp = transform.property("Opacity");
            opProp.setValueAtTime(startT, 0);
            opProp.setValueAtTime(startT + 0.03, opacityPeak);
            opProp.setValueAtTime(peakT, opacityPeak);
            opProp.setValueAtTime(endT, 0);

            // Additive light blur & glow
            if (blurRadius > 0) {
                var blur = layer.property("Effects").addProperty("Gaussian Blur");
                blur.property("Blurriness").setValue(blurRadius);
            }
            if (glowRadius > 0) {
                var glow = layer.property("Effects").addProperty("Glow");
                glow.property("Glow Radius").setValue(glowRadius);
                glow.property("Glow Intensity").setValue(glowIntensity);
            }

            return layer;
        }

        // 1. Wide Ambient Volumetric Bloom (Outer soft feathered halo)
        addEnergyArc("01_Volumetric_Bloom", 90, 36, 16.0, 30, 1.8, 0.02, 0.16, 0.44, -40, 165, 10, 45);

        // 2. Mid Energy Body Stream (Plasmatic filament body)
        addEnergyArc("02_Energy_Stream", 90, 18, 5.0, 14, 2.0, 0.01, 0.15, 0.40, -40, 165, 10, 85);

        // 3. Secondary Echo Ribbon (Parallel inner stream for striation / depth)
        addEnergyArc("03_Inner_Echo_Ribbon", 74, 9, 2.5, 6, 1.4, 0.03, 0.17, 0.38, -35, 145, 12, 70);

        // 4. Razor Sharp Core Spine (Pure white 100% luminous cutting edge)
        addEnergyArc("04_Razor_Core", 90, 5, 0.0, 0, 0.0, 0.00, 0.14, 0.36, -40, 165, 10, 100);

        // 5. Outer Accent Whip (Leading fine edge whisps)
        addEnergyArc("05_Accent_Whip", 96, 2.5, 1.0, 4, 1.5, 0.02, 0.14, 0.32, -38, 155, 10, 90);

        return "VFX_Pure_Slash_Created";
    })();
    """

    res = client.call_tool("run_script", {"code": jsx_code})
    print("Script execution result:", res.get("result", {}))

    # Save Project via MCP
    save_res = client.call_tool("save_project", {"path": PROJECT_PATH})
    print("Project saved via MCP:", save_res.get("result", {}))

def render_and_assemble(client):
    print("Rendering frames via EffectCraft MCP...")
    frames = []

    for f in range(16):
        t = f / 30.0
        frame_file = os.path.join(FRAMES_DIR, f"pure_slash_{f:02d}.png").replace("\\", "/")
        
        # Render frame via MCP
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
    GRID_COLS = 4
    GRID_ROWS = 4
    FRAME_SIZE = 256
    sheet = Image.new("RGBA", (GRID_COLS * FRAME_SIZE, GRID_ROWS * FRAME_SIZE), (0, 0, 0, 0))

    for idx, frm in enumerate(frames):
        c = idx % GRID_COLS
        r = idx // GRID_COLS
        sheet.paste(frm, (c * FRAME_SIZE, r * FRAME_SIZE))

    sheet_tools = os.path.join(OUTPUT_DIR, "VFX_Pure_Slash_Sheet.png")
    sheet_unity = os.path.join(UNITY_VFX_DIR, "VFX_Pure_Slash_Sheet.png")
    sheet.save(sheet_tools)
    sheet.save(sheet_unity)
    print(f"Saved Sprite Sheet -> {sheet_unity}")

    # Save Grayscale Base Preview GIF
    gif_frames = []
    for f in frames:
        bg = Image.new("RGBA", (FRAME_SIZE, FRAME_SIZE), (12, 14, 18, 255))
        bg.alpha_composite(f)
        gif_frames.append(bg.convert("RGB"))

    gif_path = os.path.join(OUTPUT_DIR, "VFX_Pure_Slash_Preview.gif")
    gif_frames[0].save(
        gif_path,
        save_all=True,
        append_images=gif_frames[1:],
        duration=int(1000 / 30),
        loop=0
    )
    print(f"Saved Preview GIF -> {gif_path}")

    # Save Electric Cyan Demo (Clean Slash without hit blink)
    cyan_frames = []
    for f in frames:
        r, g, b, a = f.split()
        # Electric Cyan: White hot core stays pure white, glowing halo turns electric blue/cyan
        tr = r.point(lambda v: int(v * 0.70 + (255 if v > 210 else 40) * 0.30))
        tg = g.point(lambda v: int(v * 0.85 + (255 if v > 210 else 200) * 0.15))
        tb = b.point(lambda v: int(v * 1.0))
        tinted = Image.merge("RGBA", (tr, tg, tb, a))

        bg = Image.new("RGBA", (FRAME_SIZE, FRAME_SIZE), (12, 14, 18, 255))
        bg.alpha_composite(tinted)
        cyan_frames.append(bg.convert("RGB"))

    cyan_gif = os.path.join(OUTPUT_DIR, "VFX_Pure_Slash_Cyan.gif")
    cyan_frames[0].save(
        cyan_gif,
        save_all=True,
        append_images=cyan_frames[1:],
        duration=int(1000 / 30),
        loop=0
    )
    print(f"Saved Cyan Demo -> {cyan_gif}")

def main():
    client = EffectCraftMCPClient(CLI_PATH)
    try:
        client.start()
        build_pure_slash_vfx(client)
        render_and_assemble(client)
        print("MCP Workflow Completed Successfully!")
    finally:
        client.close()

if __name__ == "__main__":
    main()
