using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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
        [Authorize(Policy = "Bearer")]
        public StorageList GetList(string name)
        {
            return _fileSvc.ReadFromFile(Request.HttpContext.User.Identity.Name, name);
        }
        [HttpGet]
        [Authorize(Policy = "Bearer")]
        public bool CreateList(string name)
        {
            return _fileSvc.CreateFile(Request.HttpContext.User.Identity.Name, name);
        }
        [HttpGet]
        [Authorize(Policy = "Bearer")]
        public FileList ListFiles()
        {
            return _fileSvc.ListFiles(Request.HttpContext.User.Identity.Name);
        }

        [HttpPost]
        [Authorize(Policy = "Bearer")]
        public bool SaveItem(ItemRequest req)
        {
            if (!_fileSvc.WriteToFile(Request.HttpContext.User.Identity.Name, req))
            {
                return false;
            }
            return true;
        }
        [HttpPost]
        [Authorize(Policy = "Bearer")]
        public bool EditItem(ItemEditRequest req)
        {
            return _fileSvc.EditItem(Request.HttpContext.User.Identity.Name, req);
        }
        [HttpPost]
        [Authorize(Policy = "Bearer")]
        public bool ReorderItem(ReorderRequest req)
        {
            return _fileSvc.ReorderItem(Request.HttpContext.User.Identity.Name, req);
        }

        [HttpDelete]
        [Authorize(Policy = "Bearer")]
        public bool DeleteItem(ItemDeleteRequest req)
        {
            return _fileSvc.RemoveItem(Request.HttpContext.User.Identity.Name, req);
        }
        [HttpDelete]
        [Authorize(Policy = "Bearer")]
        public bool DeleteList(string name)
        {
            return _fileSvc.DeleteFile(Request.HttpContext.User.Identity.Name, name);
        }
    }
}
