Shader "Custom/SkillIndicatorLine"
{
    Properties
    {
        [NoScaleOffset] _IndicatorPattern ("Indicator Pattern", 2D) = "white" {}
        _MainColor ("Main Color", Color) = (0, 0.5, 1, 0.3)
        _BorderColor ("Border Color", Color) = (0, 0.8, 1, 1)
        _WorldThickness ("Border Width (World)", Range(0, 1)) = 0.1
        _StripeCount ("Stripe Count", Range(1, 20)) = 5.0
        _StripeSpeed ("Stripe Speed", Range(-10, 10)) = 2.0
        _StripeIntensity ("Stripe Intensity", Range(0, 1)) = 0.2
        _ScrollSpeed ("Texture Scroll Speed", Range(-10, 10)) = -1.0
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
                float2 worldScale : TEXCOORD1;
            };

            sampler2D _IndicatorPattern;
            fixed4 _MainColor;
            fixed4 _BorderColor;
            float _WorldThickness;
            float _StripeCount;
            float _StripeSpeed;
            float _StripeIntensity;
            float _ScrollSpeed;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.uv = v.uv;

                float3 scale;
                scale.x = length(unity_ObjectToWorld._m00_m01_m02);
                scale.y = length(unity_ObjectToWorld._m10_m11_m12);
                o.worldScale = scale.xy;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                
                // UV thickness adjustment
                float uvThicknessX = _WorldThickness / (i.worldScale.x + 0.0001);
                float uvThicknessY = _WorldThickness / (i.worldScale.y + 0.0001);

                // Texture Scrolling
                float2 scrolledUV = uv;
                scrolledUV.y += _Time.y * _ScrollSpeed;
                fixed4 patternColor = tex2D(_IndicatorPattern, scrolledUV);

                // Scrolling stripes logic
                float stripe = frac(uv.y * _StripeCount - _Time.y * _StripeSpeed);
                float stripeMask = smoothstep(0.4, 0.5, stripe) * smoothstep(0.6, 0.5, stripe);
                
                // Border masking logic (Left/Right edges)
                float borderX = smoothstep(0.0, uvThicknessX, uv.x) * smoothstep(1.0, 1.0 - uvThicknessX, uv.x);
                float borderY = smoothstep(0.0, uvThicknessY * 0.2, uv.y) * smoothstep(1.0, 1.0 - (uvThicknessY * 0.2), uv.y);
                float innerMask = borderX * borderY;
                
                // Base color with stripes and texture
                fixed4 baseColor = _MainColor * patternColor;
                fixed4 colorWithStripes = lerp(baseColor, baseColor * (1.0 + _StripeIntensity), stripeMask);
                
                // Final blend with border
                fixed4 finalCol = lerp(_BorderColor, colorWithStripes, innerMask);
                
                finalCol *= i.color;
                // Fade edges
                finalCol.a *= smoothstep(0.0, 0.1, uv.y) * smoothstep(1.0, 0.9, uv.y);
                
                return finalCol;
            }
            ENDCG
        }
    }
}
