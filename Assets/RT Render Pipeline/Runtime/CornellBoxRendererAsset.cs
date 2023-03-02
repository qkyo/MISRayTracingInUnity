using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

[CreateAssetMenu(fileName = "CornellBoxRendererAsset", menuName = "Rendering/PerRendererAsset/CornellBoxRendererAsset")]
public class CornellBoxRendererAsset : RayTracingManagerAsset
{
	public override RayTracingManager CreateManager()
	{
		return new CornellBoxRenderer(this);

	}
}
