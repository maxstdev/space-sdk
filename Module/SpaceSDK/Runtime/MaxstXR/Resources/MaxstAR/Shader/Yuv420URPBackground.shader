Shader "MaxstAR/Yuv420URPBackground"
{
    // The properties block of the Unity shader. In this example this block is empty
    // because the output color is predefined in the fragment shader code.
    Properties
    {
         _YTex ("Y channel texture", 2D) = "black" {}
        _UVTex ("UV channel texture", 2D) = "black" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"            

            struct Attributes
            {
                float4 positionOS   : POSITION;
                // The uv variable contains the UV coordinate on the texture for the
                // given vertex.
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                // The uv variable contains the UV coordinate on the texture for the
                // given vertex.
                float2 uv           : TEXCOORD0;
            };

            TEXTURE2D(_YTex);
            SAMPLER(sampler_YTex);

            TEXTURE2D(_UVTex);
            SAMPLER(sampler_UVTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _YTex_ST;
                float4 _UVTex_ST;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                // The TRANSFORM_TEX macro performs the tiling and offset
                // transformation.
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {

                float y = SAMPLE_TEXTURE2D(_YTex, sampler_YTex, IN.uv).r;

                float4 ycbcr = float4(y, SAMPLE_TEXTURE2D(_UVTex, sampler_UVTex, IN.uv).rg, 1.0);

				const float4x4 ycbcrToRGBTransform = float4x4(
						float4(1.0, +0.0000, +1.4020, -0.7010),
						float4(1.0, -0.3441, -0.7141, +0.5291),
						float4(1.0, +1.7720, +0.0000, -0.8860),
						float4(0.0, +0.0000, +0.0000, +1.0000)
					);

                return mul(ycbcrToRGBTransform, ycbcr);
            }
            ENDHLSL
        }
    }
}