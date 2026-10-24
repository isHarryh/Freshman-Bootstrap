using System.Windows;

namespace FreshmanBootstrap.App.Presentation.ViewModels;

public sealed class PostProcessItemViewModel
{
    public PostProcessItemViewModel(string description, bool isChecked)
    {
        Description = description;
        IsChecked = isChecked;
    }

    public string Description { get; }

    public bool IsChecked { get; }

    public Visibility CheckedVisibility => IsChecked ? Visibility.Visible : Visibility.Collapsed;

    public Visibility UncheckedVisibility => IsChecked ? Visibility.Collapsed : Visibility.Visible;
}
