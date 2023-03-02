using System;
using UnityEngine;
using static System.Math;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class CubeMap
{
    CubeMapSetting cubeMapSetting;

    private const float G_PI = 3.14159265358979323846f,
                        FLT_EPSILON = 1.192092896e-07F;

    int width;
    int height;
    Color[] colorStream;
    Color[,] colorArray;
    public int Width => width;
    public int Height => height;
    public Color[] ColorStream => colorStream;
    public Color[,] ColorArray => colorArray;

    //public List<Color> colorStream;
    //public List<List<Color>> color2DArray;

    public CubeMap() { }
    //public CubeMap(CubeMapSetting cubeMapSetting)
    //{
    //    this.cubeMapSetting = cubeMapSetting;

    //    Texture2D cross = cubeMapSetting.cubemap;
    //    this.width = cross.width;
    //    this.height = cross.height;

    //    Texture2Color(cross);
    //}

    public CubeMap(int width, int height)
    {
        this.width = width;
        this.height = height;

        Texture2Color(width, height);
    }

    public CubeMap(int width, int height, Color[] colorStream)
    {
        this.width = width;
        this.height = height;
        this.colorStream = colorStream;
        this.colorArray = new Color[height, width];

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                colorArray[i, j] = colorStream[i * width + j];
            }
        }

        Debug.Log(colorStream[270] + "debuging" + colorArray[0, 270]);

        //this.colorStream = new List<Color>();
        //this.color2DArray = new List<List<Color>>();

        //for (int i = 0; i < height; i++)
        //{
        //    List<Color> eachRow = new List<Color>();
        //    for (int j = 0; j < width; j++)
        //    {
        //        if ((i + j) % width == i)
        //            eachRow.Add(colorStream[i+j]);
        //    }
        //    color2DArray.Add(eachRow);
        //}
    }

    public void SetPixel(int width, int height, Color col) {
        this.colorArray[height, width] = col;
        this.colorStream[height + width] = col;
    }

    void Texture2Color(Texture2D cross)
    {
        this.colorStream = new Color[cross.width * cross.height]; 
        this.colorArray = new Color[height, width];

        Color[] txtColors = cross.GetPixels();

        //for (int i = 0; i < colorStream.Length; i++)
        //{
        //    colorStream[i] = txtColors[i];
        //}

        for (int i = 0; i < height; i++) 
        {
            for (int j = 0; j < width; j++)
            {
                colorStream[i + j] = txtColors[i + j];
                colorArray[i, j] = txtColors[i * width + j];
            }
        }

        // this.color2DArray = new List<List<Color>>();

        //for (int i = 0; i < height; i++)
        //{
        //    List<Color> eachRow = new List<Color>();
        //    for (int j = 0; j < width; j++)
        //    {
        //        eachRow.Add(cross.GetPixel(i, j));
        //        colorStream.Add(cross.GetPixel(i, j));
        //    }
        //    color2DArray.Add(eachRow);
        //}

        Debug.Log(txtColors[270] + "debuging" + cross.GetPixel(0, 270) + "debuging" + colorStream[270] + "debuging" + colorArray[0, 270]);
    }

    void Texture2Color(int width, int height)
    {
        this.colorStream = new Color[width * height];
        this.colorArray = new Color[height, width];

        // Initialize color as black
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                colorStream[i + j] = Color.black;
                colorArray[i, j] = Color.black;
            }
        }

        // this.color2DArray = new List<List<Color>>();

        //for (int i = 0; i < height; i++)
        //{
        //    List<Color> eachRow = new List<Color>();
        //    for (int j = 0; j < width; j++)
        //    {
        //        eachRow.Add(Color.red);
        //        colorStream.Add(Color.red);
        //    }
        //    color2DArray.Add(eachRow);
        //}

        // Debug.Log(width + ", " + height);
    }

    public Texture2D ColorStream2Texture()
    {
        if (width <= 0 || height <= 0)
            return null;

        Texture2D texture = new Texture2D(width, height);

        texture.SetPixels(colorStream);
        //for (int k = 0; k < colorStream.Length; k++)
        //    texture.SetPixel(k / width, k % width, colorStream[k]);

        texture.Apply();

        return texture;
    }

    public Texture2D Color2DArray2Texture()
    {
        if (width <= 0 || height <= 0)
            return null;

        Texture2D texture = new Texture2D(width, height);

        // Debug.Log(width + ", " + height);

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                //texture.SetPixel(i, j, color2DArray[i][j]);
                texture.SetPixel(i, j, colorArray[i, j]);
            }
        }

        texture.Apply();

        return texture;
    }

    static float Clamp(float x, float a, float b)
    {
        return ((x) < (a)) ? (a) : (((x) < (b)) ? (x) : (b));
    }

    public static Color CubemapLookup(Vector3 v0, in Color[] cmColorStream, int cmside)
    {
        int[] idx = new int[] { -1, -1, -1, -1 };
        float[] val = new float[] { -1f, -1f, -1f, -1f };

        CubemapLookup(v0, cmside, ref idx, ref val);

        Color col = cmColorStream[idx[0]] * val[0];
        col += cmColorStream[idx[1]] * val[1];
        col += cmColorStream[idx[2]] * val[2];
        col += cmColorStream[idx[3]] * val[3];

        // float col = greyStream[idx[0]] * val[0];

        return col;
    }

    public static void CubemapLookup(Vector3 v0, int cm_side, ref int[] idx, ref float[] val)
    {
        Vector3 v = v0;
        idx = new int[] { -1, -1, -1, -1 };
        val = new float[] { -1f, -1f, -1f, -1f };

        int plane;
        float sx,
              sy;

        int major_plane = 0;
        if (Abs(v.x) < Abs(v.y))
            major_plane = 1;

        if (major_plane == 1)
        {
            if (Abs(v.y) < Abs(v.z))
                major_plane = 2;
        }
        else
        {
            if (Abs(v.x) < Abs(v.z))
                major_plane = 2;
        }


        switch (major_plane)
        {
            case 0:
                if (v.x > 0)
                {
                    //{  1,  1, -1 },  // v0
                    v = v / v.x;
                    sx = (1 + v.z) / 2;
                    sy = (1 - v.y) / 2;
                    plane = 0;

                    sx = Clamp(sx, 0, 1 - FLT_EPSILON) * cm_side + .5f;
                    sy = Clamp(sy, 0, 1 - FLT_EPSILON) * cm_side + .5f;

                    int x0, y0, x1, y1;
                    x0 = (int)Clamp((float)Floor(sx), 1, cm_side) - 1;
                    y0 = (int)Clamp((float)Floor(sy), 1, cm_side) - 1;
                    x1 = (int)Clamp((float)Ceiling(sx), 1, cm_side) - 1;
                    y1 = (int)Clamp((float)Ceiling(sy), 1, cm_side) - 1;

                    int idx0, idx1, idx2, idx3;
                    idx0 = cm_side * cm_side * plane + y0 * cm_side + x0;
                    idx1 = cm_side * cm_side * plane + y0 * cm_side + x1;
                    idx2 = cm_side * cm_side * plane + y1 * cm_side + x0;
                    idx3 = cm_side * cm_side * plane + y1 * cm_side + x1;

                    float rx, ry;
                    //sx = G_CLAMP( sx, 1, cm_side ) - 1;
                    //sy = G_CLAMP( sy, 1, cm_side ) - 1;
                    rx = sx - (float)Floor(sx);
                    ry = sy - (float)Floor(sy);

                    idx[0] = idx0;
                    idx[1] = idx1;
                    idx[2] = idx2;
                    idx[3] = idx3;
                    val[0] = (1 - rx) * (1 - ry);
                    val[1] = (rx) * (1 - ry);
                    val[2] = (1 - rx) * (ry);
                    val[3] = (rx) * (ry);
                }
                else
                {
                    //{ -1,  1,  1 },  // v2
                    v = v / (-v.x);
                    sx = (1 - v.z) / 2;
                    sy = (1 - v.y) / 2;
                    plane = 1;

                    sx = Clamp(sx, 0, 1 - FLT_EPSILON) * cm_side + .5f;
                    sy = Clamp(sy, 0, 1 - FLT_EPSILON) * cm_side + .5f;

                    int x0, y0, x1, y1;
                    x0 = (int)Clamp((float)Floor(sx), 1, cm_side) - 1;
                    y0 = (int)Clamp((float)Floor(sy), 1, cm_side) - 1;
                    x1 = (int)Clamp((float)Ceiling(sx), 1, cm_side) - 1;
                    y1 = (int)Clamp((float)Ceiling(sy), 1, cm_side) - 1;

                    int idx0, idx1, idx2, idx3;
                    idx0 = cm_side * cm_side * plane + y0 * cm_side + x0;
                    idx1 = cm_side * cm_side * plane + y0 * cm_side + x1;
                    idx2 = cm_side * cm_side * plane + y1 * cm_side + x0;
                    idx3 = cm_side * cm_side * plane + y1 * cm_side + x1;

                    float rx, ry;
                    //sx = G_CLAMP( sx, 1, cm_side ) - 1;
                    //sy = G_CLAMP( sy, 1, cm_side ) - 1;
                    rx = sx - (float)Floor(sx);
                    ry = sy - (float)Floor(sy);

                    idx[0] = idx0;
                    idx[1] = idx1;
                    idx[2] = idx2;
                    idx[3] = idx3;
                    val[0] = (1 - rx) * (1 - ry);
                    val[1] = (rx) * (1 - ry);
                    val[2] = (1 - rx) * (ry);
                    val[3] = (rx) * (ry);
                }
                break;

            case 1:
                if (v.y > 0)
                {
                    //{  1,  1, -1 },  // v0
                    v = v / v.y;
                    sx = (1 - v.x) / 2;
                    sy = (1 + v.z) / 2;
                    plane = 2;

                    sx = Clamp(sx, 0, 1 - FLT_EPSILON) * cm_side + .5f;
                    sy = Clamp(sy, 0, 1 - FLT_EPSILON) * cm_side + .5f;

                    int x0, y0, x1, y1;
                    x0 = (int)Clamp((float)Floor(sx), 1, cm_side) - 1;
                    y0 = (int)Clamp((float)Floor(sy), 1, cm_side) - 1;
                    x1 = (int)Clamp((float)Ceiling(sx), 1, cm_side) - 1;
                    y1 = (int)Clamp((float)Ceiling(sy), 1, cm_side) - 1;

                    int idx0, idx1, idx2, idx3;
                    idx0 = cm_side * cm_side * plane + y0 * cm_side + x0;
                    idx1 = cm_side * cm_side * plane + y0 * cm_side + x1;
                    idx2 = cm_side * cm_side * plane + y1 * cm_side + x0;
                    idx3 = cm_side * cm_side * plane + y1 * cm_side + x1;

                    float rx, ry;
                    //sx = G_CLAMP( sx, 1, cm_side ) - 1;
                    //sy = G_CLAMP( sy, 1, cm_side ) - 1;
                    rx = sx - (float)Floor(sx);
                    ry = sy - (float)Floor(sy);

                    idx[0] = idx0;
                    idx[1] = idx1;
                    idx[2] = idx2;
                    idx[3] = idx3;
                    val[0] = (1 - rx) * (1 - ry);
                    val[1] = (rx) * (1 - ry);
                    val[2] = (1 - rx) * (ry);
                    val[3] = (rx) * (ry);
                }
                else
                {
                    //{  1, -1,  1 },  // v7
                    v = v / (-v.y);
                    sx = (1 - v.x) / 2;
                    sy = (1 - v.z) / 2;
                    plane = 3;

                    sx = Clamp(sx, 0, 1 - FLT_EPSILON) * cm_side + .5f;
                    sy = Clamp(sy, 0, 1 - FLT_EPSILON) * cm_side + .5f;

                    int x0, y0, x1, y1;
                    x0 = (int)Clamp((float)Floor(sx), 1, cm_side) - 1;
                    y0 = (int)Clamp((float)Floor(sy), 1, cm_side) - 1;
                    x1 = (int)Clamp((float)Ceiling(sx), 1, cm_side) - 1;
                    y1 = (int)Clamp((float)Ceiling(sy), 1, cm_side) - 1;

                    int idx0, idx1, idx2, idx3;
                    idx0 = cm_side * cm_side * plane + y0 * cm_side + x0;
                    idx1 = cm_side * cm_side * plane + y0 * cm_side + x1;
                    idx2 = cm_side * cm_side * plane + y1 * cm_side + x0;
                    idx3 = cm_side * cm_side * plane + y1 * cm_side + x1;

                    float rx, ry;
                    //sx = G_CLAMP( sx, 1, cm_side ) - 1;
                    //sy = G_CLAMP( sy, 1, cm_side ) - 1;
                    rx = sx - (float)Floor(sx);
                    ry = sy - (float)Floor(sy);

                    idx[0] = idx0;
                    idx[1] = idx1;
                    idx[2] = idx2;
                    idx[3] = idx3;
                    val[0] = (1 - rx) * (1 - ry);
                    val[1] = (rx) * (1 - ry);
                    val[2] = (1 - rx) * (ry);
                    val[3] = (rx) * (ry);
                }
                break;

            case 2:
                if (v.z > 0)
                {
                    //{  1,  1,  1 },  // v3
                    v = v / v.z;
                    sx = (1 - v.x) / 2;
                    sy = (1 - v.y) / 2;
                    plane = 4;

                    sx = Clamp(sx, 0, 1 - FLT_EPSILON) * cm_side + .5f;
                    sy = Clamp(sy, 0, 1 - FLT_EPSILON) * cm_side + .5f;

                    int x0, y0, x1, y1;
                    x0 = (int)Clamp((float)Floor(sx), 1, cm_side) - 1;
                    y0 = (int)Clamp((float)Floor(sy), 1, cm_side) - 1;
                    x1 = (int)Clamp((float)Ceiling(sx), 1, cm_side) - 1;
                    y1 = (int)Clamp((float)Ceiling(sy), 1, cm_side) - 1;

                    int idx0, idx1, idx2, idx3;
                    idx0 = cm_side * cm_side * plane + y0 * cm_side + x0;
                    idx1 = cm_side * cm_side * plane + y0 * cm_side + x1;
                    idx2 = cm_side * cm_side * plane + y1 * cm_side + x0;
                    idx3 = cm_side * cm_side * plane + y1 * cm_side + x1;

                    float rx, ry;
                    //sx = G_CLAMP( sx, 1, cm_side ) - 1;
                    //sy = G_CLAMP( sy, 1, cm_side ) - 1;
                    rx = sx - (float)Floor(sx);
                    ry = sy - (float)Floor(sy);

                    idx[0] = idx0;
                    idx[1] = idx1;
                    idx[2] = idx2;
                    idx[3] = idx3;
                    val[0] = (1 - rx) * (1 - ry);
                    val[1] = (rx) * (1 - ry);
                    val[2] = (1 - rx) * (ry);
                    val[3] = (rx) * (ry);
                }
                else
                {
                    //{ -1,  1, -1 },  // v1
                    v = v / (-v.z);
                    sx = (1 + v.x) / 2;
                    sy = (1 - v.y) / 2;
                    plane = 5;

                    sx = Clamp(sx, 0, 1 - FLT_EPSILON) * cm_side + .5f;
                    sy = Clamp(sy, 0, 1 - FLT_EPSILON) * cm_side + .5f;

                    int x0, y0, x1, y1;
                    x0 = (int)Clamp((float)Floor(sx), 1, cm_side) - 1;
                    y0 = (int)Clamp((float)Floor(sy), 1, cm_side) - 1;
                    x1 = (int)Clamp((float)Ceiling(sx), 1, cm_side) - 1;
                    y1 = (int)Clamp((float)Ceiling(sy), 1, cm_side) - 1;

                    int idx0, idx1, idx2, idx3;
                    idx0 = cm_side * cm_side * plane + y0 * cm_side + x0;
                    idx1 = cm_side * cm_side * plane + y0 * cm_side + x1;
                    idx2 = cm_side * cm_side * plane + y1 * cm_side + x0;
                    idx3 = cm_side * cm_side * plane + y1 * cm_side + x1;

                    float rx, ry;
                    //sx = G_CLAMP( sx, 1, cm_side ) - 1;
                    //sy = G_CLAMP( sy, 1, cm_side ) - 1;
                    rx = sx - (float)Floor(sx);
                    ry = sy - (float)Floor(sy);

                    idx[0] = idx0;
                    idx[1] = idx1;
                    idx[2] = idx2;
                    idx[3] = idx3;
                    val[0] = (1 - rx) * (1 - ry);
                    val[1] = (rx) * (1 - ry);
                    val[2] = (1 - rx) * (ry);
                    val[3] = (rx) * (ry);
                }
                break;
        };
    }


    //public static Color CubemapLookup(Vector3 v0, in List<float> greyStream, int cmside)
    //{
    //    int[] idx = new int[] { -1, -1, -1, -1 };
    //    float[] val = new float[] { -1f, -1f, -1f, -1f };

    //    // Debug.Log(v0);

    //    CubemapLookup(v0, cmside, ref idx, ref val);

    //    float col = greyStream[idx[0]] * val[0];
    //    col += greyStream[idx[1]] * val[1];
    //    col += greyStream[idx[2]] * val[2];
    //    col += greyStream[idx[3]] * val[3];

    //    // float col = greyStream[idx[0]] * val[0];

    //    return new Color(col, 0, 0, 1);
    //}


    //public static void CubemapLookup(Vector3 v0, int cm_side, ref int[] idx, ref float[] val)
    //{
    //    Vector3 v = v0;
    //    idx = new int[] { -1, -1, -1, -1 };
    //    val = new float[] { -1f, -1f, -1f, -1f };

    //    int plane;
    //    float sx, 
    //          sy;

    //    int major_plane = 0;
    //    if (Abs(v.x) < Abs(v.y))
    //        major_plane = 1;

    //    if (major_plane == 1)
    //    {
    //        if (Abs(v.y) < Abs(v.z))
    //            major_plane = 2;
    //    }
    //    else
    //    {
    //        if (Abs(v.x) < Abs(v.z))
    //            major_plane = 2;
    //    }


    //    switch (major_plane)
    //    {
    //        case 0:
    //            if (v.x > 0)
    //            {
    //                //{  1,  1, -1 },  // v0
    //                v = v / v.x;
    //                sx = (1 + v.z) / 2;
    //                sy = (1 - v.y) / 2;
    //                plane = 0;

    //                sx = Clamp(sx, 0, 1 - FLT_EPSILON) * cm_side + .5f;
    //                sy = Clamp(sy, 0, 1 - FLT_EPSILON) * cm_side + .5f;

    //                int x0, y0, x1, y1;
    //                x0 = (int)Clamp((float)Floor(sx), 1, cm_side) - 1;
    //                y0 = (int)Clamp((float)Floor(sy), 1, cm_side) - 1;
    //                x1 = (int)Clamp((float)Ceiling(sx), 1, cm_side) - 1;
    //                y1 = (int)Clamp((float)Ceiling(sy), 1, cm_side) - 1;

    //                int idx0, idx1, idx2, idx3;
    //                idx0 = cm_side * cm_side * plane + y0 * cm_side + x0;
    //                idx1 = cm_side * cm_side * plane + y0 * cm_side + x1;
    //                idx2 = cm_side * cm_side * plane + y1 * cm_side + x0;
    //                idx3 = cm_side * cm_side * plane + y1 * cm_side + x1;

    //                float rx, ry;
    //                //sx = G_CLAMP( sx, 1, cm_side ) - 1;
    //                //sy = G_CLAMP( sy, 1, cm_side ) - 1;
    //                rx = sx - (float)Floor(sx);
    //                ry = sy - (float)Floor(sy);

    //                idx[0] = idx0;
    //                idx[1] = idx1;
    //                idx[2] = idx2;
    //                idx[3] = idx3;
    //                val[0] = (1 - rx) * (1 - ry);
    //                val[1] = (rx) * (1 - ry);
    //                val[2] = (1 - rx) * (ry);
    //                val[3] = (rx) * (ry);
    //            }
    //            else
    //            {
    //                //{ -1,  1,  1 },  // v2
    //                v = v / (-v.x);
    //                sx = (1 - v.z) / 2;
    //                sy = (1 - v.y) / 2;
    //                plane = 1;

    //                sx = Clamp(sx, 0, 1 - FLT_EPSILON) * cm_side + .5f;
    //                sy = Clamp(sy, 0, 1 - FLT_EPSILON) * cm_side + .5f;

    //                int x0, y0, x1, y1;
    //                x0 = (int)Clamp((float)Floor(sx), 1, cm_side) - 1;
    //                y0 = (int)Clamp((float)Floor(sy), 1, cm_side) - 1;
    //                x1 = (int)Clamp((float)Ceiling(sx), 1, cm_side) - 1;
    //                y1 = (int)Clamp((float)Ceiling(sy), 1, cm_side) - 1;

    //                int idx0, idx1, idx2, idx3;
    //                idx0 = cm_side * cm_side * plane + y0 * cm_side + x0;
    //                idx1 = cm_side * cm_side * plane + y0 * cm_side + x1;
    //                idx2 = cm_side * cm_side * plane + y1 * cm_side + x0;
    //                idx3 = cm_side * cm_side * plane + y1 * cm_side + x1;

    //                float rx, ry;
    //                //sx = G_CLAMP( sx, 1, cm_side ) - 1;
    //                //sy = G_CLAMP( sy, 1, cm_side ) - 1;
    //                rx = sx - (float)Floor(sx);
    //                ry = sy - (float)Floor(sy);

    //                idx[0] = idx0;
    //                idx[1] = idx1;
    //                idx[2] = idx2;
    //                idx[3] = idx3;
    //                val[0] = (1 - rx) * (1 - ry);
    //                val[1] = (rx) * (1 - ry);
    //                val[2] = (1 - rx) * (ry);
    //                val[3] = (rx) * (ry);
    //            }
    //            break;

    //        case 1:
    //            if (v.y > 0)
    //            {
    //                //{  1,  1, -1 },  // v0
    //                v = v / v.y;
    //                sx = (1 - v.x) / 2;
    //                sy = (1 + v.z) / 2;
    //                plane = 2;

    //                sx = Clamp(sx, 0, 1 - FLT_EPSILON) * cm_side + .5f;
    //                sy = Clamp(sy, 0, 1 - FLT_EPSILON) * cm_side + .5f;

    //                int x0, y0, x1, y1;
    //                x0 = (int)Clamp((float)Floor(sx), 1, cm_side) - 1;
    //                y0 = (int)Clamp((float)Floor(sy), 1, cm_side) - 1;
    //                x1 = (int)Clamp((float)Ceiling(sx), 1, cm_side) - 1;
    //                y1 = (int)Clamp((float)Ceiling(sy), 1, cm_side) - 1;

    //                int idx0, idx1, idx2, idx3;
    //                idx0 = cm_side * cm_side * plane + y0 * cm_side + x0;
    //                idx1 = cm_side * cm_side * plane + y0 * cm_side + x1;
    //                idx2 = cm_side * cm_side * plane + y1 * cm_side + x0;
    //                idx3 = cm_side * cm_side * plane + y1 * cm_side + x1;

    //                float rx, ry;
    //                //sx = G_CLAMP( sx, 1, cm_side ) - 1;
    //                //sy = G_CLAMP( sy, 1, cm_side ) - 1;
    //                rx = sx - (float)Floor(sx);
    //                ry = sy - (float)Floor(sy);

    //                idx[0] = idx0;
    //                idx[1] = idx1;
    //                idx[2] = idx2;
    //                idx[3] = idx3;
    //                val[0] = (1 - rx) * (1 - ry);
    //                val[1] = (rx) * (1 - ry);
    //                val[2] = (1 - rx) * (ry);
    //                val[3] = (rx) * (ry);
    //            }
    //            else
    //            {
    //                //{  1, -1,  1 },  // v7
    //                v = v / (-v.y);
    //                sx = (1 - v.x) / 2;
    //                sy = (1 - v.z) / 2;
    //                plane = 3;

    //                sx = Clamp(sx, 0, 1 - FLT_EPSILON) * cm_side + .5f;
    //                sy = Clamp(sy, 0, 1 - FLT_EPSILON) * cm_side + .5f;

    //                int x0, y0, x1, y1;
    //                x0 = (int)Clamp((float)Floor(sx), 1, cm_side) - 1;
    //                y0 = (int)Clamp((float)Floor(sy), 1, cm_side) - 1;
    //                x1 = (int)Clamp((float)Ceiling(sx), 1, cm_side) - 1;
    //                y1 = (int)Clamp((float)Ceiling(sy), 1, cm_side) - 1;

    //                int idx0, idx1, idx2, idx3;
    //                idx0 = cm_side * cm_side * plane + y0 * cm_side + x0;
    //                idx1 = cm_side * cm_side * plane + y0 * cm_side + x1;
    //                idx2 = cm_side * cm_side * plane + y1 * cm_side + x0;
    //                idx3 = cm_side * cm_side * plane + y1 * cm_side + x1;

    //                float rx, ry;
    //                //sx = G_CLAMP( sx, 1, cm_side ) - 1;
    //                //sy = G_CLAMP( sy, 1, cm_side ) - 1;
    //                rx = sx - (float)Floor(sx);
    //                ry = sy - (float)Floor(sy);

    //                idx[0] = idx0;
    //                idx[1] = idx1;
    //                idx[2] = idx2;
    //                idx[3] = idx3;
    //                val[0] = (1 - rx) * (1 - ry);
    //                val[1] = (rx) * (1 - ry);
    //                val[2] = (1 - rx) * (ry);
    //                val[3] = (rx) * (ry);
    //            }
    //            break;

    //        case 2:
    //            if (v.z > 0)
    //            {
    //                //{  1,  1,  1 },  // v3
    //                v = v / v.z;
    //                sx = (1 - v.x) / 2;
    //                sy = (1 - v.y) / 2;
    //                plane = 4;

    //                sx = Clamp(sx, 0, 1 - FLT_EPSILON) * cm_side + .5f;
    //                sy = Clamp(sy, 0, 1 - FLT_EPSILON) * cm_side + .5f;

    //                int x0, y0, x1, y1;
    //                x0 = (int)Clamp((float)Floor(sx), 1, cm_side) - 1;
    //                y0 = (int)Clamp((float)Floor(sy), 1, cm_side) - 1;
    //                x1 = (int)Clamp((float)Ceiling(sx), 1, cm_side) - 1;
    //                y1 = (int)Clamp((float)Ceiling(sy), 1, cm_side) - 1;

    //                int idx0, idx1, idx2, idx3;
    //                idx0 = cm_side * cm_side * plane + y0 * cm_side + x0;
    //                idx1 = cm_side * cm_side * plane + y0 * cm_side + x1;
    //                idx2 = cm_side * cm_side * plane + y1 * cm_side + x0;
    //                idx3 = cm_side * cm_side * plane + y1 * cm_side + x1;

    //                float rx, ry;
    //                //sx = G_CLAMP( sx, 1, cm_side ) - 1;
    //                //sy = G_CLAMP( sy, 1, cm_side ) - 1;
    //                rx = sx - (float)Floor(sx);
    //                ry = sy - (float)Floor(sy);

    //                idx[0] = idx0;
    //                idx[1] = idx1;
    //                idx[2] = idx2;
    //                idx[3] = idx3;
    //                val[0] = (1 - rx) * (1 - ry);
    //                val[1] = (rx) * (1 - ry);
    //                val[2] = (1 - rx) * (ry);
    //                val[3] = (rx) * (ry);
    //            }
    //            else
    //            {
    //                //{ -1,  1, -1 },  // v1
    //                v = v / (-v.z);
    //                sx = (1 + v.x) / 2;
    //                sy = (1 - v.y) / 2;
    //                plane = 5;

    //                sx = Clamp(sx, 0, 1 - FLT_EPSILON) * cm_side + .5f;
    //                sy = Clamp(sy, 0, 1 - FLT_EPSILON) * cm_side + .5f;

    //                int x0, y0, x1, y1;
    //                x0 = (int)Clamp((float)Floor(sx), 1, cm_side) - 1;
    //                y0 = (int)Clamp((float)Floor(sy), 1, cm_side) - 1;
    //                x1 = (int)Clamp((float)Ceiling(sx), 1, cm_side) - 1;
    //                y1 = (int)Clamp((float)Ceiling(sy), 1, cm_side) - 1;

    //                int idx0, idx1, idx2, idx3;
    //                idx0 = cm_side * cm_side * plane + y0 * cm_side + x0;
    //                idx1 = cm_side * cm_side * plane + y0 * cm_side + x1;
    //                idx2 = cm_side * cm_side * plane + y1 * cm_side + x0;
    //                idx3 = cm_side * cm_side * plane + y1 * cm_side + x1;

    //                float rx, ry;
    //                //sx = G_CLAMP( sx, 1, cm_side ) - 1;
    //                //sy = G_CLAMP( sy, 1, cm_side ) - 1;
    //                rx = sx - (float)Floor(sx);
    //                ry = sy - (float)Floor(sy);

    //                idx[0] = idx0;
    //                idx[1] = idx1;
    //                idx[2] = idx2;
    //                idx[3] = idx3;
    //                val[0] = (1 - rx) * (1 - ry);
    //                val[1] = (rx) * (1 - ry);
    //                val[2] = (1 - rx) * (ry);
    //                val[3] = (rx) * (ry);
    //            }
    //            break;
    //    };


    //}
}

