// This shader fills the mesh shape with a color predefined in the code.
Shader "MaxstAR/ARURP"
{
    // The properties block of the Unity shader. In this example this block is empty
    // because the output color is predefined in the fragment shader code.
    Properties
    {
        _MainTex ("Base (BGRA)", 2D) = "black" {}
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

            // This macro declares _BaseMap as a Texture2D object.
            TEXTURE2D(_MainTex);
            // This macro declares the sampler for the _BaseMap texture.
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                // The following line declares the _BaseMap_ST variable, so that you
                // can use the _BaseMap variable in the fragment shader. The _ST 
                // suffix is necessary for the tiling and offset function to work.
                float4 _MainTex_ST;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                // The TRANSFORM_TEX macro performs the tiling and offset
                // transformation.
                OUT.uv = IN.uv;//TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float b = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv).b;
                float g = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv).g;
                float r = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv).r;

                return half4(r, g, b, 1.0);

                // The SAMPLE_TEXTURE2D marco samples the texture with the given
                // sampler.
                //half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                //return color;
            }
            ENDHLSL
        }
    }

/*
    SubShader
    {
        // SubShader Tags define when and under which conditions a SubShader block or
        // a pass is executed.
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
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
            };
         

            TEXTURE2D(_MainTex);

            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
            CBUFFER_END
 

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                // The TRANSFORM_TEX macro performs the tiling and offset
                // transformation.
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float b = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv).b;
                float g = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv).g;
                float r = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv).r;

                return half4(r, g, b, 1.0);
            }
            ENDHLSL
        }
    }
*/
}