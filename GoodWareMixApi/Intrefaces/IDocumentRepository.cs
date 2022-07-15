using GoodWareMixApi.Model;

namespace GoodWareMixApi.Intrefaces
{
    public interface IDocumentRepository
    {
        public Task<Document> GetIdDocument(string id);
    }
}
