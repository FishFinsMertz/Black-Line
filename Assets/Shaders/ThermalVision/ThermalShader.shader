Shader "Shaders/ThermalVision_Sprite_NoGhost"
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
    }
    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
            "IgnoreProjector"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature THERMAL_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            sampler2D _RampTex;
            fixed4 _Color;
            float _Temperature;
            float _FresnelPower;
            float _BrightnessInfluence;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 originalColor = tex2D(_MainTex, IN.texcoord);
                clip(originalColor.a - 0.05);

#ifdef THERMAL_ON
                // Normalize temperature to 0-1 multiplier
                float tempNorm = clamp(_Temperature / 100.0, 0.0, 1.0);

                // Luminance (brightness) contribution
                float grayValue = dot(originalColor.rgb, float3(0.3, 0.59, 0.11));

                // Fresnel heat based on distance from UV center
                float2 centeredUV = IN.texcoord - float2(0.5, 0.5);
                float dist = length(centeredUV) * 2.0;
                dist = clamp(dist, 0.0, 1.0);
                float fresnelHeat = 1.0 - pow(dist, _FresnelPower);

                // Combine contributions (without temperature scaling)
                float rampValueUnscaled = clamp(fresnelHeat + grayValue * _BrightnessInfluence, 0.0, 1.0);

                // Apply temperature multiplier: 0 = fully dark (cold), 1 = full effect
                float rampValue = rampValueUnscaled * tempNorm;

                fixed4 thermalColor = tex2D(_RampTex, float2(rampValue, 0.5));
                thermalColor.a = originalColor.a * IN.color.a;
                return thermalColor;
#else
                fixed4 col = originalColor * IN.color;
                col.rgb *= col.a;
                return col;
#endif
            }
            ENDCG
        }
    }
    Fallback "Sprites/Default"
}