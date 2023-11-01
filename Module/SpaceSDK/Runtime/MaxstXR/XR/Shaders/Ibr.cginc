//#include "UnityCG.cginc"
#define UNITY_PI 3.14159265359f
#define kMaxRefViews 2

CBUFFER_START(UnityPerMaterial)
    
    // TEXTURE2D(_MainTex);
    // SAMPLER(_MainTex);
    sampler2D _MainTex;
    sampler2D _MainTex2;

    sampler2D _MainTex2_1;
    sampler2D _MainTex2_2;
    sampler2D _MainTex2_3;
    sampler2D _MainTex2_4;
    sampler2D _MainTex2_5;
    sampler2D _MainTex2_6;
    sampler2D _MainTex2_7;
    sampler2D _MainTex2_8;
    
    half4 _MainTex_ST;
CBUFFER_END

inline float3 FrameObjectToViewPos(in float4x4 m, in float3 pos)
{
    return mul(m, mul(unity_ObjectToWorld, float4(pos, 1.0))).xyz;
}

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
};

struct v2f
{
    float2 uv : TEXCOORD0;
    float fogCoord  : TEXCOORD1;
    //UNITY_FOG_COORDS(1)
    float4 vertex : SV_POSITION;
    float3 positionOS : VAR_POSITION;
};

int _validTexture1;
int _validTexture2;
int _validTexture3;
int _validTexture4;
int _validTexture5;
int _validTexture6;
int _validTexture7;
int _validTexture8;
float4x4 frame_MatrixV[kMaxRefViews];
float4 frame_Data;

v2f vert (appdata v)
{
    v2f o;
    o.positionOS = v.vertex.xyz;
    o.vertex = TransformObjectToHClip(v.vertex.xyz);
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    o.fogCoord = ComputeFogFactor (o.vertex.z);
    return o;
}

half4 frag(v2f i) : SV_Target
{
    float t = frame_Data.x;
    float wSum = 0;
    half4 cSum = half4(0, 0, 0, 1);

    for (int refViewIdx = 0; refViewIdx < kMaxRefViews; ++refViewIdx)
    {
        float4x4 refWorldToLocal = frame_MatrixV[refViewIdx];
        float3 posRefVS = FrameObjectToViewPos(refWorldToLocal, i.positionOS);
        float theta = atan2(posRefVS.x, posRefVS.z) / UNITY_PI;
        float phi = acos(posRefVS.y / length(posRefVS)) / UNITY_PI;
        float2 uv_original = float2(0.5 + theta * 0.5, phi);
        float uconv = (uv_original.x - floor(uv_original.x/0.25)*0.25)/0.25;
        float vconv = (uv_original.y - floor(uv_original.y/0.5)*0.5)/0.5;
        float2 uvconv = float2(uconv, vconv);

        half4 c = half4(0.0,0,0,1);
        float w;
        if (refViewIdx == 0)
        {
            c = tex2D(_MainTex, uv_original);
            w = 1 - t;
        }
        else
        {
            c = tex2D(_MainTex2, uv_original);
            //c = fixed4(0.0,1,0,1);
            if(_validTexture1 == 1) {
                if(uv_original.x < 0.25) {
                    if(uv_original.y > 0.5) {
                        c = tex2D(_MainTex2_1, uvconv);
                    }
                }
            }

            if(_validTexture2 == 1) {
                if(uv_original.x > 0.25 && uv_original.x < 0.5) {
                    if(uv_original.y > 0.5) {
                        c = tex2D(_MainTex2_2, uvconv);
                    }
                }
            }

            if(_validTexture3 == 1) {
                if(uv_original.x > 0.5 && uv_original.x < 0.75) {
                    if(uv_original.y > 0.5) {
                        c = tex2D(_MainTex2_3, uvconv);
                    }
                }
            }

            if(_validTexture4 == 1) {
                if(uv_original.x > 0.75) {
                    if(uv_original.y > 0.5) {
                        c = tex2D(_MainTex2_4, uvconv);
                    }
                }
            }

            if(_validTexture5 == 1) {
                if(uv_original.x < 0.25) {
                    if(uv_original.y < 0.5) {
                        c = tex2D(_MainTex2_5, uvconv);
                    }
                }
            }

            if(_validTexture6 == 1) {
                if(uv_original.x > 0.25 && uv_original.x < 0.5) {
                    if(uv_original.y < 0.5) {
                        c = tex2D(_MainTex2_6, uvconv);
                    }
                }
            }

            if(_validTexture7 == 1) {
                if(uv_original.x > 0.5 && uv_original.x < 0.75) {
                    if(uv_original.y < 0.5) {
                        c = tex2D(_MainTex2_7, uvconv);
                    }
                }
            }

            if(_validTexture8 == 1) {
                if(uv_original.x > 0.75) {
                    if(uv_original.y < 0.5) {
                        c = tex2D(_MainTex2_8, uvconv);
                    }
                }
            }

            w = t;
        }
        wSum += w;
        cSum += w * c;
    }
/*
    else if(_textureCount == 1)
    {
        for (int refViewIdx = 0; refViewIdx < kMaxRefViews; ++refViewIdx)
        {
            float4x4 refWorldToLocal = frame_MatrixV[refViewIdx];
            float3 posRefVS = FrameObjectToViewPos(refWorldToLocal, i.positionOS);
            float theta = atan2(posRefVS.x, posRefVS.z) / UNITY_PI;
            float phi = acos(posRefVS.y / length(posRefVS)) / UNITY_PI;

            fixed4 c = fixed4(0.0,0,0,1);
            float w;
            float2 uv_original = float2(0.5 + theta * 0.5, phi);
            float uconv = (uv_original.x - floor(uv_original.x/0.25)*0.25)/0.25;
            float vconv = (uv_original.y - floor(uv_original.y/0.5)*0.5)/0.5;
            float2 uvconv = float2(uconv, vconv);
            if (0 != refViewIdx)
            {

                if(uv_original.x < 0.25) {
                    if(uv_original.y < 0.5) {
                        c = tex2D(_MainTex2_5, uvconv);
                    }
                    else {
                        c = tex2D(_MainTex2_1, uvconv);
                    }
                }
                else if(uv_original.x > 0.25 && uv_original.x < 0.5) {
                    if(uv_original.y < 0.5) {
                        c = tex2D(_MainTex2_6, uvconv);
                    }
                    else {
                        c = tex2D(_MainTex2_2, uvconv);
                    }
                }
                else if(uv_original.x > 0.5 && uv_original.x < 0.75) {
                    if(uv_original.y < 0.5) {
                        c = tex2D(_MainTex2_7, uvconv);
                    }
                    else {
                        c = tex2D(_MainTex2_3, uvconv);
                    }
                }
                else if(uv_original.x > 0.7) {
                    if(uv_original.y < 0.5) {
                        c = tex2D(_MainTex2_8, uvconv);
                    }
                    else {
                        c = tex2D(_MainTex2_4, uvconv);
                    }
                }
                
           
                w = t;
            }
            wSum += w;
            cSum += w * c;
        }
    }
*/

    cSum /= wSum;
    cSum.rgb = MixFog(cSum.rgb, i.fogCoord);
    return cSum;
}