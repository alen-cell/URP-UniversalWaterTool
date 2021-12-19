Shader "URP/Rapple"
{
    Properties
    {
        _Attenuation("Attenuation",Range(0,1)) = 0.99
        
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline""RenderType" = "Opaque" }

        

        Pass
        {
            Name"WaterTransmitPass"
            Tags {  "LightMode" = "UniversalForward"}

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Utils.hlsl"
            float  _Attenuation;
            float _h;
            float4 _WaterTransmitParams;

            TEXTURE2D(_PrevRT);
            SAMPLER(sampler_PrevRT);

            TEXTURE2D(_CurrentRT);
            SAMPLER(sampler_CurrentRT);

            float4 _CurrentRT_TexelSize;
            //float2 WAVE_DIR[4] = {float2(1,0),float2(0,1),float2(-1,0),float2(0,-1)};
          

            struct VertexInput{
                float4 positionOS :POSITION;
                float2 uv: TEXCOORD0;
            };

            struct VertexOutput{
                float4 positionCS:SV_POSITION;
                float2 uv:TEXCOORD0;
            };

            

            VertexOutput vert (VertexInput v)
            {
                VertexOutput o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            float4 frag(VertexOutput i) : SV_Target
            {
       

              
                float3 e = float3(_CurrentRT_TexelSize.xy,0);
                float2 uv = i.uv;
              
               float avgCur = DecodeHeight(SAMPLE_TEXTURE2D(_CurrentRT, sampler_CurrentRT, float2(uv.x,uv.y + e.y)))
                + DecodeHeight(SAMPLE_TEXTURE2D(_CurrentRT, sampler_CurrentRT, float2(uv.x, uv.y - e.y)))
                + DecodeHeight(SAMPLE_TEXTURE2D(_CurrentRT, sampler_CurrentRT, float2(uv.x - e.x, uv.y)))
                   +DecodeHeight(SAMPLE_TEXTURE2D(_CurrentRT, sampler_CurrentRT, float2(uv.x + e.x, uv.y)));



                 float curnCur = DecodeHeight(SAMPLE_TEXTURE2D(_CurrentRT, sampler_CurrentRT, uv));
                 float prevCur = DecodeHeight(SAMPLE_TEXTURE2D(_PrevRT, sampler_PrevRT, uv));

                //k1*cur,k2*pre,k3*avg;
               float d = _WaterTransmitParams.z*avgCur + _WaterTransmitParams.y*prevCur + _WaterTransmitParams.x*curnCur;
                
              
                d*= _Attenuation;
              //  d *= 0.5 + 0.5;
                
               // return d.rrrr;
                return EncodeHeight(d);
               // return curWave;
                
            }
            ENDHLSL
        }
    }
}