using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WalkerAliasRandomizer;
using Utils;
using static System.MathF;
using System;

public class WalkerAliasGenerator : MonoBehaviour
{
    public CubeMapSetting cubeMapSetting;
    public ComputeShader cm2smShader;

    RenderTexture cm2smResult;
    Texture2D sphericalMap, smapSolidAngle, smapMulSolidAngle, sumWeightedSphericalMap; 
    IList<KeyValuePair<int, double>> sphericalMapProb;
    IList<WeightedListItem<int>> weightedList;
    WeightedList<int> walkerAlias;

    readonly static float G_PI = 3.14159265358979323846f;
    
    private readonly int
        walkerAliasAliasShaderId = Shader.PropertyToID("_WalkerAliasAlias"),
        walkerAliasProbsShaderId = Shader.PropertyToID("_WalkerAliasProbs");

    
    private int[] walker_alias;
    private float[] walker_probs, _smapSolidAngle, _smapMulSolidAngle;
    public int[] Walker_alias => walker_alias;
    public float[] Walker_probs => walker_probs;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void GenerativeMethod()
    {
        cm2smResult = SetupSphericalMap();
        sphericalMap = GetSmapTexture2D(cm2smResult);

        // smapSolidAngle = new Texture2D(sphericalMap.width, sphericalMap.height);
        // smapMulSolidAngle = new Texture2D(sphericalMap.width, sphericalMap.height);

        _smapSolidAngle = new float[sphericalMap.width * sphericalMap.height];
        _smapMulSolidAngle = new float[sphericalMap.width * sphericalMap.height];
        weightedList = new List<WeightedListItem<int>>();

        PrepareWalkerAliasProbs(sphericalMap, ref _smapSolidAngle, ref _smapMulSolidAngle, ref weightedList);
        
        walkerAlias = new WeightedList<int>(weightedList);

        SaveWalkerAlias();
    }

    void SaveWalkerAlias()
    {
        float[] walker_probs = walkerAlias.Probabilities.ToArray();
        int[] walker_alias = walkerAlias.Alias.ToArray();

        Debug.Log("Now :" + walker_probs[262134] + ", " + walker_alias[262134]);
        Color[] walker_probs_array = new Color[walker_probs.Length];
        Color[] walker_alias_array = new Color[walker_alias.Length];

        float bias = .1f;

        for (int i=0; i<walker_probs.Length; i++)
        {
            walker_probs_array[i] = new Color(walker_probs[i] * bias, walker_probs[i] * bias, walker_probs[i] * bias, 1);
            walker_alias_array[i] = new Color(walker_alias[i], walker_alias[i], walker_alias[i], 1);
        }
        Texture2D walker_probs_texture = new Texture2D(1024, 512);
        Texture2D walker_alias_texture = new Texture2D(1024, 512);

        walker_probs_texture.SetPixels(walker_probs_array);
        walker_alias_texture.SetPixels(walker_alias_array);
        walker_probs_texture.Apply();
        walker_alias_texture.Apply();
        
        MyFile.SaveTexture2D(walker_probs_texture, "walker_probs_texture.pfm");
        MyFile.SaveTexture2D(walker_alias_texture, "walker_alias_texture.pfm");
        MyFile.SaveListToTextFile(walkerAlias.Alias, "walker_alias_alias.txt");
        MyFile.SaveListToTextFile(walkerAlias.Probabilities, "walker_alias_Probabilities.txt");
    }

    public RenderTexture SetupSphericalMap()
    {
        Cubemap cubeMap = cubeMapSetting.cubemapping;
        int kernelHandle = cm2smShader.FindKernel("Cm2SmMain");

        cm2smShader.GetKernelThreadGroupSizes(kernelHandle, out uint x, out uint y, out uint z);

        RenderTexture cm2smResult = new RenderTexture(1024, 512, 1);
        cm2smResult.enableRandomWrite = true;
        cm2smResult.Create();

        if (cubeMapSetting.M_cm2smParams.showFaces)
            cm2smShader.EnableKeyword("SHOW_FACES");
        else
            cm2smShader.DisableKeyword("SHOW_FACES");

        if (cubeMapSetting.M_cm2smParams.showNormal)
            cm2smShader.EnableKeyword("SHOW_NORMAL");
        else
            cm2smShader.DisableKeyword("SHOW_NORMAL");

        cm2smShader.SetTexture(kernelHandle, "Result", cm2smResult);
        cm2smShader.SetTexture(kernelHandle, "_CubeMapTexture", cubeMap);
        cm2smShader.SetVector("_TextureSize", new Vector3(1024, 512, 1));
        cm2smShader.Dispatch(kernelHandle, 1024, 512, 1);

        return cm2smResult;
    }

    Texture2D GetSmapTexture2D(RenderTexture renderTexture)
    {
        Texture2D textureReadable = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
        RenderTexture.active = renderTexture;
        textureReadable.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        textureReadable.Apply();

        return textureReadable;
    }


    void PrepareWalkerAliasProbs(Texture2D sphericalMap,
                                ref float[] smapSolidAngle,
                                ref float[] smapMulSolidAngle,
                                ref IList<WeightedListItem<int>> weightedList)
    {
        int sm_height = sphericalMap.height;
        int sm_width = sphericalMap.width;

        Color[] colorSmapArray = this.sphericalMap.GetPixels();

        float[] _geryScaleSmapArray = new float[sm_height * sm_width];
        float[] _smapSolidAngleArray = new float[sm_height * sm_width];

        int i, j;
        float sint;
        for (j = 0; j < sm_height; j++)
        {
            sint = Sin((j + .5f) * G_PI / sm_height);
            for (i = 0; i < sm_width; i++)
            {
                float scaleSolidAngle = 2 * G_PI * sint / sm_width * G_PI / sm_height;
                
                // Norm vector
                float greyScale = Sqrt(colorSmapArray[j*sm_width + i].r * colorSmapArray[j*sm_width + i].r 
                                        + colorSmapArray[j*sm_width + i].g * colorSmapArray[j*sm_width + i].g 
                                        + colorSmapArray[j*sm_width + i].b * colorSmapArray[j*sm_width + i].b );

                _geryScaleSmapArray[j*sm_width + i] = greyScale;
                _smapSolidAngleArray[j*sm_width + i] = scaleSolidAngle;
            }
        }
        // Multiply smap with solid angle.
        float[] _mulResult = new float[sm_height * sm_width];
        
        for (i = 0; i < sm_height * sm_width; i++)
        {
            _mulResult[i] = _geryScaleSmapArray[i] * _smapSolidAngleArray[i];
            // for initialize walkerAlias
            // walkerAliasProbs.Add(new KeyValuePair<int, double>(i, greyScale));

            // if (i > 262134 && i < 262144)
            //     Debug.Log(greyScale);
                // Debug.Log(pixels[i].r + " " + solidAngle[i].r + "\n"
                //         + pixels[i].g + " " + solidAngle[i].g + "\n"
                //         + pixels[i].b + " " + solidAngle[i].b + "\n");
            weightedList.Add(new WeightedListItem<int>(i, _mulResult[i]));
        }
        Debug.Log(weightedList[262134].Weight + "," + weightedList[232164].Item);
    }

    void PrepareWalkerAliasProbs(Texture2D sphericalMap,
                                ref Texture2D smapSolidAngle,
                                ref Texture2D smapMulSolidAngle,
                                ref IList<WeightedListItem<int>> weightedList)
    {
        int sm_height = sphericalMap.height;
        int sm_width = sphericalMap.width;

        Texture2D greyScaleSmap = new Texture2D(sm_width, sm_height);

        Color[] colorSmapArray = this.sphericalMap.GetPixels();
        Color[] geryScaleSmapArray = new Color[sm_height * sm_width];
        Color[] smapSolidAngleArray = new Color[sm_height * sm_width];

        int i, j;
        float sint;
        // float sumPA = 0f;
        for (j = 0; j < sm_height; j++)
        {
            sint = Sin((j + .5f) * G_PI / sm_height);
            for (i = 0; i < sm_width; i++)
            {
                float scaleSolidAngle = 2 * G_PI * sint / sm_width * G_PI / sm_height * 10000;
                
                // Norm vector
                float greyScale = Sqrt(colorSmapArray[j*sm_width + i].r * colorSmapArray[j*sm_width + i].r 
                                        + colorSmapArray[j*sm_width + i].g * colorSmapArray[j*sm_width + i].g 
                                        + colorSmapArray[j*sm_width + i].b * colorSmapArray[j*sm_width + i].b );

                smapSolidAngleArray[j*sm_width + i] = new Color(scaleSolidAngle, scaleSolidAngle, scaleSolidAngle, 1);
                geryScaleSmapArray[j*sm_width + i] = new Color(scaleSolidAngle, scaleSolidAngle, scaleSolidAngle, 1);
                
                // sumPA += scaleSolidAngle * scaleSolidAngle / 100;
            }
        }
        smapSolidAngle.SetPixels(smapSolidAngleArray);
        greyScaleSmap.SetPixels(geryScaleSmapArray);
        smapSolidAngle.Apply();
        greyScaleSmap.Apply();

        // MyFile.SaveTexture2D(sphericalMap, "sphericalMap.png");
        MyFile.SaveTexture2D(smapSolidAngle, "smapSolidAngle.pfm");
        MyFile.SaveTexture2D(greyScaleSmap, "greyScaleSmap.pfm");
        MyFile.SaveTexture2D(smapSolidAngle, "smapSolidAngle.png");
        MyFile.SaveTexture2D(greyScaleSmap, "greyScaleSmap.png");

        // Multiply smap with solid angle.
        Color[] mulResult = new Color[sphericalMap.width * sphericalMap.height];
        
        for (i = 0; i < sm_height * sm_width; i++)
        {
            mulResult[i] = new Color( geryScaleSmapArray[i].r * smapSolidAngleArray[i].r , 
                                      geryScaleSmapArray[i].g * smapSolidAngleArray[i].g , 
                                      geryScaleSmapArray[i].b * smapSolidAngleArray[i].b , 1);
            // for initialize walkerAlias
            // walkerAliasProbs.Add(new KeyValuePair<int, double>(i, greyScale));

            // if (i > 262134 && i < 262144)
            //     Debug.Log(greyScale);
                // Debug.Log(pixels[i].r + " " + solidAngle[i].r + "\n"
                //         + pixels[i].g + " " + solidAngle[i].g + "\n"
                //         + pixels[i].b + " " + solidAngle[i].b + "\n");
            weightedList.Add(new WeightedListItem<int>(i, mulResult[i].r));
        }

        // Debug.Log(weightedList[262134].Weight + "," + weightedList[232164].Item);
        smapMulSolidAngle.SetPixels(mulResult);
        smapMulSolidAngle.Apply();
        
        MyFile.SaveTexture2D(smapMulSolidAngle, "smapMulSolidAngle.pfm");
    }

}
