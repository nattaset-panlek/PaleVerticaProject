const fs = require('fs');

const path = 'C:/Users/User/.gemini/antigravity/brain/29ea3001-8df0-4155-a601-718c8cb28072/refinement_node.json';
const data = JSON.parse(fs.readFileSync(path, 'utf8'));

function dump(node, depth = 0) {
    const indent = '  '.repeat(depth);
    const box = node.absoluteBoundingBox ? `[${Math.round(node.absoluteBoundingBox.x)}, ${Math.round(node.absoluteBoundingBox.y)}, ${Math.round(node.absoluteBoundingBox.width)}x${Math.round(node.absoluteBoundingBox.height)}]` : '';
    let details = '';
    if (node.type === 'TEXT') {
        const style = node.style || {};
        details = ` "${node.characters}" (font: ${style.fontFamily} ${style.fontWeight} ${style.fontSize}pt, fill: ${JSON.stringify(node.fills?.[0]?.color)})`;
    }
    console.log(`${indent}${node.type} "${node.name}" ${box}${details}`);
    if (node.children) {
        for (const child of node.children) dump(child, depth + 1);
    }
}

dump(data);
