Shader "UI/DiamondSweep"
{
    Properties
    {
        _Progress ("Progress", Range(0.0, 1.0)) = 1.0
        [Enum(Center,0,Vertical,1,Horizontal,2,VerticalBoth,3,HorizontalBoth,4)] _Direction ("Direction", Int) = 0
        [Toggle] _Invert ("Invert", Int) = 0
        _GridSize ("Grid Size", Float) = 35.0
        _Spread ("Spread", Float) = 0.8
        _NoiseAmount ("Noise", Range(0.0, 1.0)) = 0.0
        _EdgeIntensity ("Edge Glow", Float) = 1.0
        _EdgeSoftness ("Edge Softness", Range(0.01, 1.0)) = 0.4
        _AspectRatio ("Aspect Ratio (Width/Height)", Float) = 1.0
        
        [Header(Sweep Colors)]
        _SweepColor1 ("Color 1", Color) = (1.0, 0.1, 0.4, 1.0)
        _SweepColor2 ("Color 2", Color) = (0.1, 0.8, 1.0, 1.0)
        _SweepColor3 ("Color 3", Color) = (0.8, 0.1, 1.0, 1.0)
        
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
        
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            float _Progress;
            int _Direction;
            int _Invert;
            float _GridSize;
            float _Spread;
            float _NoiseAmount;
            float _EdgeIntensity;
            float _EdgeSoftness;
            float _AspectRatio;
            float4 _ClipRect;
            float4 _SweepColor1;
            float4 _SweepColor2;
            float4 _SweepColor3;
            sampler2D _MainTex;
            float4 _TextureSampleAdd;
            float4 _MainTex_ST;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color;
                return OUT;
            }

            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 st = IN.texcoord - 0.5;
                st.x *= _AspectRatio;

                float c = 0.70710678;
                float s = 0.70710678;
                float2 rot_st = float2(st.x * c - st.y * s, st.x * s + st.y * c);

                float2 gv = frac(rot_st * _GridSize) - 0.5;
                float2 id = floor(rot_st * _GridSize);

                float2 cell_pos = (id + 0.5) / _GridSize;
                float2 unrot_pos = float2(cell_pos.x * c + cell_pos.y * s, -cell_pos.x * s + cell_pos.y * c);
                float2 norm_uv = float2(unrot_pos.x / _AspectRatio + 0.5, unrot_pos.y + 0.5);

                float gradient = 0.0;
                if (_Direction == 0) {
                    float max_dist = length(float2(0.5 * _AspectRatio, 0.5));
                    gradient = length(unrot_pos) / max_dist;
                } else if (_Direction == 1) {
                    gradient = 1.0 - norm_uv.y;
                } else if (_Direction == 2) {
                    gradient = 1.0 - norm_uv.x;
                } else if (_Direction == 3) {
                    // Vertical: ปัดจากบนและล่างมาพบกันกลาง
                    gradient = abs(norm_uv.y - 0.5) * 2.0;
                } else {
                    // Horizontal: ปัดจากซ้ายและขวามาพบกันกลาง
                    gradient = abs(norm_uv.x - 0.5) * 2.0;
                }

                if (_Invert == 1) {
                    gradient = 1.0 - gradient;
                }

                float n = random(id) * _NoiseAmount;
                gradient = clamp(gradient + (n - _NoiseAmount * 0.5), 0.0, 1.0);

                float phaseIn = clamp((1.0 - _Progress) * 4.0, 0.0, 1.0);
                float phaseOut = clamp((0.25 - _Progress) * 4.0, 0.0, 1.0);

                float s_in = clamp((phaseIn * (1.0 + _Spread)) - (gradient * _Spread), 0.0, 1.0);
                float s_out = clamp((phaseOut * (1.0 + _Spread)) - (gradient * _Spread), 0.0, 1.0);

                float fillAmount = clamp(s_in - s_out, 0.0, 1.0);

                if (fillAmount <= 0.001) {
                    return float4(0.0, 0.0, 0.0, 0.0);
                }

                float dist = max(abs(gv.x), abs(gv.y));
                // แก้เส้น Grid: ปรับ smoothstep ให้กว้างขึ้นเพื่อปิดรอยต่อ
                float shapeAlpha = 1.0 - smoothstep(0.5, 0.52, dist);
                float alpha = shapeAlpha * fillAmount;

                float3 col1 = _SweepColor1.rgb;
                float3 col2 = _SweepColor2.rgb;
                float3 col3 = _SweepColor3.rgb;
                float t = norm_uv.x * 0.5 + norm_uv.y * 0.5;
                float3 gradColor = lerp(lerp(col1, col2, t * 2.0), lerp(col2, col3, (t - 0.5) * 2.0), step(0.5, t));

                // ปรับความขนาน/ความหนาของขอบ Glow ตามค่า _EdgeSoftness
                float edgeCalc = smoothstep(1.0, 1.0 - _EdgeSoftness, fillAmount) * smoothstep(0.0, _EdgeSoftness, fillAmount);
                
                // Sample Texture
                fixed4 texColor = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                
                // Combine Texture with Sweep Glow
                float3 finalColor = texColor.rgb + (gradColor * edgeCalc * _EdgeIntensity);
                float finalAlpha = texColor.a * alpha;

                fixed4 color = float4(finalColor, finalAlpha);

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}