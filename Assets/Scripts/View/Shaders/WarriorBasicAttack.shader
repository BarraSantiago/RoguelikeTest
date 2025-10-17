Shader "Custom/WarriorBasicAttack"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,0.5,0,1)
        _Intensity ("Intensity", Range(0, 5)) = 2
        _EdgeGlow ("Edge Glow", Range(0, 1)) = 0.3
        _AnimationSpeed ("Animation Speed", Range(0, 10)) = 3
        _NoiseScale ("Noise Scale", Range(0, 10)) = 2
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
                float3 worldPos : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _Intensity;
                float _EdgeGlow;
                float _AnimationSpeed;
                float _NoiseScale;
            CBUFFER_END

            float noise(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.worldPos = TransformObjectToWorld(input.positionOS.xyz);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float time = _Time.y * _AnimationSpeed;

                // Create cone shape (wider at the top)
                float coneShape = smoothstep(0.1, 0.9, 1.0 - abs(uv.x - 0.5) * (2.0 - uv.y));

                // Add animated noise for energy effect
                float n = noise(uv * _NoiseScale + time);
                float animatedNoise = sin(n * 6.28 + time) * 0.5 + 0.5;

                // Create energy waves moving forward
                float waves = sin((uv.y - time) * 10.0) * 0.2 + 0.8;

                // Edge glow effect
                float edge = 1.0 - smoothstep(0.0, _EdgeGlow, distance(uv.x, 0.5));

                // Combine effects
                float alpha = coneShape * waves * animatedNoise * edge;
                alpha *= smoothstep(0.0, 0.1, uv.y) * smoothstep(1.0, 0.9, uv.y);

                half4 col = _Color * _Intensity;
                col.a *= alpha;

                return col;
            }
            ENDHLSL
        }
    }
}