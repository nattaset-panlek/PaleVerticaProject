# Realm of Hero: Documentation Hub

This folder is the **single source** for every Markdown document in the project. Do not create `.md` files inside `Assets/` (Unity generates `.meta` noise and the docs get lost next to scripts). New docs go into the matching folder below.

AI agents: start with `AGENTS.md` (project root) and the project skill at `.claude/skills/realm-of-hero-dev/SKILL.md`.

## 00-Overview
High-level maps of the whole game.
- [Game System Summary (TH)](./00-Overview/GAME_SYSTEM_SUMMARY.md)
- [System Analysis](./00-Overview/System_Analysis.md)
- [System Architecture & Script Dictionary](./00-Overview/System_Architecture.md) *(2026-04, partially outdated)*

## 01-Design
Game design, ideas, and classification systems.
- [Game Design Document (TH)](./01-Design/GameDesignDoc.md)
- [Game Perspective & Map Design Analysis](./01-Design/GAME_PERSPECTIVE_AND_MAP_DESIGN_ANALYSIS.md)
- [Support Skill Chroma & Mineral System](./01-Design/SupportChromaSystem.md)
- [Unique Passive Ideas (TH)](./01-Design/UniquePassiveIdeas.md)

## 02-Technical
System-level implementation docs, grouped by system. See [02-Technical/README.md](./02-Technical/README.md).

| System | Docs |
|---|---|
| Attributes | [AdditionalValue Standard](./02-Technical/Attributes/AdditionalValue_Standard.md), [All Attribute Formulas](./02-Technical/Attributes/All_Attribute_Formulas.md), [Attribute Mechanics & Classes](./02-Technical/Attributes/Attribute_Mechanics.md) |
| Character / AI | [Command Director](./02-Technical/Character/Command_Director_Manual.md), [NPC & AgentCore](./02-Technical/Character/NPC_and_AgentCore_Walkthrough.md), [Character Visual Architecture (TH)](./02-Technical/Character/Character_Visual_Architecture_TH.md), [Extensible Visual Architecture (TH)](./02-Technical/Character/Extensible_Visual_Architecture_TH.md) |
| Skills | [Unique Skills Guide](./02-Technical/Skills/UNIQUE_SKILLS_GUIDE.md), [Skill Combo / Chain (TH)](./02-Technical/Skills/Skill_Combo_Walkthrough_TH.md), [Passive System V2 Planning](./02-Technical/Skills/Passive_System_V2_Planning.md), [Skill Visuals EN](./02-Technical/Skills/Skill_Visuals_Guide_EN.md) / [TH](./02-Technical/Skills/Skill_Visuals_Guide_TH.md) |
| Skill Tree | [SkillTree API](./02-Technical/SkillTree/SkillTreeAPI.md), [Using Exported Skill Tree JSON](./02-Technical/SkillTree/HowToUseExportedJSON.md) |
| Items | [Effect Templates](./02-Technical/Items/EffectTemplates.md), [Equipment Buff Support](./02-Technical/Items/EquipmentBuffSupport.md), [Costume System](./02-Technical/Items/Costume_System_Walkthrough.md) |
| UI | [UIBuilder Guide](./02-Technical/UI/UIBuilder_Guide.md), [Hotbar Interaction](./02-Technical/UI/Hotbar_Interaction.md), [Global Drag & Drop](./02-Technical/UI/GlobalDrag.md), [UI Components](./02-Technical/UI/UI_Components.md) |
| Technical Art | [Setup EN](./02-Technical/TechnicalArt/Technical_Art_Setup_EN.md) / [TH](./02-Technical/TechnicalArt/Technical_Art_Setup_TH.md), [Milestone 1 Walkthrough (TH)](./02-Technical/TechnicalArt/Milestone1_Walkthrough_TH.md) |

## 03-Project
- [Planning Guide (ID ranges, data mobility)](./03-Project/Planning_Guide.md)
- [Project Status Snapshot (2026-03)](./03-Project/ProjectStatusSnapshot.md)

## 04-Resources
- [Resources index](./04-Resources/README.md)
- Tool docs that stay next to their tool: [Tools/Symbol Draw/README.md](../Tools/Symbol%20Draw/README.md)

## 05-Modding
- [Modding Architecture](./05-Modding/Modding_Architecture.md)

## 99-Archive
Superseded or duplicate material kept for reference only. Do not treat as current.
- `GAME_MAP_DESIGN_ANALYSIS_duplicate.md` (identical copy of the 01-Design map analysis, was in project root)
- `REALM_OF_HERO_SYSTEM_MAP_2026-03-29.txt` (old AI-generated megadoc, was in project root)

## Conventions
- File names: `Topic_Name.md`; add `_TH` / `_EN` suffix for language variants.
- Put the date of the last code verification at the top of technical docs when possible.
- When a doc becomes outdated, move it to `99-Archive/` instead of deleting it.
