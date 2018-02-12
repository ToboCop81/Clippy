/// Clippy - File: "TermsOfUse.xaml.cs"
/// Copyright © 2018 by Tobias Zorn
/// Licensed under GNU GENERAL PUBLIC LICENSE

using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace Clippy.UiElements
{
    /// <summary>
    /// Interaction logic for TermsOfUse.xaml
    /// </summary>
    public partial class TermsOfUseWindow : Window
    {
        public TermsOfUseWindow()
        {
            InitializeComponent();
            Assembly _Assembly = Assembly.GetExecutingAssembly();
            Stream str = _Assembly.GetManifestResourceStream("Clippy.Resources.gpl.txt");
            StreamReader rd = new StreamReader(str);
            string licenseText = rd.ReadToEnd();

            VersionLabel.Content = VersionLabel.Content.ToString().Replace("{version}", _Assembly.GetName().Version.ToString());
            LicenseTextBox.Text = licenseText;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
