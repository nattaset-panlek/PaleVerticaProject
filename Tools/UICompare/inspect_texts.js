const fs = require('fs');
const ref = JSON.parse(fs.readFileSync('C:/Users/User/.gemini/antigravity/brain/29ea3001-8df0-4155-a601-718c8cb28072/refinement_node.json', 'utf8'));

function formatColor(f) {
    if (!f) return 'none';
    if (f.color) {
        const r = Math.round(f.color.r * 255);
        const g = Math.round(f.color.g * 255);
        const b = Math.round(f.color.b * 255);
        const a = f.opacity !== undefined ? f.opacity : (f.color.a !== undefined ? f.color.a : 1);
        return `rgba(${r},${g},${b},${a})`;
    }
    return f.type || 'unknown';
}

function search(n) {
    if (n.characters !== undefined) {
        const fill = (n.fills && n.fills[0]) ? formatColor(n.fills[0]) : 'none';
        const st = n.style || {};
        console.log(`TEXT: "${n.characters}" | font: ${st.fontFamily} ${st.fontWeight} ${st.fontSize}px | fill: ${fill} | name: ${n.name}`);
    }
    if (n.children) n.children.forEach(search);
}

console.log('=== ALL TEXT NODES IN REFINEMENT ===');
search(ref);
