using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WPFDeveloper.Models;
using WPFDeveloper.ViewModels;

namespace WPFDeveloper.Services
{
    public class NavigationRegistry : INavigationRegistry
    {
        private const string NavigationFile = "Resources/navigation.json";

        public async Task<IReadOnlyList<NavigationItem>> GetNavigationItemsAsync()
        {
            if (!File.Exists(NavigationFile))
            {
                return Array.Empty<NavigationItem>();
            }

            await using var stream = File.OpenRead(NavigationFile);
            var items = await JsonSerializer.DeserializeAsync<List<NavigationItem>>(stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                .ConfigureAwait(false);

            return (items ?? new List<NavigationItem>())
                .OrderBy(i => i.Order)
                .ToList();
        }
    }
}


