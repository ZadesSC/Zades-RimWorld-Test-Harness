using System.Runtime.Serialization.Json;
using System.Text;

namespace RimworldTestHarness.Core.Serialization;

public static class JsonFile
{
    public static T Load<T>(string path) where T : class
    {
        using var stream = File.OpenRead(path);
        var serializer = new DataContractJsonSerializer(typeof(T));
        return (T)(serializer.ReadObject(stream) ?? throw new InvalidOperationException($"Failed to deserialize {path}."));
    }

    public static void Save<T>(string path, T value)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using var stream = File.Create(path);
        var serializer = new DataContractJsonSerializer(typeof(T));
        serializer.WriteObject(stream, value);
    }

    public static string ToJson<T>(T value)
    {
        using var stream = new MemoryStream();
        var serializer = new DataContractJsonSerializer(typeof(T));
        serializer.WriteObject(stream, value);
        return Encoding.UTF8.GetString(stream.ToArray());
    }
}
