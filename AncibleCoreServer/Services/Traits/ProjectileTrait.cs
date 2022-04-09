using System.Linq;
using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreCommon.CommonData.WorldEvent;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class ProjectileTrait : ObjectTrait
    {
        private int _travelTime = 0;
        private string[] _applyOnContact = new string[0];
        private string _projectile = string.Empty;

        private TickTimer _travelTimer = null;

        public ProjectileTrait(TraitData data) : base(data)
        {
            if (data is ProjectileTraitData projectileData)
            {
                _travelTime = projectileData.TravelTime;
                _projectile = projectileData.Projectile;
                _applyOnContact = projectileData.ApplyOnContact;
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            var travelTime = _travelTime;
            if (_sender != null)
            {
                WorldObject parentObj = null;
                this.SendMessageTo(new QueryWorldObjectMessage { DoAfter = obj => parentObj = obj }, _sender);
                if (parentObj != null)
                {
                    travelTime = (int)(parentObj.Tile.Position.Distance(_parent.Tile.Position) * _travelTime);
                    owner.Tile.EventsOnTile.Add(new ProjectileWorldEvent { OwnerId = parentObj.Id, TargetId = _parent.Id, TravelTime = travelTime, Projectile = _projectile });
                }
            }

            _travelTimer = new TickTimer(travelTime, OnContact);
            
        }

        private void OnContact()
        {
            _travelTimer.Destroy();
            _travelTimer = null;
            if (!_parent.BeingDestroy)
            {
                var traits = _applyOnContact.Select(TraitService.GetTrait).Where(t => t != null).ToArray();
                if (traits.Length > 0)
                {
                    var addTraitToObjMsg = new AddTraitToObjectMessage();
                    for (var i = 0; i < traits.Length; i++)
                    {
                        addTraitToObjMsg.Trait = traits[i];
                        _sender.SendMessageTo(addTraitToObjMsg, _parent);
                    }
                }
                this.SendMessageTo(new RemoveTraitFromObjectMessage { Trait = this }, _parent);
            }

        }

        public override void Destroy()
        {
            if (_travelTimer != null)
            {
                _travelTimer.Destroy();
                _travelTimer = null;
            }
            base.Destroy();
        }
    }
}