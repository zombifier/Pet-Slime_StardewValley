using StardewModdingAPI;
using IJsonAssetsApi = MoonShared.APIs.IJsonAssetsApi;
using BirbCore.Attributes;
using CookingSkillRedux.Core;
using MoonShared.APIs;

namespace CookingSkillRedux
{
    public class ModEntry : Mod
    {
        [SMod.Instance]
        internal static ModEntry Instance;
        internal static Config Config;
        internal static Assets Assets;

        internal static bool JALoaded => ModEntry.Instance.Helper.ModRegistry.IsLoaded("spacechase0.JsonAssets");
        internal static bool BCLoaded => ModEntry.Instance.Helper.ModRegistry.IsLoaded("leclair.bettercrafting");
        internal static bool LoveOfCookingLoaded => ModEntry.Instance.Helper.ModRegistry.IsLoaded("blueberry.LoveOfCooking");

        internal static IJsonAssetsApi JsonAssets;
        internal static IBetterCrafting BetterCrafting;

        internal ITranslationHelper I18n => this.Helper.Translation;


        public override void Entry(IModHelper helper)
        {
            Instance = this;
            Parser.ParseAll(this);
        }

        public override object GetApi()
        {
            try
            {
                return new CookingAPI();
            }
            catch
            {
                return null;
            }

        }
    }
}
