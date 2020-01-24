using AutoUpdaterDotNET;
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WFA_YGShutdown
{
    public partial class YGShutdownFrom : Form
    {
        #region values
        bool isStart = true;
        Thread t;

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
            AutoUpdater.Start("https://raw.githubusercontent.com/Linlijian/WFA-YGShutdown/master/WFA-YGShutdown/CheckUpdate.xml");
        }
        private void YGShutdownFrom_Load(object sender, EventArgs e)
        {
            AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;
        }
        #endregion

        #region action
        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                if (isStart)
                {
                    btnStart.Text = "Cancel";
                    labelprocess.Text = "Start !";
                    t = new Thread(Start);
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
            catch
            {
                MessageBox.Show("Time out plaese try again!", "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            try
            {
                Stop();
                t.Abort();
                this.Close();
            }
            catch
            {
                Stop();
                //t.Abort();
                this.Close();
            }
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
                WaitSomeTime();
            };
        }
        public async void WaitSomeTime()
        {
            await Task.Delay(5000); //wait 5sc
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
        private void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs args)
        {
            if (args != null)
            {
                if (args.IsUpdateAvailable)
                {
                    DialogResult dialogResult;
                    if (args.Mandatory)
                    {
                        dialogResult =
                            MessageBox.Show(
                                $@"There is new version {args.CurrentVersion} available. You are using version {args.InstalledVersion}. This is required update. Press Ok to begin updating the application.", @"Update Available",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                    }
                    else
                    {
                        dialogResult =
                            MessageBox.Show(
                                $@"There is new version {args.CurrentVersion} available. You are using version {
                                        args.InstalledVersion
                                    }. Do you want to update the application now?", @"Update Available",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Information);
                    }

                    // Uncomment the following line if you want to show standard update dialog instead.
                    //AutoUpdater.ShowUpdateForm();

                    if (dialogResult.Equals(DialogResult.Yes) || dialogResult.Equals(DialogResult.OK))
                    {
                        try
                        {
                            if (AutoUpdater.DownloadUpdate())
                            {
                                Application.Exit();
                            }
                        }
                        catch (Exception exception)
                        {
                            MessageBox.Show(exception.Message, exception.GetType().ToString(), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show(@"There is no update available please try again later.", @"No update available",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show(
                        @"There is a problem reaching update server please check your internet connection and try again later.",
                        @"Update check failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion
    }
}
