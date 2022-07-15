using GoodWareMixApi.Controllers;
using GoodWareMixApi.Filter;
using GoodWareMixApi.Model;
using GoodWareMixApi.Model.Entity;
using GoodWareMixApi.Quartz;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.Linq;
using System.Text;

namespace GoodWareMixApi.Service
{
    public class MongoDBService
    {

        private IMongoDatabase db;
        public List<Product> AllProduct = new List<Product>();
        public MongoDBService(string database)
        {
            var client = new MongoClient("mongodb://localhost:27017");//строка подключения
            db = client.GetDatabase(database);
            var filter = new BsonDocument();
            //AllProduct = db.GetCollection<Product>("Product").Find(filter).ToList();
        }

        public async Task StartSheduler()
        {

            List<SchedulerTask> TaskSchedulers = db.GetCollection<SchedulerTask>("Task").Aggregate().ToList();
            foreach (var taskScheduler in TaskSchedulers)
            {
                if (taskScheduler.IsEnable)
                {
                    HttpClient client = new HttpClient();
                    client.Timeout = TimeSpan.FromMinutes(12);
                    string url = $"{Connection.URLPagination}/api/Product/startScheduler/{taskScheduler.NameTask}";
                    //var response = client.GetAsync(url).Result;
                    var a = await client.GetAsync(url);
                }
            }
        }



        public async void InsertRecord<T>(string Table, T record)
        {
            var collection = db.GetCollection<T>(Table);

            await collection.InsertOneAsync(record);
             
        }
        public async System.Threading.Tasks.Task UpdateLog(string Table, Log log)
        {
            var collection = db.GetCollection<Log>(Table);
            var result = await collection.ReplaceOneAsync(x => x.id == log.id, log);
        }
        public void InsertRecordMany<T>(string Table, List<T> record)
        {
            var collection = db.GetCollection<T>(Table);

            collection.InsertMany(record);
        }
        public async Task<SchedulerTask> GetTask(string Table, string nameTask)
        {
            var filter = Builders<SchedulerTask>.Filter.Where(x => x.NameTask == nameTask);
            var supplier = await db.GetCollection<SchedulerTask>(Table).Find(filter).FirstOrDefaultAsync();
            return supplier;
        }
        public async System.Threading.Tasks.Task DeleteTask(string Table, string nameTask)
        {
            var collection = db.GetCollection<SchedulerTask>(Table);
            var result = await collection.DeleteOneAsync(x => x.NameTask == nameTask);
        }
        public async System.Threading.Tasks.Task UpdateTask(string Table, SchedulerTask task)
        {
            var collection = db.GetCollection<SchedulerTask>(Table);
            var result = await collection.ReplaceOneAsync(x => x.Id == task.Id, task);
        }
        public async Task<ProFileSupplier> GetProfileSupplier(string Table, string supplierName)
        {
            var filter = Builders<ProFileSupplier>.Filter.Where(x => x.SupplierName == supplierName);
            var supplier = await db.GetCollection<ProFileSupplier>(Table).Find(filter).FirstOrDefaultAsync();
            return supplier;
        }
        public async Task<ProFileSupplier> GetProfileSupplierByID(string Table, string id)
        {
            var filter = Builders<ProFileSupplier>.Filter.Where(x => x.Id == id);
            var supplier = await db.GetCollection<ProFileSupplier>(Table).Find(filter).FirstOrDefaultAsync();
            return supplier;
        }
        public List<List<string>> GetAttributeName(string Table, string[] attributeName)
        {
            //AttributeEntity Attributes = db.GetCollection<AttributeEntity>("Attribute").Aggregate().Match(x => x.NameAttribute == attributeName).FirstOrDefault();

            List<List<string>> ArrList = new List<List<string>>();



            for (int i = 0; i < attributeName.Length; i++)
            {
                //TODO: Exception
                
                ArrList.Add(db.GetCollection<AttributeEntity>("Attribute").Aggregate().Match(x => x.NameAttribute == attributeName[i]).FirstOrDefault().AllValue);
            }

            //var product1 = db.GetCollection<BsonDocument>(Table).Aggregate()
            // .Lookup("Attribute", "Attributes.AttributeEntity", "_id", "AttributeEntitys").ToList();
            //List<Product> ProductAll = new List<Product>();
            //foreach (var product in product1)
            //{


            //    BsonArray AttributesBD = (BsonArray)product["AttributeEntitys"];
            //    BsonArray AttributesProduct = (BsonArray)product["Attributes"];
            //    for (int i = 0; i < AttributesProduct.Count; i++)
            //    {
            //        for (int h = 0; h < AttributesBD.Count; h++)
            //        {
            //            if (AttributesProduct[i]["AttributeEntity"] == AttributesBD[h]["_id"])
            //            {
            //                AttributesProduct[i]["AttributeEntity"] = AttributesBD[h]["NameAttribute"];
            //                break;
            //            }
            //        }
            //    }
            //    product["Attributes"] = AttributesProduct;
            //    product.AsBsonDocument.Remove("AttributeEntitys");
            //    Product product2 = BsonSerializer.Deserialize<Product>(product);
            //    ProductAll.Add(product2);


            //}
            //List<List<string>> ArrList = new List<List<string>>();
            //for (int h = 0; h < attributeName.Length; h++)
            //{
            //    List<string> a = new List<string>();
            //    foreach (var item in ProductAll)
            //    {
            //        foreach (var items in item.Attributes)
            //        {

            //            if (attributeName[h] == items.AttributeEntity)
            //            {
            //                bool Check = true;
            //                foreach (var itemss in a)
            //                {
            //                    for (int i = 0; i < itemss.Length; i++)
            //                    {
            //                        if (itemss == items.Value)
            //                        {
            //                            Check = false;
            //                            break;
            //                        }
            //                    }
            //                }
            //                if (Check)
            //                {
            //                    if (items.Value != null)
            //                    {
            //                        a.Add(items.Value);
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    ArrList.Add(a);
            //}

            return ArrList;
        }
        public List<string> GetOneAttributeName(string Table, string attributeName)
        {
            List<string> a = new List<string>();
            AttributeEntity Attributes = db.GetCollection<AttributeEntity>("Attribute").Aggregate().Match(x => x.NameAttribute == attributeName).FirstOrDefault();
            //var product1 = db.GetCollection<BsonDocument>(Table).Aggregate()
            // .Lookup("Attribute", "Attributes.AttributeEntity", "_id", "AttributeEntitys").ToList();
            //List<Product> ProductAll = new List<Product>();
            //foreach (var product in product1)
            //{


            //    BsonArray AttributesBD = (BsonArray)product["AttributeEntitys"];
            //    BsonArray AttributesProduct = (BsonArray)product["Attributes"];
            //    for (int i = 0; i < AttributesProduct.Count; i++)
            //    {
            //        for (int h = 0; h < AttributesBD.Count; h++)
            //        {
            //            if (AttributesProduct[i]["AttributeEntity"] == AttributesBD[h]["_id"])
            //            {
            //                AttributesProduct[i]["AttributeEntity"] = AttributesBD[h]["NameAttribute"];
            //                break;
            //            }
            //        }
            //    }
            //    product["Attributes"] = AttributesProduct;
            //    product.AsBsonDocument.Remove("AttributeEntitys");
            //    Product product2 = BsonSerializer.Deserialize<Product>(product);
            //    ProductAll.Add(product2);


            //}
            //foreach (var item in ProductAll)
            //{
            //    foreach (var items in item.Attributes)
            //    {

            //            if (attributeName == items.AttributeEntity)
            //            {
            //                bool Check = true;
            //                foreach (var itemss in a)
            //                {
            //                    for (int i = 0; i < itemss.Length; i++)
            //                    {
            //                        if (itemss == items.Value)
            //                        {
            //                            Check = false;
            //                            break;
            //                        }
            //                    }

            //                }
            //                if (Check)
            //                {
            //                    if (items.Value != null)
            //                    {

            //                        a.Add(items.Value);
            //                    }

            //                }
            //            }




            //    }



            //}


            return Attributes == null ? new List<string>() : Attributes.AllValue;
        }
        public static List<T> RemoveDuplicatesSet<T>(List<T> items)
        {
            // Use HashSet to remember items seen.
            var result = new List<T>();
            var set = new HashSet<T>();
            for (int i = 0; i < items.Count; i++)
            {
                // Add if needed.
                if (!set.Contains(items[i]))
                {
                    result.Add(items[i]);
                    set.Add(items[i]);
                }
            }
            return result;
        }
        public async Task<List<Product>> GetCollectionTable<T>(string Table)
        {
            var filter = new BsonDocument();
            var collection = db.GetCollection<Product>(Table).Find(filter);
            List<Product> collectionList = collection.ToList();
            return collectionList.ToList();
        }
        public List<Product> GetCollectionProductSupplier(string Table, ProFileSupplier supplier)
        {
            var filter = Builders<Product>.Filter.Eq("SupplierId", supplier.Id);
            var collection = db.GetCollection<Product>(Table).Find(filter).ToList();

            List<Product> collectionList = collection.ToList();
            return collectionList.ToList();
        }
        public Product GetProductEntityById(string Table, string Id)
        {
            var searchProduct = db.GetCollection<BsonDocument>(Table).Aggregate()
                .Match(x => x["_id"] == Id)
                .Lookup("Attribute", "Attributes.AttributeEntity", "_id", "AttributeEntitys").ToList();
            var result = (searchProduct.Count == 0 ? null : searchProduct[0]);
            if (result == null)
            {
                return null;
            }
            else
            {
                BsonArray AttributesBD = (BsonArray)result["AttributeEntitys"];
                BsonArray AttributesProduct = (BsonArray)result["Attributes"];
                for (int i = 0; i < AttributesProduct.Count; i++)
                {
                    for (int h = 0; h < AttributesBD.Count; h++)
                    {
                        if (AttributesProduct[i]["AttributeEntity"] == AttributesBD[h]["_id"])
                        {
                            AttributesProduct[i]["AttributeEntity"] = AttributesBD[h]["NameAttribute"];
                            break;
                        }
                    }
                }
                result["Attributes"] = AttributesProduct;
                result.AsBsonDocument.Remove("AttributeEntitys");
                Product product1 = BsonSerializer.Deserialize<Product>(result);
                BsonDocument bsonElements = (BsonDocument)result;
                return product1;

            }



        }
        public Product GetProductEntity(string Table, string internalCode)
        {
            var searchProduct = db.GetCollection<BsonDocument>(Table).Aggregate()
                .Match(x => x["InternalCode"] == internalCode)
                .Lookup("Attribute", "Attributes.AttributeEntity", "_id", "AttributeEntitys").ToList();
            var result = (searchProduct.Count == 0 ? null : searchProduct[0]);
            if (result == null)
            {
                return null;
            }
            else
            {
                BsonArray AttributesBD = (BsonArray)result["AttributeEntitys"];
                BsonArray AttributesProduct = (BsonArray)result["Attributes"];
                for (int i = 0; i < AttributesProduct.Count; i++)
                {
                    for (int h = 0; h < AttributesBD.Count; h++)
                    {
                        if (AttributesProduct[i]["AttributeEntity"] == AttributesBD[h]["_id"])
                        {
                            AttributesProduct[i]["AttributeEntity"] = AttributesBD[h]["NameAttribute"];
                            break;
                        }
                    }
                }
                result["Attributes"] = AttributesProduct;
                result.AsBsonDocument.Remove("AttributeEntitys");
                Product product1 = BsonSerializer.Deserialize<Product>(result);
                BsonDocument bsonElements = (BsonDocument)result;
                return product1;

            }
         


        }
        public ProFileSupplier GetSupplierEntity(string Table, string id)
        {

            var filter = Builders<ProFileSupplier>.Filter.Where(x => x.Id == id || x.SupplierName == id);
            var supplier = db.GetCollection<ProFileSupplier>(Table).Find(filter).FirstOrDefault();
            return supplier;


            //var supplier = db.GetCollection<ProFileSupplier>(Table).Find(filter).FirstOrDefault();
            //return supplier;
        }
        public ProFileSupplier GetSupplierEntityName(string Table, string name)
        {
            var filter = Builders<ProFileSupplier>.Filter.Where(x => x.SupplierName == name);

            var supplier = db.GetCollection<ProFileSupplier>(Table).Find(filter).FirstOrDefault();
            return supplier;
        }
        public Product SearchProductInternalCode(string record, List<Product> products , string prefix)
        {
            Product result = products.FirstOrDefault(x => prefix + x.VendorId == record);
            return result;
        } 
        public Product SearchProduct(Product record, List<Product> products)
        {
            Product result = products.FirstOrDefault(x => x.VendorId == record.VendorId && x.SupplierId == record.SupplierId);
            return result;
        }
        public void UpdateProductInternalCode(List<Product> collectionProduct)
        {
            var collection = db.GetCollection<Product>("Product");
            foreach (var record in collectionProduct)
            {
                var update = Builders<Product>.Update;
                var updates = new List<UpdateDefinition<Product>>();
                updates.Add(update.Set(p => p.InternalCode, record.InternalCode));
                FilterDefinition<Product> filter = Builders<Product>.Filter.Eq(x => x.VendorId, record.VendorId);
                collection.UpdateOne(filter, update.Combine(updates));
            }
            
        }
        public async System.Threading.Tasks.Task UpdatePerson(string Table, Product record)
        {
            var collection = db.GetCollection<Product>(Table);
            var result = await collection.ReplaceOneAsync(x => x.Id == record.Id, record);
        }
        public async System.Threading.Tasks.Task UpdateSupplier(string Table, ProFileSupplier supplier)
        {
            var collection = db.GetCollection<ProFileSupplier>(Table);
            var result = await collection.ReplaceOneAsync(x => x.Id == supplier.Id, supplier);
        }
        public async System.Threading.Tasks.Task DeleteSupplier(string Table, string suppplierName)
        {
            var collection = db.GetCollection<ProFileSupplier>(Table);
            var result = await collection.DeleteOneAsync(x => x.SupplierName == suppplierName);
        }
        public async System.Threading.Tasks.Task DeleteSupplierProduct(string Table, string suppplierName)
        {
            var collection = db.GetCollection<Product>("Product");
            collection.DeleteMany(x => x.SupplierId == suppplierName);
        }
        public async System.Threading.Tasks.Task UpdateAttribute(string Table, List<AttributeEntity> record)
        {

            var updateFilter = Builders<List<AttributeEntity>>.Filter.Empty;
            var updateCollection = new List<WriteModel<AttributeEntity>>();

            foreach (var attr in record)
            {

                var upsert = new ReplaceOneModel<AttributeEntity>(
                        filter: Builders<AttributeEntity>.Filter.Eq(p => p.NameAttribute, attr.NameAttribute),
                        replacement: attr)
                { IsUpsert = true };

                updateCollection.Add(upsert);

            }
            await db.GetCollection<AttributeEntity>(Table).BulkWriteAsync(updateCollection);
        }
        public void UpdateLogOne(Log log)
        {
            var collection = db.GetCollection<Log>("Product");
            var update = Builders<Log>.Update;
            var updates = new List<UpdateDefinition<Log>>();
            if (log.Status != null)
            {
                updates.Add(update.Set(p => p.Status, log.Status));
            }   
            if (log.Result != null)
            {
                updates.Add(update.Set(p => p.Result, log.Result));
            }
            FilterDefinition<Log> filter = Builders<Log>.Filter.Eq(x => x.id, log.id);
            collection.UpdateOne(filter, update.Combine(updates));
        }

        //public void AttributeIdUpdate(List<AttributeEntity> attributeEntities)
        //{
        //    var collection = db.GetCollection<AttributeEntity>("Attribute");
        //    foreach (var record in attributeEntities)
        //    {
        //        var update = Builders<AttributeEntity>.Update;
        //        var updates = new List<UpdateDefinition<AttributeEntity>>();
        //        if (record.SupplierId != null)
        //        {
        //            updates.Add(update.Set(p => p.SupplierId, record.SupplierId));
        //        }
        //        FilterDefinition<AttributeEntity> filter = Builders<AttributeEntity>.Filter.Eq(x => x.Id, record.Id);
        //        collection.UpdateOne(filter, update.Combine(updates));
        //    }
        //}
        public void UpdateProductTest(List<Product> collectionProduct)
        {
            var collection = db.GetCollection<Product>("Product");
            foreach (var record in collectionProduct)
            {
                var update = Builders<Product>.Update;
                var updates = new List<UpdateDefinition<Product>>();
                if (record.Title != null)
                {
                    updates.Add(update.Set(p => p.Title, record.Title));
                }
                if (record.TitleLong != null)
                {
                    updates.Add(update.Set(p => p.TitleLong, record.TitleLong));
                }
                if (record.Description != null)
                {
                    updates.Add(update.Set(p => p.Description, record.Description));
                }
                if (record.Vendor != null)
                {
                    updates.Add(update.Set(p => p.Vendor, record.Vendor));
                }
                if (record.Image360 != null)
                {
                    updates.Add(update.Set(p => p.Image360, record.Image360));
                }
                if (record.Images != null)
                {
                    updates.Add(update.Set(p => p.Images, record.Images));
                }
                if (record.Attributes != null)
                {
                    updates.Add(update.Set(p => p.Attributes, record.Attributes));
                }
                if (record.Documents != null)
                {
                    updates.Add(update.Set(p => p.Documents, record.Documents));
                }
                FilterDefinition<Product> filter = Builders<Product>.Filter.Eq(x => x.Id, record.Id);
                collection.UpdateOne(filter, update.Combine(updates));
            }
        }
        public async System.Threading.Tasks.Task UpdateManyProducts(string Table, List<Product> record)
        {

            var updateFilter = Builders<List<Product>>.Filter.Empty;
            var updateCollection = new List<WriteModel<Product>>();

            foreach (var product in record)
            {

                var upsert = new ReplaceOneModel<Product>(
                        filter: Builders<Product>.Filter.Eq(p => p.VendorId, product.VendorId),
                        replacement: product)
                { IsUpsert = true };

                updateCollection.Add(upsert);

            }
            await db.GetCollection<Product>(Table).BulkWriteAsync(updateCollection);
        }
        public List<T> GetTables<T>(string Table, T Value)
        {
            var filter = new BsonDocument();
            var collection = db.GetCollection<T>(Table).Find(filter);
            List<T> list = collection.ToList();
            return list;
        }
        public List<Document> GetDocumentSearch(string Search)
        {
            var collection = db.GetCollection<Document>("Document").Aggregate().Match(x=> x.SupplierId == Search);
            List<Document> list = collection.ToList();
            return list;   
        }

        public List<Product> GetSupplierIdProduct(string SupplierId)
        {
            var Products = db.GetCollection<Product>("Product").Aggregate().Match(x=>x.SupplierId == SupplierId).ToList();
            return Products;
        }
        public async Task<ProductSettings<AttributeEntity>> GetDataAttribute(string Table, PaginationFilter filter) //PagedResponse
        {
            var bsonFilter = new BsonDocument();
            List<AttributeEntity> AttributeEntitys = new List<AttributeEntity>();
            ProductSettings<AttributeEntity> productSettings = new ProductSettings<AttributeEntity>();
            productSettings.Data = new List<AttributeEntity>();
            if (filter.Search == "") // 0 0
            {
                var CountProduct = db.GetCollection<AttributeEntity>(Table).Aggregate().Count().FirstOrDefault();
                var collection = db.GetCollection<BsonDocument>(Table).Aggregate().SortByDescending(x => x["Rating"]).Skip((filter.PageNumber - 1) * filter.PageSize).Limit(filter.PageSize).Lookup("Supplier", "SupplierId", "_id", "AttributeEntitys11231231").ToListAsync();
                foreach (BsonDocument item in await collection)
                {
                    AttributeEntity Log1 = new AttributeEntity();
                    BsonArray AttributesBD = (BsonArray)item["AttributeEntitys11231231"];
                    var AttributesProduct = item["SupplierId"];


                    for (int h = 0; h < AttributesBD.Count; h++)
                    {
                        if (item["SupplierId"] == AttributesBD[h]["_id"])
                        {
                            item["SupplierId"] = AttributesBD[h]["SupplierName"];
                            break;
                        }
                    }


                    item.AsBsonDocument.Remove("AttributeEntitys11231231");
                    Log1 = BsonSerializer.Deserialize<AttributeEntity>(item);
                    AttributeEntitys.Add(Log1);
                }

                productSettings.Data = AttributeEntitys;
                productSettings.Count = CountProduct == null ? 0 : CountProduct.Count;
                return productSettings;
            }
            else
            {
                var CountProduct = db.GetCollection<AttributeEntity>(Table).Aggregate().Match(x => x.NameAttribute.ToLower().Contains(filter.Search.ToLower())).Count().FirstOrDefault();
                productSettings.Data = db.GetCollection<AttributeEntity>(Table).Aggregate().SortByDescending(x => x.Rating).Match(x => x.NameAttribute.ToLower().Contains(filter.Search.ToLower())).Skip((filter.PageNumber - 1) * filter.PageSize).Limit(filter.PageSize).ToList();
                productSettings.Count = CountProduct == null ? 0 : CountProduct.Count;
                return productSettings;
            }

            return productSettings;
        }

        public async Task<ProductSettings<Product>> GetDataProduct(string Table, PaginationFilter filter) //PagedResponse
        {
            var bsonFilter = new BsonDocument();
            List<Product> products = new List<Product>();
            ProductSettings<Product> productSettings = new ProductSettings<Product>();
            productSettings.Data = new List<Product>();
            var AllProductMongodb = db.GetCollection<Product>(Table).Aggregate();



            if (filter.Search != "")
            {
                AllProductMongodb = AllProductMongodb.Match(x => x.Title.ToLower().Contains(filter.Search.ToLower()) || x.TitleLong.ToLower().Contains(filter.Search.ToLower()) || x.InternalCode == filter.Search || x.Vendor.ToLower() == filter.Search.ToLower() || x.VendorId.ToLower() == filter.Search.ToLower());
            }
            
            if (filter.Attributes.Length != 0)
            {
                List<string> AttributeKey = new List<string>();   //products
                for (int i = 0; i < filter.Attributes.Length; i++)
                {
                    string[] Buf = filter.Attributes[i].Split(';');
                    AttributeEntity attribute = db.GetCollection<AttributeEntity>("Attribute").Aggregate().Match(x => x.NameAttribute == Buf[0]).FirstOrDefault();
                    AllProductMongodb = AllProductMongodb.Match(x => x.Attributes.Any(y => y.Value == Buf[1] && y.AttributeEntity == attribute.Id));

                }
            }
            if (filter.InternalCodeCheckBool != false)
            {
                AllProductMongodb = AllProductMongodb.Match(x => x.InternalCode != null);
            }

            var ad = AllProductMongodb.Lookup("Supplier", "SupplierId", "_id", "AttributeEntitys11231231").ToList();

            foreach (BsonDocument item in ad)
            {
                Product Log1 = new Product();
                BsonArray AttributesBD = (BsonArray)item["AttributeEntitys11231231"];
                var AttributesProduct = item["SupplierId"];


                for (int h = 0; h < AttributesBD.Count; h++)
                {
                    if (item["SupplierId"] == AttributesBD[h]["_id"])
                    {
                        item["SupplierId"] = AttributesBD[h]["SupplierName"];
                        break;
                    }
                    
                }

                item.AsBsonDocument.Remove("AttributeEntitys11231231");
                Log1 = BsonSerializer.Deserialize<Product>(item);
                products.Add(Log1);
            }

            if (filter.Supplier != "")
            {
                products = products.Where(x => x.SupplierId.ToLower() == filter.Supplier.ToLower()).ToList();
            }
            if  (filter.OrderBy == "OrderBy")
            {
                products = products.OrderBy(x => x.CreatedAt).ToList();
            }
            if (filter.OrderBy == "OrderByDescending")
            {
                products = products.OrderByDescending(x => x.CreatedAt).ToList();
            }
            productSettings.Data = products.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).ToList();
            

            if (productSettings.Data.Count == 0)
            {
                productSettings.Count = 0;
            }
            else
            {
                productSettings.Count = products.Count();
            }
            return productSettings;

        }

        public async Task<List<SchedulerTask>> GetDataTask(string Table, PaginationFilter filter)
        {

            List<SchedulerTask> AttributeEntitys = new List<SchedulerTask>();
            if (filter.Search == "")
            {


                var collection = db.GetCollection<BsonDocument>(Table).Aggregate().Skip((filter.PageNumber - 1) * filter.PageSize).Limit(filter.PageSize).Lookup("Supplier", "SupplierId", "_id", "AttributeEntitys11231231").ToListAsync();
                foreach (BsonDocument item in await collection)
                {
                    SchedulerTask Log1 = new SchedulerTask();
                    BsonArray AttributesBD = (BsonArray)item["AttributeEntitys11231231"];
                    var AttributesProduct = item["SupplierId"];


                    for (int h = 0; h < AttributesBD.Count; h++)
                    {
                        if (item["SupplierId"] == AttributesBD[h]["_id"])
                        {
                            item["SupplierId"] = AttributesBD[h]["SupplierName"];
                            break;
                        }
                    }


                    item.AsBsonDocument.Remove("AttributeEntitys11231231");
                    Log1 = BsonSerializer.Deserialize<SchedulerTask>(item);
                    AttributeEntitys.Add(Log1);
                }
                return AttributeEntitys;
            }
            else
            {

                var collection = db.GetCollection<BsonDocument>(Table).Aggregate().Match(x => x["NameTask"].ToString().ToLower().Contains(filter.Search.ToLower())).Skip((filter.PageNumber - 1) * filter.PageSize).Limit(filter.PageSize).Lookup("Supplier", "SupplierId", "_id", "AttributeEntitys11231231").ToListAsync();
                foreach (BsonDocument item in await collection)
                {
                    SchedulerTask Log1 = new SchedulerTask();
                    BsonArray AttributesBD = (BsonArray)item["AttributeEntitys11231231"];
                    var AttributesProduct = item["SupplierId"];


                    for (int h = 0; h < AttributesBD.Count; h++)
                    {
                        if (item["SupplierId"] == AttributesBD[h]["_id"])
                        {
                            item["SupplierId"] = AttributesBD[h]["SupplierName"];
                            break;
                        }
                    }


                    item.AsBsonDocument.Remove("AttributeEntitys11231231");
                    Log1 = BsonSerializer.Deserialize<SchedulerTask>(item);
                    AttributeEntitys.Add(Log1);
                }
                return AttributeEntitys;

            }

        }
        public async Task<List<T>> GetDataSupplier<T>(string Table, PaginationFilter filter)
        {
            if (filter.Search == "")
            {
                var collection = db.GetCollection<T>(Table).Find(new BsonDocument())
              .Skip((filter.PageNumber - 1) * filter.PageSize)
              .Limit(filter.PageSize);
                List<T> list = await collection.ToListAsync();
                return list;
            }
            else
            {
                var collection = db.GetCollection<T>(Table).Aggregate().Match(new BsonDocument("SupplierName", new BsonDocument("$regex", filter.Search)))
              .Skip((filter.PageNumber - 1) * filter.PageSize)
              .Limit(filter.PageSize);
                List<T> list = await collection.ToListAsync();
                return list;
            }

        }
        public async Task<ProductSettings<Log>> GetLogData(string Table, PaginationFilter filter)
        {

            var bsonFilter = new BsonDocument();
            List<Log> AttributeEntitys = new List<Log>();
            ProductSettings<Log> productSettings = new ProductSettings<Log>();
            productSettings.Data = new List<Log>();
            if (filter.Search == "" && filter.Attributes.Length == 0) // 0 0
            {

                var collection = db.GetCollection<BsonDocument>(Table).Aggregate().SortByDescending(x => x["Date"]).Skip((filter.PageNumber - 1) * filter.PageSize).Limit(filter.PageSize).Lookup("Supplier", "SupplierId", "_id", "AttributeEntitys11231231").ToListAsync();
                foreach (BsonDocument item in await collection)
                {
                    Log Log1 = new Log();
                    BsonArray AttributesBD = (BsonArray)item["AttributeEntitys11231231"];
                    var AttributesProduct = item["SupplierId"];


                    for (int h = 0; h < AttributesBD.Count; h++)
                    {
                        if (item["SupplierId"] == AttributesBD[h]["_id"])
                        {
                            item["SupplierId"] = AttributesBD[h]["SupplierName"];
                            break;
                        }
                    }


                    item.AsBsonDocument.Remove("AttributeEntitys11231231");
                    Log1 = BsonSerializer.Deserialize<Log>(item);
                    AttributeEntitys.Add(Log1);
                }

                productSettings.Data = AttributeEntitys;
                productSettings.Count = db.GetCollection<Log>(Table).Aggregate().Count().FirstOrDefault() == null ? 0 : db.GetCollection<Log>(Table).Aggregate().Count().FirstOrDefault().Count;
                return productSettings;
            }
            else
            {
                productSettings.Data = db.GetCollection<Log>(Table).Aggregate().Match(x=> x.SupplierId.ToLower().Contains(filter.Search.ToLower())).SortByDescending(x => x.Date).Skip((filter.PageNumber - 1) * filter.PageSize).Limit(filter.PageSize).ToList();
                productSettings.Count = db.GetCollection<Log>(Table).Aggregate().Match(x => x.SupplierId.ToLower().Contains(filter.Search.ToLower())).Count().FirstOrDefault() == null ? 0 : db.GetCollection<Log>(Table).Aggregate().Count().FirstOrDefault().Count;
                return productSettings;
            }

               
            return productSettings;
            
            //else
            //{
            //    var collection = db.GetCollection<T>(Table).Aggregate().Match(new BsonDocument("SupplierName", new BsonDocument("$regex", filter.Search)))
            //  .Skip((filter.PageNumber - 1) * filter.PageSize)
            //  .Limit(filter.PageSize);
            //    List<T> list = await collection.ToListAsync();
            //    return list;
            //}

        }
        public void UpdateKeyAttribute(AttributeHelper attributeHelper)
        {
 
            List<Product> productsUpdate = new List<Product>();
            List<Product> products = db.GetCollection<Product>("Product").Aggregate().ToList();
            AttributeEntity  attributeEntity = db.GetCollection<AttributeEntity>("Attribute").Aggregate().Match(x=>x.Id == attributeHelper.attributeUpdate).FirstOrDefault();
            List<AttributeEntity> attributeEntities = new List<AttributeEntity>();
            for (int i = 0; i < attributeHelper.attributeList.Count; i++)
            {
                attributeEntities.AddRange(db.GetCollection<AttributeEntity>("Attribute").Aggregate().Match(x=> x.NameAttribute == attributeHelper.attributeList[i]).ToList());
            }
            foreach (var item in attributeEntities)
            {
                attributeEntity.AllValue.AddRange(item.AllValue);
                attributeEntity.Rating = attributeEntity.Rating + item.Rating;
                ProFileSupplier Supplier = db.GetCollection<ProFileSupplier>("Supplier").Aggregate().Match(x => x.Id == item.SupplierId).FirstOrDefault();
                if (Supplier.SupplierConfigs.productAttributeKeys == null)
                {
                    Supplier.SupplierConfigs.productAttributeKeys = new List<ProductAttributeKey>();
                }
                foreach (var UpdateAttributeOne in attributeEntities)
                {
                    ProductAttributeKey productAttributeKey = new ProductAttributeKey();
                    productAttributeKey.AttributeIdBD = attributeEntity.Id;
                    productAttributeKey.AttributeBDName = attributeEntity.NameAttribute;
                    productAttributeKey.KeySupplier = UpdateAttributeOne.NameAttribute;
                    Supplier.SupplierConfigs.productAttributeKeys.Add(productAttributeKey);
                }
                UpdateSupplier("Supplier", Supplier);
            }
            foreach (Product product in products)
            {
                bool Check = false;
                foreach (var attribute in product.Attributes)
                {
                    foreach (var UpdateAttributeOne in attributeEntities)
                    {
                        if (UpdateAttributeOne.Id == attribute.AttributeEntity)
                        {
                            Check = true;
                            attribute.AttributeEntity = attributeEntity.Id;//UpdateOneAttribute
                        }
                    }
                }
                if (Check)
                {
                    productsUpdate.Add(product);
                }
            }
            UpdateProductTest(productsUpdate);
            UpdateOneAttribute(attributeEntity);
            DeleteAttributes(attributeEntities);
        }
        public void AddAttribute(AttributeEntity attribute)
        {
            attribute.Id = ObjectId.GenerateNewId().ToString();
            InsertRecord("Attribute", attribute);
        }
        public async void DeleteOneAttribute(string id)
        {
            List<Product> productsUpdate = new List<Product>();
            var collection = db.GetCollection<AttributeEntity>("Attribute");
            var products  = db.GetCollection<Product>("Product").Aggregate().ToList();
            AttributeEntity attributeEntity = db.GetCollection<AttributeEntity>("Attribute").Aggregate().Match(x => x.Id == id).FirstOrDefault();
            if (attributeEntity != null)
            {
                foreach (var product in products)
                {
                    foreach (var attribute in product.Attributes)
                    {
                        if (attribute.AttributeEntity == attributeEntity.Id)
                        {
                            product.Attributes.Remove(attribute);
                            productsUpdate.Add(product);
                            break;
                        }
                    }
                }
            }
            collection.DeleteOneAsync(x=>x.Id == attributeEntity.Id);
            UpdateProductTest(productsUpdate);
        }

        public Document GetIdDocument(string id)
        {
            var collection = db.GetCollection<Document>("Document").Aggregate().Match(x=> x.Id == id).FirstOrDefault();
            return collection;
        }
        public async void DeleteAttributes(List<AttributeEntity> attribute)
        {
            var collection = db.GetCollection<AttributeEntity>("Attribute");

            foreach (var item in attribute)
            {
                collection.DeleteOneAsync(x=>x.Id == item.Id);
            }
        }
        public void UpdateOneAttribute(AttributeEntity attribute)
        {

            var collection = db.GetCollection<AttributeEntity>("Attribute");
            collection.ReplaceOneAsync(x => x.Id == attribute.Id, attribute);
            //var collection = db.GetCollection<AttributeEntity>("Attribute");

            //var update = Builders<AttributeEntity>.Update;
            //var updates = new List<UpdateDefinition<AttributeEntity>>();
            //if (attribute.AllValue != null)
            //{
            //    updates.Add(update.Set(p => p.AllValue, attribute.AllValue));
            //}
            //FilterDefinition<AttributeEntity> filter = Builders<AttributeEntity>.Filter.Eq(x => x.AllValue, attribute.AllValue);
            //collection.UpdateOne(filter, update.Combine(updates));

        }
        public AttributeEntity GetDataAttributeByID(string Table , string idAttribute)
        {
            AttributeEntity attribute = db.GetCollection<AttributeEntity>(Table).Aggregate().Match(x => x.Id == idAttribute).FirstOrDefault();
            return attribute;
        }
        public async Task<long> GetDataAttributeCount<T>(string Table, PaginationFilter filter)
        {
            if (filter.Search != "")
            {
                var collection = (db.GetCollection<T>(Table).Aggregate().Match(new BsonDocument("NameAttribute", new BsonDocument("$regex", filter.Search)))
               .Count().FirstOrDefault() == null ? 0 : db.GetCollection<T>(Table).Aggregate().Match(new BsonDocument("NameAttribute", new BsonDocument("$regex", filter.Search)))
               .Count().FirstOrDefault().Count);
                return collection;
            }
            else
            {
                var collection = await db.GetCollection<T>(Table).Find(new BsonDocument()).CountDocumentsAsync();
                long count = collection;
                return count;
            }

        }
        public async Task<long> GetDataProductCount<T>(string Table, PaginationFilter filter)
        {
            if (filter.Search == "" && filter.Attributes.Length == 0)
            {
                var collection = await db.GetCollection<T>(Table).Find(new BsonDocument()).CountDocumentsAsync();
                long count = collection;
                return count;
               
            }
            else if (filter.Attributes.Length != 0 && filter.Search != "")
            {
                long number;
                if (long.TryParse(filter.Search, out number))
                {
                    List<Product> products = new List<Product>();
                    var collection = db.GetCollection<T>(Table).Aggregate().Lookup("Attribute", "Attributes.AttributeEntity", "_id", "AttributeEntitys11231231")
                        .Match(new BsonDocument("InternalCode", new BsonDocument("$eq", filter.Search))).ToListAsync();
                    foreach (BsonDocument item in await collection)
                    {
                        Product product1 = new Product();
                        BsonArray AttributesBD = (BsonArray)item["AttributeEntitys11231231"];
                        BsonArray AttributesProduct = (BsonArray)item["Attributes"];
                        for (int i = 0; i < AttributesProduct.Count; i++)
                        {
                            for (int h = 0; h < AttributesBD.Count; h++)
                            {
                                if (AttributesProduct[i]["AttributeEntity"] == AttributesBD[h]["_id"])
                                {
                                    AttributesProduct[i]["AttributeEntity"] = AttributesBD[h]["NameAttribute"];
                                    break;
                                }
                            }
                        }
                        item["Attributes"] = AttributesProduct;
                        item.AsBsonDocument.Remove("AttributeEntitys11231231");
                        product1 = BsonSerializer.Deserialize<Product>(item);
                        products.Add(product1);
                    }
                    List<AttributeProduct> attributes = new List<AttributeProduct>();
                    foreach (var attribute in filter.Attributes)
                    {
                        var attributeClient = attribute.Split(";");
                        AttributeProduct attributeProduct = new AttributeProduct();
                        attributeProduct.AttributeEntity = attributeClient[0];
                        attributeProduct.Value = attributeClient[1];
                        attributes.Add(attributeProduct);
                    }
                    //var attribute = filter.Attributes;
                    List<Product> listNewProduct = new List<Product>();
                    foreach (var item in products)
                    {
                        bool Check = true;
                        foreach (var items in attributes)
                        {
                            bool Check1 = true;
                            foreach (var attr in item.Attributes)
                            {
                                if (items.AttributeEntity == attr.AttributeEntity && items.Value == attr.Value)
                                {
                                    Check1 = false;
                                    break;
                                }
                            }
                            if (Check1)
                            {
                                Check = false;
                                break;
                            }
                        }
                        if (Check)
                        {
                            listNewProduct.Add(item);
                        }
                    }
                    // var newListProduct = products.Where(x => x.Attributes.Select(x => x.AttributeEntity) == filter.Attributes);

                    if (listNewProduct.Count == 0)
                    {
                        return 0;
                    }
                    else
                    {
                        return listNewProduct.Count;
                    }
                    //.Count().FirstOrDefault() == null ? 0 : db.GetCollection<T>(Table).Aggregate().Match(new BsonDocument("InternalCode", new BsonDocument("$eq", filter.Search)))
                    //.Count().FirstOrDefault().Count); 
                    //    return collection;
                }
                else
                {
                    List<Product> products = new List<Product>();
                    var collection = db.GetCollection<T>(Table).Aggregate().Lookup("Attribute", "Attributes.AttributeEntity", "_id", "AttributeEntitys11231231")
                        .Match(new BsonDocument("Title", new BsonDocument("$regex", filter.Search))).ToListAsync();

                    foreach (BsonDocument item in await collection)
                    {
                        Product product1 = new Product();
                        BsonArray AttributesBD = (BsonArray)item["AttributeEntitys11231231"];
                        BsonArray AttributesProduct = (BsonArray)item["Attributes"];
                        for (int i = 0; i < AttributesProduct.Count; i++)
                        {
                            for (int h = 0; h < AttributesBD.Count; h++)
                            {
                                if (AttributesProduct[i]["AttributeEntity"] == AttributesBD[h]["_id"])
                                {
                                    AttributesProduct[i]["AttributeEntity"] = AttributesBD[h]["NameAttribute"];
                                    break;
                                }
                            }
                        }
                        item["Attributes"] = AttributesProduct;
                        item.AsBsonDocument.Remove("AttributeEntitys11231231");
                        product1 = BsonSerializer.Deserialize<Product>(item);
                        products.Add(product1);
                    }
                    List<AttributeProduct> attributes = new List<AttributeProduct>();
                    foreach (var attribute in filter.Attributes)
                    {
                        var attributeClient = attribute.Split(";");
                        AttributeProduct attributeProduct = new AttributeProduct();
                        attributeProduct.AttributeEntity = attributeClient[0];
                        attributeProduct.Value = attributeClient[1];
                        attributes.Add(attributeProduct);
                    }
                    //var attribute = filter.Attributes;
                    List<Product> listNewProduct = new List<Product>();
                    foreach (var item in products)
                    {
                        bool Check = true;
                        foreach (var items in attributes)
                        {
                            bool Check1 = true;
                            foreach (var attr in item.Attributes)
                            {
                                if (items.AttributeEntity == attr.AttributeEntity && items.Value == attr.Value)
                                {
                                    Check1 = false;
                                    break;
                                }
                            }
                            if (Check1)
                            {
                                Check = false;
                                break;
                            }
                        }
                        if (Check)
                        {
                            listNewProduct.Add(item);
                        }
                    }
                    // var newListProduct = products.Where(x => x.Attributes.Select(x => x.AttributeEntity) == filter.Attributes);

                    if (listNewProduct.Count == 0)
                    {
                        return 0;
                    }
                    else
                    {
                        return listNewProduct.Count;
                    }

                }
            }
            else if (filter.Search != "")
            {
                long number;
                if (long.TryParse(filter.Search, out number))
                {
                    List<Product> products = new List<Product>();
                    var collection = db.GetCollection<T>(Table).Aggregate().Lookup("Attribute", "Attributes.AttributeEntity", "_id", "AttributeEntitys11231231")
                        .Match(new BsonDocument("InternalCode", new BsonDocument("$eq", filter.Search))).ToListAsync();
                    foreach (BsonDocument item in await collection)
                    {
                        Product product1 = new Product();
                        BsonArray AttributesBD = (BsonArray)item["AttributeEntitys11231231"];
                        BsonArray AttributesProduct = (BsonArray)item["Attributes"];
                        for (int i = 0; i < AttributesProduct.Count; i++)
                        {
                            for (int h = 0; h < AttributesBD.Count; h++)
                            {
                                if (AttributesProduct[i]["AttributeEntity"] == AttributesBD[h]["_id"])
                                {
                                    AttributesProduct[i]["AttributeEntity"] = AttributesBD[h]["NameAttribute"];
                                    break;
                                }
                            }
                        }
                        item["Attributes"] = AttributesProduct;
                        item.AsBsonDocument.Remove("AttributeEntitys11231231");
                        product1 = BsonSerializer.Deserialize<Product>(item);
                        products.Add(product1);
                    }
                    if (products.Count == 0)
                    {
                        return 0;
                    }
                    else
                    {
                        return products.Count;
                    }
                    //.Count().FirstOrDefault() == null ? 0 : db.GetCollection<T>(Table).Aggregate().Match(new BsonDocument("InternalCode", new BsonDocument("$eq", filter.Search)))
                    //.Count().FirstOrDefault().Count); 
                    //    return collection;
                }
                else
                {
                    List<Product> products = new List<Product>();
                    var collection = db.GetCollection<T>(Table).Aggregate().Lookup("Attribute", "Attributes.AttributeEntity", "_id", "AttributeEntitys11231231")
                        .Match(new BsonDocument("Title", new BsonDocument("$regex", filter.Search))).ToListAsync();

                    foreach (BsonDocument item in await collection)
                    {
                        Product product1 = new Product();
                        BsonArray AttributesBD = (BsonArray)item["AttributeEntitys11231231"];
                        BsonArray AttributesProduct = (BsonArray)item["Attributes"];
                        for (int i = 0; i < AttributesProduct.Count; i++)
                        {
                            for (int h = 0; h < AttributesBD.Count; h++)
                            {
                                if (AttributesProduct[i]["AttributeEntity"] == AttributesBD[h]["_id"])
                                {
                                    AttributesProduct[i]["AttributeEntity"] = AttributesBD[h]["NameAttribute"];
                                    break;
                                }
                            }
                        }
                        item["Attributes"] = AttributesProduct;
                        item.AsBsonDocument.Remove("AttributeEntitys11231231");
                        product1 = BsonSerializer.Deserialize<Product>(item);
                        products.Add(product1);
                    }
                    if (products.Count == 0)
                    {
                        return 0;
                    }
                    else
                    {
                        return products.Count;
                    }

                }

            }
            else if (filter.Attributes.Length != 0)
            {
            
                    List<Product> products = new List<Product>();
                    var collection = db.GetCollection<T>(Table).Aggregate().Lookup("Attribute", "Attributes.AttributeEntity", "_id", "AttributeEntitys11231231")
                    .ToListAsync();
                    foreach (BsonDocument item in await collection)
                    {
                        Product product1 = new Product();
                        BsonArray AttributesBD = (BsonArray)item["AttributeEntitys11231231"];
                        BsonArray AttributesProduct = (BsonArray)item["Attributes"];
                    for (int i = 0; i < AttributesProduct.Count; i++)
                    {
                        for (int h = 0; h < AttributesBD.Count; h++)
                        {
                            if (AttributesProduct[i]["AttributeEntity"] == AttributesBD[h]["_id"])
                            {
                                AttributesProduct[i]["AttributeEntity"] = AttributesBD[h]["NameAttribute"];
                                break;
                            }
                        }
                    }
                    item["Attributes"] = AttributesProduct;
                        item.AsBsonDocument.Remove("AttributeEntitys11231231");
                        product1 = BsonSerializer.Deserialize<Product>(item);
                        products.Add(product1);
                    }
                    List<AttributeProduct> attributes = new List<AttributeProduct>();
                    foreach (var attribute in filter.Attributes)
                    {
                        var attributeClient = attribute.Split(";");
                        AttributeProduct attributeProduct = new AttributeProduct();
                        attributeProduct.AttributeEntity = attributeClient[0];
                        attributeProduct.Value = attributeClient[1];
                        attributes.Add(attributeProduct);
                    }
                    //var attribute = filter.Attributes;
                    List<Product> listNewProduct = new List<Product>();
                //for (int j = 0; j < attributes.Count; j++)
                //{
                //    foreach (var item in products.Where(x => x.Attributes.Any(x => x.AttributeEntity == attributes[j].AttributeEntity
                //      && x.Value == attributes[j].Value)))
                //    {
                //        if (!listNewProduct.Contains(item))
                //        {
                //            listNewProduct.Add(item);
                //        }
                //    }
                //}
                foreach (var item in products)
                {
                    bool Check = true;
                    foreach (var items in attributes)
                    {
                        bool Check1 = true;
                        foreach (var attr in item.Attributes)
                        {
                            if (items.AttributeEntity == attr.AttributeEntity && items.Value == attr.Value)
                            {
                                Check1 = false;
                                break;
                            }
                        }
                        if (Check1)
                        {
                            Check = false;
                            break;
                        }
                    }
                    if (Check)
                    {
                        listNewProduct.Add(item);
                    }
                }
                // var newListProduct = products.Where(x => x.Attributes.Select(x => x.AttributeEntity) == filter.Attributes);

                if (listNewProduct.Count == 0)
                    {
                        return 0;
                    }
                    else
                    {
                        return listNewProduct.Count;
                    }
                    //.Count().FirstOrDefault() == null ? 0 : db.GetCollection<T>(Table).Aggregate().Match(new BsonDocument("InternalCode", new BsonDocument("$eq", filter.Search)))
                    //.Count().FirstOrDefault().Count); 
                    //    return collection;
              
            }
            else
            {
                var collection = await db.GetCollection<T>(Table).Find(new BsonDocument()).CountDocumentsAsync();
                long count = collection;
                return count;
            }
           
        }
        public async Task<long> GetDataTaskCount<T>(string Table, PaginationFilter filter)
        {
            if (filter.Search != "")
            {
                //long count = 0;
                long collection = 0;

                collection = (db.GetCollection<T>(Table).Aggregate().Match(new BsonDocument("NameTask", new BsonDocument("$regex", filter.Search)))
               .Count().FirstOrDefault() == null ? 0 : db.GetCollection<T>(Table).Aggregate().Match(new BsonDocument("NameTask", new BsonDocument("$regex", filter.Search)))
               .Count().FirstOrDefault().Count);

                return collection;
            }
            else
            {
                var collection = await db.GetCollection<T>(Table).Find(new BsonDocument()).CountDocumentsAsync();
                long count = collection;
                return count;
            }
        }
        public async Task<long> GetDataSupplierCount<T>(string Table, PaginationFilter filter)
        {
            if (filter.Search != "")
            {
                //long count = 0;
                long collection = 0;
        
                    collection = (db.GetCollection<T>(Table).Aggregate().Match(new BsonDocument("SupplierName", new BsonDocument("$regex", filter.Search)))
                   .Count().FirstOrDefault() == null ? 0 : db.GetCollection<T>(Table).Aggregate().Match(new BsonDocument("SupplierName", new BsonDocument("$regex", filter.Search)))
                   .Count().FirstOrDefault().Count);                

                return collection;
            }
            else
            {
                var collection = await db.GetCollection<T>(Table).Find(new BsonDocument()).CountDocumentsAsync();
                long count = collection;
                return count;
            }
        }
        public async Task<long> GetDataAttributeCount(string Table, PaginationFilter filter)
        {
            if (filter.Search != "")
            {
                //long count = 0;
                long collection = 0;

                collection = (db.GetCollection<AttributeEntity>(Table).Aggregate().Match(new BsonDocument("NameAttribute", new BsonDocument("$regex", filter.Search)))
               .Count().FirstOrDefault() == null ? 0 : db.GetCollection<AttributeEntity>(Table).Aggregate().Match(new BsonDocument("NameAttribute", new BsonDocument("$regex", filter.Search)))
               .Count().FirstOrDefault().Count);


                return collection;




            }
            else
            {
                var collection = await db.GetCollection<AttributeEntity>(Table).Find(new BsonDocument()).CountDocumentsAsync();
                long count = collection;
                return count;
            }
        }
        public void UpdateCategory(string Table, Category record)
        {
            var collection = db.GetCollection<Category>(Table);
            collection.ReplaceOneAsync(x => x.Id == record.Id, record);
        }
        public void UpdateAttribute(string Table, AttributeEntity record)
        {
            var collection = db.GetCollection<AttributeEntity>(Table);
            collection.ReplaceOneAsync(x => x.Id == record.Id, record);
        }
        public void InsertManyRecord<T>(string Table, List<T> records)
        {
            var collection = db.GetCollection<T>(Table);
          
            if (records.Count != 0)
            {
                collection.InsertMany(records);
            }
        }
        public void DeleteLogs(string Table)
        {
            var result = db.GetCollection<Log>(Table);
            var filter = Builders<Log>.Filter.Empty;
            result.DeleteMany(filter);  
        }
        public void UpdatePerson(string Table, List<Product> record)
        {
            var collection = db.GetCollection<Product>(Table);
            foreach (var item in record)
            {
                collection.ReplaceOneAsync(x => x.Id == item.Id, item);
            }

        }

    }
}
