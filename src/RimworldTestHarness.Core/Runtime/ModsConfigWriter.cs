using System.Security;
using System.Text;

namespace RimworldTestHarness.Core.Runtime;

public static class ModsConfigWriter
{
    private static readonly string[] ExpansionIds =
    [
        "ludeon.rimworld.royalty",
        "ludeon.rimworld.ideology",
        "ludeon.rimworld.biotech",
        "ludeon.rimworld.anomaly",
        "ludeon.rimworld.odyssey",
    ];

    public static void Write(string path, IReadOnlyCollection<string> enabledMods)
    {
        var builder = new StringBuilder();
        builder.AppendLine("<?xml version=\"1.0\" ?>");
        builder.AppendLine("<ModsConfigData>");
        builder.AppendLine("  <version>1.6.4633 rev1260</version>");
        builder.AppendLine("  <activeMods>");

        foreach (var mod in enabledMods)
        {
            builder.Append("    <li>");
            builder.Append(SecurityElement.Escape(mod));
            builder.AppendLine("</li>");
        }

        builder.AppendLine("  </activeMods>");
        builder.AppendLine("  <knownExpansions>");

        foreach (var expansion in ExpansionIds)
        {
            builder.Append("    <li>");
            builder.Append(SecurityElement.Escape(expansion));
            builder.AppendLine("</li>");
        }

        builder.AppendLine("  </knownExpansions>");
        builder.AppendLine("</ModsConfigData>");

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, builder.ToString(), new UTF8Encoding(false));
    }
}
