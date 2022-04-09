using System.Collections.Generic;
using System.Linq;
using AncibleCoreCommon.CommonData.Client;
using AncibleCoreCommon.CommonData.Items;
using AncibleCoreServer.Services.ObjectManager;
using AncibleCoreServer.Services.Traits;
using MessageBusLib;

namespace AncibleCoreServer.Services.Items
{
    public class EquippedItem
    {
        private ObjectTrait[] _equippedTraits = new ObjectTrait[0];

        public ClientEquippedItemData Data { get; private set; }

        public EquippedItem(WorldObject obj, EquippableItemData itemData, string itemId)
        {
            var traits = itemData.ApplyOnEquip.Select(TraitService.GetTrait).Where(t => t != null).ToArray();
            var addTraitToObjMsg = new AddTraitToObjectMessage();
            var equippedTraits = new List<ObjectTrait>();
            for (var i = 0; i < traits.Length; i++)
            {
                if (!traits[i].Instant)
                {
                    equippedTraits.Add(traits[i]);
                }

                addTraitToObjMsg.Trait = traits[i];
                obj.SendMessageTo(addTraitToObjMsg, obj);
            }
            Data = new ClientEquippedItemData { ItemId = itemId, Item = itemData.Name, Slot = itemData.Slot};
            _equippedTraits = equippedTraits.ToArray();
        }

        public void Unequip(WorldObject obj)
        {
            if (_equippedTraits.Length > 0)
            {
                var removeTraitFromObjMsg = new RemoveTraitFromObjectMessage();
                for (var i = 0; i < _equippedTraits.Length; i++)
                {
                    removeTraitFromObjMsg.Trait = _equippedTraits[i];
                    this.SendMessageTo(removeTraitFromObjMsg, obj);
                }
                _equippedTraits = new ObjectTrait[0];
            }
        }

        
    }
}