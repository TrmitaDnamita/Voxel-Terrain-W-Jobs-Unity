using UnityEditor;

//This basically enables and disables the capacity to edit the scriptable objects on the inspector
[CustomEditor(typeof(WorldHandler))]
public class ScriptEditor : Editor
{
    WorldHandler builder;

    Editor WorldData;
    Editor HumanData;
    Editor VoxelData;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        DrawSettingsEditor(builder._data,ref builder._dataActive, ref WorldData);
//        DrawSettingsEditor(builder.Player,ref builder.HumanDataState, ref HumanData);
//        DrawSettingsEditor(builder.VerticesData, ref builder.VerticesDataState, ref VoxelData);
    }

    private void DrawSettingsEditor(UnityEngine.Object settings,ref bool State, ref Editor editor)
    {
        if(settings != null)
        {
            using var check = new EditorGUI.ChangeCheckScope();
            State = EditorGUILayout.InspectorTitlebar(State, settings);
            if (State)
            {
                CreateCachedEditor(settings, null, ref editor);
                editor.OnInspectorGUI();
            }
        }
    }
    private void OnEnable()
    {
        builder = (WorldHandler)target;
    }
}
