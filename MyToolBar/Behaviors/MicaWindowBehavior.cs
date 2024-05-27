﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using Microsoft.Xaml.Behaviors;
using MyToolBar.WinApi;

namespace MyToolBar.Behaviors
{

    public class MicaWindowBehavior : Behavior<Window>
    {
        static readonly Dictionary<Window, WindowAccentCompositor> s_allWindowsAccentCompositors = new();

        static MicaWindowBehavior()
        {
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
        }

        private WindowAccentCompositor? _windowAccentCompositor;

        private static void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            var isDarkMode = !ToolWindowApi.GetIsLightTheme();

            foreach (var wac in s_allWindowsAccentCompositors.Values)
            {
                UpdateWindowBlurMode(wac, isDarkMode);
            }
        }

        private static void UpdateWindowBlurMode(WindowAccentCompositor wac, bool isDarkMode, float opacity = .7f)
        {
            wac.DarkMode = isDarkMode;
            wac.Color = isDarkMode ?
                Color.FromScRgb(opacity, 0, 0, 0) :
                Color.FromScRgb(opacity, 1, 1, 1);
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject.IsLoaded)
            {
                InitializeBehavior();
            }
            else
            {
                AssociatedObject.Loaded += AssociatedObject_Loaded;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            // unregister event
            AssociatedObject.Loaded -= AssociatedObject_Loaded;

            // remove and disable wac
            if (s_allWindowsAccentCompositors.TryGetValue(AssociatedObject, out var acc))
            {
                s_allWindowsAccentCompositors.Remove(AssociatedObject);

                acc.IsEnabled = false;
            }
        }

        private void InitializeBehavior()
        {
            s_allWindowsAccentCompositors[AssociatedObject]
                = _windowAccentCompositor
                = CreateWindowAccentCompositor();
        }

        private WindowAccentCompositor CreateWindowAccentCompositor()
        {
            var isDarkMode = !ToolWindowApi.GetIsLightTheme();
            var wac = new WindowAccentCompositor(
                AssociatedObject, false, (c) =>
                {
                    c.A = 255;
                    AssociatedObject.Background = new SolidColorBrush(c);
                });

            UpdateWindowBlurMode(wac, isDarkMode, Opacity);
            wac.IsEnabled = true;

            return wac;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeBehavior();
        }


        private static void OpacityChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not MicaWindowBehavior behavior ||
                e.NewValue is not float opacity)
                return;

            if (behavior._windowAccentCompositor is null)
                return;

            var color = behavior.WindowAccentCompositor.Color;
            color.ScA = opacity;

            behavior.WindowAccentCompositor.Color = color;
            behavior.WindowAccentCompositor.IsEnabled = true;
        }


        public float Opacity
        {
            get { return (float)GetValue(OpacityProperty); }
            set { SetValue(OpacityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Opacity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OpacityProperty =
            DependencyProperty.Register(nameof(Opacity), typeof(float), typeof(MicaWindowBehavior), new PropertyMetadata(1.0f, OpacityChangedCallback));

        public WindowAccentCompositor WindowAccentCompositor => _windowAccentCompositor ?? throw new InvalidOperationException("Window is not loaded");
    }
}
