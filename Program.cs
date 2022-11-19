using HeimdallUpdate.Models;
using Newtonsoft.Json;
using System.Net;
using System.Runtime.InteropServices;

namespace HeimdallUpdate
{
    internal class Program
    {
        static bool debugMode { get; set; } = true;

        static void Main(string[] args)
        {
            //check for --debug flag
            if (args.Length > 0 && args[0] == "--debug")
            {
                debugMode = true;
            }

            string author = "JustCallMeSimon26";
            string repo = "Heimdall";


            Console.WriteLine("Heimdall Updater v1.0");

            //fetch latest release from GitHub API

            //get appdomain current path
            string workingDirectory = AppDomain.CurrentDomain.BaseDirectory;

            //get latest release from GitHub API
            GithubReleaseResponse[] allReleases = FetchRepo(author, repo);
            GithubReleaseResponse latestRelease = allReleases[0];

            if (debugMode)
            {
                Console.WriteLine("Fetched latest release from GitHub API:");
                Console.WriteLine("Latest release: " + latestRelease.tag_name);
                Console.WriteLine("Latest name: " + latestRelease.name);
                Console.WriteLine("Is draft?: " + latestRelease.draft);
                Console.WriteLine("Is prerelease?: " + latestRelease.prerelease);
                Console.WriteLine("Created at: " + latestRelease.created_at);
                Console.WriteLine("Published at: " + latestRelease.published_at);
                Console.WriteLine("Body: " + latestRelease.body);
            }

            //check if there is a file called updateconfig.json in the working directory
            if (System.IO.File.Exists(workingDirectory + "updateconfig.json"))
            {
                //convert updateconfig.json to UpdateConfig object
                UpdateConfig updateConfig = JsonConvert.DeserializeObject<UpdateConfig>(System.IO.File.ReadAllText(workingDirectory + "updateconfig.json"));

                if (debugMode)
                {
                    Console.WriteLine("Update config found:");
                    Console.WriteLine("Last updated: " + updateConfig.lastUpdated);
                    Console.WriteLine("Ignore minor updates: " + updateConfig.ignoreMinorUpdates);
                    Console.WriteLine("Ignore major updates: " + updateConfig.ignoreMajorUpdates);
                }

                //check if the latest release is newer than the last updated release
                if (latestRelease.published_at > DateTime.Parse(updateConfig.lastUpdated))
                {
                    Console.WriteLine("Newer release found!");
                    
                    
                }
            }

            else
            {
                if (debugMode)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("No Heimdall installation found.");
                    Console.WriteLine("Make sure that the updater is in the same directory as the Heimdall updateconfig.json file.");
                    Console.WriteLine("Exiting in 5 seconds...");
                }
            }


            // wait for 5 seconds
            Thread.Sleep(5000);
        }

        public static GithubReleaseResponse[] FetchRepo(string creator, string repo)
        {
            string url = "https://api.github.com/repos/" + creator + "/" + repo + "/releases";

            //get json from url
            HttpClient client = new HttpClient();

            //set user agent
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36\t");

            //get json from url
            string json = client.GetStringAsync(url).Result;


            //deserialize json
            GithubReleaseResponse[] release = JsonConvert.DeserializeObject<GithubReleaseResponse[]>(json);

            return release;
        }
    }
}