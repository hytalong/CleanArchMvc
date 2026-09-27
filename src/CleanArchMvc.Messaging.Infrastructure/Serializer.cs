using System.Text.Json;

namespace CleanArchMvc.Messaging.Infrastructure;

internal static class Serializer
{
    public static byte[] Serialize<T>(T obj)
    {
        return JsonSerializer.SerializeToUtf8Bytes(obj, obj?.GetType() ?? typeof(object));
    }

    public static T? Deserialize<T>(byte[] bytes)
    {
        return JsonSerializer.Deserialize<T>(bytes);
    }

    public static object? Deserialize(byte[] bytes, Type type)
    {
        return JsonSerializer.Deserialize(bytes, type);
    }
}
