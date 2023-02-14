using Freya;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class ChunkRenderer : MonoBehaviour
{
    ChunkData chunkData;
    NativeArray<VoxelType> _terrainMap;

    private void Awake()
    {
        chunkData = (ChunkData)ScriptableObject.CreateInstance(typeof(ChunkData));

        chunkData.MeshCollider = GetComponent<MeshCollider>();
        chunkData.MeshFilter = GetComponent<MeshFilter>();
        chunkData.MeshRenderer = GetComponent<MeshRenderer>();
    }

    public JobHandle DataInit(Data _Data, NativeArray<float3> VertexPosition, NativeArray<int3> QuadFacePosition, bool jobs)
    {
        chunkData.Rcoords = transform.position;
        chunkData.Data = _Data;
        chunkData.SetArrays();

        int width = _Data._Width + 2; int height = _Data._Height;
        _terrainMap = new(width * height * width, Allocator.Persistent);

        NoiseCalculation noiseCalculation = new()
        {
            Width = (byte)width,
            Height = height,
            Noise = _Data.NoiseData,
            Coords = chunkData.Rcoords,
            VoxelScale = _Data.VoxelScale,
            VertexData = VertexPosition,
            VertexFace = QuadFacePosition,

            TerrainMap = _terrainMap,

            _vertices = chunkData._vertices,
            _triangles = chunkData._triangles,
            _trianglesCollider = chunkData._trianglesCollider,
            _verticesCollider = chunkData._verticesCollider,
            _uvs = chunkData._uvs
        };

        return noiseCalculation.Schedule();
    }

    public void DataInit(Data _Data, NativeArray<float3> VertexPosition, NativeArray<int3> QuadFacePosition)
    {
        chunkData.Rcoords = transform.position;
        chunkData.Data = _Data;
        chunkData.SetArrays();
        
        int Width = _Data._Width + 2; int Height = _Data._Height;
        _terrainMap = new(Width * Height * Width, Allocator.Persistent);

        //Execute Main Thread Job

        int size = Width * Width;

        NoiseValues Noise = chunkData.Data.NoiseData;
        NativeArray<float> heightMap = new(size, Allocator.Temp);

        for (int z = 0; z < Width; z++)
            for (int x = 0; x < Width; x++)
            {
                float amplitude = 1f;
                float frecuency = 1f;
                float noiseHeight = 0f;

                float X = (x + chunkData.Rcoords.x) / Noise.Scale;
                float Z = (z + chunkData.Rcoords.z) / Noise.Scale;

                for (int o = 0; o < Noise.Octaves; o++)
                {
                    noiseHeight += noise.snoise(new float2(X * frecuency, Z * frecuency)) * amplitude;

                    amplitude *= Noise.Persistance;
                    frecuency *= Noise.Lacunarity;
                }

                heightMap[z * Width + x] = Mathfs.Floor(Noise.BaseHeight + Noise.ScaleHeight * ((noiseHeight + 2) / 4));
            }

        //terrainMap and vertices
        for (int z = 0; z < Width; z++)
            for (int x = 0; x < Width; x++)
            {
                int index = z * Width + x;
                int noiseH = Mathfs.FloorToInt(heightMap[index]);

                int y = 0;
                do { _terrainMap[x + Width * y + Width * Height * z] = VoxelType.Error404; y++; } while (y < 1);
                do { _terrainMap[x + Width * y + Width * Height * z] = VoxelType.Stone; y++; } while (y < noiseH * .7f);
                do { _terrainMap[x + Width * y + Width * Height * z] = VoxelType.Dirt; y++; } while (y < noiseH * .9f);
                do { _terrainMap[x + Width * y + Width * Height * z] = VoxelType.Grass; y++; } while (y < noiseH);
                do { _terrainMap[x + Width * y + Width * Height * z] = VoxelType.Nothing; y++; } while (y < Height);
            }

        heightMap.Dispose();

        chunkData.CreateMesh(_terrainMap.ToArray(), VertexPosition, QuadFacePosition);
    }
    public void DebugDotLog()
    {
        chunkData.SetMesh();
    }

    private void OnDestroy()
    {
        _terrainMap.Dispose();
    }
}
