using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(VolumePropertyData))]
public class VolumePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position,
        SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position.height = EditorGUIUtility.singleLineHeight;

        if (property.propertyType == SerializedPropertyType.ManagedReference
            && property.managedReferenceValue == null)
        {
            var createPosition = EditorGUI.PrefixLabel(position, label);
            if (GUI.Button(createPosition, "Create"))
            {
                property.managedReferenceValue = new VolumePropertyData();
                GUI.changed = true;
            }

            EditorGUI.EndProperty();
            return;
        }

        property.isExpanded = EditorGUI.Foldout(
            position, property.isExpanded, label, true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            DrawField(ref position, property, "volumeType");
            DrawField(ref position, property, "propertyName");
            DrawField(ref position, property, "valueType");

            var valueTypeProperty = property.FindPropertyRelative("valueType");
            if (valueTypeProperty == null)
            {
                EditorGUI.indentLevel--;
                EditorGUI.EndProperty();
                return;
            }

            string valueName = GetValueFieldName(valueTypeProperty);

            if (valueName != null)
                DrawField(ref position, property, valueName);

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    private static void DrawField(
        ref Rect position,
        SerializedProperty property,
        string fieldName)
    {
        var field = property.FindPropertyRelative(fieldName);

        if (field == null)
            return;

        position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
        position.height = EditorGUI.GetPropertyHeight(field, true);
        EditorGUI.PropertyField(position, field, true);
    }

    public override float GetPropertyHeight(
        SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.ManagedReference
            && property.managedReferenceValue == null)
            return EditorGUIUtility.singleLineHeight;

        if (!property.isExpanded)
            return EditorGUIUtility.singleLineHeight;

        float height = EditorGUIUtility.singleLineHeight;
        AddFieldHeight(ref height, property, "volumeType");
        AddFieldHeight(ref height, property, "propertyName");
        AddFieldHeight(ref height, property, "valueType");

        var valueTypeProperty = property.FindPropertyRelative("valueType");
        if (valueTypeProperty != null)
        {
            string valueName = GetValueFieldName(valueTypeProperty);
            if (valueName != null)
                AddFieldHeight(ref height, property, valueName);
        }

        return height;
    }

    private static void AddFieldHeight(
        ref float height,
        SerializedProperty property,
        string fieldName)
    {
        var field = property.FindPropertyRelative(fieldName);
        if (field == null)
            return;

        height += EditorGUIUtility.standardVerticalSpacing
                  + EditorGUI.GetPropertyHeight(field, true);
    }

    private static string GetValueFieldName(SerializedProperty valueTypeProperty)
    {
        return (VolumeValueType)valueTypeProperty.enumValueIndex switch
        {
            VolumeValueType.Float => "floatValue",
            VolumeValueType.Int => "intValue",
            VolumeValueType.Bool => "boolValue",
            VolumeValueType.Color => "colorValue",
            VolumeValueType.Vector2 => "vector2Value",
            VolumeValueType.Vector3 => "vector3Value",
            VolumeValueType.Vector4 => "vector4Value",
            VolumeValueType.Texture => "textureValue",
            VolumeValueType.Enum => "intValue",
            _ => null
        };
    }
}