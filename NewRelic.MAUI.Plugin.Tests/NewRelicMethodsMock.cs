using System;
using System.Reflection;
using NewRelic.MAUI.Plugin;

internal class NewRelicMethodsMock : IDisposable
{
    private readonly Lazy<INewRelicMethods> _originalInstance;

    public NewRelicMethodsMock(INewRelicMethods mockImplementation)
    {
        _originalInstance = GetImplementation();
        SetImplementation(new Lazy<INewRelicMethods>(() => mockImplementation, System.Threading.LazyThreadSafetyMode.PublicationOnly));
    }

    public void Dispose()
    {
        SetImplementation(_originalInstance);
    }

    private static Lazy<INewRelicMethods> GetImplementation()
    {
        var field = typeof(CrossNewRelic).GetField("_implementation", BindingFlags.Static | BindingFlags.NonPublic);
        return (Lazy<INewRelicMethods>)field.GetValue(null);
    }

    private static void SetImplementation(Lazy<INewRelicMethods> implementation)
    {
        var field = typeof(CrossNewRelic).GetField("_implementation", BindingFlags.Static | BindingFlags.NonPublic);
        field.SetValue(null, implementation);
    }
}

