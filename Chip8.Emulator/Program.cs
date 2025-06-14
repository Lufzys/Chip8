namespace Chip8.Emulator;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new Windows.MainWindow());
    }
}