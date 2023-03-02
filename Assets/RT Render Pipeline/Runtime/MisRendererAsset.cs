using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

[CreateAssetMenu(fileName = "MisRendererAsset", menuName = "Rendering/PerRendererAsset/MisRendererAsset")]
public class MisRendererAsset : RayTracingManagerAsset
{
	public bool enableAccumulate;

	public override RayTracingManager CreateManager()
	{
		return new MisRenderer(this, enableAccumulate);
	}
}
