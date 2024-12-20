using Microsoft.Xna.Framework.Graphics;
using BirbCore.Attributes;

namespace CookingSkillRedux
{
    [SAsset(Priority = 0)]
    public class Assets
    {
        [SAsset.Asset("assets/cookingiconA.png")]
        public Texture2D IconA { get; set; }

        [SAsset.Asset("assets/cookingiconB_0.png")]
        public Texture2D IconB_0 { get; set; }

        [SAsset.Asset("assets/cookingiconB_1.png")]
        public Texture2D IconB_1 { get; set; }

        [SAsset.Asset("assets/cookingiconB_2.png")]
        public Texture2D IconB_2 { get; set; }

        [SAsset.Asset("assets/cooking5a.png")]
        public Texture2D Cooking5a { get; set; }

        [SAsset.Asset("assets/cooking5b.png")]
        public Texture2D Cooking5b { get; set; }

        [SAsset.Asset("assets/cooking10a1.png")]
        public Texture2D Cooking10a1 { get; set; }

        [SAsset.Asset("assets/cooking10a2.png")]
        public Texture2D Cooking10a2 { get; set; }

        [SAsset.Asset("assets/cooking10b1.png")]
        public Texture2D Cooking10b1 { get; set; }

        [SAsset.Asset("assets/cooking10b2.png")]
        public Texture2D Cooking10b2 { get; set; }


        [SAsset.Asset("assets/random_buff.png")]
        public Texture2D Random_Buff { get; set; }

    }
}
