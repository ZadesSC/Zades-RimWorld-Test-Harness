using System.Xml.Linq;

namespace RimworldTestHarness.Core.Build;

public sealed class DefReader
{
    public IReadOnlyList<string> ReadThingDefs(string modDirectory)
    {
        var defsDirectory = Path.Combine(modDirectory, "Defs");
        if (!Directory.Exists(defsDirectory))
        {
            return [];
        }

        var result = new List<string>();
        foreach (var file in Directory.EnumerateFiles(defsDirectory, "*.xml", SearchOption.AllDirectories))
        {
            var document = XDocument.Load(file);
            var thingDefs = document.Descendants("ThingDef")
                .Select(static element => element.Element("defName")?.Value?.Trim())
                .Where(static value => !string.IsNullOrWhiteSpace(value));

            result.AddRange(thingDefs!);
        }

        return result;
    }
}
