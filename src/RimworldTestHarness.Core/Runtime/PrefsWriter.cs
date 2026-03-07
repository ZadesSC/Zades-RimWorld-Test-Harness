using System.Xml.Linq;
using RimworldTestHarness.Core.Models;

namespace RimworldTestHarness.Core.Runtime;

public static class PrefsWriter
{
    public static void Ensure(string prefsPath, HarnessManifest manifest)
    {
        XDocument document;
        XElement root;

        if (File.Exists(prefsPath))
        {
            document = XDocument.Load(prefsPath);
            root = document.Root ?? new XElement("PrefsData");
            if (document.Root is null)
            {
                document.Add(root);
            }
        }
        else
        {
            root = new XElement("PrefsData");
            document = new XDocument(new XDeclaration("1.0", "utf-8", null), root);
        }

        SetValue(root, "screenWidth", manifest.WindowWidth.ToString());
        SetValue(root, "screenHeight", manifest.WindowHeight.ToString());
        SetValue(root, "fullscreen", "False");
        SetValue(root, "runInBackground", "True");
        SetValue(root, "pauseOnLoad", "False");
        SetValue(root, "automaticPauseMode", "Never");

        Directory.CreateDirectory(Path.GetDirectoryName(prefsPath)!);
        document.Save(prefsPath);
    }

    private static void SetValue(XElement root, string elementName, string value)
    {
        var element = root.Element(elementName);
        if (element is null)
        {
            root.Add(new XElement(elementName, value));
            return;
        }

        element.Value = value;
    }
}
