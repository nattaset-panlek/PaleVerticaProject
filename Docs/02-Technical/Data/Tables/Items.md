# Items Table

Generated 2026-11-10 by `node Tools/DataDocs/build_data_tables.js` from `Assets/Resources/Data/Item.json`, `Assets/Resources/Data/ItemSet.json`, `Assets/Resources/Data/Attribute.json`. Do not edit by hand; change the data and run the script again.

202 items in 10 types and 2 sets. Effects show the attribute by its `AttributeType` name; an id that is not in `Attribute.json` shows as `#id (unknown)`. The detailed page with known problems is [Items.md](../Items.md).

Back to [all tables](./README.md).

## Overview

| Type | Count | Id range | With effects |
|---|---|---|---|
| Currency | 21 | 1000-1020 | 0 |
| Consumable | 20 | 2001-2020 | 0 |
| Equipment | 48 | 3001-3048 | 29 |
| Fragment | 20 | 4001-4020 | 6 |
| Resource | 20 | 5001-5020 | 0 |
| SkillRune | 17 | 6001-6017 | 0 |
| SupportRune | 22 | 7001-7022 | 0 |
| Costume | 12 | 8001-8012 | 1 |
| Additional | 20 | 9001-9020 | 0 |
| Progression | 2 | 10001-10002 | 0 |

## Currency (21)

| ID | Name | Rarity | Stack | Effects | Description |
|---|---|---|---|---|---|
| 1000 | Xeny Credit | Common | 999000000 |  | Standard galactic credit currency used for forging, socketing, and celestial trades. |
| 1001 | Gold Coin | Common | 9999 |  | A standard coin bearing the face of King Ailsen. Though the Aetheranil Kingdom fell aft... |
| 1002 | Silver Coin | Common | 9999 |  | A common currency traded in remote borders like Arvelle village, far from the gaze of t... |
| 1003 | Copper Coin | Common | 9999 |  | A rusted remnant of an era when humanity falsely believed they had true freedom. |
| 1004 | Diamond | Legendary | 9999 |  | A precious gem reflecting starlight, said to be used by the Ancient Gods to adorn their... |
| 1005 | Ruby | Epic | 9999 |  | A crimson stone resembling the blood spilled by the Royal Guards who stood with Alveros... |
| 1006 | Sapphire | Epic | 9999 |  | A cold, blue gem emitting a faint cosmic aura, similar to Hymeira's shifting eyes. |
| 1007 | Emerald | Epic | 9999 |  | A vibrant green stone holding the faint, corrupted life force of the mutated lands. |
| 1008 | Topaz | Rare | 9999 |  | A pale yellow gem found deep within the ruins of the shattered Aetheranil. |
| 1009 | Amethyst | Rare | 9999 |  | A purple gem that resonates softly when Serina's shadow calling is active nearby. |
| 1010 | Guild Token | Uncommon | 9999 |  | A wooden badge used by wandering scavengers who band together in the post-Severance world. |
| 1011 | Arena Point | Uncommon | 9999 |  | Tokens earned in underground blood-pits where desperate humans fight corrupted beasts. |
| 1012 | Honor Badge | Epic | 9999 |  | A faded crest once awarded to Aetheranil knights before the kingdom lost its balance. |
| 1013 | Crystal Shard | Rare | 9999 |  | A fragmented piece of the holy covenant, whispering forgotten truths. |
| 1014 | Void Matter | Legendary | 9999 |  | Tangible darkness leaked from the Cosmic Veil, often traded by Outer Gods and chaotic e... |
| 1015 | Star Fragment | Epic | 9999 |  | A piece of a fallen star, carrying the crushing pressure of The Remnant's distant gaze. |
| 1016 | Soul Coin | Epic | 9999 |  | Forged from the essence of bound spirits. Alveros used these to seal the eternal oaths ... |
| 1017 | Blood Token | Rare | 9999 |  | A crystalline drop of corrupted blood from the monstrosities born of the broken covenant. |
| 1018 | Light Essence | Epic | 9999 |  | A fleeting spark of the original peace granted by the Ancient Gods, now incredibly rare. |
| 1019 | Dark Essence | Epic | 9999 |  | Condensed cosmic malice. Mere exposure causes ordinary humans to hear maddening whispers. |
| 1020 | Ancient Coin | Legendary | 9999 |  | A relic from the First King's era, predating the fateful pact with the Ancient Gods. |

## Consumable (20)

| ID | Name | Heal HP | Heal MP | Stack | Effects | Description |
|---|---|---|---|---|---|---|
| 2001 | Minor Health Potion | 50 | 0 | 99 |  | A traditional herbal brew used by the villagers of Arvelle to mend minor wounds. |
| 2002 | Health Potion | 150 | 0 | 99 |  | A standard concoction that temporarily numbs the pain of living in a ruined world. |
| 2003 | Greater Health Potion | 400 | 0 | 99 |  | Distilled with faint traces of magic, capable of sealing deep gashes left by corrupted ... |
| 2004 | Superior Health Potion | 1000 | 0 | 99 |  | Brewed using knowledge salvaged from Aetheranil's fallen archives. |
| 2005 | Divine Health Potion | 2500 | 0 | 99 |  | Infused with the residual energy of the ancient pact, pulling the drinker back from the... |
| 2006 | Apple | 20 | 0 | 99 |  | A simple fruit grown in the untainted soil near the village of Arvelle. |
| 2007 | Bread | 40 | 0 | 99 |  | Hardtack bread, the staple diet of scavengers wandering the shattered plains. |
| 2008 | Meat | 80 | 0 | 99 |  | Roasted meat from a local beast, safe to consume if cooked thoroughly. |
| 2009 | Fish | 60 | 0 | 99 |  | Caught from a river that miraculously escaped the cosmic corruption. |
| 2010 | Bandage | 100 | 0 | 99 |  | Strips of cloth torn from old Aetheranil banners, used to stop bleeding. |
| 2011 | First Aid Kit | 300 | 0 | 99 |  | A survival kit essential for those braving the shadows of the ruins. |
| 2012 | Herb | 30 | 0 | 99 |  | A bitter leaf that slightly eases the cold feeling when shadows are summoned. |
| 2013 | Mushroom | 0 | 30 | 99 |  | A faintly glowing fungi found in areas where reality has worn thin. |
| 2014 | Berry | 15 | 0 | 99 |  | Tart wild berries foraged by Serina during her solitary travels. |
| 2015 | Chicken | 120 | 0 | 99 |  | A rare source of fresh poultry in the desolated world. |
| 2016 | Steak | 200 | 0 | 99 |  | A hearty meal that restores strength to exhausted shadow callers. |
| 2017 | Soup | 90 | 0 | 99 |  | A warm broth that provides brief comfort in this cold, unforgiving era. |
| 2018 | Elixir of Life | 99999 | 0 | 99 |  | Said to be the tears of the Ancient Gods. A legendary artifact in the era of the Pale T... |
| 2019 | Magic Water | 0 | 100 | 99 |  | Water drawn from an ancient well that still echoes with forgotten covenants. |
| 2020 | Miracle Drop | 99999 | 99999 | 99 |  | A pure droplet of primordial creation magic, defying the cosmic decay. |

## Equipment (48)

| ID | Name | Slot | Rarity | Phys | Magic | Sockets | Set | Effects | Description |
|---|---|---|---|---|---|---|---|---|---|
| 3001 | Peasant Hat | Helm | Common | 5 | 1 | 0 |  |  | A woven hat worn by the simple folk of Arvelle, shielding them from the harsh sun, but ... |
| 3002 | Peasant Shirt | Armor | Common | 10 | 2 | 0 |  |  | Rough cotton clothing, often stained with the ashes of a fallen civilization. |
| 3003 | Leather Helm | Helm | Uncommon | 12 | 5 | 1 |  |  | Crafted from beasts hunted in the borderlands, offering meager protection against Outer... |
| 3004 | Leather Armor | Armor | Uncommon | 20 | 8 | 1 |  |  | A scavenger's essential gear, allowing stealthy movement through the ruined capital. |
| 3005 | Iron Sword | WeaponOneHandLeft | Uncommon | 35 | 5 | 1 |  |  | A standard blade of the Aetheranil guards. Though rusted, it still remembers the weight... |
| 3006 | Iron Shield | AccessoryLeft | Uncommon | 25 | 10 | 1 |  |  | A heavy shield once bearing the King's crest, now defaced by those who realize the cove... |
| 3007 | Steel Axe | WeaponTwoHand | Rare | 65 | 15 | 2 |  |  | Forged for executioners of the old regime, re-purposed to sever the limbs of cosmic hor... |
| 3008 | Steel Gauntlets | Glove | Rare | 15 | 5 | 1 |  | +25 WaterResist; SkillUnlockEffect skill 102 | Cold to the touch. The metal resonates slightly when Serina channels the shadows. |
| 3009 | Mithril Chainmail | Armor | Rare | 35 | 15 | 2 |  |  | Armor worn by the Royal Guard before Alveros bound his soul to the shattered covenant. |
| 3010 | Mithril Boots | AccessoryRight | Rare | 10 | 10 | 1 |  | +15 MoveSpeed; +10 ASPD | Lightweight greaves that allowed knights to swiftly patrol the grand halls of Aetheranil. |
| 3011 | Adamantite Helm | Helm | Epic | 45 | 20 | 2 |  |  | An unbreakable helm that blocked the maddening whispers of the Outer Gods during the in... |
| 3012 | Adamantite Mace | WeaponOneHandLeft | Epic | 95 | 35 | 3 |  |  | A heavy weapon used to shatter statues of the Ancient Gods during King Ailsen's rebellion. |
| 3013 | Dragon Scale Armor | Armor | Legendary | 80 | 40 | 3 |  | +25 DragonDamage; BonusCondition ATK +50; AutoCastEffect skill 202 (OnTakeDamage 10%) | Forged from a beast that existed before the gods, representing a power untethered from ... |
| 3014 | Dragon Tooth Pendant | Talisman | Legendary | 5 | 20 | 1 |  | +25 DragonDamage; BonusCondition ATK +50; AutoCastEffect skill 202 (OnTakeDamage 10%) | A sharp relic of ancient defiance, amplifying the destructive will of its wearer. |
| 3015 | Demon Blade | WeaponOneHandLeft | Legendary | 150 | 70 | 4 |  | +20 DemonDamage; AutoCastEffect skill 108 (OnAttack 8%); BonusCondition Defense +100 | Corrupted by the whispers of The Remnant. It grants immense power, but slowly devours t... |
| 3016 | Demon Horn | Helm | Legendary | 30 | 65 | 2 |  | +20 DemonDamage; AutoCastEffect skill 108 (OnAttack 8%); BonusCondition Defense +100 | A crystallized manifestation of cosmic malice, radiating an aura that distorts reality ... |
| 3017 | Angelic Halo | Helm | Legendary | 25 | 75 | 2 |  |  | A false blessing from the Ancient Gods. It brings peace, but at the cost of absolute su... |
| 3018 | Angelic Wings | Armor | Legendary | 60 | 85 | 3 |  |  | Feathers of blinding light. They shield the wearer from chaos, but blind them to the tr... |
| 3019 | Void Robe | Armor | Epic | 35 | 95 | 3 |  | +20 GhostDamage; AutoCastEffect skill 110 (OnHit 5%); BonusCondition Defense +100 | Woven from the cosmic darkness that invaded after The Severance. It absorbs all light a... |
| 3020 | Void Gloves | Glove | Epic | 15 | 40 | 2 |  | +20 GhostDamage; AutoCastEffect skill 110 (OnHit 5%); BonusCondition Defense +100 | Hands wrapped in these gloves leave no trace, moving like shadows bound to the Pale Thr... |
| 3021 | Crystal Staff | WeaponTwoHand | Rare | 1 | 110 | 3 |  |  | A catalyst that resonates with the scattered fragments of the holy covenant. |
| 3022 | Crystal Ring | AccessoryLeft | Rare | 5 | 25 | 1 |  |  | Glows with swirling nebula-like colors when chaotic entities like Hymeira are near. |
| 3023 | Shadow Cowl | Helm | Epic | 30 | 50 | 2 |  |  | Conceals the wearer's identity, a favored garment for those who speak with ancient spir... |
| 3024 | Shadow Dagger | WeaponOneHandLeft | Epic | 85 | 45 | 2 |  |  | A blade that strikes from the blind spots of the gods. Perfect for Serina's quiet rebel... |
| 3025 | Fire Wand | WeaponOneHandLeft | Uncommon | 15 | 70 | 1 |  | +15 FireDamage; SkillUnlockEffect skill 101 | Channels the furious flames of the fallen kingdom, burning both enemies and the wielder... |
| 3026 | Fire Amulet | Talisman | Uncommon | 2 | 20 | 0 |  | +15 FireDamage; SkillUnlockEffect skill 1008 | Contains an ember from the First King's pyre. Its warmth is unnatural. |
| 3027 | Ice Crown | Helm | Epic | 25 | 60 | 2 |  | +25 WaterResist; SkillUnlockEffect skill 1002; BonusCondition Defense +100 | A frozen coronet that numbs the mind to the overwhelming psychic pressure of The Remnant. |
| 3028 | Ice Orb | AccessoryRight | Epic | 5 | 30 | 2 |  | +25 WaterResist; SkillUnlockEffect skill 1004; BonusCondition Defense +100 | An accessory that drastically lowers the surrounding temperature, much like Alveros's s... |
| 3029 | Lightning Spear | WeaponTwoHand | Legendary | 140 | 85 | 3 |  |  | A weapon of divine retribution, stolen from the armories of the imprisoned deities. |
| 3030 | Lightning Pendant | Accessory | Legendary | 20 | 35 | 2 |  |  | Footwear that sparks with raw chaotic energy, allowing movement between dimensions. |
| 3031 | Overpower Talisman | Talisman | Legendary | 5 | 5 | 2 |  |  | A legendary charm that overflows with life energy, granting unstoppable recovery. |
| 3032 | Sage's Ring | Accessory | Epic | 0 | 15 | 1 |  | +10 INT; ScalingBonusEffect MPRecovery +50 | A ring that hums with intellectual power. Every 10 INT increases MP Recovery by 50. |
| 3033 | Vibrant Belt | Glove | Rare | 15 | 0 | 1 |  | +5 VIT; ScalingBonusEffect MaxHP +1 | A belt woven from life-threads. Every 2 Max MP increases Max HP by 1. |
| 3034 | Chrono Pendant | AccessoryRight | Legendary | 0 | 20 | 1 |  | +5 CooldownReduc; ScalingBonusEffect VariableCastTimeReducPercent +10; SkillUnlockEffect skill 1014 | An artifact that warps time. Every 50 (DEX + INT) reduces variable cast time by 10%. |
| 3035 | Hero's Medallion | Talisman | Legendary | 10 | 10 | 2 |  | ScalingBonusEffect ATK +5; ScalingBonusEffect MATK +5; BonusCondition MaxHP +1000 | A medal for the destined one. Every 50 Levels grants +5 to all core stats. |
| 3036 | Swift Blade | WeaponOneHandRight | Epic | 15 | 5 | 2 | Swift Blader | +15 MoveSpeed; +10 ASPD; BonusCondition Defense +100 | A lightweight rapier that feels like an extension of your arm. |
| 3037 | Swift Jacket | Armor | Epic | 10 | 0 | 1 | Swift Blader | +15 MoveSpeed; +10 ASPD; BonusCondition Defense +100 | A sleek leather jacket designed for maximum mobility. |
| 3038 | Swift Cap | Helm | Epic | 5 | 0 | 1 | Swift Blader | +15 MoveSpeed; +10 ASPD; BonusCondition Defense +100 | A comfortable cap that doesn't obstruct vision. |
| 3039 | Swift Gloves | Glove | Epic | 5 | 0 | 1 | Swift Blader | +15 MoveSpeed; +10 ASPD; BonusCondition Defense +100 | Gloves that improve grip and finger dexterity. |
| 3040 | Swift Ring | Accessory | Epic | 0 | 5 | 0 | Swift Blader | +15 MoveSpeed; +10 ASPD; BonusCondition Defense +100 | A magic ring that quickens the wearer's pulse. |
| 3041 | Swift Charm | Talisman | Epic | 0 | 5 | 0 | Swift Blader | +15 MoveSpeed; +10 ASPD; BonusCondition Defense +100 | A talisman that seems to vibrate with kinetic energy. |
| 3042 | Time Staff | WeaponTwoHand | Legendary | 5 | 25 | 3 | Temporal Sage | +10 CooldownReduc; +1.5 VariableCastTimeReduc; BonusCondition Defense +100; SkillUnlockEffect skill 1015 (OnTakeDamage 10%); RefineBonusEffect AOE +150 | A heavy staff topped with a ticking clock face. |
| 3043 | Time Robe | Armor | Legendary | 0 | 15 | 2 | Temporal Sage | +10 CooldownReduc; +1.5 VariableCastTimeReduc; BonusCondition Defense +100; AutoCastEffect skill 202 (OnTakeDamage 10%) | Robes that seem to exist in multiple time periods at once. |
| 3044 | Time Hat | Helm | Legendary | 0 | 10 | 1 | Temporal Sage | +10 CooldownReduc; +1.5 VariableCastTimeReduc; BonusCondition Defense +100; AutoCastEffect skill 202 (OnTakeDamage 10%) | A pointed hat that distorts the flow of time around the wearer. |
| 3045 | Time Mittens | Glove | Legendary | 0 | 5 | 1 | Temporal Sage | +10 CooldownReduc; +1.5 VariableCastTimeReduc; BonusCondition Defense +100; AutoCastEffect skill 202 (OnTakeDamage 10%) | Mittens that feel warm regardless of the external time. |
| 3046 | Time Hourglass | Accessory | Legendary | 0 | 5 | 0 | Temporal Sage | +10 CooldownReduc; +1.5 VariableCastTimeReduc; BonusCondition Defense +100; AutoCastEffect skill 1001 (OnTakeDamage 10%) | A small hourglass accessory that flows backwards. |
| 3047 | Swiftwind Bracers | Glove | Legendary | 0 | 0 | 1 |  | +40 ASPD | Legendary bracers woven from distilled elemental wind. Grants a flat +40 Attack Speed, ... |
| 3048 | Vampiric Soul-Reaver | WeaponTwoHand | Legendary | 85 | 0 | 3 |  | UniquePassiveEffect | A cursed legendary scythe that drinks the blood of those it slays, replenishing the wie... |

## Fragment (20)

| ID | Name | Kind | Rarity | Effects | Description |
|---|---|---|---|---|---|
| 4001 | Red Gem | BasicFragment | Common |  | A crimson fragment echoing with the bloodlines of the old world. |
| 4002 | Blue Gem | BasicFragment | Common |  | A deep blue fragment reflecting the cosmic sea beyond the veil. |
| 4003 | Green Gem | BasicFragment | Common |  | A shard pulsing with the distorted vitality of the mutated lands. |
| 4004 | Yellow Gem | BasicFragment | Common |  | A bright fragment that seems to accelerate time around it slightly. |
| 4005 | White Gem | BasicFragment | Uncommon |  | A pure shard containing a terrifying, absolute order demanded by the ancient pact. |
| 4006 | Black Gem | BasicFragment | Uncommon |  | A fragment of pure void, singing with the chaotic whispers of The Remnant. |
| 4007 | Purple Gem | BasicFragment | Rare |  | A shimmering stone that alters fate, reminiscent of Hymeira's reality glitches. |
| 4008 | Fire Rune | Rune | Rare | +15 FireDamage; SkillUnlockEffect skill 101 | An ancient glyph burned into stone, holding the wrath of the severed kingdom. |
| 4009 | Ice Rune | Rune | Rare | +25 WaterResist; SkillUnlockEffect skill 102 | A freezing sigil that mimics the chilling aura of Alveros's bound soul. |
| 4010 | Wind Rune | Rune | Rare |  | A rune of liberation, born from the desire to break the chains of the Pale Throne. |
| 4011 | Earth Rune | Rune | Epic |  | A sturdy mark representing the foundational magic of Aetheranil before its collapse. |
| 4012 | Light Rune | Rune | Epic |  | A glowing symbol of the original covenant. Its light is blinding and absolute. |
| 4013 | Dark Rune | Rune | Epic |  | A cursed glyph drawn by the Outer Gods, twisting the mind of the weak. |
| 4014 | Beast Essence | Essence | Epic |  | The feral will of a creature that survived the dimensional tears of The Severance. |
| 4015 | Demon Essence | Essence | Legendary | +20 DemonDamage; AutoCastEffect skill 108 (OnAttack 8%); BonusCondition Defense +100 | A crystallized shard of an entity from beyond the stars, fundamentally opposed to human... |
| 4016 | Angel Essence | Essence | Legendary |  | A concentrated drop of the Ancient Gods' grace—a beautiful, terrifying cage. |
| 4017 | Dragon Essence | Essence | Legendary | +25 DragonDamage; BonusCondition ATK +50; AutoCastEffect skill 202 (OnTakeDamage 10%) | The indomitable soul of an elder wyrm, refusing to bow to any throne, pale or golden. |
| 4018 | Void Essence | Essence | Legendary | +20 GhostDamage; AutoCastEffect skill 110 (OnHit 5%); BonusCondition Defense +100 | The absolute nothingness from which The Remnant plots its cosmic subjugation. |
| 4019 | Spirit Essence | Essence | Rare |  | A whisper from an unbound shadow, guiding Serina across the ruined civilization. |
| 4020 | Nature Essence | Essence | Rare | +20 DemonDamage; AutoCastEffect skill 108 (OnAttack 8%) | The raw, uncorrupted energy of the earth, a reminder of the world before the gods inter... |

## Resource (20)

| ID | Name | Rarity | Stack | Effects | Description |
|---|---|---|---|---|---|
| 5001 | Wood | Common | 999 |  | Harvested from the withered forests near Arvelle, essential for survival fires. |
| 5002 | Hardwood | Uncommon | 999 |  | Petrified timber found near the cosmic rifts, incredibly durable. |
| 5003 | Iron Ore | Common | 999 |  | Mined from the collapsed veins beneath the ruins of the capital. |
| 5004 | Gold Ore | Uncommon | 999 |  | Glimmering rock that once built the statues of the Ancient Gods. |
| 5005 | Copper Ore | Common | 999 |  | Basic ore used by the peasants who lived outside the covenant's blessing. |
| 5006 | Silver Ore | Uncommon | 999 |  | Pure metal capable of conducting faint spiritual energy. |
| 5007 | Leather | Common | 999 |  | Hide stripped from the mutated fauna wandering the desolation. |
| 5008 | Hard Leather | Uncommon | 999 |  | Thick hide resistant to both physical blows and minor reality distortions. |
| 5009 | Cloth | Common | 999 |  | Scraps torn from the banners and tapestries of Aetheranil. |
| 5010 | Silk | Rare | 999 |  | Fine fabric once worn by King Ailsen's court, untouched by time. |
| 5011 | Common Herb | Common | 999 |  | A staple in the apothecary of Arvelle village. |
| 5012 | Rare Herb | Rare | 999 |  | A strange plant blooming only under the aurora of the Cosmic Veil. |
| 5013 | Monster Bone | Common | 999 |  | Calcified remains of abominations born after The Severance. |
| 5014 | Monster Fang | Uncommon | 999 |  | A jagged tooth dripping with chaotic residue. |
| 5015 | Slime Gel | Common | 999 |  | A puddle of liquefied ambient magic, common in forgotten ruins. |
| 5016 | Magic Dust | Epic | 999 |  | Pulverized fragments of the original divine covenant. |
| 5017 | Coal | Common | 999 |  | Burnt remnants of the great library of Aetheranil. |
| 5018 | Thread | Common | 999 |  | Strong twine used by survivors to stitch together their broken lives. |
| 5019 | Iron Ingot | Uncommon | 999 |  | Purified iron, ready to be forged into weapons against the shadows. |
| 5020 | Gold Ingot | Rare | 999 |  | A heavy bar of gold, useless for eating but prized for enchanting. |

## SkillRune (17)

| ID | Name | Grants skill | Rarity | Description |
|---|---|---|---|---|
| 6001 | Tackle Rune | 1001 Tackle | Rare | Grants the active skill: Tackle |
| 6002 | Healing Rune | 1002 Healing | Rare | Grants the active skill: Healing |
| 6003 | Hellfire Blast Rune | 1003 Hellfire Blast | Rare | Grants the active skill: Hellfire Blast |
| 6004 | Divine Blessing Rune | 1004 Divine Blessing | Rare | Grants the active skill: Divine Blessing |
| 6005 | Earthshatter Rune | 1005 Earthshatter | Rare | Grants the active skill: Earthshatter |
| 6006 | Lightning Strike Rune | 1006 Lightning Strike | Rare | Grants the active skill: Lightning Strike |
| 6007 | Shadow Dash Rune | 1007 Shadow Dash | Rare | Grants the active skill: Shadow Dash |
| 6008 | Aura of Flames Rune | 1008 Aura of Flames | Rare | Grants the active skill: Aura of Flames |
| 6009 | Lightning Blessing Rune | 1009 Lightning Blessing | Rare | Grants the active skill: Lightning Blessing |
| 6010 | Fire Pila Rune | 1010 Fire Pila | Rare | Grants the active skill: Fire Pila |
| 6011 | Perfect Parry Rune | 1011 Perfect Parry | Rare | Grants the active skill: Perfect Parry |
| 6012 | Chain Lightning Rune | 1012 Chain Lightning | Rare | Grants the active skill: Chain Lightning |
| 6013 | Blink Strike Rune | 1013 Blink Strike | Rare | Grants the active skill: Blink Strike |
| 6014 | Time Dilation Rune | 1014 Time Dilation | Rare | Grants the active skill: Time Dilation |
| 6015 | Force Nova Rune | 1015 Force Nova | Rare | Grants the active skill: Force Nova |
| 6016 | Blink Rune | 1016 Blink | Rare | Grants the active skill: Blink |
| 6017 | Chornos Field Rune | 1017 Chornos Field | Rare | Grants the active skill: Chornos Field |

## SupportRune (22)

| ID | Name | Support | Rarity | Description |
|---|---|---|---|---|
| 7001 | Multi-Shot Rune | 1001 Multi-Shot | Rare | Adds {0} projectile(s). |
| 7002 | Echo Rune | 1002 Echo | Rare | Converts damage to {0}% and adds {1}x cooldown as extra damage. |
| 7003 | Pierce Rune | 1003 Pierce | Rare | Allows projectiles to pierce targets. |
| 7004 | Chain Rune | 1004 Chain | Rare | Allows projectiles to chain between targets. |
| 7005 | Area Increase Rune | 1005 Area Increase | Rare | Increases area of effect radius by {0}% but increases cast time by {1}%. |
| 7006 | Damage Up Rune | 1006 Damage Up | Rare | Increases base damage by {0}%. |
| 7007 | Cooldown Reduction Rune | 1007 Cooldown Reduction | Rare | Reduces cooldown by {0}%. |
| 7008 | Cost Reduction Rune | 1008 Cost Reduction | Rare | Reduces mana cost by {0}%. |
| 7009 | Duration Up Rune | 1009 Duration Up | Rare | Increases skill duration by {0}%. |
| 7010 | Status Chance Up Rune | 1010 Status Chance Up | Rare | Increases chance to apply status effects by {0}%. |
| 7011 | Burn Chance Rune | 1011 Burn Chance | Rare | Adds a {0}% chance to ignite/burn targets. |
| 7012 | Freeze Chance Rune | 1012 Freeze Chance | Rare | Adds a {0}% chance to freeze targets. |
| 7013 | Knockback Rune | 1013 Knockback | Rare | Knocks targets back by {0} meters. |
| 7014 | Critical Power Rune | 1014 Critical Power | Rare | Multiplies base damage by {0}% plus {1}% of character LUK attribute value. |
| 7015 | Mana Leech Rune | 1015 Mana Leech | Rare | Leeches {0}% damage dealt as mana. |
| 7016 | Life Leech Rune | 1016 Life Leech | Rare | Leeches {0}% physical damage dealt as life. |
| 7017 | Projectile Speed Rune | 1017 Projectile Speed | Rare | Increases projectile speed by {0}%. |
| 7018 | Split Projectile Rune | 1018 Split Projectile | Rare | Fires {0} additional projectiles. |
| 7019 | Homing Rune | 1019 Homing | Rare | Projectiles track and homing towards targets. |
| 7020 | Empowered Cast Rune | 1020 Empowered Cast | Rare | Adds {0} flat damage, plus {1}% of character INT attribute value. |
| 7021 | Echo Spell Rune | 1021 Echo Spell | Rare | Repeats the skill 0.15s after casting. Repeated cast has {0}% increased area but {1}% l... |
| 7022 | Spell Cascade Rune | 1022 Spell Cascade | Rare | Creates 3 cascading AOE circles. Left/right circles deal {0}% damage, main circle deals... |

## Costume (12)

| ID | Name | Slot | Rarity | Effects | Description |
|---|---|---|---|---|---|
| 8001 | Skel Head | Head | Rare | +2 UndeadResistPercent | A bleached skull from a fallen warrior, now a hollow vessel for cosmetic style. |
| 8002 | Skel Body | Body | Rare |  | Bony ribcage armor that rattles with every step. |
| 8003 | Skel Arms | Arms | Rare |  | Skeletal arm guards. Does not include flesh. |
| 8004 | Skel Legs | Legs | Rare |  | Bleached bone leggings. Light and airy. |
| 8005 | Undead Head | Head | Epic |  | A mask resembling the pale, sunken face of the walking dead. |
| 8006 | Undead Body | Body | Epic |  | Tattered burial robes that still carry the scent of old graves. |
| 8007 | Undead Arms | Arms | Epic |  | Bandaged limbs that mimic the appearance of a mummy. |
| 8008 | Undead Legs | Legs | Epic |  | Tattered leggings draped in old bandages. |
| 8009 | Axer Head | Head | Legendary |  | An executioner's hood, dark and imposing. |
| 8010 | Axer Body | Body | Legendary |  | Heavy leather vestment of a professional Axer. |
| 8011 | Axer Arms | Arms | Legendary |  | Bracers designed for heavy swinging. |
| 8012 | Axer Legs | Legs | Legendary |  | Sturdy boots that stand firm against the spray of gore. |

## Additional (20)

| ID | Name | Rarity | Stack | Effects | Description |
|---|---|---|---|---|---|
| 9001 | Old Key | Common | 99 |  | An iron key that unlocks forgotten cellars in the ruined capital. |
| 9002 | Rusty Key | Common | 99 |  | A brittle key meant for long-abandoned scavenger caches. |
| 9003 | Golden Key | Rare | 99 |  | An ornate key bearing the royal seal, granting access to restricted vaults. |
| 9004 | Boss Key | Epic | 99 |  | A heavy, pulsating key required to breach the chambers of high-tier Outer God vessels. |
| 9005 | Mysterious Map | Rare | 1 |  | A parchment charting the locations of scattered covenant fragments. |
| 9006 | Torn Journal Page 1 | Common | 1 |  | '...King Ailsen has gone mad. He speaks of breaking the holy pact. We are doomed.' |
| 9007 | Torn Journal Page 2 | Common | 1 |  | 'The sky tore open today. Things from the dark poured in. Alveros stood his ground... b... |
| 9008 | Lore Book | Uncommon | 1 |  | A thick tome documenting the history of Aetheranil before the tragedy of The Severance. |
| 9009 | Empty Bottle | Common | 99 |  | A glass vial, useful for collecting essence or pure water. |
| 9010 | Broken Sword | Common | 99 |  | A shattered blade, useless in combat but holding sentimental value to old soldiers. |
| 9011 | Goblin Ear | Common | 999 |  | Proof of a kill against the lesser mutants roaming the outskirts of Arvelle. |
| 9012 | Wolf Pelt | Common | 999 |  | A thick fur taken from a beast driven mad by the cosmic taint. |
| 9013 | Bear Claw | Uncommon | 999 |  | A massive claw from an apex predator warped by the void. |
| 9014 | Spider Web | Common | 999 |  | Sticky silk from arachnids that grew to monstrous sizes after the kingdom fell. |
| 9015 | Bat Wing | Common | 999 |  | A leathery wing from creatures that thrive in the eternal shadows. |
| 9016 | Zombie Eye | Common | 999 |  | A milky eye from a corpse reanimated by the residual magic of the broken covenant. |
| 9017 | Skeleton Bone | Common | 999 |  | A brittle bone belonging to a fallen Aetheranil citizen. |
| 9018 | Magic Core | Epic | 99 |  | The beating heart of a magical construct left behind by the Ancient Gods. |
| 9019 | Ancient Relic | Legendary | 1 |  | A mysterious artifact resonating with the overwhelming presence of The Remnant. |
| 9020 | Trash | Common | 999 |  | Useless debris. A grim reminder of the once-beautiful world that was destroyed. |

## Progression (2)

| ID | Name | Kind | Stack | Description |
|---|---|---|---|---|
| 10001 | Base EXP | BaseExp | 999999999 | Experience points contributing towards Base Level progression. |
| 10002 | Job EXP | JobExp | 999999999 | Experience points contributing towards Job Level progression. |

## Item sets

| Set ID | Name | Bonuses | Pieces |
|---|---|---|---|
| SwiftBlader | Swift Blader | 2 pieces: +10 CooldownReducPercent; 4 pieces: +10 CooldownReducPercent; 6 pieces: +20 CooldownReducPercent | Swift Blade, Swift Jacket, Swift Cap, Swift Gloves, Swift Ring, Swift Charm |
| TemporalSage | Temporal Sage | 3 pieces: +20 CooldownReducPercent; 5 pieces: +20 CooldownReducPercent | Time Staff, Time Robe, Time Hat, Time Mittens, Time Hourglass |
