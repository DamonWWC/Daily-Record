using Avalonia.Controls;
using CefNet.Avalonia;

namespace AvaloniaApplication2.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        //WebViewControl.WebView webView = new WebViewControl.WebView();
        //Content = webView;
        //webView.LoadUrl("http://dev-k8s.pcitech.com:31114//hjmos-lnc-web/#/H5Page/h5Panorama");
        WebView webview = new() { Focusable = true };
        Content = webview;

        webview.BrowserCreated += (s, e) => webview.Navigate("http://dev-k8s.pcitech.com:31114//hjmos-lnc-web/#/H5Page/h5Panorama");

        webview.DocumentTitleChanged += (s, e) => Title = e.Title;

    }

   
}
