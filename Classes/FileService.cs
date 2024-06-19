using System.IO;
using System.Linq;
using System.Collections.ObjectModel;

namespace FlatFileStorage;

public class FileService(StorageService storageService)
{
    private readonly StorageService _storageSvc = storageService;

    public bool WriteToFile(string user, ItemRequest req)
    {
        // Get file (or new empty set)
        StorageList storageList = _storageSvc.ReadFromFile(user, req.File);
        // Add item
        storageList.Items.Add(new Item() { Title = req.Title, Body = req.Body });
        return _storageSvc.SendToFile(storageList, user, req.File);
    }
    public bool EditItem(string user, ItemEditRequest req)
    {
        // Get file (or new empty set)
        StorageList storageList = _storageSvc.ReadFromFile(user, req.File);
        storageList.Items[req.Id] = new Item() { Title = req.Title, Body = req.Body };
        return _storageSvc.SendToFile(storageList, user, req.File);
    }
    public bool ReorderItem(string user, ReorderRequest req)
    {
        if (req.CurrentPos == req.NewPos) return true;
        StorageList storageList = _storageSvc.ReadFromFile(user, req.File);
        if (storageList.Items.Count == 0) return false;
        ObservableCollection<Item> storageListItems = new(storageList.Items);
        storageListItems.Move(req.CurrentPos, req.NewPos);
        storageList.Items = storageListItems.ToList();
        return _storageSvc.SendToFile(storageList, user, req.File);
    }
    public bool RemoveItem(string user, ItemDeleteRequest req)
    {
        // Get file (or new empty set)
        StorageList storageList = _storageSvc.ReadFromFile(user, req.File);
        if (storageList.Items.ElementAtOrDefault(req.Id) != null)
        {
            storageList.Items.RemoveAt(req.Id);
            return _storageSvc.SendToFile(storageList, user, req.File);
        }
        else
        {
            return false;
        }
    }
    public FileList ListFiles(string user)
    {
        FileList response = new();
        var path = Path.Combine(_storageSvc.FilePath, user);
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
}