/*
 * @Author: Qkyo
 * @Date: 2023-02-01 16:35:25
 * @LastEditors: Qkyo
 * @LastEditTime: 2023-02-01 19:33:38
 * @FilePath: \RayTracingRenderPipeline\Assets\RT Render Pipeline\Runtime\RayTracingRenderPipeline.cs
 * @Description: 
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

public class RayTracingRenderPipeline : RenderPipeline
{
    private RayTracingRenderPipelineAsset rtRenderPipeplineAsset;
    private RayTracingManager manager;
    private RayTracingAccelerationStructure accelerationStructure;
    public readonly int accelerationStructureShaderId = Shader.PropertyToID("_AccelerationStructure");

    /// <summary>
    /// All Pseudo Random Number Generator states for camera.
    /// </summary>
    private readonly Dictionary<int, ComputeBuffer> PRNGStates = new Dictionary<int, ComputeBuffer>();

    public RayTracingRenderPipeline(RayTracingRenderPipelineAsset asset)
    {
        rtRenderPipeplineAsset = asset;
        accelerationStructure = new RayTracingAccelerationStructure();

        manager = asset.managerAsset.CreateManager();

        if (manager == null)
        {
            Debug.LogError("Can't create manager.");
            return;
        }
        if (manager.Init(this) == false)
        {
            manager = null;
            Debug.LogError("Initialize manager failed.");
            return;
        }
    }


    /// <summary>
    /// build the ray tracing acceleration structure.
    /// </summary>
    private void BuildAccelerationStructure()
    {
        if (SceneManager.Instance == null || !SceneManager.Instance.isDirty) return;

        accelerationStructure.Dispose();
        accelerationStructure = new RayTracingAccelerationStructure();

        SceneManager.Instance.FillAccelerationStructure(ref accelerationStructure);

        accelerationStructure.Build();

        SceneManager.Instance.isDirty = false;
    }

    /// <summary>
    /// require the ray tracing acceleration structure.
    /// </summary>
    /// <returns>the ray tracing acceleration structure.</returns>
    public RayTracingAccelerationStructure RequestAccelerationStructure()
    {
      return this.accelerationStructure;
    }

    /// <summary>
    /// require a PRNG compute buffer for camera.
    /// </summary>
    /// <param name="width">the buffer width.</param>
    /// <param name="height">the buffer height.</param>
    /// <returns></returns>
    public ComputeBuffer RequirePRNGStates(Camera camera)
    {
      var id = camera.GetInstanceID();
      if (PRNGStates.TryGetValue(id, out var buffer))
        return buffer;

      buffer = new ComputeBuffer(camera.pixelWidth * camera.pixelHeight, 4 * 4, ComputeBufferType.Structured, ComputeBufferMode.Immutable);

      var _mt19937 = new MersenneTwister.MT.mt19937ar_cok_opt_t();
      _mt19937.init_genrand((uint)System.DateTime.Now.Ticks);

      var data = new uint[camera.pixelWidth * camera.pixelHeight * 4];
      for (var i = 0; i < camera.pixelWidth * camera.pixelHeight * 4; ++i)
          data[i] = _mt19937.genrand_int32();
      buffer.SetData(data);

      PRNGStates.Add(id, buffer);
      return buffer;
    }

    protected override void Dispose(bool disposing)
      {
          if (null != manager)
          {
              manager.Dispose(disposing);
              manager = null;
          }

          foreach (var pair in PRNGStates)
          {
            pair.Value.Release();
          }
          PRNGStates.Clear();

          if (null != accelerationStructure)
          {
            accelerationStructure.Dispose();
            accelerationStructure = null;
          }
      }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        
        if (!SystemInfo.supportsRayTracing)
        {
            Debug.LogError("You system is not support ray tracing. Please check your graphic API is D3D12 and os is Windows 10.");
            return;
        }
        

        BeginFrameRendering(context, cameras);

        System.Array.Sort(cameras, (lhs, rhs) => (int)(lhs.depth - rhs.depth));

        BuildAccelerationStructure();

        foreach (var camera in cameras)
        {
            // Only render game and scene view camera.
            if (camera.cameraType != CameraType.Game && camera.cameraType != CameraType.SceneView)
                continue;

            BeginCameraRendering(context, camera);
            manager?.Render(context, camera);
            context.Submit();
            EndCameraRendering(context, camera);
        }

        EndFrameRendering(context, cameras);
    }
}
