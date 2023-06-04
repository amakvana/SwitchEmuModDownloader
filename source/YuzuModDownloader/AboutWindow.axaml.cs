using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Reflection;

namespace YuzuModDownloader
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();

            // set form values
            this.Title = $"About {AssemblyTitle}";
            LblProductName.Content = AssemblyProduct;
            LblVersion.Content = $"Version: {AssemblyVersion}";
            LblCopyright.Content = AssemblyCopyright;
            LblCompanyName.Content = AssemblyCompany;
            TxtDescription.Text = string.Format(
                "Yuzu Mod Downloader{0}" +
                "A One-Click Yuzu Mod downloader for Switch Game Mods (Unofficial) by amakvana.{0}{0}" +
                "https://github.com/amakvana/YuzuModDownloader{0}{0}" +
                "This software is licensed under GNU GPL-3.0.{0}" +
                "Source code is available in repository above.{0}{0}" +
                "Credits: https://github.com/amakvana/YuzuModDownloader#acknowledgements{0}{0}" +
                "Disclaimer:{0}This software comes with no warranty, express or implied nor does the author makes no representation of warranties. The author claims no responsibility for damages resulting from any use or misuse of the software."
                , Environment.NewLine);
        }

        public void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #region Assembly Attribute Accessors

        public static string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);
            }
        }

        public static string AssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version?.ToString()!;

        public static string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public static string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public static string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public static string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
        #endregion

    }
}
