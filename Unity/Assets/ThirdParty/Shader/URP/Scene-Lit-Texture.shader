Shader "X_Shader/URP/B_Scene/Light Texture"
{
    Properties
    {
        _Color ("Main Color", Color) = (0.5,0.5,0.5,1)
        _EmissionColor ("Emission Color", Color) = (1, 1, 1, 1)
        _MainTex ("Base (RGB)", 2D) = "white" {}

        _AlphaTex("AlphaTex", 2D) = "white" {}
        [Enum(MainTex(A),0,Channel(R),1,Channel(G),2,Channel(B),3)] _AlphaChannel("AlphaChannel", int) = 0

        _BloomTex("BloomTex", 2D) = "white" {}
        _BloomIntensity("BloomIntensity", Float) = 1.0
        _BloomColor("BloomColor", Color) = (1,1,1,1)

        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", int) = 2
        [Enum(Off,0,On,1)] _ZWrite ("ZWrite", int) = 1
        _Fog ("Fog", Range(0,1)) = 1
        [HideInInspector]_AlphaCtl("AlphaCtl", Range(0,1)) = 1
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
        [Toggle] _AlphaClip ("Alpha Clip", Float) = 1
        [HideInInspector] _Mode ("__mode", Float) = 0.0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
    }

    SubShader
    {
        Name "FORWARD"
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }
        LOD 300

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }

            Cull [_Cull]
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _ALPHATEST_ON
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uvMain : TEXCOORD0;
                half fogFactor : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _EmissionColor;
                float4 _MainTex_ST;
                float4 _BloomColor;
                float _BloomIntensity;
                float _AlphaChannel;
                float _Fog;
                float _AlphaCtl;
                float _Cutoff;
                float _AlphaClip;
                float _Mode;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_AlphaTex);
            SAMPLER(sampler_AlphaTex);
            TEXTURE2D(_BloomTex);
            SAMPLER(sampler_BloomTex);

            half GetAlphaValue(half4 mainSample, half4 alphaSample)
            {
                if (_AlphaChannel < 0.5h)
                {
                    return mainSample.a;
                }

                if (_AlphaChannel < 1.5h)
                {
                    return alphaSample.r;
                }

                if (_AlphaChannel < 2.5h)
                {
                    return alphaSample.g;
                }

                return alphaSample.b;
            }

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);

                output.positionCS = positionInputs.positionCS;
                output.uvMain = TRANSFORM_TEX(input.uv, _MainTex);
                output.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half4 mainSample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uvMain);
                mainSample.rgb *= _Color.rgb;

                half4 alphaSample = SAMPLE_TEXTURE2D(_AlphaTex, sampler_AlphaTex, input.uvMain);
                half alphaSource = GetAlphaValue(mainSample, alphaSample);
                half bloomMask = SAMPLE_TEXTURE2D(_BloomTex, sampler_BloomTex, input.uvMain).r;

                half useClip = step(0.5h, _AlphaClip);
                half modeIsCutout = step(0.5h, _Mode) * (1.0h - step(1.5h, _Mode));
                useClip = max(useClip, modeIsCutout);

                #ifdef _ALPHATEST_ON
                useClip = 1.0h;
                #endif

                if (useClip > 0.5h)
                {
                    clip(alphaSource - _Cutoff);
                }

                half3 color = mainSample.rgb;
                color += mainSample.rgb * _EmissionColor.rgb;
                color = lerp(color, color * _BloomIntensity * _BloomColor.rgb, bloomMask);

                if (_Fog > 0.5h)
                {
                    color = MixFog(color, input.fogFactor * _Fog);
                }

                half alpha = saturate(alphaSource * _Color.a * _AlphaCtl);
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }

    CustomEditor "SceneShaderGUI"
}
