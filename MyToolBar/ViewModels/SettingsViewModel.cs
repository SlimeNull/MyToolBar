﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using MyToolBar.Views.Pages;
using MyToolBar.Views.Pages.Settings;

namespace MyToolBar.ViewModels
{

    public partial class SettingsViewModel : ObservableObject
    {
        public record struct SettingsPage(string Name, Geometry Icon, Type PageType);

        public ObservableCollection<SettingsPage> SettingsPages { get; } = new()
        {
            new SettingsPage("Capsules", (Geometry)App.Current.FindResource("Icon_Capsule"), typeof(CapsulesSettingsPage)),
            new SettingsPage("Outer Controls", (Geometry)App.Current.FindResource("Icon_OuterControl"), typeof(OuterControlSettingsPage)),
            new SettingsPage("Components", (Geometry)App.Current.FindResource("Icon_Component"), typeof(ComponentsSettingsPage)),
            new SettingsPage("About", (Geometry)App.Current.FindResource("Icon_About"), typeof(AboutPage)),
        };

        [ObservableProperty]
        private SettingsPage _selectedPage;

        [ObservableProperty]
        private object? _currentPageContent;

        partial void OnSelectedPageChanged(SettingsPage value)
        {
            var scope = App.Host.Services.CreateScope();
            var pageContent = scope.ServiceProvider.GetRequiredService(value.PageType);

            CurrentPageContent = pageContent;
        }
    }
}
