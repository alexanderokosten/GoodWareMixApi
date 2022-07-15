using GoodWareMixApi.Intrefaces;
using GoodWareMixApi.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GoodWareMixApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private IDocumentRepository _iDocumentRepository;
        public DocumentController( IDocumentRepository iDocumentRepository)
        {
           this._iDocumentRepository = iDocumentRepository;
        }
        [HttpGet("{id}")]
        public async Task<Document> GetIdDocument(string id)
        {
            Document document = await _iDocumentRepository.GetIdDocument(id);
            return document;
        }
    }
}
