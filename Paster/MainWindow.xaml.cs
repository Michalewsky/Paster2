using System;
using System.IO;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Runtime.InteropServices;


namespace PasterApp
{

    public partial class App : Application
    {
        private System.Windows.Forms.NotifyIcon _notifyIcon;
        private bool _isExit;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindow = new MainWindow();
            MainWindow.Closing += MainWindow_Closing;

            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.DoubleClick += (s, args) => ShowMainWindow();
            _notifyIcon.Icon = PasterApp.Properties.Resources.INTPaster;
            _notifyIcon.Visible = true;

            CreateContextMenu();
            ShowMainWindow();

        }

        private void CreateContextMenu()
        {
            _notifyIcon.ContextMenuStrip =
              new System.Windows.Forms.ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add("Open").Click += (s, e) => ShowMainWindow();
            _notifyIcon.ContextMenuStrip.Items.Add("Exit").Click += (s, e) => ExitApplication();
        }

        private void ExitApplication()
        {
            _isExit = true;
            MainWindow.Close();
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }

        private void ShowMainWindow()
        {
            if (MainWindow.IsVisible)
            {
                if (MainWindow.WindowState == WindowState.Minimized)
                {
                    MainWindow.WindowState = WindowState.Normal;
                }
                MainWindow.Activate();
            }
            else
            {
                MainWindow.Show();
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (!_isExit)
            {
                e.Cancel = true;
                MainWindow.Hide();
            }
        }



    }

    public partial class MainWindow : Window
    {
        private string txtPlaceholder = "Placeholder for data from Clipboard";
        private bool isViewing;
        private HwndSource hWndSource;
        private IntPtr hWndNextViewer;

        public MainWindow()
        {
            InitializeComponent();

        }


        #region Win32 Clipboard handlers
        internal static class Win32
        {
            internal const int WM_DRAWCLIPBOARD = 0x0308;
            internal const int WM_CHANGECBCHAIN = 0x030D;

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            internal static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            internal static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            internal static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
        }
        #endregion

        #region Button Handlers
        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            txtBox.Text = txtPlaceholder;
            Clipboard.Clear();
        }

        private void GetBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowClipboard();
        }

        private void SwitchBtn_Click(object sender, RoutedEventArgs e)
        {
            StartViewing();
        }
        #endregion

        /*
        [ComImport(), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214e8-0000-0000-c000-000000000046")]
        internal interface IShellExtInit
        {
            void Initialize(
                IntPtr pidlFolder,
                IntPtr pDataObj,
                IntPtr  hKeyProgID);
        }
        [ComImport(), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214e4-0000-0000-c000-000000000046")]
        internal interface IContextMenu
        {
            [PreserveSig]
            int QueryContextMenu(
                IntPtr  hMenu,
                uint iMenu,
                uint idCmdFirst,
                uint idCmdLast,
                uint uFlags);
            void InvokeCommand(IntPtr pici);
            void GetCommandString(
                UIntPtr idCmd,
                uint uFlags,
                IntPtr pReserved,
                System.Text.StringBuilder pszName,
                uint cchMax);
        }

        [ClassInterface(ClassInterfaceType.None)]
        [Guid("B1F1405D-94A1-4692-B72F-FC8CAF8B8700"), ComVisible(true)]
        public class FileContextMenuExt : IShellExtInit, IContextMenu
        {
            public void Initialize(IntPtr pidlFolder, IntPtr pDataObj, IntPtr hKeyProgID)
            {
        
    }

            public int QueryContextMenu(IntPtr hMenu, uint iMenu, uint idCmdFirst, uint idCmdLast, uint uFlags)
            {
        
            }
            public void InvokeCommand(IntPtr pici)
            {
        
            }
            public void GetCommandString(
                UIntPtr idCmd,
                uint uFlags,
                IntPtr pReserved,
                System.Text.StringBuilder pszName,
                uint cchMax)
            {
            }
        }

*/
        protected virtual void ShowClipboard()
        {

            var CopiedText = txtPlaceholder;
            CopiedText = Clipboard.GetText(); ;
            txtBox.Text = CopiedText;


            if (!string.IsNullOrWhiteSpace(CopiedText))
            {
                txtBox.Text = CopiedText;
            }
            else
            {
                txtBox.Text = txtPlaceholder;
            }

        }

        static Array parseText(string text)
        {
            string[] elements;
            elements = text.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);
            return elements;

        }

        private void InitCBViewer()
        {
            WindowInteropHelper wih = new WindowInteropHelper(this);
            hWndSource = HwndSource.FromHwnd(wih.Handle);

            hWndSource.AddHook(this.WinProc);
            hWndNextViewer = Win32.SetClipboardViewer(hWndSource.Handle);
            isViewing = true;
        }

        private IntPtr WinProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case Win32.WM_CHANGECBCHAIN:
                    if (wParam == hWndNextViewer)
                    {
                        hWndNextViewer = lParam;
                    }
                    else if (hWndNextViewer != IntPtr.Zero)
                    {
                        Win32.SendMessage(hWndNextViewer, msg, wParam, lParam);
                    }
                    break;

                case Win32.WM_DRAWCLIPBOARD:
                    this.DrawContent();
                    Win32.SendMessage(hWndNextViewer, msg, wParam, lParam);
                    break;
            }

            return IntPtr.Zero;
        }

        private void DrawContent()
        {
            pnlContent.Children.Clear();

            if (Clipboard.ContainsText())
            {
                TextBox tb = new TextBox();
                tb.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                tb.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                tb.Text = Clipboard.GetText();
                tb.IsReadOnly = true;
                tb.TextWrapping = TextWrapping.NoWrap;
                
                pnlContent.Children.Add(tb);
            }
            /*else if (Clipboard.ContainsFileDropList())
            {
                // we have a file drop list in the clipboard
                ListBox lb = new ListBox();
                lb.ItemsSource = Clipboard.GetFileDropList();
                pnlContent.Children.Add(lb);
            }
            else if (Clipboard.ContainsImage())
            {
                // Because of a known issue in WPF,
                // we have to use a workaround to get correct
                // image that can be displayed.
                // The image have to be saved to a stream and then 
                // read out to workaround the issue.
                MemoryStream ms = new MemoryStream();
                BmpBitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(Clipboard.GetImage()));
                enc.Save(ms);
                ms.Seek(0, SeekOrigin.Begin);

                BmpBitmapDecoder dec = new BmpBitmapDecoder(ms,
                    BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

                Image img = new Image();
                img.Stretch = Stretch.Uniform;
                img.Source = dec.Frames[0];
                pnlContent.Children.Add(img);
            }*/
            else
            {
                Label lb = new Label();
                lb.Content = "The type of the data in the clipboard is not supported.";
                pnlContent.Children.Add(lb);
            }
        }

        private void StartViewing()
        {
            if (!isViewing)
            {
                this.InitCBViewer();
                SwitchBtn.Content = "Stop";
            }
            else
            {
                this.CloseCBViewer();
                SwitchBtn.Content = "Start";
            }
        }




        private void CloseCBViewer()
        {
            // remove this window from the clipboard viewer chain
            Win32.ChangeClipboardChain(hWndSource.Handle, hWndNextViewer);

            hWndNextViewer = IntPtr.Zero;
            hWndSource.RemoveHook(this.WinProc);
            pnlContent.Children.Clear();
            isViewing = false;
        }
    }
}


