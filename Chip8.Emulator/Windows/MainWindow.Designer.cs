using System.ComponentModel;

namespace Chip8.Emulator.Windows;

partial class MainWindow
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        TopMenu = new MenuStrip();
        fileToolStripMenuItem = new ToolStripMenuItem();
        rOMsToolStripMenuItem = new ToolStripMenuItem();
        loadToolStripMenuItem = new ToolStripMenuItem();
        exitToolStripMenuItem = new ToolStripMenuItem();
        gameDisplay = new Chip8.Emulator.Windows.Controls.Display();
        TopMenu.SuspendLayout();
        SuspendLayout();
        // 
        // TopMenu
        // 
        TopMenu.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem });
        TopMenu.Location = new Point(0, 0);
        TopMenu.Name = "TopMenu";
        TopMenu.Size = new Size(800, 24);
        TopMenu.TabIndex = 1;
        TopMenu.Text = "menuStrip1";
        // 
        // fileToolStripMenuItem
        // 
        fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { rOMsToolStripMenuItem });
        fileToolStripMenuItem.Name = "fileToolStripMenuItem";
        fileToolStripMenuItem.Size = new Size(37, 20);
        fileToolStripMenuItem.Text = "File";
        // 
        // rOMsToolStripMenuItem
        // 
        rOMsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { loadToolStripMenuItem, exitToolStripMenuItem });
        rOMsToolStripMenuItem.Name = "rOMsToolStripMenuItem";
        rOMsToolStripMenuItem.Size = new Size(106, 22);
        rOMsToolStripMenuItem.Text = "ROMs";
        // 
        // loadToolStripMenuItem
        // 
        loadToolStripMenuItem.Name = "loadToolStripMenuItem";
        loadToolStripMenuItem.Size = new Size(100, 22);
        loadToolStripMenuItem.Text = "Load";
        loadToolStripMenuItem.Click += loadToolStripMenuItem_Click;
        // 
        // exitToolStripMenuItem
        // 
        exitToolStripMenuItem.Name = "exitToolStripMenuItem";
        exitToolStripMenuItem.Size = new Size(100, 22);
        exitToolStripMenuItem.Text = "Exit";
        exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
        // 
        // gameDisplay
        // 
        gameDisplay.Dock = DockStyle.Fill;
        gameDisplay.Location = new Point(0, 24);
        gameDisplay.Name = "gameDisplay";
        gameDisplay.Size = new Size(800, 426);
        gameDisplay.TabIndex = 2;
        gameDisplay.Text = "display1";
        // 
        // MainWindow
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(800, 450);
        Controls.Add(gameDisplay);
        Controls.Add(TopMenu);
        MainMenuStrip = TopMenu;
        Name = "MainWindow";
        Text = "MainWindow";
        Load += MainWindow_Load;
        TopMenu.ResumeLayout(false);
        TopMenu.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private MenuStrip TopMenu;
    private ToolStripMenuItem fileToolStripMenuItem;
    private ToolStripMenuItem rOMsToolStripMenuItem;
    private ToolStripMenuItem loadToolStripMenuItem;
    private ToolStripMenuItem exitToolStripMenuItem;
    private Controls.Display gameDisplay;
}