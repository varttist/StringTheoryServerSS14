using Content.Server.PowerCell;
using Content.Shared.Alert;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.PowerCell;
using Content.Shared.PowerCell.Components;
using Content.Shared._ST14.Races.Android;
using Content.Shared.Bed.Sleep;
using Content.Shared.StatusEffect;
using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Random;
using Content.Shared.Movement.Systems;

namespace Content.Server._ST14.Races.Android;

/// <inheritdoc/>
public sealed partial class AndroidSystem : SharedAndroidSystem
{
    [ValidatePrototypeId<StatusEffectPrototype>]
    private const string StatusEffectKey = "ForcedSleep";

    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly PowerCellSystem _powerCell = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AndroidComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<AndroidComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<AndroidComponent, PowerCellChangedEvent>(OnPowerCellChanged);
        SubscribeLocalEvent<AndroidComponent, PowerCellSlotEmptyEvent>(OnPowerCellSlotEmpty);
        SubscribeLocalEvent<AndroidComponent, ItemSlotInsertAttemptEvent>(OnItemSlotInsertAttempt);
        SubscribeLocalEvent<AndroidComponent, WakeActionEvent>(OnWakeIfDeactivated);
    }

    private void OnMapInit(EntityUid uid, AndroidComponent component, MapInitEvent args)
    {
        _powerCell.SetDrawEnabled(uid, true);
    }

    private void OnWakeIfDeactivated(EntityUid uid, AndroidComponent component, WakeActionEvent args)
    {
        //Log.Debug("WakeCatch_____________________________");
        if (!component.hasCharge && !_statusEffects.HasStatusEffect(uid, StatusEffectKey))
        {
            //Log.Debug("TrueWake_____________________________");
            component.timerUntilSleep = _random.NextFloat(component.minTimeUntilSleep, component.maxTimeUntilSleep);
            component.startTimer = true;
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<AndroidComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.startTimer)
            {
                if (comp.timerUntilSleep > 0)
                {
                    //Log.Debug("timer!=0_____________________________" + comp.timerUntilSleep);
                    comp.timerUntilSleep -= frameTime;
                }
                else if (!comp.hasCharge)
                {
                    //Log.Debug("Timer==0_____________________________");
                    comp.startTimer = false;
                    ChangeAndroidState((uid, comp), false);
                }
            }
        }
    }

    private void OnMobStateChanged(EntityUid uid, AndroidComponent component, MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Alive)
        {
            if (_mind.TryGetMind(uid, out _, out _)) _powerCell.SetDrawEnabled(uid, true);
            //else _powerCell.SetDrawEnabled(uid, false); Off for test
        }
        else
        {
            _powerCell.SetDrawEnabled(uid, false);
        }
    }

    private void OnPowerCellChanged(EntityUid uid, AndroidComponent component, PowerCellChangedEvent args)
    {
        UpdateBattery((uid, component));

        if (_powerCell.HasDrawCharge(uid)) _powerCell.SetDrawEnabled(uid, _mobState.IsAlive(uid));
    }

    private void OnPowerCellSlotEmpty(EntityUid uid, AndroidComponent component, ref PowerCellSlotEmptyEvent args)
    {
        UpdateBattery((uid, component));
        //Log.Debug("PowerSellEmpty_____________________________");
    }

    private void OnItemSlotInsertAttempt(EntityUid uid, AndroidComponent component, ItemSlotInsertAttemptEvent args)
    {
        base.OnItemSlotInsertAttempt(uid, component, args);
        UpdateBattery((uid, component));
        //Log.Debug("EntInsert_____________________________");
    }

    private void UpdateBattery(Entity<AndroidComponent> ent)
    {
        //Log.Debug("UpdateBattery_____________________________");
        if (!_powerCell.TryGetBatteryFromSlot(ent, out var battery))
        {
            _alerts.ClearAlert(ent, ent.Comp.BatteryAlert);
            _alerts.ShowAlert(ent, ent.Comp.NoBatteryAlert);

            ChangeAndroidState(ent, false);
            //Log.Debug("NoBattery_Sleep_UpdateBattery______________________________");

            return;
        }

        var chargePercent = (short) MathF.Round(battery.CurrentCharge / battery.MaxCharge * 10f);

        if (chargePercent == 0 && ent.Comp.hasCharge)
        {
            //Log.Debug("Sleeeeeeep_____________________________");
            ChangeAndroidState(ent, false);
        }
        else if (chargePercent != 0 && !ent.Comp.hasCharge)
        {
            //Log.Debug("WakeUp_____________________________");
            ChangeAndroidState(ent, true);
        }

        _alerts.ClearAlert(ent, ent.Comp.NoBatteryAlert);
        _alerts.ShowAlert(ent, ent.Comp.BatteryAlert, chargePercent);
    }

    private void ChangeAndroidState(Entity<AndroidComponent> ent, bool activeStatus)
    {
        if (activeStatus)
        {
            Popup.PopupEntity(Loc.GetString("android-on"), ent);
            _statusEffects.TryRemoveStatusEffect(ent, StatusEffectKey);
            //Log.Debug("WakeUp_ChangeState_____________________________");
        }
        else
        {
            var baseSleepDuration = _random.NextFloat(ent.Comp.minSleepTime, ent.Comp.maxSleepTime);
            Popup.PopupEntity(Loc.GetString("android-off"), ent);
            _statusEffects.TryAddStatusEffect<ForcedSleepingComponent>(ent, StatusEffectKey, TimeSpan.FromSeconds(baseSleepDuration), true);
            //Log.Debug("Sleep_ChangeState_____________________________" + TimeSpan.FromSeconds(baseSleepDuration));
        }

        _powerCell.SetDrawEnabled(ent.Owner, activeStatus);
        ent.Comp.hasCharge = activeStatus;
        _movementSpeedModifier.RefreshMovementSpeedModifiers(ent);
    }
}
