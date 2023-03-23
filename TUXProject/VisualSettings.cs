using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TUX;
[Serializable]
public struct VisualSettings
{
    public string path;
    public string[] texturesPath;
    public Texture DiffuseTexture, MettallicTexture, NormalTexture,
        OcclusionTexture, EmissionTexture, PaintMapTexture;
    static float mMetalicSmoothness = 0, mSmoothnessScale = 1, mMipBias = 0.8f, nDetailNormalScale = 1, nDetailNormalTiling = 1f, oOcclusionStrenght = 1,
        timeOfDayMin = -0.005f, timeOfDayMax = 0.005f, pmSmoothnessScale = 1, pmRimFalloff = 1f;
    static bool useTimeOfDay = false, pmSmoothnessOverride = false;
}
