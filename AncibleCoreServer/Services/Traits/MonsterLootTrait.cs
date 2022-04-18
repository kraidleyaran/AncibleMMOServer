using System.Collections.Generic;
using AncibleCoreCommon.CommonData;
using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreServer.Services.Combat;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class MonsterLootTrait : ObjectTrait
    {
        private string _lootTable = string.Empty;
        private IntNumberRange _experience = new IntNumberRange {Minimum = 1, Maximum = 2};

        private List<string> _looters = new List<string>();

        public MonsterLootTrait(TraitData data) : base(data)
        {
            if (data is MonsterLootTraitData lootData)
            {
                _lootTable = lootData.LootTable;
                _experience = lootData.Experience;
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _parent.SubscribeWithFilter<AddLooterMessage>(AddLooter, _instanceId);
            _parent.SubscribeWithFilter<SpawnLootMessage>(SpawnLoot, _instanceId);
            _parent.SubscribeWithFilter<BroadcastHealMessage>(BroadcastHeal, _instanceId);
            _parent.SubscribeWithFilter<AggroDroppedMessage>(AggroDropped, _instanceId);
        }

        private void AddLooter(AddLooterMessage msg)
        {
            if (!_looters.Contains(msg.Obj.Id))
            {
                _looters.Add(msg.Obj.Id);
            }
        }

        private void SpawnLoot(SpawnLootMessage msg)
        {
            if (_looters.Count > 0)
            {
                var validLooters = new List<WorldObject>();
                for (var i = 0; i < _looters.Count; i++)
                {
                    var obj = ObjectManagerService.GetObjectFromId(_looters[i]);
                    if (obj != null)
                    {
                        validLooters.Add(obj);
                    }
                }

                if (validLooters.Count > 0)
                {
                    var experience = _experience.GenerateRandomNumber(RNGService.RANDOM);
                    if (validLooters.Count > 1)
                    {
                        experience = (int)((float)experience / validLooters.Count * CombatService.CombatSettings.ExperiencePerPlayerBonus);
                        if (experience <= 0)
                        {
                            experience = 1;
                        }
                    }
                    var gainClassExperienceMsg = new GainClassExperienceMessage { Amount = experience};
                    for (var i = 0; i < validLooters.Count; i++)
                    {
                        this.SendMessageTo(gainClassExperienceMsg, validLooters[i]);
                    }
                    var lootTable = LootTableService.GetLootTable(_lootTable);
                    if (lootTable != null)
                    {

                        ObjectManagerService.GenerateLootObject(lootTable, _parent.Map, _parent.Tile.Position, validLooters.ToArray());
                        //var gold = lootTable.Gold.GenerateRandomNumber(RNGService.RANDOM);
                        //var items = lootTable.GenerateItems();

                        //var addGold = gold / validLooters.Count;
                        //if (gold > 0 && addGold <= 0)
                        //{
                        //    addGold = 1;
                        //}

                        //if (addGold > 0)
                        //{
                        //    var addGoldMsg = new AddGoldMessage();
                        //    addGoldMsg.Amount = addGold;
                        //    for (var i = 0; i < validLooters.Count; i++)
                        //    {
                        //        this.SendMessageTo(addGoldMsg, validLooters[i]);
                        //    }
                        //}

                        //var addItemToInventoryMsg = new AddItemToInventoryMessage {Announce = true};
                        //for (var i = 0; i < items.Length; i++)
                        //{
                        //    var stack = items[i].Stack;
                        //    addItemToInventoryMsg.Item = items[i].Item;
                        //    addItemToInventoryMsg.Stack = stack;
                        //    addItemToInventoryMsg.ReturnStack = returnStack => { stack = returnStack; };
                        //    var looted = false;
                        //    var looters = validLooters.ToList();
                        //    while (!looted)
                        //    {
                        //        var looter = looters.Count > 1 ? looters[RNGService.RollRange(0, looters.Count)] : looters[0];
                        //        looters.Remove(looter);
                        //        addItemToInventoryMsg.Stack = stack;
                        //        this.SendMessageTo(addItemToInventoryMsg, looter);
                        //        if (stack <= 0 || looters.Count <= 0)
                        //        {
                        //            looted = true;
                        //        }
                        //    }
                        //}
                    }
                }
            }
        }

        private void BroadcastHeal(BroadcastHealMessage msg)
        {
            if (!_looters.Contains(msg.Owner.Id))
            {
                _looters.Add(msg.Owner.Id);
            }
        }

        private void AggroDropped(AggroDroppedMessage msg)
        {
            _looters.Clear();
        }
    }
}