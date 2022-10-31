using UnityEngine;
using UnityEngine.Rendering;

namespace DungeonAutomata._Project.Scripts.ComputeShaders
{
	public class ComputeShaderTest : MonoBehaviour
	{
		public ComputeShader computeShader;
		public RenderTexture renderTexture;
		public int textureWidth;
		public int textureHeight;

		private void Start()
		{
			PreviewRenderTexture();
			//RenderPipelineManager.beginCameraRendering += OnEndContextRendering;
		}

		private void OnRenderImage(RenderTexture src, RenderTexture dest)
		{
			//OnEndContextRendering(dest);
		}

		private void PreviewRenderTexture()
		{
			renderTexture = new RenderTexture(textureWidth, textureHeight, 24);
			renderTexture.enableRandomWrite = true;
			renderTexture.Create();

			computeShader.SetTexture(0, "Result", renderTexture);
			computeShader.Dispatch(0, textureWidth / 8, textureHeight / 8, 1);
		}

		private void OnEndContextRendering(ScriptableRenderContext context, Camera camera)
		{
			if (renderTexture == null)
			{
				renderTexture = new RenderTexture(textureWidth, textureHeight, 24);
				renderTexture.enableRandomWrite = true;
				renderTexture.Create();
			}

			computeShader.SetTexture(0, "Result", renderTexture);
			computeShader.Dispatch(0, textureWidth / 8, textureHeight / 8, 1);
			Graphics.Blit(renderTexture, camera.targetTexture);
		}
	}
}