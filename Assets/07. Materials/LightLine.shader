// 조준선(LineRenderer)용 셰이더.
// LineRenderer의 색(버텍스 컬러)은 1을 넘길 수 없어서, 이 셰이더가 _Intensity 값을 곱해 HDR 밝기(1 이상)를 만들어준다.
// 밝기가 Bloom의 Threshold를 넘으면 선 주변이 번져서 빛나 보인다.
Shader "Celeritas/LightLine"
{
    Properties
    {
        _Intensity ("Intensity (HDR 밝기)", Float) = 3
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        // LightMode 태그를 일부러 넣지 않는다. (URP 2D 렌더러가 태그 없는 패스를 Unlit으로 그린다. 기본 Sprite-Unlit-Default와 같은 방식)
        Pass
        {
            Name "LightLine"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;   // LineRenderer가 넣어주는 색
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color      : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)
                float _Intensity;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.color = input.color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 c = input.color;
                c.rgb *= _Intensity;   // 색에 밝기를 곱해서 1 이상으로 만든다 (알파는 그대로)
                return c;
            }
            ENDHLSL
        }
    }
}
