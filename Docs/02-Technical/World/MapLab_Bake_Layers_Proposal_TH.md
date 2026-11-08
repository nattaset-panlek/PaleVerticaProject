# MapLab Bake Layers + Paint: ข้อเสนอแนวทาง (TH)

สถานะ: **ช่วง 1-3 ทำแล้ว** (2026-10-23) ตามตัวเลือกที่แนะนำทั้งหมด: Export/Import PSD และ PNG, รายการ layer เก็บบน MapLabMap, syntax `<layer>_paint` / `<layer>_paint_<ชื่อ>`, Sorted layer, paint ของ Sorted, Flatten วิธีใช้: [MapLab_Bake_Layers_Walkthrough_TH.md](./MapLab_Bake_Layers_Walkthrough_TH.md)

ต่างจากข้อเสนอในช่วง 2-3: Sorted layer ไม่ได้ถ่ายแต่ละชิ้นแยก แต่ถ่ายภาพรวมแล้วตัดตาม "เจ้าของพิกเซล" (ชิ้นหน้าสุดที่ทึบ) ทำให้รอยต่อไม่เป็นเส้น และ paint ของ Sorted เก็บเป็น chunk ยึดโลกแล้วระบบตัดลงชิ้นเอง (ไม่ต้องวาดทีละชิ้น และไม่หลุดเมื่อ shape ถูกหั่นใหม่)

ต่างจากข้อเสนอ: ตารางช่องยึดกับจุด 0,0 ของโลก (ไม่ใช่ Bounds ของแมพ) เพื่อให้ช่องไม่ขยับแม้แก้ Bounds และโฟลเดอร์ History ชื่อ `History~` เพื่อไม่ให้ Unity import ภาพเก่า

โจทย์จากผู้ใช้:

1. เลือก (ติ๊ก) ได้ว่าจะ bake อะไร แล้วเก็บเป็น **layer**
2. ของที่ Y Sort (กำแพง ที่ราบ เสา) ก็ bake ได้ แต่ต้องยังเรียงหน้า/หลังกับตัวละครถูก
3. bake แล้ว = ใช้ภาพ จนกว่าจะ unbake (แก้ shape ไม่ส่งผลจนกว่าจะ bake ใหม่) อันนี้ตั้งใจ
4. ให้คนทำ art **เอาภาพที่ bake ไปวาดต่อ (paint)** ได้ และ **bake ใหม่ต้องไม่ทับงานที่วาด**
5. ระบบกับอิสระของ art ต้องไปด้วยกัน

## 1. แนวคิดหลัก: ภาพที่ระบบสร้าง แยกจากภาพที่คนวาด

```
แต่ละ layer มี 2 ชั้นภาพ ซ้อนกันตอนเล่น

   Paint   <- คนวาด  (PNG โปร่งใส)   ระบบไม่เขียนทับ ไม่ลบ ตลอดไป
   ------
   Base    <- ระบบสร้างจาก shape    bake ใหม่ = สร้างใหม่ (ของเก่าเก็บใน History)
```

- **Base** = ผลการ bake จาก shape/sprite ในแมพ ระบบเป็นเจ้าของ สร้างใหม่ได้ทุกครั้ง
- **Paint** = งานวาดเพิ่ม (ดิน หญ้า รอยแตก แสงเงาที่วาดเอง) คนทำ art เป็นเจ้าของ **ระบบไม่แตะ**
- ตอนเล่น วาด Base แล้ววาด Paint ทับ ด้วยตารางช่องเดียวกัน ตำแหน่งเดียวกัน
- เพราะแยกกัน กด bake ใหม่กี่ครั้ง งานวาดก็ไม่หาย

## 2. Layer คืออะไร

| ช่อง | ตัวอย่าง |
|---|---|
| Name | Ground, Shadows, Cliffs |
| Include (ติ๊ก) | ตาม **Set** (Cliffs, Plaza, Roads, ...) และ/หรือ ตาม **ชนิด** (พื้น sprite, ถนน, รูปทรงแบน, เงารูปทรง, เงาพร็อพ, รูปทรง Y Sort) |
| Kind | **Ground** (ใต้ตัวละคร ภาพใหญ่เป็นช่อง) หรือ **Sorted** (Y Sort ภาพต่อชิ้น) |
| Resolution | px ต่อหน่วย |
| Mode | **Live** (ยังไม่ bake), **Baked** (ใช้ภาพ, bake ใหม่ได้), **Locked** (art เป็นเจ้าของทั้ง layer, ระบบไม่ bake ทับเลย) |

ตัวอย่างแมพ demo:

```
Layer          Kind     Include                               Mode
Ground         Ground   Grass, Ground Patches, Roads, Plaza    Baked  + Paint
Shadows        Ground   เงารูปทรง, เงาพร็อพ                    Baked
Cliffs         Sorted   Set Cliffs                              Live (ยังแก้รูปอยู่)
```

แต่ละ layer bake / unbake / lock ได้แยกกัน หน้าต่าง MapLab มีหน้า **Layers** แทนหน้า Static ground เดิม

## 3. Y Sort bake ได้อย่างไร (Kind = Sorted)

- ทุกชิ้นของรูปทรงที่เรียงกับตัวละคร (แถบกำแพง, ด้านบนชิ้นเดียว, ชั้นบน) ถูกถ่ายเป็น **ภาพต่อชิ้น** วางที่จุดเรียงเดิม ใน SortingGroup order เดิม
- ผล: ตัวละครเดินหน้า/หลังได้ถูกเหมือนเดิมทุกอย่าง เพราะใช้จุดเรียงชุดเดิม แค่เปลี่ยนจาก mesh เป็นภาพ
- ภาพทั้ง layer รวมใน atlas เดียว ให้วาดรวมกันได้มากที่สุด
- ข้อจำกัด: draw call ไม่ลดเท่า layer Ground (ยังเป็นชิ้นตามแถบเรียง) ข้อดีหลักคือ **ให้ art วาดลงบนกำแพง/หน้าผาได้**
- Paint ของ Sorted ทำต่อชิ้น (ภาพโปร่งใสขนาดเท่าชิ้น) ทำทีหลัง Ground ได้

## 4. วิธีวาด (Paint) ที่แนะนำ: ส่งออกไปโปรแกรมวาด แล้วนำเข้ากลับ

```
[Export Paint Canvas]  ->  Environment_Ground_Base.png  (ทั้งแมพต่อกัน + เส้นช่อง)
                           Environment_Ground_Paint.png (โปร่งใส หรือ paint เดิมที่มีอยู่)
          |
     Photoshop / Krita / Aseprite: วาดใน layer ของ Paint เท่านั้น
          |
[Import Paint]         ->  ตัดเป็น Paint/Chunk_x_y.png อัตโนมัติ
```

- ทำไมไม่วาดใน Unity: เครื่องมือวาดใน Editor ทำได้แต่สู้โปรแกรมวาดจริงไม่ได้ (brush, layer, blend) และต้องดูแลเยอะ
- ไฟล์ Base ที่ส่งออกไว้ดูเทียบเท่านั้น ระบบนำเข้าเฉพาะ Paint

### 4.1 Paint layer ผูกด้วยชื่อ (syntax) ซ้อนระหว่าง layer ได้

ไม่จำกัดว่ามี paint กี่ชั้น และไม่ต้องอยู่บนสุด ระบบรู้ว่า paint ชั้นไหนเป็นของ bake layer ไหนจาก **ชื่อ** และวางลำดับตาม **ลำดับในไฟล์**

```
ลำดับ (บนสุด = วาดทีหลัง)      ชนิด          เป็นของ
Top                             bake layer    -           (ระบบสร้าง, bake ใหม่ได้)
mid_paint_cracks                paint         Mid         (art, ระบบไม่แตะ)
mid_paint                       paint         Mid         (art, ระบบไม่แตะ)
Mid                             bake layer    -
ground_paint                    paint         Ground
Ground                          bake layer    -
sketch idea                     ไม่ตรง syntax  -           (ข้ามตอนนำเข้า ใช้ร่างได้อิสระ)
```

**Syntax**: `<ชื่อ bake layer>_paint` หรือ `<ชื่อ bake layer>_paint_<คำอะไรก็ได้>`

- ไม่สนตัวพิมพ์เล็ก/ใหญ่ (`Mid_paint` = `mid_paint`) คำต่อท้ายใช้ตั้งชื่อให้จำง่าย เช่น `ground_paint_dirt`, `mid_paint_moss 2`
- ใช้ได้ทั้ง layer เดี่ยวและ **กลุ่ม**: กลุ่มที่ชื่อตรง syntax = paint 1 ชั้น ทุก layer ข้างในรวมกันแบบ Normal
- layer ที่ชื่อ **ตรงกับ bake layer** เป๊ะ (เช่น `Mid`) คือภาพที่ระบบ bake ล็อกไว้ดูเทียบ นำเข้าจะไม่อ่าน
- layer ที่ไม่ตรง syntax ไม่ถูกนำเข้า (ใช้ร่าง อ้างอิง ได้อิสระ) รายงานตอนนำเข้าบอกว่าข้ามอะไรไปบ้าง
- ชื่อ bake layer ที่ไม่มีอยู่ในแมพ (เช่น `cave_paint` แต่ไม่มี layer Cave) รายงานว่า "ไม่รู้จัก Cave" แล้วข้าม
- ตั้งชื่อไฟล์ PNG ด้วย syntax เดียวกันก็ได้ (`mid_paint_cracks.png`) ถ้าไม่ใช้ PSD

ในหน้าต่าง MapLab หน้า Layers จะเห็นแบบนี้:

```
Top            bake    Up to date        [Bake] [Lock] [eye]
Mid            bake    Out of date       [Rebake] [Lock] [eye]
   mid_paint_cracks    paint, 6 chunks   [eye] [Up] [Down]   ไม่โดน bake ทับ
   mid_paint           paint, 12 chunks  [eye] [Up] [Down]   ไม่โดน bake ทับ
Ground         bake    Locked            [Unlock] [eye]
   ground_paint        paint, 12 chunks  [eye] [Up] [Down]
```

- bake layer ไหนมี paint อยู่ จะเห็นทันที (paint เยื้องอยู่ใต้ชื่อ bake layer ของมัน พร้อมจำนวนช่องที่มีงานวาด)
- ลำดับวาดใน Unity = ลำดับในไฟล์ (ปรับในหน้าต่างด้วย Up/Down ได้ แล้ว Export ครั้งหน้าจะออกมาตามลำดับนี้)
- Bake ใหม่ = สร้างเฉพาะ bake layer; ทุก paint layer อยู่ครบ เพราะเป็นคนละชั้นกัน
- เปลี่ยนชื่อ bake layer ในหน้าต่าง: ชื่อ paint ที่ผูกอยู่เปลี่ยนตามให้ (`mid_paint` เป็น `cliff_paint`) ไฟล์ PSD ที่ส่งออกไปก่อนหน้าต้องแก้ชื่อเอง หรือ Export ใหม่
- ตอนเล่น ทุกชั้น (bake และ paint) ใช้ตารางช่องเดียวกัน order ถูกแจกตามลำดับในช่วงใต้ตัวละคร

### 4.2 เลือกส่งออกเป็น PSD ได้ (แนะนำเป็นค่าเริ่ม)

ปุ่ม Export มีให้เลือก **PSD** หรือ **PNG** (ไฟล์ละชั้น ตั้งชื่อตาม syntax)

```
Environment.psd                 (ทุก layer ของแมพในไฟล์เดียว เรียงตามลำดับจริง)
   Top                (ล็อก)    ภาพที่ระบบ bake
   mid_paint_cracks             งานวาดเดิม (ถ้ามี) วาดต่อได้
   mid_paint
   Mid                (ล็อก)
   ground_paint
   Ground             (ล็อก)
   Grid               (ซ่อน)    เส้นแบ่งช่อง chunk + ชื่อช่อง
```

- เปิดได้ใน Photoshop, Krita, Affinity Photo, GIMP, Photopea
- อยากเพิ่ม paint ชั้นใหม่: สร้าง layer หรือกลุ่มใหม่ ตั้งชื่อตาม syntax วางไว้ตรงลำดับที่ต้องการ แล้ว Import
- นำเข้า: อ่านเฉพาะ layer/กลุ่มที่ชื่อตรง syntax ตัดเป็น `Layers/<bake layer>/Paint/<ชื่อ paint>/Chunk_x_y.png` และจดลำดับไว้
- ข้อจำกัด (บอกในหน้าต่างตอนนำเข้า):
  - Blend mode อื่นนอกจาก Normal ใน paint layer/กลุ่ม จะถูกรวมแบบ Normal (ภาพอาจต่างจากในโปรแกรม) ให้ Merge/Rasterize ก่อนถ้าใช้
  - Layer effect, adjustment layer, smart object ไม่ถูกอ่าน ให้ Rasterize ก่อน
  - ขนาดภาพห้ามเปลี่ยน (ระบบเช็กกว้าง x สูงให้ตรงกับตอนส่งออก ไม่ตรงจะไม่นำเข้าและบอกเหตุผล)
- ขนาด: แมพ demo ที่ 64 px/หน่วย ได้ราว 3,300 x 2,300 px ใช้ PSD ปกติได้ แมพใหญ่เกิน 30,000 px ต่อด้าน ส่งออกเป็นส่วน (เลือกช่วงช่องที่จะวาด) หรือเป็น PSB
- ระบบเขียน/อ่าน PSD เอง (รูปแบบ RGBA 8 bit, layer + กลุ่ม, บีบอัด RLE) ไม่ต้องลง package เพิ่ม

## 5. กติกากันทับ (bake ใหม่ปลอดภัย)

1. **ตารางช่องคงที่**: ยึดกับ Bounds ของ MapLabMap ไม่ใช่ขอบของของที่ bake (ตอนนี้ยึดขอบของ จึงขยับได้) ย้ายของไปมากี่ครั้ง ช่องเดิมก็อยู่ที่เดิม งานวาดไม่เลื่อน
2. **Bake ใหม่เขียนเฉพาะ Base** Paint ไม่ถูกแตะ
3. **History**: Base เก่าย้ายไป `History/<วันเวลา>/` ก่อนเขียนใหม่ ย้อนได้
4. **เตือนช่องที่เปลี่ยนใต้งานวาด**: จดลายนิ้วมือ Base ต่อช่อง ถ้าช่องที่มี Paint ถูก bake แล้วเปลี่ยน ช่องนั้นขึ้นกรอบม่วงในหน้า Layers ("Base เปลี่ยนใต้งานวาด 2 ช่อง") ให้ art มาดู
5. **Lock**: layer ที่ art อยากเป็นเจ้าของ 100% กด Lock ระบบจะไม่ bake layer นั้นอีก (ปุ่ม Bake หาย มีแค่ Unlock)
6. **Flatten** (ไม่บังคับ): รวม Paint ลง Base แล้ว Lock ให้อัตโนมัติ เพื่อไม่ให้ bake ครั้งหน้าทับ

## 6. โฟลเดอร์

```
<ชื่อฉาก>_MapLab/<ชื่อแมพ>/
    Walkable.json, Walkable_Mask.png
    Layers/
        Ground/
            Base/Chunk_0_0.png ...       ระบบเขียน
            Paint/<ชื่อ paint>/Chunk_0_0.png ...   art เขียน (ระบบไม่แตะ) กี่ชั้นก็ได้ เช่น Paint/ground_paint/, Paint/ground_paint_dirt/
            History/2026-10-23_1530/     Base เก่า
        Cliffs/
            Base/<ชิ้น>.png + atlas
```

## 7. ระหว่าง bake อยู่ แก้แมพยังไง

- **Show Live** ต่อ layer: เปิดของจริงให้เห็นและแก้ได้ ภาพ layer ยังอยู่บนดิสก์ (เหมือน Unbake ตอนนี้)
- แก้เสร็จกด **Bake** ของ layer นั้น: Base ใหม่, Paint เดิม, ช่องที่เปลี่ยนใต้งานวาดถูกเตือน
- Physics/Walkable ไม่เกี่ยวกับการ bake ภาพ (collider ยังเป็นของจริงตลอด)

## 8. แผนทำเป็นช่วง

| ช่วง | ทำอะไร | ได้อะไร |
|---|---|---|
| 1 | Layer สำหรับ Ground: ติ๊กเลือกตาม Set/ชนิด, ตารางช่องคงที่, Paint layer ผูกด้วย syntax ซ้อนระหว่าง layer ได้, History, Lock, Export/Import PSD และ PNG, เตือนช่องที่เปลี่ยน | bake เป็นชั้น วาดต่อได้หลายชั้น bake ใหม่ไม่ทับ |
| 2 | Layer แบบ Sorted (Y Sort) เป็นภาพต่อชิ้น + atlas | bake กำแพง/หน้าผาได้ ตัวละครยังเรียงถูก |
| 3 | Paint ของ Sorted, Flatten | วาดลงบนกำแพง/หน้าผาได้ |

## 9. สิ่งที่ต้องตัดสินใจ

1. วาดที่ไหน: **ส่งออก/นำเข้ากับโปรแกรมวาด (แนะนำ)** หรือทำ brush ใน Unity และส่งออกเป็น **PSD (แนะนำ)**, PNG หรือมีทั้งสองแบบให้เลือก
2. เก็บรายการ layer ที่ไหน: **บน MapLabMap (แนะนำ, ง่าย)** หรือเป็นไฟล์ Bake Profile ที่ใช้ซ้ำหลายแมพได้
3. syntax ชื่อ paint: **`<layer>_paint` / `<layer>_paint_<ชื่อ>` (แนะนำ)** หรือแบบอื่น
4. เริ่ม **ช่วง 1** เลยไหม
