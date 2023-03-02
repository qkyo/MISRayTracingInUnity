using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class CornellBoxRenderer : RayTracingManager
{
  private readonly int
      frameIndexShaderId = Shader.PropertyToID("_FrameIndex"),
      prngStatesShaderId = Shader.PropertyToID("_PRNGStates");

  private int frameIndex = 0;
  public CornellBoxRenderer(RayTracingManagerAsset asset) : base(asset) { }

  public override void Render(ScriptableRenderContext context, Camera camera)
  {
    base.Render(context, camera);

    var outputTarget = RequireOutputTarget(camera);
    var outputTargetSize = RequireOutputTargetSize(camera);
    var accelerationStructure = rtRenderPipeline.RequestAccelerationStructure();
    var PRNGStates = rtRenderPipeline.RequirePRNGStates(camera);

    var cmd = CommandBufferPool.Get(typeof(RayTracingManager).Name);
    try
    {
      if (frameIndex < 2000)
      {
      using (new ProfilingScope(cmd, new ProfilingSampler("RayTracing")))
        {
          cmd.SetRayTracingShaderPass(rtShader, "RayTracing");
          cmd.SetRayTracingIntParam(rtShader, frameIndexShaderId, frameIndex);
          cmd.SetRayTracingBufferParam(rtShader, prngStatesShaderId, PRNGStates);
          cmd.SetRayTracingVectorParam(rtShader, outputTargetSizeShaderId, outputTargetSize);
          cmd.SetRayTracingAccelerationStructure(rtShader, rtRenderPipeline.accelerationStructureShaderId, accelerationStructure);
          cmd.SetRayTracingTextureParam(rtShader, outputTargetShaderId, outputTarget);
          // Invoke the OutputColorRayGenShader function in rtShader
          cmd.DispatchRays(rtShader, "CornellBoxGenShader", (uint)outputTarget.rt.width, (uint)outputTarget.rt.height, 1, camera);
        }
        context.ExecuteCommandBuffer(cmd);
        if (camera.cameraType == CameraType.Game)
          frameIndex++;
        using (new ProfilingScope(cmd, new ProfilingSampler("FinalBlit")))
        {
          cmd.Blit(outputTarget, BuiltinRenderTextureType.CameraTarget, Vector2.one, Vector2.zero);
        }
        context.ExecuteCommandBuffer(cmd);
      }
    }
    finally
    {
      CommandBufferPool.Release(cmd);
    }
  }

}