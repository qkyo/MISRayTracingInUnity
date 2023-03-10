using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/Enviornment Map/Cube Map Setting")]
public class CubeMapSetting : ScriptableObject
{
    public Cubemap cubemapping;

    // Reference to set flag : https://zhuanlan.zhihu.com/p/35096536
    public enum Flag_samLinearClampHDR 
    {
        pc_default = 0,
        android_gamma_space,
        android_linear_space
    }

    [Serializable]
    public struct HDRParams
    {
        [Range(0f, 8f)]
        public float exposureToGamma;
        [ColorUsage(false, true)]
        public Color Tint;
        public Flag_samLinearClampHDR colorDecodeFlag;
    }

    [SerializeField]
    HDRParams m_HDRParams = new HDRParams {
        exposureToGamma = 1.0f,
        Tint = new Color(.5f, .5f, .5f, .5f),
        colorDecodeFlag = 0
    };
    public HDRParams M_HDRParams => m_HDRParams;


    [Serializable]
    public struct CubeToSphericalMapParams
    {
        public bool useImportEnviornmentMap;
        public bool showNormal;
        public bool showFaces;
        public bool debug;
    }

    [SerializeField]
    CubeToSphericalMapParams m_cm2smParams = new CubeToSphericalMapParams
    {
        useImportEnviornmentMap = true,
        showNormal = false,
        showFaces = false,
        debug = false
    };
    public CubeToSphericalMapParams M_cm2smParams => m_cm2smParams;


    /// <summary>
    /// color flag setting reference: https://zhuanlan.zhihu.com/p/35096536
    /// </summary>
    /// <param name="flag"></param>
    /// <returns></returns>
    public Vector4 SetHDRDecodeFlag(Flag_samLinearClampHDR flag)
    {
        if (flag == Flag_samLinearClampHDR.pc_default)
            return new Vector4(1f, 1f, 0f, 1f);
        else if (flag == Flag_samLinearClampHDR.android_gamma_space)
            return new Vector4(2f, 1f, 0f, 0f);
        else if (flag == Flag_samLinearClampHDR.android_linear_space)
            return new Vector4(4.59f, 1f, 0f, 0f);      //return new Vector4(GammaToLinearSpace(2f), 1f, 0f, 0f);

        return new Vector4(1f, 1f, 0f, 1f);
    }

    //Vector3 GammaToLinearSpace(Vector3 sRGB)
    //{
    //    // Approximate version from http://chilliant.blogspot.com.au/2012/08/srgb-approximations-for-hlsl.html?m=1
    //    return sRGB * (sRGB * (sRGB * 0.305306011h + 0.682171111h) + 0.012522878h);

    //    // Precise version, useful for debugging.
    //    //return half3(GammaToLinearSpaceExact(sRGB.r), GammaToLinearSpaceExact(sRGB.g), GammaToLinearSpaceExact(sRGB.b));
    //}
}