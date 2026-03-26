import json
import os

path = r'F:\GameProject\Realm of Hero\My project\Assets\Resources\Data\Item.json'

with open(path, 'r', encoding='utf-8') as f:
    data = json.load(f)

for item in data['Items']:
    # Target all items, but user specifically mentioned 'Equipment' should be clean 'all times'
    if 'Effects' in item and (item['Effects'] is None or len(item['Effects']) == 0):
        del item['Effects']

with open(path, 'w', encoding='utf-8') as f:
    json.dump(data, f, indent=4, ensure_ascii=False)
