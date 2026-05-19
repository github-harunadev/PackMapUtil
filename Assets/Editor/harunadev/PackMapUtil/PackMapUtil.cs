#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public enum Channel
{
    R = 0,
    G = 1,
    B = 2,
    A = 3,
    external = 4,
}

public enum ChannelAdditional
{
    R = 0,
    G = 1,
    B = 2,
    A = 3,
    zero = 4,
    one = 5,
}

public enum ChannelNoExternal
{
    R = 0,
    G = 1,
    B = 2,
    A = 3,
}

public class PackMapUtil : EditorWindow
{
    public const string version = "1.1";

    [MenuItem("harunadev/PackMap Util")]
    public static void ShowWindow()
    {
        var window = GetWindow<PackMapUtil>("PackMap Util");

        if (Selection.objects.Length > 1)
        {
            foreach (Object o in Selection.objects)
            {
                if (o is Texture2D)
                {
                    window.sourceTextures.Add(o as Texture2D);
                }
            }

            window.singleMode = false;
        }
        else
        {
            window.sourceTexture = Selection.activeObject as Texture2D;

            window.singleMode = true;
        }

        window.Show();
        window.Initialize();
    }

    public List<Texture2D> sourceTextures = new List<Texture2D>();
    public Texture2D sourceTexture;
    public Texture2D externalMetallicTexture;
    public ChannelAdditional externalMetallicChannel = ChannelAdditional.R;
    public Texture2D externalSmoothnessTexture;
    public ChannelAdditional externalSmoothnessChannel = ChannelAdditional.R;
    public bool externalSmoothnessIsRoughness = false;
    public Texture2D externalOcclusionTexture;
    public ChannelAdditional externalOcclusionChannel = ChannelAdditional.R;

    public string prefix = "";
    public string postfix = "";
    public string replaceSource = "";
    public string replaceTarget = "ORM";


    public Channel sourceMetallicChannel = Channel.B;
    public Channel sourceSmoothnessChannel = Channel.G;
    public Channel sourceOcclusionChannel = Channel.R;
    public bool sourceIsRoughness = false;
    public bool sourceIsSpecular = false;

    public Color targetBaseColor = Color.white;
    public ChannelNoExternal targetMetallicChannel = ChannelNoExternal.B;
    public ChannelNoExternal targetSmoothnessChannel = ChannelNoExternal.G;
    public ChannelNoExternal targetOcclusionChannel = ChannelNoExternal.R;
    public bool targetIsRoughness = true;


    Material PackMapMaterial;
    SerializedObject serializedObject;
    SerializedProperty _sourceTextures;
    bool singleMode = false;

    public void Initialize()
    {
        MonoScript script = MonoScript.FromScriptableObject(this);
        string scriptPath = AssetDatabase.GetAssetPath(script);
        string folderPath = Path.GetDirectoryName(scriptPath);
        string materialPath = Path.Combine(folderPath, "PackMapMaterial.mat");
        materialPath = materialPath.Replace("\\", "/");

        PackMapMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

        serializedObject = new SerializedObject(this);
        _sourceTextures = serializedObject.FindProperty("sourceTextures");

        string name = "";
        if (sourceTextures.Count > 0)
        {
            Texture2D tex = sourceTextures[0];
            name = tex.name;
        }
        else if (sourceTexture != null)
        {
            name = sourceTexture.name;

            UpdateName();
        }

        if (name.Contains("ORM"))
        {
            sourceIsRoughness = true;

            targetIsRoughness = false;
            targetMetallicChannel = ChannelNoExternal.R;
            targetSmoothnessChannel = ChannelNoExternal.A;
            targetOcclusionChannel = ChannelNoExternal.G;

            replaceSource = "ORM";
            replaceTarget = "PackMap";
        }
        else if (name.Contains("PackMap"))
        {
            sourceMetallicChannel = Channel.R;
            sourceSmoothnessChannel = Channel.A;
            sourceOcclusionChannel = Channel.G;

            replaceSource = "PackMap";
            replaceTarget = "ORM";
            prefix = "";
            postfix = "";
        }
        else if (name.Contains("MetallicSmoothness"))
        {
            sourceMetallicChannel = Channel.R;
            sourceSmoothnessChannel = Channel.A;
            sourceOcclusionChannel = Channel.external;
            externalOcclusionChannel = ChannelAdditional.one;

            replaceSource = "MetallicSmoothness";
            replaceTarget = "ORM";
            prefix = "";
            postfix = "";
        }
        else if (name.Contains("MS"))
        {
            sourceMetallicChannel = Channel.R;
            sourceSmoothnessChannel = Channel.A;
            sourceOcclusionChannel = Channel.external;
            externalOcclusionChannel = ChannelAdditional.one;

            replaceSource = "MS";
            replaceTarget = "ORM";
            prefix = "";
            postfix = "";
        }
        else if (name.Contains("Specular"))
        {
            sourceMetallicChannel = Channel.R;
            sourceSmoothnessChannel = Channel.A;
            sourceOcclusionChannel = Channel.external;
            externalOcclusionChannel = ChannelAdditional.one;

            replaceSource = "Specular";
            replaceTarget = "ORM";
            prefix = "";
            postfix = "";
        }
    }

    float preserveLabelWidth;
    string newName;
    void UpdateName()
    {
        if (sourceTexture != null)
        {
            newName = prefix + (string.IsNullOrEmpty(replaceSource) ? sourceTexture.name : sourceTexture.name.Replace(replaceSource, replaceTarget)) + postfix + ".png";
        } else
        {
            newName = "no source texture";
        }
    }

    private void OnGUI()
    {
        if (_sourceTextures == null)
        {
            Initialize();
        }

        preserveLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 250;

        GUILayout.Label($"PackMap Util v{version} by harunadev", EditorStyles.boldLabel);
        if (GUILayout.Button("GitHub"))
        {
            Application.OpenURL("https://github.com/github-harunadev/PackMapUtil");
        }

        EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

        singleMode = EditorGUILayout.Toggle("Single Mode", singleMode);

        EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

        if (singleMode)
        {
            EditorGUI.BeginChangeCheck();
            sourceTexture = (Texture2D)EditorGUILayout.ObjectField("Source Texture", sourceTexture, typeof(Texture2D), false);
            if (EditorGUI.EndChangeCheck())
            {
                UpdateName();
            }

            sourceIsSpecular = EditorGUILayout.Toggle("Source Is Specular", sourceIsSpecular);
            if (sourceIsSpecular)
            {
                EditorGUILayout.HelpBox("Specular => Metallic conversion is imperfect. Result may not be 100% exact equal to original.", MessageType.Warning);
            }
            else
            {
                sourceMetallicChannel = (Channel)EditorGUILayout.EnumPopup("Source Metallic Channel", sourceMetallicChannel);
                if (sourceMetallicChannel == Channel.external)
                {
                    externalMetallicTexture = (Texture2D)EditorGUILayout.ObjectField("External Metallic Texture", externalMetallicTexture, typeof(Texture2D), false);
                    externalMetallicChannel = (ChannelAdditional)EditorGUILayout.EnumPopup("External Metallic Channel", externalMetallicChannel);
                }
            }
            sourceSmoothnessChannel = (Channel)EditorGUILayout.EnumPopup("Source Smoothness Channel", sourceSmoothnessChannel);
            if (sourceSmoothnessChannel == Channel.external)
            {
                externalSmoothnessTexture = (Texture2D)EditorGUILayout.ObjectField("External Smoothness Texture", externalSmoothnessTexture, typeof(Texture2D), false);
                externalSmoothnessChannel = (ChannelAdditional)EditorGUILayout.EnumPopup("External Smoothness Channel", externalSmoothnessChannel);
                externalSmoothnessIsRoughness = EditorGUILayout.Toggle("External Smoothness Is Roughness", externalSmoothnessIsRoughness);
            }
            else
            {
                sourceIsRoughness = EditorGUILayout.Toggle("Source Smoothness Is Roughness", sourceIsRoughness);
            }
            sourceOcclusionChannel = (Channel)EditorGUILayout.EnumPopup("Source Occlusion Channel", sourceOcclusionChannel);
            if (sourceOcclusionChannel == Channel.external)
            {
                externalOcclusionTexture = (Texture2D)EditorGUILayout.ObjectField("External Occlusion Texture", externalOcclusionTexture, typeof(Texture2D), false);
                externalOcclusionChannel = (ChannelAdditional)EditorGUILayout.EnumPopup("External Occlusion Channel", externalOcclusionChannel);
            }
        }
        else
        {
            EditorGUILayout.PropertyField(_sourceTextures, true);

            sourceIsSpecular = EditorGUILayout.Toggle("Source Is Specular", sourceIsSpecular);
            if (sourceIsSpecular)
            {
                EditorGUILayout.HelpBox("Specular => Metallic conversion is imperfect. Result may not be 100% exact equal to original.", MessageType.Warning);
            }
            else
            {
                sourceMetallicChannel = (Channel)EditorGUILayout.EnumPopup("Source Metallic Channel", sourceMetallicChannel);
                if (sourceMetallicChannel == Channel.external)
                {
                    externalMetallicChannel = EditorGUILayout.Toggle("Metallic Fill 1", externalMetallicChannel == ChannelAdditional.one) ? ChannelAdditional.one : ChannelAdditional.zero;
                }
            }
            sourceSmoothnessChannel = (Channel)EditorGUILayout.EnumPopup("Source Smoothness Channel", sourceSmoothnessChannel);
            if (sourceSmoothnessChannel == Channel.external)
            {
                externalSmoothnessChannel = EditorGUILayout.Toggle("Smoothness Fill 1", externalSmoothnessChannel == ChannelAdditional.one) ? ChannelAdditional.one : ChannelAdditional.zero;
            }
            else
            {
                sourceIsRoughness = EditorGUILayout.Toggle("Source Smoothness Is Roughness", sourceIsRoughness);
            }
            sourceOcclusionChannel = (Channel)EditorGUILayout.EnumPopup("Source Occlusion Channel", sourceOcclusionChannel);
            if (sourceOcclusionChannel == Channel.external)
            {
                externalOcclusionChannel = EditorGUILayout.Toggle("Occlusion Fill 1", externalOcclusionChannel == ChannelAdditional.one) ? ChannelAdditional.one : ChannelAdditional.zero;
            }
        }


        EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

        targetBaseColor = EditorGUILayout.ColorField("Target Base Pixel Color", targetBaseColor);
        targetMetallicChannel = (ChannelNoExternal)EditorGUILayout.EnumPopup("Target Metallic Channel", targetMetallicChannel);
        targetSmoothnessChannel = (ChannelNoExternal)EditorGUILayout.EnumPopup("Target Smoothness Channel", targetSmoothnessChannel);
        targetOcclusionChannel = (ChannelNoExternal)EditorGUILayout.EnumPopup("Target Occlusion Channel", targetOcclusionChannel);
        targetIsRoughness = EditorGUILayout.Toggle("Target Is Roughness", targetIsRoughness);

        EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

        EditorGUI.BeginChangeCheck();
        prefix = EditorGUILayout.TextField("Prefix", prefix);
        postfix = EditorGUILayout.TextField("Postfix", postfix);
        replaceSource = EditorGUILayout.TextField("Replace Source", replaceSource);
        replaceTarget = EditorGUILayout.TextField("Replace Target", replaceTarget);
        if (EditorGUI.EndChangeCheck())
        {
            UpdateName();
        }
        EditorGUILayout.HelpBox("If replaceSource is not empty, it will be replaced with the value in replaceTarget.", MessageType.Info);

        EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

        if (singleMode)
        {
            if (sourceTexture)
            {
                if (GUILayout.Button($"Save PackMap as {newName}"))
                {
                    void process()
                    {
                        string tmp = AssetDatabase.GetAssetPath(sourceTexture).Replace("Assets/", "");
                        tmp = Path.Combine(Application.dataPath,
                            tmp.Substring(0, tmp.LastIndexOf('/')), newName);

                        ProcessSinglePackMap(
                            sourceTexture,
                            externalMetallicTexture, externalMetallicChannel,
                            externalSmoothnessTexture, externalSmoothnessChannel, externalSmoothnessIsRoughness,
                            externalOcclusionTexture, externalOcclusionChannel,
                            sourceMetallicChannel, sourceSmoothnessChannel, sourceOcclusionChannel, sourceIsRoughness, sourceIsSpecular,
                            targetBaseColor, targetMetallicChannel, targetSmoothnessChannel, targetOcclusionChannel, targetIsRoughness,
                            tmp
                        );

                        ResetMaterial();
                        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath(tmp, typeof(Texture2D)));
                        AssetDatabase.Refresh();
                    }

                    if (string.IsNullOrEmpty(prefix) && string.IsNullOrEmpty(postfix) && string.IsNullOrEmpty(replaceTarget))
                    {
                        if (EditorUtility.DisplayDialog("Overwrite Warning", "Prefix, postfix, and replace target are empty. This action will overwrite the existing texture file. Press Cancel to abort.", "Confirm", "Cancel"))
                        {
                            process();
                        }
                    } else
                    {
                        process();
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Please assign a source texture.", MessageType.Warning);
            }
        }
        else
        {
            if (sourceTextures.Count > 0)
            {
                if (GUILayout.Button("Save PackMaps"))
                {
                    void process()
                    {
                        foreach (var texture in sourceTextures)
                        {
                            string newName = prefix +
                                (string.IsNullOrEmpty(replaceSource)
                                    ? texture.name
                                    : texture.name.Replace(replaceSource, replaceTarget))
                                + postfix + ".png";
                            string tmp = AssetDatabase.GetAssetPath(texture).Replace('\\', '/').Replace("Assets/", "");

                            ProcessSinglePackMap(
                                texture,
                                null, externalMetallicChannel,
                                null, externalSmoothnessChannel, false,
                                null, externalOcclusionChannel,
                                sourceMetallicChannel, sourceSmoothnessChannel, sourceOcclusionChannel, sourceIsRoughness, sourceIsSpecular,
                                targetBaseColor, targetMetallicChannel, targetSmoothnessChannel, targetOcclusionChannel, targetIsRoughness,
                                Path.Combine(Application.dataPath, tmp.Substring(0, tmp.LastIndexOf('/')), newName)
                            );
                        }

                        ResetMaterial();
                        AssetDatabase.Refresh();
                    }
                    
                    if (string.IsNullOrEmpty(prefix) && string.IsNullOrEmpty(postfix) && string.IsNullOrEmpty(replaceTarget))
                    {
                        if (EditorUtility.DisplayDialog("Overwrite Warning", "Prefix, postfix, and replace target are empty. This action will overwrite existing texture files. Press Cancel to abort.", "Confirm", "Cancel"))
                        {
                            process();
                        }
                    }
                    else
                    {
                        process();
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Please add source textures to the list.", MessageType.Warning);
            }
        }

        serializedObject.ApplyModifiedProperties();

        EditorGUIUtility.labelWidth = preserveLabelWidth;
    }

    public void ProcessSinglePackMap(
        Texture2D source,
        Texture2D externalMetallic, ChannelAdditional externalMetallicChannel,
        Texture2D externalSmoothness, ChannelAdditional externalSmoothnessChannel, bool externalSmoothnessIsRoughness,
        Texture2D externalOcclusion, ChannelAdditional externalOcclusionChannel,
        Channel sourceMetallicChannel, Channel sourceSmoothnessChannel, Channel sourceOcclusionChannel, bool sourceIsRoughness, bool sourceIsSpecular,
        Color targetBaseColor, ChannelNoExternal targetMetallicChannel, ChannelNoExternal targetSmoothnessChannel, ChannelNoExternal targetOcclusionChannel, bool targetIsRoughness,
        string savePath
        )
    {
        RenderTexture prevactive = RenderTexture.active;

        RenderTexture renderTexture = new RenderTexture(source.width, source.height, 0, RenderTextureFormat.ARGB32);

        PackMapMaterial.SetTexture("_MainTex", source);
        PackMapMaterial.SetTexture("_ExternalMetallicMap", externalMetallic);
        PackMapMaterial.SetInt("_ExternalMetallicChannel", (int)externalMetallicChannel);
        PackMapMaterial.SetTexture("_ExternalSmoothnessMap", externalSmoothness);
        PackMapMaterial.SetInt("_ExternalSmoothnessChannel", (int)externalSmoothnessChannel);
        PackMapMaterial.SetFloat("_ExternalSmoothnessIsRoughness", externalSmoothnessIsRoughness ? 1f : 0f);
        PackMapMaterial.SetTexture("_ExternalOcclusionMap", externalOcclusion);
        PackMapMaterial.SetInt("_ExternalOcclusionChannel", (int)externalOcclusionChannel);

        PackMapMaterial.SetInt("_SourceMetallicChannel", (int)sourceMetallicChannel);
        PackMapMaterial.SetInt("_SourceSmoothnessChannel", (int)sourceSmoothnessChannel);
        PackMapMaterial.SetFloat("_SourceIsRoughness", sourceIsRoughness ? 1f : 0f);
        PackMapMaterial.SetFloat("_SourceIsSpecular", sourceIsSpecular ? 1f : 0f);
        PackMapMaterial.SetInt("_SourceOcclusionChannel", (int)sourceOcclusionChannel);

        PackMapMaterial.SetColor("_TargetBaseColor", targetBaseColor);
        PackMapMaterial.SetInt("_TargetMetallicChannel", (int)targetMetallicChannel);
        PackMapMaterial.SetInt("_TargetSmoothnessChannel", (int)targetSmoothnessChannel);
        PackMapMaterial.SetInt("_TargetOcclusionChannel", (int)targetOcclusionChannel);
        PackMapMaterial.SetFloat("_TargetIsRoughness", targetIsRoughness ? 1f : 0f);

        RenderTexture.active = renderTexture;
        Graphics.Blit(null, renderTexture, PackMapMaterial);

        Texture2D outputTexture = new Texture2D(source.width, source.height, TextureFormat.ARGB32, false);

        outputTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        outputTexture.Apply();

        byte[] rawPNG = outputTexture.EncodeToPNG();
        File.WriteAllBytes(savePath.Replace('\\', '/'), rawPNG);

        renderTexture.Release();
        RenderTexture.active = prevactive;
    }

    public void ResetMaterial()
    {
        PackMapMaterial.SetTexture("_MainTex", null);
        PackMapMaterial.SetTexture("_ExternalMetallicMap", null);
        PackMapMaterial.SetInt("_ExternalMetallicChannel", 0);
        PackMapMaterial.SetTexture("_ExternalSmoothnessMap", null);
        PackMapMaterial.SetInt("_ExternalSmoothnessChannel", 0);
        PackMapMaterial.SetFloat("_ExternalSmoothnessIsRoughness", 0f);
        PackMapMaterial.SetTexture("_ExternalOcclusionMap", null);
        PackMapMaterial.SetInt("_ExternalOcclusionChannel", 0);

        PackMapMaterial.SetInt("_SourceMetallicChannel", 0);
        PackMapMaterial.SetInt("_SourceSmoothnessChannel", 0);
        PackMapMaterial.SetFloat("_SourceIsRoughness", 0f);
        PackMapMaterial.SetFloat("_SourceIsSpecular", 0f);
        PackMapMaterial.SetInt("_SourceOcclusionChannel", 0);

        PackMapMaterial.SetColor("_TargetBaseColor", Color.white);
        PackMapMaterial.SetInt("_TargetMetallicChannel", 0);
        PackMapMaterial.SetInt("_TargetSmoothnessChannel", 0);
        PackMapMaterial.SetInt("_TargetOcclusionChannel", 0);
        PackMapMaterial.SetFloat("_TargetIsRoughness", 0f);
    }
}
#endif