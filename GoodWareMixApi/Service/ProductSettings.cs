using GoodWareMixApi.Model;

namespace GoodWareMixApi.Service
{
    public class ProductSettings<T>
    {
        public List<T> Data { get; set; }

        public long Count { get; set; }
    }
}
