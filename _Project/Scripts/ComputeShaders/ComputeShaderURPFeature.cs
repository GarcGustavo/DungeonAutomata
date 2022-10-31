using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DungeonAutomata._Project.Scripts.ComputeShaders
{
	struct cell {
		Vector3 position;
		Vector4 color;
	};
	public class ComputeShaderURPFeature : ScriptableRendererFeature
	{
		#region Renderer Pass

		private class PixelateRenderPass : ScriptableRenderPass
		{
			private readonly int _blockSize;
			private readonly ComputeShader _filterComputeShader;
			private readonly string _kernelName;
			private readonly int _renderTargetId;

			private RenderTargetIdentifier _renderTargetIdentifier;
			private readonly int _renderTextureHeight;
			private float _renderTextureResolution;
			private readonly int _renderTextureWidth;
			private readonly Color _color;

			public PixelateRenderPass(ComputeShader filterComputeShader,
				string kernelName,
				int blockSize,
				int renderTargetId,
				int renderTextureWidth,
				int renderTextureHeight,
				Color Color)
			{
				_filterComputeShader = filterComputeShader;
				_kernelName = kernelName;
				_blockSize = blockSize;
				_renderTargetId = renderTargetId;
				_renderTextureHeight = renderTextureWidth;
				_renderTextureWidth = renderTextureHeight;
				_color = Color;
			}

			public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
			{
				// Get the current render target and initialize it
				var cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
				cameraTargetDescriptor.enableRandomWrite = true;
				cmd.GetTemporaryRT(_renderTargetId, cameraTargetDescriptor);
				_renderTargetIdentifier = new RenderTargetIdentifier(_renderTargetId);
				_renderTextureResolution = _renderTextureWidth * _renderTextureHeight;
				//_renderTextureResolution = cameraTargetDescriptor.width * cameraTargetDescriptor.height;
			}

			public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
			{
				if (renderingData.cameraData.isSceneViewCamera)
					return;

				var cmd = CommandBufferPool.Get();
				var mainKernel = _filterComputeShader.FindKernel(_kernelName);
				/*
				_filterComputeShader.GetKernelThreadGroupSizes(mainKernel,
				    out uint xGroupSize,
				    out uint yGroupSize,
				    out _);
				//cmd.Blit(renderingData.cameraData.targetTexture, _renderTargetIdentifier);
				cmd.SetComputeTextureParam(_filterComputeShader, mainKernel, _renderTargetId, _renderTargetIdentifier);
				cmd.SetComputeIntParam(_filterComputeShader, "_ResultWidth", _renderTextureWidth);
				cmd.SetComputeIntParam(_filterComputeShader, "_ResultHeight", _renderTextureHeight);
				cmd.DispatchCompute(_filterComputeShader, mainKernel,
					//(int) (_renderTextureWidth / xGroupSize),
					//(int) (_renderTextureHeight / yGroupSize),
					Mathf.CeilToInt(_renderTextureWidth / (float) _blockSize), // / xGroupSize),
					Mathf.CeilToInt(_renderTextureHeight / (float) _blockSize), // / yGroupSize),
					1);
				*/
				//cmd.Blit(renderingData.cameraData.targetTexture, _renderTargetIdentifier);
				//cmd.SetComputeTextureParam(_filterComputeShader, mainKernel, _renderTargetId, _renderTargetIdentifier);
				cmd.SetComputeTextureParam(_filterComputeShader, mainKernel, "Source", _renderTargetIdentifier);
				cmd.SetComputeTextureParam(_filterComputeShader, 
					mainKernel,
					_renderTargetId, 
					_renderTargetIdentifier);
				cmd.SetComputeVectorParam(_filterComputeShader, "Color", _color);

				cmd.DispatchCompute(_filterComputeShader, mainKernel,
					(_renderTextureWidth + _blockSize - 1) / _blockSize,
					(_renderTextureHeight + _blockSize - 1) / _blockSize, // / yGroupSize),
					1);
				
				cmd.Blit(_renderTargetIdentifier, 
					renderingData.cameraData.renderer.cameraColorTargetHandle);

				context.ExecuteCommandBuffer(cmd);
				cmd.Clear();
				CommandBufferPool.Release(cmd);
			}

			public override void OnCameraCleanup(CommandBuffer cmd)
			{
				cmd.ReleaseTemporaryRT(_renderTargetId);
			}
		}

		#endregion

		#region Renderer Feature

		private PixelateRenderPass _scriptablePass;
		private bool _initialized;

		public ComputeShader FilterComputeShader;
		public string KernelName = "TintShader";
		[Range(1, 40)] public int BlockSize = 1;
		public int renderTextureWidth = 256;
		public int renderTextureHeight = 256;
		public Color Color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

		public override void Create()
		{
			if (FilterComputeShader == null)
			{
				_initialized = false;
				return;
			}

			var renderTargetId = Shader.PropertyToID("Result");
			_scriptablePass = new PixelateRenderPass(FilterComputeShader,
				KernelName,
				BlockSize,
				renderTargetId,
				renderTextureHeight,
				renderTextureWidth,
				Color
			);
			_scriptablePass.renderPassEvent = RenderPassEvent.AfterRendering;
			_initialized = true;
		}

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			if (_initialized) renderer.EnqueuePass(_scriptablePass);
		}

		#endregion
	}
}