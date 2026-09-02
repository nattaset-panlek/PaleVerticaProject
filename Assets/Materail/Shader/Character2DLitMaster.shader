Shader "Custom/Character2DLitMaster"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)

        [Header(Hit Flash)]
        _FlashColor ("Flash Color", Color) = (1, 1, 1, 1)
        _FlashAmount ("Flash Amount", Range(0, 1)) = 0.0

        [Header(Status Effect)]
        _StatusColor ("Status Color", Color) = (0.2, 1.0, 0.2, 1)
        _StatusIntensity ("Status Intensity", Range(0, 1)) = 0.0

        [Header(Dissolve)]
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0.0
        _DissolveEdgeColor ("Dissolve Edge Color", Color) = (1, 0.6, 0.1, 1)
        _DissolveEdgeWidth ("Dissolve Edge Width", Range(0, 0.2)) = 0.05

        [HideInInspector] _Flip ("Flip", Vector) = (1, 1, 1, 1)
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1, 1, 1, 1)
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
            "CanUseSpriteAtlas" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest LEqual

        Pass
        {
            Name "Character2DForward"
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
                float4 _MainTex_TexelSize;
                half4  _Color;
                half4  _FlashColor;
                half   _FlashAmount;
                half4  _StatusColor;
                half   _StatusIntensity;
                half   _DissolveAmount;
                half4  _DissolveEdgeColor;
                half   _DissolveEdgeWidth;
                half4  _RendererColor;
                half4  _Flip;
            CBUFFER_END

            float PseudoNoise(float2 uv)
            {
                return frac(sin(dot(uv.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.worldPos    = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.uv          = TRANSFORM_TEX(IN.uv, _MainTex);

                float2 flipUV = lerp(float2(0, 0), float2(1, 0), _Flip.x < 0);
                OUT.uv.x = abs(OUT.uv.x - flipUV.x);

                OUT.color = IN.color * _RendererColor * _Color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                clip(tex.a - 0.01);

                if (_DissolveAmount > 0.001)
                {
                    float noise = PseudoNoise(IN.uv * 10.0);
                    if (noise < _DissolveAmount)
                    {
                        discard;
                    }
                    else if (noise < (_DissolveAmount + _DissolveEdgeWidth))
                    {
                        return half4(_DissolveEdgeColor.rgb, tex.a * IN.color.a);
                    }
                }

                half3 baseCol = tex.rgb * IN.color.rgb;

                if (_StatusIntensity > 0.01)
                {
                    baseCol = lerp(baseCol, baseCol * _StatusColor.rgb + _StatusColor.rgb * 0.2, _StatusIntensity);
                }

                if (_FlashAmount > 0.01)
                {
                    baseCol = lerp(baseCol, _FlashColor.rgb, _FlashAmount);
                }

                return half4(baseCol, tex.a * IN.color.a);
            }
            ENDHLSL
        }
    }

    FallBack "Sprites/Default"
}
