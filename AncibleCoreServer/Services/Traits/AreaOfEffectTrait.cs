using System.Collections.Generic;
using System.Linq;
using AncibleCoreCommon.CommonData.Ability;
using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreServer.Services.Maps;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class AreaOfEffectTrait : ObjectTrait
    {
        public override bool Instant => true;

        private int _area = 1;
        private string[] _applyToTargets;
        private AbilityAlignment _alignment = AbilityAlignment.All;

        public AreaOfEffectTrait(TraitData data) : base(data)
        {
            if (data is AreaOfEffectTraitData areaData)
            {
                _area = areaData.Area;
                _applyToTargets = areaData.ApplyToTargets;
                _alignment = areaData.AlignmentRequirement;
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            var combatAlignment = CombatAlignment.Neutral;
            this.SendMessageTo(new QueryCombatAlignmentMessage{DoAfter = alignment => combatAlignment = alignment}, _parent);
            var mapTiles = MapService.GetMapTilesInArea(_parent.Map, _parent.Tile, _area);
            var tilesWithObjs = mapTiles.Where(t => t.ObjectsOnTile.Count > 0).ToArray();
            var queryCombatAlignmentMsg = new QueryCombatAlignmentMessage();
            var addTraitToObjMsg = new AddTraitToObjectMessage();
            for (var i = 0; i < tilesWithObjs.Length; i++)
            {
                
                var objs = tilesWithObjs[i].ObjectsOnTile.ToArray();
                for (var o = 0; o < objs.Length; o++)
                {
                    var obj = objs[o];
                    var apply = false;
                    if (_alignment == AbilityAlignment.All)
                    {
                        apply = true;
                    }
                    else
                    {
                        queryCombatAlignmentMsg.DoAfter = alignment =>
                        {
                            apply = _alignment == AbilityAlignment.Friendly
                                ? alignment == combatAlignment
                                : alignment != combatAlignment;
                        };
                        this.SendMessageTo(queryCombatAlignmentMsg, obj);

                    }

                    if (apply)
                    {
                        var traits = _applyToTargets.Select(TraitService.GetTrait).Where(t => t != null).ToArray();
                        if (traits.Length > 0)
                        {
                            for (var t = 0; t < traits.Length; t++)
                            {
                                addTraitToObjMsg.Trait = traits[t];
                                _sender.SendMessageTo(addTraitToObjMsg, obj);
                            }
                        }

                    }
                }

            }
        }


        
    }
}