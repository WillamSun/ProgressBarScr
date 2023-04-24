﻿using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ProgressBarSrc
{
    public partial class ScreenSaverForm : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);
        struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public Rectangle ToRectangle()
            {
                return new Rectangle(Left, Top, Right - Left, Bottom - Top);
            }
        }

        private int maxProgressBarWidth = 300;
        private int minProgressBarWidth = 50;
        private int maxProgressBarHeight = 40;
        private int minProgressBarHeight = 10;
        private int maxMoveSpeed = 5;
        private int minMoveSpeed = 1;
        private int ProgressBarCount = 20;
        private int maxProgressIncrement = 10;
        private int minProgressIncrement = 0;

        readonly private Random random = new Random();

        public ScreenSaverForm()
        {
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.None;
            Bounds = Screen.PrimaryScreen.Bounds;
            BackColor = Color.Black;
            label1.ForeColor = Color.White;
            IsPreviewMode = false;
        }

        private IntPtr previewHandle;

        public bool IsPreviewMode;

        public ScreenSaverForm(IntPtr handle)
        {
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.None;
            Bounds = Screen.PrimaryScreen.Bounds;
            BackColor = Color.Black;
            label1.ForeColor = Color.White;
            previewHandle = handle;
            IsPreviewMode = true;
        }

        double x;
        double y;

        private void ScreenSaverForm_Load(object sender, EventArgs e)
        {
            if (IsPreviewMode)
            {
                SetParent(Handle, previewHandle);
                GetWindowRect(previewHandle, out RECT rect);
                Size = rect.ToRectangle().Size;
                //GetClientRect(previewHandle, out RECT rect);
                //Bounds = rect.ToRectangle();
            }
            Program.FilesINI iniFile = new Program.FilesINI(Path.Combine(Application.StartupPath, "ProBarScrSettings.ini"));

            string background = iniFile.Read("Background", "Settings");
            if (background.ToLower() == "light") { BackColor = Color.White; label1.ForeColor = Color.Black; }
            else if (background.ToLower() == "dark" || background.ToLower() == "default") { BackColor = Color.Black; label1.ForeColor = Color.White; }
            else if (!string.IsNullOrEmpty(background)) MessageBox.Show(
                "\"" + background + "\" is not a valid value for Background", "Error");

            maxProgressBarWidth = iniFile.ReadInteger("MaxProgressBarWidth", "Settings", maxProgressBarWidth);
            minProgressBarWidth = iniFile.ReadInteger("MinProgressBarWidth", "Settings", minProgressBarWidth);
            maxProgressBarHeight = iniFile.ReadInteger("MaxProgressBarHeight", "Settings", maxProgressBarHeight);
            minProgressBarHeight = iniFile.ReadInteger("MinProgressBarHeight", "Settings", minProgressBarHeight);
            maxMoveSpeed = iniFile.ReadInteger("MaxMoveSpeed", "Settings", maxMoveSpeed);
            minMoveSpeed = iniFile.ReadInteger("MinMoveSpeed", "Settings", minMoveSpeed);
            maxProgressIncrement = iniFile.ReadInteger("MaxProgressIncrement", "Settings", maxProgressIncrement);
            minProgressIncrement = iniFile.ReadInteger("MinProgressIncrement", "Settings", maxProgressIncrement);
            ProgressBarCount = iniFile.ReadInteger("ProgressBarCount", "Settings", ProgressBarCount);
            if (minProgressBarHeight > maxProgressBarHeight)
                MessageBox.Show("MinProgressBarHeight value can't be bigger than MaxProgressBarHeight value", "Error");
            if (minProgressBarWidth > maxProgressBarWidth)
                MessageBox.Show("MinProgressBarWidth value can't be bigger than MaxProgressBarWidth value", "Error");
            //Cursor.Hide();
            for (int i = 0; i < ProgressBarCount; i++)
            {
                ProgressBar pb = new ProgressBar();
                pb.Name = i.ToString();
                pb.Tag = random.Next(minMoveSpeed, maxMoveSpeed + 1);
                pb.Size = new Size(random.Next(minProgressBarWidth, maxProgressBarWidth + 1), random.Next(minProgressBarHeight, maxProgressBarHeight + 1));
                pb.Location = new Point(-pb.Width, random.Next(0, Height - pb.Height + 1 <=0 ? 0 : Height - pb.Height + 1));
                pb.MouseEnter += MouseHoverOnProgressBar;
                pb.MouseLeave += MouseLeaveProgressBar;
                Controls.Add(pb);
                Timer timer = new Timer();
                Timer timer2 = new Timer();
                Timer timer3 = new Timer();
                timer.Interval = 10;
                timer2.Interval = 1000;
                timer3.Interval = random.Next(1, ProgressBarCount * 100 + 1);
                timer.Tick += Update;
                timer2.Tick += UpdateValue;
                timer3.Tick += MakeDelay;
                timer2.Tag = timer;
                timer3.Tag = new object[] { false, timer };
                timer.Tag = new object[] { pb, timer2 };
                timer3.Start();
            }
        }

        private void Update(object sender, EventArgs e)
        {
            ProgressBar pb = ((sender as Timer).Tag as object[])[0] as ProgressBar;
            int tag = (int)pb.Tag;
            pb.Location = new Point(pb.Location.X + tag, pb.Location.Y);
            if (pb.Location.X > Width + pb.Width)
            {
                pb.Size = new Size(random.Next(minProgressBarWidth, maxProgressBarWidth), random.Next(minProgressBarHeight, maxProgressBarHeight + 1));
                pb.Location = new Point(-pb.Width, random.Next(0, Height - pb.Height + 1 <= 0 ? 0 : Height - pb.Height + 1));
                pb.Value = 0;
            }
        }

        private void UpdateValue(object sender, EventArgs e)
        {
            ProgressBar pb = (((sender as Timer).Tag as Timer).Tag as object[])[0] as ProgressBar;
            if (pb.Value <= 100)
            {
                int ProgressIncrement = random.Next(minProgressIncrement, maxProgressIncrement + 1);
                while (pb.Value + ProgressIncrement > 100) ProgressIncrement--;
                pb.Value += ProgressIncrement;
            }
        }

        private void MakeDelay(object sender, EventArgs e)
        {
            Timer main_timer = ((sender as Timer).Tag as object[])[1] as Timer;
            object[] controls = main_timer.Tag as object[];
            Timer this_sender = sender as Timer;
            object[] this_sender_tag = this_sender.Tag as object[];
            if ((bool)this_sender_tag[0])
            {
                main_timer.Start();
                (controls[1] as Timer).Start();
                this_sender.Stop();
            }
            else this_sender_tag[0] = true;
            this_sender.Tag = this_sender_tag;
        }

        private void MouseHoverOnProgressBar(object sender, EventArgs e)
        {
            label1.Text = (sender as ProgressBar).Value.ToString() + "%";
        }

        private void MouseLeaveProgressBar(object sender, EventArgs e)
        {
            label1.Text = "";
        }

        private void ScreenSaverForm_UserActed(object sender, MouseEventArgs e)
        {
            if (!IsPreviewMode) Close();
        }

        private void ScreenSaverForm_UserActed(object sender, KeyPressEventArgs e)
        {
            if (!IsPreviewMode) Close();
        }
    }
}