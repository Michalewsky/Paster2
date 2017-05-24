using System.ComponentModel;
using System.Windows;


namespace BackgroundApplication
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();




    }


       
    void OnClick1(object sender, RoutedEventArgs e)
    {
        txtBox.Text = Clipboard.GetText();
    }

    protected virtual void OnStartup(StartupEventArgs e)
    {

        var CopiedText = "";
        CopiedText = Clipboard.GetText(); ;
        txtBox.Text = CopiedText;


        if (!string.IsNullOrWhiteSpace(CopiedText))
        {
            txtBox.Text = CopiedText;
        }
        else
        {
            txtBox.Text = "Placeholder for data from Clipboard";
        }
            
    }

    }
}
