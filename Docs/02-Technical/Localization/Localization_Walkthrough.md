# Localization Walkthrough

How player-facing text is translated, how to add a text, and the rule that every text needs **EN and TH** (AGENTS.md section 9).

## The rule

Every text the player reads ships in at least English and Thai, in the same task that adds it. The EditMode test `LocalizationTests.ProjectText_HasEveryLanguage` and the GameDataWorkbench Dashboard fail when a string-table key or a data text has one language but not the other.

## Where text lives

| Kind of text | Where | How the game reads it |
|---|---|---|
| UI and code text (labels, buttons, messages, tooltips) | `Assets/Resources/Localization/Strings.json`, edited in **GameDataWorkbench > Localization** | `LocalizationDatabase.Get("key")`, or a `LocalizedTextView` component on the text object |
| Data text (item descriptions, quests, dialogue) | a `LocalizedString` field (`EN`, `TH`) in the data JSON, edited in its Workbench tab | the field itself: `item.Description` turns into the current language |

Code: `Assets/Script/CoreSystem/Localization/` (`LocalizationDatabase`, `LocalizationFormula`, `LocalizationEvents`, `LocalizedTextView`) and `Assets/Script/CoreSystem/Data/Common/LocalizedString.cs`. The checker used by the test and the Dashboard is `Assets/Script/Editor/Localization/LocalizationCoverageFormula.cs`.

## How a text is chosen

1. The current language is `GameSetting.Language` (`EN` or `TH`), set in **Settings > Language** and saved in PlayerPrefs. It is loaded when the game starts.
2. The text in that language is used.
3. If it is empty, the English text is used.
4. If there is no English text either, `LocalizationDatabase.Get` returns the key itself and logs a warning once, so a missing key is easy to see on screen.

`{0}`, `{1}` in a text are filled with the values the code passes: `LocalizationDatabase.Get("quest.progress", done, total)`. A template that does not match the values is shown as it is instead of throwing.

## Add a UI text

1. Open **Window > Pale Vertica > Pale Vertica GameDataWorkbench** and pick **SYSTEM > Localization** in the left list.
2. Press **+** to add an entry. Set the **Key** with dots, area first: `inventory.title`, `settings.language`, `common.close`. Reuse a `common.*` key when one fits.
3. Fill **EN** and **TH**. The header turns green when both are filled. **Missing only** shows the keys that still need a language.
4. Save with **Ctrl+S**. The table is written to `Assets/Resources/Localization/Strings.json` and reloaded.
5. Use the key:
   - Text in a prefab or scene: add **Localized Text View** to the object with the `TMP_Text` (or legacy `Text`) and type the key. It refreshes by itself when the language changes.
   - Text built in code: `label.text = LocalizationDatabase.Get("inventory.title");`. If the window stays open while the language changes, listen to `LocalizationEvents.OnLanguageChanged` and set the text again, or use `LocalizedTextView.SetKey(key, args)`.
6. Run the EditMode tests (or open the Workbench Dashboard). `ProjectText_HasEveryLanguage` must pass.

## Add a data text

1. Make the field a `LocalizedString`, not a `string`.
2. Give its Workbench tab two inputs, EN and TH, like **Items > Description** or **Quests > Name**.
3. Fill both for every entry. The checker finds every `LocalizedString` in `Item.json`, `Quest.json` and `Dialogue.json` by itself, including inside lists. For a new data file, add it to `DataFiles` in `LocalizationCoverageFormula`.

## Add a language

1. Add the code to `LocalizationFormula.Languages` and a field with the same name to `LocalizedString`, then a `case` in `LocalizationFormula.Raw`.
2. Add a `language.<CODE>` key and fill the new language for every key and data text. The test fails until every text has it.
3. Check that the TextMeshPro font has the glyphs (Thai uses the Noto Sans Thai assets in `Assets/Art/Front/`).

## Expected result

Changing **Settings > Language** switches every `LocalizedTextView` at once, data text reads in the new language next time it is shown, the choice survives a restart, and `LocalizationTests` passes.

## Not moved yet (later phases)

These are still plain English strings and do not follow the language yet:

- UI text written in C# (window titles, buttons, Settings labels).
- Item, skill, support and unique passive names; skill and passive descriptions.

Move them a screen or a data type at a time: add the keys or turn the field into a `LocalizedString`, fill EN and TH, then switch the code to read it.

## Common mistakes

- **Adding only English "for now".** The test fails; fill TH in the same task.
- **Hard-coding a label in C#.** Put it in the string table and read it with `LocalizationDatabase.Get`.
- **Editing `Strings.json` by hand while the Workbench is open.** The Workbench saves its own copy over it on **Ctrl+S**. Use one or the other.
- **Reading `.EN` or `.TH` directly in game code.** Use the `LocalizedString` itself (or `LocalizationFormula.Resolve`) so the English fallback works.
- **Keys with spaces or upper case.** Use lower case and dots so keys stay easy to search.
