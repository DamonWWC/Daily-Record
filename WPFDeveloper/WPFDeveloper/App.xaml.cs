﻿using System.Configuration;
﻿using System.Data;
﻿using System.Windows;
﻿using Microsoft.Extensions.DependencyInjection;
﻿using WPFDeveloper.Services;
﻿using WPFDeveloper.ViewModels;
﻿using WPFDeveloper.Views;

﻿namespace WPFDeveloper;

﻿/// <summary>
﻿/// Interaction logic for App.xaml
﻿/// </summary>
﻿public partial class App : Application
﻿{
﻿    private readonly ServiceProvider _serviceProvider;

﻿    public App()
﻿    {
﻿        var services = new ServiceCollection();
﻿        ConfigureServices(services);
﻿        _serviceProvider = services.BuildServiceProvider();
﻿    }

﻿    private void ConfigureServices(IServiceCollection services)
﻿    {
﻿        services.AddSingleton<INavigationService, NavigationService>();
﻿        services.AddSingleton<MainViewModel>();
﻿        services.AddSingleton<MainWindow>();
        
﻿        // 注册页面
﻿        services.AddTransient<HomePage>();
﻿        services.AddTransient<SettingsPage>();
﻿        services.AddTransient<AboutPage>();
﻿    }

﻿    protected override void OnStartup(StartupEventArgs e)
﻿    {
﻿        var navigationService = _serviceProvider.GetRequiredService<INavigationService>();
﻿        navigationService.Configure("Home", typeof(HomePage));
﻿        navigationService.Configure("Settings", typeof(SettingsPage));
﻿        navigationService.Configure("About", typeof(AboutPage));

﻿        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
﻿        mainWindow.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();
﻿        mainWindow.Show();
        
﻿        base.OnStartup(e);
﻿    }
﻿}
