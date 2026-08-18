using System.Reflection;

namespace mouse_nudge;

static class AppIcon
{
    const string ResourceName = "mouse_nudge.app.ico";

    public static Icon Load()
    {
        using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName);
        if (stream is null)
        {
            throw new InvalidOperationException($"Embedded resource '{ResourceName}' was not found.");
        }

        return new Icon(stream);
    }
}
