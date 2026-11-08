const fs = require('fs');

function buildSocketPerfect() {
    let idCounter = 2000000;
    function nextId() { return (idCounter += 100).toString(); }

    const rootGoId = '8200100000000000001';
    const rootRtId = nextId(), rootCrId = nextId(), rootCgId = nextId(), rootWinId = nextId();
    const bgGoId = nextId(), bgRtId = nextId(), bgCrId = nextId(), bgImgId = nextId();

    // Fonts
    const DEMIBOLD_FONT = 'ed472c2881e9b894fb40179e7c9b4332'; // Novecentosanswide-DemiBold SDF
    const NORMAL_FONT = 'a66aeea0855f6544883cbd79dc661f3a';   // Novecentosanswide-Normal SDF
    const KANIT_BOLD = '0063cb4c629a50d499805b5a4274846a';    // Kanit-Bold SDF
    const NOTO_SEMIBOLD = '57d8cea201ff3e04b8aef8e01a97988a';  // NotoSansThai-SemiBold SDF

    // Sprites
    const SPRITE_BG = '9b8a1bc9e5013cad0608d7ed26b8afc8';          // UI_Socket_Background.png
    const SPRITE_ROUNDED = '9df5b6060bad4fb4b9b050483f64c324';     // Assets/Art/UI/Old/rounded.png (Standard 9-slice)
    const SPRITE_TAG_STAR = 'a1000000000000000000000000000010';    // tag_star_icon.png
    const SPRITE_CLOSE = 'a9d6579a209c58146a11b71d4368b9ee';       // Close icon
    const SPRITE_MANDALA = 'e87674ca24bb978f5209f15c7da1b94d';     // UI_Socket_Mandala.png
    const SPRITE_CENTER_SLOT = '1d0359e7f6e6484793b83e45f03de78d'; // UI_Item_Slot_Center.png
    const SPRITE_FRAG_SLOT = '48b5e1af025ba29469fbedc86d4a2af5';   // UI_Fragment_Slot.png
    const SPRITE_CIRCLE = '6251a4bfa3edd424ab73ffa1e08b9789';      // Circle.png
    const SPRITE_GEM_ICON = '5a10f011d88bbcc11223344556677881';    // UI_Fragment_Gem_Icon.png
    const SPRITE_LINK_ICON = '5a20f022d88bbcc11223344556677882';   // UI_Fragment_Link_Icon.png
    const SPRITE_BTN_PRIMARY = 'a1000000000000000000000000000001'; // btn_primary_clean_290x50.png

    // Header
    const tagGoId = nextId(), tagRtId = nextId(), tagCrId = nextId(), tagImgId = nextId();
    const starGoId = nextId(), starRtId = nextId(), starCrId = nextId(), starImgId = nextId();
    const titleGoId = nextId(), titleRtId = nextId(), titleCrId = nextId(), titleTmpId = nextId();
    const closeBtnGoId = nextId(), closeBtnRtId = nextId(), closeBtnCrId = nextId(), closeBtnImgId = nextId(), closeBtnCompId = nextId();

    // Left container
    const dialGoId = nextId(), dialRtId = nextId(), dialCrId = nextId(), dialImgId = nextId();
    const centerSlotGoId = nextId(), centerSlotRtId = nextId(), centerSlotCrId = nextId(), centerSlotImgId = nextId();
    const itemIconGoId = nextId(), itemIconRtId = nextId(), itemIconCrId = nextId(), itemIconImgId = nextId();

    // Right container items
    const rarityBadgeGoId = nextId(), rarityBadgeRtId = nextId(), rarityBadgeCrId = nextId(), rarityBadgeImgId = nextId();
    const rarityTxtGoId = nextId(), rarityTxtRtId = nextId(), rarityTxtCrId = nextId(), rarityTxtTmpId = nextId();
    const nameGoId = nextId(), nameRtId = nextId(), nameCrId = nextId(), nameTmpId = nextId();
    const typeGoId = nextId(), typeRtId = nextId(), typeCrId = nextId(), typeTmpId = nextId();

    // 4 Sockets
    const sockets = [];
    const socketCoords = [
        { x: 703, y: 208, filled: true, iconSprite: SPRITE_GEM_ICON },
        { x: 862, y: 208, filled: false, iconSprite: '' },
        { x: 703, y: 353, filled: false, iconSprite: '' },
        { x: 862, y: 353, filled: true, iconSprite: SPRITE_LINK_ICON }
    ];

    for (let i = 0; i < 4; i++) {
        sockets.push({
            goId: nextId(), rtId: nextId(), crId: nextId(), imgId: nextId(), btnId: nextId(),
            circGoId: nextId(), circRtId: nextId(), circCrId: nextId(), circImgId: nextId(),
            iconGoId: nextId(), iconRtId: nextId(), iconCrId: nextId(), iconImgId: nextId(),
            emptyGoId: nextId(), emptyRtId: nextId(),
            hlGoId: nextId(), hlRtId: nextId(), hlCrId: nextId(), hlImgId: nextId()
        });
    }

    // Cost & Action Button
    const costLblGoId = nextId(), costLblRtId = nextId(), costLblCrId = nextId(), costLblTmpId = nextId();
    const costValGoId = nextId(), costValRtId = nextId(), costValCrId = nextId(), costValTmpId = nextId();

    const infuseBtnGoId = nextId(), infuseBtnRtId = nextId(), infuseBtnCrId = nextId(), infuseBtnImgId = nextId(), infuseBtnCompId = nextId();
    const infuseBtnTxtGoId = nextId(), infuseBtnTxtRtId = nextId(), infuseBtnTxtCrId = nextId(), infuseBtnTxtTmpId = nextId();

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

    // Direct root children
    const rootChildren = [
        bgRtId, tagRtId, closeBtnRtId,
        dialRtId,
        rarityBadgeRtId, nameRtId, typeRtId,
        sockets[0].rtId, sockets[1].rtId, sockets[2].rtId, sockets[3].rtId,
        costLblRtId, costValRtId,
        infuseBtnRtId
    ];

    addGo(rootGoId, 'Socket Window', [rootRtId, rootCrId, rootCgId, rootWinId]);
    out += `--- !u!225 &${rootCgId}\nCanvasGroup:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: ${rootGoId}}\n  m_Enabled: 1\n  m_Alpha: 1\n  m_Interactable: 1\n  m_BlocksRaycasts: 1\n  m_IgnoreParentGroups: 0\n`;
    addCr(rootCrId, rootGoId);
    addRt(rootRtId, rootGoId, 0, rootChildren, {x:0.5,y:0.5}, {x:0.5,y:0.5}, {x:0.5,y:0.5}, {x:0,y:0}, {x:1083,y:705});

    // SocketWindow MonoBehaviour component (guid: 2af0dc3312c8bf907a5d4383f5c1af6a)
    out += `--- !u!114 &${rootWinId}\nMonoBehaviour:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: ${rootGoId}}\n  m_Enabled: 1\n  m_EditorHideFlags: 0\n  m_Script: {fileID: 11500000, guid: 2af0dc3312c8bf907a5d4383f5c1af6a, type: 3}\n  m_Name: \n  m_EditorClassIdentifier: \n  _destroyOnClose: 0\n  _isDraggable: 1\n  _headerRect: {fileID: ${tagRtId}}\n  _clampToCanvas: 0\n  _headerTitle: {fileID: ${titleTmpId}}\n  _itemNameText: {fileID: ${nameTmpId}}\n  _itemTypeText: {fileID: ${typeTmpId}}\n  _rarityText: {fileID: ${rarityTxtTmpId}}\n  _itemIcon: {fileID: ${itemIconImgId}}\n  _costText: {fileID: ${costValTmpId}}\n  _infuseButton: {fileID: ${infuseBtnCompId}}\n  _buttonText: {fileID: ${infuseBtnTxtTmpId}}\n  _closeButton: {fileID: ${closeBtnCompId}}\n  _socketSlotButtons:\n`;
    sockets.forEach(s => out += `  - {fileID: ${s.btnId}}\n`);
    out += `  _socketSlotBackgrounds:\n`;
    sockets.forEach(s => out += `  - {fileID: ${s.imgId}}\n`);
    out += `  _socketSlotIcons:\n`;
    sockets.forEach(s => out += `  - {fileID: ${s.iconImgId}}\n`);
    out += `  _socketEmptyIndicators:\n`;
    sockets.forEach(s => out += `  - {fileID: ${s.emptyGoId}}\n`);
    out += `  _socketActiveHighlights:\n`;
    sockets.forEach(s => out += `  - {fileID: ${s.hlGoId}}\n`);

    // Background
    addGo(bgGoId, 'Background', [bgRtId, bgCrId, bgImgId]);
    addCr(bgCrId, bgGoId);
    addRt(bgRtId, bgGoId, rootRtId, [], {x:0,y:0}, {x:1,y:1}, {x:0.5,y:0.5}, {x:0,y:0}, {x:0,y:0});
    addImg(bgImgId, bgGoId, SPRITE_BG, '21300000', 0, {r:1,g:1,b:1,a:1}, 0, 1, 1, 0);

    // Header Title Tag: x: 17, y: 18, w: 266, h: 40 using project rounded.png
    addGo(tagGoId, 'Title Tag', [tagRtId, tagCrId, tagImgId]);
    addCr(tagCrId, tagGoId);
    addRt(tagRtId, tagGoId, rootRtId, [starRtId, titleRtId], {x:0,y:1}, {x:0,y:1}, {x:0,y:1}, {x:17,y:-18}, {x:266,y:40});
    addImg(tagImgId, tagGoId, SPRITE_ROUNDED, '21300000', 1, {r:1,g:1,b:1,a:1}, 0, 1, 1, 0);

    addGo(starGoId, 'Star Icon', [starRtId, starCrId, starImgId]);
    addCr(starCrId, starGoId);
    addRt(starRtId, starGoId, tagRtId, [], {x:0,y:1}, {x:0,y:1}, {x:0,y:1}, {x:10,y:-4}, {x:32,y:32});
    addImg(starImgId, starGoId, SPRITE_TAG_STAR, '21300000', 0, {r:0,g:0,b:0,a:1});

    addGo(titleGoId, 'Title Text', [titleRtId, titleCrId, titleTmpId]);
    addCr(titleCrId, titleGoId);
    addRt(titleRtId, titleGoId, tagRtId, [], {x:0,y:1}, {x:0,y:1}, {x:0,y:1}, {x:56,y:-8}, {x:204,y:26});
    addTmp(titleTmpId, titleGoId, 'SOCKET MASTERY', 24, KANIT_BOLD, 513, {r:0,g:0,b:0,a:1});

    // Close Button: x: 1022, y: 27, w: 24, h: 24
    addGo(closeBtnGoId, 'Close Button', [closeBtnRtId, closeBtnCrId, closeBtnImgId, closeBtnCompId]);
    addCr(closeBtnCrId, closeBtnGoId);
    addRt(closeBtnRtId, closeBtnGoId, rootRtId, [], {x:0,y:1}, {x:0,y:1}, {x:0,y:1}, {x:1022,y:-27}, {x:24,y:24});
    addImg(closeBtnImgId, closeBtnGoId, SPRITE_CLOSE, '21300000', 0);
    addBtn(closeBtnCompId, closeBtnGoId, closeBtnImgId);

    // Mandala Dial: center (286, 353), size 460x460 -> pos: (56, -123)
    addGo(dialGoId, 'Mandala Dial', [dialRtId, dialCrId, dialImgId]);
    addCr(dialCrId, dialGoId);
    addRt(dialRtId, dialGoId, rootRtId, [centerSlotRtId], {x:0,y:1}, {x:0,y:1}, {x:0,y:1}, {x:56,y:-123}, {x:460,y:460});
    addImg(dialImgId, dialGoId, SPRITE_MANDALA, '21300000', 0, {r:1,g:1,b:1,a:1}, 0, 1, 1, 1);

    // Center Item Slot: x: 247, y: 311, w: 79, h: 79 (rel to modal root)
    // Child of dial: center is (230, -230) in dial coordinates
    addGo(centerSlotGoId, 'Center Item Slot', [centerSlotRtId, centerSlotCrId, centerSlotImgId]);
    addCr(centerSlotCrId, centerSlotGoId);
    addRt(centerSlotRtId, centerSlotGoId, dialRtId, [itemIconRtId], {x:0.5,y:0.5}, {x:0.5,y:0.5}, {x:0.5,y:0.5}, {x:0,y:0}, {x:79,y:79});
    addImg(centerSlotImgId, centerSlotGoId, SPRITE_CENTER_SLOT, '21300000', 0, {r:1,g:1,b:1,a:1}, 0, 1, 1, 1);

    addGo(itemIconGoId, 'Item Icon', [itemIconRtId, itemIconCrId, itemIconImgId]);
    addCr(itemIconCrId, itemIconGoId);
    addRt(itemIconRtId, itemIconGoId, centerSlotRtId, [], {x:0.5,y:0.5}, {x:0.5,y:0.5}, {x:0.5,y:0.5}, {x:0,y:0}, {x:60,y:60});
    addImg(itemIconImgId, itemIconGoId, '', '0', 0, {r:1,g:1,b:1,a:1}, 0, 1, 0);

    // Right Content
    // COMMON badge: x: 622, y: 33, w: 131, h: 35 using project rounded.png with dynamic blue tint (#0048ff)
    addGo(rarityBadgeGoId, 'Rarity Badge', [rarityBadgeRtId, rarityBadgeCrId, rarityBadgeImgId]);
    addCr(rarityBadgeCrId, rarityBadgeGoId);
    addRt(rarityBadgeRtId, rarityBadgeGoId, rootRtId, [rarityTxtRtId], {x:0,y:1}, {x:0,y:1}, {x:0,y:1}, {x:622,y:-33}, {x:131,y:35});
    addImg(rarityBadgeImgId, rarityBadgeGoId, SPRITE_ROUNDED, '21300000', 1, {r:0,g:0.28235,b:1,a:1}, 0, 1, 1, 0);

    addGo(rarityTxtGoId, 'Rarity Text', [rarityTxtRtId, rarityTxtCrId, rarityTxtTmpId]);
    addCr(rarityTxtCrId, rarityTxtGoId);
    addRt(rarityTxtRtId, rarityTxtGoId, rarityBadgeRtId, [], {x:0.5,y:0.5}, {x:0.5,y:0.5}, {x:0.5,y:0.5}, {x:0,y:0}, {x:110,y:24});
    addTmp(rarityTxtTmpId, rarityTxtGoId, 'COMMON', 18, DEMIBOLD_FONT, 514, {r:1,g:1,b:1,a:1}, 0, 0);

    // Item Name: x: 622, y: 89, w: 404, h: 32
    addGo(nameGoId, 'Item Name', [nameRtId, nameCrId, nameTmpId]);
    addCr(nameCrId, nameGoId);
    addRt(nameRtId, nameGoId, rootRtId, [], {x:0,y:1}, {x:0,y:1}, {x:0,y:1}, {x:622,y:-89}, {x:404,y:32});
    addTmp(nameTmpId, nameGoId, 'Dragon Tooth Pen...', 28, DEMIBOLD_FONT, 257, {r:0.96,g:0.96,b:0.96,a:1}, 0, 2, 0);

    // Item Type: x: 622, y: 124, w: 215, h: 24
    addGo(typeGoId, 'Item Type', [typeRtId, typeCrId, typeTmpId]);
    addCr(typeCrId, typeGoId);
    addRt(typeRtId, typeGoId, rootRtId, [], {x:0,y:1}, {x:0,y:1}, {x:0,y:1}, {x:622,y:-124}, {x:215,y:24});
    addTmp(typeTmpId, typeGoId, 'TALISMAN', 16, NORMAL_FONT, 257, {r:0.45,g:0.45,b:0.45,a:1});

    // 4 Sockets (2x2 Grid)
    socketCoords.forEach((c, idx) => {
        const s = sockets[idx];
        const children = [s.circRtId, s.iconRtId, s.emptyRtId, s.hlRtId];
        addGo(s.goId, `Socket Slot ${idx + 1}`, [s.rtId, s.crId, s.imgId, s.btnId]);
        addCr(s.crId, s.goId);
        addRt(s.rtId, s.goId, rootRtId, children, {x:0,y:1}, {x:0,y:1}, {x:0,y:1}, {x:c.x,y:-c.y}, {x:90,y:90});
        addImg(s.imgId, s.goId, SPRITE_FRAG_SLOT, '21300000', 0, {r:1,g:1,b:1,a:1}, 0, 1, 1, 1);
        addBtn(s.btnId, s.goId, s.imgId);

        // Circular backing for filled sockets
        addGo(s.circGoId, 'Backing Circle', [s.circRtId, s.circCrId, s.circImgId], c.filled ? 1 : 0);
        addCr(s.circCrId, s.circGoId);
        addRt(s.circRtId, s.circGoId, s.rtId, [], {x:0.5,y:0.5}, {x:0.5,y:0.5}, {x:0.5,y:0.5}, {x:0,y:0}, {x:64,y:64});
        addImg(s.circImgId, s.circGoId, SPRITE_CIRCLE, '21300000', 0, {r:0.851,g:0.851,b:0.851,a:1}, 0, 1, c.filled ? 1 : 0, 1);

        // Icon inside socket
        addGo(s.iconGoId, 'Fragment Icon', [s.iconRtId, s.iconCrId, s.iconImgId], c.filled ? 1 : 0);
        addCr(s.iconCrId, s.iconGoId);
        addRt(s.iconRtId, s.iconGoId, s.rtId, [], {x:0.5,y:0.5}, {x:0.5,y:0.5}, {x:0.5,y:0.5}, {x:0,y:0}, {x:48,y:48});
        addImg(s.iconImgId, s.iconGoId, c.iconSprite, '21300000', 0, {r:1,g:1,b:1,a:1}, 0, 1, c.filled ? 1 : 0, 1);

        // Empty indicator (for slot 2 & 3)
        addGo(s.emptyGoId, 'Empty Indicator', [s.emptyRtId], c.filled ? 0 : 1);
        addRt(s.emptyRtId, s.emptyGoId, s.rtId, [], {x:0.5,y:0.5}, {x:0.5,y:0.5}, {x:0.5,y:0.5}, {x:0,y:0}, {x:24,y:24});

        // Active Highlight
        addGo(s.hlGoId, 'Active Highlight', [s.hlRtId, s.hlCrId, s.hlImgId], idx === 0 ? 1 : 0);
        addCr(s.hlCrId, s.hlGoId);
        addRt(s.hlRtId, s.hlGoId, s.rtId, [], {x:0.5,y:0.5}, {x:0.5,y:0.5}, {x:0.5,y:0.5}, {x:0,y:0}, {x:96,y:96});
        addImg(s.hlImgId, s.hlGoId, SPRITE_ROUNDED, '21300000', 1, {r:0.4,g:0.91,b:0.98,a:0.35}, 0, 1, idx === 0 ? 1 : 0, 0);
    });

    // Cost & Consumption
    addGo(costLblGoId, 'Cost Label', [costLblRtId, costLblCrId, costLblTmpId]);
    addCr(costLblCrId, costLblGoId);
    addRt(costLblRtId, costLblGoId, rootRtId, [], {x:0,y:1}, {x:0,y:1}, {x:0,y:1}, {x:622,y:-545}, {x:200,y:18});
    addTmp(costLblTmpId, costLblGoId, 'CONSUMPTION', 16, NORMAL_FONT, 257, {r:0.45,g:0.45,b:0.45,a:1});

    addGo(costValGoId, 'Cost Value', [costValRtId, costValCrId, costValTmpId]);
    addCr(costValCrId, costValGoId);
    addRt(costValRtId, costValGoId, rootRtId, [], {x:0,y:1}, {x:0,y:1}, {x:1,y:1}, {x:1020,y:-540}, {x:180,y:24});
    addTmp(costValTmpId, costValGoId, '5,000 CREDITS', 22, NORMAL_FONT, 258, {r:0.96,g:0.96,b:0.96,a:1});

    // Reusable Primary Button with separate Text child: pos x: 732, y: -605, w: 290, h: 50
    addGo(infuseBtnGoId, 'Infuse Button', [infuseBtnRtId, infuseBtnCrId, infuseBtnImgId, infuseBtnCompId]);
    addCr(infuseBtnCrId, infuseBtnGoId);
    addRt(infuseBtnRtId, infuseBtnGoId, rootRtId, [infuseBtnTxtRtId], {x:0,y:1}, {x:0,y:1}, {x:0,y:1}, {x:732,y:-605}, {x:290,y:50});
    addImg(infuseBtnImgId, infuseBtnGoId, SPRITE_BTN_PRIMARY, '21300000', 0, {r:1,g:1,b:1,a:1}, 0, 1, 1, 0);
    addBtn(infuseBtnCompId, infuseBtnGoId, infuseBtnImgId);

    addGo(infuseBtnTxtGoId, 'Button Text', [infuseBtnTxtRtId, infuseBtnTxtCrId, infuseBtnTxtTmpId]);
    addCr(infuseBtnTxtCrId, infuseBtnTxtGoId);
    addRt(infuseBtnTxtRtId, infuseBtnTxtGoId, infuseBtnRtId, [], {x:0,y:0.5}, {x:0,y:0.5}, {x:0,y:0.5}, {x:125,y:0}, {x:115,y:30});
    addTmp(infuseBtnTxtTmpId, infuseBtnTxtGoId, 'Infuse Socket', 20, NOTO_SEMIBOLD, 514, {r:0.22,g:0.173,b:0.173,a:1}, 0);

    return { content: out, rootId: rootGoId };
}

const r = buildSocketPerfect();
fs.writeFileSync('Assets/Prefab/Game Page/Socket Window.prefab', r.content);
console.log('Successfully generated Socket Window.prefab');
