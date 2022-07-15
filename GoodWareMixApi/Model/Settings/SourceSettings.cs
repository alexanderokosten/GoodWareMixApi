using GoodWareMixApi.Model.Settings;
using System.ComponentModel.DataAnnotations;

namespace GoodWareMixApi.Model
{
    public class SourceSettings
    {
        //[Url(ErrorMessage = "Некорректный адрес")]
        public string? Url { get; set; }
        public string? Prefix { get; set; }

        public string? MethodType { get; set; }
        public string? Header { get; set; }
        public string? Body { get; set; }//
        public string? CountPage { get; set; }
        public string? StartPage { get; set; }
        public string? FileEncoding { get; set; }

    }
}
