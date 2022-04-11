using System.Collections.Generic;
using System.Linq;
using AncibleCoreCommon;
using AncibleCoreCommon.CommonData.Client;
using AncibleCoreCommon.CommonData.PlayerEvent;
using AncibleCoreCommon.CommonData.WorldEvent;
using AncibleCoreServer.Data;
using AncibleCoreServer.Services.CharacterClass;
using AncibleCoreServer.Services.ObjectManager;
using AncibleCoreServer.Services.Talents;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class PlayerClassTrait : ObjectTrait
    {
        private const string NAME = "Player Class Trait";

        private string _class = string.Empty;
        private int _level = 0;
        private int _experience = 0;
        private int _unspentTalentPoints = 0;

        private string _playerId = string.Empty;

        private Dictionary<string, ClientTalentData> _talents = new Dictionary<string, ClientTalentData>();

        public PlayerClassTrait(string playerClass, CharacterTalent[] talents, int level, int experience, int unspentPoints, string playerId)
        {
            Name = NAME;
            _playerId = playerId;
            _class = playerClass;
            _level = level;
            _unspentTalentPoints = unspentPoints;
            _experience = experience;
            for (var i = 0; i < talents.Length; i++)
            {
                if (!_talents.ContainsKey(talents[i].Name))
                {
                    _talents.Add(talents[i].Name, new ClientTalentData{Name = talents[i].Name, Rank = talents[i].Rank});
                }
            }
        }

        private bool GainExperience(int experience)
        {
            var totalExperience = _experience + experience;
            var nextLevel = CharacterClassService.GetLevelExperience(_level);
            var leveledUp = false;
            if (nextLevel > 0 && totalExperience >= nextLevel)
            {
                _experience = 0;
                _level++;
                this.SendMessageTo(new ApplyGrowthStatsMessage{Stats = CharacterClassService.GetGrowthForClass(_class)}, _parent);
                leveledUp = true;
                _unspentTalentPoints++;
                var remainingExperience = totalExperience - nextLevel;
                var traits = CharacterClassService.GetTraitsForClassLevel(_class, _level);
                if (traits.Length > 0)
                {
                    var addTraitToObjMsg = new AddTraitToObjectMessage();
                    for (var i = 0; i < traits.Length; i++)
                    {
                        addTraitToObjMsg.Trait = traits[i];
                        _parent.SendMessageTo(addTraitToObjMsg, _parent);
                    }
                }
                GainExperience(remainingExperience);
            }
            else
            {
                _experience = totalExperience;
            }

            return leveledUp;
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            this.SubscribeWithFilter<ClientTalentUpgradeRequestMessage>(ClientTalentUpgradeRequest, _playerId);

            _parent.SubscribeWithFilter<QueryPlayerClassDataMessage>(QueryPlayerClassData, _instanceId);
            _parent.SubscribeWithFilter<GainClassExperienceMessage>(GainClassExperience, _instanceId);
        }

        private void QueryPlayerClassData(QueryPlayerClassDataMessage msg)
        {
            msg.DoAfter.Invoke(_class, _experience, _level, _unspentTalentPoints, _talents.Values.ToArray());
        }

        private void GainClassExperience(GainClassExperienceMessage msg)
        {
            if (GainExperience(msg.Amount))
            {
                _parent.Tile.EventsOnTile.Add(new LevelUpWorldEvent{OwnerId = _parent.Id, OwnerName = _parent.DisplayName, Level = _level});
                this.SendMessageTo(FullHealMessage.INSTANCE, _parent);
            }
            this.SendMessageTo(new RegisterPlayerEventMessage{Event = new PlayerExperienceEvent{Amount = msg.Amount}}, _parent);
            this.SendMessageTo(FlagPlayerForUpdateMessage.INSTANCE, _parent);
        }

        private void ClientTalentUpgradeRequest(ClientTalentUpgradeRequestMessage msg)
        {
            var talents = msg.Upgrades.Select(t => TalentService.GetTalentByName(t.Talent)).Where(t => t != null).OrderBy(t => t.UnlockLevel).ThenBy(t => t.RequiredTalents.Length).ToArray();
            for (var i = 0; i < talents.Length; i++)
            {
                if (_unspentTalentPoints > 0)
                {
                    if (_talents.TryGetValue(talents[i].Name, out var talent))
                    {
                        var upgrade = msg.Upgrades.FirstOrDefault(u => u.Talent == talents[i].Name);
                        if (upgrade != null)
                        {
                            var rank = talent.Rank + upgrade.IncreasedRank;
                            if (rank > talents[i].Ranks.Length - 1)
                            {
                                rank = talents[i].Ranks.Length - 1;
                            }

                            if (rank > talent.Rank)
                            {
                                var difference = rank - talent.Rank;
                                var addTraitToObjMsg = new AddTraitToObjectMessage();
                                for (var r = 1; r <= difference; r++)
                                {
                                    var traits = talents[i].Ranks[r + talent.Rank].ApplyOnRank.Select(TraitService.GetTrait).Where(t => t != null).ToArray();
                                    if (traits.Length > 0)
                                    {
                                        for (var t = 0; t < traits.Length; t++)
                                        {
                                            addTraitToObjMsg.Trait = traits[i];
                                            this.SendMessageTo(addTraitToObjMsg, _parent);
                                        }
                                    }
                                }

                                _unspentTalentPoints -= difference;
                                talent.Rank = rank;
                            }
                            
                        }
                    }
                    else
                    {
                        var missingTalents = talents[i].RequiredTalents.Where(t => !_talents.ContainsKey(t)).ToArray();
                        if (missingTalents.Length > 0)
                        {
                            missingTalents = missingTalents.Where(m => msg.Upgrades.FirstOrDefault(u => u.Talent == m) == null).ToArray();
                            if (missingTalents.Length > 0)
                            {
                                break;
                            }
                        }
                        var upgrade = msg.Upgrades.FirstOrDefault(u => u.Talent == talents[i].Name);
                        if (upgrade != null)
                        {
                            var rank = upgrade.IncreasedRank - 1;
                            if (rank > talents[i].Ranks.Length - 1)
                            {
                                rank = talents[i].Ranks.Length - 1;
                            }

                            var difference = rank + 1;
                            _talents.Add(talents[i].Name, new ClientTalentData { Name = talents[i].Name, Rank = rank });
                            var addTraitToObjMsg = new AddTraitToObjectMessage();
                            for (var r = 0; r <= rank; r++)
                            {
                                var traits = talents[i].Ranks[r].ApplyOnRank.Select(TraitService.GetTrait).Where(t => t != null).ToArray();
                                if (traits.Length > 0)
                                {
                                    for (var t = 0; t < traits.Length; t++)
                                    {
                                        addTraitToObjMsg.Trait = traits[t];
                                        this.SendMessageTo(addTraitToObjMsg, _parent);
                                    }
                                }
                            }
                            _unspentTalentPoints -= difference;
                        }
                    }
                }
                else
                {
                    break;
                }
            }
            this.SendMessageTo(FlagPlayerForUpdateMessage.INSTANCE, _parent);
        }
    }
}