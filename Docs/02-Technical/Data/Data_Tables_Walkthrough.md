# Data Tables Walkthrough

How to refresh the quick-reference tables in [Tables/](./Tables/README.md): active skills, support skills, unique passives and items.

The tables are generated from the data files, so they are always correct for the data at the time you ran the script. The hand-written pages in this folder ([Active_Skills.md](./Active_Skills.md), [Unique_Passives.md](./Unique_Passives.md), [Items.md](./Items.md), [Support_Skills.md](./Support_Skills.md)) explain status, merge rules and known problems, and are updated by hand ([Data_Catalog_Walkthrough.md](./Data_Catalog_Walkthrough.md)).

## Where to run it

From the project root, in any terminal (Unity can stay open):

```bash
node Tools/DataDocs/build_data_tables.js
```

It needs Node.js (any recent version). It only reads files and writes `Docs/02-Technical/Data/Tables/`.

## Inputs

| File | Used for |
|---|---|
| `Assets/Resources/Data/Skill.json` | Active skill rows (runtime numbers, targeting, tags) |
| `Assets/Resources/Skill/ActiveSkill/**/*.asset` | Which skill has an asset, and assets with no JSON entry |
| `Assets/Resources/Data/SupportSkill.json` | Support skill rows, support rune names |
| `Assets/Resources/Data/UniquePassive.json` | Passive names and descriptions |
| `Assets/Script/CoreSystem/Entity/UniquePassives/*.cs` | Which passive has a script, and the hooks each one overrides |
| `Assets/Resources/Data/SkillTree.json` | How many tree nodes grant each passive |
| `Assets/Resources/Data/Item.json`, `ItemSet.json` | Item rows per type and item sets |
| `Assets/Resources/Data/Attribute.json` | Attribute names in effect text |

## Steps

1. Change the data (Workbench, JSON or C#) and save.
2. Run the command above. It prints the counts it wrote, for example `24 skills, 22 supports, 61 passives, 202 items, 2 sets`.
3. Open [Tables/README.md](./Tables/README.md) and the table you care about. Check the notes under each table: skills with an asset but no JSON entry, passives with no script, scripts with no JSON entry.
4. Commit the regenerated files together with the data change.

## Expected result

Every page in `Tables/` shows today's date in its first line and the counts match the data. Running the script twice gives the same files apart from that date.

## Common mistakes

- **Editing a table by hand.** The next run overwrites it. Change the data, or change the script if a column is wrong.
- **Reading support or passive numbers from these tables.** They are in the C# classes; the tables show names, descriptions and hooks only.
- **An effect shows `#id (unknown)`.** The attribute id is not in `Attribute.json`, so the game reads it as 0. Record it in [Known_Data_Issues.md](./Known_Data_Issues.md) and fix the data as its own task.
- **Treating "Hooks" as what a passive does.** It is the list of methods the class overrides, which tells you when it acts. Read the class for the numbers.
