using System.Windows;
using FreshmanBootstrap.Core;
using FreshmanBootstrap.Core.Models;
using FreshmanBootstrap.Core.Services;
using Wpf.Ui.Controls;

namespace FreshmanBootstrap.App.Presentation.Views;

public partial class PathRepairDialog : FluentWindow
{
    public PathRepairDialog(IReadOnlyList<PathRepairPlan> plans)
    {
        InitializeComponent();

        Title = Strings.PathRepairDialog.Title;
        DialogTitleBar.Title = Strings.PathRepairDialog.Title;
        SummaryText.Text = Strings.PathRepairDialog.Summary;
        HeaderText.Text = Strings.PathRepairDialog.Header;
        ConfirmButton.Content = Strings.PathRepairDialog.Confirm;
        CancelButton.Content = Strings.Common.Cancel;

        ShowSections(PathRepairPreview.Build(plans));
    }

    private void ShowSections(IReadOnlyList<PathRepairSection> sections)
    {
        if (sections.Count > 1)
        {
            SectionTabs.ItemsSource = sections.Select(ToDisplay).ToList();
            SectionTabs.Visibility = Visibility.Visible;
            SingleSection.Visibility = Visibility.Collapsed;
            return;
        }

        SectionTabs.Visibility = Visibility.Collapsed;
        SingleSection.Visibility = Visibility.Visible;
        SingleSection.Content = sections.Count == 1 ? ToDisplay(sections[0]) : null;
    }

    private static PathSectionDisplay ToDisplay(PathRepairSection section)
    {
        return new PathSectionDisplay(
            section.Level == EnvironmentLevel.Machine
                ? Strings.PathRepairDialog.MachineLevel
                : Strings.PathRepairDialog.UserLevel,
            section.Changes.Select(ToDisplay).ToList());
    }

    private static PathChangeDisplay ToDisplay(PathChange change)
    {
        return new PathChangeDisplay(
            change.Kind,
            DescribeKind(change.Kind),
            DescribeGlyph(change.Kind),
            change.Entry);
    }

    private static string DescribeKind(PathChangeKind kind)
    {
        return kind switch
        {
            PathChangeKind.Add => Strings.PathRepairDialog.KindAdd,
            PathChangeKind.Move => Strings.PathRepairDialog.KindMove,
            PathChangeKind.Remove => Strings.PathRepairDialog.KindRemove,
            _ => kind.ToString(),
        };
    }

    private static string DescribeGlyph(PathChangeKind kind)
    {
        return kind switch
        {
            PathChangeKind.Add => "+",
            PathChangeKind.Move => "→",
            PathChangeKind.Remove => "−",
            _ => "•",
        };
    }

    private void OnConfirmClick(object sender, RoutedEventArgs e) => DialogResult = true;

    private void OnCancelClick(object sender, RoutedEventArgs e) => DialogResult = false;
}

public sealed record PathChangeDisplay(
    PathChangeKind Kind,
    string KindLabel,
    string Glyph,
    string Entry);

public sealed record PathSectionDisplay(
    string LevelLabel,
    IReadOnlyList<PathChangeDisplay> Changes);
