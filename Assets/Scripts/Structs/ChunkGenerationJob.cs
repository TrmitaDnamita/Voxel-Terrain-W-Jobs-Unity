using Freya;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public struct NoiseCalculation : IJob
{
    [ReadOnly] public byte Width;
    [ReadOnly] public int Height;
    [ReadOnly] public float VoxelScale;
    [ReadOnly] public float3 Coords;
    [ReadOnly] public NoiseValues Noise;
    [ReadOnly] public NativeArray<float3> VertexData;
    [ReadOnly] public NativeArray<int3> VertexFace;

    public NativeArray<VoxelType> TerrainMap;

    public NativeList<float3> _vertices;
    public NativeList<float3> _verticesCollider;
    public NativeList<int> _triangles;
    public NativeList<int> _trianglesCollider;
    public NativeList<float2> _uvs;
    
    public void Execute()
    {
        //NoiseData
        int size = Width * Width;

        NativeArray<float> heightMap = new(size, Allocator.Temp);

        for (int z = 0; z < Width; z++)
            for (int x = 0; x < Width; x++)
            {
                float amplitude = 1f;
                float frecuency = 1f;
                float noiseHeight = 0f;

                float X = (x + Coords.x) / Noise.Scale;
                float Z = (z + Coords.z) / Noise.Scale;

                for (int o = 0; o < Noise.Octaves; o++)
                {
                    noiseHeight += noise.snoise(new float2(X * frecuency, Z * frecuency)) * amplitude;

                    amplitude *= Noise.Persistance;
                    frecuency *= Noise.Lacunarity;
                }

                heightMap[z * Width + x] = Mathfs.Floor(Noise.BaseHeight + Noise.ScaleHeight * ((noiseHeight + 2) / 4));
            }
        int index;

        //terrainMap
        for (int z = 0; z < Width; z++)
            for (int x = 0; x < Width; x++)
            {
                index = z * Width + x;
                int noiseH = Mathfs.FloorToInt(heightMap[index]);

                int y = 0;
                do { TerrainMap[x + Width * y + Width * Height * z] = VoxelType.Error404; y++; } while (y < 1);
                do { TerrainMap[x + Width * y + Width * Height * z] = VoxelType.Stone; y++; } while (y < noiseH * .7f);
                do { TerrainMap[x + Width * y + Width * Height * z] = VoxelType.Dirt; y++; } while (y < noiseH * .9f);
                do { TerrainMap[x + Width * y + Width * Height * z] = VoxelType.Grass; y++; } while (y < noiseH);
                do { TerrainMap[x + Width * y + Width * Height * z] = VoxelType.Nothing; y++; } while (y < Height);
            }

        heightMap.Dispose();

        //------------------------------------------------------//

        //Vertices and Triangles

        int substract = -(Width) / 2;
        bool GenerateCollider = true; //I need to fix this later

        int3 PointIndex; int3 PointAux; int3 FaceCompare;

        for (int z = 1; z < Width - 1; z++)
            for (int x = 1; x < Width - 1; x++)
                for (int y = 1; y < Height - 1; y++)
                {
                    index = x + Width * y + Width * Height * z;
                    VoxelType terrainType = TerrainMap[index];
                    if (terrainType != VoxelType.Nothing)
                    {
                        PointAux = new(x, y, z);
                        //Faces For
                        for(VoxelFace face = VoxelFace.Front; face <= VoxelFace.Left; face++)
                        {
                            FaceCompare = PointAux + VertexFace[(int)face];
                            index = FaceCompare.x + Width * FaceCompare.y + Width * Height * FaceCompare.z;
                            if (TerrainMap[index] == VoxelType.Nothing)
                            {
                                //Set Vertices And UVs
                                PointIndex = PointAux;
                                PointIndex.x += substract; PointIndex.z += substract;
                                float3 aux = PointIndex;

                                for (int v = 0; v < 4; v++)
                                {
                                    aux += VertexData[v + (int)face * 4] * VoxelScale;
                                    _vertices.Add(aux);

                                    if (GenerateCollider)
                                        _verticesCollider.Add(aux);
                                    aux = PointIndex;
                                }

                                //Set Uvs
                                _uvs.Add(new(0 + .001f, 1 - .001f));                             //(0, 1) 
                                _uvs.Add(new(0 + .001f, 0 + .001f));                             //(0, 0)
                                _uvs.Add(new(1 - .001f, 0 + .001f));                             //(1, 0)
                                _uvs.Add(new(1 - .001f, 1 - .001f));                             //(1, 1)

                                //Set Quads
                                int Count = _vertices.Length;

                                _triangles.Add(Count - 4);
                                _triangles.Add(Count - 1);
                                _triangles.Add(Count - 3);
                                _triangles.Add(Count - 3);
                                _triangles.Add(Count - 1);
                                _triangles.Add(Count - 2);

                                if (GenerateCollider)
                                {
                                    Count = _verticesCollider.Length;

                                    _trianglesCollider.Add(Count - 4);
                                    _trianglesCollider.Add(Count - 1);
                                    _trianglesCollider.Add(Count - 3);
                                    _trianglesCollider.Add(Count - 3);
                                    _trianglesCollider.Add(Count - 1);
                                    _trianglesCollider.Add(Count - 2);
                                }
                            }
                        }
                    }
                }
    }
}