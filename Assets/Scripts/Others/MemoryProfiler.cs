using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Profiling;
using UnityEngine;

public class MemoryProfiler : MonoBehaviour
{
    string _statsText;
    ProfilerRecorder _totalReservedMemoryRecorder;
    ProfilerRecorder _gcReservedMemoryRecorder;
    ProfilerRecorder _textureMemoryRecorder;
    ProfilerRecorder _meshMemoryRecorder;
    void OnEnable()
    {
        _totalReservedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Reserved Memory");
        _gcReservedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory");
        _textureMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Texture Memory");
        _meshMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "MeshRender Memory");
    }
    void OnDisable()
    {
        _totalReservedMemoryRecorder.Dispose();
        _gcReservedMemoryRecorder.Dispose();
        _textureMemoryRecorder.Dispose();
        _meshMemoryRecorder.Dispose();
    }
    void Update()
    {
        var sb = new StringBuilder(500);
        if (_totalReservedMemoryRecorder.Valid)
            sb.AppendLine($"Total Reserved Memory: {_totalReservedMemoryRecorder.LastValue / (1024 * 1024)} MB");
        if (_gcReservedMemoryRecorder.Valid)
            sb.AppendLine($"GC Reserved Memory: {_gcReservedMemoryRecorder.LastValue / (1024 * 1024)} MB");
        if (_textureMemoryRecorder.Valid)
            sb.AppendLine($"Texture Used Memory: {_textureMemoryRecorder.LastValue / (1024 * 1024)}  MB");
        if (_meshMemoryRecorder.Valid)
            sb.AppendLine($"MeshRender Used Memory: {_meshMemoryRecorder.LastValue / (1024 * 1024)}  MB");
        _statsText = sb.ToString();
    }
    void OnGUI()
    {
        GUI.TextArea(new Rect(10, 30, 250, 70), _statsText);
    }
    private void OnDestroy()
    {
        _totalReservedMemoryRecorder.Dispose();
        _gcReservedMemoryRecorder.Dispose();
        _textureMemoryRecorder.Dispose();
        _meshMemoryRecorder.Dispose();

        Resources.UnloadUnusedAssets();
        GC.Collect();
    }
}
