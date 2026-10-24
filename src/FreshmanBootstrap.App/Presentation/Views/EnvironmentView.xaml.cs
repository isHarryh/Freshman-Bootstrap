using System.Windows.Controls;
using FreshmanBootstrap.Core;

namespace FreshmanBootstrap.App.Presentation.Views;

public partial class EnvironmentView : UserControl
{
    public EnvironmentView()
    {
        InitializeComponent();

        NameColumn.Header = Strings.EnvironmentPage.Name;
        UsageColumn.Header = Strings.EnvironmentPage.Usage;
        PackagedVersionColumn.Header = Strings.EnvironmentPage.PackagedVersion;
        InstalledVersionColumn.Header = Strings.EnvironmentPage.InstalledVersion;
        StatusColumn.Header = Strings.EnvironmentPage.Status;
        ActionColumn.Header = Strings.EnvironmentPage.Actions;
    }
}
