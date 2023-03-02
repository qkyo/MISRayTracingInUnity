using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/Enviornment Map/Cube Map Setting")]
public class CubeMapSetting : ScriptableObject
{
    public Cubemap cubemapping;
    public bool useImportEnviornmentMap = true;
    public bool showNormal = false;
    public bool showFaces = false;
    public bool debug = false;
}