#!/usr/bin/env node
// Builds simple Markdown tables of skills, supports, unique passives and items from the game data.
// Usage (from the project root): node Tools/DataDocs/build_data_tables.js
// Output: Docs/02-Technical/Data/Tables/*.md (overwritten every run).

const fs = require('fs');
const path = require('path');

const root = path.resolve(__dirname, '..', '..');
const dataDir = path.join(root, 'Assets', 'Resources', 'Data');
const skillAssetDir = path.join(root, 'Assets', 'Resources', 'Skill', 'ActiveSkill');
const passiveDir = path.join(root, 'Assets', 'Script', 'CoreSystem', 'Entity', 'UniquePassives');
const outDir = path.join(root, 'Docs', '02-Technical', 'Data', 'Tables');
const today = new Date().toISOString().slice(0, 10);

function readJson(name) {
  return JSON.parse(fs.readFileSync(path.join(dataDir, name), 'utf8'));
}

function listFiles(dir, filter) {
  if (!fs.existsSync(dir)) return [];
  const result = [];
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const full = path.join(dir, entry.name);
    if (entry.isDirectory()) result.push(...listFiles(full, filter));
    else if (filter(entry.name)) result.push(full);
  }
  return result;
}

function cell(value) {
  if (value === undefined || value === null || value === '') return '';
  return String(value).replace(/\r?\n/g, ' ').replace(/\|/g, '\\|').trim();
}

function num(value) {
  if (value === undefined || value === null || value === '') return '';
  const n = Number(value);
  return Number.isFinite(n) ? String(Math.round(n * 100) / 100) : String(value);
}

function text(value) {
  if (value && typeof value === 'object') return value.EN || value.en || Object.values(value)[0] || '';
  return value || '';
}

function table(headers, rows) {
  const lines = [`| ${headers.join(' | ')} |`, `|${headers.map(() => '---').join('|')}|`];
  for (const row of rows) lines.push(`| ${row.map(cell).join(' | ')} |`);
  return lines.join('\n');
}

function header(title, sources, intro) {
  return [
    `# ${title}`,
    '',
    `Generated ${today} by \`node Tools/DataDocs/build_data_tables.js\` from ${sources.map((s) => `\`${s}\``).join(', ')}. Do not edit by hand; change the data and run the script again.`,
    '',
    intro,
    '',
    'Back to [all tables](./README.md).',
    ''
  ].join('\n');
}

function write(name, content) {
  fs.writeFileSync(path.join(outDir, name), content.trimEnd() + '\n', 'utf8');
}

const attributes = new Map();
for (const attribute of readJson('Attribute.json').Attributes) attributes.set(attribute.Id, attribute.Mapping || attribute.Name);

function attributeName(id) {
  return attributes.has(id) ? attributes.get(id) : `#${id} (unknown)`;
}

function effectText(effect) {
  if (!effect) return '';
  const signed = (value) => (Number(value) >= 0 ? `+${num(value)}` : num(value));
  let core;
  if (effect.EffectType === 'AttributeBonusEffect' || effect.EffectType === 'StatBonus') core = `${signed(effect.Value)} ${attributeName(effect.AttributeID)}`;
  else if (effect.EffectType === 'DamageEffect') core = `damage ${num(effect.Value)}`;
  else {
    const parts = [effect.EffectType];
    if (effect.AttributeID) parts.push(attributeName(effect.AttributeID));
    if (effect.Value) parts.push(signed(effect.Value));
    if (effect.SkillID) parts.push(`skill ${effect.SkillID}`);
    if (effect.BuffName) parts.push(effect.BuffName);
    core = parts.join(' ');
  }
  const extra = [];
  if (effect.TriggerType) extra.push(`${effect.TriggerType}${effect.TriggerChance ? ` ${num(effect.TriggerChance)}%` : ''}`);
  if (effect.Duration) extra.push(`${num(effect.Duration)}s`);
  return extra.length ? `${core} (${extra.join(', ')})` : core;
}

function effectsText(effects) {
  return (effects || []).map(effectText).filter(Boolean).join('; ');
}

fs.mkdirSync(outDir, { recursive: true });

const skillData = readJson('Skill.json');
const activeSkills = [...skillData.ActiveSkills].sort((a, b) => a.SkillID - b.SkillID);
const supportData = readJson('SupportSkill.json');
const supports = [...(Array.isArray(supportData) ? supportData : Object.values(supportData).find(Array.isArray) || [])].sort((a, b) => a.SkillID - b.SkillID);
const supportNames = new Map(supports.map((s) => [s.SkillID, s.ModifierName]));
const skillNames = new Map(activeSkills.map((s) => [s.SkillID, s.SkillName]));

const skillAssets = new Map();
for (const file of listFiles(skillAssetDir, (n) => n.endsWith('.asset'))) {
  const match = path.basename(file).match(/_(\d+)\.asset$/);
  if (match) skillAssets.set(Number(match[1]), path.relative(skillAssetDir, file).replace(/\\/g, '/'));
}

const skillRows = activeSkills.map((s) => {
  const targeting = s.BaseTargeting || {};
  return [
    s.SkillID, s.SkillName, s.SkillElement, s.DamageType, num(s.BaseDamage), num(s.BaseManaCost),
    num(s.BaseVCT), num(s.BaseFCT), num(s.BaseCastDelay), num(s.BaseCooldown),
    num(targeting.Radius), targeting.Type || '', s.IsToggle ? 'yes' : '',
    (s.Tags || []).join(', '), (s.SupportSkillIDs || []).length, skillAssets.get(s.SkillID) || 'missing'
  ];
});
const assetsWithoutJson = [...skillAssets.keys()].filter((id) => !skillNames.has(id)).sort((a, b) => a - b);

write('Active_Skills.md', [
  header('Active Skills Table', ['Assets/Resources/Data/Skill.json', 'Assets/Resources/Skill/ActiveSkill/**/*.asset'],
    `${activeSkills.length} active skills. Numbers are the \`Skill.json\` values, which override the asset at runtime. Radius is \`BaseTargeting.Radius\`. Time is in seconds. The detailed page with status and notes is [Active_Skills.md](../Active_Skills.md).`),
  table(['ID', 'Name', 'Element', 'Type', 'Base dmg', 'MP', 'VCT', 'FCT', 'Cast delay', 'CD', 'Radius', 'Targeting', 'Toggle', 'Tags', 'Support links', 'Asset'], skillRows),
  '',
  assetsWithoutJson.length ? `Assets with no \`Skill.json\` entry (they use the asset values only): ${assetsWithoutJson.map((id) => `${id} (\`${skillAssets.get(id)}\`)`).join(', ')}.` : 'Every skill asset has a `Skill.json` entry.'
].join('\n'));

write('Support_Skills.md', [
  header('Support Skills Table', ['Assets/Resources/Data/SupportSkill.json'],
    `${supports.length} support skills. Their numbers live in the C# modifier classes; \`{0}\` in a description is filled in by the game. The detailed page is [Support_Skills.md](../Support_Skills.md).`),
  table(['ID', 'Name', 'Description'], supports.map((s) => [s.SkillID, s.ModifierName, text(s.Description)]))
].join('\n'));

const passiveData = readJson('UniquePassive.json').UniquePassives;
const passiveScripts = new Map();
for (const file of listFiles(passiveDir, (n) => n.endsWith('.cs'))) {
  const source = fs.readFileSync(file, 'utf8');
  const id = (source.match(/\[UniquePassiveId\("([^"]+)"\)\]/) || [])[1];
  if (!id) continue;
  const hooks = [...source.matchAll(/public override [\w<>\[\], ]+? (\w+)\s*\(/g)].map((m) => m[1]);
  passiveScripts.set(id, { file: path.basename(file), hooks: [...new Set(hooks)] });
}
const treeSource = fs.readFileSync(path.join(dataDir, 'SkillTree.json'), 'utf8');
const treeUses = new Map();
for (const match of treeSource.matchAll(/"uniquePassiveId"\s*:\s*"([^"]+)"/g)) treeUses.set(match[1], (treeUses.get(match[1]) || 0) + 1);

const passiveIds = new Set([...passiveData.map((p) => p.Id), ...passiveScripts.keys()]);
const passiveRows = [...passiveIds].sort().map((id) => {
  const data = passiveData.find((p) => p.Id === id);
  const script = passiveScripts.get(id);
  return [id, data ? data.Name : '(no JSON entry)', data ? text(data.Description) : '', script ? script.hooks.join(', ') : '(no script)', treeUses.get(id) || ''];
});
const noScript = passiveData.filter((p) => !passiveScripts.has(p.Id)).map((p) => p.Id);
const noJson = [...passiveScripts.keys()].filter((id) => !passiveData.some((p) => p.Id === id));

write('Unique_Passives.md', [
  header('Unique Passives Table', ['Assets/Resources/Data/UniquePassive.json', 'Assets/Script/CoreSystem/Entity/UniquePassives/*.cs', 'Assets/Resources/Data/SkillTree.json'],
    `${passiveIds.size} unique passives. The name and description come from the JSON; the numbers are in each C# class. **Hooks** are the methods the class overrides (when it acts). **Tree nodes** is how many Skill Tree nodes grant it. The detailed page is [Unique_Passives.md](../Unique_Passives.md).`),
  table(['ID', 'Name', 'Description', 'Hooks', 'Tree nodes'], passiveRows),
  '',
  noScript.length ? `In the JSON but with no script (does nothing in game): ${noScript.join(', ')}.` : 'Every JSON entry has a script.',
  '',
  noJson.length ? `Script with no JSON entry (no name or description in game): ${noJson.join(', ')}.` : 'Every script has a JSON entry.'
].join('\n'));

const items = [...readJson('Item.json').Items].sort((a, b) => a.ItemId - b.ItemId);
const sets = readJson('ItemSet.json').Sets || [];
const setNames = new Map(sets.map((s) => [s.SetID, s.SetName]));
const types = [...new Set(items.map((i) => i.ItemType))];
const describe = (item) => {
  const value = text(item.Description);
  return value.length > 90 ? `${value.slice(0, 87)}...` : value;
};

const itemColumns = {
  Equipment: [['Slot', (i) => i.EquipmentSlot], ['Rarity', (i) => i.Rarity], ['Phys', (i) => num(i.BasePhysical)], ['Magic', (i) => num(i.BaseMagic)], ['Sockets', (i) => num(i.MaxSockets)], ['Set', (i) => (i.SetID ? setNames.get(i.SetID) || i.SetID : '')], ['Effects', (i) => effectsText(i.Effects)]],
  Consumable: [['Heal HP', (i) => num(i.HealHP)], ['Heal MP', (i) => num(i.HealMP)], ['Stack', (i) => num(i.MaxStack)], ['Effects', (i) => effectsText(i.Effects)]],
  SkillRune: [['Grants skill', (i) => (i.SkillID ? `${i.SkillID} ${skillNames.get(i.SkillID) || '(missing)'}` : '')], ['Rarity', (i) => i.Rarity]],
  SupportRune: [['Support', (i) => (i.SkillID ? `${i.SkillID} ${supportNames.get(i.SkillID) || '(missing)'}` : '')], ['Rarity', (i) => i.Rarity]],
  Fragment: [['Kind', (i) => i.FragmentKind], ['Rarity', (i) => i.Rarity], ['Effects', (i) => effectsText(i.Effects)]],
  Progression: [['Kind', (i) => i.ProgressionKind], ['Stack', (i) => num(i.MaxStack)]],
  Costume: [['Slot', (i) => i.CostumeSlot], ['Rarity', (i) => i.Rarity], ['Effects', (i) => effectsText(i.Effects)]]
};
const defaultColumns = [['Rarity', (i) => i.Rarity], ['Stack', (i) => num(i.MaxStack)], ['Effects', (i) => effectsText(i.Effects)]];

const itemSections = types.map((type) => {
  const list = items.filter((i) => i.ItemType === type);
  const columns = itemColumns[type] || defaultColumns;
  return [
    `## ${type} (${list.length})`,
    '',
    table(['ID', 'Name', ...columns.map((c) => c[0]), 'Description'], list.map((i) => [i.ItemId, i.Name, ...columns.map((c) => c[1](i)), describe(i)]))
  ].join('\n');
});

const setRows = sets.map((s) => [s.SetID, s.SetName, (s.BonusLevels || []).map((b) => `${b.RequiredPieces} pieces: ${effectsText(b.Effects)}`).join('; '), items.filter((i) => i.SetID === s.SetID).map((i) => i.Name).join(', ')]);

write('Items.md', [
  header('Items Table', ['Assets/Resources/Data/Item.json', 'Assets/Resources/Data/ItemSet.json', 'Assets/Resources/Data/Attribute.json'],
    `${items.length} items in ${types.length} types and ${sets.length} sets. Effects show the attribute by its \`AttributeType\` name; an id that is not in \`Attribute.json\` shows as \`#id (unknown)\`. The detailed page with known problems is [Items.md](../Items.md).`),
  '## Overview',
  '',
  table(['Type', 'Count', 'Id range', 'With effects'], types.map((type) => {
    const list = items.filter((i) => i.ItemType === type);
    return [type, list.length, `${list[0].ItemId}-${list[list.length - 1].ItemId}`, list.filter((i) => (i.Effects || []).length).length];
  })),
  '',
  ...itemSections.map((s) => s + '\n'),
  '## Item sets',
  '',
  table(['Set ID', 'Name', 'Bonuses', 'Pieces'], setRows)
].join('\n'));

write('README.md', [
  '# Data Tables',
  '',
  `Generated ${today} by \`node Tools/DataDocs/build_data_tables.js\`. Quick tables for reading; the hand-written pages one folder up explain status, merge rules and known problems.`,
  '',
  table(['Table', 'Count', 'Source'], [
    ['[Active Skills](./Active_Skills.md)', activeSkills.length, 'Skill.json, skill assets'],
    ['[Support Skills](./Support_Skills.md)', supports.length, 'SupportSkill.json'],
    ['[Unique Passives](./Unique_Passives.md)', passiveIds.size, 'UniquePassive.json, C# classes, SkillTree.json'],
    ['[Items](./Items.md)', items.length, 'Item.json, ItemSet.json']
  ]),
  '',
  'To refresh after changing data: `node Tools/DataDocs/build_data_tables.js` from the project root. Walkthrough: [Data_Tables_Walkthrough.md](../Data_Tables_Walkthrough.md).'
].join('\n'));

console.log(`Wrote ${outDir}: ${activeSkills.length} skills, ${supports.length} supports, ${passiveIds.size} passives, ${items.length} items, ${sets.length} sets.`);
