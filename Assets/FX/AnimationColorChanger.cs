using UnityEngine;
using UnityEditor;
using System.IO;

public class AnimationColorChanger : EditorWindow
{
    private AnimationClip targetClip;
    private Color newRGBColor = Color.white;

    [MenuItem("Tools/애니메이션 RGB 일괄 치환기")]
    public static void ShowWindow()
    {
        GetWindow<AnimationColorChanger>("Anim RGB Changer");
    }

    void OnGUI()
    {
        GUILayout.Label("애니메이션 파일(.anim) 내부의 RGB 키값만 통째로 바꿉니다.", EditorStyles.boldLabel);
        GUILayout.Label("(기존에 작업하신 알파 프레임/곡선은 완벽히 유지됩니다)", EditorStyles.miniLabel);
        EditorGUILayout.Space();

        // 1. 수정할 애니메이션 클립 등록
        targetClip = (AnimationClip)EditorGUILayout.ObjectField("대상 애니메이션 클립", targetClip, typeof(AnimationClip), false);
        
        // 2. 변경할 RGB 색상 선택
        newRGBColor = EditorGUILayout.ColorField(new GUIContent("변경할 RGB 색상"), newRGBColor, false, false, false);
        
        EditorGUILayout.Space();

        if (GUILayout.Button("애니메이션 내부 RGB 일괄 치환", GUILayout.Height(40)))
        {
            if (targetClip == null)
            {
                EditorUtility.DisplayDialog("경고", "애니메이션 클립(.anim)을 넣어주세요!", "확인");
                return;
            }

            ModifyAnimationRGB();
        }
    }

    private void ModifyAnimationRGB()
    {
        // 되돌리기(Ctrl+Z) 등록
        Undo.RegisterCompleteObjectUndo(targetClip, "Modify Anim RGB");

        // 애니메이션 클립에 등록된 모든 프로퍼티 경로(바인딩) 가져오기
        EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(targetClip);
        int modifiedCurves = 0;

        foreach (var binding in bindings)
        {
            // 머티리얼 컬러를 제어하는 프로퍼티인지 확인 (_Color 또는 _BaseColor 등)
            bool isR = binding.propertyName.EndsWith(".r");
            bool isG = binding.propertyName.EndsWith(".g");
            bool isB = binding.propertyName.EndsWith(".b");

            if (isR || isG || isB)
            {
                // 기존 애니메이션 곡선(Curve) 추출
                AnimationCurve curve = AnimationCurveEditorUtility.GetAnimCurve(targetClip, binding);
                if (curve == null) continue;

                // 새롭게 덮어씌울 target 색상값 결정
                float targetValue = 0f;
                if (isR) targetValue = newRGBColor.r;
                if (isG) targetValue = newRGBColor.g;
                if (isB) targetValue = newRGBColor.b;

                // 기존 곡선의 모든 키프레임을 돌며 시간(Time)과 탄젠트(곡률)는 유지하고 '값(Value)'만 타겟 RGB로 고정
                Keyframe[] keys = curve.keys;
                for (int i = 0; i < keys.Length; i++)
                {
                    keys[i].value = targetValue;
                }

                // 수정된 키프레임들로 새로운 곡선 생성 및 애니메이션 클립에 재적용
                AnimationCurve newCurve = new AnimationCurve(keys);
                AnimationUtility.SetEditorCurve(targetClip, binding, newCurve);
                modifiedCurves++;
            }
        }

        // 변경사항 파일에 강제 저장 및 에디터 갱신
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("완료", $"기존 알파 곡선은 유지하고, {modifiedCurves}개의 RGB 애니메이션 곡선 데이터를 변경했습니다!", "확인");
    }
}

// 구버전 및 신버전 유니티 호환성을 위한 헬퍼 클래스
public static class AnimationCurveEditorUtility
{
    public static AnimationCurve GetAnimCurve(AnimationClip clip, EditorCurveBinding binding)
    {
        return AnimationUtility.GetEditorCurve(clip, binding);
    }
}