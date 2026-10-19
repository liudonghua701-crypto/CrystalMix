using System.Reflection;
using System.Text.RegularExpressions;
using CrystalMix;

internal static class HelpChecks
{
    public static void RunUi()
    {
        Exception? failure = null;
        var thread = new Thread(() => { try { CheckWindows(); } catch (Exception e) { failure = e; } });
        thread.SetApartmentState(ApartmentState.STA); thread.Start(); thread.Join();
        if (failure is not null) throw failure;
    }
    private static void CheckWindows()
    {
        var assembly = typeof(MainForm).Assembly;
        var localizerType = assembly.GetType("CrystalMix.Localizer")!;
        var languageType = assembly.GetType("CrystalMix.UiLanguage")!;
        var helpType = assembly.GetType("CrystalMix.HelpDialog")!;
        foreach (var name in new[] { "English", "Chinese" })
        {
            var localizer = Activator.CreateInstance(localizerType)!;
            localizerType.GetProperty("Language")!.SetValue(localizer, Enum.Parse(languageType, name));
            using var form = (Form)Activator.CreateInstance(helpType, localizer)!;
            form.Font = new Font(form.Font.FontFamily, 16.5f);
            form.Show(); form.Size = new Size(700, 600); Application.DoEvents();
            var body = form.Controls.OfType<Panel>().Single(p => p is not FlowLayoutPanel);
            var text = body.Controls.OfType<TextBox>().Single();
            if (!text.ReadOnly || !text.Multiline || text.ScrollBars != ScrollBars.Vertical || text.Height < 100)
                throw new Exception("Help is not readable and scrollable");
            if (name == "English" && Regex.IsMatch(text.Text, @"\p{IsCJKUnifiedIdeographs}")) throw new Exception("English window contains Chinese");
            if (!text.Text.Contains("\r\n")) throw new Exception("Windows help line breaks missing");
            Directory.CreateDirectory("dist/ui-checks");
            using var bitmap = new Bitmap(form.Width, form.Height);
            form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
            bitmap.Save($"dist/ui-checks/help-{name}.png");
            text.SelectionStart = text.TextLength; text.ScrollToCaret(); Application.DoEvents();
            form.Close();
        }
    }
    public static void Run()
    {
        var assembly = typeof(MainForm).Assembly;
        var language = assembly.GetType("CrystalMix.UiLanguage")!;
        var localizer = assembly.GetType("CrystalMix.Localizer")!;
        var advanced = assembly.GetType("CrystalMix.AdvancedHelp")!;
        foreach (var name in new[] { "English", "Chinese" })
        {
            var value = Enum.Parse(language, name);
            var instance = Activator.CreateInstance(localizer)!;
            localizer.GetProperty("Language")!.SetValue(instance, value);
            var basic = (string)localizer.GetProperty("Item")!.GetValue(instance, new object[] { "helpBody" })!;
            var extra = (string)advanced.GetMethod("Get")!.Invoke(null, new[] { value })!;
            if (name == "English" && Regex.IsMatch(basic + extra, @"\p{IsCJKUnifiedIdeographs}")) throw new Exception("Chinese leaked into English help");
            if (name == "Chinese" && extra.Contains("Isotopes")) throw new Exception("English prose leaked into Chinese help");
            foreach (var example in new[] { "[175Lu]2O3", "[180mTa]", "Ba1-xKxFe2As2", "Ba1-xKxFeAs" })
                if (!extra.Contains(example)) throw new Exception("Missing localized example: " + example);
        }
        using var resource = assembly.GetManifestResourceStream("CrystalMix.Assets.CrystalMix.ico")!;
        using var reader = new BinaryReader(resource);
        if (reader.ReadUInt16() != 0 || reader.ReadUInt16() != 1 || reader.ReadUInt16() != 7) throw new Exception("Invalid multi-size icon");
        if (assembly.GetName().Version!.Major != 1 || assembly.GetName().Version!.Minor != 1) throw new Exception("Release version mismatch");
    }
}
