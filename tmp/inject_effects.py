import json
import os

def get_dynamic_effects(item):
    name = item.get("Name", "").lower()
    desc_en = item.get("Description", {}).get("EN", "").lower()
    rarity = item.get("Rarity", "")
    item_type = item.get("ItemType", "")
    
    if item_type not in ["Equipment", "Fragment"]:
        return None

    if rarity == "Common":
        return None

    effects = []

    if "fire" in name or "flame" in desc_en or "burn" in desc_en:
        effects.append({"EffectType": "AttributeBonusEffect", "AttributeID": 63, "Value": 15.0})
        effects.append({"EffectType": "SkillUnlockEffect", "SkillID": 101})
    elif "ice" in name or "freez" in desc_en or "cold" in desc_en:
        effects.append({"EffectType": "AttributeBonusEffect", "AttributeID": 57, "Value": 25.0})
        effects.append({"EffectType": "SkillUnlockEffect", "SkillID": 102})
    elif "time" in name or "chrono" in desc_en:
        effects.append({"EffectType": "AttributeBonusEffect", "AttributeID": 151, "Value": 10.0})
        effects.append({"EffectType": "AttributeBonusEffect", "AttributeID": 147, "Value": 1.5})
    elif "demon" in name or "corrupt" in desc_en:
        effects.append({"EffectType": "AttributeBonusEffect", "AttributeID": 115, "Value": 20.0})
        effects.append({"EffectType": "AutoCastEffect", "SkillID": 108, "TriggerType": "OnAttack", "TriggerChance": 8.0})
    elif "swift" in name or "speed" in desc_en or "lightweight" in desc_en:
        effects.append({"EffectType": "AttributeBonusEffect", "AttributeID": 131, "Value": 15.0})
        effects.append({"EffectType": "AttributeBonusEffect", "AttributeID": 41, "Value": 10.0})
    elif "dragon" in name:
        effects.append({"EffectType": "AttributeBonusEffect", "AttributeID": 127, "Value": 25.0})
        effects.append({"EffectType": "BonusCondition", "ConditionAttributeID": 3, "ConditionComparison": ">=", "ConditionThreshold": 5000.0, "AttributeID": 21, "Value": 50.0})
    elif "void" in name or "dark" in desc_en:
        effects.append({"EffectType": "AttributeBonusEffect", "AttributeID": 71, "Value": 20.0})
        effects.append({"EffectType": "AutoCastEffect", "SkillID": 110, "TriggerType": "OnHit", "TriggerChance": 5.0})

    if rarity in ["Epic", "Legendary"] and len(effects) > 0:
        if not any(e["EffectType"] == "BonusCondition" for e in effects):
            effects.append({
                "EffectType": "BonusCondition",
                "ConditionAttributeID": 2, 
                "ConditionComparison": "<=",
                "ConditionThreshold": 30.0,
                "AttributeID": 31,
                "Value": 100.0
            })
        if rarity == "Legendary" and not any(e["EffectType"] == "AutoCastEffect" for e in effects):
            effects.append({
                "EffectType": "AutoCastEffect",
                "SkillID": 202,
                "TriggerType": "OnTakeDamage",
                "TriggerChance": 10.0
            })

    if len(effects) == 1:
        effects.append({"EffectType": "AttributeBonusEffect", "AttributeID": 3, "Value": 100.0})

    return effects if len(effects) >= 2 else None

def inject_effects():
    file_path = r"F:\GameProject\Realm of Hero\My project\Assets\Resources\Data\Item.json"
    
    if not os.path.exists(file_path):
        print(f"File not found: {file_path}")
        return

    with open(file_path, 'r', encoding='utf-8') as f:
        data = json.load(f)

    specific_effects = {
        3032: [
            {"EffectType": "AttributeBonusEffect", "AttributeID": 15, "Value": 10.0},
            {"EffectType": "ScalingBonusEffect", "AttributeID": 137, "ScalingThreshold": 10.0, "Value": 50.0, "ScalingSourceIDs": [15]}
        ],
        3033: [
            {"EffectType": "AttributeBonusEffect", "AttributeID": 13, "Value": 5.0},
            {"EffectType": "ScalingBonusEffect", "AttributeID": 3, "ScalingThreshold": 2.0, "Value": 1.0, "ScalingSourceIDs": [7]}
        ],
        3034: [
            {"EffectType": "AttributeBonusEffect", "AttributeID": 151, "Value": 5.0},
            {"EffectType": "ScalingBonusEffect", "AttributeID": 148, "ScalingThreshold": 50.0, "Value": 10.0, "ScalingSourceIDs": [15, 17]},
            {"EffectType": "SkillUnlockEffect", "SkillID": 207}
        ],
        3035: [
            {"EffectType": "ScalingBonusEffect", "AttributeID": 21, "ScalingThreshold": 50.0, "Value": 5.0, "ScalingSourceIDs": [210]},
            {"EffectType": "ScalingBonusEffect", "AttributeID": 23, "ScalingThreshold": 50.0, "Value": 5.0, "ScalingSourceIDs": [210]},
            {"EffectType": "BonusCondition", "ConditionAttributeID": 210, "ConditionComparison": ">=", "ConditionThreshold": 100.0, "AttributeID": 3, "Value": 1000.0}
        ]
    }

    modified_count = 0

    for item in data.get("Items", []):
        item_id = item.get("ItemId")
        
        effects_to_apply = None
        
        if item_id in specific_effects:
            effects_to_apply = specific_effects[item_id]
        else:
            effects_to_apply = get_dynamic_effects(item)

        if effects_to_apply:
            item["Effects"] = effects_to_apply
            modified_count += 1
        else:
            if "Effects" in item:
                del item["Effects"]

    with open(file_path, 'w', encoding='utf-8') as f:
        json.dump(data, f, ensure_ascii=False, indent=4)

    print(f"Successfully injected effects to {modified_count} items.")

if __name__ == "__main__":
    inject_effects()
