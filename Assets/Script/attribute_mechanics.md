# Realm of Hero: Attribute Mechanics & Class Guide

This document details the flow of attributes, calculation formulas, and class archetypes found in the game.

---

## 1. Full Attribute Table (AttributeType)
The following table lists all attributes defined in the system and their roles.

| ID | Attribute Name | Type | Description / Formula Reference |
| :--- | :--- | :--- | :--- |
| 1-4 | **HP / MaxHP** | Resource | Base HP + Flat Bonus + (VIT * 1%) |
| 5-8 | **MP / MaxMP** | Resource | Base MP + Flat Bonus + (INT * 1%) |
| 9-10 | **STR** | Core | Increases Physical ATK (1:1) and Weight Limit (1:30) |
| 11-12 | **AGI** | Core | Increases Flee (1:1), ASPD (1:0.5), and Status DEF (1:0.2) |
| 13-14 | **VIT** | Core | Increases MaxHP (1:1%), HP Recovery (1:0.2), and Status DEF (1:0.7) |
| 15-16 | **INT** | Core | Increases MATK (1:1.5), MaxMP (1:1%), MP Recovery (1:0.16), and Status DEF (1:1) |
| 17-18 | **DEX** | Core | Increases Hit (1:1), ASPD (1:0.15), and slightly ATK/MATK (5:1) |
| 19-20 | **LUK** | Core | Increases Crit Rate (1:0.3), Perfect Dodge (1:0.1), and minorly all stats |
| 21-24 | **ATK / MATK** | Combat | Primary damage stats for Physical and Magic |
| 33-34 | **Status DEF** | Defense | Soft defense derived from VIT, AGI, INT, DEX, LUK |
| 37-38 | **Hit** | Combat | Accuracy derived from DEX and LUK |
| 39-40 | **Flee** | Combat | Evasion derived from AGI and LUK |
| 41-42 | **ASPD** | Combat | Attack Speed derived from AGI and DEX |
| 43-44 | **Critical Rate** | Combat | Chance to land critical hits (LUK-based) |
| 45-46 | **Perfect Dodge** | Combat | Chance to avoid any physical hit (LUK-based) |
| 51-82 | **Elemental Stats** | Combat | Damage & Resistance for Neutral, Water, Earth, Fire, Electric, Ghost, Holy |
| 83-90 | **Size Modifiers** | Combat | Damage & Resistance for Small, Medium, Large targets |
| 91-130 | **Race Modifiers** | Combat | Damage & Resistance for 10 Races (Formless, Undead, Dragon, etc.) |
| 131-132 | **Move Speed** | Utility | Base character movement velocity |
| 133-134 | **Weight Limit** | Utility | Maximum inventory capacity (STR-based) |
| 141-148 | **Cast Time Reduc** | Utility | Reduction for Fixed (FCT) and Variable (VCT) cast times |
| 151-152 | **Cooldown Reduc** | Utility | Global and per-skill cooldown reduction |

---

## 2. Derived Stat Formulas (Summary)

### Offensive Stats
- **ATK**: `STR + (DEX / 5) + (LUK / 3)`
- **MATK**: `(INT * 1.5) + (DEX / 5) + (LUK / 3)`
- **Hit**: `DEX + (LUK / 3)`
- **Critical Rate**: `LUK * 0.3`
- **Status DMG**: `(LUK * 0.5) + (INT / 4)`

### Defensive Stats
- **Status DEF**: `(VIT * 0.7) + (AGI * 0.2) + (LUK * 0.2) + (DEX * 0.2) + INT`
- **Flee**: `AGI + (LUK / 5)`
- **Perfect Dodge**: `LUK / 10`
- **Crit Resistance**: `LUK / 5`

---

## 3. Class Archetypes (Base Nodes)
Referenced from `SkillTree.json` (role: base). These represent the 12 primary paths in the game.

| Class | Primary Stats | Role / Strategy |
| :--- | :--- | :--- |
| **Warrior** | STR, VIT | Classic tank and heavy melee attacker. High HP and Weight Limit. |
| **Mage** | INT, DEX | Pure magic damage. Focuses on VCT reduction and high MATK. |
| **Archer** | DEX, AGI | High-speed ranged attacker. Focuses on never missing (Hit) and ASPD. |
| **Assassin** | AGI, LUK | Evasive melee specialist. Relies on Flee and Critical burst damage. |
| **Sorcerer** | INT, LUK | Magic specialist focused on status effects and magical criticals. |
| **Gunner** | DEX, LUK | Precision ranged DPS. High critical rate and status damage. |
| **Inquisitor** | STR, INT | Holy/Magic hybrid warrior. High Status DEF and diverse damage types. |
| **Enchanter** | INT, AGI | Spell-blade style. Uses high ASPD to deliver magic-infused attacks. |
| **Alchemist** | VIT, INT | Supportive tank. High survival and effective use of items/buffs. |
| **Temmer** | VIT, DEX | Pet master. Needs VIT to survive and DEX to coordinate with pets. |
| **Necromancer** | INT, VIT | Dark magic user. Uses INT for MATK and VIT for sustain. |
| **Priest** | INT, VIT | Primary healer. Focuses on high MP Recovery and Max HP. |

---

## 4. Advanced Synergies & Mixing

### **The "Unbreakable Wall" (VIT + INT + STR)**
- **Classes**: Warrior, Inquisitor, Alchemist.
- **Benefit**: Combines high **Status DEF** with large **Max HP** and **Max MP**.
- **Result**: Extremely hard to kill with either physical or magical attacks, while having plenty of resources for abilities.

### **The "Blade Weaver" (AGI + DEX + INT)**
- **Classes**: Enchanter, Mage.
- **Benefit**: Maximizes **ASPD** and minimizes **VCT**.
- **Result**: Can spam spells or magical attacks at an incredible rate.

### **The "Lucky Striker" (LUK + DEX + AGI)**
- **Classes**: Assassin, Gunner, Archer.
- **Benefit**: Highest possible **Crit Rate**, **Hit**, and **Perfect Dodge**.
- **Result**: A glass cannon that rarely gets hit and always lands devastating criticals.

### **The "Status Overlord" (INT + LUK + DEX)**
- **Classes**: Sorcerer, Necromancer.
- **Benefit**: High **Status DMG**, **MATK**, and **Hit**.
- **Result**: Ensures that magical debuffs land consistently and deal maximum damage.
