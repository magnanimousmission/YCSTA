Shader "Custom/TetherGlow"
{
    Properties
    {
        _TintColor ("Tint Color", Color) = (0.2, 0.6, 1, 1)
        _ScrollSpeed ("Scroll Speed", Float) = 1.5
        _PulseSpeed ("Pulse Speed", Float) = 2.0
        _PulseIntensity ("Pulse Intensity", Float) = 0.3
        _Brightness ("Brightness", Float) = 2.0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "TetherGlowPass"
            Blend One One           // Additive blending = glow effect
            ZWrite Off
            Cull Off                // Two-sided for LineRenderer
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float4 color       : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _TintColor;
                float  _ScrollSpeed;
                float  _PulseSpeed;
                float  _PulseIntensity;
                float  _Brightness;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv          = IN.uv;
                OUT.color       = IN.color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Scroll UV along the wire (X axis = along wire length)
                float scrolledU = IN.uv.x - (_Time.y * _ScrollSpeed);

                // Soft repeating bands to fake energy pulses
                float band = abs(sin(scrolledU * 6.2831853));  // 0..1 repeating
                band = pow(band, 3.0);                         // sharpen the bands

                // Pulse the overall brightness
                float pulse = 1.0 - (_PulseIntensity * (sin(_Time.y * _PulseSpeed) * 0.5 + 0.5));

                // Soft edge fade across wire width (V axis = across width)
                float edgeFade = 1.0 - abs(IN.uv.y * 2.0 - 1.0);
                edgeFade = pow(edgeFade, 1.5);

                float3 col = _TintColor.rgb * _Brightness * band * pulse * edgeFade;

                // Vertex color carries the aura ratio tint from TetherManager
                col *= IN.color.rgb;

                return half4(col, 1.0);  // alpha ignored in additive blend
            }
            ENDHLSL
        }
    }
}
