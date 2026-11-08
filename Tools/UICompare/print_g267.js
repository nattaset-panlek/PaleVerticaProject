const fs = require('fs');
const d = JSON.parse(fs.readFileSync('C:/Users/User/.gemini/antigravity/brain/29ea3001-8df0-4155-a601-718c8cb28072/socket_node.json', 'utf8'));

function findNode(n, name) {
    if (n.name === name) return n;
    if (n.children) {
        for (const c of n.children) {
            const r = findNode(c, name);
            if (r) return r;
        }
    }
    return null;
}

const g = findNode(d, 'Group 267');
const mb = { x: 394, y: 189 };
if (g && g.children) {
    g.children.forEach(c => {
        const b = c.absoluteBoundingBox;
        console.log(c.type, `"${c.name}"`, `rel=(${Math.round(b.x - mb.x)}, ${Math.round(b.y - mb.y)}, ${Math.round(b.width)}x${Math.round(b.height)})`);
    });
}
