using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct NoiseValues
{
    public int _seed;

    public int BaseHeight;
    public int ScaleHeight;

    public int Octaves;
    public float Scale;
    public float Persistance;
    public float Lacunarity;

    public float AtlasTextureInBlocks;
    public float NormlizedBlockTexture;
    public void SetSeed(string Seed)
    {
        _seed = Seed.GetHashCode();
    }
}
