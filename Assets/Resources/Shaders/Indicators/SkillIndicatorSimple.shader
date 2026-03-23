Shader "Custom/SkillIndicatorSimple"
{
    Properties
    {
        [NoScaleOffset] _IndicatorPattern ("Indicator Pattern", 2D) = "white" {}
        _MainColor ("Main Color", Color) = (0, 0.5, 1, 0.3)
        _BorderColor ("Border Color", Color) = (0, 0.8, 1, 1)
        _WorldThickness ("Border Width (World)", Range(0, 1)) = 0.1
        _Softness ("Softness", Range(0, 0.1)) = 0.01
        _RotationSpeed ("Rotation Speed", Range(-10, 10)) = 0.0
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
            float _Softness;
            float _RotationSpeed;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.uv = v.uv;

                // Extract average scale from unity_ObjectToWorld
                float3 scale;
                scale.x = length(unity_ObjectToWorld._m00_m01_m02);
                scale.y = length(unity_ObjectToWorld._m10_m11_m12);
                o.worldScale = scale.xy;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Centered UV for rotation and distance calculations (-1 to 1)
                float2 pos = (i.uv - 0.5) * 2.0;
                float dist = length(pos);

                // Constant thickness calculation (UV space)
                float avgScale = (i.worldScale.x + i.worldScale.y) * 0.5;
                float uvThickness = _WorldThickness / (avgScale + 0.0001);

                // Rotation Math
                float angle = _Time.y * _RotationSpeed;
                float s = sin(angle);
                float c = cos(angle);
                float2x2 rotationMatrix = float2x2(c, -s, s, c);
                float2 rotUV = mul(rotationMatrix, pos) * 0.5 + 0.5;

                // Sample Texture Pattern
                fixed4 patternColor = tex2D(_IndicatorPattern, saturate(rotUV));

                // Circle masking
                float alpha = smoothstep(1.0 + _Softness, 1.0 - _Softness, dist);
                
                // Border masking logic (UV space thicknes)
                float edge1 = 1.0 - uvThickness;
                float borderAlpha = smoothstep(edge1 - _Softness, edge1 + _Softness, dist);
                
                // Combine Fill and Texture
                fixed4 fillColor = _MainColor * patternColor;
                
                // Overlay Border
                fixed4 finalCol = lerp(fillColor, _BorderColor, borderAlpha);
                
                // Apply global fading
                finalCol *= i.color;
                finalCol.a *= alpha;

                return finalCol;
            }
            ENDCG
        }
    }
}
