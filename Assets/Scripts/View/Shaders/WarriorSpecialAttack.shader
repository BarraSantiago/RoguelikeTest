Shader "Custom/WarriorSpecialAttack"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,0.2,0,1)
        _InnerColor ("Inner Color", Color) = (1,1,0.5,1)
        _Intensity ("Intensity", Range(0, 10)) = 5
        _RingCount ("Ring Count", Range(1, 10)) = 4
        _AnimationSpeed ("Animation Speed", Range(0, 10)) = 2
        _ExplosionRadius ("Explosion Radius", Range(0, 1)) = 0.8
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
                float4 _InnerColor;
                float _Intensity;
                float _RingCount;
                float _AnimationSpeed;
                float _ExplosionRadius;
            CBUFFER_END

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

                // Create expanding rings
                float rings = sin(dist * _RingCount * 6.28 - time * 3.0) * 0.5 + 0.5;
                rings = pow(rings, 2.0);

                // Create explosion mask
                float explosion = smoothstep(_ExplosionRadius, 0.0, dist);
                explosion *= smoothstep(0.0, 0.2, dist);

                // Add radial energy pulses
                float angle = atan2(uv.y, uv.x);
                float radialPulse = sin(angle * 8.0 + time * 2.0) * 0.3 + 0.7;

                // Combine effects
                float intensity = rings * explosion * radialPulse;

                // Color gradient from center to edge
                half4 col = lerp(_InnerColor, _Color, dist);
                col *= _Intensity;
                col.a *= intensity;

                // Add bright flash at center
                float centerFlash = 1.0 - smoothstep(0.0, 0.3, dist);
                col.rgb += centerFlash * _InnerColor.rgb * 2.0;

                return col;
            }
            ENDHLSL
        }
    }
}