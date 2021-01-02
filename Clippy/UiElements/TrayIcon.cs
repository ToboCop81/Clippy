/// Clippy - File: "TrayIcon.cs"
/// Copyright © 2020 by Tobias Zorn
/// Licensed under GNU GENERAL PUBLIC LICENSE

using Clippy.Common;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Drawing = System.Drawing;
using Forms = System.Windows.Forms;

namespace Clippy.UiElements
{
    [ContentProperty("Text")]
    [DefaultEvent("MouseDoubleClick")]
    public partial class TrayIcon : FrameworkElement, IAddChild
    {
        public static readonly RoutedEvent MouseClickEvent = EventManager.RegisterRoutedEvent(
            "MouseClick", RoutingStrategy.Bubble, typeof(MouseButtonEventHandler),typeof(TrayIcon));

        public static readonly RoutedEvent MouseDoubleClickEvent = EventManager.RegisterRoutedEvent(
            "MouseDoubleClick",RoutingStrategy.Bubble, typeof(MouseButtonEventHandler), typeof(TrayIcon));

        public static readonly DependencyProperty BalloonTipIconProperty =
            DependencyProperty.Register("BalloonTipIcon", typeof(BalloonTipIcon), typeof(TrayIcon));

        public static readonly DependencyProperty BalloonTipTextProperty =
            DependencyProperty.Register("BalloonTipText", typeof(string), typeof(TrayIcon));

        public static readonly DependencyProperty BalloonTipTitleProperty =
            DependencyProperty.Register("BalloonTipTitle", typeof(string), typeof(TrayIcon));

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            "Icon", typeof(ImageSource), typeof(TrayIcon), new FrameworkPropertyMetadata(OnIconChanged));

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",typeof(string), typeof(TrayIcon), new PropertyMetadata(OnTextChanged));

        private Forms.NotifyIcon _trayIcon;

        static TrayIcon()
        {
            VisibilityProperty.OverrideMetadata(typeof(TrayIcon), new PropertyMetadata(OnVisibilityChanged));
        }

        public BalloonTipIcon BalloonTipIcon
        {
            get { return (BalloonTipIcon)GetValue(BalloonTipIconProperty); }
            set { SetValue(BalloonTipIconProperty, value); }
        }

        public string BalloonTipText
        {
            get { return (string)GetValue(BalloonTipTextProperty); }
            set { SetValue(BalloonTipTextProperty, value); }
        }

        public string BalloonTipTitle
        {
            get { return (string)GetValue(BalloonTipTitleProperty); }
            set { SetValue(BalloonTipTitleProperty, value); }
        }

        public event MouseButtonEventHandler MouseClick
        {
            add { AddHandler(MouseClickEvent, value); }
            remove { RemoveHandler(MouseClickEvent, value); }
        }

        public event MouseButtonEventHandler MouseDoubleClick
        {
            add { AddHandler(MouseDoubleClickEvent, value); }
            remove { RemoveHandler(MouseDoubleClickEvent, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public override void BeginInit()
        {
            base.BeginInit();
            InitTrayIcon();
        }

        public void ShowBalloonTip(int timeout)
        {
            _trayIcon.BalloonTipTitle = BalloonTipTitle;
            _trayIcon.BalloonTipText = BalloonTipText;
            _trayIcon.BalloonTipIcon = (Forms.ToolTipIcon)BalloonTipIcon;
            _trayIcon.ShowBalloonTip(timeout);
        }

        public void ShowBalloonTip(int timeout, string tipTitle, string tipText, BalloonTipIcon tipIcon)
        {
            _trayIcon.ShowBalloonTip(timeout, tipTitle, tipText, (Forms.ToolTipIcon)tipIcon);
        }

        #region IAddChild Members

        void IAddChild.AddChild(object value)
        {
            throw new InvalidOperationException();
        }

        void IAddChild.AddText(string text)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }

        #endregion

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.Closed += (s, a) => _trayIcon.Dispose();
            }
        }

        private static MouseButtonEventArgs CreateMouseButtonEventArgs(RoutedEvent handler, Forms.MouseButtons button)
        {
            return new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MapMouseButtons(button))
            {
                RoutedEvent = handler
            };
        }

        private static Drawing.Icon FromImageSource(ImageSource icon)
        {
            if (icon == null) return null;

            var iconUri = new Uri(icon.ToString());
            return new Drawing.Icon(Application.GetResourceStream(iconUri).Stream);
        }

        private static void OnIconChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(target))
            {
                TrayIcon control = (TrayIcon)target;
                control._trayIcon.Icon = FromImageSource(control.Icon);
            }
        }

        private static void OnTextChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TrayIcon control = (TrayIcon)target;
            control._trayIcon.Text = control.Text;
        }

        private static void OnVisibilityChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TrayIcon control = (TrayIcon)target;
            control._trayIcon.Visible = control.Visibility == Visibility.Visible;
        }

        private static MouseButton MapMouseButtons(Forms.MouseButtons button)
        {
            switch (button)
            {
                case Forms.MouseButtons.Left:
                    return MouseButton.Left;
                case Forms.MouseButtons.Right:
                    return MouseButton.Right;
                case Forms.MouseButtons.Middle:
                    return MouseButton.Middle;
                case Forms.MouseButtons.XButton1:
                    return MouseButton.XButton1;
                case Forms.MouseButtons.XButton2:
                    return MouseButton.XButton2;
            }

            throw new InvalidOperationException();
        }

        private void InitTrayIcon()
        {
            _trayIcon = new Forms.NotifyIcon();
            _trayIcon.Text = Text;
            _trayIcon.Icon = FromImageSource(Icon);
            _trayIcon.Visible = Visibility == Visibility.Visible;

            _trayIcon.MouseDown += OnMouseDown;
            _trayIcon.MouseUp += OnMouseUp;
            _trayIcon.MouseClick += OnMouseClick;
            _trayIcon.MouseDoubleClick += OnMouseDoubleClick;

            InitializeNativeHooks();
        }

        private void OnMouseDown(object sender, Forms.MouseEventArgs e)
        {
            RaiseEvent(CreateMouseButtonEventArgs(MouseDownEvent, e.Button));
        }

        private void OnMouseDoubleClick(object sender, Forms.MouseEventArgs e)
        {
            RaiseEvent(CreateMouseButtonEventArgs(MouseDoubleClickEvent, e.Button));
        }

        private void OnMouseClick(object sender, Forms.MouseEventArgs e)
        {
            RaiseEvent(CreateMouseButtonEventArgs(MouseClickEvent, e.Button));
        }

        private void OnMouseUp(object sender, Forms.MouseEventArgs e)
        {
            if (e.Button == Forms.MouseButtons.Right)
            {
                ShowContextMenu();
            }

            RaiseEvent(CreateMouseButtonEventArgs(MouseUpEvent, e.Button));
        }

        private void ShowContextMenu()
        {
            if (ContextMenu != null)
            {
                AttachContextMenu();
                ContextMenu.IsOpen = true;
            }
        }

        partial void AttachContextMenu();

        partial void InitializeNativeHooks();
    }
}