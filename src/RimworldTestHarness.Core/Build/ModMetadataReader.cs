using System.Xml.Linq;
using RimworldTestHarness.Core.Models;

namespace RimworldTestHarness.Core.Build;

public sealed class ModMetadataReader
{
    public ModMetadata Read(string modDirectory)
    {
        var aboutPath = Path.Combine(modDirectory, "About", "About.xml");
        var document = XDocument.Load(aboutPath);
        var root = document.Root ?? throw new InvalidOperationException($"Invalid About.xml at {aboutPath}");

        return new ModMetadata
        {
            Name = root.Element("name")?.Value?.Trim() ?? string.Empty,
            PackageId = root.Element("packageId")?.Value?.Trim() ?? string.Empty,
            SupportedVersions = root.Element("supportedVersions")?.Elements("li").Select(static element => element.Value.Trim()).Where(static value => value.Length > 0).ToList() ?? [],
            Dependencies = root.Element("modDependencies")?.Elements("li").Select(static element => element.Element("packageId")?.Value?.Trim() ?? string.Empty).Where(static value => value.Length > 0).ToList() ?? [],
        };
    }
}
