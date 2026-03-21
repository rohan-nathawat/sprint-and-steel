Shader "Sprint&Steel/DiagonalGrid"
{
    Properties
    {
        _MainTex       ("Sprite Texture",   2D)               = "white" {}
        _BaseColor     ("Base Color",       Color)            = (0.05, 0.12, 0.09, 1)
        _LineColor     ("Line Color",       Color)            = (0.15, 0.30, 0.22, 1)
        _GridScale     ("Grid Scale",       Float)            = 5.0
        _LineThickness ("Line Thickness",   Range(0.01, 0.2)) = 0.05
        _PulseStrength ("Pulse Strength",   Range(0.0, 1.0))  = 0.5
        _PulseSpeed    ("Pulse Speed",      Float)            = 1.2
        _WaveFrequency ("Wave Frequency",   Float)            = 2.0
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue"          = "Geometry"
        }

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _LineColor;
                float  _GridScale;
                float  _LineThickness;
                float  _PulseStrength;
                float  _PulseSpeed;
                float  _WaveFrequency;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float2 worldXY     : TEXCOORD1;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv          = IN.uv;
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.worldXY     = worldPos.xy;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                // Scale and rotate UVs 45 degrees
                float2 scaledUV = IN.worldXY / _GridScale;
                float  s        = 0.7071;
                float2 rotUV;
                rotUV.x = scaledUV.x * s - scaledUV.y * s;
                rotUV.y = scaledUV.x * s + scaledUV.y * s;

                // Grid cells
                float2 cellUV  = frac(rotUV);
                float  maskX   = step(1.0 - _LineThickness, cellUV.x);
                float  maskY   = step(1.0 - _LineThickness, cellUV.y);
                float  gridMask = saturate(maskX + maskY);

                // Travelling pulse wave
                float diagVal    = (IN.worldXY.x + IN.worldXY.y) * _WaveFrequency;
                float pulse      = sin(diagVal - _Time.y * _PulseSpeed) * 0.5 + 0.5;
                float pulsedMask = gridMask * (1.0 + pulse * _PulseStrength);

                float4 col = lerp(_BaseColor, _LineColor, saturate(pulsedMask));
                col.a = 1.0;
                return col;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}
