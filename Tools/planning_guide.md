# Realm of Hero: Project Planning Guide

This guide outlines the standards and best practices for managing data and systems in the "Realm of Hero" project to ensure high **Mobility** (ease of refactoring/scaling) and effective **Planning**.

## 1. Data Mobility Standards

### ID Range Allocation
To avoid collisions and maintain organization, use the following ID ranges for new entries in JSON files:

| Category | ID Range | JSON File |
| :--- | :--- | :--- |
| **Core Attributes** | 0 - 199 | `Attribute.json` |
| **Currencies** | 1000 - 1999 | `Item.json` |
| **Consumables** | 2000 - 2999 | `Item.json` |
| **Equipment** | 3000 - 5999 | `Item.json` |
| **Skill Runes** | 6000 - 6999 | `Item.json` |
| **Materials/Resources** | 7000 - 8999 | `Item.json` |
| **Active Skills** | 100 - 999 | `Skill.json` |
| **Support Skills** | 1000 - 1999 | `SupportSkill.json` |

### JSON Reference Safety
- **Always** ensure that `AttributeID` in `Item.json` or `Skill.json` exists in `Attribute.json`.
- Use the **Mobility Assistant** tool in Unity (`Window > Realm of Hero > Mobility Assistant`) to validate references before committing changes.

## 2. Planning for Balance

### Attribute Scaling
When designing new items or skills, refer to the **Project Status Snapshot** to see existing power levels. 
- **Physical Scaling**: Primarily STR and DEX.
- **Magic Scaling**: Primarily INT and DEX.
- **Defensive Scaling**: Primarily VIT and AGI.

### Effect Complexity
Prefer simple `StatBonus` effects for base items. Reserve `ScalingBonusEffect` and `AutoCastEffect` for Legendary or Unique equipment to maintain game balance and performance.

## 3. Mobility Assistant Tool
The `ProjectMobilityAssistant` script in `Assets/Script/Editor` is your primary tool for:
1. **Validating Data**: Checking for broken links between JSON files.
2. **Batch Updating**: Safely renaming or shifting IDs across all data files.
3. **Snapshot Generation**: Updating the documentation with current game values.
