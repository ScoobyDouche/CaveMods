using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace NexusMods.App.UI.Pages.Mods;

public partial class ModsView : ReactiveUserControl<IModsViewModel>
{
    public ModsView()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            this.WhenAnyValue(view => view.ViewModel!.Downloaded)
                .Subscribe(groups =>
                {
                    DownloadedItemsControl.ItemsSource = groups;
                    DownloadedEmpty.IsVisible = groups.Count == 0;
                })
                .DisposeWith(d);

            this.WhenAnyValue(view => view.ViewModel!.Installed)
                .Subscribe(groups =>
                {
                    InstalledItemsControl.ItemsSource = groups;
                    InstalledEmpty.IsVisible = groups.Count == 0;
                })
                .DisposeWith(d);
        });
    }
}
