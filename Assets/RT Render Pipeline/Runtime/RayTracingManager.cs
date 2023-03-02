using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

/// <summary>
/// Manage the ray tracing interface.
/// </summary>
public abstract class RayTracingManager
{
    /// <summary>
    /// the camera shader parameters
    /// </summary>
    private static class CameraShaderParams
    {
        public static readonly int
            _WorldSpaceCameraPos = Shader.PropertyToID("_WorldSpaceCameraPos"),
            _InvCameraViewProj = Shader.PropertyToID("_InvCameraViewProj"),
            _CameraFarDistance = Shader.PropertyToID("_CameraFarDistance");
    }

    protected readonly int
        outputTargetSizeShaderId = Shader.PropertyToID("_OutputTargetSize"),
        outputTargetShaderId = Shader.PropertyToID("_OutputTarget");

    private RayTracingManagerAsset rtManagerAsset;
    protected RayTracingRenderPipeline rtRenderPipeline;
    protected RayTracingShader rtShader;

    private readonly Dictionary<int, RTHandle> outputTargets = new Dictionary<int, RTHandle>();
    private readonly Dictionary<int, Vector4> outputTargetSizes = new Dictionary<int, Vector4>();


    protected RayTracingManager(RayTracingManagerAsset asset)
    {
        this.rtManagerAsset = asset;
    }

    public virtual bool Init(RayTracingRenderPipeline pipeline)
    {
        rtRenderPipeline = pipeline;
        rtShader = rtManagerAsset.shader;
        return true;
    }

    public virtual void Render(ScriptableRenderContext context, Camera camera)
    {
        SetupCamera(camera);
    }

    public virtual void Dispose(bool disposing)
    {
        foreach (var pair in outputTargets)
        {
            RTHandles.Release(pair.Value);
        }
        outputTargets.Clear();
    }

    /// <summary>
    /// Require a output target for camera.
    /// </summary>
    /// <param name="camera">the camera.</param>
    /// <returns>the output target.</returns>
    protected RTHandle RequireOutputTarget(Camera camera)
    {
        var id = camera.GetInstanceID();

        if (outputTargets.TryGetValue(id, out var outputTarget))
            return outputTarget;

        outputTarget = RTHandles.Alloc(
          camera.pixelWidth,
          camera.pixelHeight,
          1,
          DepthBits.None,
          GraphicsFormat.R32G32B32A32_SFloat,
          FilterMode.Point,
          TextureWrapMode.Clamp,
          TextureDimension.Tex2D,
          true,
          false,
          false,
          false,
          1,
          0f,
          MSAASamples.None,
          false,
          false,
          RenderTextureMemoryless.None,
          $"OutputTarget_{camera.name}");
        outputTargets.Add(id, outputTarget);

        return outputTarget;
    }


    /// <summary>
    /// require a output target size for camera.
    /// </summary>
    /// <param name="camera">the camera.</param>
    /// <returns>the output target size.</returns>
    protected Vector4 RequireOutputTargetSize(Camera camera)
    {
        var id = camera.GetInstanceID();

        if (outputTargetSizes.TryGetValue(id, out var outputTargetSize))
        return outputTargetSize;

        outputTargetSize = new Vector4(camera.pixelWidth, camera.pixelHeight, 1.0f / camera.pixelWidth, 1.0f / camera.pixelHeight);
        outputTargetSizes.Add(id, outputTargetSize);

        return outputTargetSize;
    }

    /// <summary>
    /// Setup camera. Pass camera's parameter to static CameraShaderParams class
    /// </summary>
    /// <param name="camera">the camera.</param>
    private static void SetupCamera(Camera camera)
    {
        Shader.SetGlobalVector(CameraShaderParams._WorldSpaceCameraPos, camera.transform.position);
        var projMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false);
        var viewMatrix = camera.worldToCameraMatrix;
        var viewProjMatrix = projMatrix * viewMatrix;
        var invViewProjMatrix = Matrix4x4.Inverse(viewProjMatrix);
        Shader.SetGlobalMatrix(CameraShaderParams._InvCameraViewProj, invViewProjMatrix);
        Shader.SetGlobalFloat(CameraShaderParams._CameraFarDistance, camera.farClipPlane);
    }
}