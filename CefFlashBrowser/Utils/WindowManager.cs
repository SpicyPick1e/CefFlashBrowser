﻿using CefFlashBrowser.Views;
using CefFlashBrowser.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Windows;

namespace CefFlashBrowser.Utils
{
    public static class WindowManager
    {
        private static readonly Dictionary<Type, Window> _windows;

        static WindowManager()
        {
            _windows = new Dictionary<Type, Window>();
        }

        public static TWindow ShowWindow<TWindow>(bool modal = false, bool save = false, Action<TWindow> initializer = null) where TWindow : Window, new()
        {
            TWindow window = null;

            if (save)
            {
                if (_windows.ContainsKey(typeof(TWindow)))
                {
                    window = (TWindow)_windows[typeof(TWindow)];
                    window.WindowState = window.WindowState == WindowState.Minimized ? WindowState.Normal : window.WindowState;
                    window.Activate();
                    return window;
                }
                else
                {
                    window = new TWindow();
                    window.Closing += (s, e) => _windows.Remove(s.GetType());
                    _windows.Add(typeof(TWindow), window);
                }
            }
            else
            {
                window = new TWindow();
            }

            if (initializer != null)
            {
                window.SourceInitialized += WindowSourceInitializedHandler;
                void WindowSourceInitializedHandler(object sender, EventArgs e)
                {
                    initializer((TWindow)sender);
                    ((TWindow)sender).SourceInitialized -= WindowSourceInitializedHandler;
                }
            }

            if (modal)
            {
                window.ShowDialog();
            }
            else
            {
                window.Show();
            }

            return window;
        }

        public static void ShowMainWindow()
        {
            ShowWindow<MainWindow>(save: true);
        }

        public static BrowserWindow ShowBrowser(string address)
        {
            return ShowWindow<BrowserWindow>(initializer: window => window.browser.Address = address);
        }

        public static SwfPlayerWindow ShowSwfPlayer(string fileName)
        {
            return ShowWindow<SwfPlayerWindow>(initializer: window => window.FileName = fileName);
        }

        public static PopupWebPage ShowPopupWebPage(string address, CefSharp.IPopupFeatures popupFeatures = null)
        {
            return ShowWindow<PopupWebPage>(initializer: window =>
            {
                window.Address = address;
                if (popupFeatures != null)
                {
                    window.Left = popupFeatures.X;
                    window.Top = popupFeatures.Y;
                    window.Width = popupFeatures.Width;
                    window.Height = popupFeatures.Height;
                }
            });
        }

        public static ViewSourceWindow ShowViewSourceWindow(string address)
        {
            return ShowWindow<ViewSourceWindow>(initializer: window => window.Address = address);
        }

        public static void ShowFavoritesManager()
        {
            ShowWindow<FavoritesManager>(save: true);
        }

        public static void ShowSettingsWindow()
        {
            ShowWindow<SettingsWindow>(save: true);
        }

        public static bool ShowSelectLanguageDialog()
        {
            return ShowWindow<SelectLanguageDialog>(true).DialogResult == true;
        }

        public static bool ShowAddFavoriteDialog(string name = "", string url = "")
        {
            return ShowWindow<AddFavoriteDialog>(true, initializer: window =>
            {
                window.ItemName = name;
                window.ItemUrl = url;
                window.NameTextBox.SelectAll();
            }).DialogResult == true;
        }

        public static void Alert(string message = "", string title = "", Action<bool> callback = null)
        {
            bool result = ShowWindow<JsAlertDialog>(true, initializer: window =>
            {
                window.Title = title;
                window.Message = message;
            }).DialogResult == true;
            callback?.Invoke(result);
        }

        public static void Confirm(string message = "", string title = "", Action<bool?> callback = null)
        {
            bool? result = ShowWindow<JsConfirmDialog>(true, initializer: window =>
            {
                window.Title = title;
                window.Message = message;
            }).DialogResult;
            callback?.Invoke(result);
        }

        public static void Prompt(string message = "", string title = "", string defaultInputText = "", Action<bool?, string> callback = null)
        {
            JsPromptDialog dialog = ShowWindow<JsPromptDialog>(true, initializer: window =>
            {
                window.Title = title;
                window.Message = message;
                window.InputText = defaultInputText;
            });
            callback?.Invoke(dialog.DialogResult, dialog.InputText);
        }

        public static void ShowError(string errMsg)
        {
            MessageBox.Show(errMsg, LanguageManager.GetString("title_error"), MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
