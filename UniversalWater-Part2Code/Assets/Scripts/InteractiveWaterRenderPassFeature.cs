using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;



public class InteractiveWaterRenderPassFeature : ScriptableRendererFeature
{

    [System.Serializable]
    public class Settings
    {
        
        public Material TriggerInputMat;

        public Material WaterTransmitMat;
        public Material WaterMaterial;
        public Material NormalGeneratorMaterial;

       public RenderTexture InteractiveRT;

    

        public bool isPointerInteractive = false;
        
        


        [Range(0, 1.0f)]
        public float drawRadius = 0.1f;
        [Range(0, 1.0f)]
        public float waveAttenuation = 0.99f;

        [Range(0, 1.0f)]

        public float WaveSpeed = 0.5f;
        [Range(0, 1.0f)]

        public float WaveViscosity = 0.15f; //粘度
        public float WaveHeight = 0.999f;

        
        public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;
        public int RenderTextureSize = 128;
     

    }


    public Settings setting = new Settings();





    class WaterRenderPass : ScriptableRenderPass
    {

        public Settings setting;
        public ShaderTagId shaderTag = new ShaderTagId("UniversalForward");
        public FilteringSettings filteringSetting;
        RenderTexture prevRT;
        RenderTexture currentRT;
        RenderTexture tempRT;
        RenderTexture normalRT;

        private Material WaterTransmitMaterial;
        private Vector4 WaterTransmitParams;
        private Vector4 WaterMarkParams;
 
        //采样计数器
        //private int m_sampleCounter;

        private void InitRT()
        {
            currentRT = new RenderTexture(setting.RenderTextureSize, setting.RenderTextureSize, 0, RenderTextureFormat.Default);
            prevRT = new RenderTexture(setting.RenderTextureSize, setting.RenderTextureSize, 0, RenderTextureFormat.Default);
            tempRT = new RenderTexture(setting.RenderTextureSize, setting.RenderTextureSize, 0, RenderTextureFormat.Default);
            normalRT = new RenderTexture(setting.RenderTextureSize, setting.RenderTextureSize, 0, RenderTextureFormat.Default);
           

        }

        private void ExchangeRT(ref RenderTexture a, ref RenderTexture b)
        {

            RenderTexture rt = a;
            a = b;
            b = rt;
        }
        //波动方程
        void InitWaveTransmitParams()
        {
            float uvStep = 1.0f / setting.RenderTextureSize;
            float dt = Time.fixedDeltaTime;
            float maxWaveStepVisosity = uvStep / (2 * dt) * (Mathf.Sqrt(setting.WaveViscosity * dt + 2));
            //粘度平方
            float waveVisositySqr = setting.WaveViscosity * setting.WaveViscosity;
            float curWaveSpeed = maxWaveStepVisosity * setting.WaveSpeed;
            float ut = setting.WaveViscosity * dt;

            float f1 = curWaveSpeed * curWaveSpeed * dt * dt / (uvStep * uvStep);
            float f2 = 1.0f / (ut + 2);

            float k1 = (4.0f - 8.0f * f1) * f2;
            float k2 = (ut - 2) * f2;
            float k3 = 2.0f * f1 * f2;

            WaterTransmitParams.Set(k1, k2, k3, uvStep);
            //Debug.LogFormat("k1,k2,k3", k1, k2, k3);

        }


        public WaterRenderPass(Settings setting)
        {
            this.setting = setting;
            RenderQueueRange queue = new RenderQueueRange();
            filteringSetting = new FilteringSettings(queue);
            CameraRayCast.drawRadius = setting.drawRadius;
   
            InitRT();
            InitWaveTransmitParams();

            // m_sampleCounter = setting.sampleTextureCount;

        }
        //渲染之前计算涟漪
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {


            
            if (setting.TriggerInputMat != null && setting.WaterTransmitMat != null)
            {
              
                if (setting.isPointerInteractive == true && CameraRayCast.isRayCast == true)
                {
                    setting.TriggerInputMat.SetVector("_HitPoint", CameraRayCast.currentPos);
                    setting.TriggerInputMat.SetFloat("_isRenderMousePointer", setting.isPointerInteractive == true ? 1 : 0);
                    if (setting.InteractiveRT != null)
                    {
                        setting.TriggerInputMat.SetTexture("_InteractiveTex", setting.InteractiveRT);
                    }
                  
                    setting.TriggerInputMat.SetTexture("_CurrentRT", currentRT);
                    cmd.Blit(null, tempRT, setting.TriggerInputMat);
                    ExchangeRT(ref tempRT, ref currentRT);
                   
                }
                
                else
                {
                    setting.TriggerInputMat.SetVector("_HitPoint",new Vector4(0,0,0,0));
                    if (setting.InteractiveRT != null)
                    {
                        setting.TriggerInputMat.SetTexture("_InteractiveTex", setting.InteractiveRT);
                    }
                    
                    setting.TriggerInputMat.SetTexture("_CurrentRT", currentRT);
                 
                    cmd.Blit(null, tempRT, setting.TriggerInputMat);
                    ExchangeRT(ref tempRT, ref currentRT);
                }

                


                   // 传入上一帧RT
                setting.WaterTransmitMat.SetVector("_WaterTransmitParams", WaterTransmitParams);
                setting.WaterTransmitMat.SetTexture("_PrevRT", prevRT);
                setting.WaterTransmitMat.SetTexture("_CurrentRT", currentRT);
                setting.WaterTransmitMat.SetFloat("_Attenuation", setting.waveAttenuation);
                
                setting.NormalGeneratorMaterial.SetTexture("_CurrentRT", currentRT);
                cmd.Blit(null, normalRT, setting.NormalGeneratorMaterial);
                setting.WaterMaterial.SetTexture("_NormalRT", normalRT);

                cmd.Blit(null, tempRT, setting.WaterTransmitMat);
                cmd.Blit(tempRT, prevRT);
                ExchangeRT(ref prevRT, ref currentRT);
                

            }




        }

        //执行渲染
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {

             if (setting.WaterMaterial != null)
            {
                var drawFrame = CreateDrawingSettings(shaderTag, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
                setting.WaterMaterial.SetTexture("_NormalRT", normalRT);
                drawFrame.overrideMaterial = setting.WaterMaterial;
                drawFrame.overrideMaterialPassIndex = 0;
                context.DrawRenderers(renderingData.cullResults, ref drawFrame, ref filteringSetting);
            }
 
        }


        public override void FrameCleanup(CommandBuffer cmd)
        {
            //tempRT.Release();
            
        }
    }

    WaterRenderPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new WaterRenderPass(setting);

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = setting.Event;
    }


    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


