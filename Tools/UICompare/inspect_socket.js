const fs = require('fs');
const path = 'C:/Users/User/.gemini/antigravity/brain/29ea3001-8df0-4155-a601-718c8cb28072/socket_node.json';
const data = JSON.parse(fs.readFileSync(path, 'utf8'));

function findNode(node, name) {
    if (node.name === name) return node;
    if (node.children) {
        for (const c of node.children) {
            const found = findNode(c, name);
            if (found) return found;
        }
    }
    return null;
}

const g307 = findNode(data, 'Group 307');
const mb = g307.absoluteBoundingBox;

function walk(node, depth = 0) {
    if (!node.absoluteBoundingBox) return;
    const b = node.absoluteBoundingBox;
    const rx = Math.round(b.x - mb.x);
    const ry = Math.round(b.y - mb.y);
    const rw = Math.round(b.width);
    const rh = Math.round(b.height);

    if (node.type === 'TEXT') {
        const s = node.style || {};
        console.log(`TEXT: "${node.name}" characters="${node.characters}" rel=(${rx}, ${ry}, ${rw}x${rh}) font="${s.fontFamily}" weight=${s.fontWeight} size=${s.fontSize}`);
    } else if (node.name.includes('Icon') || node.name.includes('Button') || node.name.includes('Slot') || node.name.includes('Badge') || node.name.includes('Rectangle 18') || node.name.includes('Rectangle 122') || node.name.includes('Ellipse') || node.name.includes('Group 234') || node.name.includes('Group 267')) {
        console.log(`NODE: [${node.type}] "${node.name}" rel=(${rx}, ${ry}, ${rw}x${rh}) cornerRadius=${node.cornerRadius || 0}`);
    }
    if (node.children) node.children.forEach(c => walk(c, depth + 1));
}

walk(g307);
