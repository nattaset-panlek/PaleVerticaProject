# Technical Documentation

System-level implementation docs, one folder per system. Source code lives in `Assets/Script/`; these docs describe it.

| Folder | Code area | Contents |
|---|---|---|
| `Attributes/` | `CoreSystem/Attributes`, `CoreSystem/Enums/AttributeType.cs` | Stat pipeline, AdditionalChain, all formulas |
| `Character/` | `CoreSystem/Entity`, `CoreSystem/Agent`, `CoreSystem/Visuals` | CommandDirector, NPC/AgentCore, EntityVisualController |
| `Skills/` | `CoreSystem/Skills` | Unique skills, combo chains, passive V2 plan, skill VFX handover |
| `SkillTree/` | `CoreSystem/SkillTree` | SkillTree API and JSON export usage |
| `Items/` | `CoreSystem/Data/Items` | Equipment effect JSON templates, equipment buffs, costumes |
| `UI/` | `CoreSystem/UI`, `Script/UI` | UIBuilder framework, Hotbar, Global Drag & Drop, custom components |
| `TechnicalArt/` | `CoreSystem/Visuals`, shaders/materials | Shader + material setup, milestone walkthrough |
