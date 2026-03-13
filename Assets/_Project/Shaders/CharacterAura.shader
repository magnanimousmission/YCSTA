Shader "Custom/CharacterAura"
{
    Properties
    {
        [HDR]_AuraColor ("Aura Color", Color) = (0.2, 0.9, 1.0, 1.0)
        [HDR]_RimColor ("Rim Color", Color) = (0.0, 0.45, 1.0, 1.0)
        _Opacity ("Base Opacity", Range(0, 1)) = 0.35
        _FresnelPower ("Fresnel Power", Range(0.5, 8.0)) = 3.0
        _ShellWidth ("Shell Width", Range(0.0, 0.2)) = 0.03

        _PulseSpeed ("Pulse Speed", Range(0.0, 10.0)) = 2.5
        _PulseStrength ("Pulse Strength", Range(0.0, 1.0)) = 0.25

        _FlowScale ("Flow Scale", Range(0.1, 10.0)) = 2.0
        _FlowSpeed ("Flow Speed", Range(0.0, 5.0)) = 1.0
        _FlowStrength ("Flow Strength", Range(0.0, 1.0)) = 0.2

        _WaveAmplitude ("Wave Amplitude", Range(0.0, 0.05)) = 0.008
        _WaveFrequency ("Wave Frequency", Range(0.0, 20.0)) = 7.0
        _WaveSpeed ("Wave Speed", Range(0.0, 10.0)) = 2.0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha One
            ZWrite Off
            // Render backfaces of an expanded shell so the aura stays around the silhouette.
            Cull Front

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _AuraColor;
                half4 _RimColor;
                half _Opacity;
                half _FresnelPower;
                half _ShellWidth;
                half _PulseSpeed;
                half _PulseStrength;
                half _FlowScale;
                half _FlowSpeed;
                half _FlowStrength;
                half _WaveAmplitude;
                half _WaveFrequency;
                half _WaveSpeed;
            CBUFFER_END

            // Cheap animated value from world position that gives the aura moving detail.
            half FlowNoise(float3 posWS, half t)
            {
                half n = sin(posWS.x * _FlowScale + t) * 0.5h + 0.5h;
                n += sin(posWS.y * _FlowScale * 1.37h - t * 1.2h) * 0.5h + 0.5h;
                n += sin(posWS.z * _FlowScale * 0.73h + t * 0.85h) * 0.5h + 0.5h;
                return saturate((n / 3.0h) * _FlowStrength);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                float wavePhase = _Time.y * _WaveSpeed
                    + input.positionOS.x * _WaveFrequency
                    + input.positionOS.y * (_WaveFrequency * 0.73)
                    + input.positionOS.z * (_WaveFrequency * 1.21);
                float wave = sin(wavePhase) * _WaveAmplitude;

                float3 shellPositionOS = input.positionOS.xyz + normalize(input.normalOS) * (_ShellWidth + wave);
                VertexPositionInputs posInputs = GetVertexPositionInputs(shellPositionOS);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);

                output.positionCS = posInputs.positionCS;
                output.positionWS = posInputs.positionWS;
                output.normalWS = normalize(normalInputs.normalWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 viewDir = SafeNormalize(GetCameraPositionWS() - input.positionWS);
                half ndv = saturate(dot(normalize(input.normalWS), viewDir));

                // Stronger glow at glancing angles.
                half fresnel = pow(1.0h - ndv, _FresnelPower);

                half t = _Time.y * _FlowSpeed;
                half pulse = 1.0h + (sin(_Time.y * _PulseSpeed) * 0.5h + 0.5h) * _PulseStrength;
                half flow = FlowNoise(input.positionWS, t);

                half intensity = saturate((fresnel + flow) * pulse);
                half alpha = saturate(_Opacity * intensity);

                half3 color = lerp(_AuraColor.rgb, _RimColor.rgb, fresnel) * intensity;
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
