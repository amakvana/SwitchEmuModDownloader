using System;
using System.Collections.Generic;

namespace YuzuModDownloader
{
    public sealed class Game
    {
        public string TitleID { get; set; } = "";
        public string TitleName { get; set; } = "";
        public string TitleVersion { get; set; } = "";

        /// <summary>
        /// Absolute path to the current games' Mod Data Location
        /// </summary>
        public string ModDataLocation { get; set; } = "";
        
        /// <summary>
        /// A list containing all downloadable mods for the current game 
        /// </summary>
        public List<Uri> ModDownloadUrls { get; set; } = new List<Uri>();

        public Game() { }
    }
}
