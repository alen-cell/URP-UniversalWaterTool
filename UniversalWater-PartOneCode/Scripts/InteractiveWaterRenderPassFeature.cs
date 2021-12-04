using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class InteractiveWaterRenderPassFeature : ScriptableRendererFeature
{

    [System.Serializable]
    public class Settings
    {
        public Material TriggerInputMat;

        public Material RippleMat;
        public Material WaterMaterial;

        public RenderTexture InteractiveRT;

        public int sampleTextureCount;

        public bool isPointerInteractive = false;

        [Range(0, 1.0f)]
        public float drawRadius = 0.1f;
        [Range(0, 1.0f)]
        public float waveAttenuation = 0.99f;

        public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;
        public int RenderTextureSize = 512;

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

        //采样计数器
        //private int m_sampleCounter;

        private void InitRT()  
        {
            currentRT = new RenderTexture(setting.RenderTextureSize, setting.RenderTextureSize, 0, RenderTextureFormat.RFloat);
            prevRT = new RenderTexture(setting.RenderTextureSize, setting.RenderTextureSize, 0, RenderTextureFormat.RFloat);
          
            tempRT = new RenderTexture(setting.RenderTextureSize, setting.RenderTextureSize, 0, RenderTextureFormat.RFloat);
     


        }

        private void ExchangeRT(ref RenderTexture a , ref RenderTexture b)
        {

            RenderTexture rt = a;
            a = b;
            b = rt;
        }

        public WaterRenderPass(Settings setting)
        {
            this.setting = setting;
            RenderQueueRange queue= new RenderQueueRange();
            filteringSetting = new FilteringSettings(queue);
            CameraRayCast.drawRadius = setting.drawRadius;

            InitRT();
           // m_sampleCounter = setting.sampleTextureCount;
            
        }
        //渲染之前计算涟漪
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {


            
            if (setting.TriggerInputMat != null && setting.RippleMat != null)
                {

                

                
                    if (setting.isPointerInteractive == true && CameraRayCast.isRayCast == true)
                    {
                        setting.TriggerInputMat.SetVector("_HitPoint", CameraRayCast.currentPos);
                        setting.TriggerInputMat.SetFloat("_isRenderMousePointer", setting.isPointerInteractive == true ? 1 : 0);
                         setting.TriggerInputMat.SetTexture("_CurrentRT", currentRT);
                        cmd.Blit(null, tempRT, setting.TriggerInputMat);
                         ExchangeRT(ref tempRT, ref currentRT);
                }

               

                // 传入触发器的数值

                //    setting.TriggerInputMat.SetTexture("_InteractiveTex", setting.InteractiveRT);
                //        // 传入上一帧RT

                setting.RippleMat.SetTexture("_PrevRT", prevRT);
                setting.RippleMat.SetTexture("_CurrentRT", currentRT);
                setting.RippleMat.SetFloat("_Attenuation", setting.waveAttenuation);

                cmd.Blit(null, tempRT, setting.RippleMat);
                cmd.Blit(tempRT,prevRT);

                ExchangeRT(ref prevRT, ref currentRT);







            }
            



        }

        //执行渲染
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {

    if (setting.WaterMaterial != null)
    {
        var drawFrame = CreateDrawingSettings(shaderTag, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
        setting.WaterMaterial.SetTexture("_SourceTex", currentRT);
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


