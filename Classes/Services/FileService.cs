using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlatFileStorage
{
    public class FileService
    {
        private readonly AppSettings _config;
        private string OutputFilePath;
        public FileService()
        {
            var jsonData = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "config.json"));
            AppSettings config = JsonSerializer.Deserialize<AppSettings>(jsonData);
            _config = config;
            OutputFilePath = Path.Combine(Directory.GetCurrentDirectory(), _config.WorkingDirectory, _config.FileName);
            if (!File.Exists(OutputFilePath))
            {
                File.Create(OutputFilePath);
            }
        }
        public bool WriteToFile(Item item)
        {
            StorageList storageList = ReadFromFile();
            storageList.items.Add(item);
            string json = JsonSerializer.Serialize(storageList);
            try
            {
                using (StreamWriter outputFile = new StreamWriter(OutputFilePath))
                {
                    outputFile.Write(json);
                }
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        public StorageList ReadFromFile()
        {
            StorageList response = new StorageList();
            try
            {
                string text = File.ReadAllText(OutputFilePath);
                response = JsonSerializer.Deserialize<StorageList>(text);
            }
            catch (System.Exception e)
            { }
            return response;
        }
    }
}