# Skill Icon Prompts (Full)

Prompt ที่ประกอบเสร็จแล้ว 6 สไตล์ กดปุ่ม copy ของกล่องแล้ววางใน AI ได้เลย

- แนบภาพอ้างอิง 3 ภาพจาก [`Skill_Icon_Reference/`](./Skill_Icon_Reference/) ไปด้วย (ถ้าแนบได้ภาพเดียว ใช้ `PV_Ref_A_IconGlyphs.png`)
- ตั้งภาพเป็น 1:1 ขนาด 2048 px ขึ้นไป
- ตาราง index, วิธีตัด และวิธีแก้ทีละช่อง อยู่ใน [Skill Icon Generation Walkthrough](./Skill_Icon_Generation_Walkthrough.md)

---

## Style 1: Ink Glyph (Dark UI)

```text
STYLE: flat vector glyph icons that match a minimal occult game UI. Each frame is a near-black rounded square (#141313) with a 2px light-grey outline and a thin inner bevel line, and very faint topographic contour lines in the frame background (about 8% opacity). The glyph is bold, flat, solid off-white (#F2F2F2): no gradients, no shading, no texture, crisp geometric shapes, clean edges ready for vector tracing. Only the element part of the glyph uses its accent colour. Quiet, mature, high contrast.

REFERENCE: three reference images are attached. Match their glyph language exactly: bold flat shapes, uniform thick strokes with fully rounded caps and corners, simple geometric construction (diamonds, four-point stars, arcs, rings), no inner detail lines, one solid colour per glyph, the glyph filling about half of its frame. Image A is the game's existing icon set, image B shows the hotbar skill slot and skill-tree node frames the icons sit in, image C shows icons inside the diamond skill node. Do not copy these exact symbols or any numbers from the references; design new symbols for the skills below in the same visual language.

A single square sprite sheet of 24 skill icons for "Pale Vertica", a mature dark-fantasy 2D action RPG.

LAYOUT: one 2048x2048 image, a strict grid of exactly 5 rows and 5 columns (25 equal square cells) with even thin gutters. Read left-to-right, top-to-bottom. Every cell holds one icon centred inside the same rounded-square frame (corner radius about 10% of the cell). In the top-left corner of each cell put a small index tag with a two-digit number, 01 to 25, in a clean sans-serif. Cell 25 is an empty frame with only its index tag. No skill names, no other text, no watermark, no signature.

READABILITY: every icon must still read at 48x48 px. One bold central silhouette per icon, strong figure-ground contrast, no tiny details, identical line weight, identical lighting direction (top-left) and identical icon scale in every cell.

COLOUR RULE: the sheet is greyscale except for one element accent colour per icon, used only on the magical or elemental part. Accents: Fire #FF7A1A, Holy #F2D27A, Electric #5FE3FF, Earth #C8913A, Ghost #9A86C8, Time #A9C4FF, Mind #C04BFF, Wind #7FE0C8, Petal #F4A6C0. Skills marked "none" use no accent.

CELLS (index - skill - icon subject - accent):
01 Tackle - a shoulder charging forward with speed lines and an impact burst - none
02 Healing - an open palm cradling a small glowing cross of light, rising sparks - Holy
03 Hellfire Blast - a wide cone of fire erupting forward - Fire
04 Divine Blessing - a radiant halo ring with three feathers rising above it - Holy
05 Earthshatter - a fist striking the ground, cracks spreading in a cone, flying rocks - Earth
06 Lightning Strike - one straight lightning beam piercing across the icon - Electric
07 Shadow Dash - a dark silhouette dashing with ghostly afterimages behind it - Ghost
08 Aura of Flames - a ring of flames encircling a standing figure - Fire
09 Lightning Blessing - an upright sword wrapped in crackling sparks - Electric
10 Fire Pila - a tall pillar of fire bursting from a magic circle on the ground - Fire
11 Summon Valkyrie - a winged helmet above a spear, summoned from light - none
12 Perfect Parry - two crossed blades with a bright flash at the contact point - none
13 Chain Lightning - a lightning bolt jumping between three node points - Electric
14 Blink Strike - a dagger thrusting out of a torn teleport rift - none
15 Time Dilation - an hourglass inside a slowing spiral clock ring - Time
16 Force Nova - concentric shockwave rings bursting outward from a centre point - none
17 Blink - a figure dissolving into sparks and reappearing on the other side, dotted arc between - Electric
18 Chornos Field - a large astrolabe clock face forming a dome over the ground - Time
19 Mind Control - an eye with puppet strings hanging from it - Mind
20 Wind Slash - a sharp curved blade of wind with motion streaks - Wind
21 Crescent Sweep - a sword arc shaped like a crescent moon - none
22 Cherry Blossom Execution - a katana slash circled by falling cherry petals - Petal
23 Fire Bolt - a fireball streaking diagonally upward-right with a short trail - Fire
24 Sprint - a boot with wind speed lines and a forward chevron arrow - none
25 (empty frame)
```

---

## Style 2: Paper Glyph (Light UI)

```text
STYLE: flat vector glyph icons for a light paper-toned game UI. Each frame is a paper-white rounded square (#F2F2F2) with a 2px near-black outline (#141313) and white topographic contour lines over a slightly darker band (#E6E6E6) at the top of the frame. The glyph is bold, flat, solid near-black ink (#141313), like a crisp ink stamp: no gradients, no shading, clean edges ready for vector tracing. Only the element part of the glyph uses its accent colour.

REFERENCE: three reference images are attached. Match their glyph language exactly: bold flat shapes, uniform thick strokes with fully rounded caps and corners, simple geometric construction (diamonds, four-point stars, arcs, rings), no inner detail lines, one solid colour per glyph, the glyph filling about half of its frame. Image A is the game's existing icon set, image B shows the hotbar skill slot and skill-tree node frames the icons sit in, image C shows icons inside the diamond skill node. Do not copy these exact symbols or any numbers from the references; design new symbols for the skills below in the same visual language.

A single square sprite sheet of 24 skill icons for "Pale Vertica", a mature dark-fantasy 2D action RPG.

LAYOUT: one 2048x2048 image, a strict grid of exactly 5 rows and 5 columns (25 equal square cells) with even thin gutters. Read left-to-right, top-to-bottom. Every cell holds one icon centred inside the same rounded-square frame (corner radius about 10% of the cell). In the top-left corner of each cell put a small index tag with a two-digit number, 01 to 25, in a clean sans-serif. Cell 25 is an empty frame with only its index tag. No skill names, no other text, no watermark, no signature.

READABILITY: every icon must still read at 48x48 px. One bold central silhouette per icon, strong figure-ground contrast, no tiny details, identical line weight, identical lighting direction (top-left) and identical icon scale in every cell.

COLOUR RULE: the sheet is greyscale except for one element accent colour per icon, used only on the magical or elemental part. Accents: Fire #FF7A1A, Holy #F2D27A, Electric #5FE3FF, Earth #C8913A, Ghost #9A86C8, Time #A9C4FF, Mind #C04BFF, Wind #7FE0C8, Petal #F4A6C0. Skills marked "none" use no accent.

CELLS (index - skill - icon subject - accent):
01 Tackle - a shoulder charging forward with speed lines and an impact burst - none
02 Healing - an open palm cradling a small glowing cross of light, rising sparks - Holy
03 Hellfire Blast - a wide cone of fire erupting forward - Fire
04 Divine Blessing - a radiant halo ring with three feathers rising above it - Holy
05 Earthshatter - a fist striking the ground, cracks spreading in a cone, flying rocks - Earth
06 Lightning Strike - one straight lightning beam piercing across the icon - Electric
07 Shadow Dash - a dark silhouette dashing with ghostly afterimages behind it - Ghost
08 Aura of Flames - a ring of flames encircling a standing figure - Fire
09 Lightning Blessing - an upright sword wrapped in crackling sparks - Electric
10 Fire Pila - a tall pillar of fire bursting from a magic circle on the ground - Fire
11 Summon Valkyrie - a winged helmet above a spear, summoned from light - none
12 Perfect Parry - two crossed blades with a bright flash at the contact point - none
13 Chain Lightning - a lightning bolt jumping between three node points - Electric
14 Blink Strike - a dagger thrusting out of a torn teleport rift - none
15 Time Dilation - an hourglass inside a slowing spiral clock ring - Time
16 Force Nova - concentric shockwave rings bursting outward from a centre point - none
17 Blink - a figure dissolving into sparks and reappearing on the other side, dotted arc between - Electric
18 Chornos Field - a large astrolabe clock face forming a dome over the ground - Time
19 Mind Control - an eye with puppet strings hanging from it - Mind
20 Wind Slash - a sharp curved blade of wind with motion streaks - Wind
21 Crescent Sweep - a sword arc shaped like a crescent moon - none
22 Cherry Blossom Execution - a katana slash circled by falling cherry petals - Petal
23 Fire Bolt - a fireball streaking diagonally upward-right with a short trail - Fire
24 Sprint - a boot with wind speed lines and a forward chevron arrow - none
25 (empty frame)
```

---

## Style 3: Cartographer's Engraving

```text
STYLE: antique atlas engraving. Each frame is a rounded square drawn like an old map cartouche, with a small compass-star ornament in the bottom-right corner. Icons are monochrome dark-sepia ink line art on aged parchment (#EDE3CF), with bold outer contours and simple parallel hatching for shadow (keep hatching sparse so it reads at 48 px). The element part gets a single flat watercolour-like wash of its accent colour, inside the lines. Looks like an illustration from a cartographer's grimoire.

REFERENCE: three reference images are attached. Match their glyph language exactly: bold flat shapes, uniform thick strokes with fully rounded caps and corners, simple geometric construction (diamonds, four-point stars, arcs, rings), no inner detail lines, one solid colour per glyph, the glyph filling about half of its frame. Image A is the game's existing icon set, image B shows the hotbar skill slot and skill-tree node frames the icons sit in, image C shows icons inside the diamond skill node. Do not copy these exact symbols or any numbers from the references; design new symbols for the skills below in the same visual language.

A single square sprite sheet of 24 skill icons for "Pale Vertica", a mature dark-fantasy 2D action RPG.

LAYOUT: one 2048x2048 image, a strict grid of exactly 5 rows and 5 columns (25 equal square cells) with even thin gutters. Read left-to-right, top-to-bottom. Every cell holds one icon centred inside the same rounded-square frame (corner radius about 10% of the cell). In the top-left corner of each cell put a small index tag with a two-digit number, 01 to 25, in a clean sans-serif. Cell 25 is an empty frame with only its index tag. No skill names, no other text, no watermark, no signature.

READABILITY: every icon must still read at 48x48 px. One bold central silhouette per icon, strong figure-ground contrast, no tiny details, identical line weight, identical lighting direction (top-left) and identical icon scale in every cell.

COLOUR RULE: the sheet is greyscale except for one element accent colour per icon, used only on the magical or elemental part. Accents: Fire #FF7A1A, Holy #F2D27A, Electric #5FE3FF, Earth #C8913A, Ghost #9A86C8, Time #A9C4FF, Mind #C04BFF, Wind #7FE0C8, Petal #F4A6C0. Skills marked "none" use no accent.

CELLS (index - skill - icon subject - accent):
01 Tackle - a shoulder charging forward with speed lines and an impact burst - none
02 Healing - an open palm cradling a small glowing cross of light, rising sparks - Holy
03 Hellfire Blast - a wide cone of fire erupting forward - Fire
04 Divine Blessing - a radiant halo ring with three feathers rising above it - Holy
05 Earthshatter - a fist striking the ground, cracks spreading in a cone, flying rocks - Earth
06 Lightning Strike - one straight lightning beam piercing across the icon - Electric
07 Shadow Dash - a dark silhouette dashing with ghostly afterimages behind it - Ghost
08 Aura of Flames - a ring of flames encircling a standing figure - Fire
09 Lightning Blessing - an upright sword wrapped in crackling sparks - Electric
10 Fire Pila - a tall pillar of fire bursting from a magic circle on the ground - Fire
11 Summon Valkyrie - a winged helmet above a spear, summoned from light - none
12 Perfect Parry - two crossed blades with a bright flash at the contact point - none
13 Chain Lightning - a lightning bolt jumping between three node points - Electric
14 Blink Strike - a dagger thrusting out of a torn teleport rift - none
15 Time Dilation - an hourglass inside a slowing spiral clock ring - Time
16 Force Nova - concentric shockwave rings bursting outward from a centre point - none
17 Blink - a figure dissolving into sparks and reappearing on the other side, dotted arc between - Electric
18 Chornos Field - a large astrolabe clock face forming a dome over the ground - Time
19 Mind Control - an eye with puppet strings hanging from it - Mind
20 Wind Slash - a sharp curved blade of wind with motion streaks - Wind
21 Crescent Sweep - a sword arc shaped like a crescent moon - none
22 Cherry Blossom Execution - a katana slash circled by falling cherry petals - Petal
23 Fire Bolt - a fireball streaking diagonally upward-right with a short trail - Fire
24 Sprint - a boot with wind speed lines and a forward chevron arrow - none
25 (empty frame)
```

---

## Style 4: Gothic Lancet (Stained Glass)

```text
STYLE: gothic stained glass. Inside each rounded-square cell, the icon sits in a tall pointed lancet-arch window. Thick black leading lines divide the image into flat colour panes. Most panes are muted greys from #2A2A2A to #D9D9D9; only 1 or 2 panes use the element accent colour. Tiny star dots in the background panes for a celestial feel. Flat colours only: no glow, no texture, no gradients. Sacred and ceremonial.

REFERENCE: three reference images are attached. Match their glyph language exactly: bold flat shapes, uniform thick strokes with fully rounded caps and corners, simple geometric construction (diamonds, four-point stars, arcs, rings), no inner detail lines, one solid colour per glyph, the glyph filling about half of its frame. Image A is the game's existing icon set, image B shows the hotbar skill slot and skill-tree node frames the icons sit in, image C shows icons inside the diamond skill node. Do not copy these exact symbols or any numbers from the references; design new symbols for the skills below in the same visual language.

A single square sprite sheet of 24 skill icons for "Pale Vertica", a mature dark-fantasy 2D action RPG.

LAYOUT: one 2048x2048 image, a strict grid of exactly 5 rows and 5 columns (25 equal square cells) with even thin gutters. Read left-to-right, top-to-bottom. Every cell holds one icon centred inside the same rounded-square frame (corner radius about 10% of the cell). In the top-left corner of each cell put a small index tag with a two-digit number, 01 to 25, in a clean sans-serif. Cell 25 is an empty frame with only its index tag. No skill names, no other text, no watermark, no signature.

READABILITY: every icon must still read at 48x48 px. One bold central silhouette per icon, strong figure-ground contrast, no tiny details, identical line weight, identical lighting direction (top-left) and identical icon scale in every cell.

COLOUR RULE: the sheet is greyscale except for one element accent colour per icon, used only on the magical or elemental part. Accents: Fire #FF7A1A, Holy #F2D27A, Electric #5FE3FF, Earth #C8913A, Ghost #9A86C8, Time #A9C4FF, Mind #C04BFF, Wind #7FE0C8, Petal #F4A6C0. Skills marked "none" use no accent.

CELLS (index - skill - icon subject - accent):
01 Tackle - a shoulder charging forward with speed lines and an impact burst - none
02 Healing - an open palm cradling a small glowing cross of light, rising sparks - Holy
03 Hellfire Blast - a wide cone of fire erupting forward - Fire
04 Divine Blessing - a radiant halo ring with three feathers rising above it - Holy
05 Earthshatter - a fist striking the ground, cracks spreading in a cone, flying rocks - Earth
06 Lightning Strike - one straight lightning beam piercing across the icon - Electric
07 Shadow Dash - a dark silhouette dashing with ghostly afterimages behind it - Ghost
08 Aura of Flames - a ring of flames encircling a standing figure - Fire
09 Lightning Blessing - an upright sword wrapped in crackling sparks - Electric
10 Fire Pila - a tall pillar of fire bursting from a magic circle on the ground - Fire
11 Summon Valkyrie - a winged helmet above a spear, summoned from light - none
12 Perfect Parry - two crossed blades with a bright flash at the contact point - none
13 Chain Lightning - a lightning bolt jumping between three node points - Electric
14 Blink Strike - a dagger thrusting out of a torn teleport rift - none
15 Time Dilation - an hourglass inside a slowing spiral clock ring - Time
16 Force Nova - concentric shockwave rings bursting outward from a centre point - none
17 Blink - a figure dissolving into sparks and reappearing on the other side, dotted arc between - Electric
18 Chornos Field - a large astrolabe clock face forming a dome over the ground - Time
19 Mind Control - an eye with puppet strings hanging from it - Mind
20 Wind Slash - a sharp curved blade of wind with motion streaks - Wind
21 Crescent Sweep - a sword arc shaped like a crescent moon - none
22 Cherry Blossom Execution - a katana slash circled by falling cherry petals - Petal
23 Fire Bolt - a fireball streaking diagonally upward-right with a short trail - Fire
24 Sprint - a boot with wind speed lines and a forward chevron arrow - none
25 (empty frame)
```

---

## Style 5: Celestial Sigil

```text
STYLE: arcane sigils. Each skill is drawn as an occult seal: the core symbol is built from thin precise geometric lines, circles, compass stars and sacred-geometry construction lines, in silver-white (#E6E6E6) on deep black (#09090A) rounded-square frames with a 2px grey outline. Consistent 3px line weight, one or two faint orbit rings around the symbol, one small accent-coloured mark for the element. Minimal, elegant, engraved-silver feel. Line art only, easy to trace to vector.

REFERENCE: three reference images are attached. Match their glyph language exactly: bold flat shapes, uniform thick strokes with fully rounded caps and corners, simple geometric construction (diamonds, four-point stars, arcs, rings), no inner detail lines, one solid colour per glyph, the glyph filling about half of its frame. Image A is the game's existing icon set, image B shows the hotbar skill slot and skill-tree node frames the icons sit in, image C shows icons inside the diamond skill node. Do not copy these exact symbols or any numbers from the references; design new symbols for the skills below in the same visual language.

A single square sprite sheet of 24 skill icons for "Pale Vertica", a mature dark-fantasy 2D action RPG.

LAYOUT: one 2048x2048 image, a strict grid of exactly 5 rows and 5 columns (25 equal square cells) with even thin gutters. Read left-to-right, top-to-bottom. Every cell holds one icon centred inside the same rounded-square frame (corner radius about 10% of the cell). In the top-left corner of each cell put a small index tag with a two-digit number, 01 to 25, in a clean sans-serif. Cell 25 is an empty frame with only its index tag. No skill names, no other text, no watermark, no signature.

READABILITY: every icon must still read at 48x48 px. One bold central silhouette per icon, strong figure-ground contrast, no tiny details, identical line weight, identical lighting direction (top-left) and identical icon scale in every cell.

COLOUR RULE: the sheet is greyscale except for one element accent colour per icon, used only on the magical or elemental part. Accents: Fire #FF7A1A, Holy #F2D27A, Electric #5FE3FF, Earth #C8913A, Ghost #9A86C8, Time #A9C4FF, Mind #C04BFF, Wind #7FE0C8, Petal #F4A6C0. Skills marked "none" use no accent.

CELLS (index - skill - icon subject - accent):
01 Tackle - a shoulder charging forward with speed lines and an impact burst - none
02 Healing - an open palm cradling a small glowing cross of light, rising sparks - Holy
03 Hellfire Blast - a wide cone of fire erupting forward - Fire
04 Divine Blessing - a radiant halo ring with three feathers rising above it - Holy
05 Earthshatter - a fist striking the ground, cracks spreading in a cone, flying rocks - Earth
06 Lightning Strike - one straight lightning beam piercing across the icon - Electric
07 Shadow Dash - a dark silhouette dashing with ghostly afterimages behind it - Ghost
08 Aura of Flames - a ring of flames encircling a standing figure - Fire
09 Lightning Blessing - an upright sword wrapped in crackling sparks - Electric
10 Fire Pila - a tall pillar of fire bursting from a magic circle on the ground - Fire
11 Summon Valkyrie - a winged helmet above a spear, summoned from light - none
12 Perfect Parry - two crossed blades with a bright flash at the contact point - none
13 Chain Lightning - a lightning bolt jumping between three node points - Electric
14 Blink Strike - a dagger thrusting out of a torn teleport rift - none
15 Time Dilation - an hourglass inside a slowing spiral clock ring - Time
16 Force Nova - concentric shockwave rings bursting outward from a centre point - none
17 Blink - a figure dissolving into sparks and reappearing on the other side, dotted arc between - Electric
18 Chornos Field - a large astrolabe clock face forming a dome over the ground - Time
19 Mind Control - an eye with puppet strings hanging from it - Mind
20 Wind Slash - a sharp curved blade of wind with motion streaks - Wind
21 Crescent Sweep - a sword arc shaped like a crescent moon - none
22 Cherry Blossom Execution - a katana slash circled by falling cherry petals - Petal
23 Fire Bolt - a fireball streaking diagonally upward-right with a short trail - Fire
24 Sprint - a boot with wind speed lines and a forward chevron arrow - none
25 (empty frame)
```

---

## Style 6: HD-2D Cel-shaded

```text
STYLE: premium HD-2D JRPG skill icons, cel-shaded. Each frame is a near-black rounded square (#141313) with a 2px light-grey outline and faint topographic contour lines. Icons are semi-detailed with exactly three flat tones per colour (shadow, base, highlight), a thick dark outline and a crisp rim light from the top-left. Greyscale for everything except the element accent, which may use its own three tones. No soft gradients, no airbrush, no noise, so the art can be traced to vector shapes.

REFERENCE: three reference images are attached. Match their glyph language exactly: bold flat shapes, uniform thick strokes with fully rounded caps and corners, simple geometric construction (diamonds, four-point stars, arcs, rings), no inner detail lines, one solid colour per glyph, the glyph filling about half of its frame. Image A is the game's existing icon set, image B shows the hotbar skill slot and skill-tree node frames the icons sit in, image C shows icons inside the diamond skill node. Do not copy these exact symbols or any numbers from the references; design new symbols for the skills below in the same visual language.

A single square sprite sheet of 24 skill icons for "Pale Vertica", a mature dark-fantasy 2D action RPG.

LAYOUT: one 2048x2048 image, a strict grid of exactly 5 rows and 5 columns (25 equal square cells) with even thin gutters. Read left-to-right, top-to-bottom. Every cell holds one icon centred inside the same rounded-square frame (corner radius about 10% of the cell). In the top-left corner of each cell put a small index tag with a two-digit number, 01 to 25, in a clean sans-serif. Cell 25 is an empty frame with only its index tag. No skill names, no other text, no watermark, no signature.

READABILITY: every icon must still read at 48x48 px. One bold central silhouette per icon, strong figure-ground contrast, no tiny details, identical line weight, identical lighting direction (top-left) and identical icon scale in every cell.

COLOUR RULE: the sheet is greyscale except for one element accent colour per icon, used only on the magical or elemental part. Accents: Fire #FF7A1A, Holy #F2D27A, Electric #5FE3FF, Earth #C8913A, Ghost #9A86C8, Time #A9C4FF, Mind #C04BFF, Wind #7FE0C8, Petal #F4A6C0. Skills marked "none" use no accent.

CELLS (index - skill - icon subject - accent):
01 Tackle - a shoulder charging forward with speed lines and an impact burst - none
02 Healing - an open palm cradling a small glowing cross of light, rising sparks - Holy
03 Hellfire Blast - a wide cone of fire erupting forward - Fire
04 Divine Blessing - a radiant halo ring with three feathers rising above it - Holy
05 Earthshatter - a fist striking the ground, cracks spreading in a cone, flying rocks - Earth
06 Lightning Strike - one straight lightning beam piercing across the icon - Electric
07 Shadow Dash - a dark silhouette dashing with ghostly afterimages behind it - Ghost
08 Aura of Flames - a ring of flames encircling a standing figure - Fire
09 Lightning Blessing - an upright sword wrapped in crackling sparks - Electric
10 Fire Pila - a tall pillar of fire bursting from a magic circle on the ground - Fire
11 Summon Valkyrie - a winged helmet above a spear, summoned from light - none
12 Perfect Parry - two crossed blades with a bright flash at the contact point - none
13 Chain Lightning - a lightning bolt jumping between three node points - Electric
14 Blink Strike - a dagger thrusting out of a torn teleport rift - none
15 Time Dilation - an hourglass inside a slowing spiral clock ring - Time
16 Force Nova - concentric shockwave rings bursting outward from a centre point - none
17 Blink - a figure dissolving into sparks and reappearing on the other side, dotted arc between - Electric
18 Chornos Field - a large astrolabe clock face forming a dome over the ground - Time
19 Mind Control - an eye with puppet strings hanging from it - Mind
20 Wind Slash - a sharp curved blade of wind with motion streaks - Wind
21 Crescent Sweep - a sword arc shaped like a crescent moon - none
22 Cherry Blossom Execution - a katana slash circled by falling cherry petals - Petal
23 Fire Bolt - a fireball streaking diagonally upward-right with a short trail - Fire
24 Sprint - a boot with wind speed lines and a forward chevron arrow - none
25 (empty frame)
```
