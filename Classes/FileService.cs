using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.ObjectModel;

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
        public bool WriteToFile(string user, ItemRequest req)
        {
            // Get file (or new empty set)
            StorageList storageList = ReadFromFile(user, req.File);
            // Add item
            storageList.Items.Add(new Item() { Title = req.Title, Body = req.Body });
            return SendToFile(storageList, user, req.File);
        }
        public bool EditItem(string user, ItemEditRequest req)
        {
            // Get file (or new empty set)
            StorageList storageList = ReadFromFile(user, req.File);
            storageList.Items[req.Id] = new Item() { Title = req.Title, Body = req.Body };
            return SendToFile(storageList, user, req.File);
        }
        public bool ReorderItem(string user, ReorderRequest req)
        {
            if (req.CurrentPos == req.NewPos) return true;
            StorageList storageList = ReadFromFile(user, req.File);
            if (storageList.Items.Count == 0) return false;
            ObservableCollection<Item> storageListItems = new ObservableCollection<Item>(storageList.Items);
            storageListItems.Move(req.CurrentPos, req.NewPos);
            storageList.Items = storageListItems.ToList();
            return SendToFile(storageList, user, req.File);
        }
        public bool RemoveItem(string user, ItemDeleteRequest req)
        {
            // Get file (or new empty set)
            StorageList storageList = ReadFromFile(user, req.File);
            if (storageList.Items.ElementAtOrDefault(req.Id) != null)
            {
                storageList.Items.RemoveAt(req.Id);
                return SendToFile(storageList, user, req.File);
            }
            else
            {
                return false;
            }
        }
        /**
         * Get contents of file, or return a new empty set
         */
        public StorageList ReadFromFile(string user, string name)
        {
            StorageList response = new StorageList();
            if (CreateFile(user, name))
                // If we just created the file new, return an empty set
                return response;

            // File already existed, parse its contents
            string text = File.ReadAllText(Path.Combine(FilePath, user, name));
            // If it was empty, return an empty set
            if (string.IsNullOrEmpty(text)) return response;
            response = JsonSerializer.Deserialize<StorageList>(text);
            return response;
        }
        /**
         * Creates file at given string path and returns true.
         * If file already exists, returns false
         */
        public bool CreateFile(string user, string name)
        {
            string path = Path.Combine(FilePath, user, name);
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
        public FileList ListFiles(string user)
        {
            FileList response = new FileList();
            var path = Path.Combine(FilePath, user);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                if (fileName[0].Equals('.') || fileName.Equals("null")) continue;
                response.Files.Add(fileName);
            }
            return response;
        }
        public bool DeleteFile(string user, string name)
        {
            try
            {
                string path = Path.Combine(FilePath, user, name);
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
        private bool SendToFile(StorageList storageList, string user, string file)
        {
            try
            {
                // Send to file
                string json = JsonSerializer.Serialize(storageList);
                using (StreamWriter outputFile = new StreamWriter(Path.Combine(FilePath, user, file)))
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
    }
}