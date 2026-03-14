using System.IO;
using System.Runtime.Serialization.Json;

namespace RimworldTestHarness.Mod;

public static class JsonFile
{
    public static T Load<T>(string path) where T : class
    {
        using (var stream = File.OpenRead(path))
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            return (T)serializer.ReadObject(stream);
        }
    }

    public static void Save<T>(string path, T value)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        using (var stream = File.Create(path))
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            serializer.WriteObject(stream, value);
        }
    }
}
