namespace ReceiptRen;

/// <summary>
/// Application entry point.
/// </summary>
static class Program
{
    /// <summary>Starts the application and opens the main window.</summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
