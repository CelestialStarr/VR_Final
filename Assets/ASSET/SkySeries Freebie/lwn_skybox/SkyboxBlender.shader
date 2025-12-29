Shader "Skybox/Blended"
{
    Properties
    {
        _Tint ("Tint Color", Color) = (.5, .5, .5, .5)
        [Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
        _Rotation ("Rotation", Range(0, 360)) = 0
        [NoScaleOffset] _TexA ("Day Cubemap (Day)", Cube) = "grey" {}
        [NoScaleOffset] _TexB ("Night Cubemap (Night)", Cube) = "grey" {}
        _Blend ("Blend (0=Day, 1=Night)", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"

            samplerCUBE _TexA;
            samplerCUBE _TexB;
            half4 _TexA_HDR;
            half4 _TexB_HDR;
            half4 _Tint;
            half _Exposure;
            float _Rotation;
            float _Blend;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float3 RotateAroundYInDegrees (float3 vertex, float degrees)
            {
                float alpha = degrees * UNITY_PI / 180.0;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float3(mul(m, vertex.xz), vertex.y).xzy;
            }

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                float3 rotated = RotateAroundYInDegrees(v.vertex.xyz, _Rotation);
                o.vertex = UnityObjectToClipPos(rotated);
                o.texcoord = v.vertex.xyz;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 texA = texCUBE(_TexA, i.texcoord);
                half4 texB = texCUBE(_TexB, i.texcoord);

                // 解码 HDR (如果有的话)
                half3 cA = DecodeHDR(texA, _TexA_HDR);
                half3 cB = DecodeHDR(texB, _TexB_HDR);

                // 混合
                half3 finalColor = lerp(cA, cB, _Blend);

                // 应用曝光和染色
                finalColor = finalColor * _Tint.rgb * unity_ColorSpaceDouble.rgb;
                finalColor *= _Exposure;

                return half4(finalColor, 1);
            }
            ENDCG
        }
    }
    Fallback Off
}