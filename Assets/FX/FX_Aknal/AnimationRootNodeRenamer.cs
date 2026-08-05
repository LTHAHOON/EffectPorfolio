using UnityEditor;
using UnityEngine;

public class AnimationRootNodeRenamer : EditorWindow
{
    private string _rootNodeName = "Aknal";

    [MenuItem("Tools/Animation/Root Node Renamer")]
    private static void Open()
    {
        GetWindow<AnimationRootNodeRenamer>(
            "Root Node Renamer"
        );
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField(
            "Animation Root Node Renamer",
            EditorStyles.boldLabel
        );

        EditorGUILayout.Space(10);

        _rootNodeName = EditorGUILayout.TextField(
            "New Root Node Name",
            _rootNodeName
        );

        EditorGUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "선택한 Animation Clip의 모든 Animation Curve 경로에서\n" +
            "첫 번째 Root Node를 지정한 이름으로 변경합니다.",
            MessageType.Info
        );

        EditorGUILayout.Space(10);

        GUI.enabled = !string.IsNullOrWhiteSpace(_rootNodeName);

        if (GUILayout.Button(
                "Rename Selected Animation Clips",
                GUILayout.Height(35)))
        {
            RenameSelectedClips();
        }

        GUI.enabled = true;
    }

    private void RenameSelectedClips()
    {
        Object[] selectedObjects = Selection.objects;

        int count = 0;

        foreach (Object obj in selectedObjects)
        {
            if (obj is not AnimationClip clip)
                continue;

            RenameRootNode(clip);

            count++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            $"Animation Root Node 변경 완료 : {count}개 Clip"
        );
    }

    private void RenameRootNode(
        AnimationClip clip)
    {
        Undo.RecordObject(
            clip,
            "Rename Animation Root Node"
        );

        RenameFloatCurves(clip);

        RenameObjectReferenceCurves(clip);

        EditorUtility.SetDirty(clip);
    }

    private void RenameFloatCurves(
        AnimationClip clip)
    {
        EditorCurveBinding[] bindings =
            AnimationUtility.GetCurveBindings(clip);

        foreach (EditorCurveBinding binding in bindings)
        {
            string newPath =
                ReplaceRootNode(
                    binding.path,
                    _rootNodeName
                );

            if (newPath == binding.path)
                continue;

            AnimationCurve curve =
                AnimationUtility.GetEditorCurve(
                    clip,
                    binding
                );

            // 기존 Curve 제거
            AnimationUtility.SetEditorCurve(
                clip,
                binding,
                null
            );

            // 새로운 Binding 생성
            EditorCurveBinding newBinding = binding;
            newBinding.path = newPath;

            // 새로운 경로에 Curve 추가
            AnimationUtility.SetEditorCurve(
                clip,
                newBinding,
                curve
            );
        }
    }

    private void RenameObjectReferenceCurves(
        AnimationClip clip)
    {
        EditorCurveBinding[] bindings =
            AnimationUtility
                .GetObjectReferenceCurveBindings(clip);

        foreach (EditorCurveBinding binding in bindings)
        {
            string newPath =
                ReplaceRootNode(
                    binding.path,
                    _rootNodeName
                );

            if (newPath == binding.path)
                continue;

            ObjectReferenceKeyframe[] keyframes =
                AnimationUtility.GetObjectReferenceCurve(
                    clip,
                    binding
                );

            // 기존 Binding 제거
            AnimationUtility.SetObjectReferenceCurve(
                clip,
                binding,
                null
            );

            // 새로운 Binding
            EditorCurveBinding newBinding = binding;

            newBinding.path = newPath;

            // 새로운 경로에 추가
            AnimationUtility.SetObjectReferenceCurve(
                clip,
                newBinding,
                keyframes
            );
        }
    }

    private string ReplaceRootNode(
        string path,
        string newRootName)
    {
        // Root 자체가 없는 경우
        if (string.IsNullOrEmpty(path))
        {
            return newRootName;
        }

        int slashIndex = path.IndexOf('/');

        // Root Node만 존재
        if (slashIndex == -1)
        {
            return newRootName;
        }

        // Root Node 이하의 경로
        string childPath =
            path.Substring(slashIndex + 1);

        return newRootName + "/" + childPath;
    }
}