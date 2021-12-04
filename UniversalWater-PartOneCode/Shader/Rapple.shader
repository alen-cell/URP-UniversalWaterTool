Shader "URP/Rapple"
{
    Properties
    {
        _Attenuation("Attenuation",Range(0,1)) = 0.99
        _h("Step",Range(0,1)) = 0.1
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
            float  _Attenuation;
            float _h;

            TEXTURE2D(_PrevRT);
            SAMPLER(sampler_PrevRT);

            TEXTURE2D(_CurrentRT);
            SAMPLER(sampler_CurrentRT);

            float4 _CurrentRT_TexelSize;
            float2 WAVE_DIR[4] = {float2(1,0),float2(0,1),float2(-1,0),float2(0,-1)};
          

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
                float p10 = SAMPLE_TEXTURE2D(_CurrentRT, sampler_CurrentRT, uv - e.zy).r;//下
                float p01 = SAMPLE_TEXTURE2D(_CurrentRT, sampler_CurrentRT, uv - e.xz).r;//左
                float p21 = SAMPLE_TEXTURE2D(_CurrentRT, sampler_CurrentRT, uv + e.xz).r;//
                float p12 = SAMPLE_TEXTURE2D(_CurrentRT, sampler_CurrentRT, uv + e.zy).r;
                float p00 = SAMPLE_TEXTURE2D(_CurrentRT, sampler_CurrentRT, uv).r;
                float c = 0.8;
                float h = 1.2;
                float p11 = SAMPLE_TEXTURE2D(_PrevRT, sampler_PrevRT, uv ).r;
                float d = c*c*(p10 + p01 + p21 + p12- 4*p00)/(h*h )- p11 + 2*p00;
                d*= 0.99;
                
               // float preRT = SAMPLE_TEXTURE2D(_PrevRT, sampler_PrevRT, uv).r;
               // d += preRT;
                return float4(d.rrrr);
               // return curWave;
                
            }
            ENDHLSL
        }
    }
}