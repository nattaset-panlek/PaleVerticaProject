const fs = require('fs');
const ref = JSON.parse(fs.readFileSync('C:/Users/User/.gemini/antigravity/brain/29ea3001-8df0-4155-a601-718c8cb28072/refinement_node.json', 'utf8'));

function formatColor(f) {
    if (!f) return 'none';
    if (typeof f.color === 'string') return f.color;
    if (f.color) {
        return `rgb(${Math.round(f.color.r*255)},${Math.round(f.color.g*255)},${Math.round(f.color.b*255)})`;
    }
    return f.type || 'unknown';
}

function dumpAll(n, depth=0) {
    const pad = '  '.repeat(depth);
    const b = n.absoluteBoundingBox;
    const box = b ? `(${Math.round(b.x)},${Math.round(b.y)},${Math.round(b.width)},${Math.round(b.height)})` : '';
    let fill = 'none';
    if (n.fills && n.fills[0]) fill = formatColor(n.fills[0]);
    let txt = n.characters ? ` "${n.characters}"` : '';
    let font = n.style ? ` ${n.style.fontFamily} ${n.style.fontWeight} ${n.style.fontSize}px` : '';
    console.log(`${pad}${n.name} [${n.type}] box:${box} fill:${fill}${txt}${font}`);
    if (n.children) n.children.forEach(c => dumpAll(c, depth+1));
}

dumpAll(ref);
