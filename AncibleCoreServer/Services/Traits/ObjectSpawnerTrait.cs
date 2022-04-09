using System.Collections.Generic;
using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreServer.Services.Maps;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class ObjectSpawnerTrait : ObjectTrait
    {
        private string _template = string.Empty;
        private List<WorldObject> _spawned = new List<WorldObject>();
        private List<WorldObject> _removed = new List<WorldObject>();
        private int _maxSpawns = 1;
        private int _maxSpawnDistance = 1;
        private int _spawnCooldown = 0;

        private TickTimer _cooldownTimer = null;

        public ObjectSpawnerTrait(TraitData data) : base(data)
        {
            if (data is ObjectSpawnerTraitData spawnerData)
            {
                _template = spawnerData.Template;
                _maxSpawns = spawnerData.MaxSpawns;
                _maxSpawnDistance = spawnerData.SpawnDistance;
                _spawnCooldown = spawnerData.SpawnCooldown;
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            var objTemplate = ObjectTemplateService.GetObjectTemplate(_template);
            if (objTemplate != null)
            {
                for (var i = 0; i < _maxSpawns; i++)
                {
                    var spawnableTiles = MapService.GetMapTilesInArea(_parent.Map, _parent.Tile, _maxSpawnDistance);
                    if (spawnableTiles.Length > 0)
                    {
                        var spawnTile = spawnableTiles.Length > 1 ? spawnableTiles[RNGService.RollRange(0, spawnableTiles.Length)] : spawnableTiles[0];
                        var obj = ObjectManagerService.GenerateObjectFromTemplate(objTemplate, spawnTile.Position, _parent.Map);
                        this.SendMessageTo(new AddTraitToObjectMessage { Trait = new SpawnedObjectTrait(_parent) }, obj);
                        _spawned.Add(obj);
                    }
                }
            }

            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            this.Subscribe<WorldTickMessage>(WorldTick);
            this.Subscribe<ResolveTickMessage>(ResolveTick);

            _parent.SubscribeWithFilter<ObjectDeadMessage>(ObjectDead, _instanceId);
        }

        private void WorldTick(WorldTickMessage msg)
        {
            if (_cooldownTimer == null && _spawned.Count < _maxSpawns)
            {
                var objTemplate = ObjectTemplateService.GetObjectTemplate(_template);
                if (objTemplate != null)
                {
                    var spawnableTiles = MapService.GetMapTilesInArea(_parent.Map, _parent.Tile, _maxSpawnDistance);
                    if (spawnableTiles.Length > 0)
                    {
                        var spawnTile = spawnableTiles.Length > 1 ? spawnableTiles[RNGService.RollRange(0, spawnableTiles.Length)] : spawnableTiles[0];
                        var obj = ObjectManagerService.GenerateObjectFromTemplate(objTemplate, spawnTile.Position, _parent.Map);
                        this.SendMessageTo(new AddTraitToObjectMessage { Trait = new SpawnedObjectTrait(_parent) }, obj);
                        _spawned.Add(obj);
                    }
                }
            }
        }

        private void ObjectDead(ObjectDeadMessage msg)
        {
            _removed.Add(msg.Object);
            _spawned.Remove(msg.Object);
        }

        private void ResolveTick(ResolveTickMessage msg)
        {
            if (_removed.Count > 0)
            {
                var removed = _removed.ToArray();
                _removed.Clear();
                for (var i = 0; i < removed.Length; i++)
                {
                    ObjectManagerService.RemoveObjectFromWorld(removed[i].Map, removed[i]);
                }

                _cooldownTimer = new TickTimer(_spawnCooldown, () =>
                {
                    _cooldownTimer.Destroy();
                    _cooldownTimer = null;
                });
            }

        }

        public override void Destroy()
        {
            if (_cooldownTimer != null)
            {
                _cooldownTimer.Destroy();
                _cooldownTimer = null;
            }
            base.Destroy();
        }
    }
}