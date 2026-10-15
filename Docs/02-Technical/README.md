# Technical Documentation

System-level implementation docs, one folder per system. Source code lives in `Assets/Script/`; these docs describe it.

| Folder | Code area | Contents |
|---|---|---|
| `Combat/` | `CoreSystem/Combat` | `CombatCore` pipeline: Gate, Dispatch, Offense, Defense, Riders, Apply |
| `Attributes/` | `CoreSystem/Attributes`, `CoreSystem/Enums/AttributeType.cs` | Stat pipeline, AdditionalChain, all formulas |
| `Character/` | `CoreSystem/Entity`, `CoreSystem/Agent`, `CoreSystem/Player`, `CoreSystem/Visuals` | Character System (Character, ControlSystem, PlayerCore, AgentCore), architecture plan, visual architecture |
| `Skills/` | `CoreSystem/Skills` | Unique skills, combo chains, passive V2 plan, skill VFX handover |
| `SkillTree/` | `CoreSystem/SkillTree` | SkillTree API and JSON export usage |
| `Items/` | `CoreSystem/Data/Items` | Equipment effect JSON templates, equipment buffs, costumes |
| `UI/` | `CoreSystem/UI`, `Script/UI` | UIBuilder framework, Hotbar, Global Drag & Drop, custom components |
| `TechnicalArt/` | `CoreSystem/Visuals`, shaders/materials | Shader + material setup, milestone walkthrough |
| `Coding_Standards.md` | All of `Assets/Script` | Script-type suffixes, runtime-created objects, reference rules, rename safety, migration plan |
| `Testing.md` | `Script/Editor/Tests` | EditMode test suite and how to run it |
