using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using NexusMods.Abstractions.Serialization.Attributes;
using NexusMods.App.UI.WorkspaceSystem;
using NexusMods.UI.Sdk.Icons;

namespace NexusMods.App.UI.Pages.Mods;

[JsonName("CaveMods.App.UI.Pages.ModsPageContext")]
public record ModsPageContext : IPageFactoryContext;

[UsedImplicitly]
public class ModsPageFactory : APageFactory<IModsViewModel, ModsPageContext>
{
    public ModsPageFactory(IServiceProvider serviceProvider) : base(serviceProvider) { }

    public static readonly PageFactoryId StaticId = PageFactoryId.From(Guid.Parse("5B0E7C2A-9D41-4C8E-A3F6-2E7D19C4B8A1"));
    public override PageFactoryId Id => StaticId;

    public override IModsViewModel CreateViewModel(ModsPageContext context)
    {
        return ServiceProvider.GetRequiredService<IModsViewModel>();
    }

    public override IEnumerable<PageDiscoveryDetails?> GetDiscoveryDetails(IWorkspaceContext workspaceContext)
    {
        if (workspaceContext is not HomeContext) yield break;

        yield return new PageDiscoveryDetails
        {
            SectionName = "Mods",
            ItemName = "Mods",
            Icon = IconValues.ModLibrary,
            PageData = new PageData
            {
                FactoryId = Id,
                Context = new ModsPageContext(),
            },
        };
    }
}
