Shader "Custom/SkillIndicatorFill"
{
    Properties
    {
        [Header(Textures)]
        [NoScaleOffset] _TailTex ("Tail Texture (Start)", 2D) = "white" {}
        [NoScaleOffset] _BodyTex ("Body Texture (Tiling)", 2D) = "white" {}
        [NoScaleOffset] _HeadTex ("Head Texture (End)", 2D) = "white" {}
        
        [Header(Colors)]
        _MainColor ("Fill Color", Color) = (1, 1, 1, 1)
        
        [Header(Slicing Settings)]
        _TailSize ("Tail Size (World)", float) = 0.5
        _HeadSize ("Head Size (World)", float) = 1.0
        _NaturalWidth ("Natural Width (World)", float) = 1.0
        _WorldSize ("World Size (Internal)", Vector) = (1, 1, 0, 0)
        
        [Header(Progress)]
        _FillAmount ("Fill Amount", Range(0, 1)) = 0.0
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
            };

            sampler2D _TailTex;
            sampler2D _BodyTex;
            sampler2D _HeadTex;
            fixed4 _MainColor;
            
            float _TailSize;
            float _HeadSize;
            float _NaturalWidth;
            float4 _WorldSize;
            float _FillAmount;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // PROGRESS CLIP: Clip anything above FillAmount
                if (i.uv.y > _FillAmount) discard;

                float2 uv = i.uv;
                float totalLength = _WorldSize.y;
                float totalWidth = _WorldSize.x;
                float currentWorldY = uv.y * totalLength;
                
                // Aspect Ratio Protection for X-axis (Same as main shader)
                float xRatio = totalWidth / max(_NaturalWidth, 0.0001);
                float uvX = (uv.x - 0.5) * xRatio + 0.5;
                if (uvX < 0.0 || uvX > 1.0) discard;
                
                fixed4 finalCol = fixed4(0,0,0,0);
                
                // --- 3-PART ASSEMBLY ---
                if (currentWorldY < _TailSize)
                {
                    float vTail = currentWorldY / max(_TailSize, 0.0001);
                    finalCol = tex2D(_TailTex, float2(uvX, vTail));
                }
                else if (currentWorldY > (totalLength - _HeadSize))
                {
                    float vHead = (currentWorldY - (totalLength - _HeadSize)) / max(_HeadSize, 0.0001);
                    finalCol = tex2D(_HeadTex, float2(uvX, vHead));
                }
                else
                {
                    float bodyWorldY = currentWorldY - _TailSize;
                    float2 bodyUV = float2(uvX, bodyWorldY / max(totalWidth, 0.0001));
                    finalCol = tex2D(_BodyTex, bodyUV);
                }

                return finalCol * _MainColor * i.color;
            }
            ENDCG
        }
    }
}
