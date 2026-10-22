// 배경용 Unlit 셰이더: 기존 Unlit/Transparent와 똑같이 동작하되, 색조(_Color)와 명도(_Brightness)를 조절할 수 있다.
// - 텍스처 Offset/Tiling(_MainTex_ST)을 그대로 적용하므로 Background.cs의 mainTextureOffset 스크롤이 계속 동작한다.
// - 라이트(Light2D)의 영향은 받지 않는다 (Unlit).
Shader "Celeritas/BackgroundUnlit"
{
    Properties
    {
        [MainTexture] _MainTex ("Texture", 2D) = "white" {}
        [MainColor] _Color ("Tint (색조)", Color) = (1, 1, 1, 1)
        _Brightness ("Brightness (명도, 1 = 원본)", Range(0, 2)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        // LightMode 태그를 일부러 달지 않는다 (URP 2D 렌더러가 태그 없는 패스를 그려준다)
        Pass
        {
            Name "BackgroundUnlit"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half _Brightness;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR; // SpriteRenderer의 Color가 들어온다
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex); // Offset/Tiling 적용 (스크롤이 되는 이유)
                OUT.color = IN.color * _Color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * IN.color;
                c.rgb *= _Brightness; // 명도: 1보다 작으면 어두워지고, 크면 밝아진다 (알파는 그대로)
                return c;
            }
            ENDHLSL
        }
    }
}
