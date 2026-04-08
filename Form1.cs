using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using CosmoCal.Services;

namespace CosmoCal
{
    public partial class Form1 : Form
    {
        // Win32 constants for borderless drag
        private const int WM_NCHITTEST   = 0x0084;
        private const int HTCAPTION      = 2;
        private const int HTLEFT         = 10;
        private const int HTRIGHT        = 11;
        private const int HTTOP          = 12;
        private const int HTTOPLEFT      = 13;
        private const int HTTOPRIGHT     = 14;
        private const int HTBOTTOM       = 15;
        private const int HTBOTTOMLEFT   = 16;
        private const int HTBOTTOMRIGHT  = 17;
        private const int RESIZE_BORDER  = 8;

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        public Form1()
        {
            // Borderless window — custom title bar rendered inside WebView
            this.FormBorderStyle = FormBorderStyle.None;
            this.Text            = "CosmoCal";
            this.Width           = 450;
            this.Height          = 750;
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.BackColor       = System.Drawing.Color.FromArgb(2, 2, 4);

            // Load custom icon (taskbar, Alt+Tab, window)
            var iconPath = System.IO.Path.Combine(AppContext.BaseDirectory, "icon.ico");
            if (System.IO.File.Exists(iconPath))
                this.Icon = new System.Drawing.Icon(iconPath);

            // Rounded corners on Windows 11 (DWM attribute 33 = DWMWCP_ROUND)
            try
            {
                var attr  = 33;
                var round = 2; // DWMWCP_ROUND
                DwmSetWindowAttribute(this.Handle, attr, ref round, sizeof(int));
            }
            catch { /* Ignore on older Windows */ }

            // Window drop-shadow for borderless window via DWM
            try
            {
                var ncrAttr   = 2;  // DWMWA_NCRENDERING_POLICY
                var ncrEnable = 2;  // DWMNCRP_ENABLED
                DwmSetWindowAttribute(this.Handle, ncrAttr, ref ncrEnable, sizeof(int));
            }
            catch { }

            // Create WindowControlService and subscribe before building DI container
            var winSvc = new WindowControlService();
            winSvc.CloseRequested           += () => this.Invoke(() => this.Close());
            winSvc.MinimizeRequested         += () => this.Invoke(() => this.WindowState = FormWindowState.Minimized);
            winSvc.MaximizeRestoreRequested  += () => this.Invoke(() =>
            {
                this.WindowState = this.WindowState == FormWindowState.Maximized
                    ? FormWindowState.Normal
                    : FormWindowState.Maximized;
            });

            // Set up dependency injection for Blazor
            var services = new ServiceCollection();
            services.AddWindowsFormsBlazorWebView();
            services.AddScoped<SubscriptionService>();
            services.AddSingleton(winSvc);          // expose to Blazor components

            var blazorWebView = new BlazorWebView
            {
                Dock     = DockStyle.Fill,
                HostPage = "wwwroot\\index.html",
                Services = services.BuildServiceProvider()
            };

            // Enable -webkit-app-region: drag support in WebView2
            blazorWebView.WebView.CoreWebView2InitializationCompleted += (_, _) =>
            {
                try { blazorWebView.WebView.CoreWebView2.Settings.IsNonClientRegionSupportEnabled = true; }
                catch { /* Older WebView2 runtimes may not support this — drag will gracefully degrade */ }
            };

            blazorWebView.RootComponents.Add<CosmoCal.Components.App>("#app");
            this.Controls.Add(blazorWebView);
        }

        // Allow resizing on all edges of a borderless window
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_NCHITTEST && !this.IsMaximized())
            {
                var pos   = new System.Drawing.Point(m.LParam.ToInt32());
                var local = this.PointToClient(pos);
                int w     = this.Width;
                int h     = this.Height;
                int b     = RESIZE_BORDER;

                if (local.X <= b && local.Y <= b)        { m.Result = (IntPtr)HTTOPLEFT;     return; }
                if (local.X >= w - b && local.Y <= b)    { m.Result = (IntPtr)HTTOPRIGHT;    return; }
                if (local.X <= b && local.Y >= h - b)    { m.Result = (IntPtr)HTBOTTOMLEFT;  return; }
                if (local.X >= w - b && local.Y >= h - b){ m.Result = (IntPtr)HTBOTTOMRIGHT; return; }
                if (local.X <= b)                         { m.Result = (IntPtr)HTLEFT;        return; }
                if (local.X >= w - b)                    { m.Result = (IntPtr)HTRIGHT;       return; }
                if (local.Y <= b)                        { m.Result = (IntPtr)HTTOP;         return; }
                if (local.Y >= h - b)                    { m.Result = (IntPtr)HTBOTTOM;      return; }
            }
            base.WndProc(ref m);
        }

        private bool IsMaximized() => this.WindowState == FormWindowState.Maximized;

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
    }
}
