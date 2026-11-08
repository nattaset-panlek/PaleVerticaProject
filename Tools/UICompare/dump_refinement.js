const fs = require('fs');
const ref = JSON.parse(fs.readFileSync('C:/Users/User/.gemini/antigravity/brain/29ea3001-8df0-4155-a601-718c8cb28072/refinement_node.json', 'utf8'));

function dump(n, indent='') {
    let info = indent + n.name + ' [' + n.type + '] (id:' + n.id + ')';
    if (n.absoluteBoundingBox) {
        const b = n.absoluteBoundingBox;
        info += ' box:(' + Math.round(b.x) + ',' + Math.round(b.y) + ',' + Math.round(b.width) + ',' + Math.round(b.height) + ')';
    }
    if (n.cornerRadius) info += ' r:' + n.cornerRadius;
    if (n.characters) info += ' text:' + JSON.stringify(n.characters) + ' font:' + (n.style ? n.style.fontFamily + ' ' + n.style.fontWeight : '');
    console.log(info);
    if (n.children) {
        for (const c of n.children) dump(c, indent + '  ');
    }
}
dump(ref);
