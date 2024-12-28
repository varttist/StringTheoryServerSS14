using Content.Shared.Clothing;
using Robust.Shared.GameObjects;
using Robust.Shared.Player;
using Content.Shared.Inventory;

namespace Content.Shared._ST14.NightVisionLight;

public sealed class NightVisionLightSystem : EntitySystem
{
    [Dependency] private readonly SharedPointLightSystem _light = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NightVisionLightComponent, ClothingGotEquippedEvent>(OnPlayerEquipped);
        SubscribeLocalEvent<NightVisionLightComponent, ClothingGotUnequippedEvent>(OnPlayerUnequipped);
        SubscribeLocalEvent<InventoryComponent, NightVisionLightSwitchEvent>(OnSwitchLight);
    }

    private void OnPlayerEquipped(EntityUid uid, NightVisionLightComponent component, ClothingGotEquippedEvent args)
    {
        Logger.Error("Att");
        SwitchLight(uid, true);
    }

    private void OnPlayerUnequipped(EntityUid uid, NightVisionLightComponent component, ClothingGotUnequippedEvent args)
    {
        Logger.Error("Dett");
        SwitchLight(uid, false);
    }

    private void OnSwitchLight(EntityUid uid, InventoryComponent component, NightVisionLightSwitchEvent args)
    {
        Logger.Error("Switch event");
        if (_inventorySystem.TryGetContainerSlotEnumerator(uid, out var containerSlotEnumerator))
        {
            while (containerSlotEnumerator.NextItem(out var item, out var _))
            {
                if (!TryComp<NightVisionLightComponent>(item, out var _))
                    continue;

                SwitchLight(item, args.active);
            }
        }
    }

    private void SwitchLight(EntityUid ent, bool enabled)
    {
        if (ent == null)
        {
            return;
        }

        _light.SetEnabled(ent, enabled);
    }
}
