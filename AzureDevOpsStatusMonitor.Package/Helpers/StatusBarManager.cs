using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AzureDevOpsStatusMonitor.Helpers
{
    internal class StatusBarManager
    {
        private readonly Window _mainWindow;

        private FrameworkElement _statusBar;

        private Panel _panel;

        public StatusBarManager(Window pMainWindow)
        {
            _mainWindow = pMainWindow;
            _mainWindow.Initialized += MainWindowInitialized;

            FindStatusBar();
        }

        private static DependencyObject FindChild(DependencyObject parent, string childName)
        {
            if (parent == null)
            {
                return null;
            }

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is FrameworkElement frameworkElement && frameworkElement.Name == childName)
                {
                    return frameworkElement;
                }

                child = FindChild(child, childName);

                if (child != null)
                {
                    return child;
                }
            }

            return null;
        }

        private void FindStatusBar()
        {
            _statusBar = FindChild(_mainWindow, "StatusBarContainer") as FrameworkElement;
            var frameworkElement = _statusBar;

            if (frameworkElement != null)
            {
                _panel = frameworkElement.Parent as DockPanel;
            }
        }

        private void RefindStatusBar()
        {
            if (_panel == null)
            {
                FindStatusBar();
            }
        }

        public void InjectControl(FrameworkElement pControl)
        {
            RefindStatusBar();

            _panel?.Dispatcher.Invoke(() =>
            {
                pControl.SetValue(DockPanel.DockProperty, Dock.Right);
                _panel.Children.Insert(1, pControl);
            });
        }

        public bool IsInjected(FrameworkElement pControl)
        {
            RefindStatusBar();

            var flag = false;

            _panel?.Dispatcher.Invoke(() =>
            {
                flag = _panel.Children.Contains(pControl);
            });

            return flag;
        }

        public void UninjectControl(FrameworkElement pControl)
        {
            RefindStatusBar();

            _panel?.Dispatcher.Invoke(() => _panel.Children.Remove(pControl));
        }

        private void MainWindowInitialized(object sender, EventArgs e)
        {
        }
    }
}
