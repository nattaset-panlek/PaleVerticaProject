const fs = require('fs');

const path = 'C:/Users/User/.gemini/antigravity/brain/29ea3001-8df0-4155-a601-718c8cb28072/refinement_node.json';
const data = JSON.parse(fs.readFileSync(path, 'utf8'));

function findNodes(node, queries) {
    const name = (node.name || '').toLowerCase();
    const chars = (node.characters || '').toLowerCase();
    
    for (const q of queries) {
        if (name.includes(q) || chars.includes(q)) {
            console.log(`\n=== MATCH: [${q}] ===`);
            console.log(`Name: "${node.name}", Type: ${node.type}`);
            console.log(`Box:`, node.absoluteBoundingBox);
            if (node.characters) {
                console.log(`Text: "${node.characters}"`);
                console.log(`Style:`, JSON.stringify(node.style, null, 2));
            }
            if (node.fills) {
                console.log(`Fills:`, JSON.stringify(node.fills, null, 2));
            }
            if (node.strokes) {
                console.log(`Strokes:`, JSON.stringify(node.strokes, null, 2));
            }
            break;
        }
    }

    if (node.children) {
        for (const child of node.children) {
            findNodes(child, queries);
        }
    }
}

findNodes(data, ['forge', 'dragon', 'lv.', 'common', 'refinement', 'health', '65%', 'credits', '5,000']);
