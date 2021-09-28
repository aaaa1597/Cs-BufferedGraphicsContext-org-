using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BufferedGraphicsContextBitBltBmp
{
    public partial class Form1 : Form
    {
        enum TernaryRasterOperations : uint
        {
            SRCCOPY = 0x00CC0020,
            SRCPAINT = 0x00EE0086,
            SRCAND = 0x008800C6,
            SRCINVERT = 0x00660046,
            SRCERASE = 0x00440328,
            NOTSRCCOPY = 0x00330008,
            NOTSRCERASE = 0x001100A6,
            MERGECOPY = 0x00C000CA,
            MERGEPAINT = 0x00BB0226,
            PATCOPY = 0x00F00021,
            PATPAINT = 0x00FB0A09,
            PATINVERT = 0x005A0049,
            DSTINVERT = 0x00550009,
            BLACKNESS = 0x00000042,
            WHITENESS = 0x00FF0062,
            CAPTUREBLT = 0x40000000
        }

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight,
          IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

        [DllImport("user32.dll")]
        static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            // DoubleBufferオブジェクトの作成
            mDoubleBufferBitmap = new Bitmap[] { new Bitmap(pictureBox1.Width, pictureBox1.Height), new Bitmap(pictureBox1.Width, pictureBox1.Height) };
            /* 拡縮率を求める */
            mScale = pictureBox1.Height / (double)mOrgBitmap.Height;
            /* 左上オフセットを求める */
            mLeftTopScaleOffset = new Point(-(((int)(mOrgBitmap.Width * mScale) - pictureBox1.Width) / 2), -(((int)(mOrgBitmap.Height * mScale) - pictureBox1.Height) / 2));
            /* 左上移動オフセットを初期化 */
            mLeftTopMoveOffset.X = 0;
            mLeftTopMoveOffset.Y = 0;

            sw.Stop();
            double time = sw.ElapsedTicks / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
            System.IO.File.AppendAllText(@"../../../aaaalog.log", $"Form1_Load {DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss.fff")} DrawTime =	{ time.ToString()}	[ms]\n");
        }

        Bitmap mOrgBitmap = new Bitmap(@"../../../4k4k-67.png");
        double mScale = 1;
        Point mLeftTopScaleOffset = new Point();
        Point mLeftTopMoveOffset = new Point();
        Bitmap[] mDoubleBufferBitmap = null;
        bool mIsOdd = false;

        private void mnuTest_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            /* 拡縮率を求める */
            mScale = pictureBox1.Height / (double)mOrgBitmap.Height;
            /* 左上オフセットを求める */
            mLeftTopScaleOffset = new Point(-(((int)(mOrgBitmap.Width * mScale) - pictureBox1.Width) / 2), -(((int)(mOrgBitmap.Height * mScale) - pictureBox1.Height) / 2));
            /* 左上移動オフセットを初期化 */
            mLeftTopMoveOffset.X = 0;
            mLeftTopMoveOffset.Y = 0;

            pictureBox1.Refresh();

            sw.Stop();
            double time = sw.ElapsedTicks / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
            System.IO.File.AppendAllText(@"../../../aaaalog.log", $"mnuTest_Click {DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss.fff")} DrawTime =	{ time.ToString()}	[ms]\n");
        }

        Point mPrevMousePoint = new Point();
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            Point deltamove = new Point(e.X - mPrevMousePoint.X, e.Y - mPrevMousePoint.Y);
            /* 左上移動オフセットを求める */
            mLeftTopMoveOffset.X += deltamove.X;
            mLeftTopMoveOffset.Y += deltamove.Y;

            pictureBox1.Refresh();

            mPrevMousePoint = new Point(e.X, e.Y);

            sw.Stop();
            double time = sw.ElapsedTicks / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
            System.IO.File.AppendAllText(@"../../../aaaalog.log", $"pictureBox1_MouseMove {DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss.fff")} DrawTime =	{ time.ToString()}	[ms] ");
            System.IO.File.AppendAllText(@"../../../aaaalog.log", $"deltamove={deltamove} mLeftTopMoveOffset.X={mLeftTopMoveOffset.X} mLeftTopMoveOffset.Y={mLeftTopMoveOffset.Y}\n");
        }

        const double SCALE_MIN = 0.01;

        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            /* 拡縮率を求める */
            mScale += (e.Delta / 120 * 0.01);
            if (mScale < SCALE_MIN) { mScale = SCALE_MIN; }
            /* 左上拡縮オフセットを求める */
            mLeftTopScaleOffset = new Point(-(((int)(mOrgBitmap.Width * mScale) - pictureBox1.Width) / 2), -(((int)(mOrgBitmap.Height * mScale) - pictureBox1.Height) / 2));

            pictureBox1.Refresh();

            sw.Stop();
            double time = sw.ElapsedTicks / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
            System.IO.File.AppendAllText(@"../../../aaaalog.log", $"pictureBox1_MouseWheel {DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss.fff")} DrawTime =	{ time.ToString()}	[ms]\n");
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) { return; }
            mPrevMousePoint = new Point(e.X, e.Y);
            return;
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            Bitmap nowBitmap = mDoubleBufferBitmap[(mIsOdd = !mIsOdd) ? 0 : 1];
            Graphics g = Graphics.FromImage(nowBitmap);
            g.Clear(BackColor);
            g.DrawImage(mOrgBitmap, mLeftTopScaleOffset.X + mLeftTopMoveOffset.X, mLeftTopScaleOffset.Y + mLeftTopMoveOffset.Y, (int)(mOrgBitmap.Width * mScale), (int)(mOrgBitmap.Height * mScale));
            g.Dispose();

            /* 出力DC取得 */
            IntPtr dstpbxhdc = e.Graphics.GetHdc();
            /* 入力DC取得 */
            IntPtr srchdc = CreateCompatibleDC(dstpbxhdc);
            IntPtr porg = SelectObject(srchdc, nowBitmap.GetHbitmap());

            BitBlt(dstpbxhdc, 0, 0, pictureBox1.Width, pictureBox1.Height, srchdc, 0, 0, TernaryRasterOperations.SRCCOPY);

            DeleteDC(srchdc);
            e.Graphics.ReleaseHdc(dstpbxhdc);

            sw.Stop();
            double time = sw.ElapsedTicks / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
            System.IO.File.AppendAllText(@"../../../aaaalog.log", $"pictureBox1_Paint {DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss.fff")} DrawTime =	{ time.ToString()}[ms]	\n");
        }
    }
}
