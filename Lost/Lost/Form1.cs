using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Device.Location;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lost.Services;
using System.Net;
using System.Timers;

namespace Lost
{

    public partial class Form1 : Form
    {
        private GeoCoordinateWatcher gcw;
        private SimpleService sv;
        private System.Timers.Timer t;

        public Form1()
        {
            InitializeComponent();

            this.notifyIcon1.Icon = new Icon(SystemIcons.WinLogo, 40, 40);
            this.notifyIcon1.MouseDoubleClick += notifyIcon1_MouseDoubleClick; 
            this.Resize +=Form1_Resize;

            String user, pwd;
            ReadPassword(out user, out pwd);

            sv = new SimpleService();
            NetworkCredential credentials = new NetworkCredential(user, pwd);
            sv.Credentials = credentials;
            sv.PreAuthenticate = true;

            t = new System.Timers.Timer(1000);
            t.Elapsed += SendLocation;

            gcw = new GeoCoordinateWatcher();
            gcw.Start();
            gcw.StatusChanged += gcw_StatusChanged;
        }

        private void ReadPassword(out string user, out string pwd)
        {
            string[] lines = System.IO.File.ReadAllLines(@"Credentials.txt");
            user = lines[0];
            pwd = lines[1];
        }

        void Form1_Resize(object sender, EventArgs e)
        {             
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                this.Hide();
            }
        }

        void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.ShowInTaskbar = true;
        }
                
        void gcw_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            if (e.Status == GeoPositionStatus.Ready)
                t.Start();
        }

        protected override void SetVisibleCore(bool value)
        {
            if (!this.IsHandleCreated)
            {
                value = false;
                this.CreateHandle();
            }
            base.SetVisibleCore(value);
        }

        private void SendLocation(object sender, ElapsedEventArgs e)
        {
            try
            {
                GeoCoordinate gc = gcw.Position.Location;
                Location loc = new Location();
                loc.Latitude = gc.Latitude;
                loc.Longitude = gc.Longitude;

                string stResponse = sv.ProcessMyLocation(loc);
                Console.WriteLine(stResponse);

                if (stResponse == "Lock Computer")
                    LockComputer();
            }
            catch
            {               
            }
                                 
        }

        private void LockComputer()
        {
            System.Diagnostics.Process.Start(@"C:\WINDOWS\system32\rundll32.exe", "user32.dll,LockWorkStation");
        }


    }
}
