namespace Json5.Tests.Helpers;

using System.Text;

public static class Extensions {
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
        foreach (var item in source) action(item);
    }

    public static string ToUnicode(this int x) => Encoding.Unicode.GetString(BitConverter.GetBytes(checked((ushort)x)));
}
