using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MyBlurRenderFeature : ScriptableRendererFeature
{
    public Shader myBlurShader;

    MyBlurRenderPass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        if (myBlurShader == null)
        {
            Debug.LogError("模糊shader不能为空");
        }
        m_ScriptablePass = new MyBlurRenderPass(RenderPassEvent.BeforeRenderingPostProcessing, CoreUtils.CreateEngineMaterial(myBlurShader));
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_ScriptablePass.Setup(renderer);
        renderer.EnqueuePass(m_ScriptablePass);
    }
}

class MyBlurRenderPass : ScriptableRenderPass
{
    // This method is called before executing the render pass.
    // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
    // When empty this render pass will render to the active camera render target.
    // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
    // The render pipeline will ensure target setup and clearing happens in a performant manner.

    Material myBlurMaterial;
    MyBlurVolume m_MyBlurVolume;
    ScriptableRenderer m_Renderer;

    static readonly string m_ProfilerTag = "Render blur effects";
    static readonly int TempTarget1 = Shader.PropertyToID("Blur temp1");
    static readonly int TempTarget2 = Shader.PropertyToID("Blur temp2");

    //shader参数
    static readonly int _GAUSSIANBLUR_Offsets = Shader.PropertyToID("_GAUSSIANBLUR_Offsets");
    public MyBlurRenderPass(RenderPassEvent evt, Material blitMaterial)
    {
        renderPassEvent = evt;
        myBlurMaterial = blitMaterial;
    }
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {

    }

    // Here you can implement the rendering logic.
    // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
    // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
    // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var stack = VolumeManager.instance.stack;
        m_MyBlurVolume = stack.GetComponent<MyBlurVolume>();

        if (m_MyBlurVolume == null || !m_MyBlurVolume.IsActive())
        {
            return;
        }
        if (!myBlurMaterial.IsKeywordEnabled(m_MyBlurVolume.mode.ToString()))
        {
            foreach (var keyword in myBlurMaterial.shaderKeywords)
            {
                myBlurMaterial.DisableKeyword(keyword);
            }
            myBlurMaterial.EnableKeyword(m_MyBlurVolume.mode.value.ToString());
        }

        UpdateMaterial(context, ref renderingData);
    }

    // Cleanup any allocated resources that were created during the execution of this render pass.
    public override void OnCameraCleanup(CommandBuffer cmd)
    {

    }

    public void Setup(ScriptableRenderer m_Renderer)
    {
        this.m_Renderer = m_Renderer;
    }

    void UpdateMaterial(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
        RenderTextureDescriptor desc = renderingData.cameraData.cameraTargetDescriptor;
        int width = desc.width;
        int height = desc.height;
        switch (m_MyBlurVolume.mode.ToString())
        {
            case "None":
                break;
            case "_GAUSSIANBLUR":

                cmd.GetTemporaryRT(TempTarget1, (int)(width / m_MyBlurVolume._GAUSSIANBLUR_Downscale.value), (int)(height / m_MyBlurVolume._GAUSSIANBLUR_Downscale.value), 0, FilterMode.Bilinear, desc.colorFormat);
                cmd.GetTemporaryRT(TempTarget2, (int)(width / m_MyBlurVolume._GAUSSIANBLUR_Downscale.value), (int)(height / m_MyBlurVolume._GAUSSIANBLUR_Downscale.value), 0, FilterMode.Bilinear, desc.colorFormat);

                cmd.Blit(m_Renderer.cameraColorTarget, TempTarget1);
                for (int i = 1; i <= m_MyBlurVolume._GAUSSIANBLUR_Iterations.value; i++)
                {
                    cmd.SetGlobalVector(_GAUSSIANBLUR_Offsets, new Vector4(m_MyBlurVolume._GAUSSIANBLUR_Offsets.value / width, 0, 0, 0));
                    cmd.SetGlobalTexture(ShaderPropertyId.sourceTex, TempTarget1);
                    cmd.Blit(TempTarget1, TempTarget2, myBlurMaterial);
                    cmd.SetGlobalVector(_GAUSSIANBLUR_Offsets, new Vector4(0, m_MyBlurVolume._GAUSSIANBLUR_Offsets.value / height, 0, 0));
                    cmd.SetGlobalTexture(ShaderPropertyId.sourceTex, TempTarget2);
                    cmd.Blit(TempTarget2, TempTarget1, myBlurMaterial);
                }
                cmd.Blit(TempTarget1, m_Renderer.cameraColorTarget);
                break;
            case "_BOXBLUR":
                break;
            case "_KAWASEBLUR":
                break;
            case "_DUALBLUR":
                break;
            case "_BOKEHBLUR":
                break;
            case "_TILTSHIFTBLUR":
                break;
            case "_IRISBLUR":
                break;
            case "_GRAINYBLUR":
                break;
            case "_RADIALBLUR":
                break;
            case "_DIRECTIONALBLUR":
                break;
        }
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}
