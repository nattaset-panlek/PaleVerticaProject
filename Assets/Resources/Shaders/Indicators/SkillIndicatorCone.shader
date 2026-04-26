Shader "Custom/SkillIndicatorCone"
{
    Properties
    {
        [NoScaleOffset] _IndicatorPattern ("Indicator Pattern", 2D) = "white" {}
        _MainColor ("Main Color", Color) = (1, 0.5, 0, 0.3)
        _BorderColor ("Border Color", Color) = (1, 0.8, 0, 1)
        _BorderThickness ("Border Thickness (World)", Range(0, 0.5)) = 0.05
        _ConeAngle ("Cone Angle", Range(0, 360)) = 60.0
        
        [Header(Texture Settings)]
        _ScrollSpeed ("Scroll Speed", Range(-10, 10)) = 0.0
        _Tiling ("Tiling", Vector) = (1, 1, 0, 0)
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
            float _BorderThickness;
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
                // Convert UV to -1 to 1 space
                float2 pos = (i.uv - 0.5) * 2.0;
                float dist = length(pos);
                
                // Thickness in 0-1 UV space
                float avgScale = (i.worldScale.x + i.worldScale.y) * 0.5;
                float uvThickness = _BorderThickness / (avgScale + 0.0001);

                // Angle calculation
                float angle = degrees(atan2(pos.x, pos.y));
                if (angle < 0) angle += 360;

                float halfAngle = _ConeAngle * 0.5;
                
                // 1. Sector Mask (The "Cone" shape)
                float sectorMask = (angle <= halfAngle || angle >= (360 - halfAngle)) ? 1.0 : 0.0;
                
                // 2. Circular Mask (Outer radius)
                float circleMask = step(dist, 1.0);
                
                // 3. Sharp Outer Border
                float outerBorder = smoothstep(1.0 - uvThickness, 1.0 - uvThickness + 0.01, dist) * circleMask;
                
                // 4. Sharp Side Borders (Distance based for uniform thickness)
                float rad1 = radians(halfAngle);
                float rad2 = radians(360.0 - halfAngle);
                
                // Distance from point to the two edge lines
                float2 dir1 = float2(sin(rad1), cos(rad1));
                float2 dir2 = float2(sin(rad2), cos(rad2));
                
                float distToLine1 = length(pos - dir1 * clamp(dot(pos, dir1), 0, 1));
                float distToLine2 = length(pos - dir2 * clamp(dot(pos, dir2), 0, 1));
                
                float sideBorder = smoothstep(uvThickness, uvThickness - 0.01, min(distToLine1, distToLine2));
                
                // Combine Borders
                float finalBorderMask = max(outerBorder, sideBorder) * sectorMask;
                
                // Textures
                float2 patternUV = i.uv * _Tiling.xy + float2(0, _Time.y * _ScrollSpeed);
                fixed4 texColor = tex2D(_IndicatorPattern, patternUV);

                // Final Blend
                fixed4 fillColor = _MainColor * texColor;
                fixed4 col = lerp(fillColor, _BorderColor, finalBorderMask);
                
                col *= i.color;
                col.a *= circleMask * sectorMask;
                
                return col;
            }
            ENDCG
        }
    }
}