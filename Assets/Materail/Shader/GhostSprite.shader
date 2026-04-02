Shader "Custom/GhostSprite"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        // Ghost color tint - use alpha to control overall ghost transparency
        _GhostColor ("Ghost Color", Color) = (0.4, 0.8, 1.0, 0.45)

        // Desaturate the sprite so it looks more ghost-like
        _Desaturate ("Desaturate Amount", Range(0, 1)) = 0.85

        // Fresnel/rim glow on the edges of the alpha boundary
        _RimStrength ("Rim Glow Strength", Range(0, 3)) = 1.0
        _RimColor ("Rim Color", Color) = (0.5, 0.9, 1.0, 1.0)

        // Scanline effect - gives ethereal shimmer stripes
        _ScanlineFreq ("Scanline Frequency", Float) = 12.0
        _ScanlineAlpha ("Scanline Alpha", Range(0, 1)) = 0.15

        // Flicker speed - subtle fading pulse
        _FlickerSpeed ("Flicker Speed", Float) = 2.5
        _FlickerAmount ("Flicker Amount", Range(0, 0.5)) = 0.12

        // Sprite sheet support: offset & scale handled via renderer's material block
        [HideInInspector] _Flip ("Flip", Vector) = (1, 1, 1, 1)
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest LEqual

        Pass
        {
            Name "GhostSprite"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                float3 worldPos     : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _GhostColor;
                half4 _RimColor;
                half4 _RendererColor;
                half4 _Flip;
                half  _Desaturate;
                half  _RimStrength;
                half  _ScanlineFreq;
                half  _ScanlineAlpha;
                half  _FlickerSpeed;
                half  _FlickerAmount;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.worldPos    = TransformObjectToWorld(IN.positionOS.xyz);

                // Sprite sheet UV: apply _MainTex_ST (Tiling/Offset set per-frame by SpriteRenderer)
                OUT.uv    = TRANSFORM_TEX(IN.uv, _MainTex);

                // Flip support (SpriteRenderer horizontal/vertical flip)
                float2 flipUV = lerp(float2(0,0), float2(1,0), _Flip.x < 0);
                OUT.uv.x = abs(OUT.uv.x - flipUV.x);

                OUT.color = IN.color * _RendererColor;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // 1. Sample sprite texture
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                // Discard fully transparent pixels from the sprite sheet
                clip(tex.a - 0.01);

                // 2. Desaturate: mix color towards grayscale
                half lum = dot(tex.rgb, half3(0.299, 0.587, 0.114));
                half3 desatColor = lerp(tex.rgb, half3(lum, lum, lum), _Desaturate);

                // 3. Apply ghost tint color
                half3 ghostCol = desatColor * _GhostColor.rgb;

                // 4. Rim glow at alpha edges
                //    Approximate: pixels near the alpha edge get brighter
                half alphaEdge = 1.0 - saturate(tex.a * 2.0 - 0.5);
                ghostCol += _RimColor.rgb * alphaEdge * _RimStrength;

                // 5. Scanline shimmer: vertical stripes over world Y
                float scanline = sin(IN.worldPos.y * _ScanlineFreq + _Time.y * 3.0) * 0.5 + 0.5;
                ghostCol += scanline * _ScanlineAlpha;

                // 6. Flicker pulse
                half flicker = sin(_Time.y * _FlickerSpeed) * _FlickerAmount;
                half finalAlpha = saturate(tex.a * _GhostColor.a + flicker);

                // 7. Incorporate vertex/renderer color (SpriteRenderer tint)
                ghostCol *= IN.color.rgb;
                finalAlpha *= IN.color.a;

                return half4(ghostCol, finalAlpha);
            }
            ENDHLSL
        }
    }

    FallBack "Sprites/Default"
}
