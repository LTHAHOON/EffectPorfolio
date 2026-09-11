using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace EffectPortfolio.Wind.Editor
{
    public static class WindShaderGraphGenerator
    {
        private const string GraphPath = "Assets/FX/FX_Aknal/FX/Shadergraph/SG_Wind.shadergraph";
        private const string MaterialPath = "Assets/FX/FX_Aknal/FX/Material/M_Wind_Sample.mat";
        private const string DefaultTexturePath = "Assets/FX/FX_Aknal/FX/Texture/Wave_Wind.tga";

        private static readonly BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly BindingFlags AnyStatic = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        [InitializeOnLoadMethod]
        private static void ScheduleGeneration()
        {
            EditorApplication.delayCall += () =>
            {
                if (File.Exists(GraphPath) && File.ReadAllText(GraphPath).Contains("_WindStrength"))
                    return;

                try
                {
                    Generate();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            };
        }

        [MenuItem("Tools/Effect Portfolio/Generate Wind Shader Graph")]
        public static void Generate()
        {
            Type graphType = FindType("UnityEditor.ShaderGraph.GraphData");
            Type targetType = FindType("UnityEditor.ShaderGraph.Target");
            Type universalTargetType = FindType("UnityEditor.Rendering.Universal.ShaderGraph.UniversalTarget");
            Type unlitSubTargetType = FindType("UnityEditor.Rendering.Universal.ShaderGraph.UniversalUnlitSubTarget");
            Type blockDescriptorType = FindType("UnityEditor.ShaderGraph.BlockFieldDescriptor");

            object graph = Create(graphType);
            Invoke(graph, "AddContexts");

            object target = Create(universalTargetType);
            Invoke(target, "TrySetActiveSubTarget", unlitSubTargetType);
            SetEnum(target, "surfaceType", "Transparent");
            SetEnum(target, "alphaMode", "Alpha");
            SetEnum(target, "renderFace", "Both");
            SetEnum(target, "zWriteControl", "ForceDisabled");
            Set(target, "castShadows", false);
            Set(target, "receiveShadows", false);

            Array targets = Array.CreateInstance(targetType, 1);
            targets.SetValue(target, 0);

            object[] descriptorValues =
            {
                StaticField("UnityEditor.ShaderGraph.BlockFields+VertexDescription", "Position"),
                StaticField("UnityEditor.ShaderGraph.BlockFields+SurfaceDescription", "BaseColor"),
                StaticField("UnityEditor.ShaderGraph.BlockFields+SurfaceDescription", "Alpha")
            };
            Array descriptors = Array.CreateInstance(blockDescriptorType, descriptorValues.Length);
            for (int i = 0; i < descriptorValues.Length; i++)
                descriptors.SetValue(descriptorValues[i], i);
            Invoke(graph, "InitializeOutputs", targets, descriptors);

            object mainTexture = CreateProperty(graph, "UnityEditor.ShaderGraph.Internal.Texture2DShaderProperty", "Main Texture", "_Main_Tex");
            object tiling = CreateVector2Property(graph, "Main Texture Tiling", "_Main_Tex_Tiling", new Vector2(1f, 1f));
            object scrollSpeed = CreateVector2Property(graph, "Scroll Speed", "_ScrollSpeed", new Vector2(0.35f, 0.08f));
            object tint = CreateProperty(graph, "UnityEditor.ShaderGraph.Internal.ColorShaderProperty", "Tint", "_TintColor");
            Set(tint, "value", new Color(0.5f, 0.95f, 1f, 1f));
            object brightness = CreateFloatProperty(graph, "Brightness", "_Brightness", 2f, 0f, 10f);
            object alpha = CreateFloatProperty(graph, "Alpha", "_Alpha", 1f, 0f, 1f);

            object direction = CreateVector2Property(graph, "Wind Direction", "_WindDirection", new Vector2(1f, 0.25f));
            object speed = CreateFloatProperty(graph, "Wind Speed", "_WindSpeed", 1.6f, -5f, 5f);
            object frequency = CreateFloatProperty(graph, "Wind Frequency", "_WindFrequency", 2.4f, 0.01f, 12f);
            object strength = CreateFloatProperty(graph, "Wind Strength", "_WindStrength", 0.18f, 0f, 2f);
            object gustStrength = CreateFloatProperty(graph, "Gust Strength", "_GustStrength", 0.12f, 0f, 1f);
            object rootFalloff = CreateFloatProperty(graph, "Root Falloff", "_RootFalloff", 1.5f, 0.1f, 8f);

            object mainTextureNode = PropertyNode(graph, mainTexture, R(-1550, -470));
            object tilingNode = PropertyNode(graph, tiling, R(-1550, -390));
            object scrollNode = PropertyNode(graph, scrollSpeed, R(-1550, -310));
            object tintNode = PropertyNode(graph, tint, R(-690, -500));
            object brightnessNode = PropertyNode(graph, brightness, R(-690, -420));
            object alphaNode = PropertyNode(graph, alpha, R(-420, -250));

            object directionNode = PropertyNode(graph, direction, R(-1700, 150));
            object speedNode = PropertyNode(graph, speed, R(-1700, 240));
            object frequencyNode = PropertyNode(graph, frequency, R(-1450, 420));
            object strengthNode = PropertyNode(graph, strength, R(-650, 500));
            object gustStrengthNode = PropertyNode(graph, gustStrength, R(-650, 700));
            object rootFalloffNode = PropertyNode(graph, rootFalloff, R(-900, 920));

            object timeFragment = Node(graph, "UnityEditor.ShaderGraph.TimeNode", R(-1550, -220));
            object scrollMultiply = Node(graph, "UnityEditor.ShaderGraph.MultiplyNode", R(-1280, -280));
            object uv = Node(graph, "UnityEditor.ShaderGraph.UVNode", R(-1280, -470));
            object tilingOffset = Node(graph, "UnityEditor.ShaderGraph.TilingAndOffsetNode", R(-1040, -410));
            object sample = Node(graph, "UnityEditor.ShaderGraph.SampleTexture2DNode", new Rect(-790, -500, 210, 270));
            object colorMultiply = Node(graph, "UnityEditor.ShaderGraph.MultiplyNode", R(-420, -500));
            object brightnessMultiply = Node(graph, "UnityEditor.ShaderGraph.MultiplyNode", R(-150, -470));
            object alphaMultiply = Node(graph, "UnityEditor.ShaderGraph.MultiplyNode", R(-150, -250));

            Connect(graph, timeFragment, 0, scrollMultiply, 0);
            Connect(graph, scrollNode, 0, scrollMultiply, 1);
            Connect(graph, uv, 0, tilingOffset, 0);
            Connect(graph, tilingNode, 0, tilingOffset, 1);
            Connect(graph, scrollMultiply, 2, tilingOffset, 2);
            Connect(graph, mainTextureNode, 0, sample, 1);
            Connect(graph, tilingOffset, 3, sample, 0);
            Connect(graph, sample, 0, colorMultiply, 0);
            Connect(graph, tintNode, 0, colorMultiply, 1);
            Connect(graph, colorMultiply, 2, brightnessMultiply, 0);
            Connect(graph, brightnessNode, 0, brightnessMultiply, 1);
            Connect(graph, sample, 7, alphaMultiply, 0);
            Connect(graph, alphaNode, 0, alphaMultiply, 1);

            object position = Node(graph, "UnityEditor.ShaderGraph.PositionNode", R(-1700, 430));
            SetEnum(position, "m_Space", "Object");
            object positionSplit = Node(graph, "UnityEditor.ShaderGraph.SplitNode", R(-1490, 520));
            object normalizeDirection = Node(graph, "UnityEditor.ShaderGraph.NormalizeNode", R(-1450, 150));
            object directionSplit = Node(graph, "UnityEditor.ShaderGraph.SplitNode", R(-1240, 160));
            object phaseX = Node(graph, "UnityEditor.ShaderGraph.MultiplyNode", R(-1200, 400));
            object phaseZ = Node(graph, "UnityEditor.ShaderGraph.MultiplyNode", R(-1200, 540));
            object phaseAdd = Node(graph, "UnityEditor.ShaderGraph.AddNode", R(-980, 460));
            object phaseFrequency = Node(graph, "UnityEditor.ShaderGraph.MultiplyNode", R(-760, 390));
            object timeVertex = Node(graph, "UnityEditor.ShaderGraph.TimeNode", R(-1450, 290));
            object timeSpeed = Node(graph, "UnityEditor.ShaderGraph.MultiplyNode", R(-1200, 280));
            object mainPhase = Node(graph, "UnityEditor.ShaderGraph.AddNode", R(-520, 350));
            object mainWave = Node(graph, "UnityEditor.ShaderGraph.SineNode", R(-300, 340));

            Connect(graph, position, 0, positionSplit, 0);
            Connect(graph, directionNode, 0, normalizeDirection, 0);
            Connect(graph, normalizeDirection, 1, directionSplit, 0);
            Connect(graph, positionSplit, 1, phaseX, 0);
            Connect(graph, directionSplit, 1, phaseX, 1);
            Connect(graph, positionSplit, 3, phaseZ, 0);
            Connect(graph, directionSplit, 2, phaseZ, 1);
            Connect(graph, phaseX, 2, phaseAdd, 0);
            Connect(graph, phaseZ, 2, phaseAdd, 1);
            Connect(graph, phaseAdd, 2, phaseFrequency, 0);
            Connect(graph, frequencyNode, 0, phaseFrequency, 1);
            Connect(graph, timeVertex, 0, timeSpeed, 0);
            Connect(graph, speedNode, 0, timeSpeed, 1);
            Connect(graph, phaseFrequency, 2, mainPhase, 0);
            Connect(graph, timeSpeed, 2, mainPhase, 1);
            Connect(graph, mainPhase, 2, mainWave, 0);

            object secondaryPhaseScale = FloatNode(graph, 1.73f, R(-760, 560));
            object secondaryTimeScale = FloatNode(graph, 1.37f, R(-760, 700));
            object secondaryAmount = FloatNode(graph, 0.35f, R(-80, 520));
            object phaseSecondaryMultiply = Node(graph, "UnityEditor.ShaderGraph.MultiplyNode", R(-520, 560));
            object timeSecondaryMultiply = Node(graph, "UnityEditor.ShaderGraph.MultiplyNode", R(-520, 700));
            object secondaryPhase = Node(graph, "UnityEditor.ShaderGraph.AddNode", R(-300, 620));
            object secondaryWave = Node(graph, "UnityEditor.ShaderGraph.SineNode", R(-80, 640));
            object secondaryScale = Node(graph, "UnityEditor.ShaderGraph.MultiplyNode", R(140, 610));
            object waveAdd = Node(graph, "UnityEditor.ShaderGraph.AddNode", R(370, 420));

            Connect(graph, phaseFrequency, 2, phaseSecondaryMultiply, 0);
            Connect(graph, secondaryPhaseScale, 0, phaseSecondaryMultiply, 1);
            Connect(graph, timeSpeed, 2, timeSecondaryMultiply, 0);
            Connect(graph, secondaryTimeScale, 0, timeSecondaryMultiply, 1);
            Connect(graph, phaseSecondaryMultiply, 2, secondaryPhase, 0);
            Connect(graph, timeSecondaryMultiply, 2, secondaryPhase, 1);
            Connect(graph, secondaryPhase, 2, secondaryWave, 0);
            Connect(graph, secondaryWave, 1, secondaryScale, 0);
            Connect(graph, secondaryAmount, 0, secondaryScale, 1);
            Connect(graph, mainWave, 1, waveAdd, 0);
            Connect(graph, secondaryScale, 2, waveAdd, 1);

            object gustPhaseScale = FloatNode(graph, 0.35f, R(-300, 800));
            object gustTimeScale = FloatNode(graph, 0.23f, R(-300, 930));
            object halfA = FloatNode(graph, 0.5f, R(370, 780));
            object halfB = FloatNode(graph, 0.5f, R(370, 900));
            object gustPhaseMultiply = Node(graph, "UnityEditor.ShaderGraph.MultiplyNode", R(-80, 800));
            object gustTimeMultiply = Node(graph, "UnityEditor.ShaderGraph.MultiplyNode", R(-80, 930));
            object gustPhase = Node(graph, "UnityEditor.ShaderGraph.AddNode", R(140, 850));
            object gustSine = Node(graph, "UnityEditor.ShaderGraph.SineNode", R(370, 850));
            object gustHalf = Node(graph, "UnityEditor.ShaderGraph.MultiplyNode", R(590, 800));
            object gustRemap = Node(graph, "UnityEditor.ShaderGraph.AddNode", R(810, 840));
            object gustAmount = Node(graph, "UnityEditor.ShaderGraph.MultiplyNode", R(1030, 800));
            object strengthWithGust = Node(graph, "UnityEditor.ShaderGraph.AddNode", R(1250, 660));
            object waveStrength = Node(graph, "UnityEditor.ShaderGraph.MultiplyNode", R(1470, 470));

            Connect(graph, phaseFrequency, 2, gustPhaseMultiply, 0);
            Connect(graph, gustPhaseScale, 0, gustPhaseMultiply, 1);
            Connect(graph, timeSpeed, 2, gustTimeMultiply, 0);
            Connect(graph, gustTimeScale, 0, gustTimeMultiply, 1);
            Connect(graph, gustPhaseMultiply, 2, gustPhase, 0);
            Connect(graph, gustTimeMultiply, 2, gustPhase, 1);
            Connect(graph, gustPhase, 2, gustSine, 0);
            Connect(graph, gustSine, 1, gustHalf, 0);
            Connect(graph, halfA, 0, gustHalf, 1);
            Connect(graph, gustHalf, 2, gustRemap, 0);
            Connect(graph, halfB, 0, gustRemap, 1);
            Connect(graph, gustRemap, 2, gustAmount, 0);
            Connect(graph, gustStrengthNode, 0, gustAmount, 1);
            Connect(graph, strengthNode, 0, strengthWithGust, 0);
            Connect(graph, gustAmount, 2, strengthWithGust, 1);
            Connect(graph, waveAdd, 2, waveStrength, 0);
            Connect(graph, strengthWithGust, 2, waveStrength, 1);

            object uvVertex = Node(graph, "UnityEditor.ShaderGraph.UVNode", R(600, 1050));
            object uvSplit = Node(graph, "UnityEditor.ShaderGraph.SplitNode", R(810, 1040));
            object rootPower = Node(graph, "UnityEditor.ShaderGraph.PowerNode", R(1030, 1020));
            object maskedAmount = Node(graph, "UnityEditor.ShaderGraph.MultiplyNode", R(1690, 530));
            Connect(graph, uvVertex, 0, uvSplit, 0);
            Connect(graph, uvSplit, 2, rootPower, 0);
            Connect(graph, rootFalloffNode, 0, rootPower, 1);
            Connect(graph, waveStrength, 2, maskedAmount, 0);
            Connect(graph, rootPower, 2, maskedAmount, 1);

            object verticalAmount = FloatNode(graph, 0.12f, R(1690, 720));
            object offsetX = Node(graph, "UnityEditor.ShaderGraph.MultiplyNode", R(1910, 420));
            object offsetY = Node(graph, "UnityEditor.ShaderGraph.MultiplyNode", R(1910, 580));
            object offsetZ = Node(graph, "UnityEditor.ShaderGraph.MultiplyNode", R(1910, 740));
            object offsetVector = Node(graph, "UnityEditor.ShaderGraph.Vector3Node", R(2140, 540));
            object displacedPosition = Node(graph, "UnityEditor.ShaderGraph.AddNode", R(2380, 470));

            Connect(graph, maskedAmount, 2, offsetX, 0);
            Connect(graph, directionSplit, 1, offsetX, 1);
            Connect(graph, maskedAmount, 2, offsetY, 0);
            Connect(graph, verticalAmount, 0, offsetY, 1);
            Connect(graph, maskedAmount, 2, offsetZ, 0);
            Connect(graph, directionSplit, 2, offsetZ, 1);
            Connect(graph, offsetX, 2, offsetVector, 1);
            Connect(graph, offsetY, 2, offsetVector, 2);
            Connect(graph, offsetZ, 2, offsetVector, 3);
            Connect(graph, position, 0, displacedPosition, 0);
            Connect(graph, offsetVector, 0, displacedPosition, 1);

            object vertexBlock = FindBlock(graph, "VertexDescription.Position");
            object baseColorBlock = FindBlock(graph, "SurfaceDescription.BaseColor");
            object alphaBlock = FindBlock(graph, "SurfaceDescription.Alpha");
            Connect(graph, displacedPosition, 2, vertexBlock, 0);
            Connect(graph, brightnessMultiply, 2, baseColorBlock, 0);
            Connect(graph, alphaMultiply, 2, alphaBlock, 0);

            Invoke(graph, "ValidateGraph");
            Type fileUtilities = FindType("UnityEditor.ShaderGraph.FileUtilities");
            MethodInfo writer = fileUtilities.GetMethod("WriteShaderGraphToDisk", AnyStatic);
            writer.Invoke(null, new[] { GraphPath, graph });
            AssetDatabase.ImportAsset(GraphPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            CreateOrUpdateMaterial();
            AssetDatabase.SaveAssets();
            Debug.Log("[SG_Wind] Generated layered wind, gust, root mask, and scrolling texture graph.");
        }

        private static void CreateOrUpdateMaterial()
        {
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(GraphPath);
            if (shader == null)
                throw new InvalidOperationException("SG_Wind failed to import.");

            Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, MaterialPath);
            }
            else
                material.shader = shader;

            material.SetTexture("_Main_Tex", AssetDatabase.LoadAssetAtPath<Texture2D>(DefaultTexturePath));
            material.SetVector("_Main_Tex_Tiling", new Vector4(1f, 1f, 0f, 0f));
            material.SetVector("_ScrollSpeed", new Vector4(0.35f, 0.08f, 0f, 0f));
            material.SetColor("_TintColor", new Color(0.5f, 0.95f, 1f, 1f));
            material.SetFloat("_Brightness", 2f);
            material.SetFloat("_Alpha", 0.75f);
            material.SetVector("_WindDirection", new Vector4(1f, 0.25f, 0f, 0f));
            material.SetFloat("_WindSpeed", 1.6f);
            material.SetFloat("_WindFrequency", 2.4f);
            material.SetFloat("_WindStrength", 0.18f);
            material.SetFloat("_GustStrength", 0.12f);
            material.SetFloat("_RootFalloff", 1.5f);
            EditorUtility.SetDirty(material);
        }

        private static Rect R(float x, float y) => new Rect(x, y, 180, 100);

        private static object FloatNode(object graph, float value, Rect rect)
        {
            object node = Create(FindType("UnityEditor.ShaderGraph.Vector1Node"));
            Set(node, "m_Value", value);
            Invoke(node, "UpdateNodeAfterDeserialization");
            SetDrawState(node, rect);
            Invoke(graph, "AddNode", node);
            return node;
        }

        private static object CreateProperty(object graph, string typeName, string displayName, string referenceName)
        {
            object property = Create(FindType(typeName));
            Set(property, "displayName", displayName);
            Set(property, "overrideReferenceName", referenceName);
            Set(property, "generatePropertyBlock", true);
            Invoke(graph, "AddGraphInput", property, -1);
            return property;
        }

        private static object CreateFloatProperty(object graph, string displayName, string referenceName, float value, float min, float max)
        {
            object property = CreateProperty(graph, "UnityEditor.ShaderGraph.Internal.Vector1ShaderProperty", displayName, referenceName);
            SetEnum(property, "floatType", "Slider");
            Set(property, "rangeValues", new Vector2(min, max));
            Set(property, "value", value);
            return property;
        }

        private static object CreateVector2Property(object graph, string displayName, string referenceName, Vector2 value)
        {
            object property = CreateProperty(graph, "UnityEditor.ShaderGraph.Internal.Vector2ShaderProperty", displayName, referenceName);
            Set(property, "value", new Vector4(value.x, value.y, 0f, 0f));
            return property;
        }

        private static object PropertyNode(object graph, object property, Rect rect)
        {
            object node = Node(graph, "UnityEditor.ShaderGraph.PropertyNode", rect);
            Set(node, "property", property);
            return node;
        }

        private static object Node(object graph, string typeName, Rect rect)
        {
            object node = Create(FindType(typeName));
            SetDrawState(node, rect);
            Invoke(graph, "AddNode", node);
            return node;
        }

        private static void Connect(object graph, object outputNode, int outputSlot, object inputNode, int inputSlot)
        {
            Invoke(graph, "Connect", Invoke(outputNode, "GetSlotReference", outputSlot), Invoke(inputNode, "GetSlotReference", inputSlot));
        }

        private static object FindBlock(object graph, string name)
        {
            Type blockType = FindType("UnityEditor.ShaderGraph.BlockNode");
            MethodInfo getNodes = graph.GetType().GetMethods(AnyInstance)
                .First(method => method.Name == "GetNodes" && method.IsGenericMethodDefinition && method.GetParameters().Length == 0)
                .MakeGenericMethod(blockType);
            foreach (object block in (IEnumerable)getNodes.Invoke(graph, null))
            {
                if ((string)Get(block, "name") == name)
                    return block;
            }
            throw new InvalidOperationException($"Missing block: {name}");
        }

        private static void SetDrawState(object node, Rect rect)
        {
            PropertyInfo property = FindProperty(node.GetType(), "drawState");
            object state = property.GetValue(node);
            Set(state, "position", rect);
            Set(state, "expanded", true);
            property.SetValue(node, state);
        }

        private static Type FindType(string fullName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(fullName, false);
                if (type != null)
                    return type;
            }
            throw new TypeLoadException(fullName);
        }

        private static object Create(Type type) => Activator.CreateInstance(type, true);

        private static object StaticField(string typeName, string fieldName)
        {
            FieldInfo field = FindType(typeName).GetField(fieldName, AnyStatic);
            return field.GetValue(null);
        }

        private static object Get(object target, string name)
        {
            PropertyInfo property = FindProperty(target.GetType(), name);
            if (property != null)
                return property.GetValue(target);
            FieldInfo field = FindField(target.GetType(), name);
            if (field != null)
                return field.GetValue(target);
            throw new MissingMemberException(target.GetType().FullName, name);
        }

        private static void Set(object target, string name, object value)
        {
            PropertyInfo property = FindProperty(target.GetType(), name);
            if (property != null)
            {
                property.SetValue(target, value);
                return;
            }
            FieldInfo field = FindField(target.GetType(), name);
            if (field != null)
            {
                field.SetValue(target, value);
                return;
            }
            throw new MissingMemberException(target.GetType().FullName, name);
        }

        private static void SetEnum(object target, string name, string value)
        {
            PropertyInfo property = FindProperty(target.GetType(), name);
            if (property != null)
            {
                property.SetValue(target, Enum.Parse(property.PropertyType, value));
                return;
            }
            FieldInfo field = FindField(target.GetType(), name);
            if (field != null)
            {
                field.SetValue(target, Enum.Parse(field.FieldType, value));
                return;
            }
            throw new MissingMemberException(target.GetType().FullName, name);
        }

        private static PropertyInfo FindProperty(Type type, string name)
        {
            for (Type current = type; current != null; current = current.BaseType)
            {
                PropertyInfo property = current.GetProperty(name, AnyInstance | BindingFlags.DeclaredOnly);
                if (property != null)
                    return property;
            }
            return null;
        }

        private static FieldInfo FindField(Type type, string name)
        {
            for (Type current = type; current != null; current = current.BaseType)
            {
                FieldInfo field = current.GetField(name, AnyInstance | BindingFlags.DeclaredOnly);
                if (field != null)
                    return field;
            }
            return null;
        }

        private static object Invoke(object target, string name, params object[] arguments)
        {
            MethodInfo method = target.GetType().GetMethods(AnyInstance)
                .Where(candidate => candidate.Name == name)
                .FirstOrDefault(candidate => ParametersMatch(candidate.GetParameters(), arguments));
            if (method == null)
                throw new MissingMethodException(target.GetType().FullName, name);
            return method.Invoke(target, arguments);
        }

        private static bool ParametersMatch(ParameterInfo[] parameters, object[] arguments)
        {
            if (parameters.Length != arguments.Length)
                return false;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (arguments[i] != null && !parameters[i].ParameterType.IsInstanceOfType(arguments[i]))
                    return false;
            }
            return true;
        }
    }
}
