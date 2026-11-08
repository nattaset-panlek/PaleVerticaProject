import os
import re

# Read current prefab (dev version)
dev_prefab_path = 'Assets/Prefab/Game Page/Refinement Window.prefab'
with open(dev_prefab_path, 'r', encoding='utf-8', errors='ignore') as f:
    dev_content = f.read()

# Let's inspect all GameObjects and RectTransforms in dev_content
def parse_unity_yaml(content):
    objects = {}
    current_id = None
    current_type = None
    current_lines = []

    for line in content.splitlines():
        m = re.match(r'^--- !u!(\d+) &(\d+)', line)
        if m:
            if current_id:
                objects[current_id] = {'type': current_type, 'lines': current_lines}
            current_type = m.group(1)
            current_id = m.group(2)
            current_lines = []
        else:
            current_lines.append(line)
    if current_id:
        objects[current_id] = {'type': current_type, 'lines': current_lines}
    return objects

dev_objs = parse_unity_yaml(dev_content)
print(f'Total objects in Dev Prefab: {len(dev_objs)}')

# Find all GameObjects and their RectTransforms
go_map = {}
for obj_id, obj in dev_objs.items():
    if obj['type'] == '1': # GameObject
        name = ''
        components = []
        for l in obj['lines']:
            if 'm_Name:' in l:
                name = l.split('m_Name:')[1].strip()
            if 'component:' in l:
                cid = re.search(r'fileID:\s*(\d+)', l)
                if cid:
                    components.append(cid.group(1))
        go_map[name] = {'go_id': obj_id, 'comps': components}

print('\nDev Prefab GameObjects and Components:')
for name, data in go_map.items():
    types = []
    rt_info = ''
    tmp_info = ''
    img_info = ''
    for cid in data['comps']:
        if cid in dev_objs:
            ctype = dev_objs[cid]['type']
            types.append(ctype)
            if ctype == '224': # RectTransform
                for l in dev_objs[cid]['lines']:
                    if 'm_AnchoredPosition:' in l or 'm_SizeDelta:' in l or 'm_AnchorMin:' in l or 'm_AnchorMax:' in l:
                        rt_info += ' ' + l.strip()
            elif ctype == '114': # MonoBehaviour
                cl = dev_objs[cid]['lines']
                for l in cl:
                    if 'm_text:' in l:
                        tmp_info += ' ' + l.strip()
                    if 'm_fontSize:' in l:
                        tmp_info += ' ' + l.strip()
                    if 'm_fontAsset:' in l:
                        tmp_info += ' ' + l.strip()
                    if 'm_Sprite:' in l:
                        img_info += ' ' + l.strip()
                    if 'm_Color:' in l:
                        img_info += ' ' + l.strip()
                    if 'UnityEngine.UI.Slider' in l:
                        types.append('Slider')
    print(f'- {name:20} [types: {",".join(types)}] | RT:{rt_info} | {tmp_info} | {img_info}')
