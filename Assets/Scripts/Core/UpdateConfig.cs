namespace EggMerge.Core
{
    public static class UpdateConfig
    {
        public const string AppVersion = "0.1.0";

        /// <summary>
        /// После первого push замени owner/repo на свой репозиторий.
        /// </summary>
        public const string ManifestUrl =
            "https://raw.githubusercontent.com/romangolotvin/egg-merge-rush/main/updates/version.json";
    }
}
