namespace ELearning.Tests.Unit;

internal static class Helpers
{
    public static void SetPrivate(object obj, string prop, object value)
    {
        var p = obj.GetType().GetProperty(prop,
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);
        p?.SetValue(obj, value);
    }
}
