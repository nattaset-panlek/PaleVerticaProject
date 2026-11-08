Shader "Pale Vertica/VFX/Sprite Gradient Map"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _ShadowColor ("Shadow (black)", Color) = (0.35, 0.38, 0.45, 1)
        _MidColor ("Mid (gray)", Color) = (1, 1, 1, 1)
        _HighlightColor ("Highlight (white)", Color) = (1, 1, 1, 1)
        _Intensity ("Intensity", Range(0, 4)) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 10
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1, 1, 1, 1)
        [HideInInspector] _Flip ("Flip", Vector) = (1, 1, 1, 1)
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

        Blend [_SrcBlend] [_DstBlend]
        Cull Off
        ZWrite Off
        ZTest LEqual

        Pass
        {
            Name "SpriteGradientMap"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _ShadowColor;
                half4 _MidColor;
                half4 _HighlightColor;
                half4 _RendererColor;
                half4 _Flip;
                half _Intensity;
                half _SrcBlend;
                half _DstBlend;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.color = IN.color * _RendererColor;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                half value = saturate(tex.r);
                half3 low = lerp(_ShadowColor.rgb, _MidColor.rgb, saturate(value * 2.0));
                half3 high = lerp(_MidColor.rgb, _HighlightColor.rgb, saturate(value * 2.0 - 1.0));
                half3 rgb = (value < 0.5 ? low : high) * IN.color.rgb * _Intensity;
                half alpha = tex.a * IN.color.a;
                return half4(rgb, alpha);
            }
            ENDHLSL
        }
    }

    FallBack "Sprites/Default"
}
