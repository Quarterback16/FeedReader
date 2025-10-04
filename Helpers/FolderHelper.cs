namespace FeedReader.Helpers
{
    public static class FolderHelper
    {
        public static string GetObsidianStemFolder(string dropboxfolder) =>

            $"{DropboxFolder(dropboxfolder)}\\Obsidian\\ChestOfNotes\\";

        public static string GetObsidianNflStemFolder(string dropboxfolder) =>

            $"{DropboxFolder(dropboxfolder)}\\Obsidian\\ChestOfNotes\\01 - nfl\\";

        public static string GetObsidianYahooStemFolder(string dropboxfolder) =>

            $"{DropboxFolder(dropboxfolder)}\\Obsidian\\ChestOfNotes\\02 - Yahoo\\";

        public static string GetObsidianGridstatsStemFolder(string dropboxfolder) =>

            $"{DropboxFolder(dropboxfolder)}\\Obsidian\\ChestOfNotes\\02 - GridStats\\";


        public static string DropboxFolder(string dropboxfolder)
        {
            return dropboxfolder;
        }


        public static string ObsidianFolder(string obsidianFolder) =>

            obsidianFolder;

        public static string CsvFolder(string dropboxfolder) =>
      
                $"{dropboxfolder}CSV\\";

        private static string MahomesRoot() => "C:\\Users\\quart\\";


        //public static string JsonFolder(string dropboxfolder) =>
        //     $"{dropboxfolder}JSON\\";

        public static string JsonFolder(string dropboxfolder) =>
            "c:\\developer\\Projects\\FeedReader\\bin\\Debug\\net8.0\\Data\\";

    }
}
