using System;

namespace WPFDeveloper.Services
{
    public interface INavigationService
    {
        void NavigateTo(string pageKey);
        void Configure(string key, Type pageType);
    }
}