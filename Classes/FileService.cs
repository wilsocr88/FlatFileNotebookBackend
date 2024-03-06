using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlatFileStorage
{
    public class FileService
    {
        private readonly AppSettings _config;
        private string FilePath;
        public FileService()
        {
            // Get config
            var configJson = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "config.json"));
            _config = JsonSerializer.Deserialize<AppSettings>(configJson);

            // Working directory
            FilePath = Path.Combine(Directory.GetCurrentDirectory(), _config.WorkingDirectory);
            if (!Directory.Exists(FilePath))
            {
                Directory.CreateDirectory(FilePath);
            }
        }
        public bool WriteToFile(ItemRequest req)
        {
            // Get file (or new empty set)
            StorageList storageList = ReadFromFile(req.file);
            // Add item
            storageList.items.Add(new Item() { title = req.title, body = req.body });
            try
            {
                // Send to file
                string json = JsonSerializer.Serialize(storageList);
                using (StreamWriter outputFile = new StreamWriter(Path.Combine(FilePath, req.file)))
                {
                    outputFile.Write(json);
                    outputFile.Close();
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
        public bool EditItem(ItemEditRequest req)
        {
            // Get file (or new empty set)
            StorageList storageList = ReadFromFile(req.file);
            storageList.items[req.id] = new Item() { title = req.title, body = req.body };
            try
            {
                // Send to file
                string json = JsonSerializer.Serialize(storageList);
                using (StreamWriter outputFile = new StreamWriter(Path.Combine(FilePath, req.file)))
                {
                    outputFile.Write(json);
                    outputFile.Close();
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
        public bool RemoveItem(ItemDeleteRequest req)
        {
            // Get file (or new empty set)
            StorageList storageList = ReadFromFile(req.file);
            if (storageList.items.ElementAtOrDefault(req.id) != null)
            {
                storageList.items.RemoveAt(req.id);
                try
                {
                    // Send to file
                    string json = JsonSerializer.Serialize(storageList);
                    using (StreamWriter outputFile = new StreamWriter(Path.Combine(FilePath, req.file)))
                    {
                        outputFile.Write(json);
                        outputFile.Close();
                    }
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }
        /**
         * Get contents of file, or return a new empty set
         */
        public StorageList ReadFromFile(string name)
        {
            StorageList response = new StorageList();
            if (CreateFile(name))
                // If we just created the file new, return an empty set
                return response;

            // File already existed, parse its contents
            string text = File.ReadAllText(Path.Combine(FilePath, name));
            // If it was empty, return an empty set
            if (string.IsNullOrEmpty(text)) return response;
            response = JsonSerializer.Deserialize<StorageList>(text);
            return response;
        }
        /**
         * Creates file at given string path and returns true.
         * If file already exists, returns false
         */
        public bool CreateFile(string name)
        {
            string path = Path.Combine(FilePath, name);
            try
            {
                if (!File.Exists(path))
                {
                    var file = File.Create(path);
                    file.Close();
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch
            {
                return false;
            }
        }
        public FileList ListFiles()
        {
            FileList response = new FileList();
            string[] files = Directory.GetFiles(FilePath);
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                if (fileName[0].Equals('.') || fileName.Equals("null")) continue;
                response.files.Add(fileName);
            }
            return response;
        }
        public bool DeleteFile(string name)
        {
            try
            {
                string path = Path.Combine(FilePath, name);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}