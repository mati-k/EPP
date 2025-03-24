using Avalonia.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;
using EPP.Services;
using EPP.ViewModels;

namespace EPP.Views;

public partial class EditorView : UserControl
{
    public EditorView()
    {
        InitializeComponent();
    }

    private async void ListBoxItem_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        var gfxService = Ioc.Default.GetService<IGfxService>();
        var viewModel = DataContext as EditorViewModel;

        ListBoxItem? listboxItem = sender as ListBoxItem;
        string? picture = listboxItem?.DataContext as string;


        if (picture != null && viewModel != null && gfxService != null && gfxService.HasVariants(picture))
        {
            e.Handled = true;
            await viewModel.ShowVariantDialog(picture);
        }
    }
}