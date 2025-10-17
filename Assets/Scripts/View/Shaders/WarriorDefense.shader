Shader "Custom/WarriorDefense"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (0.2,0.5,1,1)
        _ShieldColor ("Shield Color", Color) = (0.5,0.8,1,1)
        _Intensity ("Intensity", Range(0, 5)) = 3
        _HexScale ("Hex Scale", Range(1, 20)) = 8
        _AnimationSpeed ("Animation Speed", Range(0, 5)) = 1
        _EdgeThickness ("Edge Thickness", Range(0, 0.5)) = 0.1
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" "Queue"="Transparent"
        }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
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
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _ShieldColor;
                float _Intensity;
                float _HexScale;
                float _AnimationSpeed;
                float _EdgeThickness;
            CBUFFER_END

            float hexPattern(float2 uv, float scale)
            {
                uv *= scale;
                uv.y += fmod(floor(uv.x), 2.0) * 0.5;
                float2 hex = frac(uv) - 0.5;
                float2 cell = abs(hex);
                float hexDist = max(cell.x * 0.866 + cell.y * 0.5, cell.y);
                return 1.0 - smoothstep(0.4, 0.5, hexDist);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv - 0.5;
                float dist = length(uv) * 2.0;
                float time = _Time.y * _AnimationSpeed;

                // Create circular shield
                float shield = 1.0 - smoothstep(0.8, 1.0, dist);

                // Add hexagonal pattern
                float hex = hexPattern(input.uv + time * 0.1, _HexScale);

                // Create energy flow along edges
                float edge = smoothstep(0.7, 0.8, dist) * smoothstep(1.0, 0.9, dist);
                float energyFlow = sin(time * 3.0 + dist * 10.0) * 0.5 + 0.5;

                // Combine patterns
                float pattern = hex * 0.3 + edge * energyFlow * 0.7;
                pattern *= shield;

                // Add pulsing effect
                float pulse = sin(time * 2.0) * 0.2 + 0.8;

                // Color mixing
                half4 col = lerp(_Color, _ShieldColor, pattern);
                col *= _Intensity * pulse;
                col.a *= pattern * shield;

                // Add bright edge
                float brightEdge = edge * energyFlow;
                col.rgb += brightEdge * _ShieldColor.rgb * 2.0;

                return col;
            }
            ENDHLSL
        }
    }
}