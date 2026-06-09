Shader "Shaders/ThermalVision_Sprite_Lit"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _RampTex ("Ramp Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [Toggle(THERMAL_ON)] _ThermalEnabled ("Thermal Vision", Float) = 1
        _Temperature ("Temperature", Range(0.0, 100.0)) = 100.0
        _FresnelPower ("Fresnel Power", Range(0.0, 1.0)) = 0.5
        _BrightnessInfluence ("Brightness Influence", Range(0.0, 1.0)) = 0.08
        _FresnelCenter ("Fresnel Center", Vector) = (0.5, 0.5, 0, 0)
        _FresnelRadius ("Fresnel Radius", Range(0.1, 5.0)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha

        Pass
        {
            Name "Sprite Lit"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex CombinedShapeLightVertex
            #pragma fragment CombinedShapeLightFragment
            #pragma shader_feature THERMAL_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/ShapeLightShared.hlsl"

            struct Attributes
            {
                float3 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                half4 color        : COLOR;
                float2 uv          : TEXCOORD0;
                half2 lightingUV   : TEXCOORD1;
                float2 localPos    : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_RampTex);
            SAMPLER(sampler_RampTex);
            half4 _Color;
            float _Temperature;
            float _FresnelPower;
            float _BrightnessInfluence;
            float2 _FresnelCenter;
            float _FresnelRadius;

            #if USE_SHAPE_LIGHT_TYPE_0
            SHAPE_LIGHT(0)
            #endif
            #if USE_SHAPE_LIGHT_TYPE_1
            SHAPE_LIGHT(1)
            #endif
            #if USE_SHAPE_LIGHT_TYPE_2
            SHAPE_LIGHT(2)
            #endif
            #if USE_SHAPE_LIGHT_TYPE_3
            SHAPE_LIGHT(3)
            #endif

            Varyings CombinedShapeLightVertex(Attributes v)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = v.uv;
                o.localPos = v.positionOS.xy;
                o.lightingUV = half2(ComputeScreenPos(o.positionCS / o.positionCS.w).xy);
                o.color = v.color * _Color;
                return o;
            }

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/CombinedShapeLightShared.hlsl"

            half4 CombinedShapeLightFragment(Varyings i) : SV_Target
            {
                const half4 main = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                const half4 mask = half4(1,1,1,1);
                SurfaceData2D surfaceData;
                InputData2D inputData;

                InitializeSurfaceData(main.rgb, main.a, mask, surfaceData);
                InitializeInputData(i.uv, i.lightingUV, inputData);

                #ifdef THERMAL_ON
                    float tempNorm = clamp(_Temperature / 100.0, 0.0, 1.0);
                    float grayValue = dot(main.rgb, float3(0.3, 0.59, 0.11));
                    float2 spritePos = i.localPos * 2.0;
                    float2 centerOffset = _FresnelCenter * 2.0 - 1.0;
                    float2 centeredPos = spritePos - centerOffset;
                    float dist = length(centeredPos);
                    dist = clamp(dist / _FresnelRadius, 0.0, 1.0);
                    float fresnelHeat = 1.0 - pow(dist, _FresnelPower);
                    float rampValueUnscaled = clamp(fresnelHeat + grayValue * _BrightnessInfluence, 0.0, 1.0);
                    float rampValue = rampValueUnscaled * tempNorm;
                    half4 thermalColor = SAMPLE_TEXTURE2D(_RampTex, sampler_RampTex, float2(rampValue, 0.5));
                    thermalColor.a = main.a;
                    return thermalColor;
                #endif

                return CombinedShapeLightShared(surfaceData, inputData);
            }
            ENDHLSL
        }
    }
    Fallback "Universal Render Pipeline/2D/Sprite-Lit-Default"
}