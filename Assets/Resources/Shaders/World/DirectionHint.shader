Shader "Custom/World/DirectionHint"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Shape SDF", 2D) = "black" {}
        [HDR] _Color ("Color", Color) = (1, 1, 1, 0.85)
        _EdgeSoftness ("Edge Softness", Range(0.5, 4)) = 1
        _Thicken ("Thicken", Range(-0.1, 0.1)) = 0
        [HDR] _GlowColor ("Glow Color", Color) = (1, 1, 1, 0)
        _GlowWidth ("Glow Width", Range(0, 0.5)) = 0.1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
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

            sampler2D _MainTex;
            fixed4 _Color;
            float _EdgeSoftness;
            float _Thicken;
            fixed4 _GlowColor;
            float _GlowWidth;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float distance = tex2D(_MainTex, i.uv).r + _Thicken;
                float edge = max(fwidth(distance) * _EdgeSoftness, 1e-4);
                float shape = smoothstep(0.5 - edge, 0.5 + edge, distance);
                float glow = smoothstep(0.5 - _GlowWidth, 0.5, distance) * (1 - shape) * _GlowColor.a;
                fixed4 tint = _Color * i.color;
                fixed4 color;
                color.rgb = tint.rgb * shape + _GlowColor.rgb * glow;
                color.a = saturate(tint.a * shape + glow * i.color.a);
                return color;
            }
            ENDCG
        }
    }
}
