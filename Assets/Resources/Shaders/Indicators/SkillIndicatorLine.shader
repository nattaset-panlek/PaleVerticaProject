Shader "Custom/SkillIndicatorLine"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [Header(Textures)]
        [NoScaleOffset] _TailTex ("Tail Texture (Start)", 2D) = "white" {}
        [NoScaleOffset] _BodyTex ("Body Texture (Tiling)", 2D) = "white" {}
        [NoScaleOffset] _HeadTex ("Head Texture (End)", 2D) = "white" {}
        
        [Header(Colors)]
        _MainColor ("Main Color", Color) = (1, 1, 1, 1)
        
        [Header(Slicing)]
        _TailSize ("Tail Size (at Natural Width)", float) = 0.5
        _HeadSize ("Head Size (at Natural Width)", float) = 1.0
        _NaturalWidth ("Natural Width (Reference)", float) = 1.0
        _WorldSize ("World Size (Internal)", Vector) = (1, 1, 0, 0)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" }
        LOD 100
        ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float2 worldDims : TEXCOORD1;
            };

            sampler2D _TailTex;
            sampler2D _BodyTex;
            sampler2D _HeadTex;
            fixed4 _MainColor;

            float _TailSize;
            float _HeadSize;
            float _NaturalWidth;
            float4 _WorldSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.uv = v.uv;
                o.worldDims = _WorldSize.xy;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float totalWidth = i.worldDims.x;
                float totalHeight = i.worldDims.y;

                // Width ratio: how much wider than the reference
                float wRatio = totalWidth / max(_NaturalWidth, 0.0001);

                // Head/Tail scale proportionally with width
                // When wider -> Head/Tail get taller by the same ratio
                // This keeps their aspect ratio identical (no stretching)
                float tailH = _TailSize * wRatio;
                float headH = _HeadSize * wRatio;

                float worldY = uv.y * totalHeight;
                fixed4 col = fixed4(0,0,0,0);

                if (worldY < tailH)
                {
                    // Tail: Full width, height proportional to width
                    float v = worldY / max(tailH, 0.0001);
                    col = tex2D(_TailTex, float2(uv.x, v));
                }
                else if (worldY > (totalHeight - headH))
                {
                    // Head: Full width, height proportional to width
                    float v = (worldY - (totalHeight - headH)) / max(headH, 0.0001);
                    col = tex2D(_HeadTex, float2(uv.x, v));
                }
                else
                {
                    // Body: Full width (to match Head/Tail edges)
                    // Tile along Y based on Width to keep the pattern square/proportional
                    float bodyY = worldY - tailH;
                    
                    // This tiles the texture every time the length equals the current width
                    // making the pattern "square" in world space regardless of scaling
                    float2 bodyUV = float2(uv.x, bodyY / max(totalWidth, 0.0001));
                    col = tex2D(_BodyTex, bodyUV);
                }


                col *= _MainColor;
                col *= i.color;

                return col;
            }
            ENDCG
        }
    }
}