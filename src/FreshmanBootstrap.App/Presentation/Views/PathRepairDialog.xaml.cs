using System.Windows;
using FreshmanBootstrap.Core;
using FreshmanBootstrap.Core.Models;
using Wpf.Ui.Controls;

namespace FreshmanBootstrap.App.Presentation.Views;

public partial class PathRepairDialog : FluentWindow
{
    public PathRepairDialog(IReadOnlyList<PathRepairPlan> plans)
    {
        InitializeComponent();

        Title = Strings.PathRepairDialog.Title;
        DialogTitleBar.Title = Strings.PathRepairDialog.Title;
        HeaderText.Text = Strings.PathRepairDialog.Header;
        ConfirmButton.Content = Strings.PathRepairDialog.Confirm;
        CancelButton.Content = Strings.Common.Cancel;

        PlansList.ItemsSource = plans.Select(ToDisplay).ToList();
    }

    private static PathRepairPlanDisplay ToDisplay(PathRepairPlan plan)
    {
        return new PathRepairPlanDisplay(
            plan.Level == EnvironmentLevel.Machine
                ? Strings.PathRepairDialog.MachineLevel
                : Strings.PathRepairDialog.UserLevel,
            string.Format(Strings.PathRepairDialog.CurrentValueFormat, plan.Original.RawValue),
            string.Format(Strings.PathRepairDialog.NewValueFormat, plan.NewValue),
            plan.Changes
                .Select(change => string.Format(
                    Strings.PathRepairDialog.ChangeFormat,
                    DescribeKind(change.Kind),
                    change.Entry,
                    DescribeReason(change.Reason)))
                .ToList());
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

    private static string DescribeReason(PathChangeReason reason)
    {
        return reason switch
        {
            PathChangeReason.MissingFromPath => Strings.PathRepairDialog.ReasonMissing,
            PathChangeReason.LowerPriority => Strings.PathRepairDialog.ReasonLowerPriority,
            PathChangeReason.WindowsAppsPriority => Strings.PathRepairDialog.ReasonWindowsApps,
            PathChangeReason.Duplicate => Strings.PathRepairDialog.ReasonDuplicate,
            _ => reason.ToString(),
        };
    }

    private void OnConfirmClick(object sender, RoutedEventArgs e) => DialogResult = true;

    private void OnCancelClick(object sender, RoutedEventArgs e) => DialogResult = false;
}

public sealed record PathRepairPlanDisplay(
    string HeaderText,
    string CurrentText,
    string NewText,
    IReadOnlyList<string> Changes);
