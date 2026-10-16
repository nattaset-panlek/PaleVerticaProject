# Items Catalog

Verified against `Assets/Resources/Data/Item.json`, `ItemSet.json` and `Assets/Script/CoreSystem/Data/Items/`: **2026-10-17**. Generated from the data files (199 items, 2 sets); the effect text is how `ItemDatabase` reads each JSON effect.

How the effects work: [Base_Formulas 8](./Base_Formulas.md#8-equipment-and-progression-progressionformula). Problems marked below are explained in [Known_Data_Issues.md](./Known_Data_Issues.md).

Markers:
- **[U]** element / size / race value. The damage formula reads these as fractions (0.25 = 25%), so the whole numbers in the data act as huge multipliers (`x (1 + 25)`) or full immunity (`1 - 25` -> 0).
- **[A]** condition on `HPPercent`. That attribute is a bonus stat (normally 0), not "current HP %", so `<= 30` is always true and the bonus is always on.
- **[T]** trigger type `OnAttack` does not exist (only `OnHit`, `OnTakeDamage`); `ItemDatabase` drops the effect.
- **[M]** the skill id does not exist in `Skill.json`; nothing is granted or cast.

## Overview

| Type | Count | Id range | Has effects |
|---|---|---|---|
| Currency | 20 | 1001-1020 | 0 |
| Consumable | 20 | 2001-2020 | 0 |
| Equipment | 48 | 3001-3048 | 29 |
| Fragment | 20 | 4001-4020 | 6 |
| Resource | 20 | 5001-5020 | 0 |
| SkillRune | 17 | 6001-6017 | 0 |
| SupportRune | 22 | 7001-7022 | 0 |
| Costume | 12 | 8001-8012 | 1 |
| Additional | 20 | 9001-9020 | 0 |

## Equipment (3001-3048)

Baseline from `BasePhysical` / `BaseMagic`, at refine +0 (each value is at least 1): weapons give **WeaponATK / WeaponMATK**, every other slot gives **Defense / StatusDEF**. Refine multiplies the baseline (refine table in Base_Formulas 8b), reforge adds +5 per level. Sockets hold fragments, whose effects apply while the item is equipped.

| ID | Name | Slot | Rarity | Baseline (refine +0) | Sockets | Set | Effects |
|---|---|---|---|---|---|---|---|
| 3001 | Peasant Hat | Helm | Common | DEF 5 / SDEF 1 | 0 | - | - |
| 3002 | Peasant Shirt | Armor | Common | DEF 10 / SDEF 2 | 0 | - | - |
| 3003 | Leather Helm | Helm | Uncommon | DEF 12 / SDEF 5 | 1 | - | - |
| 3004 | Leather Armor | Armor | Uncommon | DEF 20 / SDEF 8 | 1 | - | - |
| 3005 | Iron Sword | WeaponOneHandLeft | Uncommon | WATK 35 / WMATK 5 | 1 | - | - |
| 3006 | Iron Shield | AccessoryLeft | Uncommon | DEF 25 / SDEF 10 | 1 | - | - |
| 3007 | Steel Axe | WeaponTwoHand | Rare | WATK 65 / WMATK 15 | 2 | - | - |
| 3008 | Steel Gauntlets | Glove | Rare | DEF 15 / SDEF 5 | 1 | - | WaterResist +25 [U]; grants missing skill 102 [M] |
| 3009 | Mithril Chainmail | Armor | Rare | DEF 35 / SDEF 15 | 2 | - | - |
| 3010 | Mithril Boots | AccessoryRight | Rare | DEF 10 / SDEF 10 | 1 | - | MoveSpeed +15%; ASPD +10 |
| 3011 | Adamantite Helm | Helm | Epic | DEF 45 / SDEF 20 | 2 | - | - |
| 3012 | Adamantite Mace | WeaponOneHandLeft | Epic | WATK 95 / WMATK 35 | 3 | - | - |
| 3013 | Dragon Scale Armor | Armor | Legendary | DEF 80 / SDEF 40 | 3 | - | DragonDamage +25 [U]; if MaxHP >= 5000: ATK +50; 10% OnTakeDamage: cast missing skill 202 [M] |
| 3014 | Dragon Tooth Pendant | Talisman | Legendary | DEF 5 / SDEF 20 | 1 | - | DragonDamage +25 [U]; if MaxHP >= 5000: ATK +50; 10% OnTakeDamage: cast missing skill 202 [M] |
| 3015 | Demon Blade | WeaponOneHandLeft | Legendary | WATK 150 / WMATK 70 | 4 | - | DemonDamage +20 [U]; 8% OnAttack: cast missing skill 108 [M] [T]; if HPPercent <= 30: Defense +100 [A] |
| 3016 | Demon Horn | Helm | Legendary | DEF 30 / SDEF 65 | 2 | - | DemonDamage +20 [U]; 8% OnAttack: cast missing skill 108 [M] [T]; if HPPercent <= 30: Defense +100 [A] |
| 3017 | Angelic Halo | Helm | Legendary | DEF 25 / SDEF 75 | 2 | - | - |
| 3018 | Angelic Wings | Armor | Legendary | DEF 60 / SDEF 85 | 3 | - | - |
| 3019 | Void Robe | Armor | Epic | DEF 35 / SDEF 95 | 3 | - | GhostDamage +20 [U]; 5% OnHit: cast missing skill 110 [M]; if HPPercent <= 30: Defense +100 [A] |
| 3020 | Void Gloves | Glove | Epic | DEF 15 / SDEF 40 | 2 | - | GhostDamage +20 [U]; 5% OnHit: cast missing skill 110 [M]; if HPPercent <= 30: Defense +100 [A] |
| 3021 | Crystal Staff | WeaponTwoHand | Rare | WATK 1 / WMATK 110 | 3 | - | - |
| 3022 | Crystal Ring | AccessoryLeft | Rare | DEF 5 / SDEF 25 | 1 | - | - |
| 3023 | Shadow Cowl | Helm | Epic | DEF 30 / SDEF 50 | 2 | - | - |
| 3024 | Shadow Dagger | WeaponOneHandLeft | Epic | WATK 85 / WMATK 45 | 2 | - | - |
| 3025 | Fire Wand | WeaponOneHandLeft | Uncommon | WATK 15 / WMATK 70 | 1 | - | FireDamage +15 [U]; grants missing skill 101 [M] |
| 3026 | Fire Amulet | Talisman | Uncommon | DEF 2 / SDEF 20 | 0 | - | FireDamage +15 [U]; grants Aura of Flames (1008) |
| 3027 | Ice Crown | Helm | Epic | DEF 25 / SDEF 60 | 2 | - | WaterResist +25 [U]; grants Healing (1002); if HPPercent <= 30: Defense +100 [A] |
| 3028 | Ice Orb | AccessoryRight | Epic | DEF 5 / SDEF 30 | 2 | - | WaterResist +25 [U]; grants Divine Blessing (1004); if HPPercent <= 30: Defense +100 [A] |
| 3029 | Lightning Spear | WeaponTwoHand | Legendary | WATK 140 / WMATK 85 | 3 | - | - |
| 3030 | Lightning Pendant | Accessory | Legendary | DEF 20 / SDEF 35 | 2 | - | - |
| 3031 | Overpower Talisman | Talisman | Legendary | DEF 5 / SDEF 5 | 2 | - | - |
| 3032 | Sage's Ring | Accessory | Epic | DEF 1 / SDEF 15 | 1 | - | INT +10; MPRecovery +50 per 10 INT |
| 3033 | Vibrant Belt | Glove | Rare | DEF 15 / SDEF 1 | 1 | - | VIT +5; MaxHP +1 per 2 MaxMP |
| 3034 | Chrono Pendant | AccessoryRight | Legendary | DEF 1 / SDEF 20 | 1 | - | CooldownReduc +5; VariableCastTimeReducPercent +10 per 50 INT + DEX; grants Time Dilation (1014) |
| 3035 | Hero's Medallion | Talisman | Legendary | DEF 10 / SDEF 10 | 2 | - | ATK +5 per 50 BaseLevel; MATK +5 per 50 BaseLevel; if BaseLevel >= 100: MaxHP +1000 |
| 3036 | Swift Blade | WeaponOneHandRight | Epic | WATK 15 / WMATK 5 | 2 | SwiftBlader | MoveSpeed +15%; ASPD +10; if HPPercent <= 30: Defense +100 [A] |
| 3037 | Swift Jacket | Armor | Epic | DEF 10 / SDEF 1 | 1 | SwiftBlader | MoveSpeed +15%; ASPD +10; if HPPercent <= 30: Defense +100 [A] |
| 3038 | Swift Cap | Helm | Epic | DEF 5 / SDEF 1 | 1 | SwiftBlader | MoveSpeed +15%; ASPD +10; if HPPercent <= 30: Defense +100 [A] |
| 3039 | Swift Gloves | Glove | Epic | DEF 5 / SDEF 1 | 1 | SwiftBlader | MoveSpeed +15%; ASPD +10; if HPPercent <= 30: Defense +100 [A] |
| 3040 | Swift Ring | Accessory | Epic | DEF 1 / SDEF 5 | 0 | SwiftBlader | MoveSpeed +15%; ASPD +10; if HPPercent <= 30: Defense +100 [A] |
| 3041 | Swift Charm | Talisman | Epic | DEF 1 / SDEF 5 | 0 | SwiftBlader | MoveSpeed +15%; ASPD +10; if HPPercent <= 30: Defense +100 [A] |
| 3042 | Time Staff | WeaponTwoHand | Legendary | WATK 5 / WMATK 25 | 3 | TemporalSage | CooldownReduc +10; VariableCastTimeReduc +1.5; if HPPercent <= 30: Defense +100 [A]; grants Force Nova (1015); at refine +6: AOE +150% |
| 3043 | Time Robe | Armor | Legendary | DEF 1 / SDEF 15 | 2 | TemporalSage | CooldownReduc +10; VariableCastTimeReduc +1.5; if HPPercent <= 30: Defense +100 [A]; 10% OnTakeDamage: cast missing skill 202 [M] |
| 3044 | Time Hat | Helm | Legendary | DEF 1 / SDEF 10 | 1 | TemporalSage | CooldownReduc +10; VariableCastTimeReduc +1.5; if HPPercent <= 30: Defense +100 [A]; 10% OnTakeDamage: cast missing skill 202 [M] |
| 3045 | Time Mittens | Glove | Legendary | DEF 1 / SDEF 5 | 1 | TemporalSage | CooldownReduc +10; VariableCastTimeReduc +1.5; if HPPercent <= 30: Defense +100 [A]; 10% OnTakeDamage: cast missing skill 202 [M] |
| 3046 | Time Hourglass | Accessory | Legendary | DEF 1 / SDEF 5 | 0 | TemporalSage | CooldownReduc +10; VariableCastTimeReduc +1.5; if HPPercent <= 30: Defense +100 [A]; 10% OnTakeDamage: cast Tackle (1001) |
| 3047 | Swiftwind Bracers | Glove | Legendary | DEF 1 / SDEF 1 | 1 | - | ASPD +40 |
| 3048 | Vampiric Soul-Reaver | WeaponTwoHand | Legendary | WATK 85 / WMATK 1 | 3 | - | unique passive Vampire |

By rarity: Legendary 18, Epic 15, Rare 7, Uncommon 6, Common 2.
By slot: Helm 9, Armor 8, Glove 6, WeaponOneHandLeft 5, WeaponTwoHand 5, Talisman 5, Accessory 4, AccessoryRight 3, AccessoryLeft 2, WeaponOneHandRight 1.

## Equipment sets (`ItemSet.json`)

Each tier is active while at least that many pieces of the set are equipped, and tiers add up.

| Set | Pieces | Bonus | Total at this tier | Members |
|---|---|---|---|---|
| Swift Blader (`SwiftBlader`) | 2 | CooldownReducPercent +10% | 10% | 3036 Swift Blade, 3037 Swift Jacket, 3038 Swift Cap, 3039 Swift Gloves, 3040 Swift Ring, 3041 Swift Charm |
| Swift Blader (`SwiftBlader`) | 4 | CooldownReducPercent +10% | 20% | 3036 Swift Blade, 3037 Swift Jacket, 3038 Swift Cap, 3039 Swift Gloves, 3040 Swift Ring, 3041 Swift Charm |
| Swift Blader (`SwiftBlader`) | 6 | CooldownReducPercent +20% | 40% | 3036 Swift Blade, 3037 Swift Jacket, 3038 Swift Cap, 3039 Swift Gloves, 3040 Swift Ring, 3041 Swift Charm |
| Temporal Sage (`TemporalSage`) | 3 | CooldownReducPercent +20% | 20% | 3042 Time Staff, 3043 Time Robe, 3044 Time Hat, 3045 Time Mittens, 3046 Time Hourglass |
| Temporal Sage (`TemporalSage`) | 5 | CooldownReducPercent +20% | 40% | 3042 Time Staff, 3043 Time Robe, 3044 Time Hat, 3045 Time Mittens, 3046 Time Hourglass |

`CooldownReducPercent` is flagged `IsPercent`, so each tier also adds the same percent to the flat `CooldownReduc` attribute (id - 1 rule).

## Fragments (4001-4020)

Socketed into equipment. MaxStack 99.

| ID | Name | Rarity | Kind | Effects when socketed |
|---|---|---|---|---|
| 4001 | Red Gem | Common | BasicFragment | - |
| 4002 | Blue Gem | Common | BasicFragment | - |
| 4003 | Green Gem | Common | BasicFragment | - |
| 4004 | Yellow Gem | Common | BasicFragment | - |
| 4005 | White Gem | Uncommon | BasicFragment | - |
| 4006 | Black Gem | Uncommon | BasicFragment | - |
| 4007 | Purple Gem | Rare | BasicFragment | - |
| 4008 | Fire Rune | Rare | Rune | FireDamage +15 [U]; grants missing skill 101 [M] |
| 4009 | Ice Rune | Rare | Rune | WaterResist +25 [U]; grants missing skill 102 [M] |
| 4010 | Wind Rune | Rare | Rune | - |
| 4011 | Earth Rune | Epic | Rune | - |
| 4012 | Light Rune | Epic | Rune | - |
| 4013 | Dark Rune | Epic | Rune | - |
| 4014 | Beast Essence | Epic | Essence | - |
| 4015 | Demon Essence | Legendary | Essence | DemonDamage +20 [U]; 8% OnAttack: cast missing skill 108 [M] [T]; if HPPercent <= 30: Defense +100 [A] |
| 4016 | Angel Essence | Legendary | Essence | - |
| 4017 | Dragon Essence | Legendary | Essence | DragonDamage +25 [U]; if MaxHP >= 5000: ATK +50; 10% OnTakeDamage: cast missing skill 202 [M] |
| 4018 | Void Essence | Legendary | Essence | GhostDamage +20 [U]; 5% OnHit: cast missing skill 110 [M]; if HPPercent <= 30: Defense +100 [A] |
| 4019 | Spirit Essence | Rare | Essence | - |
| 4020 | Nature Essence | Rare | Essence | DemonDamage +20 [U]; 8% OnAttack: cast missing skill 108 [M] [T] |

## Skill runes (6001-6017) and support runes (7001-7022)

Each rune carries one skill (`SkillID`), MaxStack 1, rarity Rare.

| Rune | Carries |
|---|---|
| 6001 Tackle Rune | active skill Tackle (1001) |
| 6002 Healing Rune | active skill Healing (1002) |
| 6003 Hellfire Blast Rune | active skill Hellfire Blast (1003) |
| 6004 Divine Blessing Rune | active skill Divine Blessing (1004) |
| 6005 Earthshatter Rune | active skill Earthshatter (1005) |
| 6006 Lightning Strike Rune | active skill Lightning Strike (1006) |
| 6007 Shadow Dash Rune | active skill Shadow Dash (1007) |
| 6008 Aura of Flames Rune | active skill Aura of Flames (1008) |
| 6009 Lightning Blessing Rune | active skill Lightning Blessing (1009) |
| 6010 Fire Pila Rune | active skill Fire Pila (1010) |
| 6011 Perfect Parry Rune | active skill Perfect Parry (1011) |
| 6012 Chain Lightning Rune | active skill Chain Lightning (1012) |
| 6013 Blink Strike Rune | active skill Blink Strike (1013) |
| 6014 Time Dilation Rune | active skill Time Dilation (1014) |
| 6015 Force Nova Rune | active skill Force Nova (1015) |
| 6016 Blink Rune | active skill Blink (1016) |
| 6017 Chornos Field Rune | active skill Chornos Field (1017) |
| 7001 Multi-Shot Rune | support Multi-Shot (1001) |
| 7002 Echo Rune | support Echo (1002) |
| 7003 Pierce Rune | support Pierce (1003) |
| 7004 Chain Rune | support Chain (1004) |
| 7005 Area Increase Rune | support Area Increase (1005) |
| 7006 Damage Up Rune | support Damage Up (1006) |
| 7007 Cooldown Reduction Rune | support Cooldown Reduction (1007) |
| 7008 Cost Reduction Rune | support Cost Reduction (1008) |
| 7009 Duration Up Rune | support Duration Up (1009) |
| 7010 Status Chance Up Rune | support Status Chance Up (1010) |
| 7011 Burn Chance Rune | support Burn Chance (1011) |
| 7012 Freeze Chance Rune | support Freeze Chance (1012) |
| 7013 Knockback Rune | support Knockback (1013) |
| 7014 Critical Power Rune | support Critical Power (1014) |
| 7015 Mana Leech Rune | support Mana Leech (1015) |
| 7016 Life Leech Rune | support Life Leech (1016) |
| 7017 Projectile Speed Rune | support Projectile Speed (1017) |
| 7018 Split Projectile Rune | support Split Projectile (1018) |
| 7019 Homing Rune | support Homing (1019) |
| 7020 Empowered Cast Rune | support Empowered Cast (1020) |
| 7021 Echo Spell Rune | support Echo Spell (1021) |
| 7022 Spell Cascade Rune | support Spell Cascade (1022) |

There is no rune for 1019 Mind Control, 1020 Fire Bolt, 3001-3003 or 0 Summon Valkyrie.

## Costumes (8001-8012)

| ID | Name | Rarity | Slot | Effects |
|---|---|---|---|---|
| 8001 | Skel Head | Rare | Head | UndeadResistPercent +2% |
| 8002 | Skel Body | Rare | Body | - |
| 8003 | Skel Arms | Rare | Arms | - |
| 8004 | Skel Legs | Rare | Legs | - |
| 8005 | Undead Head | Epic | Head | - |
| 8006 | Undead Body | Epic | Body | - |
| 8007 | Undead Arms | Epic | Arms | - |
| 8008 | Undead Legs | Epic | Legs | - |
| 8009 | Axer Head | Legendary | Head | - |
| 8010 | Axer Body | Legendary | Body | - |
| 8011 | Axer Arms | Legendary | Arms | - |
| 8012 | Axer Legs | Legendary | Legs | - |

`UndeadResistPercent` +2% only multiplies the flat `UndeadResist` (0 by default), so Skel Head has no effect on its own.

## Other item types

These have no effects in the data. `Consumable.OnUse` is still empty, so using a potion or food does nothing yet.

**Consumables** (2001-2020): 2001 Minor Health Potion (Common, stack 99), 2002 Health Potion (Common, stack 99), 2003 Greater Health Potion (Uncommon, stack 99), 2004 Superior Health Potion (Rare, stack 99), 2005 Divine Health Potion (Epic, stack 99), 2006 Apple (Common, stack 99), 2007 Bread (Common, stack 99), 2008 Meat (Common, stack 99), 2009 Fish (Common, stack 99), 2010 Bandage (Common, stack 99), 2011 First Aid Kit (Uncommon, stack 99), 2012 Herb (Common, stack 99), 2013 Mushroom (Common, stack 99), 2014 Berry (Common, stack 99), 2015 Chicken (Uncommon, stack 99), 2016 Steak (Uncommon, stack 99), 2017 Soup (Uncommon, stack 99), 2018 Elixir of Life (Legendary, stack 99), 2019 Magic Water (Common, stack 99), 2020 Miracle Drop (Legendary, stack 99).

**Currencies** (1001-1020): 1001 Gold Coin (Common, stack 9999), 1002 Silver Coin (Common, stack 9999), 1003 Copper Coin (Common, stack 9999), 1004 Diamond (Legendary, stack 9999), 1005 Ruby (Epic, stack 9999), 1006 Sapphire (Epic, stack 9999), 1007 Emerald (Epic, stack 9999), 1008 Topaz (Rare, stack 9999), 1009 Amethyst (Rare, stack 9999), 1010 Guild Token (Uncommon, stack 9999), 1011 Arena Point (Uncommon, stack 9999), 1012 Honor Badge (Epic, stack 9999), 1013 Crystal Shard (Rare, stack 9999), 1014 Void Matter (Legendary, stack 9999), 1015 Star Fragment (Epic, stack 9999), 1016 Soul Coin (Epic, stack 9999), 1017 Blood Token (Rare, stack 9999), 1018 Light Essence (Epic, stack 9999), 1019 Dark Essence (Epic, stack 9999), 1020 Ancient Coin (Legendary, stack 9999).

**Resources** (5001-5020): 5001 Wood (Common, stack 999), 5002 Hardwood (Uncommon, stack 999), 5003 Iron Ore (Common, stack 999), 5004 Gold Ore (Uncommon, stack 999), 5005 Copper Ore (Common, stack 999), 5006 Silver Ore (Uncommon, stack 999), 5007 Leather (Common, stack 999), 5008 Hard Leather (Uncommon, stack 999), 5009 Cloth (Common, stack 999), 5010 Silk (Rare, stack 999), 5011 Common Herb (Common, stack 999), 5012 Rare Herb (Rare, stack 999), 5013 Monster Bone (Common, stack 999), 5014 Monster Fang (Uncommon, stack 999), 5015 Slime Gel (Common, stack 999), 5016 Magic Dust (Epic, stack 999), 5017 Coal (Common, stack 999), 5018 Thread (Common, stack 999), 5019 Iron Ingot (Uncommon, stack 999), 5020 Gold Ingot (Rare, stack 999).

**Additional (keys, quest, loot)** (9001-9020): 9001 Old Key (Common, stack 99), 9002 Rusty Key (Common, stack 99), 9003 Golden Key (Rare, stack 99), 9004 Boss Key (Epic, stack 99), 9005 Mysterious Map (Rare, stack 1), 9006 Torn Journal Page 1 (Common, stack 1), 9007 Torn Journal Page 2 (Common, stack 1), 9008 Lore Book (Uncommon, stack 1), 9009 Empty Bottle (Common, stack 99), 9010 Broken Sword (Common, stack 99), 9011 Goblin Ear (Common, stack 999), 9012 Wolf Pelt (Common, stack 999), 9013 Bear Claw (Uncommon, stack 999), 9014 Spider Web (Common, stack 999), 9015 Bat Wing (Common, stack 999), 9016 Zombie Eye (Common, stack 999), 9017 Skeleton Bone (Common, stack 999), 9018 Magic Core (Epic, stack 99), 9019 Ancient Relic (Legendary, stack 1), 9020 Trash (Common, stack 999).
