const fs = require('fs');
const filePath = 'f:/GameProject/Realm of Hero/My project/Assets/Resources/Data/Item.json';

const rawData = fs.readFileSync(filePath, 'utf8');
const data = JSON.parse(rawData);

const prefix = "Assets/Addressable/Sprite/Item/";
let updateCount = 0;

data.Items.forEach(item => {
    if (item.IconPath && item.IconPath !== "") {
        let current = item.IconPath;
        
        // 1. Ensure .png extension
        if (!current.toLowerCase().endsWith(".png")) {
            current = current.split(':')[0] + ".png";
        }
        
        // 2. Add full path prefix if not already present
        if (!current.startsWith(prefix)) {
            // If it has a partial prefix (like "Item/"), remove it first
            const fileName = current.split('/').pop();
            current = prefix + fileName;
        }
        
        if (item.IconPath !== current) {
            item.IconPath = current;
            updateCount++;
        }
    }
});

fs.writeFileSync(filePath, JSON.stringify(data, null, 4), 'utf8');
process.stdout.write(`Success: Updated ${updateCount} IconPaths with full path prefix.\n`);
