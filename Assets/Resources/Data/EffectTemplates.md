# Item Effect Templates

This document specifies the JSON format for various item effects supported by the `CoreSystem`.

## 1. AttributeBonusEffect
Adds a flat bonus to a specific attribute.
```json
{
    "EffectType": "AttributeBonusEffect",
    "AttributeID": 1, 
    "Value": 10.0
}
```

## 2. ScalingBonusEffect
Adds a bonus to one attribute based on the sum of other "source" attributes.
```json
{
    "EffectType": "ScalingBonusEffect",
    "AttributeID": 5,
    "ScalingThreshold": 10.0,
    "Value": 1.0,
    "ScalingSourceIDs": [1, 2]
}
```
*Complexity: Adds `Value` to `AttributeID` for every `ScalingThreshold` units of total sum of `ScalingSourceIDs`.*

## 3. SkillUnlockEffect
Unlocks a specific skill for the player when equipped.
```json
{
    "EffectType": "SkillUnlockEffect",
    "SkillID": 101
}
```

## 4. AutoCastEffect
Gives a chance to trigger a skill automatically.
```json
{
    "EffectType": "AutoCastEffect",
    "SkillID": 202,
    "TriggerType": "OnAttack",
    "TriggerChance": 5.5
}
```

## 5. BonusCondition
Grants an attribute bonus only if another attribute meets a certain threshold.
```json
{
    "EffectType": "BonusCondition",
    "ConditionAttributeID": 1,
    "ConditionComparison": ">=",
    "ConditionThreshold": 50.0,
    "AttributeID": 5,
    "Value": 20.0
}
```
*Example: If STR (1) is >= 50, grant +20 Physical ATK (5).*

## JSON Data Structure (Item.json)
```json
{
    "ItemId": 3001,
    "Name": "Example Sword",
    "ItemType": "Equipment",
    "Effects": [
        { ... Effect Body ... }
    ]
}
```
> [!NOTE]
> Items with no effects should have the `"Effects"` field completely removed or set to `null` to minimize data size.
