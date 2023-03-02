using UnityEngine;
using UnityEngine.Experimental.Rendering;

public abstract class RayTracingManagerAsset : ScriptableObject
{
    /// <summary>
    /// Manage the ray tracing shader.
    /// </summary>
    public RayTracingShader shader;

    public abstract RayTracingManager CreateManager();
}