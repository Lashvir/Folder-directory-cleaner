using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Newtonsoft.Json;

namespace ConsoleApplication1
{
    internal class FolderDirectoryCleaner
    {
        
        private static DirPair[] _dirPairs = new DirPair[100];

        private static Dictionary<string, string[]> _fileTypeMap = new Dictionary<string, string[]>
        {
            ["books"]=new[]{"mobi","azw3","epub","azw","fb2"},
            ["videos"]=new[]{"mp4","mov","wmv","avi","avchd","flv","swf","f4v","mkv"},
            ["docs"]=new[]{"docx","pdf","odt","html","txt"},
            ["pictures"]=new[]{"jpg","png","jpeg","bmp","tiff","psd"},
            ["audio"]=new[]{"mp3","wav","ogg","aac","flac","wma","m4a"},
        };
        //Options: books, videos, audio, pictures, or documents
        private static void CreateDest(string destination, string source, string[] fTypesArr)
        {
           DirPair dPTmp = new DirPair(source,destination,fTypesArr);
           for (int i = 0; i <= _dirPairs.Length-1; i++)
           {
               if (_dirPairs[i] is null)
               {
                   _dirPairs[i] = dPTmp;
                   break;
               }
           }
           Console.WriteLine("New destination, source, and file types added successfully");
        }//end create new destination method

        private static void LoadData(string saveFilePath)
        {
            
            if (File.Exists(saveFilePath))
            {
                try
                {
                    _dirPairs = JsonConvert.DeserializeObject<DirPair[]>(File.ReadAllText(saveFilePath)) ?? new DirPair[100];
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading data: {ex.Message}");
                    _dirPairs = new DirPair[100];
                }
            }
            else
            {
                _dirPairs = new DirPair[100];
            }
           
        }//end load method

        private static void SaveData(string saveFilePath)
        {
            string jsonData = JsonConvert.SerializeObject(_dirPairs);
            File.WriteAllText(saveFilePath,jsonData);
        }//end save method

        private static void UpdateDest()
        {
            Console.WriteLine("Please enter the destination folder location to delete this specific entry (Copy and paste it to avoid issues)");
            string destDelete=Console.ReadLine();
            Console.WriteLine("Please enter the source folder location to delete this specific entry (Copy and paste it to avoid issues)");
            string srcDelete=Console.ReadLine();
            List<DirPair> dPList = new List<DirPair>(_dirPairs);
            dPList.RemoveAll(dP => dP != null && dP.Source == srcDelete && dP.Dest == destDelete);
            _dirPairs = dPList.ToArray();
            Console.WriteLine($"Entry with source '{srcDelete}' and destination '{destDelete}' has been deleted");
        }

        private static void CleanMode()
        {
            foreach (var dP in _dirPairs)
            {
                if (dP == null || dP.Dest == null || dP.Source == null)
                {//need all for proper function 
                    continue;
                }
                string[] fileTypes = dP.FileTypes;
                string[] files = Directory.GetFiles(dP.Source);
                foreach (var file in files)
                {
                    string extension = Path.GetExtension(file).TrimStart('.');
                    foreach (var fileType in fileTypes)
                    {
                        if (_fileTypeMap[fileType].Contains(extension))
                        {
                            string destinationFile = Path.Combine(dP.Dest, Path.GetFileName(file));
                            bool success = false;
                            int retryCount = 3;

                             while (!success && retryCount > 0)
                             {
                                 try
                                 {//checks, retries, and techniques to avoid "file is being accessed" issues along with some others
                                     // Use FileStream with FileShare to allow other processes to read the file
                                     using (FileStream sourceStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                                     {
                                         using (FileStream destinationStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write, FileShare.None))
                                         {
                                             sourceStream.CopyTo(destinationStream);
                                         }
                                     }

                                     // If copy is successful, delete the source file
                                     File.Delete(file);
                                     success = true;
                                 }
                                 catch (IOException ioEx)
                                 {
                                     int hr = Marshal.GetHRForException(ioEx);
                                     if (hr == -2147024864) // error sharing violation
                                     {
                                         retryCount--;
                                         if (retryCount == 0)
                                         {
                                             Console.WriteLine($"IO Error handling file {file}: {ioEx.Message}");
                                         }
                                         else
                                         {
                                             Thread.Sleep(1000); // Wait for a second before retrying
                                         }
                                     }
                                     else
                                     {
                                         Console.WriteLine($"IO Error handling file {file}: {ioEx.Message}");
                                         break;
                                     }
                                 }
                                 catch (Exception ex)
                                 {
                                     Console.WriteLine($"Error handling file {file}: {ex.Message}");
                                     break;
                                 }
                            }
                        }
                    }
                }
            }
            Console.WriteLine("~~Cleaning done!~~");
        }

        private class DirPair
        {
            public string Source { get; set; }
            public string Dest { get; set; }
            public string[] FileTypes { get; set; }

            public DirPair(string source, string dest, string[] fileTypes)
            {
                Source = source;
                Dest = dest;
                FileTypes = fileTypes;
            }
        }
        public static void Main(string[] args)
        {//load save file if it exists
            string saveFilePath = "cleaningSafe.txt";
            string introArt = "\n\n  _____     _     _            ______  _               _                      ____ _                            \n |  ___|__ | | __| | ___ _ __ / /  _ \\(_)_ __ ___  ___| |_ ___  _ __ _   _   / ___| | ___  __ _ _ __   ___ _ __ \n | |_ / _ \\| |/ _` |/ _ \\ '__/ /| | | | | '__/ _ \\/ __| __/ _ \\| '__| | | | | |   | |/ _ \\/ _` | '_ \\ / _ \\ '__|\n |  _| (_) | | (_| |  __/ | / / | |_| | | | |  __/ (__| || (_) | |  | |_| | | |___| |  __/ (_| | | | |  __/ |   \n |_|  \\___/|_|\\__,_|\\___|_|/_/  |____/|_|_|  \\___|\\___|\\__\\___/|_|   \\__, |  \\____|_|\\___|\\__,_|_| |_|\\___|_|   \n                                                                     |___/                                      \n\n";
            if (File.Exists(saveFilePath))
            {
                LoadData(saveFilePath);
            }
            
            bool quit = true;
            Console.WriteLine(introArt);
            //Console.WriteLine("The Window width is {0}, and the height is {1}",Console.WindowWidth, Console.WindowHeight);
            Console.WriteLine($"Welcome to this small folder/directory cleaner program. {Environment.NewLine}" +
                              "I made this mainly to clean my download folder but you can set it up to clean" +
                              $" multiple different folders/directories. {Environment.NewLine}" +
                              "You can find the setup options below.");
            do
            {
                Console.WriteLine($"Here are the following options (NOTE ignore the '' around the words below and press enter after each option):{Environment.NewLine}" +
                                  $"type 'q' to quit. {Environment.NewLine}" +
                                  $"type 'd' to setup a new folder/directory to move files to {Environment.NewLine}" +
                                  $"type 'v' to see the saved/current destinations and filetypes {Environment.NewLine}" +
                                  $"type 'u' to delete an entry {Environment.NewLine}" +
                                  "type 'c' to clean!");
                string userCheck = Console.ReadLine();
                switch (userCheck)
                {
                    case "q":
                        quit = false;
                        break;
                    case "d":
                        string dest ="";
                        bool destVerify = true;
                       
                        string source ="";
                        while (destVerify)
                        {
                            Console.WriteLine($"Enter the destination folder/directory link (where files will be moved, Copy and Paste to make it easier): {Environment.NewLine}" +
                                              "Ex: C:\\Users\\dimag\\Pictures\\ or something like C:\\Users\\dimag\\Documents\\backups\\Books");
                            dest= Console.ReadLine();
                            if (Directory.Exists(dest))
                            {
                                destVerify = false;
                            }
                            else
                            {
                                Console.WriteLine("Please enter an appropriate folder/directory link.");
                            }
                        
                        }
                        
                        destVerify = true;
                        while (destVerify)
                        {
                            Console.WriteLine($"Please enter the location of the source folder/dir. you want to be cleaned:(Copy and Paste to make it easier) {Environment.NewLine}" +
                                              $"Ex: C:\\Users\\dimag\\Pictures\\ or something like C:\\Users\\dimag\\Documents\\backups\\Books {Environment.NewLine}");
                            source = Console.ReadLine();
                            if (Directory.Exists(source))
                            {
                                destVerify = false;
                            }
                            else
                            {
                                Console.WriteLine("Please enter an appropriate folder/directory link.");
                            }
                        }
                        
                        destVerify = true;
                        while (destVerify)
                        {
                            Console.Write($"Please enter the file types you want to be cleaned and moved to this new location (Options: books, pictures, docs, audio, videos) {Environment.NewLine}" +
                                          $"You can have more than one type like books, docs, videos (please put a , between each valid file type):{Environment.NewLine}");
                            var fTypes=Console.ReadLine();
                            if (fTypes != null)
                            {
                                string[] fTypesArr = fTypes.Split(',');
                                int count = 0;
                                foreach (var str in fTypesArr)
                                {
                                    if (_fileTypeMap.ContainsKey(str))
                                    {
                                        count++;
                                    
                                    }
                                    else
                                    {
                                        Console.WriteLine("{0} is not a valid file type (Valid file types are: books, pictures, docs, audio, and videos)",str);
                                        break;
                                    }
                                }

                                if (count == fTypesArr.Length)
                                {
                                    destVerify = false;
                                    CreateDest(dest,source, fTypesArr);
                                    SaveData(saveFilePath);
                                }
                            }
                        }
                        break;
                    case "v":
                        Console.WriteLine("------ view portion ------");
                        foreach (var dP in _dirPairs)
                        {
                            Console.WriteLine("Source: {0} | Destination: {1} | File types: {2}", dP.Source, dP.Dest, dP.FileTypes);
                        }
                        Console.WriteLine("------ end view portion ------");
                        break;
                    case "u":
                        UpdateDest();
                        SaveData(saveFilePath);
                        break;
                    case "c":
                        CleanMode();
                        break;
                    default:
                        Console.WriteLine("Please type in one of the options");
                        break;
                }//end switch options
               
            } while (quit);
        }

        
    }
}