using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

[CreateAssetMenu(fileName = "RayTracingRenderPipelineAsset", menuName = "Rendering/RayTracingRenderPipelineAsset", order = -1)]
public class RayTracingRenderPipelineAsset : RenderPipelineAsset
{
    public RayTracingManagerAsset managerAsset;

    // Override default CreatePipeline method in RenderPipelineAsset Class.
    protected override RenderPipeline CreatePipeline()
    {
        return new RayTracingRenderPipeline(this);
    }
}
