using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FlatFileStorage.Controllers
{
    [ApiController]
    [Route("[controller]/[Action]")]
    public class FileController : ControllerBase
    {
        private readonly FileService _fileSvc;

        public FileController(FileService fileService)
        {
            _fileSvc = fileService;
        }

        [HttpGet]
        public StorageList GetList(string name)
        {
            return _fileSvc.ReadFromFile(name);
        }
        [HttpGet]
        public bool CreateList(string name)
        {
            return _fileSvc.CreateFile(name);
        }
        [HttpGet]
        public FileList ListFiles()
        {
            return _fileSvc.ListFiles();
        }

        [HttpPost]
        public bool SaveItem(ItemRequest req)
        {
            if (!_fileSvc.WriteToFile(req))
            {
                return false;
            }
            return true;
        }
        [HttpPost]
        public bool EditItem(ItemEditRequest req)
        {
            return _fileSvc.EditItem(req);
        }
    }
}
