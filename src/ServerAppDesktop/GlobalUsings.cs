#region .NET Base Class Library (BCL)
global using global::System.Collections.ObjectModel;
global using global::System.Diagnostics;
global using global::System.Runtime.InteropServices;
global using global::System.Collections;
global using global::System.Diagnostics.CodeAnalysis;
global using global::System.Text.Json.Nodes;
#endregion

#region Windows Native & WinRT APIs
global using global::Windows.System;
global using global::WinRT;
global using global::Windows.Win32;
global using global::Windows.Win32.Foundation;
global using global::Windows.Win32.UI.WindowsAndMessaging;
#endregion

#region Windows App SDK
global using global::Microsoft.Windows.AppLifecycle;
global using global::Microsoft.Windows.AppNotifications;
global using global::Microsoft.Windows.AppNotifications.Builder;
global using global::Microsoft.Windows.Storage.Pickers;
global using global::Microsoft.Windows.Globalization;
#endregion

#region Microsoft UI XAML Core
global using global::Microsoft.UI.Xaml;
global using global::Microsoft.UI.Xaml.Controls;
global using global::Microsoft.UI.Xaml.Input;
global using global::Microsoft.UI;
global using global::Microsoft.UI.Xaml.Media;
global using global::Microsoft.UI.Xaml.Media.Animation;
global using TitleBar = global::Microsoft.UI.Xaml.Controls.TitleBar;
#endregion

#region Microsoft UI Systems (Backdrop, Windowing, Dispatching)
global using global::Microsoft.UI.Composition.SystemBackdrops;
global using global::Microsoft.UI.Dispatching;
global using global::Microsoft.UI.Windowing;
global using DispatcherQueue = global::Microsoft.UI.Dispatching.DispatcherQueue;
#endregion

#region Community Toolkit MVVM
global using global::CommunityToolkit.Mvvm.ComponentModel;
global using global::CommunityToolkit.Mvvm.Input;
global using global::CommunityToolkit.Mvvm.Messaging;
global using global::CommunityToolkit.Mvvm.Messaging.Messages;
#endregion

#region Dependency Injection & Hosting
global using global::Microsoft.Extensions.Hosting;
global using global::Microsoft.Extensions.DependencyInjection;
#endregion

#region Third Party Extensions (H, WinUIEx, CSharpEx)
global using global::H.NotifyIcon.EfficiencyMode;
global using global::WinUIEx;
global using global::CSharpEx;
#endregion

#region Server App Desktop - Infrastructure & Logic
global using global::ServerAppDesktop.Handlers;
global using global::ServerAppDesktop.Helpers;
global using global::ServerAppDesktop.Messaging;
global using global::ServerAppDesktop.Services;
#endregion

#region Server App Desktop - Data Models
global using global::ServerAppDesktop.Models;
#endregion

#region Server App Desktop - MVVM ViewModels
global using global::ServerAppDesktop.ViewModels;
#endregion

#region Server App Desktop - UI & Presentation
global using global::ServerAppDesktop.Controls;
global using global::ServerAppDesktop.Views;
global using global::ServerAppDesktop.Views.Pages;
global using global::ServerAppDesktop.Views.Pages.AboutPages;
#endregion
