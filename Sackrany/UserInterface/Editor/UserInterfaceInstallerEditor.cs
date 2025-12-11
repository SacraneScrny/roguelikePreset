using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Sackrany.UserInterface.Abstract;
using Sackrany.UserInterface.Components;

using UnityEditor;

using UnityEngine;

namespace Sackrany.UserInterface.Editor
{
    [CustomEditor(typeof(UserInterfaceInstaller))]
    public class UserInterfaceInstallerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Update Presenters"))
            {
                GeneratePresenterEnum();
            }
        }

        private static void GeneratePresenterEnum()
        {
            var presenterType = typeof(Presenter);

            // Получаем все типы, наследующие от Presenter
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch (ReflectionTypeLoadException e) { return e.Types.Where(t => t != null); }
                })
                .Where(t => t.IsClass && !t.IsAbstract && presenterType.IsAssignableFrom(t))
                .Where(t => t.Namespace != null && t.Namespace.StartsWith("Logic.UserInterface"))
                .OrderBy(t => t.Name)
                .ToList();

            if (types.Count == 0)
            {
                Debug.LogWarning("Не найдено ни одного класса, наследуемого от Presenter в Logic.UserInterface.");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("// Автоматически сгенерировано скриптом UserInterfaceInstallerEditor");
            sb.AppendLine("namespace Logic.UserInterface.Factory");
            sb.AppendLine("{");
            sb.AppendLine("    public enum PresenterType");
            sb.AppendLine("    {");

            foreach (var t in types)
                sb.AppendLine($"        {t.Name},");

            sb.AppendLine("    }");
            sb.AppendLine("}");

            var path = "Assets/Logic/UserInterface/Factory/PresenterType.cs";
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);

            AssetDatabase.Refresh();
            Debug.Log($"Сгенерировано {types.Count} Presenter-классов в {path}");
        }
    }
}
