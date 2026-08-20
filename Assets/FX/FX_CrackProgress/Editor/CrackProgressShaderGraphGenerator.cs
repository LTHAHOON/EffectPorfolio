using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace EffectPortfolio.CrackProgress.Editor
{
    public static class CrackProgressShaderGraphGenerator
    {
        private const string Root = "Assets/FX/FX_CrackProgress";
        private const string GraphPath = Root + "/SG_CrackProgress.shadergraph";
        private const string TexturePath = Root + "/T_CrackProgress_Sample.png";
        private const string MaterialPath = Root + "/M_CrackProgress_Sample.mat";

        private static readonly BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly BindingFlags AnyStatic = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        [MenuItem("Tools/Effect Portfolio/Generate Branching Crack Shader Graph")]
        public static void Generate()
        {
            Directory.CreateDirectory(Root);
            GenerateMaskTexture();
            GenerateShaderGraph();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ConfigureMaskImporter();
            CreateOrUpdateMaterial();
            AssetDatabase.SaveAssets();
            Debug.Log($"[CrackProgress] Generated Shader Graph package at {Root}");
        }

        private static void GenerateShaderGraph()
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

            object maskProperty = CreateProperty(
                graph,
                "UnityEditor.ShaderGraph.Internal.Texture2DShaderProperty",
                "Crack Progress Mask (R=Crack G=Progress)",
                "_CrackProgressMask");
            object progressProperty = CreateFloatProperty(graph, "Progress", "_Progress", 0f, 0f, 1f);
            object softnessProperty = CreateFloatProperty(graph, "Edge Softness", "_EdgeSoftness", 0.035f, 0.001f, 0.25f);
            object colorProperty = CreateProperty(
                graph,
                "UnityEditor.ShaderGraph.Internal.ColorShaderProperty",
                "Crack Color",
                "_CrackColor");
            Set(colorProperty, "value", new Color(1f, 0.12f, 0.015f, 1f));
            object emissionProperty = CreateFloatProperty(graph, "Emission", "_Emission", 7f, 0f, 20f);

            object maskNode = CreatePropertyNode(graph, maskProperty, new Rect(-1050, -20, 180, 34));
            object progressNode = CreatePropertyNode(graph, progressProperty, new Rect(-780, 360, 160, 34));
            object softnessNode = CreatePropertyNode(graph, softnessProperty, new Rect(-1030, 220, 160, 34));
            object colorNode = CreatePropertyNode(graph, colorProperty, new Rect(-510, -260, 160, 34));
            object emissionNode = CreatePropertyNode(graph, emissionProperty, new Rect(-510, -150, 160, 34));

            object sampleNode = CreateNode(graph, "UnityEditor.ShaderGraph.SampleTexture2DNode", new Rect(-790, -40, 210, 270));
            object subtractNode = CreateNode(graph, "UnityEditor.ShaderGraph.SubtractNode", new Rect(-510, 120, 190, 120));
            object smoothstepNode = CreateNode(graph, "UnityEditor.ShaderGraph.SmoothstepNode", new Rect(-250, 100, 190, 150));
            object alphaMultiplyNode = CreateNode(graph, "UnityEditor.ShaderGraph.MultiplyNode", new Rect(20, 40, 190, 120));
            object colorMultiplyNode = CreateNode(graph, "UnityEditor.ShaderGraph.MultiplyNode", new Rect(-220, -250, 190, 120));

            Connect(graph, maskNode, 0, sampleNode, 1);
            Connect(graph, sampleNode, 5, subtractNode, 0);
            Connect(graph, softnessNode, 0, subtractNode, 1);
            Connect(graph, subtractNode, 2, smoothstepNode, 0);
            Connect(graph, sampleNode, 5, smoothstepNode, 1);
            Connect(graph, progressNode, 0, smoothstepNode, 2);
            Connect(graph, sampleNode, 4, alphaMultiplyNode, 0);
            Connect(graph, smoothstepNode, 3, alphaMultiplyNode, 1);
            Connect(graph, colorNode, 0, colorMultiplyNode, 0);
            Connect(graph, emissionNode, 0, colorMultiplyNode, 1);

            object baseColorBlock = FindBlock(graph, "SurfaceDescription.BaseColor");
            object alphaBlock = FindBlock(graph, "SurfaceDescription.Alpha");
            Connect(graph, colorMultiplyNode, 2, baseColorBlock, 0);
            Connect(graph, alphaMultiplyNode, 2, alphaBlock, 0);

            Invoke(graph, "ValidateGraph");

            Type fileUtilitiesType = FindType("UnityEditor.ShaderGraph.FileUtilities");
            MethodInfo writer = fileUtilitiesType.GetMethod("WriteShaderGraphToDisk", AnyStatic);
            if (writer == null)
                throw new MissingMethodException(fileUtilitiesType.FullName, "WriteShaderGraphToDisk");
            writer.Invoke(null, new[] { GraphPath, graph });
            AssetDatabase.ImportAsset(GraphPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
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
            object property = CreateProperty(
                graph,
                "UnityEditor.ShaderGraph.Internal.Vector1ShaderProperty",
                displayName,
                referenceName);
            SetEnum(property, "floatType", "Slider");
            Set(property, "rangeValues", new Vector2(min, max));
            Set(property, "value", value);
            return property;
        }

        private static object CreatePropertyNode(object graph, object property, Rect rect)
        {
            object node = CreateNode(graph, "UnityEditor.ShaderGraph.PropertyNode", rect);
            Set(node, "property", property);
            return node;
        }

        private static object CreateNode(object graph, string typeName, Rect rect)
        {
            object node = Create(FindType(typeName));
            SetDrawState(node, rect);
            Invoke(graph, "AddNode", node);
            return node;
        }

        private static void Connect(object graph, object outputNode, int outputSlot, object inputNode, int inputSlot)
        {
            object from = Invoke(outputNode, "GetSlotReference", outputSlot);
            object to = Invoke(inputNode, "GetSlotReference", inputSlot);
            Invoke(graph, "Connect", from, to);
        }

        private static object FindBlock(object graph, string blockName)
        {
            Type blockType = FindType("UnityEditor.ShaderGraph.BlockNode");
            MethodInfo getNodes = graph.GetType().GetMethods(AnyInstance)
                .First(method => method.Name == "GetNodes" && method.IsGenericMethodDefinition && method.GetParameters().Length == 0)
                .MakeGenericMethod(blockType);

            foreach (object block in (IEnumerable)getNodes.Invoke(graph, null))
            {
                string name = (string)Get(block, "name");
                if (name == blockName)
                    return block;
            }

            throw new InvalidOperationException($"Could not find Shader Graph block '{blockName}'.");
        }

        private static void SetDrawState(object node, Rect rect)
        {
            PropertyInfo property = FindProperty(node.GetType(), "drawState");
            if (property == null)
                return;

            object state = property.GetValue(node);
            Set(state, "position", rect);
            Set(state, "expanded", true);
            property.SetValue(node, state);
        }

        private static void GenerateMaskTexture()
        {
            const int size = 512;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
            Color32[] pixels = Enumerable.Repeat(new Color32(0, 255, 0, 255), size * size).ToArray();

            var segments = new List<CrackSegment>
            {
                S(0.50f, 0.08f, 0.50f, 0.28f, 0.00f, 0.18f, 4.2f),
                S(0.50f, 0.28f, 0.44f, 0.43f, 0.18f, 0.36f, 3.6f),
                S(0.44f, 0.43f, 0.27f, 0.55f, 0.36f, 0.62f, 3.2f),
                S(0.27f, 0.55f, 0.08f, 0.59f, 0.62f, 1.00f, 2.4f),
                S(0.44f, 0.43f, 0.51f, 0.62f, 0.36f, 0.58f, 3.3f),
                S(0.51f, 0.62f, 0.45f, 0.78f, 0.58f, 0.78f, 2.7f),
                S(0.45f, 0.78f, 0.53f, 0.94f, 0.78f, 1.00f, 2.0f),
                S(0.51f, 0.62f, 0.68f, 0.74f, 0.58f, 0.78f, 2.8f),
                S(0.68f, 0.74f, 0.78f, 0.93f, 0.78f, 1.00f, 2.0f),
                S(0.50f, 0.28f, 0.61f, 0.38f, 0.18f, 0.36f, 3.3f),
                S(0.61f, 0.38f, 0.79f, 0.33f, 0.36f, 0.67f, 2.7f),
                S(0.79f, 0.33f, 0.94f, 0.43f, 0.67f, 1.00f, 2.0f),
                S(0.61f, 0.38f, 0.71f, 0.54f, 0.36f, 0.62f, 2.8f),
                S(0.71f, 0.54f, 0.91f, 0.61f, 0.62f, 1.00f, 2.0f)
            };

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 point = new Vector2(x + 0.5f, y + 0.5f);
                    float bestDistance = float.MaxValue;
                    float bestProgress = 1f;
                    float bestWidth = 1f;

                    foreach (CrackSegment segment in segments)
                    {
                        float t;
                        float distance = DistanceToSegment(point, segment.start * size, segment.end * size, out t);
                        if (distance >= bestDistance)
                            continue;

                        bestDistance = distance;
                        bestProgress = Mathf.Lerp(segment.startProgress, segment.endProgress, t);
                        bestWidth = segment.width;
                    }

                    float feather = Mathf.InverseLerp(bestWidth, bestWidth + 1.5f, bestDistance);
                    float crack = 1f - feather * feather * (3f - 2f * feather);
                    if (crack <= 0f)
                        continue;

                    byte r = (byte)Mathf.RoundToInt(crack * 255f);
                    byte g = (byte)Mathf.RoundToInt(Mathf.Clamp01(bestProgress) * 255f);
                    pixels[y * size + x] = new Color32(r, g, 0, 255);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            File.WriteAllBytes(TexturePath, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(TexturePath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        }

        private static void ConfigureMaskImporter()
        {
            if (AssetImporter.GetAtPath(TexturePath) is not TextureImporter importer)
                return;

            importer.sRGBTexture = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        private static void CreateOrUpdateMaterial()
        {
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(GraphPath);
            if (shader == null)
                throw new InvalidOperationException($"Shader Graph failed to import: {GraphPath}");

            Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, MaterialPath);
            }
            else
            {
                material.shader = shader;
            }

            material.SetTexture("_CrackProgressMask", AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath));
            material.SetFloat("_Progress", 0.45f);
            material.SetFloat("_EdgeSoftness", 0.035f);
            material.SetColor("_CrackColor", new Color(1f, 0.12f, 0.015f, 1f));
            material.SetFloat("_Emission", 7f);
            EditorUtility.SetDirty(material);
        }

        private static CrackSegment S(float ax, float ay, float bx, float by, float start, float end, float width)
        {
            return new CrackSegment(new Vector2(ax, ay), new Vector2(bx, by), start, end, width);
        }

        private static float DistanceToSegment(Vector2 point, Vector2 start, Vector2 end, out float t)
        {
            Vector2 delta = end - start;
            float lengthSquared = delta.sqrMagnitude;
            t = lengthSquared > 0f ? Mathf.Clamp01(Vector2.Dot(point - start, delta) / lengthSquared) : 0f;
            return Vector2.Distance(point, start + delta * t);
        }

        private static Type FindType(string fullName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(fullName, false);
                if (type != null)
                    return type;
            }

            throw new TypeLoadException($"Could not find type '{fullName}'.");
        }

        private static object Create(Type type)
        {
            return Activator.CreateInstance(type, true);
        }

        private static object StaticField(string typeName, string fieldName)
        {
            FieldInfo field = FindType(typeName).GetField(fieldName, AnyStatic);
            if (field == null)
                throw new MissingFieldException(typeName, fieldName);
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
            if (property == null)
                throw new MissingMemberException(target.GetType().FullName, name);
            property.SetValue(target, Enum.Parse(property.PropertyType, value));
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
                if (arguments[i] == null)
                    continue;
                if (!parameters[i].ParameterType.IsInstanceOfType(arguments[i]))
                    return false;
            }

            return true;
        }

        private readonly struct CrackSegment
        {
            public readonly Vector2 start;
            public readonly Vector2 end;
            public readonly float startProgress;
            public readonly float endProgress;
            public readonly float width;

            public CrackSegment(Vector2 start, Vector2 end, float startProgress, float endProgress, float width)
            {
                this.start = start;
                this.end = end;
                this.startProgress = startProgress;
                this.endProgress = endProgress;
                this.width = width;
            }
        }
    }
}
