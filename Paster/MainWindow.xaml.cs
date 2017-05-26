using System.ComponentModel;
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

        private void parseText(string text)
        {

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


