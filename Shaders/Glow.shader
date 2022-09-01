Shader "My Shaders/Glow"
{
    Properties
    {
        _ColorA("Color A", Color) = (1,0,0,1)
        _ColorB("Color B", Color) = (1,0,0,1)
        _ColorStart("Color Start", Range(0, 1)) = 0
        _ColorEnd("Color End", Range(0, 1)) = 1
        _Freq1 ("Frequency X", float) = 8
        _Freq2 ("Frequency Y", float) = 8
        _Test ("Test", float) = 1
    }
        SubShader
    {
        Tags { 
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Pass
        {
            Cull Front
            ZWrite Off
            Blend One One

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            #define TAU 6.28318530718

            float4 _ColorA;
            float4 _ColorB;
            float _ColorStart;
            float _ColorEnd;
            float _Freq1;
            float _Freq2;
            float _Test;

            struct MeshData
            {
                float4 vertex : POSITION;
                float3 normals : NORMAL;
                float4 uv0 : TEXCOORD0;
            };

            struct Interpolators
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Interpolators vert (MeshData v)
            {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv0;
                return o;
            }

            float4 frag (Interpolators i) : SV_Target
            {
                float xOffset = cos(i.uv.x * TAU * _Freq1) * 0.01;
                float t = cos((i.uv.y + xOffset - _Time.y * 0.1) * TAU * _Freq2) * 0.5 + 0.5;
                t *= 1 - i.uv.y;

                float4 outColor = lerp(_ColorA, _ColorB, i.uv.y);
                return t * outColor;
            }
            ENDCG
        }
    }
}
