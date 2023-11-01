Shader "Maxst/IBR Cull Back"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Texture1", 2D) = "black" {}
        _MainTex2("Texture2", 2D) = "black" {}
        _MainTex2_1("Texture2_1", 2D) = "black" {}
        _MainTex2_2("Texture2_2", 2D) = "black" {}
        _MainTex2_3("Texture2_3", 2D) = "black" {}
        _MainTex2_4("Texture2_4", 2D) = "black" {}
        _MainTex2_5("Texture2_5", 2D) = "black" {}
        _MainTex2_6("Texture2_6", 2D) = "black" {}
        _MainTex2_7("Texture2_7", 2D) = "black" {}
        _MainTex2_8("Texture2_8", 2D) = "black" {}
    }
    
    SubShader
    {
        Tags { "RenderType" = "Geometry" "Queue" = "Geometry-1000" "RenderPipeline" = "UniversalRenderPipeline"}
        LOD 100

        Pass
        {
            Cull Back
            ZTest LEqual
            ZWrite On
            Blend Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Ibr.cginc"
            

            ENDHLSL
        }

    }
}
