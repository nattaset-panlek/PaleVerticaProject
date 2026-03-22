import json
import os
import random
import shutil

# File paths
JSON_OUTPUT_PATH = "Assets/Resources/Data/Skill.json"
ASSET_OUTPUT_DIR = "Assets/Resources/Skill/ActiveSkill"

os.makedirs("Assets/Resources/Data", exist_ok=True)
os.makedirs(ASSET_OUTPUT_DIR, exist_ok=True)

# Generate Random RID for Unity YAML
def generate_rid():
    return str(random.randint(1000000000000000000, 9223372036854775807))

skills = [
    {
        "ID": 101,
        "Name": "Hellfire Blast",
        "Element": 3,
        "ElementStr": "Fire",
        "BaseDamage": 180.0,
        "BaseAOERadius": 3.0,
        "BaseProjectileCount": 1,
        "BaseManaCost": 10.0,
        "BaseVCT": 1.5,
        "BaseFCT": 0.5,
        "BaseCastDelay": 0.2,
        "BaseCooldown": 5.0,
        "Heal": 0.0,
        "VisualCategory": 1,
        "Scalings": [
            {"TargetField": 0, "ScalingAttribute": "INT", "AttributeMultiplier": 2.0, "GlobalMultiplier": 1.0, "FlatBonus": 0.0, "LevelMultiplier": 50.0}
        ],
        "SO_TargetingClass": "SphereAOETargeting",
        "SO_TargetingData": "AlignmentAction: 1\n        IncludeSelf: 0",
        "SO_Effects": [
            {"class": "DamageEffect", "data": "TargetAttribute: 0"}
        ]
    },
    {
        "ID": 102,
        "Name": "Divine Blessing",
        "Element": 6,
        "ElementStr": "Holy",
        "BaseDamage": 0.0,
        "BaseAOERadius": 5.0,
        "BaseProjectileCount": 0,
        "BaseManaCost": 20.0,
        "BaseVCT": 1.0,
        "BaseFCT": 1.0,
        "BaseCastDelay": 0.5,
        "BaseCooldown": 8.0,
        "Heal": 200.0,
        "VisualCategory": 0,
        "Scalings": [
            {"TargetField": 3, "ScalingAttribute": "INT", "AttributeMultiplier": 1.5, "GlobalMultiplier": 1.0, "FlatBonus": 50.0, "LevelMultiplier": 80.0}
        ],
        "SO_TargetingClass": "SphereAOETargeting",
        "SO_TargetingData": "AlignmentAction: 0\n        IncludeSelf: 1",
        "SO_Effects": [
            {"class": "HealEffect", "data": "TargetAttribute: 0\n        BaseHealAmount: 0"}
        ]
    },
    {
        "ID": 103,
        "Name": "Earthshatter",
        "Element": 2, # Earth
        "ElementStr": "Earth",
        "BaseDamage": 300.0,
        "BaseAOERadius": 0.0,
        "BaseProjectileCount": 0,
        "BaseManaCost": 25.0,
        "BaseVCT": 2.5,
        "BaseFCT": 0.5,
        "BaseCastDelay": 1.0,
        "BaseCooldown": 12.0,
        "Heal": 0.0,
        "VisualCategory": 1,
        "Scalings": [
            {"TargetField": 0, "ScalingAttribute": "VIT", "AttributeMultiplier": 3.0, "GlobalMultiplier": 1.0, "FlatBonus": 100.0, "LevelMultiplier": 100.0}
        ],
        "SO_TargetingClass": "ConeTargeting",
        "SO_TargetingData": "ConeAngle: 90",
        "SO_Effects": [
            {"class": "DamageEffect", "data": "TargetAttribute: 0"}
        ]
    },
    {
        "ID": 104,
        "Name": "Lightning Strike",
        "Element": 4, # Electric
        "ElementStr": "Electric",
        "BaseDamage": 220.0,
        "BaseAOERadius": 0.0,
        "BaseProjectileCount": 1,
        "BaseManaCost": 15.0,
        "BaseVCT": 0.2,
        "BaseFCT": 0.0,
        "BaseCastDelay": 0.1,
        "BaseCooldown": 2.5,
        "Heal": 0.0,
        "VisualCategory": 3,
        "Scalings": [
            {"TargetField": 0, "ScalingAttribute": "DEX", "AttributeMultiplier": 1.8, "GlobalMultiplier": 1.0, "FlatBonus": 20.0, "LevelMultiplier": 60.0}
        ],
        "SO_TargetingClass": "LinePierceTargeting",
        "SO_TargetingData": "LineWidth: 1",
        "SO_Effects": [
            {"class": "DamageEffect", "data": "TargetAttribute: 0"}
        ]
    },
    {
        "ID": 105,
        "Name": "Shadow Dash",
        "Element": 5, # Ghost
        "ElementStr": "Ghost",
        "BaseDamage": 0.0,
        "BaseAOERadius": 0.0,
        "BaseProjectileCount": 0,
        "BaseManaCost": 8.0,
        "BaseVCT": 0.1,
        "BaseFCT": 0.0,
        "BaseCastDelay": 0.1,
        "BaseCooldown": 6.0,
        "IsToggle": False,
        "Heal": 0.0,
        "VisualCategory": 0,
        "Scalings": [],
        "SO_TargetingClass": "SelfTargeting",
        "SO_TargetingData": "",
        "SO_Effects": [
            {"class": "ApplyTemporaryBuffEffect", "data": "TargetAttribute: 3\n        ModifierValue: 30\n        ModType: 0\n        Duration: 5"}
        ]
    },
    {
        "ID": 106,
        "Name": "Aura of Flames",
        "Element": 3, # Fire
        "ElementStr": "Fire",
        "BaseDamage": 50.0,
        "BaseAOERadius": 6.0,
        "BaseProjectileCount": 0,
        "BaseManaCost": 5.0,
        "BaseVCT": 0.0,
        "BaseFCT": 0.0,
        "BaseCastDelay": 0.0, # Toggles usually don't have cast delay
        "BaseCooldown": 1.0, # Ticks every 1 second
        "IsToggle": True,
        "Heal": 0.0,
        "VisualCategory": 1,
        "Scalings": [
            {"TargetField": 0, "ScalingAttribute": "INT", "AttributeMultiplier": 0.5, "GlobalMultiplier": 1.0, "FlatBonus": 10.0, "LevelMultiplier": 20.0}
        ],
        "SO_TargetingClass": "SphereAOETargeting",
        "SO_TargetingData": "AlignmentAction: 1\n        IncludeSelf: 0",
        "SO_Effects": [
            {"class": "DamageEffect", "data": "TargetAttribute: 0"}
        ]
    }
]

skill_data_list = {
    "ActiveSkills": [],
    "SupportSkills": [] 
}

for s in skills:
    active_skill_json = {
        "SkillID": s["ID"],
        "SkillName": s["Name"],
        "SkillElement": s["ElementStr"],
        "BaseDamage": s["BaseDamage"],
        "BaseAOERadius": s["BaseAOERadius"],
        "BaseProjectileCount": s["BaseProjectileCount"],
        "BaseManaCost": s["BaseManaCost"],
        "BaseVCT": s["BaseVCT"],
        "BaseFCT": s["BaseFCT"],
        "BaseCastDelay": s["BaseCastDelay"],
        "BaseCooldown": s["BaseCooldown"],
        "IsToggle": s.get("IsToggle", False),
        "Heal": s["Heal"],
        "BaseTargeting": {
            "Type": s["SO_TargetingClass"].replace("Targeting", ""),
            "Radius": s["BaseAOERadius"]
        },
        "Scalings": s["Scalings"]
    }
    skill_data_list["ActiveSkills"].append(active_skill_json)

# Save JSON
with open(JSON_OUTPUT_PATH, 'w', encoding='utf-8') as f:
    json.dump(skill_data_list, f, indent=4)
print(f"Created JSON at {JSON_OUTPUT_PATH}")

ACTIVESKILL_GUID = "8dfaa4b0ae584104b8074ca83d465ece"

YAML_TEMPLATE = """%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 0}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {guid}, type: 3}}
  m_Name: {asset_name}
  m_EditorClassIdentifier: 
  Id: {id}
  Name: {name}
  SkillElement: {element}
  BaseTargeting:
    rid: {targeting_rid}
  Effects:
{effects_list}  VisualData:
    VisualCategory: {visual_category}
  BaseDamage: 0
  BaseAOERadius: 0
  BaseProjectileCount: 0
  BaseManaCost: 0
  BaseVCT: 0
  BaseFCT: 0
  BaseCastDelay: 0
  BaseCooldown: 0
  Scalings: []
  references:
    version: 2
    RefIds:
    - rid: {targeting_rid}
      type: {{{{class: {targeting_class}, ns: CoreSystem.Skills.Logic, asm: Assembly-CSharp}}}}
      data:
        {targeting_data}
{effects_ref_list}"""

for s in skills:
    targeting_rid = generate_rid()
    
    effects_list = ""
    effects_ref_list = ""
    
    for effect in s["SO_Effects"]:
        eff_rid = generate_rid()
        effects_list += f"  - rid: {eff_rid}\n"
        
        eff_ref = f"""    - rid: {eff_rid}
      type: {{{{class: {effect['class']}, ns: CoreSystem.Skills.Logic, asm: Assembly-CSharp}}}}
      data:
        {effect['data']}
"""
        effects_ref_list += eff_ref

    yaml_content = YAML_TEMPLATE.format(
        guid=ACTIVESKILL_GUID,
        asset_name=s["Name"].replace(" ", ""),
        id=s["ID"],
        name=s["Name"],
        element=s["Element"],
        visual_category=s["VisualCategory"],
        targeting_rid=targeting_rid,
        targeting_class=s["SO_TargetingClass"],
        targeting_data=s["SO_TargetingData"] if s["SO_TargetingData"] else "{}",
        effects_list=effects_list,
        effects_ref_list=effects_ref_list
    )
    
    # Fix empty targeting formatting issue
    if s["SO_TargetingData"] == "":
        yaml_content = yaml_content.replace("        {}", "{}", 1)
        
    asset_path = os.path.join(ASSET_OUTPUT_DIR, f"{s['Name'].replace(' ', '')}.asset")
    with open(asset_path, 'w', encoding='utf-8') as f:
        f.write(yaml_content)
    print(f"Created SO Asset at {asset_path}")

print("\nSuccess! Process fully automated.")
