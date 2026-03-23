import json

path = r'f:\GameProject\Realm of Hero\My project\Assets\Resources\Data\Attribute.json'

with open(path, 'r', encoding='utf-8') as f:
    data = json.load(f)

for attr in data['Attributes']:
    mapping = ""
    id = attr['Id']
    
    mapping_map = {
        1: "HP", 2: "HPPercent", 3: "MaxHP", 4: "MaxHPPercent",
        5: "MP", 6: "MPPercent", 7: "MaxMP", 8: "MaxMPPercent",
        9: "STR", 10: "STRPercent", 11: "AGI", 12: "AGIPercent",
        13: "VIT", 14: "VITPercent", 15: "INT", 16: "INTPercent",
        17: "DEX", 18: "DEXPercent", 19: "LUK", 20: "LUKPercent",
        21: "ATK", 22: "ATKPercent", 23: "MATK", 24: "MATKPercent",
        25: "StatusDMG", 26: "StatusDMGPercent", 
        27: "WeaponATK", 28: "WeaponATKPercent", 
        29: "WeaponMATK", 30: "WeaponMATKPercent",
        31: "Defense", 32: "DefensePercent", 
        33: "StatusDEF", 34: "StatusDEFPercent",
        35: "MDEF", 36: "MDEFPercent",
        37: "Hit", 38: "HitPercent",
        39: "Flee", 40: "FleePercent",
        41: "ASPD", 42: "ASPDPercent",
        43: "CriticalRate", 44: "CriticalRatePercent",
        45: "PerfectDodge", 46: "PerfectDodgePercent",
        47: "CriticalDamage", 48: "CriticalDamagePercent",
        49: "CritResistance", 50: "CritResistancePercent"
    }

    if id in mapping_map:
        mapping = mapping_map[id]
    elif 51 <= id <= 78:
        names = ["Neutral", "Water", "Earth", "Fire", "Electric", "Ghost", "Holy"]
        idx = (id - 51) // 4
        type_str = "Damage" if (id - 51) % 4 < 2 else "Resist"
        pct = "Percent" if id % 2 == 0 else ""
        mapping = f"{names[idx]}{type_str}{pct}"
    elif 79 <= id <= 90:
        sizes = ["Small", "Medium", "Large"]
        idx = (id - 79) // 4
        type_str = "Damage" if (id - 79) % 4 < 2 else "Resist"
        pct = "Percent" if id % 2 == 0 else ""
        mapping = f"{sizes[idx]}{type_str}{pct}"
    elif 91 <= id <= 130:
        races = ["Formless", "Undead", "Brute", "Plant", "Insect", "Fish", "Demon", "DemiHuman", "Angel", "Dragon"]
        idx = (id - 91) // 4
        type_str = "Damage" if (id - 91) % 4 < 2 else "Resist"
        pct = "Percent" if id % 2 == 0 else ""
        mapping = f"{races[idx]}{type_str}{pct}"
    elif 131 <= id <= 140:
        misc = ["MoveSpeed", "MoveSpeedPercent", "WeightLimit", "WeightLimitPercent", "HPRecovery", "HPRecoveryPercent", "MPRecovery", "MPRecoveryPercent", "HealingItems", "HealingItemsPercent"]
        mapping = misc[id - 131]
    elif 143 == id: mapping = "CastTime"
    elif 144 == id: mapping = "CastTimePercent"
    elif 145 == id: mapping = "GearVCT"
    elif 146 == id: mapping = "GearVCTPercent"
    elif 147 == id: mapping = "SkillVCT"
    elif 148 == id: mapping = "SkillVCTPercent"
    elif 149 == id: mapping = "VCTReduc"
    elif 150 == id: mapping = "VCTReducPercent"
    elif 151 == id: mapping = "MaxFCTReduc"
    elif 152 == id: mapping = "MaxFCTReducPercent"
    elif 153 == id: mapping = "TrueDamage"
    elif 154 == id: mapping = "AOE"
    
    attr['Mapping'] = mapping

with open(path, 'w', encoding='utf-8') as f:
    json.dump(data, f, indent=4)
