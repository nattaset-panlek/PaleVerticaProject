Shader "Custom/SkillIndicatorArea"
{
    Properties
    {
        [NoScaleOffset] _IndicatorPattern ("Indicator Pattern", 2D) = "white" {}
        [HDR] _MainColor ("Main Color", Color) = (0, 0.5, 1, 0.3)
        [HDR] _BorderColor ("Border Color", Color) = (0, 0.8, 1, 1)
        _WorldThickness ("Border Width (World)", Range(0, 1)) = 0.1
        _Softness ("Softness", Range(0, 0.1)) = 0.01
        _RotationSpeed ("Rotation Speed", Range(-10, 10)) = 0.0
        _InnerFade ("Inner Fade Focus", Range(0, 1)) = 0.3
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
                float2 rotUV : TEXCOORD1;
                float avgScale : TEXCOORD2;
            };

            sampler2D _IndicatorPattern;
            fixed4 _MainColor;
            fixed4 _BorderColor;
            float _WorldThickness;
            float _Softness;
            float _RotationSpeed;
            float _InnerFade;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.uv = v.uv;

                float3 scale;
                scale.x = length(unity_ObjectToWorld._m00_m01_m02);
                scale.y = length(unity_ObjectToWorld._m10_m11_m12);
                o.avgScale = (scale.x + scale.y) * 0.5;

                float angle = _Time.y * _RotationSpeed;
                float s = sin(angle);
                float c = cos(angle);
                float2x2 rotationMatrix = float2x2(c, -s, s, c);
                
                float2 centeredUV = v.uv - 0.5;
                o.rotUV = mul(rotationMatrix, centeredUV) + 0.5;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 pos = (i.uv - 0.5) * 2.0;
                float dist = length(pos);

                float uvThickness = _WorldThickness / (i.avgScale + 0.0001);

                fixed4 patternColor = tex2D(_IndicatorPattern, saturate(i.rotUV));

                float alpha = smoothstep(1.0 + _Softness, 1.0 - _Softness, dist);

                float edge1 = 1.0 - uvThickness;
                float borderAlpha = smoothstep(edge1 - _Softness, edge1 + _Softness, dist);

                float innerFadeAlpha = smoothstep(0.0, max(_InnerFade, 0.001), dist);

                fixed4 fillColor = _MainColor * patternColor;
                fillColor.a *= innerFadeAlpha;

                fixed4 finalCol = lerp(fillColor, _BorderColor, borderAlpha);

                finalCol *= i.color;
                finalCol.a *= alpha;

                return finalCol;
            }
            ENDCG
        }
    }
}