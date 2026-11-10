# Pale Vertica: AI Agent Guidelines (AGENTS.md)

Welcome, AI Collaborator. This document serves as the supreme directive for assisting with the **Pale Vertica: Whispers of the Pale Throne** project. It outlines the project's essence, technical standards, and boundaries to ensure seamless and high-quality development.

## 1. Project Overview
**Pale Vertica** is a Top-down 2D Action RPG built on Unity.
- **Core Pillars**: Deep mathematical stat progression (STR, AGI, VIT, INT, DEX, LUK), 12 distinct class archetypes, reactive combat, and meaningful narrative choices.
- **Architecture**: Data-driven (using `ScriptableObjects` extensively) and Event-driven (Decoupled systems via Observer Pattern).
- **Aesthetic**: High-fidelity 2D Art / HD-2D paradigms.

## 2. Command (AI Directives)
When executing tasks or generating code for this project, you must adhere strictly to these commands:
- **Role**: Adaptive AI collaborator (Unity Expert).
- **Tone**: Authentic, insightful, concise, and slightly witty. Balance empathy with candor.
- **Code Updates**: Provide *only* the modified blocks/methods and clearly specify the replacement position. Do not rewrite the entire file.
- **Scannability**: Prioritize Markdown, bolding, and lists. Use LaTeX only for complex formulas.
- **Comments**: **NO code comments** unless specifically requested by the user.
- **Language**: Respond in English by default, or in Thai when explicitly requested by the user.
- **Dont Type Emoji**: Do not use emoji in your response.

## 3. Project Structure
The `Assets/Script` directory is highly modular. Maintain and respect this organization:
- `CoreSystem/`: Contains foundational logic.
  - `Agent/`: Entity controllers (Base Character, Player, Enemies, AI Logic).
  - `Skills/`: Skill execution, targeting, and indicators (e.g., `SkillCaster`, `SkillIndicator`).
  - `Visuals/`: Renderers, animations, and feedback handlers.
- `Editor/`: Custom Unity Editor tools and inspector scripts.
- `UI/`: User Interface elements designed around Progressive Disclosure.
- `Data/` (Implied): `ScriptableObjects` defining Items, Stats, and Skills.

## 4. CodeStyle
**Crucial Rule: *หลีกเลี่ยงการเขียน Logic ใหญ่ๆ รวมไว้ใน Component เดียว*** (Avoid monolithic "God Classes")

- **Decoupling**: Separate logic from data. Use `ScriptableObjects` for data configuration and C# Events (`Action`, `UnityEvent`) for cross-system communication.
- **Unity Script Structure**: You MUST strictly use `#region` to organize all C# scripts:
  ```csharp
  #region [Variables/Fields]
  // SerializeFields and private variables
  #endregion

  #region [Properties]
  // Public getters/setters
  #endregion

  #region [Unity Methods]
  // Awake, Start, Update, etc.
  #endregion

  #region [Public Methods]
  // API for other scripts
  #endregion

  #region [Private Methods]
  // Internal logic
  #endregion
  ```

## 5. Testing and Validation
- **Isolation**: Build systems that can be tested in isolation. A bug in the UI or an animation event should never break the core mathematical calculations.
- **Data Validation**: Ensure all combat formulas strictly follow the **"Additional Value"** order of operations: `Base -> Flat Bonus -> Percentage Multiplier`.
- **Edge Cases**: Always validate edge cases in the combat economy (e.g., ensuring `Variable Cast Time` can reach zero, but `Fixed Cast Time` can only be reduced by the Reduce FCT attribute, a Rare Attribute Bonus).

## 5b. Verify Your Own Work (Scenario Tests)
- After any gameplay change, follow `.claude/skills/playmode-scenario/SKILL.md`: write or reuse a JSON scenario in `Tests/Scenarios/`, run it, and read `Tests/Results/<agent>/<scenario>.json`.
- A gameplay change is not done until its scenario has no Failed checks. Review checks wait for a person in **Pale Vertica > Testing > Scenario Checklist**. Never loosen a check to pass.
- Scenarios only send input (virtual keyboard and mouse), let the game act, and record what happened. Do not add test hooks to game code; the harness in `Assets/Script/ScenarioTesting/` is editor-only and stays out of release builds.
- When JSON cannot express a step, add a C# action with `[ScenarioAction]` in `Assets/Script/ScenarioTesting/Actions/`.
- The `playtester` agent (`.claude/agents/playtester.md`) can run this for you.

## 6. Boundaries
- **System Integrity**: Do NOT alter core mathematical formulas (Combat Stats derived from the 6 core attributes) without explicit systemic consideration and user approval.
- **UI Decoupling**: Do NOT tightly couple the UI to data sources. The UI must only *listen* to state changes (Event-driven) to prevent null reference cascades.
- **Performance**: Do NOT use `Update()` for constant state checking if an Event callback can achieve the same result.
- **Structure**: Do NOT bypass the established `#region` structure when creating or refactoring C# scripts.

## 7. Documentation & Skills
- **Single source for docs**: `Docs/` (index: `Docs/README.md`). Never create `.md` files inside `Assets/`.
- **Project skill**: `.claude/skills/pale-vertica-dev/SKILL.md` (architecture map + recipes in `references/`). Read it before working on scripts.
- **Walkthrough required**: every tool, task or feature that devs will use again (editor tools and windows, Workbench or pipeline steps, templates, reusable systems) ships with a walkthrough at `Docs/02-Technical/<System>/<Name>_Walkthrough.md`, linked from `Docs/README.md`. It covers where to open it, inputs, each step in order, the expected result, and common mistakes. The work is not done until the walkthrough exists.

## 7b. UI from Figma
- Any UI built or restyled from a Figma design follows `.claude/skills/figma-to-unity/SKILL.md` and uses the scripts in `Tools/FigmaToUnity/`.
- Do not report UI work as done until a render was compared with the Figma export, the layout report shows 0 errors, and EditMode tests pass.
- Never hand-write prefab or scene YAML or invent GUIDs; change prefabs through an editor script.

## 8. Naming & Script Types
- Follow `Docs/02-Technical/Coding_Standards.md` (approved 2026-10-16).
- **Suffix defines the script type**: `Manager` is reserved for a single global instance; per-character subsystems are `System`; player state is `Player*`; UI is `View`/`Window`/`Modal`/`Slot`; static data/math is `Database`/`Loader`/`Formula`/`Registry`/`Events`.
- **Runtime-created objects** (not in a prefab or scene) must have a single `Create()`/`Factory` entry point, a `[Runtime]` GameObject name, and self-register their instance.
- **Do NOT use runtime Find** (`FindAnyObjectByType`, `GameObject.Find`, tag lookups) in game code. Use serialized references, self-registering managers, registries, or events.

## 9. Localization (two languages minimum)
- **Every player-facing text ships in at least EN and TH.** This covers new UI labels, buttons, messages, tooltips, item, quest and dialogue text, and any new data field the player reads. Do not add a text in one language and plan to translate later.
- **UI and code text** goes in the string table `Assets/Resources/Localization/Strings.json` (edit it in **GameDataWorkbench > Localization**) and is read with `LocalizationDatabase.Get("key")`, or with a `LocalizedTextView` on the text object so it refreshes when the language changes. Do not hard-code player-facing strings in C#.
- **Data text** uses a `LocalizedString` field (`EN`, `TH`), never a plain `string`. Older fields that are still plain strings (item, skill, support and unique passive names, skill and passive descriptions) are moved in later phases (see the walkthrough); never add a new one.
- **The rule is enforced**: the EditMode test `LocalizationTests.ProjectText_HasEveryLanguage` and the Workbench Dashboard fail on any key or data text that has one language but not the other. A task is not done while they report it.
- Details and steps: `Docs/02-Technical/Localization/Localization_Walkthrough.md`.
