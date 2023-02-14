using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "ChunkData", menuName = "Scriptable/ChunkData")]
public class ChunkData : ScriptableObject
{
    public NativeList<float3> _vertices;
    public NativeList<float3> _verticesCollider;
    public NativeList<int> _triangles;
    public NativeList<int> _trianglesCollider;
    public NativeList<float2> _uvs;

    //public NativeArray<float3> _normals;

    public Data Data { get; set; }
    public Vector3 Rcoords { get; set; }
    public Mesh Mesh { get; set; }
    public MeshRenderer MeshRenderer { get; set; }
    public MeshFilter MeshFilter { get; set; }
    public MeshCollider MeshCollider { get; set; }
    
    public void SetArrays()
    {
        _vertices = new NativeList<float3>(Allocator.Persistent);
        _verticesCollider = new NativeList<float3>(Allocator.Persistent);
        _triangles = new NativeList<int>(Allocator.Persistent);
        _trianglesCollider = new NativeList<int>(Allocator.Persistent);
        _uvs = new NativeList<float2>(Allocator.Persistent);
    }
    public void CreateMesh(VoxelType[] TerrainMap, NativeArray<float3> VertexData, NativeArray<int3> VertexFace)
    {
        int Width = Data._Width + 2; 
        int Height = Data._Height;
        int substract = - (Width) / 2;
        //Vertices and Triangles
        bool GenerateCollider = true; //I need to fix this later

        int3 PointIndex; int3 FaceCompare; int index;

        for (int z = 1; z < Width - 1; z++)
            for (int x = 1; x < Width - 1; x++)
                for (int y = 0; y < Height - 1; y++)
                {
                    index = x + Width * y + Width * Height * z;
                    VoxelType terrainType = TerrainMap[index];
                    if (terrainType != VoxelType.Nothing)
                    {
                        PointIndex = new(x, y, z);

                        if (y > 0)
                        {
                            //Front
                            FaceCompare = PointIndex + VertexFace[(int)VoxelFace.Front];
                            index = FaceCompare.x + Width * FaceCompare.y + Width * Height * FaceCompare.z;
                            if (TerrainMap[index] == VoxelType.Nothing)
                                FaceSet(TerrainMap, VoxelFace.Front, terrainType, PointIndex, index, substract, GenerateCollider, VertexData, VertexFace);

                            //Back
                            FaceCompare = PointIndex + VertexFace[(int)VoxelFace.Back];
                            index = FaceCompare.x + Width * FaceCompare.y + Width * Height * FaceCompare.z;
                            if (TerrainMap[index] == VoxelType.Nothing)
                                FaceSet(TerrainMap, VoxelFace.Back, terrainType, PointIndex, index, substract, GenerateCollider, VertexData, VertexFace);

                            //Down
                            FaceCompare = PointIndex + VertexFace[(int)VoxelFace.Down];
                            index = FaceCompare.x + Width * FaceCompare.y + Width * Height * FaceCompare.z;
                            if (TerrainMap[index] == VoxelType.Nothing)
                                FaceSet(TerrainMap, VoxelFace.Down, terrainType, PointIndex, index, substract, GenerateCollider, VertexData, VertexFace);

                            //Right
                            FaceCompare = PointIndex + VertexFace[(int)VoxelFace.Right];
                            index = FaceCompare.x + Width * FaceCompare.y + Width * Height * FaceCompare.z;
                            if (TerrainMap[index] == VoxelType.Nothing)
                                FaceSet(TerrainMap, VoxelFace.Right, terrainType, PointIndex, index, substract, GenerateCollider, VertexData, VertexFace);

                            //Left
                            FaceCompare = PointIndex + VertexFace[(int)VoxelFace.Left];
                            index = FaceCompare.x + Width * FaceCompare.y + Width * Height * FaceCompare.z;
                            if (TerrainMap[index] == VoxelType.Nothing)
                                FaceSet(TerrainMap, VoxelFace.Left, terrainType, PointIndex, index, substract, GenerateCollider, VertexData, VertexFace);
                        }

                        //UP
                        FaceCompare = PointIndex + VertexFace[(int)VoxelFace.Up];
                        index = FaceCompare.x + Width * FaceCompare.y + Width * Height * FaceCompare.z;
                        if (TerrainMap[index] == VoxelType.Nothing)
                            FaceSet(TerrainMap ,VoxelFace.Up, terrainType, PointIndex, index, substract, GenerateCollider, VertexData, VertexFace);
                    }
                }
    }
    void FaceSet(VoxelType[] TerrainMap, VoxelFace face, VoxelType type, int3 PointIndex, int index, int substract, bool GenerateCollider, NativeArray<float3> VertexData, NativeArray<int3> VertexFace)
    {
        if (TerrainMap[index] == VoxelType.Nothing)
        {
            //Set Vertices And UVs
            PointIndex.x += substract; PointIndex.z += substract;
            float3 aux = PointIndex;

            for (int v = 0; v < 4; v++)
            {
                aux += VertexData[v + (int)face * 4] * Data.VoxelScale;
                _vertices.Add(aux);

                if (GenerateCollider)
                    _verticesCollider.Add(aux);
                aux = PointIndex;
            }

            //Set Uvs
            _uvs.Add(new(0 + .001f, Data.NoiseData.NormlizedBlockTexture - .001f));                                      //(0, 1) 
            _uvs.Add(new(0 + .001f, 0 + .001f));                                                                         //(0, 0)
            _uvs.Add(new(Data.NoiseData.NormlizedBlockTexture - .001f, 0 + .001f));                                      //(1, 0)
            _uvs.Add(new(Data.NoiseData.NormlizedBlockTexture - .001f, Data.NoiseData.NormlizedBlockTexture - .001f));   //(1, 1)

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
    public void SetMesh()
    {
        Mesh = new()
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32,
            vertices = Data.ToArray(_vertices.ToArray()),
            triangles = _triangles.ToArray(),
            uv = Data.ToArray(_uvs.ToArray())
        };
        Mesh.RecalculateNormals();

        MeshFilter.mesh = Mesh;
        MeshRenderer.materials = Data._materials;

        MeshCollider.sharedMesh = null;
        Mesh collisionMesh = new()
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32,
            vertices = Data.ToArray(_verticesCollider.ToArray()),
            triangles = _trianglesCollider.ToArray()
        };
        collisionMesh.RecalculateNormals();

        MeshCollider.sharedMesh = collisionMesh;

        _vertices.Dispose();
        _verticesCollider.Dispose();
        _triangles.Dispose();
        _trianglesCollider.Dispose();
        _uvs.Dispose();
    }
}
