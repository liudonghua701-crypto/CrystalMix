namespace CrystalMix;

internal static class AppIcon
{
    public static Icon Load()
    {
        using var stream = typeof(AppIcon).Assembly.GetManifestResourceStream("CrystalMix.Assets.CrystalMix.ico")
            ?? throw new InvalidDataException("Application icon resource missing.");
        using var icon = new Icon(stream, new Size(48, 48));
        return (Icon)icon.Clone();
    }
}
