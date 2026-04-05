Shader "Custom/UI/UILine"
{
    Properties
    {
        _MainTex        ("Texture",          2D)            = "white" {}
        _Color          ("Tint",             Color)         = (1,1,1,1)
        _AntialiasWidth ("Antialias Width",  Range(0,0.5))  = 0.05

        _StencilComp    ("Stencil Comparison",  Float) = 8
        _Stencil        ("Stencil ID",          Float) = 0
        _StencilOp      ("Stencil Operation",   Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask",  Float) = 255
        _ColorMask       ("Color Mask",         Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"           = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType"      = "Transparent"
            "PreviewType"     = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref       [_Stencil]
            Comp      [_StencilComp]
            Pass      [_StencilOp]
            ReadMask  [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull     Off
        Lighting Off
        ZWrite   Off
        ZTest    [unity_GUIZTestMode]
        Blend    SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "UILine"
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color  : COLOR;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos      : SV_POSITION;
                float4 color    : COLOR;
                float2 uv       : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4    _MainTex_ST;
            fixed4    _Color;
            float4    _TextureSampleAdd;
            float4    _ClipRect;
            float     _AntialiasWidth;

            v2f vert(appdata v)
            {
                v2f o;
                o.worldPos = v.vertex;
                o.pos      = UnityObjectToClipPos(v.vertex);
                o.uv       = TRANSFORM_TEX(v.uv, _MainTex);
                o.color    = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // uv.y : 0 = edge, 0.5 = center, 1 = edge
                float dist = abs(i.uv.y - 0.5) * 2.0;           // 0 center → 1 edge
                float aa   = smoothstep(1.0, 1.0 - _AntialiasWidth * 2.0, dist);

                fixed4 col = (tex2D(_MainTex, i.uv) + _TextureSampleAdd) * i.color;
                col.a     *= aa * UnityGet2DClipping(i.worldPos.xy, _ClipRect);

                clip(col.a - 0.001);
                return col;
            }
            ENDCG
        }
    }
}
