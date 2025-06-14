using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8.Emulator.Windows.Controls
{
    internal class Display : Control
    {
        public Display()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint, true);
            this.UpdateStyles();
        }

        private int[,]? framebuffer;
        public void Render(int[,] framebuffer)
        {
            this.framebuffer = framebuffer;
            this.Invalidate(); // Paint tetikler
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (framebuffer == null)
                return;

            Graphics g = e.Graphics;
            int cols = framebuffer.GetLength(0);
            int rows = framebuffer.GetLength(1);

            float pixelWidth = (float)this.Width / cols;
            float pixelHeight = (float)this.Height / rows;

            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    g.FillRectangle(framebuffer[x, y] != 0 ? Brushes.White : Brushes.Black, x * pixelWidth, y * pixelHeight, pixelWidth, pixelHeight);
                }
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            this.Invalidate();
        }
    }
}
