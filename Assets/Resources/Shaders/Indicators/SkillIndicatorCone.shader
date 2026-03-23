Shader "Custom/SkillIndicatorCone"
{
    Properties
    {
        [NoScaleOffset] _IndicatorPattern ("Indicator Pattern", 2D) = "white" {}
        _MainColor ("Main Color", Color) = (1, 0.5, 0, 0.3)
        _BorderColor ("Border Color", Color) = (1, 0.8, 0, 1)
        _WorldThickness ("Border Width (World)", Range(0, 1)) = 0.1
        _ConeAngle ("Cone Angle", Range(0, 360)) = 60.0
        _ScrollSpeed ("Texture Scroll Speed", Range(-10, 10)) = 1.0
        _Tiling ("Texture Tiling", Vector) = (1, 1, 0, 0)
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
            float _ConeAngle;
            float _ScrollSpeed;
            float4 _Tiling;

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
                float2 pos = (i.uv - 0.5) * 2.0;
                float dist = length(pos);
                
                // UV thickness adjustment
                float avgScale = (i.worldScale.x + i.worldScale.y) * 0.5;
                float uvThickness = _WorldThickness / (avgScale + 0.0001);

                float angle = degrees(atan2(pos.x, pos.y));
                if (angle < 0) angle += 360;

                // Polar Coordinates for Texture
                float2 polarUV = float2(angle / 360.0, dist);
                polarUV.x *= _Tiling.x;
                polarUV.y = polarUV.y * _Tiling.y - _Time.y * _ScrollSpeed;
                
                fixed4 texColor = tex2D(_IndicatorPattern, polarUV);

                float halfAngle = _ConeAngle * 0.5;
                float sectorMask = (angle <= halfAngle || angle >= (360 - halfAngle)) ? 1.0 : 0.0;
                
                float circleMask = smoothstep(1.0, 0.98, dist);
                
                float borderAlpha = smoothstep(1.0 - uvThickness, 1.0 - uvThickness + 0.02, dist);
                
                float edgeMask = (abs(angle - halfAngle) < 1.0 || abs(angle - (360 - halfAngle)) < 1.0) ? 1.0 : 0.0;
                
                // Base Fill
                fixed4 fillColor = _MainColor * texColor;
                
                fixed4 col = lerp(fillColor, _BorderColor, borderAlpha * (1.0 - edgeMask));
                
                col *= i.color;
                col.a *= circleMask * sectorMask;
                
                return col;
            }
            ENDCG
        }
    }
}
