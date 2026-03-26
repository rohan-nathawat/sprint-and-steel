Shader "Custom/PulsatingGrid2DRGB"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.86, 0.86, 0.86, 1)
        _LineAlpha ("Line Alpha", Range(0, 1)) = 0.75
        _CellSize ("Cells Per Unit", Float) = 8
        _LineWidth ("Line Width", Range(0.001, 0.2)) = 0.03
        _PulseSpeed ("RGB Pulse Speed", Float) = 2
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 1
        _MinPulse ("Minimum Pulse", Range(0, 1)) = 0.35
        _UseWorldSpace ("Use World Space Grid", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float _LineAlpha;
                float _CellSize;
                float _LineWidth;
                float _PulseSpeed;
                float _PulseAmount;
                float _MinPulse;
                float _UseWorldSpace;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            float GridMask(float2 coords)
            {
                float2 local = abs(frac(coords) - 0.5);
                float nearestLine = min(local.x, local.y);
                float aa = max(fwidth(nearestLine), 1e-5) * 1.5;
                return 1.0 - smoothstep(_LineWidth - aa, _LineWidth + aa, nearestLine);
            }

            float3 RGBPulse(float t)
            {
                float r = 0.5 + 0.5 * sin(t);
                float g = 0.5 + 0.5 * sin(t + 2.0943951); // +120°
                float b = 0.5 + 0.5 * sin(t + 4.1887902); // +240°
                return float3(r, g, b);
            }

            half4 frag(Varyings input) : SV_Target
            {
                float useWorldSpace = saturate(_UseWorldSpace);
                float2 gridCoords = lerp(input.uv * _CellSize, input.positionWS.xy * _CellSize, useWorldSpace);

                float lineMask = GridMask(gridCoords);

                float t = _Time.y * _PulseSpeed;
                float3 rgbCycle = RGBPulse(t);

                float pulse01 = 0.5 + 0.5 * sin(t);
                float animatedPulse = lerp(_MinPulse, 1.0, pulse01);
                float pulse = lerp(1.0, animatedPulse, saturate(_PulseAmount));

                half4 baseCol = _BaseColor;
                half4 lineCol = half4(rgbCycle * pulse, _LineAlpha * pulse);

                half4 finalColor = lerp(baseCol, lineCol, lineMask);
                return finalColor;
            }
            ENDHLSL
        }
    }
}
