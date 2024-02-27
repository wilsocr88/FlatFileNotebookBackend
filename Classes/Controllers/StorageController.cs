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
    public class StorageController : ControllerBase
    {
        private readonly ILogger<StorageController> _log;
        private readonly FileService _fileSvc;

        public StorageController(ILogger<StorageController> log, FileService fileService)
        {
            _log = log;
            _fileSvc = fileService;
        }

        [HttpGet]
        public StorageList GetAll()
        {
            return _fileSvc.ReadFromFile();
        }

        [HttpPost]
        public Item SaveItem(Item item)
        {
            Item response = new Item();
            if (_fileSvc.WriteToFile(item))
            {
                response = item;
            }
            return response;
        }

    }
}
