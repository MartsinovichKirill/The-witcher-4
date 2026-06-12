Shader "WitcherRightVersion/CinematicGrading"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        CGINCLUDE
        #include "UnityCG.cginc"

        sampler2D _MainTex;
        float4 _MainTex_TexelSize;
        sampler2D _BloomTex;

        float _Exposure;
        float _Contrast;
        float _Saturation;
        float _BloomThreshold;
        float _BloomIntensity;
        float _VignetteStrength;
        float _BlurOffset;

        struct appdata
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct v2f
        {
            float4 vertex : SV_POSITION;
            float2 uv : TEXCOORD0;
        };

        v2f vert(appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = v.uv;
            return o;
        }

        float3 AcesTonemap(float3 x)
        {
            const float a = 2.51;
            const float b = 0.03;
            const float c = 2.43;
            const float d = 0.59;
            const float e = 0.14;
            return saturate((x * (a * x + b)) / (x * (c * x + d) + e));
        }
        ENDCG

        // Pass 0: bloom prefilter (bright areas only)
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            fixed4 frag(v2f i) : SV_Target
            {
                float3 color = tex2D(_MainTex, i.uv).rgb;
                float brightness = max(color.r, max(color.g, color.b));
                float contribution = max(brightness - _BloomThreshold, 0.0);
                contribution /= max(brightness, 0.0001);
                return fixed4(color * contribution, 1.0);
            }
            ENDCG
        }

        // Pass 1: 4-tap diagonal box blur
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            fixed4 frag(v2f i) : SV_Target
            {
                float2 offset = _MainTex_TexelSize.xy * _BlurOffset;
                float3 color = tex2D(_MainTex, i.uv + float2(offset.x, offset.y)).rgb;
                color += tex2D(_MainTex, i.uv + float2(-offset.x, offset.y)).rgb;
                color += tex2D(_MainTex, i.uv + float2(offset.x, -offset.y)).rgb;
                color += tex2D(_MainTex, i.uv + float2(-offset.x, -offset.y)).rgb;
                return fixed4(color * 0.25, 1.0);
            }
            ENDCG
        }

        // Pass 2: composite with grading, tonemap, and vignette
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            fixed4 frag(v2f i) : SV_Target
            {
                float3 color = tex2D(_MainTex, i.uv).rgb;
                float3 bloom = tex2D(_BloomTex, i.uv).rgb;

                color += bloom * _BloomIntensity;
                color *= _Exposure;
                color = AcesTonemap(color);

                float luminance = dot(color, float3(0.2126, 0.7152, 0.0722));
                color = lerp(float3(luminance, luminance, luminance), color, _Saturation);
                color = (color - 0.5) * _Contrast + 0.5;

                float2 centered = i.uv - 0.5;
                float vignette = 1.0 - _VignetteStrength * smoothstep(0.18, 0.72, dot(centered, centered) * 2.0);
                color *= vignette;

                return fixed4(saturate(color), 1.0);
            }
            ENDCG
        }
    }

    Fallback Off
}
