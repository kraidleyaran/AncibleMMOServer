using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AncibleCoreCommon.CommonData;
using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreServer.Services.Combat;
using AncibleCoreServer.Services.Maps;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class AiAggroTrait : ObjectTrait
    {
        private int _aggroDistance = 1;
        private int _ticksUntilAggroCheck = 1;
        private bool _healOnAggroDrop = false;

        private int _aggroTickCount = 0;
        private CombatAlignment _combatAlignment = CombatAlignment.Neutral;
        private AiState _aiState = AiState.Wandering;
        private ObjectState _objectState = ObjectState.Active;
        private MapTile _targetTile = null;
        private List<MapTile> _path = new List<MapTile>();
        private Dictionary<WorldObject, int> _aggroTable = new Dictionary<WorldObject, int>();

        private Thread _pathingThread = null;

        public AiAggroTrait(TraitData data) : base(data)
        {
            if (data is AiAggroTraitData aggroData)
            {
                _aggroDistance = aggroData.AggroRange;
                _ticksUntilAggroCheck = aggroData.AggroCheckTicks;
                _healOnAggroDrop = aggroData.HealOnAggroDrop;
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            SubscribeToMessages();
        }

        private void AggroRangeCheck()
        {
            if (_combatAlignment != CombatAlignment.Neutral)
            {
                var aggroTiles = MapService.GetMapTilesInArea(_parent.Map, _parent.Tile, _aggroDistance + 1, true);
                var queryCombatAlignmentMsg = new QueryCombatAlignmentMessage();
                var aggrodObjs = new List<WorldObject>();
                for (var t = 0; t < aggroTiles.Length; t++)
                {
                    if (aggroTiles[t].ObjectsOnTile.Count > 0)
                    {
                        var objsOnTile = aggroTiles[t].ObjectsOnTile;
                        for (var o = 0; o < objsOnTile.Count; o++)
                        {
                            var index = o;
                            queryCombatAlignmentMsg.DoAfter = objAlignment =>
                            {
                                if (objAlignment != CombatAlignment.Neutral && objAlignment != CombatAlignment.Object && objAlignment != _combatAlignment)
                                {
                                    aggrodObjs.Add(objsOnTile[index]);
                                }
                            };
                            this.SendMessageTo(queryCombatAlignmentMsg, objsOnTile[index]);
                        }
                    }
                }

                for (var i = 0; i < aggrodObjs.Count; i++)
                {
                    if (!_aggroTable.ContainsKey(aggrodObjs[i]))
                    {
                        _aggroTable.Add(aggrodObjs[i], 0);
                        this.SendMessageTo(new AddAggrodMonsterMessage { Monster = _parent }, aggrodObjs[i]);
                    }
                }

                var objs = _aggroTable.Keys.ToArray();
                for (var i = 0; i < objs.Length; i++)
                {
                    if (objs[i].Map != _parent.Map)
                    {
                        _aggroTable.Remove(objs[i]);
                        this.SendMessageTo(new RemoveAggrodMonsterMessage { Monster = _parent }, objs[i]);
                    }
                    else
                    {
                        var removed = false;
                        var index = i;
                        if (objs[i].BeingDestroy)
                        {
                            removed = true;
                        }
                        else
                        {
                            var queryObjStateMsg = new QueryObjectStateMessage
                            {
                                DoAfter = objState =>
                                {
                                    if (objState == ObjectState.Dead)
                                    {
                                        removed = true;
                                    }
                                }
                            };
                            this.SendMessageTo(queryObjStateMsg, objs[index]);
                        }
                        if (removed)
                        {
                            _aggroTable.Remove(objs[i]);
                            this.SendMessageTo(new RemoveAggrodMonsterMessage { Monster = _parent }, objs[i]);
                        }
                        else
                        {
                            var distance = _parent.Tile.Position.Distance(objs[i].Tile.Position);
                            if (distance > _aggroDistance)
                            {
                                var aggro = _aggroTable[objs[i]];
                                var lossDistance = (float)(distance - _aggroDistance);
                                aggro -= CombatService.GetAggroLossFromDistance(lossDistance);
                                if (aggro < 0)
                                {
                                    if (aggrodObjs.Contains(objs[i]))
                                    {
                                        _aggroTable[objs[i]] = 0;
                                    }
                                    else
                                    {
                                        _aggroTable.Remove(objs[i]);
                                        this.SendMessageTo(new RemoveAggrodMonsterMessage { Monster = _parent }, objs[i]);
                                    }
                                }
                                else
                                {
                                    _aggroTable[objs[i]] = aggro;
                                }
                            }
                        }

                    }

                }




            }
        }

        private void AggroTargetCheck()
        {
            if (_aggroTable.Count > 0 && _objectState != ObjectState.Casting)
            {
                if (_aiState != AiState.Aggrod)
                {
                    this.SendMessageTo(new SetAiStateMessage{State = AiState.Aggrod}, _parent);
                }
                var orderedTargets = _aggroTable.OrderByDescending(kv => kv.Value).ToArray();
                var target = orderedTargets[0].Key;
                var useAbility = false;
                this.SendMessageTo(new AbilityCheckMessage{Target = target, OnAbilityUse = used => useAbility = used}, _parent);
                if (useAbility)
                {
                    this.SendMessageTo(new SetDirectionMessage{Direction = Vector2IntData.zero}, _parent);
                }
                else
                {
                    var surroundingTiles = MapService.GetMapTilesInArea(target.Map, target.Tile).ToList();
                    if (surroundingTiles.Contains(_parent.Tile))
                    {
                        this.SendMessageTo(new SetDirectionMessage { Direction = Vector2IntData.zero }, _parent);
                    }
                    else if (_pathingThread == null)
                    {
                        var resetPath = false;
                        if (_targetTile == null || !surroundingTiles.Contains(_targetTile))
                        {
                            if (surroundingTiles.Count > 0)
                            {
                                _targetTile = surroundingTiles.Count > 1 ? surroundingTiles[RNGService.RollRange(0, surroundingTiles.Count)] : surroundingTiles[0];
                                resetPath = true;
                            }
                            else
                            {
                                _targetTile = null;
                                _aggroTable.Remove(target);
                                _path.Clear();
                            }
                        }
                        if (_targetTile != null)
                        {
                            if (resetPath && _pathingThread == null)
                            {
                                _path.Clear();
                                _pathingThread = new Thread(() =>
                                {
                                    if (!_parent.BeingDestroy)
                                    {
                                        _path = MapService.GetPathToTileInMap(_parent.Map, _parent.Tile.Position, _targetTile.Position).ToList();

                                        if (_path.Count > 0)
                                        {
                                            var direction = _parent.Tile.Position.Direction(_path[0].Position);
                                            this.SendMessageTo(new SetDirectionMessage { Direction = direction }, _parent);
                                        }
                                        else
                                        {
                                            this.SendMessageTo(new SetDirectionMessage { Direction = Vector2IntData.zero }, _parent);
                                        }
                                    }

                                    _pathingThread = null;
                                });
                                _pathingThread.Start();

                            }


                        }
                    }
                }
            }
            else if (_aiState == AiState.Aggrod)
            {
                if (_healOnAggroDrop)
                {
                    this.SendMessageTo(FullHealMessage.INSTANCE, _parent);
                }
                this.SendMessageTo(new SetAiStateMessage{State = AiState.Wandering}, _parent);
            }
            
        }

        private void SubscribeToMessages()
        {
            this.Subscribe<WorldTickMessage>(WorldTick);

            _parent.SubscribeWithFilter<UpdateCombatAlignmentMessage>(UpdateCombatAlignment, _instanceId);
            _parent.SubscribeWithFilter<UpdateAiStateMessage>(UpdateAiState, _instanceId);
            _parent.SubscribeWithFilter<UpdateTileMessage>(UpdateTile, _instanceId);
            _parent.SubscribeWithFilter<TakeDamageMessage>(TakeDamage, _instanceId);
            _parent.SubscribeWithFilter<BroadcastHealMessage>(BroadcastHeal, _instanceId);
            _parent.SubscribeWithFilter<UpdateObjectStateMessage>(UpdateObjectState, _instanceId);
            _parent.SubscribeWithFilter<ApplyStatusEffectMessage>(ApplyStatusEffect, _instanceId);
        }

        private void UpdateCombatAlignment(UpdateCombatAlignmentMessage msg)
        {
            _combatAlignment = msg.Alignment;
        }

        private void WorldTick(WorldTickMessage msg)
        {
            _aggroTickCount++;
            if (_aggroTickCount >= _ticksUntilAggroCheck)
            {
                _aggroTickCount = 0;
                AggroRangeCheck();
                AggroTargetCheck();
            }
        }

        private void UpdateAiState(UpdateAiStateMessage msg)
        {
            _aiState = msg.State;
        }

        private void UpdateTile(UpdateTileMessage msg)
        {
            if (_aiState == AiState.Aggrod && _objectState != ObjectState.Dead)
            {
                if (_path.Count > 0)
                {
                    if (_path[0] == msg.Tile)
                    {
                        _path.RemoveAt(0);
                    }

                    if (_path.Count > 0)
                    {
                        if (_objectState != ObjectState.Casting)
                        {
                            var direction = _parent.Tile.Position.Direction(_path[0].Position);
                            this.SendMessageTo(new SetDirectionMessage { Direction = direction }, _parent);
                        }
                    }
                    else
                    {
                        this.SendMessageTo(new SetDirectionMessage{Direction = Vector2IntData.zero}, _parent);
                    }
                }


            }
        }

        private void BroadcastHeal(BroadcastHealMessage msg)
        {
            if (msg.Owner != null)
            {
                var ownerAlignment = CombatAlignment.Neutral;
                this.SendMessageTo(new QueryCombatAlignmentMessage { DoAfter = alignment => ownerAlignment = alignment }, msg.Owner);
                if (ownerAlignment != _combatAlignment)
                {
                    if (!_aggroTable.ContainsKey(msg.Owner))
                    {
                        _aggroTable.Add(msg.Owner, 0);
                        this.SendMessageTo(new AddAggrodMonsterMessage { Monster = _parent }, msg.Owner);
                    }

                    _aggroTable[msg.Owner] += CombatService.CalculateAggroPerHeal(msg.Amount);
                }
            }

        }

        private void TakeDamage(TakeDamageMessage msg)
        {
            if (msg.Owner != null)
            {
                var ownerAlignment = CombatAlignment.Neutral;
                this.SendMessageTo(new QueryCombatAlignmentMessage { DoAfter = alignment => ownerAlignment = alignment }, msg.Owner);
                if (ownerAlignment != _combatAlignment)
                {
                    if (!_aggroTable.ContainsKey(msg.Owner))
                    {
                        _aggroTable.Add(msg.Owner, 0);
                        this.SendMessageTo(new AddAggrodMonsterMessage{Monster = _parent}, msg.Owner);
                    }

                    _aggroTable[msg.Owner] += CombatService.CalculateAggroPerDamage(msg.Amount);
                }

                if (_aiState != AiState.Aggrod)
                {
                    this.SendMessageTo(new SetAiStateMessage{State = AiState.Aggrod}, _parent);
                }
            }

        }

        private void UpdateObjectState(UpdateObjectStateMessage msg)
        {
            _objectState = msg.State;
        }

        private void ApplyStatusEffect(ApplyStatusEffectMessage msg)
        {
            if (msg.Owner != null)
            {
                var ownerAlignment = CombatAlignment.Neutral;
                this.SendMessageTo(new QueryCombatAlignmentMessage { DoAfter = alignment => ownerAlignment = alignment }, msg.Owner);
                if (ownerAlignment != _combatAlignment)
                {
                    if (!_aggroTable.ContainsKey(msg.Owner))
                    {
                        _aggroTable.Add(msg.Owner, 0);
                        this.SendMessageTo(new AddAggrodMonsterMessage { Monster = _parent }, msg.Owner);
                    }

                    _aggroTable[msg.Owner] += CombatService.GetAggroFromStatusEffect();
                }
            }
        }

        public override void Destroy()
        {
            if (_aggroTable.Count > 0)
            {
                var objs = _aggroTable.Keys.ToArray();
                for (var i = 0; i < objs.Length; i++)
                {
                    this.SendMessageTo(new RemoveAggrodMonsterMessage { Monster = _parent }, objs[i]);
                }
            }
            base.Destroy();
        }
    }
}