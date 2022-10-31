using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DungeonAutomata._Project.Scripts.ComputeShaders
{
    public class ComputeFeature : ScriptableRendererFeature
    {
        class ComputeRenderPass : ScriptableRenderPass
        {
            ComputeShader _computeShader;
            string _kernelName;
            Color _tintColor;
            int _renderTargetId;

            //RenderTargetIdentifier _renderTargetIdentifier;
            RenderTargetIdentifier _renderTargetIdentifier;
            int _renderTextureWidth;
            int _renderTextureHeight;
            public ComputeRenderPass(ComputeShader filterComputeShader,
                string kernelName,
                int renderTargetId,
                Color tintColor)
            {
                _computeShader = filterComputeShader;
                _kernelName = kernelName;
                _renderTargetId = renderTargetId;
                _tintColor = tintColor;
            }
            // This method is called before executing the render pass.
            // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
            // When empty this render pass will render to the active camera render target.
            // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
            // The render pipeline will ensure target setup and clearing happens in a performant manner.
            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                var cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
                cameraTargetDescriptor.enableRandomWrite = true;
                cmd.GetTemporaryRT(_renderTargetId, cameraTargetDescriptor);
                //cmd.GetTemporaryRT(_renderTargetId, cameraTargetDescriptor);
                _renderTargetIdentifier = new RenderTargetIdentifier(_renderTargetId);
                //_renderTargetIdentifier = new RenderTargetIdentifier(_renderTargetId);
                _renderTextureWidth = cameraTargetDescriptor.width;
                _renderTextureHeight = cameraTargetDescriptor.height;
            }

            // Here you can implement the rendering logic.
            // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
            // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
            // You don't have to call ScriptableRend
            // erContext.submit, the render pipeline will call it at specific points in the pipeline.
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (renderingData.cameraData.isSceneViewCamera)
                    return;

                CommandBuffer cmd = CommandBufferPool.Get();
                var mainKernel = _computeShader.FindKernel(_kernelName);

                _computeShader.GetKernelThreadGroupSizes(mainKernel, out uint xGroupSize, out uint yGroupSize, out _);
                //cmd.Blit(renderingData.cameraData.targetTexture, _renderSourceIdentifier);
                //cmd.Blit(renderingData.cameraData.targetTexture, _renderSourceIdentifier);
                cmd.SetComputeTextureParam(_computeShader, mainKernel, _renderTargetId, _renderTargetIdentifier);
                //cmd.SetComputeTextureParam(_computeShader, mainKernel, _renderTargetId, _renderTargetIdentifier);
                cmd.SetComputeVectorParam(_computeShader, "Color", _tintColor);
                cmd.SetComputeIntParam(_computeShader, "_ResultWidth", _renderTextureWidth);
                cmd.SetComputeIntParam(_computeShader, "_ResultHeight", _renderTextureHeight);
                cmd.DispatchCompute(_computeShader, mainKernel,
                    Mathf.CeilToInt(_renderTextureWidth / (float) xGroupSize),
                    Mathf.CeilToInt(_renderTextureHeight / (float) yGroupSize),
                    1);
                cmd.Blit(_renderTargetIdentifier, renderingData.cameraData.renderer.cameraColorTargetHandle);
                //cmd.Blit(_renderTargetIdentifier, renderingData.cameraData.renderer.cameraColorTargetHandle);
                //cmd.Blit(_renderSourceIdentifier, renderingData.cameraData.renderer.cameraColorTargetHandle);
            
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);
            }

            // Cleanup any allocated resources that were created during the execution of this render pass.
            public override void OnCameraCleanup(CommandBuffer cmd)
            {
                cmd.ReleaseTemporaryRT(_renderTargetId);
                //cmd.ReleaseTemporaryRT(_renderTargetId);
            }
        }

        ComputeRenderPass _scriptablePass;
        bool _initialized;

        public ComputeShader CompShader;
        public string KernelName = "TintShader";
        public Color TintColor = Color.red;
        public Texture2D SourceTexture;

        /// <inheritdoc/>
        public override void Create()
        {
            if (CompShader == null)
            {
                _initialized = false;
                return;
            }
    
            int renderTargetId = Shader.PropertyToID("Result");
            _scriptablePass = new ComputeRenderPass(CompShader, KernelName, renderTargetId, TintColor);
            _scriptablePass.renderPassEvent = RenderPassEvent.AfterRendering;
            _initialized = true;
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(_scriptablePass);
        }
    }
}


