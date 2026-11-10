# Unique Passives Table

Generated 2026-11-10 by `node Tools/DataDocs/build_data_tables.js` from `Assets/Resources/Data/UniquePassive.json`, `Assets/Script/CoreSystem/Entity/UniquePassives/*.cs`, `Assets/Resources/Data/SkillTree.json`. Do not edit by hand; change the data and run the script again.

61 unique passives. The name and description come from the JSON; the numbers are in each C# class. **Hooks** are the methods the class overrides (when it acts). **Tree nodes** is how many Skill Tree nodes grant it. The detailed page is [Unique_Passives.md](../Unique_Passives.md).

Back to [all tables](./README.md).

| ID | Name | Description | Hooks | Tree nodes |
|---|---|---|---|---|
| AlphasBurden | Alpha's Burden | You can have only one summon, but it grows larger and much stronger. | Tick | 1 |
| AnsweredPrayer | Answered Prayer | Self-healing grows stronger the lower your HP is. | ModifyOutgoingHeal | 1 |
| AnvilHeart | Anvil Heart | You cannot critically hit, but every attack deals your average critical damage. | TriggerOnHit | 1 |
| Bleeding | Bleeding | Ticking DOT dealing 1% Max HP per second. | Tick |  |
| BlessingOfMagearna | Blessing of Magearna | 40% damage absorbed by Mana. | TriggerOnTakeDamage | 1 |
| BloodBleeding | Blood Bleeding | 20% chance to apply a Bleeding DOT dealing 1% Max HP + 1% (INT + DEX) damage. | TriggerOnPostHit | 1 |
| BloodMage | Blood Mage | Use HP instead of Mana, skills deal 30% more final damage. | ModifyManaCost, TriggerOnHit | 1 |
| BoneChoir | Bone Choir | Skeletons dying in range rise as temporary allies. | OnEquip, OnUnequip | 1 |
| BurdenOfKings | Burden of Kings | ATK rises with carried weight, but movement slows with it. | TriggerOnHit, Tick, OnUnequip | 1 |
| CovenantKeeper | Covenant Keeper | All healing is greatly increased, but you can only deal Holy damage. | ModifyOutgoingHeal, TriggerOnHit | 1 |
| CurseOfKanix | Curse of Kanix | Swaps core stats: INT <-> STR, DEX <-> VIT, LUK <-> AGI. | ModifyCoreAttributes | 1 |
| DepressionChaos | Depression Chaos | Deals 200% INT magic damage on casting a non-damaging AOE skill. | TriggerOnSkillExecuted | 1 |
| DoubleAttack | Double Attack | Every basic attack strikes twice. Basic attacks only. |  | 1 |
| DoubleSpell | Double Spell | Level 1 skills cast a second time for free; the second cast deals 60% less damage. Stacks with Echo Spell. |  | 1 |
| EldritchFaith | Eldritch Faith | INT -50%, adds flat damage equal to lost INT. | ModifyCoreAttributes, TriggerOnHit | 1 |
| Execution | Execution | Instantly execute targets whose HP <= 5%. | TriggerOnPostHit | 1 |
| FeralBond | Feral Bond | Summons gain a bonus from your DEX and VIT. | Tick | 1 |
| FickleStars | Fickle Stars | LUK sets the chance for a skill to cost no MP. | ModifyManaCost, TriggerOnSkillExecuted | 1 |
| GlassCannonTheorem | Glass Cannon Theorem | The more MP you have left, the higher your M.ATK. | TriggerOnHit | 1 |
| GraveTithe | Grave Tithe | Enemies dying nearby restore a little HP. | OnEquip, OnUnequip | 1 |
| HereticsPyre | Heretic's Pyre | Enemies that die while branded spread fire to nearby foes. | OnEquip, OnUnequip, TriggerOnPostHit | 1 |
| Hexweaver | Hexweaver | Curses stack on a single target and grow with each stack. | TriggerOnHit, TriggerOnPostHit | 1 |
| HollowCrown | Hollow Crown | Maximum HP is halved, but life leech can exceed maximum HP. | OnEquip, OnUnequip, TriggerOnPostHit | 1 |
| HollowQuiver | Hollow Quiver | No automatic basic attacks, but every aimed shot hits hard and pierces. | CanBasicAttack, ModifyCast, TriggerOnHit | 1 |
| IronMomentum | Iron Momentum | Damage grows while you keep hitting the same target. Switching targets resets it. | TriggerOnHit | 1 |
| LastBanner | Last Banner | Gain a temporary shield the first time HP falls below half in a fight. | TriggerOnTakeDamage, Tick | 1 |
| LivingPhilosopher | Living Philosopher | You cannot use potions, but HP regenerates steadily with VIT. | CanUseConsumable, Tick | 1 |
| LoadedChamber | Loaded Chamber | Standing still loads a special round. Your next shot grows with the time stored. | Tick, TriggerOnHit | 1 |
| ManaLeech | Mana Leech | 30% chance to leech mana on hit. | TriggerOnPostHit | 1 |
| MarkedThroat | Marked Throat | Attacks from behind a target always critically hit. | TriggerOnHit | 1 |
| OathOfSilence | Oath of Silence | You cannot use skills with a cast time, but instant skills hit far harder. | CanCast, TriggerOnHit | 1 |
| OneCleanCut | One Clean Cut | Your first hit on a target is massive. Later hits weaken until you switch targets. | TriggerOnHit | 1 |
| OverflowingSigil | Overflowing Sigil | MP restored beyond your maximum is stored as power for your next skill. | Tick, TriggerOnHit | 1 |
| PackInstinct | Pack Instinct | The more summons you have, the faster you move and attack. | Tick, OnUnequip | 1 |
| PenitentBlade | Penitent Blade | Part of your STR counts as INT for Holy skills. | TriggerOnHit | 1 |
| PinningShot | Pinning Shot | Repeated hits briefly pin the target in place. | TriggerOnPostHit | 1 |
| PowderBurn | Powder Burn | Critical hits set the target on fire. | TriggerOnPostHit | 1 |
| QuickenedWeave | Quickened Weave | Each basic attack shortens the cast time of your next skill. | TriggerOnBasicAttack, ModifyCast, TriggerOnSkillExecuted | 1 |
| RadiantRebuke | Radiant Rebuke | Healing releases a Holy wave that damages nearby enemies. | TriggerOnHealed | 1 |
| RicochetDoctrine | Ricochet Doctrine | Shots that kill an enemy bounce to the next one. | OnEquip, OnUnequip, TriggerOnPostHit | 1 |
| Rock | Rock | Immune to pushes and impulses. |  | 1 |
| RunicSkin | Runic Skin | Gain temporary resistance to the element of your last skill. | TriggerOnSkillExecuted, Tick, OnUnequip | 1 |
| SecondDose | Second Dose | Potions have a chance not to be consumed. | ShouldKeepConsumable | 1 |
| SharedWound | Shared Wound | Your summons take part of the damage dealt to you. | TriggerOnTakeDamage | 1 |
| SingularElement | Singular Element | Choose one element. It becomes far stronger and all others are unusable. | CanCast, TriggerOnSkillExecuted, TriggerOnHit | 1 |
| SixFates | Six Fates | Attacks roll one of six outcomes, from a miss to enormous damage. | TriggerOnHit | 1 |
| SkirmishersPace | Skirmisher's Pace | Shoot while moving without slowing down, but with shorter range. | OnEquip, OnUnequip | 1 |
| SlowIncantation | Slow Incantation | Long casts grow stronger with the cast time actually spent. | TriggerOnHit | 1 |
| SpellboundEdge | Spellbound Edge | After a successful cast, your next basic attack carries that skill's element. | TriggerOnSkillExecuted, ModifyBasicAttack | 1 |
| Thorn | Thorn | Reflects 10% of damage taken scaling with STR back to the attacker. | TriggerOnTakeDamage | 1 |
| TimeCollector | Time Collector | Reduces cooldowns by 0.1s on basic attack. | TriggerOnBasicAttack | 1 |
| TransmutedBlood | Transmuted Blood | Items that restore HP also restore MP. | TriggerOnConsumableUsed | 1 |
| TwinConduit | Twin Conduit | ATK and M.ATK merge into one value, but two-handed weapons cannot be equipped. | CanEquip, TriggerOnHit | 1 |
| Vampire | Vampire | Leeches 30% physical damage as HP. | TriggerOnPostHit | 1 |
| VeilStep | Veil Step | Become briefly invisible after dodging an attack. | TriggerOnTakeDamage, Tick | 1 |
| VenomLedger | Venom Ledger | The more status effects a target has, the more damage you deal to it. | TriggerOnHit | 1 |
| Verdict | Verdict | Hits brand enemies. Your next attack detonates the brand as Holy damage. | TriggerOnPostHit | 1 |
| VolatileMixture | Volatile Mixture | Potions you use explode in a ring around you. | TriggerOnConsumableUsed | 1 |
| VowOfStillness | Vow of Stillness | Standing still grants M.DEF and faster MP recovery. | Tick, OnUnequip | 1 |
| WhisperPact | Whisper Pact | Power rises with Corruption, but maximum HP falls. | TriggerOnSkillExecuted, Tick, TriggerOnHit, OnUnequip | 1 |
| Windreader | Windreader | Damage increases with distance to the target. | TriggerOnHit | 1 |

Every JSON entry has a script.

Every script has a JSON entry.
