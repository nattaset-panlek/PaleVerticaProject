"""Print a Unity prefab's hierarchy, with RectTransform geometry and key component values.

Usage:
    python -I prefab_tree.py "Assets/Prefab/....prefab" [--comps]

Read-only. Use it to understand a prefab before an editor builder script changes it.
Never edit prefab YAML by hand; change prefabs through PrefabUtility in an editor script.
"""
import re
import sys

sys.stdout.reconfigure(encoding="utf-8")

KEYS = ("m_Sprite", "m_Color", "m_Type", "m_PixelsPerUnitMultiplier", "m_text", "m_fontSize", "m_fontColor",
        "m_characterSpacing", "m_fontStyle", "m_enableAutoSizing", "m_Spacing", "m_ChildControlWidth",
        "m_ChildControlHeight", "m_ChildForceExpandWidth", "m_ChildForceExpandHeight", "m_HorizontalFit",
        "m_VerticalFit", "m_PreferredWidth", "m_PreferredHeight", "m_FlexibleWidth", "m_FlexibleHeight")


def main():
    if len(sys.argv) < 2:
        sys.exit(__doc__)
    text = open(sys.argv[1], encoding="utf-8").read()
    show_comps = "--comps" in sys.argv
    docs = {}
    for doc in re.split(r"\n--- ", text):
        m = re.match(r"!u!(\d+) &(-?\d+)( stripped)?", doc)
        if m:
            docs[m.group(2)] = (m.group(1), doc, bool(m.group(3)))

    names, rect_of_go, go_of_rect, children, parent = {}, {}, {}, {}, {}
    instances = {}
    for fid, (cls, doc, stripped) in docs.items():
        if cls == "1" and not stripped:
            m = re.search(r"m_Name: (.*)", doc)
            names[fid] = m.group(1) if m else "?"
        if cls in ("224", "4") and not stripped:
            go = re.search(r"m_GameObject: \{fileID: (-?\d+)\}", doc).group(1)
            go_of_rect[fid] = go
            rect_of_go[go] = fid
            father = re.search(r"m_Father: \{fileID: (-?\d+)\}", doc)
            parent[fid] = father.group(1) if father else "0"
            block = doc.split("m_Children:")[1].split("m_Father")[0] if "m_Children:" in doc else ""
            children[fid] = re.findall(r"\{fileID: (-?\d+)\}", block)
        if cls == "1001":
            m = re.search(r"propertyPath: m_Name\s+value: (.*)", doc)
            instances[fid] = m.group(1) if m else "?"

    def geometry(rect):
        doc = docs[rect][1]
        out = []
        for key in ("m_AnchorMin", "m_AnchorMax", "m_AnchoredPosition", "m_SizeDelta", "m_Pivot"):
            m = re.search(rf"{key}: \{{x: ([-\d.e]+), y: ([-\d.e]+)\}}", doc)
            if m:
                out.append(f"{key[2:]}=({float(m.group(1)):g},{float(m.group(2)):g})")
        return " ".join(out)

    def components(go):
        doc = docs[go][1]
        lines = []
        for comp in re.findall(r"component: \{fileID: (-?\d+)\}", doc):
            cls, cdoc, _ = docs.get(comp, ("?", "", False))
            if cls in ("224", "222"):
                continue
            script = re.search(r"m_Script: \{fileID: \d+, guid: ([0-9a-f]+)", cdoc)
            values = [l.strip() for l in cdoc.split("\n") if l.strip().split(":")[0] in KEYS]
            lines.append(f"{cls} {script.group(1) if script else ''} | {'; '.join(values)[:400]}")
        return lines

    def walk(rect, depth):
        go = go_of_rect[rect]
        print(f"{'  ' * depth}{names.get(go, '?')}  {geometry(rect)}")
        if show_comps:
            for line in components(go):
                print(f"{'  ' * depth}  - {line}")
        for child in children.get(rect, []):
            if child in go_of_rect:
                walk(child, depth + 1)
            else:
                cls, doc, _ = docs.get(child, ("?", "", False))
                pi = re.search(r"m_PrefabInstance: \{fileID: (-?\d+)\}", doc)
                if pi:
                    print(f"{'  ' * (depth + 1)}[Prefab] {instances.get(pi.group(1), '?')}")

    for rect, father in parent.items():
        if father == "0":
            walk(rect, 0)


if __name__ == "__main__":
    main()
