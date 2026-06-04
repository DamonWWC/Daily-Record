using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace WPFDeveloper.Services
{
    public class NavigationService : INavigationService
    {
        private readonly Dictionary<string, Type> _pagesByKey = new Dictionary<string, Type>();
        private Frame _mainFrame;

        public void Configure(string key, Type pageType)
        {
            if (_pagesByKey.ContainsKey(key))
            {
                _pagesByKey[key] = pageType;
            }
            else
            {
                _pagesByKey.Add(key, pageType);
            }
        }

        public void NavigateTo(string pageKey)
        {
            if (_mainFrame == null)
            {
                _mainFrame = (Frame)Application.Current.MainWindow.FindName("MainFrame");
                if (_mainFrame == null)
                {
                    throw new InvalidOperationException("Frame with name 'MainFrame' not found in MainWindow");
                }
            }

            if (!_pagesByKey.ContainsKey(pageKey))
            {
                throw new ArgumentException($"No page registered with key: {pageKey}");
            }

            var pageType = _pagesByKey[pageKey];
            _mainFrame.Navigate(Activator.CreateInstance(pageType));
        }
    }
}