Shader "URP/waterTriggerInput"
{
    Properties
    {
        
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline""RenderType" = "Opaque" }

            

        Pass
        {
            Name"TriggerGeneratingPass"
            Tags {  "LightMode" = "UniversalForward"}

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Utils.hlsl"
            float _isRenderMousePointer;
            float4 _HitPoint;
        
           

            TEXTURE2D(_InteractiveTex);
			SAMPLER(sampler_InteractiveTex);

			TEXTURE2D(_CurrentRT);
            SAMPLER(sampler_CurrentRT);

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
               
                half c = DecodeHeight(SAMPLE_TEXTURE2D(_CurrentRT,sampler_CurrentRT,i.uv));
                c += _isRenderMousePointer*(max(0,_HitPoint.z - length(i.uv - _HitPoint.xy)));
               
                c += saturate(SAMPLE_TEXTURE2D(_InteractiveTex, sampler_InteractiveTex, i.uv).r);
               
    
                return EncodeHeight(c);
           
            }
            ENDHLSL
        }
    }
}
