using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodWareMixApi.Model
{
    public class DocumentType
    {
        public string Type { get; set; } // גלוסעמ ObjectID

        public string Title { get; set; }
    }
}
