using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreServer.Services.ObjectManager;

namespace AncibleCoreServer.Services.Traits
{
    public class SpriteTrait : ObjectTrait
    {
        private string _sprite = string.Empty;

        public SpriteTrait(TraitData data) : base(data)
        {
            if (data is SpriteTraitData spriteData)
            {
                _sprite = spriteData.Sprite;
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            owner.Sprite = _sprite;
            owner.Visible = true;
        }
    }
}