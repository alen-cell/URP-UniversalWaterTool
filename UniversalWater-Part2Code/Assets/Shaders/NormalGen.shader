Shader "URP/NormalGenerator"
{
    Properties
    {

    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline""RenderType" = "Opaque" }



        Pass
        {
            Name"TriggerGeneratingPass"
            Tags {  "LightMode" = "UniversalForward"}

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Utils.hlsl"
            TEXTURE2D(_CurrentRT);
            SAMPLER(sampler_CurrentRT);
            float4 _CurrentRT_TexelSize;

            struct VertexInput {
                float4 positionOS :POSITION;
                float2 uv: TEXCOORD0;
            };

            struct VertexOutput {
                float4 positionCS:SV_POSITION;
                float2 uv:TEXCOORD0;
            };

            float3 UnpackDerivativeHeight(float4 textureData) {
                float3 dh = textureData.agb;
                dh.xy = dh.xy * 2 - 1;
                return dh;
            }


            VertexOutput vert(VertexInput v)
            {
                VertexOutput o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            float4 frag(VertexOutput i) : SV_Target
            {

                float lh = DecodeHeight(SAMPLE_TEXTURE2D(_CurrentRT, sampler_CurrentRT,i.uv+ float2(-_CurrentRT_TexelSize.x,0)));
                float rh = DecodeHeight(SAMPLE_TEXTURE2D(_CurrentRT, sampler_CurrentRT, i.uv+ float2(_CurrentRT_TexelSize.x, 0)));
                float bh = DecodeHeight(SAMPLE_TEXTURE2D(_CurrentRT, sampler_CurrentRT, i.uv + float2(0, -_CurrentRT_TexelSize.y)));
                float th = DecodeHeight(SAMPLE_TEXTURE2D(_CurrentRT, sampler_CurrentRT, i.uv + float2(0, _CurrentRT_TexelSize.y)));
            
                
                float3 normal = float3((lh - rh),(bh - th), 5 * _CurrentRT_TexelSize.x);
             // float3 normal = cross(va, vb);
               
              // return float4(normal, 1);
              // return float4((lh - rh), bh - th, 2* _CurrentRT_TexelSize.x,1);


#if defined(UNITY_NO_DXT5nm)
                return float4(normal*0.5+0.5, 1.0);
#else
#if UNITY_VERSION>2018
                return float4(normal.x*0.5+0.5,normal.y*0.5+0.5,0,1);
#else
                return float4(0, normal.y*0.5+0.5 , 0, normal.x*0.5+0.5);
#endif
#endif
                 }
                 ENDHLSL
             }
    }
}
