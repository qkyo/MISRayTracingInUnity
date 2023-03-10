using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

[CreateAssetMenu(fileName = "MisDenoiserRendererAsset", menuName = "Rendering/PerRendererAsset/MisDenoiserRendererAsset")]
public class MisDenoiserRendererAsset : RayTracingManagerAsset
{
	// public ComputeShader denoiserComputeShader;
	public CubeMapSetting cubeMapSetting;
	public bool enableAccumulate;

	public enum AIDenoiseMode
	{
		None = 0,
		
		[InspectorName("Intel Open Image Denoise")]
		OpenImageDenoise,

		[InspectorName("NVIDIA Optix Denoiser")]
		Optix
	}

	public AIDenoiseMode DenoiserType = AIDenoiseMode.None;

	public override RayTracingManager CreateManager()
	{
		return new MisDenoiserRenderer(this, enableAccumulate, cubeMapSetting, DenoiserType);
	}
}
