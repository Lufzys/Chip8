using Chip8.Emulator.Emulation;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Chip8.Emulator.Windows;

public partial class MainWindow : Form
{
    [DllImport("kernel32.dll")]
    private static extern bool AllocConsole();

    Emulation.Chip8 chip8 = new Emulation.Chip8();

    public MainWindow()
    {
        InitializeComponent();

        // Set Form Properties
        this.Text = "Chip8 Emulator";
        this.StartPosition = FormStartPosition.CenterScreen;
    }

    private void MainWindow_Load(object sender, EventArgs e)
    {
        AllocConsole();
        chip8.OnEmulationStart += Initalize;
        chip8.OnEmulationRender += Render;
        chip8.OnEmulationStop += Release;
    }

    private void Release(Context context)
    {
        Console.Clear();
    }

    private void Initalize(Context context)
    {
        Console.Clear();
        Console.WriteLine("ROM loaded!");
    }

    private void loadToolStripMenuItem_Click(object sender, EventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Title = "Select a CHIP-8 ROM",
            Filter = "CHIP-8 ROM Files (*.ch8)|*.ch8",
            CheckFileExists = true,
            CheckPathExists = true,
            Multiselect = false
        };
        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            string filePath = openFileDialog.FileName;
            byte[] romData = File.ReadAllBytes(filePath);
            chip8.Initalize(romData);
            chip8.Start();
        }
    }

    private void Render(Context context)
    {
        gameDisplay.Render(context.Framebuffer);
    }

    private void exitToolStripMenuItem_Click(object sender, EventArgs e)
    {
        chip8.Stop();
    }

    private void gameDisplay_Click(object sender, EventArgs e)
    {

    }
}