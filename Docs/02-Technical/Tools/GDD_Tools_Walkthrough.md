# GDD Tools Walkthrough

Verified: **2026-10-17**. How to run the Game Design Studio (GDD Tools), edit the Pale Vertica design document, and export it as PDF or Word.

GDD Tools is a browser app (vanilla JS, no build step) that keeps a game design document as modules and fields. The Pale Vertica GDD (24 modules: 15 industry templates plus 9 Pale Vertica modules for story, attributes, casting, skills, supports and passives, items, UI, art and known issues) is preloaded into it.

## Where things are

| Path | What it is |
|---|---|
| `D:\Games\GDD Tools\` | Source of the tool. Edit code here. |
| `index.html`, `styles.css`, `app.js` | Page, styles, editor and PDF preview |
| `doc-model.js` | Shared text-structure parser (flows, steps, lists, status tags) used by both PDF and Word |
| `docx-export.js` | Native `.docx` writer (headings, table of contents, tables, lists, page numbers), no external libraries |
| `Pale_Vertica_GDD.json` | The Pale Vertica GDD as an importable project file |
| `pv-seed.js` | One-time loader that puts the Pale Vertica GDD into a browser on first open (used only by the hosted copy) |
| `backup\2026-10-17\` | The tool before the PDF/Word changes |
| `D:\Application Project\LocalWebPort\` | Local web server (port 8080) that hosts the tool |
| `D:\Application Project\LocalWebPort\.hosted_projects\gdd-tools\` | The hosted copy that the browser actually loads |

The hosted copy is a **copy**, not a live mount. After editing the source you must sync it (step 6).

## 1. Start the server

LocalWebPort usually runs already, because Antigravity starts it as its `localwebport` MCP server. Check it:

```powershell
(Invoke-RestMethod http://localhost:8080/api/status).status
```

`running` means you can go to step 2. Otherwise start it by double-clicking `D:\Application Project\LocalWebPort\run.bat` (or `npm start` in that folder). The dashboard at `http://localhost:8080/` lists every hosted project; `gdd-tools` reloads automatically on every start.

If port 8080 is taken, a second LocalWebPort instance moves to 8081. The URL then changes, and so does the browser storage (see common mistakes).

## 2. Open the tool

- This PC: `http://localhost:8080/p/gdd-tools/`
- Phone or another PC on the same Wi-Fi: `http://<LAN IP>:8080/p/gdd-tools/` (the dashboard shows the LAN IP; on 2026-10-17 it was `192.168.1.46`).

On the first open in a browser, the document **Pale Vertica: Whispers of the Pale Throne** appears automatically. If you see "My Game Specification" instead, click **Open** and pick the Pale Vertica project, or click **Import** and choose `D:\Games\GDD Tools\Pale_Vertica_GDD.json`.

## 3. Edit the document

1. **Outline** (top-left button) shows or hides the left side nav. Click a module there to jump to it. **+ Add Module** at the bottom of the side nav adds a template module or an empty custom one.
2. Each module has **Add Field**. Field types: Short Text, Paragraph (textarea), Key-Value grid, Action Matrix (verbs with CAN/CANNOT and cues), World Zones (with POIs), Entity Roster (with attack moves), and the Department Task Board.
3. Rename the document by clicking its title in the header.
4. Every change autosaves to the browser (status "Auto-saved at ..." in the header). **Save** forces a save. **Export JSON** makes a backup file that you can import anywhere.
5. Mark the status of anything you write with a tag in square brackets: `[มีในเกมแล้ว]` (done), `[ต้นแบบ]` (prototype), `[ออกแบบไว้ ยังไม่ทำ]` (designed, not built), `[ยังไม่กำหนด]` (undecided), `[ข้อเสนอ รอทีมอนุมัติ]` (proposal). English words such as `[Done]`, `[Prototype]`, `[Planned]`, `[TBD]` and `[Proposal]` work too.

### How text is laid out in PDF and Word

Paragraph fields, and Short Text fields longer than 110 characters, print as a heading with body text (no box). Lines are formatted automatically:

| You type | It becomes |
|---|---|
| `A -> B -> C` (three or more parts) | **Flow**: boxes joined by arrows. If any part is longer than 38 characters or there are more than 7 parts, it becomes **Steps** instead. |
| `Roadmap: Phase 1 ... -> Phase 2 ...` | Steps titled "Roadmap". A part starting with `Phase N`, `Step N`, `Chapter N`, `ขั้นที่ N` or `เฟส N` shows that label in bold. |
| Lines starting with `1.` `2.` | Numbered list |
| Lines starting with `-` or `•`, or one line with two or more ` • ` separators | Bullet list |
| `Label: text` (label up to 32 characters) | Bold label followed by the text; consecutive lines become a definition list |
| `[tag]` | Status badge (color depends on the tag word) |

If the automatic layout is wrong for a field, change the **PDF: Auto** selector next to the field's type to Paragraph, Steps, Flow or Bullets. The selector is saved with the field and applies to both PDF and Word.

## 4. Choose a style and preview

Click **Export PDF** or **Export Word** (menu bar, the blue button, or the File menu on mobile). The **Export Document** dialog opens:

| Style | Looks like |
|---|---|
| **Default** | Decorated: cover page with classification banner (plus sign-off boxes in the PDF), revision register, section numbers and category tags, colored status badges, boxed flows, bordered tables. In Word every section starts on a new page. |
| **Clean** | Plain document: title, short meta block, contents, black headings, thin table rules, no fills or boxes, tags as `[italic]` text. In Word sections flow continuously. |

The tool remembers the last style you chose. **Preview** shows the document on screen; its toolbar has **Back to Editor**, a Default / Clean switch, **Export Word** and **Export PDF**.

## 5. Export

- **Export PDF** opens the browser print dialog. Choose **Save as PDF**, paper **A4**, and for the Default style turn on **Background graphics** so the colored badges and fills print.
- **Export Word (.docx)** downloads `<document title> - GDD.docx` (or `... - GDD (Clean).docx`). When Word opens it, it asks *"This document contains fields that may refer to other files. Update the fields?"*. Click **Yes** to fill the table of contents with page numbers. In Word the sections are Heading 1 and the fields are Heading 2, so **View > Navigation Pane** lists them.
- **Export MD** writes a Markdown summary. **Export JSON** writes the full project for backup or sharing.

Expected result for the Pale Vertica GDD (2026-10-17, checked by opening both files in Microsoft Word 2016+): Default `.docx` 42 pages, 47 tables and a 24-entry contents page; Clean `.docx` 31 pages and 45 tables.

## 6. After changing the tool's code

Edit files in `D:\Games\GDD Tools\`, then copy them to the hosted folder. Run this in PowerShell. It also puts the `pv-seed.js` line back into the hosted `index.html`:

```powershell
$src = 'D:\Games\GDD Tools'; $dst = 'D:\Application Project\LocalWebPort\.hosted_projects\gdd-tools'
Copy-Item "$src\app.js","$src\styles.css","$src\doc-model.js","$src\docx-export.js","$src\pv-seed.js","$src\Pale_Vertica_GDD.json" $dst -Force
(Get-Content "$src\index.html" -Raw -Encoding UTF8) -replace '<script src="app.js"></script>', "<script src=`"pv-seed.js`"></script>`r`n  <script src=`"app.js`"></script>" | Set-Content "$dst\index.html" -Encoding UTF8 -NoNewline
```

Then reload the page with **Ctrl+F5**. Script load order must stay `doc-model.js`, `docx-export.js`, (`pv-seed.js`), `app.js`.

If the `gdd-tools` project was deleted from the LocalWebPort dashboard (that also deletes the hosted folder), recreate it, then run the sync command above:

```powershell
$html = Get-Content 'D:\Games\GDD Tools\index.html' -Raw -Encoding UTF8
$body = [System.Text.Encoding]::UTF8.GetBytes((@{ slug = 'gdd-tools'; title = 'GDD Tools'; html = $html } | ConvertTo-Json -Depth 3))
Invoke-RestMethod -Method Post -Uri http://localhost:8080/api/projects -ContentType 'application/json; charset=utf-8' -Body $body
```

## Common mistakes

- **"My document is gone".** Documents live in the browser's localStorage, separately for each browser and each address. Chrome vs Edge, `localhost` vs the LAN IP, or port 8080 vs 8081 all give a different, empty workspace. Clearing site data deletes it. Use **Export JSON** to move or back up work, and **Import** to bring it back.
- **Opening `D:\Games\GDD Tools\index.html` directly** (file://) works, but it uses separate storage and does not preload the Pale Vertica GDD. Use the localhost URL, or Import the JSON.
- **The page still looks old after a code change.** Either the hosted copy was not synced (step 6) or the browser cached the old scripts; press Ctrl+F5.
- **Default PDF prints without colors.** Turn on Background graphics in the print dialog.
- **Word contents page has no page numbers.** Word's update prompt was answered No. Right-click the contents, choose **Update Field > Update entire table**.
- **A flow shows as a plain sentence.** A flow needs at least three parts (`A -> B -> C`) with spaces around `->`. Use the field's **PDF:** selector to force Flow or Steps.
- **The tool does not open.** LocalWebPort is not running (Antigravity closed): start `run.bat`.
- **Thai text looks different in Word on another computer.** The `.docx` uses the Leelawadee UI font (standard on Windows). Other systems substitute a similar Thai font.
