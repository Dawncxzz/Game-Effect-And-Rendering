using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScreenSpaceOutlines : ScriptableRendererFeature
{
    [System.Serializable]
    private class ScreenSpaceOutlineSettings
    {
        public Color backgraoudColor;
    }

    [System.Serializable]
    private class ViewSpaceNormalsTextureSettings
    {
        public Color backgraoudColor;
        public RenderTextureFormat colorFormat;
        public int depthBufferBits;
        public FilterMode filterMode;
    }
    class ViewSpaceNormalsTexturePass : ScriptableRenderPass
    {
        private ViewSpaceNormalsTextureSettings viewSpaceNormalsTextureSettings;
        private readonly List<ShaderTagId> shaderTagIdList;
        private readonly RenderTargetHandle normals;
        private readonly Material normalsMaterial;
        private FilteringSettings filteringSettings;
        private FilteringSettings occluderFilteringSetting;
        public ViewSpaceNormalsTexturePass(RenderPassEvent renderPassEvent, LayerMask outlinesLayerMask, LayerMask occluderLayerMask, ViewSpaceNormalsTextureSettings settings) 
        {
            viewSpaceNormalsTextureSettings = settings;
            normalsMaterial = new Material(Shader.Find("Hidden/ViewSpaceNormalsShader"));
            shaderTagIdList = new List<ShaderTagId> { new ShaderTagId("UniversalForward"), new ShaderTagId("UniversalForwardOnly") , new ShaderTagId("LightweightForward"), new ShaderTagId("SRPDefaultUnlit") };
            this.renderPassEvent = renderPassEvent;
            normals.Init("_SceneViewSpaceNormals");
            filteringSettings = new FilteringSettings(RenderQueueRange.opaque, outlinesLayerMask);
            occluderFilteringSetting = new FilteringSettings(RenderQueueRange.opaque, occluderLayerMask);
        }
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            RenderTextureDescriptor normalsTextureDescriptor = cameraTextureDescriptor;
            normalsTextureDescriptor.colorFormat = viewSpaceNormalsTextureSettings.colorFormat;
            normalsTextureDescriptor.depthBufferBits = viewSpaceNormalsTextureSettings.depthBufferBits;
            cmd.GetTemporaryRT(normals.id, cameraTextureDescriptor, viewSpaceNormalsTextureSettings.filterMode);
            ConfigureTarget(normals.Identifier());
            ConfigureClear(ClearFlag.All, viewSpaceNormalsTextureSettings.backgraoudColor);
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!normalsMaterial)
                return;

            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler("SceneViewSpaceNormalsTextureCreation")))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                DrawingSettings drawingSettings = CreateDrawingSettings(shaderTagIdList, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
                drawingSettings.overrideMaterial = normalsMaterial; 
                DrawingSettings occludeSettings = CreateDrawingSettings(shaderTagIdList, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
                occludeSettings.overrideMaterial = normalsMaterial;
                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
                context.DrawRenderers(renderingData.cullResults, ref occludeSettings, ref occluderFilteringSetting);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(normals.id);
        }
    }
    class ScreenSpaceOutlinePass : ScriptableRenderPass
    {
        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.

        private readonly Material screenSpaceOutlineMaterial;
        private RenderTargetIdentifier cameraColorTarget;
        private RenderTargetIdentifier temporaryBuffer;
        private int temporaryBufferID = Shader.PropertyToID("_TemporaryBuffer");

        ScreenSpaceOutlinesVolume m_Volume;
        static readonly int _OutlineScale = Shader.PropertyToID("_OutlineScale");
        static readonly int _RobertsCrossMultiplier = Shader.PropertyToID("_RobertsCrossMultiplier");
        static readonly int _DepthThreshold = Shader.PropertyToID("_DepthThreshold");
        static readonly int _NormalThreshold = Shader.PropertyToID("_NormalThreshold");
        static readonly int _SteepAngleThreshold = Shader.PropertyToID("_SteepAngleThreshold");
        static readonly int _SteepAngleMultiplier = Shader.PropertyToID("_SteepAngleMultiplier");
        static readonly int _OutlineColor = Shader.PropertyToID("_OutlineColor");
        public ScreenSpaceOutlinePass(RenderPassEvent renderPassEvent) 
        { 
            this.renderPassEvent = renderPassEvent;
            screenSpaceOutlineMaterial = new Material(Shader.Find("Hidden/OutlineShader"));
        }
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            cameraColorTarget = renderingData.cameraData.renderer.cameraColorTarget;
            temporaryBuffer = temporaryBufferID;
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!screenSpaceOutlineMaterial)
                return;

            var stack = VolumeManager.instance.stack;
            m_Volume = stack.GetComponent<ScreenSpaceOutlinesVolume>();
            if (m_Volume == null || !m_Volume.IsActive())
            {
                return;
            }
            screenSpaceOutlineMaterial.SetFloat(_OutlineScale, m_Volume._OutlineScale.value);
            screenSpaceOutlineMaterial.SetFloat(_RobertsCrossMultiplier, m_Volume._RobertsCrossMultiplier.value);
            screenSpaceOutlineMaterial.SetFloat(_DepthThreshold, m_Volume._DepthThreshold.value);
            screenSpaceOutlineMaterial.SetFloat(_NormalThreshold, m_Volume._NormalThreshold.value);
            screenSpaceOutlineMaterial.SetFloat(_SteepAngleThreshold, m_Volume._SteepAngleThreshold.value);
            screenSpaceOutlineMaterial.SetFloat(_SteepAngleMultiplier, m_Volume._SteepAngleMultiplier.value);
            screenSpaceOutlineMaterial.SetColor(_OutlineColor, m_Volume._OutlineColor.value);

            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler("ScreenSpaceOutlines")))
            {
                Blit(cmd, cameraColorTarget, temporaryBuffer);
                Blit(cmd, temporaryBuffer, cameraColorTarget, screenSpaceOutlineMaterial);
                
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(temporaryBufferID);
        }
    }

    [SerializeField] private RenderPassEvent renderPassEvent;

    private ViewSpaceNormalsTexturePass viewSpaceNormalsTexturePass;
    private ScreenSpaceOutlinePass screenSpaceOutlinePass;
    /// <inheritdoc/>
    public override void Create()
    {
        viewSpaceNormalsTexturePass = new ViewSpaceNormalsTexturePass(renderPassEvent, outlinesLayerMask, occluderLayerMask, viewSpaceNormalsTextureSettings);

        screenSpaceOutlinePass = new ScreenSpaceOutlinePass(renderPassEvent);
    }
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(viewSpaceNormalsTexturePass);
        renderer.EnqueuePass(screenSpaceOutlinePass);
    }

    [SerializeField] private ScreenSpaceOutlineSettings screenSpaceOutlineSettings;
    [SerializeField] private ViewSpaceNormalsTextureSettings viewSpaceNormalsTextureSettings;
    [SerializeField] private LayerMask outlinesLayerMask;
    [SerializeField] private LayerMask occluderLayerMask;
}


