using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public enum GlobalVolumeType
{
    #region PostProcessing
    Bloom,
    Vignette,
    SplitToning,
    ChromaticAberration,
    ColorAdjustments,
    ColorCurves,
    ColorLookup,
    ChannelMixer,
    DepthOfField,
    FilmGrain,
    LensDistortion,
    LiftGammaGain,
    MotionBlur,
    PaniniProjection,
    ShadowsMidtonesHighlights,
    Tonemapping,
    WhiteBalance
    #endregion
}

public enum VolumeValueType
{
    #region ValueType
    Float,
    Int,
    Bool,
    Color,
    Vector2,
    Vector3,
    Vector4,
    Texture,
    Enum
    #endregion
}

[Serializable]
public class VolumePropertyData
{
    #region PostProcessing Data
    public GlobalVolumeType volumeType;
    public string propertyName;

    public VolumeValueType valueType;

    public float floatValue;
    public int intValue;
    public bool boolValue;
    public Color colorValue = Color.white;
    public Vector2 vector2Value;
    public Vector3 vector3Value;
    public Vector4 vector4Value;
    public Texture textureValue;
    #endregion
}

[ExecuteAlways]
public class GlobalVolumeController : MonoBehaviour
{
    [SerializeField] private Volume volume;

    [SerializeReference]
    private List<VolumePropertyData> properties = new();
    [SerializeField]
    private VolumeProfile _profile;

    private void OnEnable()
    {
        _profile = null;
        ApplyProperties();
    }

    private void LateUpdate()
    {
        ApplyProperties();
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.EditorApplication.delayCall -= ApplyProperties;
            UnityEditor.EditorApplication.delayCall += ApplyProperties;
        }
#endif
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall -= ApplyProperties;
#endif
    }

    public void ApplyProperties()
    {
        if (this == null || volume == null)
        {
            return;
        }

        _profile ??= volume.profile;

        if (_profile == null)
        {
            return;
        }

        foreach (var data in properties)
        {
            ApplyProperty(data);
        }
    }

    private void ApplyProperty(VolumePropertyData data)
    {
        if (data == null || string.IsNullOrEmpty(data.propertyName))
            return;

        Type componentType = GlobalVolumeUtility.GetVolumeType(data.volumeType);

        if (!_profile.TryGet(componentType, out VolumeComponent component))
        {
            return;
        }

        if (!component.active)
            component.active = true;

        FieldInfo field = componentType.GetField(
            data.propertyName,
            BindingFlags.Public | BindingFlags.Instance);

        if (field == null)
        {
            return;
        }

        if (field.GetValue(component) is not VolumeParameter parameter)
        {
            return;
        }

        object value = GetValue(data);

        PropertyInfo valueProperty = parameter.GetType().GetProperty("value", BindingFlags.Public | BindingFlags.Instance);

        if (valueProperty == null || !valueProperty.CanWrite)
        {
            return;
        }

        Type targetType = valueProperty.PropertyType;

        if (targetType.IsEnum)
        {
            if (data.valueType != VolumeValueType.Enum)
            {
                return;
            }

            value = Enum.ToObject(targetType, data.intValue);
        }
        else if (value == null ||
                 !targetType.IsInstanceOfType(value))
        {
            return;
        }

        parameter.overrideState = true;
        valueProperty.SetValue(parameter, value);
    }

    private static object GetValue(VolumePropertyData data)
    {
        return data.valueType switch
        {
            VolumeValueType.Float => data.floatValue,
            VolumeValueType.Int => data.intValue,
            VolumeValueType.Bool => data.boolValue,
            VolumeValueType.Color => data.colorValue,
            VolumeValueType.Vector2 => data.vector2Value,
            VolumeValueType.Vector3 => data.vector3Value,
            VolumeValueType.Vector4 => data.vector4Value,
            VolumeValueType.Texture => data.textureValue,
            VolumeValueType.Enum => data.intValue,
            _ => null
        };
    }

    public static class GlobalVolumeUtility
    {
        public static Type GetVolumeType(GlobalVolumeType type)
        {
            return type switch
            {
                GlobalVolumeType.Bloom => typeof(Bloom),
                GlobalVolumeType.Vignette => typeof(Vignette),
                GlobalVolumeType.SplitToning => typeof(SplitToning),
                GlobalVolumeType.ChromaticAberration => typeof(ChromaticAberration),
                GlobalVolumeType.ColorAdjustments => typeof(ColorAdjustments),
                GlobalVolumeType.ColorCurves => typeof(ColorCurves),
                GlobalVolumeType.ColorLookup => typeof(ColorLookup),
                GlobalVolumeType.ChannelMixer => typeof(ChannelMixer),
                GlobalVolumeType.DepthOfField => typeof(DepthOfField),
                GlobalVolumeType.FilmGrain => typeof(FilmGrain),
                GlobalVolumeType.LensDistortion => typeof(LensDistortion),
                GlobalVolumeType.LiftGammaGain => typeof(LiftGammaGain),
                GlobalVolumeType.MotionBlur => typeof(MotionBlur),
                GlobalVolumeType.PaniniProjection => typeof(PaniniProjection),
                GlobalVolumeType.ShadowsMidtonesHighlights => typeof(ShadowsMidtonesHighlights),
                GlobalVolumeType.Tonemapping => typeof(Tonemapping),
                GlobalVolumeType.WhiteBalance => typeof(WhiteBalance),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        public static VolumeComponent GetComponent(VolumeProfile profile, GlobalVolumeType type)
        {
            if (profile == null)
            {
                return null;
            }

            Type componentType = GetVolumeType(type);

            return profile.TryGet(componentType, out VolumeComponent component)? component : null;
        }
    }
}
