const fs = require('fs');

function buildRefinementPerfect() {
    let idCounter = 1000000;
    function nextId() { return (idCounter += 100).toString(); }

    const rootGoId = '8100100000000000001';
    const rootRtId = nextId(), rootCrId = nextId(), rootCgId = nextId(), rootWinId = nextId();
    const bgGoId = nextId(), bgRtId = nextId(), bgCrId = nextId(), bgImgId = nextId();

    // Fonts
    const DEMIBOLD_FONT = 'ed472c2881e9b894fb40179e7c9b4332'; // Novecentosanswide-DemiBold SDF
    const NORMAL_FONT = 'a66aeea0855f6544883cbd79dc661f3a';   // Novecentosanswide-Normal SDF
    const KANIT_BOLD = '0063cb4c629a50d499805b5a4274846a';    // Kanit-Bold SDF
    const NOTO_SEMIBOLD = '57d8cea201ff3e04b8aef8e01a97988a';  // NotoSansThai-SemiBold SDF

    // Sprites
    const SPRITE_BG = 'd51074f4b22f7616ea3e246fbce26767';
    const SPRITE_ROUNDED = '9df5b6060bad4fb4b9b050483f64c324'; // Assets/Art/UI/Old/rounded.png (Standard 9-slice)
    const SPRITE_TAG_STAR = 'a1000000000000000000000000000010';
    const SPRITE_CLOSE = 'a9d6579a209c58146a11b71d4368b9ee';
    const SPRITE_LEVEL_FRAME = 'a1000000000000000000000000000004';
    const SPRITE_DIAL = '4c3042e015830010111da33a43be481e';
    const SPRITE_CENTER_SLOT = '234877d4ae6728879a5848f71764d45d';
    const SPRITE_SLOT_RED = '219f98baeaedc04c919a7c26af8901f0';
    const SPRITE_SLOT_BLUE = '42466307e4930d00b5f0a05bd2578099';
    const SPRITE_SCROLL_TRACK = 'a1000000000000000000000000000006';
    const SPRITE_SCROLL_THUMB = 'a1000000000000000000000000000005';
    const SPRITE_PROG_TRACK = 'a1000000000000000000000000000007';
    const SPRITE_PROG_FILL = 'a1000000000000000000000000000008';
    const SPRITE_BTN_PRIMARY = 'a1000000000000000000000000000001';

    // Header
    const tagGoId = nextId(), tagRtId = nextId(), tagCrId = nextId(), tagImgId = nextId();
    const starGoId = nextId(), starRtId = nextId(), starCrId = nextId(), starImgId = nextId();
    const titleGoId = nextId(), titleRtId = nextId(), titleCrId = nextId(), titleTmpId = nextId();
    const closeBtnGoId = nextId(), closeBtnRtId = nextId(), closeBtnCrId = nextId(), closeBtnImgId = nextId(), closeBtnCompId = nextId();

    // Left container
    const levelBadgeGoId = nextId(), levelBadgeRtId = nextId(), levelBadgeCrId = nextId(), levelBadgeImgId = nextId();
    const levelTxtGoId = nextId(), levelTxtRtId = nextId(), levelTxtCrId = nextId(), levelTxtTmpId = nextId();
    const dialGoId = nextId(), dialRtId = nextId(), dialCrId = nextId(), dialImgId = nextId();
    const centerSlotGoId = nextId(), centerSlotRtId = nextId(), centerSlotCrId = nextId(), centerSlotImgId = nextId();
    const itemIconGoId = nextId(), itemIconRtId = nextId(), itemIconCrId = nextId(), itemIconImgId = nextId();

    const matSlots = [];
    for (let i = 0; i < 3; i++) {
        matSlots.push({
            goId: nextId(), rtId: nextId(), crId: nextId(), imgId: nextId()
        });
    }

    // Right container items
    const rarityBadgeGoId = nextId(), rarityBadgeRtId = nextId(), rarityBadgeCrId = nextId(), rarityBadgeImgId = nextId();
    const rarityTxtGoId = nextId(), rarityTxtRtId = nextId(), rarityTxtCrId = nextId(), rarityTxtTmpId = nextId();
    const nameGoId = nextId(), nameRtId = nextId(), nameCrId = nextId(), nameTmpId = nextId();
    const typeGoId = nextId(), typeRtId = nextId(), typeCrId = nextId(), typeTmpId = nextId();

    const statRows = [];
    for (let i = 0; i < 4; i++) {
        statRows.push({
            goId: nextId(), rtId: nextId(), crId: nextId(), imgId: nextId(),
            lblGoId: nextId(), lblRtId: nextId(), lblCrId: nextId(), lblTmpId: nextId(),
            valGoId: nextId(), valRtId: nextId(), valCrId: nextId(), valTmpId: nextId()
        });
    }

    const scrollTrackGoId = nextId(), scrollTrackRtId = nextId(), scrollTrackCrId = nextId(), scrollTrackImgId = nextId();
    const scrollThumbGoId = nextId(), scrollThumbRtId = nextId(), scrollThumbCrId = nextId(), scrollThumbImgId = nextId();

    const rateLblGoId = nextId(), rateLblRtId = nextId(), rateLblCrId = nextId(), rateLblTmpId = nextId();
    const rateValGoId = nextId(), rateValRtId = nextId(), rateValCrId = nextId(), rateValTmpId = nextId();
    const rateBarBgGoId = nextId(), rateBarBgRtId = nextId(), rateBarBgCrId = nextId(), rateBarBgImgId = nextId(), rateBarSliderId = nextId();
    const rateBarFillGoId = nextId(), rateBarFillRtId = nextId(), rateBarFillCrId = nextId(), rateBarFillImgId = nextId();

    const costLblGoId = nextId(), costLblRtId = nextId(), costLblCrId = nextId(), costLblTmpId = nextId();
    const costValGoId = nextId(), costValRtId = nextId(), costValCrId = nextId(), costValTmpId = nextId();

    const refineBtnGoId = nextId(), refineBtnRtId = nextId(), refineBtnCrId = nextId(), refineBtnImgId = nextId(), refineBtnCompId = nextId();
    const refineBtnTxtGoId = nextId(), refineBtnTxtRtId = nextId(), refineBtnTxtCrId = nextId(), refineBtnTxtTmpId = nextId();

    let out = `%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n`;

    function addGo(id, name, comps, active = 1) {
        out += `--- !u!1 &${id}\nGameObject:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  serializedVersion: 6\n  m_Component:\n`;
        comps.forEach(c => out += `  - component: {fileID: ${c}}\n`);
        out += `  m_Layer: 5\n  m_Name: ${name}\n  m_TagString: Untagged\n  m_Icon: {fileID: 0}\n  m_NavMeshLayer: 0\n  m_StaticEditorFlags: 0\n  m_IsActive: ${active}\n`;
    }

    function addRt(id, goId, fatherId, children, min, max, pivot, pos, size) {
        out += `--- !u!224 &${id}\nRectTransform:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: ${goId}}\n  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}\n  m_LocalPosition: {x: 0, y: 0, z: 0}\n  m_LocalScale: {x: 1, y: 1, z: 1}\n  m_ConstrainProportionsScale: 0\n  m_Children:\n`;
        children.forEach(c => out += `  - {fileID: ${c}}\n`);
        out += `  m_Father: {fileID: ${fatherId}}\n  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}\n  m_AnchorMin: {x: ${min.x}, y: ${min.y}}\n  m_AnchorMax: {x: ${max.x}, y: ${max.y}}\n  m_AnchoredPosition: {x: ${pos.x}, y: ${pos.y}}\n  m_SizeDelta: {x: ${size.x}, y: ${size.y}}\n  m_Pivot: {x: ${pivot.x}, y: ${pivot.y}}\n`;
    }

    function addCr(id, goId) {
        out += `--- !u!222 &${id}\nCanvasRenderer:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: ${goId}}\n  m_CullTransparentMesh: 1\n`;
    }

    function addImg(id, goId, spriteGuid, internalId = '21300000', type = 0, color = {r:1,g:1,b:1,a:1}, fillMethod = 0, fillAmount = 1, enabled = 1, preserveAspect = 1) {
        const sprRef = spriteGuid ? `{fileID: ${internalId}, guid: ${spriteGuid}, type: 3}` : `{fileID: 0}`;
        out += `--- !u!114 &${id}\nMonoBehaviour:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: ${goId}}\n  m_Enabled: ${enabled}\n  m_EditorHideFlags: 0\n  m_Script: {fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}\n  m_Name: \n  m_EditorClassIdentifier: \n  m_Material: {fileID: 0}\n  m_Color: {r: ${color.r}, g: ${color.g}, b: ${color.b}, a: ${color.a}}\n  m_RaycastTarget: 1\n  m_RaycastPadding: {x: 0, y: 0, z: 0, w: 0}\n  m_Maskable: 1\n  m_OnCullStateChanged:\n    m_PersistentCalls:\n      m_Calls: []\n  m_Sprite: ${sprRef}\n  m_Type: ${type}\n  m_PreserveAspect: ${preserveAspect}\n  m_FillCenter: 1\n  m_FillMethod: ${fillMethod}\n  m_FillAmount: ${fillAmount}\n  m_FillClockwise: 1\n  m_FillOrigin: 0\n  m_UseSpriteMesh: 0\n  m_PixelsPerUnitMultiplier: 1\n`;
    }

    function addTmp(id, goId, text, fontSize, fontGuid, alignment = 514, color = {r:1,g:1,b:1,a:1}, wrap = 0, charSpacing = 0, overflow = 0) {
        let hAlign = 1;
        let vAlign = 512;
        if (alignment === 514) { hAlign = 2; vAlign = 512; }
        else if (alignment === 513) { hAlign = 1; vAlign = 512; }
        else if (alignment === 516) { hAlign = 4; vAlign = 512; }
        else if (alignment === 257) { hAlign = 1; vAlign = 256; }
        else if (alignment === 258) { hAlign = 4; vAlign = 256; }
        const fontRef = `{fileID: 11400000, guid: ${fontGuid}, type: 2}`;
        out += `--- !u!114 &${id}\nMonoBehaviour:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: ${goId}}\n  m_Enabled: 1\n  m_EditorHideFlags: 0\n  m_Script: {fileID: 11500000, guid: f4688fdb7df04437aeb418b961361dc5, type: 3}\n  m_Name: \n  m_EditorClassIdentifier: \n  m_Material: {fileID: 0}\n  m_Color: {r: ${color.r}, g: ${color.g}, b: ${color.b}, a: ${color.a}}\n  m_RaycastTarget: 0\n  m_RaycastPadding: {x: 0, y: 0, z: 0, w: 0}\n  m_Maskable: 1\n  m_OnCullStateChanged:\n    m_PersistentCalls:\n      m_Calls: []\n  m_text: "${text}"\n  m_isRightToLeft: 0\n  m_fontAsset: ${fontRef}\n  m_sharedMaterial: {fileID: 0}\n  m_fontSharedMaterials: []\n  m_fontMaterial: {fileID: 0}\n  m_fontMaterials: []\n  m_fontColor32:\n    serializedVersion: 2\n    rgba: 4294967295\n  m_fontColor: {r: ${color.r}, g: ${color.g}, b: ${color.b}, a: ${color.a}}\n  m_enableVertexGradient: 0\n  m_colorMode: 3\n  m_fontSize: ${fontSize}\n  m_fontSizeBase: ${fontSize}\n  m_fontWeight: 400\n  m_enableAutoSizing: 0\n  m_fontSizeMin: 18\n  m_fontSizeMax: 72\n  m_fontStyle: 0\n  m_HorizontalAlignment: ${hAlign}\n  m_VerticalAlignment: ${vAlign}\n  m_textAlignment: 65535\n  m_characterSpacing: ${charSpacing}\n  m_wordSpacing: 0\n  m_lineSpacing: 0\n  m_lineSpacingMax: 0\n  m_paragraphSpacing: 0\n  m_charWidthMaxAdj: 0\n  m_enableWordWrapping: ${wrap}\n  m_wordWrappingRatios: 0.4\n  m_overflowMode: ${overflow}\n  m_linkedTextComponent: {fileID: 0}\n  parentLinkedComponent: {fileID: 0}\n  m_enableKerning: 1\n  m_enableExtraPadding: 0\n  checkPaddingRequired: 0\n  m_isRichText: 1\n  m_parseCtrlCharacters: 1\n  m_isOrthographic: 1\n  m_isCullingEnabled: 0\n  m_horizontalMapping: 0\n  m_verticalMapping: 0\n  m_uvLineOffset: 0\n  m_geometrySortingOrder: 0\n  m_IsTextObjectScaleStatic: 0\n  m_VertexBufferAutoSizeReduction: 0\n  m_useMaxVisibleDescender: 1\n  m_pageToDisplay: 1\n  m_margin: {x: 0, y: 0, z: 0, w: 0}\n  m_isUsingLegacyAnimationComponent: 0\n  m_isVolumetricText: 0\n`;
    }

    function addBtn(id, goId, targetGraphicId) {
        out += `--- !u!114 &${id}\nMonoBehaviour:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: ${goId}}\n  m_Enabled: 1\n  m_EditorHideFlags: 0\n  m_Script: {fileID: 11500000, guid: 4e29b1a8efbd4b44bb3f3716e73f07ff, type: 3}\n  m_Name: \n  m_EditorClassIdentifier: \n  m_Navigation:\n    m_Mode: 3\n    m_WrapAround: 0\n    m_SelectOnUp: {fileID: 0}\n    m_SelectOnDown: {fileID: 0}\n    m_SelectOnLeft: {fileID: 0}\n    m_SelectOnRight: {fileID: 0}\n  m_Transition: 1\n  m_Colors:\n    m_NormalColor: {r: 1, g: 1, b: 1, a: 1}\n    m_HighlightedColor: {r: 0.96, g: 0.96, b: 0.96, a: 1}\n    m_PressedColor: {r: 0.78, g: 0.78, b: 0.78, a: 1}\n    m_SelectedColor: {r: 0.96, g: 0.96, b: 0.96, a: 1}\n    m_DisabledColor: {r: 0.78, g: 0.78, b: 0.78, a: 0.5}\n    m_ColorMultiplier: 1\n    m_FadeDuration: 0.1\n  m_SpriteState:\n    m_HighlightedSprite: {fileID: 0}\n    m_PressedSprite: {fileID: 0}\n    m_SelectedSprite: {fileID: 0}\n    m_DisabledSprite: {fileID: 0}\n  m_AnimationTriggers:\n    m_NormalTrigger: Normal\n    m_HighlightedTrigger: Highlighted\n    m_PressedTrigger: Pressed\n    m_SelectedTrigger: Selected\n    m_DisabledTrigger: Disabled\n  m_Interactable: 1\n  m_TargetGraphic: {fileID: ${targetGraphicId}}\n  m_OnClick:\n    m_PersistentCalls:\n      m_Calls: []\n`;
    }

    function addSlider(id, goId, targetGraphicId, fillRectId, value = 0.65) {
        out += `--- !u!114 &${id}\nMonoBehaviour:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: ${goId}}\n  m_Enabled: 1\n  m_EditorHideFlags: 0\n  m_Script: {fileID: 11500000, guid: 67db9e8f0e2ae9c40bc1e2b64352a6b4, type: 3}\n  m_Name: \n  m_EditorClassIdentifier: UnityEngine.UI::UnityEngine.UI.Slider\n  m_Navigation:\n    m_Mode: 3\n    m_WrapAround: 0\n    m_SelectOnUp: {fileID: 0}\n    m_SelectOnDown: {fileID: 0}\n    m_SelectOnLeft: {fileID: 0}\n    m_SelectOnRight: {fileID: 0}\n  m_Transition: 0\n  m_Colors:\n    m_NormalColor: {r: 1, g: 1, b: 1, a: 1}\n    m_HighlightedColor: {r: 0.96, g: 0.96, b: 0.96, a: 1}\n    m_PressedColor: {r: 0.78, g: 0.78, b: 0.78, a: 1}\n    m_SelectedColor: {r: 0.96, g: 0.96, b: 0.96, a: 1}\n    m_DisabledColor: {r: 0.78, g: 0.78, b: 0.78, a: 0.5}\n    m_ColorMultiplier: 1\n    m_FadeDuration: 0.1\n  m_SpriteState:\n    m_HighlightedSprite: {fileID: 0}\n    m_PressedSprite: {fileID: 0}\n    m_SelectedSprite: {fileID: 0}\n    m_DisabledSprite: {fileID: 0}\n  m_AnimationTriggers:\n    m_NormalTrigger: Normal\n    m_HighlightedTrigger: Highlighted\n    m_PressedTrigger: Pressed\n    m_SelectedTrigger: Selected\n    m_DisabledTrigger: Disabled\n  m_Interactable: 0\n  m_TargetGraphic: {fileID: ${targetGraphicId}}\n  m_FillRect: {fileID: ${fillRectId}}\n  m_HandleRect: {fileID: 0}\n  m_Direction: 0\n  m_MinValue: 0\n  m_MaxValue: 1\n  m_WholeNumbers: 0\n  m_Value: ${value}\n  m_OnValueChanged:\n    m_PersistentCalls:\n      m_Calls: []\n`;
    }

    // Direct root children
    const rootChildren = [
        bgRtId, tagRtId, closeBtnRtId,
        levelBadgeRtId, dialRtId,
        matSlots[0].rtId, matSlots[1].rtId, matSlots[2].rtId,
        rarityBadgeRtId, nameRtId, typeRtId,
        statRows[0].rtId, statRows[1].rtId, statRows[2].rtId, statRows[3].rtId,
        scrollTrackRtId,
        rateLblRtId, rateValRtId, rateBarBgRtId,
        costLblRtId, costValRtId,
        refineBtnRtId
    ];

    addGo(rootGoId, 'Refinement Window', [rootRtId, rootCrId, rootCgId, rootWinId]);
    out += `--- !u!225 &${rootCgId}\nCanvasGroup:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: ${rootGoId}}\n  m_Enabled: 1\n  m_Alpha: 1\n  m_Interactable: 1\n  m_BlocksRaycasts: 1\n  m_IgnoreParentGroups: 0\n`;
    addCr(rootCrId, rootGoId);
    addRt(rootRtId, rootGoId, 0, rootChildren, {x:0.5,y:0.5}, {x:0.5,y:0.5}, {x:0.5,y:0.5}, {x:0,y:0}, {x:1083,y:705});

    // Window component
    out += `--- !u!114 &${rootWinId}\nMonoBehaviour:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: ${rootGoId}}\n  m_Enabled: 1\n  m_EditorHideFlags: 0\n  m_Script: {fileID: 11500000, guid: ba0588ec730b4ae48e782d4171bd71a1, type: 3}\n  m_Name: \n  m_EditorClassIdentifier: \n  _destroyOnClose: 0\n  _isDraggable: 1\n  _headerRect: {fileID: ${tagRtId}}\n  _clampToCanvas: 0\n  _headerTitle: {fileID: ${titleTmpId}}\n  _itemNameText: {fileID: ${nameTmpId}}\n  _itemTypeText: {fileID: ${typeTmpId}}\n  _rarityText: {fileID: ${rarityTxtTmpId}}\n  _rarityBackground: {fileID: ${rarityBadgeImgId}}\n  _levelText: {fileID: ${levelTxtTmpId}}\n  _itemIcon: {fileID: ${itemIconImgId}}\n  _sunburstDial: {fileID: ${dialImgId}}\n  _successRateText: {fileID: ${rateValTmpId}}\n  _successRateBar: {fileID: ${rateBarFillImgId}}\n  _costText: {fileID: ${costValTmpId}}\n  _refineButton: {fileID: ${refineBtnCompId}}\n  _buttonText: {fileID: ${refineBtnTxtTmpId}}\n  _closeButton: {fileID: ${closeBtnCompId}}\n  _statContainer: {fileID: 0}\n  _statRowPrefab: {fileID: 0}\n  _statLabelList:\n`;
    statRows.forEach(r => out += `  - {fileID: ${r.lblTmpId}}\n`);
    out += `  _statValueList:\n`;
    statRows.forEach(r => out += `  - {fileID: ${r.valTmpId}}\n`);
    out += `  _materialIcons:\n`;
    matSlots.forEach(m => out += `  - {fileID: ${m.imgId}}\n`);
    out += `  _materialCountTexts: []\n`;

    // Background
    addGo(bgGoId, 'Background', [bgRtId, bgCrId, bgImgId]);
    addCr(bgCrId, bgGoId);
    addRt(bgRtId, bgGoId, rootRtId, [], {x:0,y:0}, {x:1,y:1}, {x:0.5,y:0.5}, {x:0,y:0}, {x:0,y:0});
    addImg(bgImgId, bgGoId, SPRITE_BG, '21300000', 0, {r:1,g:1,b:1,a:1}, 0, 1, 1, 0);

    // Header Title Tag: x: 20, y: 23, w: 252, h: 40 using project rounded.png
    addGo(tagGoId, 'Title Tag', [tagRtId, tagCrId, tagImgId]);
    addCr(tagCrId, tagGoId);
    addRt(tagRtId, tagGoId, rootRtId, [starRtId, titleRtId], {x:0,y:1}, {x:0,y:1}, {x:0,y:1}, {x:20,y:-23}, {x:252,y:40});
    addImg(tagImgId, tagGoId, SPRITE_ROUNDED, '21300000', 1, {r:1,g:1,b:1,a:1}, 0, 1, 1, 0);

    addGo(starGoId, 'Star Icon', [starRtId, starCrId, starImgId]);
    addCr(starCrId, starGoId);
    addRt(starRtId, starGoId, tagRtId, [], {x:0,y:1}, {x:0,y:1}, {x:0,y:1}, {x:10,y:-4}, {x:32,y:32});
    addImg(starImgId, starGoId, SPRITE_TAG_STAR, '21300000', 0, {r:0,g:0,b:0,a:1});

    addGo(titleGoId, 'Title Text', [titleRtId, titleCrId, titleTmpId]);
    addCr(titleCrId, titleGoId);
    addRt(titleRtId, titleGoId, tagRtId, [], {x:0,y:1}, {x:0,y:1}, {x:0,y:1}, {x:56,y:-8}, {x:191,y:26});
    addTmp(titleTmpId, titleGoId, 'FORGE MASTERY', 24, KANIT_BOLD, 513, {r:0,g:0,b:0,a:1});

    // Close Button: x: 1025, y: 32, w: 24, h: 24
    addGo(closeBtnGoId, 'Close Button', [closeBtnRtId, closeBtnCrId, closeBtnImgId, closeBtnCompId]);
    addCr(closeBtnCrId, closeBtnGoId);
    addRt(closeBtnRtId, closeBtnGoId, rootRtId, [], {x:0,y:1}, {x:0,y:1}, {x:0,y:1}, {x:1025,y:-32}, {x:24,y:24});
    addImg(closeBtnImgId, closeBtnGoId, SPRITE_CLOSE, '21300000', 0);
    addBtn(closeBtnCompId, closeBtnGoId, closeBtnImgId);

    // Level Badge: x: 20, y: 100, w: 200, h: 44
    addGo(levelBadgeGoId, 'Level Badge', [levelBadgeRtId, levelBadgeCrId, levelBadgeImgId]);
    addCr(levelBadgeCrId, levelBadgeGoId);
    addRt(levelBadgeRtId, levelBadgeGoId, rootRtId, [levelTxtRtId], {x:0,y:1}, {x:0,y:1}, {x:0,y:1}, {x:20,y:-100}, {x:200,y:44});
    addImg(levelBadgeImgId, levelBadgeGoId, SPRITE_LEVEL_FRAME, '21300000', 0, {r:1,g:1,b:1,a:1}, 0, 1, 1, 0);

    addGo(levelTxtGoId, 'Level Text', [levelTxtRtId, levelTxtCrId, levelTxtTmpId]);
    addCr(levelTxtCrId, levelTxtGoId);
    addRt(levelTxtRtId, levelTxtGoId, levelBadgeRtId, [], {x:0,y:0.5}, {x:0,y:0.5}, {x:0,y:0.5}, {x:79,y:0}, {x:100,y:34});
    addTmp(levelTxtTmpId, levelTxtGoId, 'Lv.12<size=75%><color=#737373>/20</color></size>', 28, NORMAL_FONT, 513, {r:1,g:1,b:1,a:1}, 0);

    // Sunburst Dial: x: 101, y: 161, w: 390, h: 390
    addGo(dialGoId, 'Sunburst Dial', [dialRtId, dialCrId, dialImgId]);
    addCr(dialCrId, dialGoId);
    addRt(dialRtId, dialGoId, rootRtId, [centerSlotRtId], {x:0,y:1}, {x:0,y:1}, {x:0,y:1}, {x:101,y:-161}, {x:390,y:390});
    addImg(dialImgId, dialGoId, SPRITE_DIAL, '21300000', 0);

    // Center Item Slot: centered in Dial, size 85x85
    addGo(centerSlotGoId, 'Center Item Slot', [centerSlotRtId, centerSlotCrId, centerSlotImgId]);
    addCr(centerSlotCrId, centerSlotGoId);
    addRt(centerSlotRtId, centerSlotGoId, dialRtId, [itemIconRtId], {x:0.5,y:0.5}, {x:0.5,y:0.5}, {x:0.5,y:0.5}, {x:0,y:0}, {x:85,y:85});
    addImg(centerSlotImgId, centerSlotGoId, SPRITE_CENTER_SLOT, '21300000', 0);

    addGo(itemIconGoId, 'Item Icon', [itemIconRtId, itemIconCrId, itemIconImgId]);
    addCr(itemIconCrId, itemIconGoId);
    addRt(itemIconRtId, itemIconGoId, centerSlotRtId, [], {x:0.5,y:0.5}, {x:0.5,y:0.5}, {x:0.5,y:0.5}, {x:0,y:0}, {x:64,y:64});
    addImg(itemIconImgId, itemIconGoId, '', '0', 0, {r:1,g:1,b:1,a:1}, 0, 1, 0);

    // Material Slots: x: 163, 257, 351, y: 575, w: 79, h: 79 (Matching Figma Ground Truth)
    const matPositions = [{x:163,y:575}, {x:257,y:573}, {x:351,y:575}];
    const matGuids = [SPRITE_SLOT_RED, SPRITE_SLOT_BLUE, SPRITE_SLOT_BLUE];
    matSlots.forEach((m, idx) => {
        addGo(m.goId, `Material Slot ${idx + 1}`, [m.rtId, m.crId, m.imgId]);
        addCr(m.crId, m.goId);
        addRt(m.rtId, m.goId, rootRtId, [], {x:0,y:1}, {x:0,y:1}, {x:0,y:1}, {x:matPositions[idx].x,y:-matPositions[idx].y}, {x:79,y:79});
        addImg(m.imgId, m.goId, matGuids[idx], '21300000', 0);
    });

    // Right Content
    // COMMON badge: x: 625, y: 38, w: 131, h: 35 using project rounded.png with dynamic blue tint
    addGo(rarityBadgeGoId, 'Rarity Badge', [rarityBadgeRtId, rarityBadgeCrId, rarityBadgeImgId]);
    addCr(rarityBadgeCrId, rarityBadgeGoId);
    addRt(rarityBadgeRtId, rarityBadgeGoId, rootRtId, [rarityTxtRtId], {x:0,y:1}, {x:0,y:1}, {x:0,y:1}, {x:625,y:-38}, {x:131,y:35});
    addImg(rarityBadgeImgId, rarityBadgeGoId, SPRITE_ROUNDED, '21300000', 1, {r:0,g:0.58245,b:1,a:1}, 0, 1, 1, 0);

    addGo(rarityTxtGoId, 'Rarity Text', [rarityTxtRtId, rarityTxtCrId, rarityTxtTmpId]);
    addCr(rarityTxtCrId, rarityTxtGoId);
    addRt(rarityTxtRtId, rarityTxtGoId, rarityBadgeRtId, [], {x:0.5,y:0.5}, {x:0.5,y:0.5}, {x:0.5,y:0.5}, {x:0,y:0}, {x:110,y:24});
    addTmp(rarityTxtTmpId, rarityTxtGoId, 'COMMON', 18, DEMIBOLD_FONT, 514, {r:1,g:1,b:1,a:1}, 0, 0);

    // Item Name: x: 625, y: 94, w: 404, h: 34
    addGo(nameGoId, 'Item Name', [nameRtId, nameCrId, nameTmpId]);
    addCr(nameCrId, nameGoId);
    addRt(nameRtId, nameGoId, rootRtId, [], {x:0,y:1}, {x:0,y:1}, {x:0,y:1}, {x:625,y:-94}, {x:404,y:34});
    addTmp(nameTmpId, nameGoId, 'Dragon Tooth Pen...', 28, DEMIBOLD_FONT, 257, {r:0.96,g:0.96,b:0.96,a:1}, 0, 2, 0);

    // Item Type: x: 625, y: 132, w: 215, h: 24
    addGo(typeGoId, 'Item Type', [typeRtId, typeCrId, typeTmpId]);
    addCr(typeCrId, typeGoId);
    addRt(typeRtId, typeGoId, rootRtId, [], {x:0,y:1}, {x:0,y:1}, {x:0,y:1}, {x:625,y:-132}, {x:215,y:24});
    addTmp(typeTmpId, typeGoId, 'TALISMAN', 16, NORMAL_FONT, 257, {r:0.45,g:0.45,b:0.45,a:1});

    // Stat Rows: y: 201, 266, 331, 396, w: 400, h: 50 using project rounded.png
    const statY = [201, 266, 331, 396];
    statRows.forEach((r, idx) => {
        addGo(r.goId, `Stat Row ${idx + 1}`, [r.rtId, r.crId, r.imgId]);
        addCr(r.crId, r.goId);
        addRt(r.rtId, r.goId, rootRtId, [r.lblRtId, r.valRtId], {x:0,y:1}, {x:0,y:1}, {x:0,y:1}, {x:625,y:-statY[idx]}, {x:400,y:50});
        addImg(r.imgId, r.goId, SPRITE_ROUNDED, '21300000', 1, {r:1,g:1,b:1,a:1}, 0, 1, 1, 0);

        addGo(r.lblGoId, 'Label', [r.lblRtId, r.lblCrId, r.lblTmpId]);
        addCr(r.lblCrId, r.lblGoId);
        addRt(r.lblRtId, r.lblGoId, r.rtId, [], {x:0,y:0.5}, {x:0,y:0.5}, {x:0,y:0.5}, {x:15,y:0}, {x:140,y:26});
        addTmp(r.lblTmpId, r.lblGoId, 'max health', 20, DEMIBOLD_FONT, 513, {r:0,g:0,b:0,a:1});

        addGo(r.valGoId, 'Value', [r.valRtId, r.valCrId, r.valTmpId]);
        addCr(r.valCrId, r.valGoId);
        addRt(r.valRtId, r.valGoId, r.rtId, [], {x:1,y:0.5}, {x:1,y:0.5}, {x:1,y:0.5}, {x:-12,y:0}, {x:160,y:26});
        addTmp(r.valTmpId, r.valGoId, '100 <color=#0062FF>+(500)</color>', 20, DEMIBOLD_FONT, 516, {r:0,g:0,b:0,a:1});
    });

    // Scrollbar: track x: 1046, y: 201, w: 6, h: 247
    addGo(scrollTrackGoId, 'Scroll Track', [scrollTrackRtId, scrollTrackCrId, scrollTrackImgId]);
    addCr(scrollTrackCrId, scrollTrackGoId);
    addRt(scrollTrackRtId, scrollTrackGoId, rootRtId, [scrollThumbRtId], {x:0,y:1}, {x:0,y:1}, {x:0,y:1}, {x:1046,y:-201}, {x:6,y:247});
    addImg(scrollTrackImgId, scrollTrackGoId, SPRITE_SCROLL_TRACK, '21300000', 0, {r:1,g:1,b:1,a:1}, 0, 1, 1, 0);

    addGo(scrollThumbGoId, 'Scroll Thumb', [scrollThumbRtId, scrollThumbCrId, scrollThumbImgId]);
    addCr(scrollThumbCrId, scrollThumbGoId);
    addRt(scrollThumbRtId, scrollThumbGoId, scrollTrackRtId, [], {x:0,y:1}, {x:1,y:1}, {x:0.5,y:1}, {x:0,y:0}, {x:0,y:66});
    addImg(scrollThumbImgId, scrollThumbGoId, SPRITE_SCROLL_THUMB, '21300000', 0, {r:1,g:1,b:1,a:1}, 0, 1, 1, 0);

    // Rate & Cost
    addGo(rateLblGoId, 'Rate Label', [rateLblRtId, rateLblCrId, rateLblTmpId]);
    addCr(rateLblCrId, rateLblGoId);
    addRt(rateLblRtId, rateLblGoId, rootRtId, [], {x:0,y:1}, {x:0,y:1}, {x:0,y:1}, {x:625,y:-476}, {x:250,y:18});
    addTmp(rateLblTmpId, rateLblGoId, 'SUCCESS PROBABILITY', 16, NORMAL_FONT, 257, {r:0.45,g:0.45,b:0.45,a:1});

    addGo(rateValGoId, 'Rate Value', [rateValRtId, rateValCrId, rateValTmpId]);
    addCr(rateValCrId, rateValGoId);
    addRt(rateValRtId, rateValGoId, rootRtId, [], {x:0,y:1}, {x:0,y:1}, {x:1,y:1}, {x:1020,y:-464}, {x:82,y:32});
    addTmp(rateValTmpId, rateValGoId, '65%', 28, DEMIBOLD_FONT, 258, {r:0.404,g:0.91,b:0.976,a:1});

    // Rate Bar with native Slider component
    addGo(rateBarBgGoId, 'Rate Bar Bg', [rateBarBgRtId, rateBarBgCrId, rateBarBgImgId, rateBarSliderId]);
    addCr(rateBarBgCrId, rateBarBgGoId);
    addRt(rateBarBgRtId, rateBarBgGoId, rootRtId, [rateBarFillRtId], {x:0,y:1}, {x:0,y:1}, {x:0,y:1}, {x:625,y:-516}, {x:399,y:11});
    addImg(rateBarBgImgId, rateBarBgGoId, SPRITE_PROG_TRACK, '21300000', 0, {r:1,g:1,b:1,a:1}, 0, 1, 1, 0);
    addSlider(rateBarSliderId, rateBarBgGoId, rateBarBgImgId, rateBarFillRtId, 0.65);

    addGo(rateBarFillGoId, 'Rate Bar Fill', [rateBarFillRtId, rateBarFillCrId, rateBarFillImgId]);
    addCr(rateBarFillCrId, rateBarFillGoId);
    addRt(rateBarFillRtId, rateBarFillGoId, rateBarBgRtId, [], {x:0,y:0}, {x:0,y:0}, {x:0,y:0}, {x:0,y:0}, {x:0,y:0});
    addImg(rateBarFillImgId, rateBarFillGoId, SPRITE_PROG_FILL, '21300000', 0, {r:1,g:1,b:1,a:1}, 0, 1, 1, 0);

    addGo(costLblGoId, 'Cost Label', [costLblRtId, costLblCrId, costLblTmpId]);
    addCr(costLblCrId, costLblGoId);
    addRt(costLblRtId, costLblGoId, rootRtId, [], {x:0,y:1}, {x:0,y:1}, {x:0,y:1}, {x:625,y:-550}, {x:200,y:18});
    addTmp(costLblTmpId, costLblGoId, 'CONSUMPTION', 16, NORMAL_FONT, 257, {r:0.45,g:0.45,b:0.45,a:1});

    addGo(costValGoId, 'Cost Value', [costValRtId, costValCrId, costValTmpId]);
    addCr(costValCrId, costValGoId);
    addRt(costValRtId, costValGoId, rootRtId, [], {x:0,y:1}, {x:0,y:1}, {x:1,y:1}, {x:1020,y:-545}, {x:180,y:24});
    addTmp(costValTmpId, costValGoId, '5,000 CREDITS', 20, NORMAL_FONT, 258, {r:0.96,g:0.96,b:0.96,a:1});

    // Reusable Primary Button with separate Text child: pos x: 735, y: -610, w: 290, h: 50
    addGo(refineBtnGoId, 'Refine Button', [refineBtnRtId, refineBtnCrId, refineBtnImgId, refineBtnCompId]);
    addCr(refineBtnCrId, refineBtnGoId);
    addRt(refineBtnRtId, refineBtnGoId, rootRtId, [refineBtnTxtRtId], {x:0,y:1}, {x:0,y:1}, {x:0,y:1}, {x:735,y:-610}, {x:290,y:50});
    addImg(refineBtnImgId, refineBtnGoId, SPRITE_BTN_PRIMARY, '21300000', 0, {r:1,g:1,b:1,a:1}, 0, 1, 1, 0);
    addBtn(refineBtnCompId, refineBtnGoId, refineBtnImgId);

    addGo(refineBtnTxtGoId, 'Button Text', [refineBtnTxtRtId, refineBtnTxtCrId, refineBtnTxtTmpId]);
    addCr(refineBtnTxtCrId, refineBtnTxtGoId);
    addRt(refineBtnTxtRtId, refineBtnTxtGoId, refineBtnRtId, [], {x:0,y:0.5}, {x:0,y:0.5}, {x:0,y:0.5}, {x:125,y:0}, {x:115,y:30});
    addTmp(refineBtnTxtTmpId, refineBtnTxtGoId, 'Refinement', 20, NOTO_SEMIBOLD, 514, {r:0.22,g:0.173,b:0.173,a:1}, 0);

    return { content: out, rootId: rootGoId };
}

const r = buildRefinementPerfect();
fs.writeFileSync('Assets/Prefab/Game Page/Refinement Window.prefab', r.content);
console.log('Successfully synchronized Refinement Window.prefab with Dev manual patterns');
