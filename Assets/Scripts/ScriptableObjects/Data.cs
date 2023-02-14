using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Scriptable/Data")]
public class Data : ScriptableObject
{
    public byte _Width;
    public int _Height;
    public float VoxelScale;

    public byte RenderDistance;

    public bool jobs;
    public string Seed;

    public NoiseValues NoiseData;
    public Material[] _materials;
    
    public Vector3[] ToArray(float3[] values)
    {
        Vector3[] result = new Vector3[values.Length];

        for (int i = 0; i < values.Length; i++)
            result[i] = values[i];

        return result;
    }
    public Vector2[] ToArray(float2[] values)
    {
        Vector2[] result = new Vector2[values.Length];

        for (int i = 0; i < values.Length; i++)
            result[i] = values[i];

        return result;
    }

    // Index <-> Position
    public Vector3Int PositionFromIndex(int index)
    {
        return new(index % _Width, (index / _Width) % _Height, index / (_Width * _Height));
    }

    public int IndexFromPosition(Vector3Int localPos)
    {
        return localPos.x + _Width * localPos.y + _Width * _Height * localPos.z;
    }
}
public enum VoxelType
{
    Nothing,
    Grass,
    GrassDirt,
    Dirt,
    Stone,
    Error404
}
public enum VoxelFace
{
    Front,
    Back,
    Up,
    Down,
    Right,
    Left
}