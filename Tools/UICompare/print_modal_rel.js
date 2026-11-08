const fs = require('fs');

const path = 'C:/Users/User/.gemini/antigravity/brain/29ea3001-8df0-4155-a601-718c8cb28072/refinement_node.json';
const data = JSON.parse(fs.readFileSync(path, 'utf8'));

// Find Group 253
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

const g253 = findNode(data, 'Group 253');
const modalBox = g253.absoluteBoundingBox;
console.log('Modal Box:', modalBox);

function printRel(node, depth = 0) {
    const indent = '  '.repeat(depth);
    if (!node.absoluteBoundingBox) return;
    const b = node.absoluteBoundingBox;
    const rx = Math.round(b.x - modalBox.x);
    const ry = Math.round(b.y - modalBox.y);
    const rw = Math.round(b.width);
    const rh = Math.round(b.height);
    
    let extra = '';
    if (node.type === 'TEXT') {
        const s = node.style || {};
        const col = node.fills?.[0]?.color;
        const hex = col ? (typeof col === 'string' ? col : `#${Math.round((col.r||0)*255).toString(16).padStart(2,'0')}${Math.round((col.g||0)*255).toString(16).padStart(2,'0')}${Math.round((col.b||0)*255).toString(16).padStart(2,'0')}`) : 'none';
        extra = ` | TXT: "${node.characters}" | ${s.fontFamily} ${s.fontWeight} ${s.fontSize}pt | ${hex}`;
    } else if (node.type === 'RECTANGLE') {
        const r = node.cornerRadius || 0;
        extra = ` | RECT r=${r}`;
    }

    console.log(`${indent}[${node.type}] "${node.name}" rel: (${rx}, ${ry}, ${rw}x${rh})${extra}`);
    if (node.children) {
        for (const c of node.children) printRel(c, depth + 1);
    }
}

printRel(g253);
