﻿﻿﻿using System.Windows;
﻿﻿﻿using Microsoft.Extensions.DependencyInjection;
﻿﻿﻿using WPFDeveloper.ViewModels;
﻿﻿﻿using WPFDeveloper.Services;

﻿﻿namespace WPFDeveloper
﻿﻿{
﻿﻿    public partial class MainWindow : Window
﻿﻿    {
﻿﻿        public MainWindow()
﻿﻿        {
﻿﻿            InitializeComponent();
            
﻿﻿            // 从DI容器获取服务
﻿﻿            var navigationService = App.Current.Services.GetRequiredService<INavigationService>();
﻿﻿            var viewModel = App.Current.Services.GetRequiredService<MainViewModel>();
            
﻿﻿            // 设置Frame控件给NavigationService
﻿﻿            if (navigationService is NavigationService navService)
﻿﻿            {
﻿﻿                navService.SetFrame(MainFrame);
﻿﻿            }

﻿﻿            DataContext = viewModel;
﻿﻿        }
﻿﻿    }
﻿﻿}