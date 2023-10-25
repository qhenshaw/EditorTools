using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;
using System;

namespace EditorTools.Editor
{
    public class MaterialCreator
    {
        private static string _uberBakedSimpleShaderName = "VFS/UberBakedSimple";
        private static string _uberBakedShaderName = "VFS/UberBaked";
        private static string _uberTiledShaderName = "VFS/UberTiled";

        private static string[] _texurePatterns = new[] { "basecolor", "normaldx", "roughaometal" };
        private static string[] _textureNames = new[] { "Base Color", "Normal Map", "Mask Map" };
        private static string[] _parameterNames = new[] { "_MainTex", "_Normal", "_Mask" };

        [MenuItem("Assets/Create/Art Pipeline/Create UberBakedSimple material", false, 1)]
        public static void CreateUberBakedSimpleMaterial()
        {
            CreateUberMaterial(_uberBakedSimpleShaderName);
        }

        [MenuItem("Assets/Create/Art Pipeline/Create UberBaked material", false, 2)]
        public static void CreateUberBakedMaterial()
        {
            CreateUberMaterial(_uberBakedShaderName);
        }

        [MenuItem("Assets/Create/Art Pipeline/Create UberTiled material", false, 2)]
        public static void CreateUberTiledMaterial()
        {
            CreateUberMaterial(_uberTiledShaderName);
        }

        private static void CreateUberMaterial(string shaderName)
        {
            Texture[] selectedTextures = Selection.GetFiltered<Texture>(SelectionMode.Assets);

            if (selectedTextures.Length != 3)
            {
                Debug.LogWarning("Select 3 PBR maps before creating material.");
                return;
            }

            Texture[] textures = new Texture[3];
            for (int i = 0; i < _texurePatterns.Length; i++)
            {
                for (int j = 0; j < selectedTextures.Length; j++)
                {
                    if (selectedTextures[j].name.ToLower().Contains(_texurePatterns[i])) textures[i] = selectedTextures[j];
                }
            }

            string output = $"Textures found:{Environment.NewLine}";
            for (int i = 0; i < _textureNames.Length; i++)
            {
                string result = "Failed";
                if (textures[i] != null)
                {
                    result = textures[i].name;
                    TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(textures[i]));
                    switch (i)
                    {
                        case 0:
                            importer.textureType = TextureImporterType.Default;
                            importer.sRGBTexture = true;
                            break;
                        case 1:
                            importer.textureType = TextureImporterType.NormalMap;
                            break;
                        case 2:
                            importer.textureType = TextureImporterType.Default;
                            importer.sRGBTexture = false;
                            break;
                    }

                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(textures[i]), ImportAssetOptions.ForceUpdate);
                }
                output += $"{_textureNames[i]}: {result}{Environment.NewLine}";
            }

            Debug.Log(output);

            for (int i = 0; i < textures.Length; i++)
            {
                if (textures[i] == null)
                {
                    Debug.LogWarning("Select 3 PBR maps before creating material.");
                    return;
                }
            }

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            Regex pathPattern = new Regex(@".+[/]");
            path = pathPattern.Match(path).Value;

            Regex fileNamePattern = new Regex(@".+[_]");
            string fileName = fileNamePattern.Match(Selection.activeObject.name).Value.Replace("_", "");

            Material material = new Material(Shader.Find(shaderName));
            AssetDatabase.CreateAsset(material, $"{path}/{fileName}.mat");

            for (int i = 0; i < _parameterNames.Length; i++)
            {
                material.SetTexture(_parameterNames[i], textures[i]);
            }

            Debug.Log($"Material created: {material}", material);
        }
    }
}