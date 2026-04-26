Shader "Custom/SkillIndicatorConeFill"
{
    Properties
    {
        [NoScaleOffset] _IndicatorPattern ("Fill Pattern", 2D) = "white" {}
        _MainColor ("Fill Color", Color) = (1, 1, 1, 1)
        _BorderColor ("Border Color", Color) = (1, 1, 1, 1)
        _BorderThickness ("Border Thickness (World)", float) = 0.05
        _ConeAngle ("Cone Angle", Range(0, 360)) = 60.0
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
                float2 worldScale : TEXCOORD1;
            };

            sampler2D _IndicatorPattern;
            fixed4 _MainColor;
            fixed4 _BorderColor;
            float _BorderThickness;
            float _ConeAngle;
            float _FillAmount;

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
                
                // PROGRESS CLIP: Only show within FillAmount radius
                if (dist > _FillAmount) discard;

                float avgScale = (i.worldScale.x + i.worldScale.y) * 0.5;
                float uvThickness = _BorderThickness / (avgScale + 0.0001);

                float angle = degrees(atan2(pos.x, pos.y));
                if (angle < 0) angle += 360;

                float halfAngle = _ConeAngle * 0.5;
                float sectorMask = (angle <= halfAngle || angle >= (360 - halfAngle)) ? 1.0 : 0.0;
                
                if (sectorMask <= 0) discard;

                // Border logic (Internal sharp edges)
                float rad1 = radians(halfAngle);
                float rad2 = radians(360.0 - halfAngle);
                float2 dir1 = float2(sin(rad1), cos(rad1));
                float2 dir2 = float2(sin(rad2), cos(rad2));
                float distToLine1 = length(pos - dir1 * clamp(dot(pos, dir1), 0, 1));
                float distToLine2 = length(pos - dir2 * clamp(dot(pos, dir2), 0, 1));
                
                float outerBorder = smoothstep(_FillAmount - uvThickness, _FillAmount - uvThickness + 0.01, dist);
                float sideBorder = smoothstep(uvThickness, uvThickness - 0.01, min(distToLine1, distToLine2));
                float finalBorderMask = max(outerBorder, sideBorder);

                fixed4 texColor = tex2D(_IndicatorPattern, i.uv);
                fixed4 fillColor = _MainColor * texColor;
                fixed4 col = lerp(fillColor, _BorderColor, finalBorderMask);
                
                return col * i.color;
            }
            ENDCG
        }
    }
}
