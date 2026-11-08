"""Print a Figma node JSON (from Talk to Figma get_node_info) as a readable tree.

Usage:
    python -I figma_tree.py node.json [max_depth] [--window "Node Name"]

Positions are relative to the root frame, or to the node named by --window
(use the window frame so numbers can go straight into SkillManagePrefabBuilder.Place).
"""
import json
import sys

sys.stdout.reconfigure(encoding="utf-8")


def paints(items):
    out = []
    for p in items or []:
        if p.get("visible") is False:
            continue
        if p.get("type") == "SOLID":
            opacity = p.get("opacity")
            out.append(p.get("color", "") + (f"@{opacity:.2f}" if opacity not in (None, 1) else ""))
        elif p.get("type") == "GRADIENT_LINEAR":
            stops = "/".join(s.get("color", "") for s in p.get("gradientStops", []))
            out.append(f"GRADIENT({stops})")
        else:
            out.append(p.get("type"))
    return ",".join(out)


def find(node, name):
    if node.get("name") == name:
        return node
    for child in node.get("children") or []:
        hit = find(child, name)
        if hit:
            return hit
    return None


def walk(node, depth, origin, max_depth):
    if depth > max_depth:
        return
    box = node.get("absoluteBoundingBox") or {}
    x = box.get("x", 0) - origin[0]
    y = box.get("y", 0) - origin[1]
    line = f"{'  ' * depth}{node.get('name')} [{node.get('type')}] x={x:.0f} y={y:.0f} {box.get('width', 0):.0f}x{box.get('height', 0):.0f}"
    if node.get("fills"):
        line += f" fill={paints(node['fills'])}"
    if node.get("strokes"):
        line += f" stroke={paints(node['strokes'])}"
    if node.get("cornerRadius"):
        line += f" r={node['cornerRadius']}"
    if node.get("characters"):
        st = node.get("style", {})
        text = node["characters"].replace("\n", " ")[:40]
        line += (f" text='{text}' font={st.get('fontFamily')} w={st.get('fontWeight')} size={st.get('fontSize')}"
                 f" ls={st.get('letterSpacing')} lh={st.get('lineHeightPx')} align={st.get('textAlignHorizontal')}")
    print(line)
    for child in node.get("children") or []:
        walk(child, depth + 1, origin, max_depth)


def main():
    args = [a for a in sys.argv[1:]]
    window = None
    if "--window" in args:
        i = args.index("--window")
        window = args[i + 1]
        del args[i:i + 2]
    root = json.load(open(args[0], encoding="utf-8"))
    max_depth = int(args[1]) if len(args) > 1 else 99
    origin_node = find(root, window) if window else root
    if origin_node is None:
        sys.exit(f"Node '{window}' not found")
    box = origin_node.get("absoluteBoundingBox", {})
    walk(root, 0, (box.get("x", 0), box.get("y", 0)), max_depth)


if __name__ == "__main__":
    main()
