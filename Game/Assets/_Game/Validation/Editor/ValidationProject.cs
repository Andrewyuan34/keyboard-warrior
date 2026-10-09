using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace KeyboardWarrior.Validation
{
    public static class ValidationProject
    {
        public const string ScenePath = "Assets/_Game/Scenes/Validation.unity";

        [MenuItem("Keyboard Warrior/Create validation scene")]
        public static void CreateScene()
        {
            Directory.CreateDirectory("Assets/_Game/Scenes");
            ConfigureSpriteFixture();
            Configure2DRenderer();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            // These are unchanged assets from the project template used for this bootstrap.
            AssetDatabase.DeleteAsset("Assets/TutorialInfo");
            AssetDatabase.DeleteAsset("Assets/Readme.asset");
            AssetDatabase.DeleteAsset("Assets/Scenes/SampleScene.unity");
            var root = new GameObject("Keyboard Warrior Validation");
            root.AddComponent<ValidationWorld>();
            EditorSceneManager.SaveScene(root.scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            EditorSettings.serializationMode = SerializationMode.ForceText;
            EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
            PlayerSettings.companyName = "VAAM";
            PlayerSettings.productName = "Keyboard Warrior - Unity Validation";
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.runInBackground = true;
            AssetDatabase.SaveAssets();
            Debug.Log("VALIDATION_SCENE_CREATED " + ScenePath);
        }

        private static void ConfigureSpriteFixture()
        {
            const string path = "Assets/_Game/Validation/Resources/Validation/White.png";
            if (!File.Exists(path))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                var fixture = new Texture2D(8, 8, TextureFormat.RGBA32, false);
                var pixels = new Color32[64];
                for (var i = 0; i < pixels.Length; i++) pixels[i] = new Color32(255, 255, 255, 255);
                fixture.SetPixels32(pixels);
                fixture.Apply();
                File.WriteAllBytes(path, fixture.EncodeToPNG());
                Object.DestroyImmediate(fixture);
            }
            // An existing unresolved LFS pointer is deliberately not regenerated or worked around.
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                throw new InvalidDataException("Validation PNG could not import. Fetch its Git LFS content: " + path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 8;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            if (AssetDatabase.LoadAssetAtPath<Sprite>(path) == null)
                throw new InvalidDataException("Validation PNG has no Sprite after import. Check Git LFS content: " + path);
        }

        private static void Configure2DRenderer()
        {
            const string settings = "Assets/_Game/Settings";
            Directory.CreateDirectory(settings);
            var renderer = AssetDatabase.LoadAssetAtPath<Renderer2DData>(settings + "/Validation2DRenderer.asset");
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance<Renderer2DData>();
                AssetDatabase.CreateAsset(renderer, settings + "/Validation2DRenderer.asset");
            }
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(settings + "/Validation2DPipeline.asset");
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(pipeline, settings + "/Validation2DPipeline.asset");
            }
            GraphicsSettings.defaultRenderPipeline = pipeline;
            var originalQuality = QualitySettings.GetQualityLevel();
            for (var i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = pipeline;
            }
            QualitySettings.SetQualityLevel(originalQuality, false);
        }
    }
}
