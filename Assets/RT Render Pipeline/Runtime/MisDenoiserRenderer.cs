using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Denoising;

// [ExecuteInEditMode]
public class MisDenoiserRenderer : RayTracingManager
{
    CubeMapSetting cubeMapSetting;
    MisDenoiserRendererAsset.AIDenoiseMode currentDenoiser;

    private readonly int
        // Data that may be required in raytrace.
        frameIndexShaderId = Shader.PropertyToID("_FrameIndex"),
        prngStatesShaderId = Shader.PropertyToID("_PRNGStates"),
        enableAccumulateShaderId = Shader.PropertyToID("_EnableAccumulate"),
        isAccumulateResetId = Shader.PropertyToID("_IsAccumulateReset"),
        cubeTextureShaderId = Shader.PropertyToID("_CubeTexture"),
        HDRExposureShaderId = Shader.PropertyToID("_HDRExposure"),
        HDRTintShaderId = Shader.PropertyToID("_HDRTint"),
        samLinearClampHDRShaderId = Shader.PropertyToID("_SamLinearClamp_HDR");

    private int frameIndex = 0;
    int enableAccumulate = 0;
    int isAccumulateReset = 1;

    bool denoiseInit = false;

    public MisDenoiserRenderer(RayTracingManagerAsset asset, bool enableAccumulate,
                               CubeMapSetting cubeMapSetting, MisDenoiserRendererAsset.AIDenoiseMode denoiserType) : base(asset)
    {
        this.enableAccumulate = enableAccumulate ? 1 : 0;
        this.cubeMapSetting = cubeMapSetting;
        this.currentDenoiser = denoiserType;

    }

    public override void Render(ScriptableRenderContext context, Camera camera)
    {
            base.Render(context, camera);

            isAccumulateReset = RayTracingResources.Instance.IsCamMoving ? 1 : 0;

            var rtOutputTarget = RequireOutputTarget(camera);
            var outputTargetSize = RequireOutputTargetSize(camera);
            var accelerationStructure = rtRenderPipeline.RequestAccelerationStructure();
            var PRNGStates = rtRenderPipeline.RequirePRNGStates(camera);
            var cmd = CommandBufferPool.Get(typeof(RayTracingManager).Name);
            int width = (int)outputTargetSize.x;
            int height = (int)outputTargetSize.y;

            //Debug.Log(width + " " + height);

            try
            {
                if (frameIndex < 50000)
                {
                    using (new ProfilingScope(cmd, new ProfilingSampler("RayTracing")))
                    {
                        cmd.SetRayTracingShaderPass(rtShader, "RayTracing");
                        cmd.SetRayTracingIntParam(rtShader, enableAccumulateShaderId, enableAccumulate);
                        //cmd.SetRayTracingIntParam(rtShader, isAccumulateResetId, isAccumulateReset);
                        cmd.SetRayTracingIntParam(rtShader, frameIndexShaderId, frameIndex);
                        cmd.SetRayTracingBufferParam(rtShader, prngStatesShaderId, PRNGStates);

                        cmd.SetRayTracingFloatParam(rtShader, HDRExposureShaderId, cubeMapSetting.M_HDRParams.exposureToGamma);
                        cmd.SetRayTracingVectorParam(rtShader, HDRTintShaderId, cubeMapSetting.M_HDRParams.Tint);
                        Vector4 HDRDecodeFlag = cubeMapSetting.SetHDRDecodeFlag(cubeMapSetting.M_HDRParams.colorDecodeFlag);
                        cmd.SetRayTracingVectorParam(rtShader, samLinearClampHDRShaderId, HDRDecodeFlag);

                        cmd.SetRayTracingVectorParam(rtShader, outputTargetSizeShaderId, outputTargetSize);
                        cmd.SetRayTracingAccelerationStructure(rtShader, rtRenderPipeline.accelerationStructureShaderId, accelerationStructure);
                        cmd.SetRayTracingTextureParam(rtShader, outputTargetShaderId, rtOutputTarget);
                        cmd.SetRayTracingTextureParam(rtShader, cubeTextureShaderId, cubeMapSetting.cubemapping);
                        cmd.DispatchRays(rtShader, "MISRayGenShader", (uint)rtOutputTarget.rt.width, (uint)rtOutputTarget.rt.height, 1, camera);
                    }
                    using (new ProfilingScope(cmd, new ProfilingSampler("Denoising")))
                    {
                        // Enable Denoiser
                        if (RayTracingResources.Instance.IsProgramRunning &&
                            !RayTracingResources.Instance.IsCamMoving &&
                            currentDenoiser != MisDenoiserRendererAsset.AIDenoiseMode.None)
                        {
                            RenderTexture denoiseSrc = rtOutputTarget;

                            CommandBufferDenoiser denoiser = new CommandBufferDenoiser(); ;
                            // Create a new denoiser object

                            Denoiser.State result;

                            if (currentDenoiser == MisDenoiserRendererAsset.AIDenoiseMode.OpenImageDenoise)
                            {
                                //// Initialize the denoising state
                                result = denoiser.Init(DenoiserType.OpenImageDenoise, width, height);

                            }
                            else if (currentDenoiser == MisDenoiserRendererAsset.AIDenoiseMode.Optix)
                            {
                                result = denoiser.Init(DenoiserType.Optix, width, height);

                            }

                            //// Create a new denoise request for a color image stored in a Render Texture
                            denoiser.DenoiseRequest(cmd, "color", denoiseSrc);

                            //// Wait until the denoising request is done executing
                            result = denoiser.WaitForCompletion(context, cmd);

                            ////// Get the results
                            var dst = new RenderTexture(denoiseSrc.descriptor);
                            result = denoiser.GetResults(cmd, dst);
                            cmd.Blit(dst, BuiltinRenderTextureType.CameraTarget, Vector2.one, Vector2.zero);
                            
                        }
                        // Disable Denoiser
                        else
                        {
                            cmd.Blit(rtOutputTarget, BuiltinRenderTextureType.CameraTarget, Vector2.one, Vector2.zero);
                        }
                    }

                    context.ExecuteCommandBuffer(cmd);
                    if (camera.cameraType == CameraType.Game)
                        frameIndex++;
                }
            }
            finally
            {
                CommandBufferPool.Release(cmd);
            }
        

        

    }

}