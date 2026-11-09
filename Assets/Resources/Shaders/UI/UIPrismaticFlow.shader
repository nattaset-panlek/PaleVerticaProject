Shader "Custom/UI/PrismaticFlow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Stop0 ("Stop 0 sRGB", Vector) = (0.9529,0.9333,0.8157,0)
        _Stop1 ("Stop 1 sRGB", Vector) = (0.8941,0.7255,0.5882,0)
        _Stop2 ("Stop 2 sRGB", Vector) = (0.4627,0.4157,0.6980,0)
        _Stop3 ("Stop 3 sRGB", Vector) = (0.3529,0.5529,0.7608,0)
        _Stop4 ("Stop 4 sRGB", Vector) = (0.5451,0.7922,0.6980,0)
        _FlowSpeed ("Flow Speed", Float) = 0.2

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "False"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

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
                float4 pos : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float3 _Stop0;
            float3 _Stop1;
            float3 _Stop2;
            float3 _Stop3;
            float3 _Stop4;
            float _FlowSpeed;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            float3 Ramp(float t)
            {
                float s = saturate(t) * 4.0;
                if (s < 1.0) return lerp(_Stop0, _Stop1, s);
                if (s < 2.0) return lerp(_Stop1, _Stop2, s - 1.0);
                if (s < 3.0) return lerp(_Stop2, _Stop3, s - 2.0);
                return lerp(_Stop3, _Stop4, s - 3.0);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float mask = tex2D(_MainTex, i.uv).a;
                float position = (i.uv.x - _Time.y * _FlowSpeed) * 0.5;
                float wave = 1.0 - abs(frac(position) * 2.0 - 1.0);
                float3 flow = Ramp(wave);

                #ifndef UNITY_COLORSPACE_GAMMA
                flow = GammaToLinearSpace(flow);
                #endif

                return fixed4(flow, mask * i.color.a);
            }
            ENDCG
        }
    }
}
