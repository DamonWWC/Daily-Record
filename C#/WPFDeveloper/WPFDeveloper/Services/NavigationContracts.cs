using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WPFDeveloper.Models;

namespace WPFDeveloper.Services
{
    public interface INavigationRegistry
    {
        Task<IReadOnlyList<NavigationItem>> GetNavigationItemsAsync();
    }

    public interface INavigationService
    {
        Type ResolveViewType(string key);
    }

    public interface IViewFactory
    {
        object CreateView(Type viewType);
    }
}


