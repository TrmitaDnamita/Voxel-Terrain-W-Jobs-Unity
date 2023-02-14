using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class WorldHandler : MonoBehaviour
{
    public Data _data;
    public bool _dataActive;

    Dictionary<Vector2Int, ChunkRenderer> chunkRenderers = new ();

    NativeArray<float3> VerticesPosition;
    NativeArray<int3> QuadFaces;

    private void Awake()
    {
        _data.NoiseData.SetSeed(_data.Seed);
        Set_auxiliar_voxelData();

        float startTime = Time.realtimeSinceStartup;

        GenerateMapJob();

        float time = Time.realtimeSinceStartup - startTime;
        Debug.Log(string.Format("Jobs: {0} | Total: {1}ms", _data.jobs, time * 1000f));
    }

    private ChunkRenderer GenerateGameObject(Vector2Int coord, NativeList<JobHandle> handles)
    {
        GameObject Chunk = new(string.Format("{0}", coord), typeof(ChunkRenderer));
        ChunkRenderer chunkRenderer = Chunk.GetComponent<ChunkRenderer>();

        Chunk.transform.parent = transform;
        Chunk.transform.position = new(coord.x * _data._Width, 0, coord.y * _data._Width);

        handles.Add(chunkRenderer.DataInit(_data, VerticesPosition, QuadFaces, true));

        return chunkRenderer;
    }
    private ChunkRenderer GenerateGameObject(Vector2Int coord)
    {
        GameObject Chunk = new(string.Format("{0}", coord), typeof(ChunkRenderer));
        ChunkRenderer chunkRenderer = Chunk.GetComponent<ChunkRenderer>();

        Chunk.transform.parent = transform;
        Chunk.transform.position = new(coord.x * _data._Width, 0, coord.y * _data._Width);
        Chunk.GetComponent<MeshCollider>().enabled = false;

        chunkRenderer.DataInit(_data, VerticesPosition, QuadFaces);

        return chunkRenderer;
    }
    public void GenerateMapJob()
    {
        int start = -_data.RenderDistance;
        int end = _data.RenderDistance + 1;

        if (_data.jobs)
        {
            NativeList<JobHandle> handles = new(Allocator.Persistent);

            for (int i = start; i < end; i++)
                for (int j = start; j < end; j++)
                {
                    Vector2Int coord = new(i, j);
                    chunkRenderers.Add(coord, GenerateGameObject(coord, handles));
                }

            JobHandle.CompleteAll(handles); handles.Dispose();

            for (int i = start; i < end; i++)
                for (int j = start; j < end; j++)
                    chunkRenderers[new(i,j)].DebugDotLog();
        }
        else
        {
            for (int i = start; i < end; i++)
                for (int j = start; j < end; j++)
                {
                    Vector2Int coord = new(i, j);
                    chunkRenderers.Add(coord, GenerateGameObject(coord));
                }

            for (int i = start; i < end; i++)
                for (int j = start; j < end; j++)
                    chunkRenderers[new(i, j)].DebugDotLog();
        }
    }

    public void Set_auxiliar_voxelData()
    {
        _data.NoiseData.NormlizedBlockTexture = 1f / _data.NoiseData.AtlasTextureInBlocks;

        VerticesPosition = new(24, Allocator.Persistent);
        //FrontFace
        VerticesPosition[0] = new float3( 1, 1, 1);
        VerticesPosition[1] = new float3( 1,-1, 1);
        VerticesPosition[2] = new float3(-1,-1, 1);
        VerticesPosition[3] = new float3(-1, 1, 1);
        //BackFace
        VerticesPosition[4] = new float3(-1, 1,-1);
        VerticesPosition[5] = new float3(-1,-1,-1);
        VerticesPosition[6] = new float3( 1,-1,-1);
        VerticesPosition[7] = new float3( 1, 1,-1);
        //TopFace
        VerticesPosition[8] = new float3(-1, 1, 1);
        VerticesPosition[9] = new float3(-1, 1,-1);
        VerticesPosition[10] = new float3( 1, 1,-1);
        VerticesPosition[11] = new float3( 1, 1, 1);
        //BottomFace
        VerticesPosition[12] = new float3(-1,-1, 1);
        VerticesPosition[13] = new float3(-1,-1,-1);
        VerticesPosition[14] = new float3( 1,-1,-1);
        VerticesPosition[15] = new float3( 1,-1, 1);
        //RightFace
        VerticesPosition[16] = new float3( 1, 1,-1);
        VerticesPosition[17] = new float3( 1,-1,-1);
        VerticesPosition[18] = new float3( 1,-1, 1);
        VerticesPosition[19] = new float3( 1, 1, 1);
        //LeftFace
        VerticesPosition[20] = new float3(-1, 1, 1);
        VerticesPosition[21] = new float3(-1,-1, 1);
        VerticesPosition[22] = new float3(-1,-1,-1);
        VerticesPosition[23] = new float3(-1, 1,-1);

        //--------------------------------------------
        QuadFaces = new(6, Allocator.Persistent);

        QuadFaces[0] = new int3( 0, 0, 1); //Front
        QuadFaces[1] = new int3( 0, 0,-1); //Back
        QuadFaces[2] = new int3( 0, 1, 0); //Top
        QuadFaces[3] = new int3( 0,-1, 0); //Bottom
        QuadFaces[4] = new int3( 1, 0, 0); //Right
        QuadFaces[5] = new int3(-1, 0, 0); //Left
    }
    private void OnDestroy()
    {
        chunkRenderers.Clear();

        VerticesPosition.Dispose();
        QuadFaces.Dispose();

        Resources.UnloadUnusedAssets();
        EditorUtility.UnloadUnusedAssetsImmediate();
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

}

//float startTime = Time.realtimeSinceStartup;
//float time = Time.realtimeSinceStartup - startTime;
//Debug.Log(string.Format("Jobs: {0} | Total: {1}ms", UseJob, time * 1000f));