using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Imports Assets/Art/Player/Frames/*.png as sprites and wires them into Player.prefab.
public static class PlayerSpriteSetup
{
    private const string FramesFolder = "Assets/Art/Player/Frames";
    private const string PrefabPath = "Assets/Prefabs/Player/Player.prefab";
    private const float PixelsPerUnit = 66f;

    private static readonly (string name, float fps, bool loop)[] ClipSettings =
    {
        ("idle", 6f, true), ("run", 12f, true), ("jump", 1f, false), ("dash", 12f, false),
        ("shoot", 8f, false), ("parry", 12f, false), ("slash", 16f, false), ("charge", 6f, false),
        ("heavy", 12f, false), ("execute", 12f, false), ("hit", 8f, false), ("death", 8f, false)
    };

    [MenuItem("Tools/Player/Setup Sprites")]
    public static void Setup()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { FramesFolder });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;

            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
            settings.spritePivot = new Vector2(0.5f, 0f);
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            PlayerSpriteAnimator animator = root.GetComponent<PlayerSpriteAnimator>();
            if (animator == null)
            {
                animator = root.AddComponent<PlayerSpriteAnimator>();
            }

            SerializedObject so = new SerializedObject(animator);
            SerializedProperty clips = so.FindProperty("clips");
            clips.arraySize = ClipSettings.Length;
            for (int i = 0; i < ClipSettings.Length; i++)
            {
                SerializedProperty element = clips.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("name").stringValue = ClipSettings[i].name;
                element.FindPropertyRelative("fps").floatValue = ClipSettings[i].fps;
                element.FindPropertyRelative("loop").boolValue = ClipSettings[i].loop;

                List<Sprite> frames = new List<Sprite>();
                for (int f = 0; ; f++)
                {
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{FramesFolder}/{ClipSettings[i].name}_{f:00}.png");
                    if (sprite == null)
                    {
                        break;
                    }

                    frames.Add(sprite);
                }

                SerializedProperty frameArray = element.FindPropertyRelative("frames");
                frameArray.arraySize = frames.Count;
                for (int f = 0; f < frames.Count; f++)
                {
                    frameArray.GetArrayElementAtIndex(f).objectReferenceValue = frames[f];
                }
            }

            so.ApplyModifiedProperties();
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }

        Debug.Log("[PlayerSpriteSetup] Player sprites imported and wired");
    }
}
