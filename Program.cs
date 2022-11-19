using HeimdallUpdate.Models;
using Newtonsoft.Json;
using System.Net;
using System.Runtime.InteropServices;
using System.IO.Compression;
using System.Runtime.CompilerServices;

namespace HeimdallUpdate
{
    internal class Program
    {
        static bool debugMode { get; set; } = false;

        static string workingDirectory { get; set; } = AppDomain.CurrentDomain.BaseDirectory;

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
            workingDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (debugMode)
            {
                Console.WriteLine("Working Directory: " + workingDirectory);
            }


            //check if there is a file called updateconfig.json in the working directory
            if (System.IO.File.Exists(workingDirectory + "updateconfig.json"))
            {
                UpdateConfig updateConfig = JsonConvert.DeserializeObject<UpdateConfig>(System.IO.File.ReadAllText(workingDirectory + "updateconfig.json"));

                if (updateConfig.ignorePreRequestWarnings != true)
                {
                #if WindowsRuntime
                    if (System.Windows.Forms.MessageBox.Show("Do you want to check for Heimdall updates? This check will request data from GitHub.com servers. This is a potential privacy/OSINT risk.", "Heimdall Update", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question) != System.Windows.Forms.DialogResult.Yes)
                    {
                        Environment.Exit(0);
                    }
                #else
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Do you want to check for Heimdall updates? This check will request data from GitHub.com servers. This is a potential privacy/OSINT risk.");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Press Y to continue, or any other key to exit.");
                ConsoleKeyInfo key = Console.ReadKey();
                if (key.Key != ConsoleKey.Y)
                {
                    Console.Clear();
                    Console.WriteLine("Exiting...");
                    Environment.Exit(0);
                }
                Console.Clear();
                #endif
                }
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

                //convert updateconfig.json to UpdateConfig object

                if (debugMode)
                {
                    Console.WriteLine("Update config found:");
                    Console.WriteLine("Last updated: " + updateConfig.lastUpdated);
                    Console.WriteLine("Ignore minor updates: " + updateConfig.ignoreMinorUpdates);
                    Console.WriteLine("Ignore major updates: " + updateConfig.ignoreMajorUpdates);
                }

                //check if the latest release is newer than the last updated release
                if (latestRelease.published_at > updateConfig.lastUpdated)
                {
                    if (debugMode)
                    {
                        Console.WriteLine("Newer release found!");
                        Console.WriteLine("Current version: " + updateConfig.lastUpdated);
                        Console.WriteLine("New version: " + latestRelease.published_at);
                        Console.WriteLine("New version name: " + latestRelease.name);
                        Console.WriteLine("New version description: " + latestRelease.body);
                    }

                    var updateType = "Unknown";

                    //check if the latest release is a major or minor update
                    if (latestRelease.body.ToLower().Contains("Major Update"))
                    {
                        //major update
                        if (updateConfig.ignoreMajorUpdates)
                        {
                            Console.WriteLine("Ignoring major updates due to config...");
                            Environment.Exit(0);
                        }
                        updateType = "Major";
                    }
                    else if (latestRelease.body.ToLower().Contains("Minor Update"))
                    {
                        //minor update
                        if (updateConfig.ignoreMinorUpdates)
                        {
                            Console.WriteLine("Ignoring update due to your configuration.");
                            Environment.Exit(0);
                        }
                        updateType = "Minor";
                    }
                    else if (latestRelease.body.ToLower().Contains("Emergency Update"))
                    {
                        //emergency update
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("This is an emergency update! Ignoring this update is EXTREMELY irresponsible and dangerous. Emergency updates are released to patch serious vulnerabilities.");
                        Console.ForegroundColor = ConsoleColor.White;
                        updateType = "Emergency";
                    }
                    else
                    {
                        //assume it's a minor update
                        if (updateConfig.ignoreMinorUpdates)
                        {
                            Console.WriteLine("Ignoring update due to your configuration.");
                            Environment.Exit(0);
                        }
                        updateType = "Unknown (release type was not specified, please check GitHub)";
                    }

                    #if WindowsRuntime
                    if (System.Windows.Forms.MessageBox.Show($"A new release of Heimdall is available. This is a {updateType} update, released on {latestRelease.published_at} Do you want to download and install this version of Heimdall from GitHub.com?", "Heimdall Update", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question) != System.Windows.Forms.DialogResult.Yes)
                    {
                        Environment.Exit(0);
                    }
                    #else
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"A new release of Heimdall is available. This is a {updateType} update, released on {latestRelease.published_at}");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"Current {updateConfig.lastUpdated} --> New {latestRelease.published_at}");
                    Console.WriteLine("Do you want to download and install this version of Heimdall from GitHub.com? This is a potential privacy/OSINT risk.");
                    Console.WriteLine("Press Y to continue, or any other key to exit.");
                    ConsoleKeyInfo ekey = Console.ReadKey();
                    if (ekey.Key != ConsoleKey.Y)
                    {
                        Console.Clear();
                        Console.WriteLine("Exiting...");
                        Environment.Exit(0);
                    }
                    #endif

                    //download latest release
                    Console.Clear();
                    Console.WriteLine("Downloading latest release...");
                    string downloadUrl = latestRelease.zipball_url;
                    try
                    {
                        DownloadAndUnzipGithubRelease(downloadUrl);
                    }
                    catch (Exception ex)
                    {
                        #if WindowsRuntime
                        System.Windows.Forms.MessageBox.Show("An error occurred while downloading the latest release. Please try again later.", "Heimdall Update", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        #else

                        Console.WriteLine("Error downloading latest release: " + ex.Message);
                        Environment.Exit(0);
                        #endif
                    }
                    #if WindowsRuntime
                    System.Windows.Forms.MessageBox.Show("Heimdall has been updated to the latest version. Please restart the application.", "Heimdall Update", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                    #else
                    Console.WriteLine("Heimdall has been updated to the latest version. Please restart the application.");
                    #endif

                    //update updateconfig.json
                    updateConfig.lastUpdated = latestRelease.published_at;
                    System.IO.File.WriteAllText(workingDirectory + "updateconfig.json", JsonConvert.SerializeObject(updateConfig));

                    //exit
                    Environment.Exit(0);
                }
                else
                {
                    #if WindowsRuntime
                    System.Windows.Forms.MessageBox.Show("You're already running the latest version.", "Heimdall Update", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                    #else
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("You're already running the latest version.");
                    Console.ForegroundColor = ConsoleColor.White;
                    Thread.Sleep(2500);
                    #endif

                    Environment.Exit(0);
                }
            }

            else
            {
                #if WindowsRuntime
                System.Windows.Forms.MessageBox.Show("No update config found in this directory. Make sure that the updater is in the same directory as the Heimdall updateconfig.json file. If you are running this for the first time, please run Heimdall.py instead.", "Heimdall Update", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                #else
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("No Heimdall installation found.");
                    Console.WriteLine("Make sure that the updater is in the same directory as the Heimdall updateconfig.json file.");
                    Thread.Sleep(5000);
                #endif
                #if WindowsRuntime
                var response =  System.Windows.Forms.MessageBox.Show($"Do you want to download and install Heimdall to {workingDirectory}?", "Heimdall Update", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Information);
                if (response != DialogResult.Yes)
                {
                    Environment.Exit(0);
                }
                #else
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Do you want to download and install Heimdall to {workingDirectory}?");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Press Y to continue, or any other key to exit.");
                ConsoleKeyInfo ekey = Console.ReadKey();
                if (ekey.Key != ConsoleKey.Y)
                {
                    Console.Clear();
                    Console.WriteLine("Exiting...");
                    Environment.Exit(0);
                }

                #endif

                
                //download latest release
                Console.Clear();
                Console.WriteLine("Downloading latest release...");
                GithubReleaseResponse[] allReleases = FetchRepo(author, repo);
                GithubReleaseResponse latestRelease = allReleases[0];

                string downloadUrl = latestRelease.zipball_url;
                try
                {
                    DownloadAndUnzipGithubRelease(downloadUrl);
                }
                catch (Exception ex)
                {
                    #if WindowsRuntime
                    System.Windows.Forms.MessageBox.Show("An error occurred while downloading the latest release. Please try again later.", "Heimdall Update", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    #else
                    Console.WriteLine("Error downloading latest release: " + ex.Message);
                    Environment.Exit(0);
                    #endif
                }

                #if WindowsRuntime
                System.Windows.Forms.MessageBox.Show("Heimdall has been installed.", "Heimdall Update", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                #else
                Console.WriteLine("Heimdall has been installed.");
                #endif

                //update updateconfig.json
                var updateConfig = new UpdateConfig();
                updateConfig.lastUpdated = latestRelease.published_at;
                System.IO.File.WriteAllText(workingDirectory + "updateconfig.json", JsonConvert.SerializeObject(updateConfig));
            }
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

        public static void DownloadAndUnzipGithubRelease(string url)
        {
            //download zip file into memory
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36\t");
            byte[] zipFile = client.GetByteArrayAsync(url).Result;

            //unzip file
            ZipArchive archive = new ZipArchive(new MemoryStream(zipFile));


            //this zip file contains a single folder, which contains the files we want
            //we need to get the name of this folder
            string folderName = archive.Entries[0].FullName;

            //unzip file into working directory
            archive.ExtractToDirectory(workingDirectory);

            //move all files and folders from the extracted folder into the working directory, overwriting existing files
            foreach (string dirPath in Directory.GetDirectories(workingDirectory + folderName, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(folderName, ""));

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(workingDirectory + folderName, "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(folderName, ""), true);


            //delete the extracted folder
            Directory.Delete(workingDirectory + folderName, true);            
        }
    }
    
}