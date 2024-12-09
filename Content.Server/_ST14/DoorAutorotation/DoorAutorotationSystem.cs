using Robust.Server.GameObjects;
using Content.Shared.Tag;
using Robust.Shared.Map.Components;
using  Content.Server.Shuttles.Components;

namespace Content.Server._ST14.DoorAutorotation;
public sealed class DoorAutorotationSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DoorAutorotationComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, DoorAutorotationComponent id, MapInitEvent args)
    {
        if (_entityManager.TryGetComponent<DockingComponent>(uid, out var _))
        {
            return;
        }

        var transform = Transform(uid);

        if (transform.GridUid == null)
        {
            return;
        }

        var grid = transform.GridUid.Value;

        if (!_entityManager.TryGetComponent<MapGridComponent>(grid, out var mapComp))
        {
            return;
        }

        var tileRef = _mapSystem.GetTileRef(grid, mapComp, transform.Coordinates);
        var pos = tileRef.GridIndices;

        var wallUp = IsTileHasWall(grid, pos + new Vector2i(0, 1));
        var wallDown = IsTileHasWall(grid, pos + new Vector2i(0, -1));
        var wallRight = IsTileHasWall(grid, pos + new Vector2i(1, 0));
        var wallLeft = IsTileHasWall(grid, pos + new Vector2i(-1, 0));

        if ( (transform.LocalRotation != Math.PI * 0.5 || transform.LocalRotation != Math.PI * 1.5 ) && (wallUp || wallDown))
        {
            _transform.SetLocalRotation(uid, Math.PI * 0.5, transform);
        }
        else if ((transform.LocalRotation != 0 || transform.LocalRotation != Math.PI ) &&  (wallRight || wallLeft))
        {
            _transform.SetLocalRotation(uid, 0, transform);
        }
    }

    private bool IsTileHasWall(EntityUid gridId, Vector2i pos)
    {
        var gridIndices = new List<Vector2i> { pos };
        foreach (var entity in _lookup.GetLocalEntitiesIntersecting(gridId, gridIndices, LookupFlags.Static))
        {
            if (_tag.HasTag(entity, "Wall") ||
                _tag.HasTag(entity, "Window") ||
                _tag.HasTag(entity, "Airlock") ||
                _tag.HasTag(entity, "GlassAirlock")
            )
                return true;
        }
        return false;
    }

}
