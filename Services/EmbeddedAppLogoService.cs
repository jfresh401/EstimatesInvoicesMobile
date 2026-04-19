using System.Reflection;

namespace FreshEstimate.Mobile.Services;

public sealed class EmbeddedAppLogoService
{
    public byte[]? LoadAppLogoBytes()
    {
        var assembly = Assembly.GetExecutingAssembly();
        const string resourceName = "FreshEstimate.Mobile.Resources.Images.app-logo.png";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
            return null;

        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }
}
