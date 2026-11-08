Shader "Custom/UI/FogAura"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        [Header(Gradient Map)]
        _RampColor0     ("Ramp 0", Color)             = (0,0,0,1)
        _RampColor1     ("Ramp 1", Color)             = (0.027,0.027,0.027,1)
        _RampColor2     ("Ramp 2", Color)             = (0.078,0.078,0.078,1)
        _RampColor3     ("Ramp 3", Color)             = (0.557,0.078,0.094,1)
        _RampColor4     ("Ramp 4", Color)             = (1,0.353,0.306,1)
        _RampStops      ("Ramp Stops (1,2,3)", Vector) = (0.15, 0.4, 0.65, 0)
        [Header(Motion)]
        _Color          ("Tint", Color)               = (1,1,1,1)
        _Aspect         ("Aspect", Float)             = 1.7777
        _Speed          ("Flow Speed", Float)         = 0.6
        _StartTime      ("Start Time", Float)         = 0
        [Header(Shape)]
        _Reach          ("Edge Reach", Range(0.05,0.6)) = 0.26
        _EdgePower      ("Edge Power", Range(0.5,4))  = 1.9
        _Warp           ("Warp Strength", Range(0,10)) = 5.5
        _Turbulence     ("Turbulence", Range(0,3))    = 1.5
        _Thresholds     ("Band Thresholds", Vector)   = (0.40, 0.66, 0.92, 0)
        _BandAlpha      ("Band Alpha", Vector)        = (0.20, 0.48, 0.86, 0)
        _BandGray       ("Band Gray", Vector)         = (0.52, 0.30, 0.08, 0)
        _RimWidth       ("Rim Width", Vector)         = (0.016, 0.011, 0.06, 0)
        [Header(Screen)]
        _BaseDim        ("Screen Dim", Range(0,1))    = 0.45
        _Fade           ("Fade", Range(0,1))          = 1

        _StencilComp    ("Stencil Comparison",  Float) = 8
        _Stencil        ("Stencil ID",          Float) = 0
        _StencilOp      ("Stencil Operation",   Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask",  Float) = 255
        _ColorMask       ("Color Mask",         Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"             = "Transparent"
            "IgnoreProjector"   = "True"
            "RenderType"        = "Transparent"
            "PreviewType"       = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref       [_Stencil]
            Comp      [_StencilComp]
            Pass      [_StencilOp]
            ReadMask  [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull     Off
        Lighting Off
        ZWrite   Off
        ZTest    [unity_GUIZTestMode]
        Blend    SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "FogAura"
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma target   3.0
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color  : COLOR;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color  : COLOR;
                float2 uv     : TEXCOORD0;
            };

            fixed4 _RampColor0;
            fixed4 _RampColor1;
            fixed4 _RampColor2;
            fixed4 _RampColor3;
            fixed4 _RampColor4;
            float4 _RampStops;
            float _StartTime;
            fixed4 _Color;
            float _Aspect;
            float _Speed;
            float _Reach;
            float _EdgePower;
            float _Warp;
            float _Turbulence;
            float4 _Thresholds;
            float4 _BandAlpha;
            float4 _BandGray;
            float4 _RimWidth;
            float _BaseDim;
            float _Fade;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv     = v.uv;
                o.color  = v.color * _Color;
                return o;
            }

            float hash21(float2 p)
            {
                p = p - floor(p / 289.0) * 289.0;
                p = frac(p * float2(0.1031, 0.1030) + 0.5);
                p += dot(p, p.yx + 33.33);
                return frac((p.x + p.y) * p.x * 43.758);
            }

            float valueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                float a = hash21(i);
                float b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1));
                float d = hash21(i + float2(1, 1));
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float fbm(float2 p, int octaves)
            {
                float v = 0.0;
                float a = 0.5;
                float s = 0.0;
                for (int o = 0; o < 5; o++)
                {
                    if (o >= octaves) break;
                    v += a * valueNoise(p + float2(o * 13.1, o * 7.7));
                    s += a;
                    a *= 0.5;
                    p *= 2.03;
                }
                return v / s;
            }

            fixed3 gradientMap(float g)
            {
                fixed3 c = lerp(_RampColor0.rgb, _RampColor1.rgb, saturate(g / max(_RampStops.x, 1e-4)));
                c = lerp(c, _RampColor2.rgb, saturate((g - _RampStops.x) / max(_RampStops.y - _RampStops.x, 1e-4)));
                c = lerp(c, _RampColor3.rgb, saturate((g - _RampStops.y) / max(_RampStops.z - _RampStops.y, 1e-4)));
                c = lerp(c, _RampColor4.rgb, saturate((g - _RampStops.z) / max(1.0 - _RampStops.z, 1e-4)));
                return c;
            }

            float auraField(float2 uv, float t)
            {
                float asp = _Aspect;
                float2 c = float2((uv.x - 0.5) * asp, uv.y - 0.5);
                float th = atan2(c.y, c.x);
                float ex = min(uv.x, 1.0 - uv.x) * asp;
                float ey = min(uv.y, 1.0 - uv.y);
                float d = min(ex, ey);

                float wx = fbm(float2(uv.x * 3.0 * asp + 0.1 * t, uv.y * 3.0 - 0.2 * t), 4);
                float wy = fbm(float2(uv.x * 3.0 * asp + 4.1, uv.y * 3.0 + 2.7 + 0.15 * t), 4);
                float r = 7.0 + d * 8.5 - t * 1.4;
                float2 p = float2(cos(th), sin(th)) * r + (float2(wx, wy) - 0.5) * _Warp;
                float n = fbm(p, 5);
                float n2 = fbm(float2(uv.x * 9.0 * asp - 0.3 * t, uv.y * 9.0 + 0.6 * t), 4);

                float corner = length(float2(max(0.0, 0.3 - ex), max(0.0, 0.3 - ey)));
                float edge = pow(saturate(1.0 - d / _Reach), _EdgePower) + corner * 1.5;
                return edge + (n - 0.5) * _Turbulence * (0.35 + edge) + (n2 - 0.5) * 0.25 * edge;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float t = max(0.0, _Time.y - _StartTime) * _Speed;
                float f = auraField(i.uv, t);
                float w = max(fwidth(f) * 0.75, 0.002);

                float b0 = smoothstep(_Thresholds.x - w, _Thresholds.x + w, f);
                float b1 = smoothstep(_Thresholds.y - w, _Thresholds.y + w, f);
                float b2 = smoothstep(_Thresholds.z - w, _Thresholds.z + w, f);

                float a = lerp(lerp(lerp(0.0, _BandAlpha.x, b0), _BandAlpha.y, b1), _BandAlpha.z, b2);
                float g = lerp(lerp(lerp(0.0, _BandGray.x, b0), _BandGray.y, b1), _BandGray.z, b2);

                float rim  = exp(-pow((f - _Thresholds.x) / _RimWidth.x, 2.0));
                float rim2 = exp(-pow((f - _Thresholds.y) / _RimWidth.y, 2.0)) * 0.7;
                float glow = exp(-pow((f - _Thresholds.x) / _RimWidth.z, 2.0)) * 0.6;
                g = max(g, max(rim, rim2 * 0.8));
                a = max(a, max(rim * 0.95, rim2 * 0.6));
                g = max(g, glow * 0.72);
                a = max(a, glow * 0.42);
                a = max(a, smoothstep(0.2, 1.2, f) * 0.45);

                fixed3 col = gradientMap(saturate(g)) * i.color.rgb;
                float aura = saturate(a) * i.color.a;
                float outA = aura + _BaseDim * (1.0 - aura);
                fixed3 outRgb = outA > 0.0001 ? col * aura / outA : col;
                return fixed4(outRgb, outA * _Fade);
            }
            ENDCG
        }
    }
}
