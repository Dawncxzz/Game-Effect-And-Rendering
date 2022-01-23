using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GhostEffectRenderFeature : ScriptableRendererFeature
{
    public Shader ghostEffectShader;

    GhostEffectRenderPass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        if (ghostEffectShader == null)
        {
            Debug.LogError("转场shader不能为空");
        }
        m_ScriptablePass = new GhostEffectRenderPass(RenderPassEvent.BeforeRenderingPostProcessing, CoreUtils.CreateEngineMaterial(ghostEffectShader));
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_ScriptablePass.Setup(renderer);
        renderer.EnqueuePass(m_ScriptablePass);
    }
}
class GhostEffectRenderPass : ScriptableRenderPass
{
    // This method is called before executing the render pass.
    // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
    // When empty this render pass will render to the active camera render target.
    // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
    // The render pipeline will ensure target setup and clearing happens in a performant manner.

    Material ghostEffectMaterial;
    GhostEffectVolume m_GhostEffectVolume;
    ScriptableRenderer m_Renderer;

    static readonly string m_ProfilerTag = "Render ghost effects";
    static readonly int TempTarget = Shader.PropertyToID("Ghost Effect temp");

    //shader参数
    static readonly int _PLAYER_Width = Shader.PropertyToID("_PLAYER_Width");
    static readonly int _PLAYER_Progress = Shader.PropertyToID("_PLAYER_Progress");

    public GhostEffectRenderPass(RenderPassEvent evt, Material blitMaterial)
    {
        renderPassEvent = evt;
        ghostEffectMaterial = blitMaterial;
    }


    // Here you can implement the rendering logic.
    // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
    // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
    // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var stack = VolumeManager.instance.stack;
        m_GhostEffectVolume = stack.GetComponent<GhostEffectVolume>();

        if (m_GhostEffectVolume == null || !m_GhostEffectVolume.IsActive())
        {
            return;
        }
        if (!ghostEffectMaterial.IsKeywordEnabled(m_GhostEffectVolume.mode.ToString()))
        {
            foreach (var keyword in ghostEffectMaterial.shaderKeywords)
            {
                ghostEffectMaterial.DisableKeyword(keyword);
            }
            ghostEffectMaterial.EnableKeyword(m_GhostEffectVolume.mode.value.ToString());
        }

        UpdateMaterial(context, ref renderingData);

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
        
        switch (m_GhostEffectVolume.mode.value)
        {
            case GhostEffectVolume.GhostEffectMode.None:
                break;
            case GhostEffectVolume.GhostEffectMode._PLAYER:
                Shader.SetGlobalFloat(_PLAYER_Width, m_GhostEffectVolume._PLAYER_Width.value);
                Shader.SetGlobalFloat(_PLAYER_Progress, m_GhostEffectVolume._PLAYER_Progress.value);
                Debug.Log(StencilState.defaultValue);
                cmd.GetTemporaryRT(TempTarget, width, height, 0, FilterMode.Bilinear, desc.colorFormat);
                cmd.Blit(m_Renderer.cameraColorTarget, TempTarget);
                cmd.SetGlobalTexture("_SourceTex", TempTarget);
                cmd.Blit(TempTarget, m_Renderer.cameraColorTarget, ghostEffectMaterial);
                break;
           
            default:
                break;
        }
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}

