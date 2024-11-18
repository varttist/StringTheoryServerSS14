using Content.Shared.Containers.ItemSlots;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.PowerCell.Components;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Wires;
using Robust.Shared.Containers;

namespace Content.Shared._ST14.Races.Android;

/// <summary>
/// This handles logic and interactions.
/// </summary>
public abstract partial class SharedAndroidSystem : EntitySystem
{
    [Dependency] protected readonly SharedContainerSystem Container = default!;
    [Dependency] protected readonly ItemSlotsSystem ItemSlots = default!;
    [Dependency] protected readonly SharedPopupSystem Popup = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AndroidComponent, ItemSlotEjectAttemptEvent>(OnItemSlotEjectAttempt);
        SubscribeLocalEvent<AndroidComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovementSpeedModifiers);
    }

    public void OnItemSlotInsertAttempt(EntityUid uid, AndroidComponent component, ItemSlotInsertAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (!TryComp<PowerCellSlotComponent>(uid, out var cellSlotComp) ||
            !TryComp<WiresPanelComponent>(uid, out var panel))
            return;

        if (!ItemSlots.TryGetSlot(uid, cellSlotComp.CellSlotId, out var cellSlot) || cellSlot != args.Slot)
            return;

        if (!panel.Open || args.User == uid)
            args.Cancelled = true;
    }

    private void OnItemSlotEjectAttempt(EntityUid uid, AndroidComponent component, ref ItemSlotEjectAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (!TryComp<PowerCellSlotComponent>(uid, out var cellSlotComp) ||
            !TryComp<WiresPanelComponent>(uid, out var panel))
            return;

        if (!ItemSlots.TryGetSlot(uid, cellSlotComp.CellSlotId, out var cellSlot) || cellSlot != args.Slot)
            return;

        if (!panel.Open || args.User == uid)
            args.Cancelled = true;
    }

    private void OnRefreshMovementSpeedModifiers(EntityUid uid, AndroidComponent component, RefreshMovementSpeedModifiersEvent args)
    {
        if (!TryComp<MovementSpeedModifierComponent>(uid, out var movement))
            return;

        if (component.hasCharge)
        {
            args.ModifySpeed(1f, 1f);
        }
        else if (args.SprintSpeedModifier == 1f)
        {
            var sprintDif = movement.BaseWalkSpeed / movement.BaseSprintSpeed;
            args.ModifySpeed(1f, sprintDif);
        }
    }
}
