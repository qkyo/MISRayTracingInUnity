using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WalkerAliasRandomizer;
using Utils;
using static System.MathF;
using System;

public class WalkerAliasReader
{
    int height;
    int width;

    private int[] walker_alias;
    private float[] walker_probs;
    public int[] Walker_alias => walker_alias;
    public float[] Walker_probs => walker_probs;

    public WalkerAliasReader(Texture2D importedAlias, Texture2D importedProb) 
    {
        this.walker_alias = new int[importedAlias.width * importedAlias.height];
        this.walker_probs = new float[importedAlias.width * importedAlias.height];
        GetWalkerAliasArrays(importedAlias, importedProb);
    }
    
    public WalkerAliasReader()
    {
        GetWalkerAliasArrays ();
    }

    void GetWalkerAliasArrays (Texture2D importedAlias, Texture2D importedProb) 
    {
        if(importedAlias == null || importedProb == null)
            return;

        Color[] importedProbPixels = importedProb.GetPixels();
        Color[] importedProbAlias = importedAlias.GetPixels();

        for (int i = 0; i < importedAlias.width * importedAlias.height; i++){
            walker_probs[i] = importedProbPixels[i].r;
            walker_alias[i] = (int)importedProbAlias[i].r;
        }
    
    }

    void GetWalkerAliasArrays () 
    {

        height = 512;
        width = 1024;

        walker_probs = new float[width * height];
        walker_alias = new int[width * height];

        this.walker_probs = MyFile.ReadFloatBinaryFile("rogers_fig2c_prob.dat", height * width);
        this.walker_alias = MyFile.ReadIntBinaryFile("rogers_fig2c_alias.dat", height * width);
    
        
        // horizontal mirror then save
        /*
        float[] walker_probs_temp = new float[width * height];
        int[] walker_alias_temp = new int[width * height];

        for (int i = 0; i < height; i++)
            for (int j = 0; j < width; j++)
            {
                walker_probs_temp[i * width + j] = walker_probs[i * width + width - 1 - j];
                walker_alias_temp[i * width + j] = walker_alias[i * width + width - 1 - j];
            }
        
        MyFile.SaveTexture2D(MyFile.FloatArray2Texture2D(1024, 512, walker_probs_temp), "result.png");
        MyFile.SaveTexture2D(MyFile.IntArray2Texture2D(1024, 512, walker_alias_temp), "result_alias.png");
        */
    }
}