using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

[CreateAssetMenu(fileName = "MisBsdfRendererAsset", menuName = "Rendering/PerRendererAsset/MisBsdfRendererAsset")]
public class MisBsdfRendererAsset : RayTracingManagerAsset
{
	public bool enableAccumulate;
	public override RayTracingManager CreateManager()
	{
		return new MisBsdfRenderer(this, enableAccumulate);
	}
}
