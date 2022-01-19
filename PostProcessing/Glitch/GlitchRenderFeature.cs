using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GlitchRenderFeature : ScriptableRendererFeature
{
    public Shader glitchShader;
    GlitchRenderPass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        if (glitchShader == null)
        {
            Debug.LogError("故障shader不能为空");
        }
        m_ScriptablePass = new GlitchRenderPass(RenderPassEvent.BeforeRenderingPostProcessing, CoreUtils.CreateEngineMaterial(glitchShader));
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_ScriptablePass.Setup(renderer);
        renderer.EnqueuePass(m_ScriptablePass);
    }
}

class GlitchRenderPass : ScriptableRenderPass
{
    // This method is called before executing the render pass.
    // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
    // When empty this render pass will render to the active camera render target.
    // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
    // The render pipeline will ensure target setup and clearing happens in a performant manner.

    Material glitchMaterial;
    GlitchVolume m_GlitchVolume;
    ScriptableRenderer m_Renderer;

    static readonly string m_ProfilerTag = "Render glitch effects";
    static readonly int TempTarget = Shader.PropertyToID("Glitch temp");

    //shader参数
    static readonly int _RGBSPLITGLITCH_NoiseTex = Shader.PropertyToID("_RGBSPLITGLITCH_NoiseTex");
    static readonly int _RGBSPLITGLITCH_Speed = Shader.PropertyToID("_RGBSPLITGLITCH_Speed");
    static readonly int _RGBSPLITGLITCH_Amplitude = Shader.PropertyToID("_RGBSPLITGLITCH_Amplitude");

    static readonly int _IMAGEBLOCKGLITCH_BlockSize = Shader.PropertyToID("_IMAGEBLOCKGLITCH_BlockSize");
    static readonly int _IMAGEBLOCKGLITCH_Speed = Shader.PropertyToID("_IMAGEBLOCKGLITCH_Speed");
    static readonly int _IMAGEBLOCKGLITCH_MaxRGBSplit = Shader.PropertyToID("_IMAGEBLOCKGLITCH_MaxRGBSplit");
    public GlitchRenderPass(RenderPassEvent evt, Material blitMaterial)
    {
        renderPassEvent = evt;
        glitchMaterial = blitMaterial;
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
        m_GlitchVolume = stack.GetComponent<GlitchVolume>();

        if (m_GlitchVolume == null || !m_GlitchVolume.IsActive())
        {
            return;
        }
        if (!glitchMaterial.IsKeywordEnabled(m_GlitchVolume.mode.ToString()))
        {
            foreach (var keyword in glitchMaterial.shaderKeywords)
            {
                glitchMaterial.DisableKeyword(keyword);
            }
            glitchMaterial.EnableKeyword(m_GlitchVolume.mode.value.ToString());
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
        switch (m_GlitchVolume.mode.ToString())
        {
            case "None":
                break;
            case "_RGBSPLITGLITCH":
                Shader.SetGlobalTexture(_RGBSPLITGLITCH_NoiseTex, m_GlitchVolume._RGBSPLITGLITCH_NoiseTex.value);
                Shader.SetGlobalFloat(_RGBSPLITGLITCH_Speed, m_GlitchVolume._RGBSPLITGLITCH_Speed.value);
                Shader.SetGlobalFloat(_RGBSPLITGLITCH_Amplitude, m_GlitchVolume._RGBSPLITGLITCH_Amplitude.value);
                cmd.GetTemporaryRT(TempTarget, width, height, 0, FilterMode.Bilinear, desc.colorFormat);
                cmd.Blit(m_Renderer.cameraColorTarget, TempTarget);
                cmd.SetGlobalTexture(ShaderPropertyId.sourceTex, TempTarget);
                cmd.Blit(TempTarget, m_Renderer.cameraColorTarget, glitchMaterial);
                break;
            case "_IMAGEBLOCKGLITCH":
                Shader.SetGlobalFloat(_IMAGEBLOCKGLITCH_BlockSize, m_GlitchVolume._IMAGEBLOCKGLITCH_BlockSize.value);
                Shader.SetGlobalFloat(_IMAGEBLOCKGLITCH_Speed, m_GlitchVolume._IMAGEBLOCKGLITCH_Speed.value);
                Shader.SetGlobalVector(_IMAGEBLOCKGLITCH_MaxRGBSplit, m_GlitchVolume._IMAGEBLOCKGLITCH_MaxRGBSplit.value);
                cmd.GetTemporaryRT(TempTarget, width, height, 0, FilterMode.Bilinear, desc.colorFormat);
                cmd.Blit(m_Renderer.cameraColorTarget, TempTarget);
                cmd.SetGlobalTexture(ShaderPropertyId.sourceTex, TempTarget);
                cmd.Blit(TempTarget, m_Renderer.cameraColorTarget, glitchMaterial);
                break;
            case "_LINEBLOCKGLITCH":
                break;
            case "_TILEJITTERGLITCH":
                break;
            case "_SCANLINEJITTERGLITCH":
                break;
            case "_DIGITALSTRIPEGLITCH":
                break;
            case "_ANALOGNOISEGLITCH":
                break;
            case "_SCREENJUMPGLITCH":
                break;
            case "_SCREENSHAKEGLITCH":
                break;
            case "_WAVEJITTERGLITCH":
                break;
        }
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}


