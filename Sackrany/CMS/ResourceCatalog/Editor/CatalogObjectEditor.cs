using System;
using System.IO;

using Sackrany.CMS.ResourceCatalog.ScriptableObjects;

using UnityEditor;

using UnityEngine;

namespace Sackrany.CMS.ResourceCatalog.Editor
{
    [CustomEditor(typeof(CatalogObject))]
    public class CatalogObjectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var data = (CatalogObject)target;

            if (GUILayout.Button("Generate all"))
            {
                CatalogEnumGenerator.GenerateCatalogTypeEnums();
                ClearOldCatalog(data);
                GenerateNewCatalog(data);
                CatalogEnumGenerator.GenerateAll(data);
            
                string path = AssetDatabase.GetAssetPath(data);
                AssetDatabase.RenameAsset(path, "ResourceCatalog");
                data.name = "ResourceCatalog";
            
                EditorUtility.SetDirty(data);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            if (GUILayout.Button("Generate catalog"))
            {
                ClearOldCatalog(data);
                GenerateNewCatalog(data);
                CatalogEnumGenerator.GenerateAll(data);
            
                string path = AssetDatabase.GetAssetPath(data);
                AssetDatabase.RenameAsset(path, "ResourceCatalog");
                data.name = "ResourceCatalog";
            
                EditorUtility.SetDirty(data);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            if (GUILayout.Button("Generate Hash"))
            {
                CatalogEnumGenerator.GenerateAll(data);
                CatalogEnumGenerator.ApplyGenerate();
            }
            if (GUILayout.Button("Debug catalog"))
            {
                DebugInfo((CatalogObject)target);
            }
        }

        private void ClearOldCatalog(CatalogObject catalogObject)
        {
            catalogObject.Catalog.Clear();
        }
        private void GenerateNewCatalog(CatalogObject catalogObject)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources/CMS"))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "CMS");
                Debug.Log("CMS folder just created, it's empty");
                return;
            }

            var res = Resources.LoadAll("CMS");
            foreach (var a in res)
            {
                if (a is not GameObject go) continue;

                var path = AssetDatabase.GetAssetPath(go); // Assets/Resources/CMS/Foo/bar.prefab
                var cmsIndex = path.IndexOf("CMS/", StringComparison.Ordinal);

                if (cmsIndex == -1)
                {
                    Debug.LogWarning($"Object {go.name} is not inside CMS folder.");
                    continue;
                }

                var relativePath = path.Substring(cmsIndex);
                var folder = Path.GetDirectoryName(relativePath)?.Replace("\\", "/");
            
                var type = ResourceCatalogTypeFactory.GetCatalogType(folder);
                var catalog = catalogObject.GetOrCreate(type);
                catalog.Add(new CatalogEntryObject() { Object = go, HashKey = CatalogHash.StableHash(relativePath + go.name) });
            }
        }
        private void DebugInfo(CatalogObject catalogObject)
        {
            foreach (var b in catalogObject.Catalog)
                foreach (var a in b.Objects)
                    Debug.Log(b.Type.ToString() + " : " + a.Object.gameObject.name);
        }
    }
}