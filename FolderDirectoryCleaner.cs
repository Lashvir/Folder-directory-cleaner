using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace ConsoleApplication1
{
    internal class FolderDirectoryCleaner
    {
        private static Dictionary<string, string[]> destinationsDictionary=new Dictionary<string, string[]>();
        private static void CreateDest(string fileType, string destination)
        {//create new destination folder and file type test
            
            //check if key exists already and either update or return to main loop
            if (destinationsDictionary.ContainsKey(destination))
            {
                Console.WriteLine("This destination already exists did you want to update this to include the new file types?" +
                                  " (Please type Y or N)");
                string responseYN = Console.ReadLine();
                if (responseYN == "Y")
                {
                    //call update method
                }
                else
                {//return to main loop
                    return;
                }
            }//add new destination with file type to the dictionary
            string[] tempArray= {fileType};
            Console.WriteLine(tempArray[0]);
            destinationsDictionary.Add(destination,tempArray);
            Console.WriteLine("New destination and file type added successfully");
            return;
        }//end create new destination method

        private static void LoadData(string saveFilePath)
        {
            destinationsDictionary = JsonConvert.DeserializeObject<Dictionary<string,string[]>>(File.ReadAllText(saveFilePath));
        }//end load method

        private static void SaveData(string saveFilePath)
        {
            string jsonData = JsonConvert.SerializeObject(destinationsDictionary);
            File.WriteAllText(saveFilePath,jsonData);
        }//end save method

        private static void UpdateDest(string fileType, string destination)
        {
            //add update code
        }
        
        public static void Main(string[] args)
        {//load save file if it exists
            string saveFilePath = "cleaningSafe.txt";
            if (File.Exists(saveFilePath))
            {
                LoadData(saveFilePath);
            }
            bool quit = true;
            string[] fileTypes={"books", "videos", "spreadsheets", "audio", "pictures", "documents"};
            Console.WriteLine($"Welcome to this small folder/directory cleaner program. {Environment.NewLine}" +
                              $"I made this mainly to clean my download folder but you can set it up to clean" +
                              $" multiple different folders/directories. {Environment.NewLine}" +
                              $"You can find the setup options below.");
            do
            {
                Console.WriteLine($"Here are the following options (NOTE ignore the '' around the words below):{Environment.NewLine}" +
                                  $"type 'quit' to quit. {Environment.NewLine}" +
                                  $"type 'dest' to setup a new folder/directory to move files to {Environment.NewLine}" +
                                  $"type 'view' to see the saved/current destinations and filetypes");
                string userCheck = Console.ReadLine();
                
                if (userCheck == "quit")
                {
                    quit = false;
                    //end quit option
                    
                }else if (userCheck == "dest") {//dest option
                    bool destVerify = true;
                    string fileType = null;
                    while (destVerify)
                    {
                        Console.WriteLine($"What type of files will be saved in this destination?{Environment.NewLine}" +
                                          $"Options: books, videos, spreadsheets, audio, pictures, or documents.");
                        fileType = Console.ReadLine();
                        if (fileTypes.Contains(fileType) == false)
                        {
                            Console.WriteLine("Please check your spelling or type one of the options presented.");
                        }
                        else
                        {
                            destVerify = false;
                        }
                    }
                    destVerify = true;
                    while (destVerify)
                    {
                        Console.WriteLine($"Enter the destination folder/directory link: {Environment.NewLine}" +
                                          $"Ex: C:\\Users\\dimag\\Pictures\\ or something like C:\\Users\\dimag\\Documents\\backups\\Books" +
                                          $"{Environment.NewLine}");
                        string destination = Console.ReadLine();
                        if (Directory.Exists(destination))
                        {
                            destVerify = false;
                            CreateDest(fileType,destination);
                            SaveData(saveFilePath);
                        }
                        else
                        {
                            Console.WriteLine("Please enter an appropriate folder/directory link.");
                        }
                        
                    }
                    
                    //end dest option
                    
                }else if(userCheck=="update") {//begin update portion
                    
                }else if (userCheck == "view") {
                    Console.WriteLine("------ view portion ------");
                    foreach (var kvp in destinationsDictionary)
                    {
                        Console.WriteLine("{0}, {1}", kvp.Key,string.Join(", ",kvp.Value));
                    }
                    Console.WriteLine("------ end view portion ------");
                }//end show saved/current destinations & file types
            } while (quit);
        }

        
    }
}