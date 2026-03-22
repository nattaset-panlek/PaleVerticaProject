import json
import os

mapping = {
    8: 40, 9: 41,     # ATK
    32: 42, 33: 43,   # MATK
    10: 20, 11: 21,   # STR
    12: 22, 13: 23,   # AGI
    14: 24, 15: 25,   # VIT
    16: 26, 17: 27,   # INT
    18: 28, 19: 29,   # DEX
    20: 30, 21: 31,   # LUK
    24: 46, 25: 47,   # WeaponATK
    26: 48, 27: 49,   # WeaponMATK
    36: 50, 37: 51,   # DEF
    38: 54, 39: 55,   # MDEF
    40: 56, 41: 57,   # Hit
    42: 58, 43: 59,   # Flee
    44: 60, 45: 61,   # ASPD
    46: 62, 47: 63,   # CritRate
    48: 80, 49: 81,   # CritDmg
    50: 82, 51: 83,   # CritRes
    52: 102, 53: 103, # WaterRes
    56: 90, 57: 91,   # FireDmg
    59: 108,          # ElectricRes
    60: 110,          # GhostResist
    62: 96,           # HolyDmg
    66: 103,          # WaterResist% -> WaterResistPercent (103)
    70: 171,          # DragonResist% -> DragonResistPercent (171)
    71: 107,          # FireResist% -> FireResistPercent (107)
    131: 65,          # PDodge% (PerfectDodgePercent 65)
    135: 159,         # PlantResist% (Nature Essence 30% -> PlantResistPercent 159)
    138: 149,         # AngelDmg% (AngelDamagePercent 149)
}

file_path = r'f:\GameProject\Realm of Hero\My project\Assets\Resources\Data\Item.json'

with open(file_path, 'r', encoding='utf-8') as f:
    data = json.load(f)

count = 0
for item in data.get('Items', []):
    if 'Effects' in item:
        for effect in item['Effects']:
            if 'AttributeID' in effect:
                old_id = effect['AttributeID']
                if old_id in mapping:
                    effect['AttributeID'] = mapping[old_id]
                    count += 1

print(f"Updated {count} attribute IDs in Item.json")

with open(file_path, 'w', encoding='utf-8') as f:
    json.dump(data, f, indent=2, ensure_ascii=False)
