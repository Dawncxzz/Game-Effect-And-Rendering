using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using XPostProcessing;

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

    static readonly int _LINEBLOCKGLITCH_Frequency = Shader.PropertyToID("_LINEBLOCKGLITCH_Frequency");
    static readonly int _LINEBLOCKGLITCH_Amount = Shader.PropertyToID("_LINEBLOCKGLITCH_Amount");
    static readonly int _LINEBLOCKGLITCH_Offset = Shader.PropertyToID("_LINEBLOCKGLITCH_Offset");
    static readonly int _LINEBLOCKGLITCH_LinesWidth = Shader.PropertyToID("_LINEBLOCKGLITCH_LinesWidth");
    static readonly int _LINEBLOCKGLITCH_Alpha = Shader.PropertyToID("_LINEBLOCKGLITCH_Alpha");

    static readonly int _TILEJITTERGLITCH_SplittingNumber = Shader.PropertyToID("_TILEJITTERGLITCH_SplittingNumber");
    static readonly int _TILEJITTERGLITCH_JitterAmount = Shader.PropertyToID("_TILEJITTERGLITCH_JitterAmount");
    static readonly int _TILEJITTERGLITCH_JitterSpeed = Shader.PropertyToID("_TILEJITTERGLITCH_JitterSpeed");
    static readonly int _TILEJITTERGLITCH_Frequency = Shader.PropertyToID("_TILEJITTERGLITCH_Frequency");

    static readonly int _SCANLINEJITTERGLITCH_Amount = Shader.PropertyToID("_SCANLINEJITTERGLITCH_Amount");
    static readonly int _SCANLINEJITTERGLITCH_Threshold = Shader.PropertyToID("_SCANLINEJITTERGLITCH_Threshold");
    static readonly int _SCANLINEJITTERGLITCH_Frequency = Shader.PropertyToID("_SCANLINEJITTERGLITCH_Frequency");

    static readonly int _DIGITALSTRIPEGLITCH_NoiseTex = Shader.PropertyToID("_DIGITALSTRIPEGLITCH_NoiseTex");
    static readonly int _DIGITALSTRIPEGLITCH_Indensity = Shader.PropertyToID("_DIGITALSTRIPEGLITCH_Indensity");
    static readonly int _DIGITALSTRIPEGLITCH_StripColorAdjustColor = Shader.PropertyToID("_DIGITALSTRIPEGLITCH_StripColorAdjustColor");
    static readonly int _DIGITALSTRIPEGLITCH_StripColorAdjustIndensity = Shader.PropertyToID("_DIGITALSTRIPEGLITCH_StripColorAdjustIndensity");

    static readonly int _SCREENJUMPGLITCH_JumpIndensity = Shader.PropertyToID("_SCREENJUMPGLITCH_JumpIndensity");

    static readonly int _SCREENSHAKEGLITCH_ScreenShake = Shader.PropertyToID("_SCREENSHAKEGLITCH_ScreenShake");
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
        switch (m_GlitchVolume.mode.value)
        {
            case GlitchVolume.GlitchMode.None:
                break;
            case GlitchVolume.GlitchMode._RGBSPLITGLITCH:
                Shader.SetGlobalTexture(_RGBSPLITGLITCH_NoiseTex, m_GlitchVolume._RGBSPLITGLITCH_NoiseTex.value);
                Shader.SetGlobalFloat(_RGBSPLITGLITCH_Speed, m_GlitchVolume._RGBSPLITGLITCH_Speed.value);
                Shader.SetGlobalFloat(_RGBSPLITGLITCH_Amplitude, m_GlitchVolume._RGBSPLITGLITCH_Amplitude.value);
                cmd.GetTemporaryRT(TempTarget, width, height, 0, FilterMode.Bilinear, desc.colorFormat);
                cmd.Blit(m_Renderer.cameraColorTarget, TempTarget);
                cmd.SetGlobalTexture(ShaderPropertyId.sourceTex, TempTarget);
                cmd.Blit(TempTarget, m_Renderer.cameraColorTarget, glitchMaterial);
                break;
            case GlitchVolume.GlitchMode._IMAGEBLOCKGLITCH:
                Shader.SetGlobalFloat(_IMAGEBLOCKGLITCH_BlockSize, m_GlitchVolume._IMAGEBLOCKGLITCH_BlockSize.value);
                Shader.SetGlobalFloat(_IMAGEBLOCKGLITCH_Speed, m_GlitchVolume._IMAGEBLOCKGLITCH_Speed.value);
                Shader.SetGlobalVector(_IMAGEBLOCKGLITCH_MaxRGBSplit, m_GlitchVolume._IMAGEBLOCKGLITCH_MaxRGBSplit.value);
                cmd.GetTemporaryRT(TempTarget, width, height, 0, FilterMode.Bilinear, desc.colorFormat);
                cmd.Blit(m_Renderer.cameraColorTarget, TempTarget);
                cmd.SetGlobalTexture(ShaderPropertyId.sourceTex, TempTarget);
                cmd.Blit(TempTarget, m_Renderer.cameraColorTarget, glitchMaterial);
                break;
            case GlitchVolume.GlitchMode._LINEBLOCKGLITCH:
                Shader.SetGlobalFloat(_LINEBLOCKGLITCH_Frequency, m_GlitchVolume._LINEBLOCKGLITCH_Frequency.value);
                Shader.SetGlobalFloat(_LINEBLOCKGLITCH_Amount, m_GlitchVolume._LINEBLOCKGLITCH_Amount.value);
                Shader.SetGlobalFloat(_LINEBLOCKGLITCH_Offset, m_GlitchVolume._LINEBLOCKGLITCH_Offset.value);
                Shader.SetGlobalFloat(_LINEBLOCKGLITCH_LinesWidth, m_GlitchVolume._LINEBLOCKGLITCH_LinesWidth.value);
                Shader.SetGlobalFloat(_LINEBLOCKGLITCH_Alpha, m_GlitchVolume._LINEBLOCKGLITCH_Alpha.value);
                cmd.GetTemporaryRT(TempTarget, width, height, 0, FilterMode.Bilinear, desc.colorFormat);
                cmd.Blit(m_Renderer.cameraColorTarget, TempTarget);
                cmd.SetGlobalTexture(ShaderPropertyId.sourceTex, TempTarget);
                cmd.Blit(TempTarget, m_Renderer.cameraColorTarget, glitchMaterial);
                break;
            case GlitchVolume.GlitchMode._TILEJITTERGLITCH:
                Shader.SetGlobalFloat(_TILEJITTERGLITCH_SplittingNumber, UnityEngine.Random.Range(0, m_GlitchVolume._TILEJITTERGLITCH_SplittingNumber.value));
                Shader.SetGlobalFloat(_TILEJITTERGLITCH_JitterAmount, m_GlitchVolume._TILEJITTERGLITCH_JitterAmount.value);
                Shader.SetGlobalFloat(_TILEJITTERGLITCH_JitterSpeed, m_GlitchVolume._TILEJITTERGLITCH_JitterSpeed.value * 100f);
                Shader.SetGlobalFloat(_TILEJITTERGLITCH_Frequency, m_GlitchVolume._TILEJITTERGLITCH_Frequency.value);
                Shader.SetGlobalFloat(_LINEBLOCKGLITCH_Alpha, m_GlitchVolume._LINEBLOCKGLITCH_Alpha.value);
                cmd.GetTemporaryRT(TempTarget, width, height, 0, FilterMode.Bilinear, desc.colorFormat);
                cmd.Blit(m_Renderer.cameraColorTarget, TempTarget);
                cmd.SetGlobalTexture(ShaderPropertyId.sourceTex, TempTarget);
                cmd.Blit(TempTarget, m_Renderer.cameraColorTarget, glitchMaterial);
                break;
            case GlitchVolume.GlitchMode._SCANLINEJITTERGLITCH:
                Shader.SetGlobalFloat(_SCANLINEJITTERGLITCH_Amount, m_GlitchVolume._SCANLINEJITTERGLITCH_Amount.value);
                Shader.SetGlobalFloat(_SCANLINEJITTERGLITCH_Threshold, m_GlitchVolume._SCANLINEJITTERGLITCH_Threshold.value);
                Shader.SetGlobalFloat(_SCANLINEJITTERGLITCH_Frequency, m_GlitchVolume._SCANLINEJITTERGLITCH_Frequency.value);
                cmd.GetTemporaryRT(TempTarget, width, height, 0, FilterMode.Bilinear, desc.colorFormat);
                cmd.Blit(m_Renderer.cameraColorTarget, TempTarget);
                cmd.SetGlobalTexture(ShaderPropertyId.sourceTex, TempTarget);
                cmd.Blit(TempTarget, m_Renderer.cameraColorTarget, glitchMaterial);
                break;
            case GlitchVolume.GlitchMode._DIGITALSTRIPEGLITCH:
                UpdateNoiseTexture(m_GlitchVolume._DIGITALSTRIPEGLITCH_Frequncy.value, (int)m_GlitchVolume._DIGITALSTRIPEGLITCH_TexSize.value.x, (int)m_GlitchVolume._DIGITALSTRIPEGLITCH_TexSize.value.y, m_GlitchVolume._DIGITALSTRIPEGLITCH_StripeLength.value);
                Shader.SetGlobalTexture(_DIGITALSTRIPEGLITCH_NoiseTex, _noiseTexture);
                Shader.SetGlobalFloat(_DIGITALSTRIPEGLITCH_Indensity, m_GlitchVolume._DIGITALSTRIPEGLITCH_Indensity.value);
                Shader.SetGlobalColor(_DIGITALSTRIPEGLITCH_StripColorAdjustColor, m_GlitchVolume._DIGITALSTRIPEGLITCH_StripColorAdjustColor.value);
                Shader.SetGlobalFloat(_DIGITALSTRIPEGLITCH_StripColorAdjustIndensity, m_GlitchVolume._DIGITALSTRIPEGLITCH_StripColorAdjustIndensity.value);
                cmd.GetTemporaryRT(TempTarget, width, height, 0, FilterMode.Bilinear, desc.colorFormat);
                cmd.Blit(m_Renderer.cameraColorTarget, TempTarget);
                cmd.SetGlobalTexture(ShaderPropertyId.sourceTex, TempTarget);
                cmd.Blit(TempTarget, m_Renderer.cameraColorTarget, glitchMaterial);
                break;
            case GlitchVolume.GlitchMode._ANALOGNOISEGLITCH:
                break;
            case GlitchVolume.GlitchMode._SCREENJUMPGLITCH:
                Shader.SetGlobalFloat(_SCREENJUMPGLITCH_JumpIndensity, m_GlitchVolume._SCREENJUMPGLITCH_JumpIndensity.value);
                cmd.GetTemporaryRT(TempTarget, width, height, 0, FilterMode.Bilinear, desc.colorFormat);
                cmd.Blit(m_Renderer.cameraColorTarget, TempTarget);
                cmd.SetGlobalTexture(ShaderPropertyId.sourceTex, TempTarget);
                cmd.Blit(TempTarget, m_Renderer.cameraColorTarget, glitchMaterial);
                break;
            case GlitchVolume.GlitchMode._SCREENSHAKEGLITCH:
                Shader.SetGlobalFloat(_SCREENSHAKEGLITCH_ScreenShake, m_GlitchVolume._SCREENSHAKEGLITCH_ScreenShake.value);
                cmd.GetTemporaryRT(TempTarget, width, height, 0, FilterMode.Bilinear, desc.colorFormat);
                cmd.Blit(m_Renderer.cameraColorTarget, TempTarget);
                cmd.SetGlobalTexture(ShaderPropertyId.sourceTex, TempTarget);
                cmd.Blit(TempTarget, m_Renderer.cameraColorTarget, glitchMaterial);
                break;
            case GlitchVolume.GlitchMode._WAVEJITTERGLITCH:
                break;
        }
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    //工具函数
    Texture2D _noiseTexture;
    RenderTexture _trashFrame1;
    RenderTexture _trashFrame2;
    void UpdateNoiseTexture(int frame, int noiseTextureWidth, int noiseTextureHeight, float stripLength)
    {
        int frameCount = Time.frameCount;
        if (frameCount % frame != 0)
        {
            return;
        }

        _noiseTexture = new Texture2D(noiseTextureWidth, noiseTextureHeight, TextureFormat.ARGB32, false);
        _noiseTexture.wrapMode = TextureWrapMode.Clamp;
        _noiseTexture.filterMode = FilterMode.Point;

        _trashFrame1 = new RenderTexture(Screen.width, Screen.height, 0);
        _trashFrame2 = new RenderTexture(Screen.width, Screen.height, 0);
        _trashFrame1.hideFlags = HideFlags.DontSave;
        _trashFrame2.hideFlags = HideFlags.DontSave;

        Color32 color = XPostProcessingUtility.RandomColor();

        for (int y = 0; y < _noiseTexture.height; y++)
        {
            for (int x = 0; x < _noiseTexture.width; x++)
            {
                //随机值若大于给定strip随机阈值，重新随机颜色
                if (UnityEngine.Random.value > stripLength)
                {
                    color = XPostProcessingUtility.RandomColor();
                }
                //设置贴图像素值
                _noiseTexture.SetPixel(x, y, color);
            }
        }

        _noiseTexture.Apply();

        var bytes = _noiseTexture.EncodeToPNG();
    }
}


