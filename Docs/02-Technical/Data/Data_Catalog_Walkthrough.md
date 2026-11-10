# Data Catalog Walkthrough

Verified: **2026-10-17**. How to keep the game data catalog in `Docs/02-Technical/Data/` in sync after skills, supports, unique passives, items, attributes or formulas change.

The catalog is written by hand from the real data and code. Quick generated tables of the same data live in [Tables/](./Tables/README.md) ([Data_Tables_Walkthrough.md](./Data_Tables_Walkthrough.md)); regenerate them after any data change, and use them to spot rows that changed since the Verified date. Every page has a **Verified** date at the top. A page is correct only for the data as of that date.

## When to update

Update the catalog in the same task that changes any of these:

| You changed | Update |
|---|---|
| `Assets/Resources/Data/Skill.json`, an asset in `Assets/Resources/Skill/ActiveSkill/` (subfolders included), or `Skills/UniqueSkill/` | [Active_Skills.md](./Active_Skills.md): summary row and detail section. Also the compatibility matrix in [Support_Skills.md](./Support_Skills.md) if tags, element, damage type or targeting changed. |
| `SupportSkill.json` or `Assets/Script/CoreSystem/Skills/Modifiers/` | [Support_Skills.md](./Support_Skills.md): catalog row, matrix, description templates |
| `UniquePassive.json` or `Assets/Script/CoreSystem/Entity/UniquePassives/` | [Unique_Passives.md](./Unique_Passives.md) |
| `Item.json`, `ItemSet.json` or `Assets/Script/CoreSystem/Data/Items/` | [Items.md](./Items.md) |
| `Attribute.json`, `AttributeSystem`, anything in `Assets/Script/CoreSystem/Combat/Formulas/` | [Base_Formulas.md](./Base_Formulas.md), including the worked examples |
| A difference found between code, data and docs | [Known_Data_Issues.md](./Known_Data_Issues.md) |

Changing a formula in code needs user approval first (AGENTS.md section 6). The catalog only describes what the code does.

## Steps

1. **Find what changed.** From the project root:
   ```bash
   git status --short -- Assets/Resources/Data Assets/Resources/Skill Assets/Script/CoreSystem/Combat/Formulas Assets/Script/CoreSystem/Skills Assets/Script/CoreSystem/Entity/UniquePassives Assets/Script/CoreSystem/Data/Items
   ```
   This also lists new files (a new skill asset is untracked until it is committed, so `git diff` alone misses it). For changes that are already committed, use `git log --since=<Verified date> --stat -- <same paths>`.

2. **Read the real values**, never from memory or old docs.
   - One active skill's runtime numbers (replace `1020`):
     ```bash
     python -c "import json;s={x['SkillID']:x for x in json.load(open('Assets/Resources/Data/Skill.json',encoding='utf-8'))['ActiveSkills']};k=s[1020];print({f:k[f] for f in ['SkillName','SkillElement','DamageType','BaseDamage','BaseManaCost','BaseVCT','BaseFCT','BaseCastDelay','BaseCooldown','BaseProjectileCount']},'radius',k['BaseTargeting']['Radius'],k['Tags'],k['Scalings'])"
     ```
   - The same skill's targeting and effects come from its asset: open `Assets/Resources/Skill/ActiveSkill/<Name>_<id>.asset` and read the `references:` block (`grep -n "type: {class" <asset>` lists the targeting class and each effect). Settings such as cast range, cone angle, box size, dash distance and stun time are in the `data:` lines under each one.
   - One item (replace `3048`):
     ```bash
     python -c "import json,sys;sys.stdout.reconfigure(encoding='utf-8');i={x['ItemId']:x for x in json.load(open('Assets/Resources/Data/Item.json',encoding='utf-8'))['Items']}[3048];print(i['Name'],i['EquipmentSlot'],i['BasePhysical'],i['BaseMagic'],i['MaxSockets'],[(e['EffectType'],e['AttributeID'],e['Value'],e['Category']) for e in i['Effects']])"
     ```
   - Support and unique passive numbers are fields in the C# class (`<Name>SupportModifier.cs`, `<Name>UniquePassive.cs`), not in the JSON.

3. **Apply the merge rules** in [README.md](./README.md#how-an-active-skill-is-assembled-at-runtime) to get the value the game actually uses: `Skill.json` overrides the asset's numbers, tags and scalings; the radius comes from `BaseTargeting.Radius`, not the top-level `BaseAOERadius`; the `Effects` block in `Skill.json` is ignored at runtime.

4. **Edit the page.** Keep the existing table columns, units (README "Units used in these pages") and status words (Working / Partial / Prototype / Data only). Change a summary row and its detail section together. If a count changes (23 skills, 22 supports, 15 unique passives, 199 items), update it on the page, in [README.md](./README.md) and in `Docs/README.md`.

5. **Recompute worked examples** in [Base_Formulas.md](./Base_Formulas.md) when their inputs change: the stat example (STR 30, AGI 40, VIT 20, INT 50, DEX 30, LUK 10), the Fire Bolt damage and cast-time examples, and the refine and reforge example. Compute them with a short script rather than by hand.

6. **Record mismatches.** When the code, the data and a description disagree (for example a JSON description promising an effect the C# class does not apply), the code wins. Add a row to [Known_Data_Issues.md](./Known_Data_Issues.md) with severity, where, and the effect today. Do not change code, JSON or assets while documenting.

7. **Close out.** Set the **Verified** date on every page you touched. Check that the relative links still work. If the same numbers are copied into the GDD (the Pale Vertica project in GDD Tools has catalog modules for skills, supports and passives, items and known issues), update them there too or mark them as a 2026-10-17 snapshot ([GDD Tools Walkthrough](../Tools/GDD_Tools_Walkthrough.md)).

8. **Optional runtime check.** For behaviour that is hard to read from code (repeats, procs, combos), confirm it in Play mode with the dev tool (press **K**) before writing it down.

## Expected result

Every number on the pages matches the current files, each touched page shows today's Verified date, every new mismatch is listed in Known_Data_Issues, and `Docs/README.md` still links all pages.

## Common mistakes

- **Copying numbers from the asset when `Skill.json` overrides them.** The asset's `BaseDamage`, MP, VCT/FCT, cooldown, tags and scalings are replaced at runtime.
- **Using `BaseAOERadius`.** The runtime radius is `BaseTargeting.Radius` in `Skill.json` (example: Chornos Field's asset says 20, the game uses 2).
- **Reading the `Effects` values in `Skill.json`** (such as 250 for Cherry Blossom Execution). They are Workbench data and do not reach the game.
- **Looking for support or passive numbers in the JSON.** They live in the C# classes; the JSON only has names and description templates.
- **Treating a scaling attribute id as valid without checking.** Ids not in `AttributeType` (for example 0 and 221) read as 0 at runtime.
- **Writing percent bonuses from equipment as normal attributes.** An `IsPercent` attribute adds to itself and a PercentAdd to the attribute with id - 1 ([Base_Formulas 2a](./Base_Formulas.md#2a-percent-attributes-on-equipment)).
- **Fixing the data while documenting it.** Record the issue instead, then fix it as its own task.
