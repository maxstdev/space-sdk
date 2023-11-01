// This shader fills the mesh shape with a color that a user can change using the
// Inspector window on a Material.
Shader "Unlit/PovShader"
{
    // The _BaseColor variable is visible in the Material's Inspector, as a field 
    // called Base Color. You can use it to select a custom color. This variable
    // has the default value (1, 1, 1, 1).
    Properties
    {
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _MainTex("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalRenderPipeline" }

        Cull Off
        ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"            

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 texcoord     : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 texcoord     : TEXCOORD0;
            };

            sampler2D _MainTex;

            // To make the Unity shader SRP Batcher compatible, declare all
            // properties related to a Material in a a single CBUFFER block with 
            // the name UnityPerMaterial.
            CBUFFER_START(UnityPerMaterial)
                // The following line declares the _BaseColor variable, so that you
                // can use it in the fragment shader.
                half4 _BaseColor;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.texcoord = IN.texcoord;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Returning the _BaseColor value.                
                // Returning the _BaseColor value.                
                half4 color = tex2D(_MainTex, IN.texcoord);
                return _BaseColor * color;
            }
            ENDHLSL
        }
    }
}