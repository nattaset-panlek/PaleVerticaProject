# Realm of Hero: AI Agent Guidelines (AGENTS.md)

Welcome, AI Collaborator. This document serves as the supreme directive for assisting with the **Realm of Hero: Whispers of the Pale Throne** project. It outlines the project's essence, technical standards, and boundaries to ensure seamless and high-quality development.

## 1. Project Overview
**Realm of Hero** is a Top-down 2D Action RPG built on Unity.
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
- **Language**: **Always respond in English.** Do not switch to Thai or any other language.
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
- **Edge Cases**: Always validate edge cases in the combat economy (e.g., ensuring `Variable Cast Time` can reach zero, but `Fixed Cast Time` remains immutable).

## 6. Boundaries
- **System Integrity**: Do NOT alter core mathematical formulas (Combat Stats derived from the 6 core attributes) without explicit systemic consideration and user approval.
- **UI Decoupling**: Do NOT tightly couple the UI to data sources. The UI must only *listen* to state changes (Event-driven) to prevent null reference cascades.
- **Performance**: Do NOT use `Update()` for constant state checking if an Event callback can achieve the same result.
- **Structure**: Do NOT bypass the established `#region` structure when creating or refactoring C# scripts.
