using Avalonia.Controls;
using CefNet;
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
         
        CefFrame main = webview.GetMainFrame();

        //webview.BrowserCreated += (s, e) => webview.Navigate("http://dev-k8s.pcitech.com:31114//hjmos-lnc-web/#/H5Page/h5Panorama");

        webview.BrowserCreated += (s, e) => webview.Navigate("https://www.bilibili.com/video/BV1Xr421s79L/?spm_id_from=333.1007.tianma.1-1-1.click");

        webview.DocumentTitleChanged += (s, e) => Title = e.Title;

    }

   
}
