using GoodWareMixApi.Interfaces;
using GoodWareMixApi.Intrefaces;
using GoodWareMixApi.Model;
using GoodWareMixApi.Service;

namespace GoodWareMixApi.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {

        public async Task<Document> GetIdDocument(string id)
        {
            return Connection.context.GetIdDocument(id);
        }



    }
}
