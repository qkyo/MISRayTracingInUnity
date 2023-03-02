using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

[CreateAssetMenu(fileName = "MisBrdfRendererAsset", menuName = "Rendering/PerRendererAsset/MisBrdfRendererAsset")]
public class MisBrdfRendererAsset : RayTracingManagerAsset
{
	public bool enableAccumulate;
	public override RayTracingManager CreateManager()
	{
		return new MisBrdfRenderer(this, enableAccumulate);
	}
}
