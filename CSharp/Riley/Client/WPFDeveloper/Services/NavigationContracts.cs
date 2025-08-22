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
}


