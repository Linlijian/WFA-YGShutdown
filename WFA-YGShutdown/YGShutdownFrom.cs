using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WFA_YGShutdown
{
    public partial class YGShutdownFrom : Form
    {
        #region values
        bool isStart = true;
        System.Threading.Thread t;

        //const and dll functions for moving form
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd,
            int Msg, int wParam, int lParam);

        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        #endregion

        #region main
        public YGShutdownFrom()
        {
            InitializeComponent();
        }
        #endregion

        #region action
        private void btnStart_Click(object sender, EventArgs e)
        {
            if (isStart)
            {
                btnStart.Text = "Cancel";
                labelprocess.Text = "Start !";
                t = new System.Threading.Thread(Start);
                t.Start();

                isStart = false;

            }
            else
            {
                btnStart.Text = "Start";
                labelprocess.Text = "Stop !";
                Stop();
                t.Abort();
                
                isStart = true;
            }
        }
        private void YGShutdownFrom_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Stop();
            t.Abort();
            this.Close();
        }
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }

        }
        #endregion

        #region method
        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("http://google.com/generate_204"))
                    return true;
            }
            catch
            {
                return false;
            }
        }
        private void Start()
        {
            while (true)
            {
                if (!CheckForInternetConnection())
                {
                    Shutdown();
                    break;
                }
            };
        }
        private void Shutdown()
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            string process = "YGOnline.exe";
            psi.FileName = "cmd.exe";
            psi.Arguments = @"/C wmic process where name='" + process + "' delete & shutdown -s -t 10";
            psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            using (Process p = Process.Start(psi))
            {
                p.WaitForExit();
            }
        }
        private void Stop()
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "cmd.exe";
            psi.Arguments = @"/C shutdown /a";
            psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            using (Process p = Process.Start(psi))
            {
                p.WaitForExit();
            }
        }
        #endregion
    }
}
