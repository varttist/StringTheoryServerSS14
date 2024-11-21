using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._ST14.Races.Android;

[RegisterComponent, NetworkedComponent]

/// <summary>
/// Specific component for android race.
/// </summary>
public sealed partial class AndroidComponent : Component
{
    [DataField]
    public ProtoId<AlertPrototype> BatteryAlert = "BorgBattery";

    [DataField]
    public ProtoId<AlertPrototype> NoBatteryAlert = "BorgBatteryNone";

    [DataField]
    public bool hasCharge = true;


    [DataField]
    public float minSleepTime = 40f; //FromSeconds

    [DataField]
    public float maxSleepTime = 80f; //FromSeconds


    [DataField]
    public float minTimeUntilSleep = 15f; //FromSeconds

    [DataField]
    public float maxTimeUntilSleep = 30f; //FromSeconds

    [DataField]
    public float timerUntilSleep = 0;

    public bool startTimer = false;
}
