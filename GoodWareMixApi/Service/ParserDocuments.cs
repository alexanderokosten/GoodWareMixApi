using ExcelDataReader;
using GoodWareMixApi.Model;
using GoodWareMixApi.Model.Entity;
using GoodWareMixApi.Model.Settings;
using GoodWareMixApi.Repositories;
using Microsoft.VisualBasic.FileIO;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Net;
using System.Text;
using System.Xml;

namespace GoodWareMixApi.Service
{
   
    public class ParserDocuments
    {
        public List<string> Urls = new List<string>();
        public Microsoft.AspNetCore.Hosting.IHostingEnvironment environment;
        public Product CSVParserOneProduct(ProFileSupplier ProductesBufBD, string[] ds)
        {
            Product product = new Product();
            product.Id = ObjectId.GenerateNewId().ToString();
            product.SupplierId = ProductesBufBD.Id;
            product.Title = (ProductesBufBD.SupplierConfigs.Title == null ? null : ds[Convert.ToInt32(ProductesBufBD.SupplierConfigs.Title)].ToString());
            product.TitleLong = (ProductesBufBD.SupplierConfigs.TitleLong == null ? null : ds[Convert.ToInt32(ProductesBufBD.SupplierConfigs.TitleLong)].ToString());
            product.Description = (ProductesBufBD.SupplierConfigs.Description == null ? null : ds[Convert.ToInt32(ProductesBufBD.SupplierConfigs.Description)].ToString());
            product.Vendor = (ProductesBufBD.SupplierConfigs.Vendor == null ? null : ds[Convert.ToInt32(ProductesBufBD.SupplierConfigs.Vendor)].ToString());
            product.VendorId = (ProductesBufBD.SupplierConfigs.VendorId == null ? null : ProductesBufBD.SourceSettings.Prefix  + ds[Convert.ToInt32(ProductesBufBD.SupplierConfigs.VendorId)].ToString());
            product.Image360 = (ProductesBufBD.SupplierConfigs.Image360 == null ? null : ds[Convert.ToInt32(ProductesBufBD.SupplierConfigs.Image360)].ToString());
            //product.Categories = (ProductesBufBD.SupplierConfigs.CategoriesStart == null ? null : (string)ds.Rows[i][Convert.ToInt32(ProductesBufBD.SupplierConfigs.CategoriesStart)]);
            product.TitleLong = (ProductesBufBD.SupplierConfigs.TitleLong == null ? null : ds[Convert.ToInt32(ProductesBufBD.SupplierConfigs.TitleLong)].ToString());
            product.Attributes = new List<AttributeProduct>();
            foreach (var item in ProductesBufBD.SupplierConfigs.productAttributeKeys)
            {
                AttributeProduct AttributeProducts = new AttributeProduct();
                AttributeProducts.AttributeEntity = item.AttributeIdBD;
                AttributeProducts.Value = ds[Convert.ToInt32(item.KeySupplier)].ToString();
                product.Attributes.Add(AttributeProducts);
            }
            return product;
        }
        public ProFileSupplier CSVConverKeySupplier(ProFileSupplier ProductesBufBD, string[] ds)
        {

            for (int i = 0; i < ds.Length; i++)
            {
                if (ds[i].ToString() == ProductesBufBD.SupplierConfigs.Title)
                {
                    ProductesBufBD.SupplierConfigs.Title = i.ToString();
                    continue;
                }
                if (ds[i].ToString() == ProductesBufBD.SupplierConfigs.TitleLong)
                {
                    ProductesBufBD.SupplierConfigs.TitleLong = i.ToString();
                    continue;
                }
                if (ds[i].ToString() == ProductesBufBD.SupplierConfigs.Description)
                {
                    ProductesBufBD.SupplierConfigs.Description = i.ToString();
                    continue;
                }
                if (ds[i].ToString() == ProductesBufBD.SupplierConfigs.Vendor)
                {
                    ProductesBufBD.SupplierConfigs.Vendor = i.ToString();
                    continue;
                }
                if (ds[i].ToString() == ProductesBufBD.SupplierConfigs.VendorId)
                {
                    ProductesBufBD.SupplierConfigs.VendorId = i.ToString();
                    continue;
                }
                if (ds[i].ToString() == ProductesBufBD.SupplierConfigs.Images)
                {
                    ProductesBufBD.SupplierConfigs.Images = i.ToString();
                    continue;
                }
                if (ds[i].ToString() == ProductesBufBD.SupplierConfigs.Image360)
                {
                    ProductesBufBD.SupplierConfigs.Image360 = i.ToString();
                    continue;
                }
                if (ds[i].ToString() == ProductesBufBD.SupplierConfigs.Features)
                {
                    ProductesBufBD.SupplierConfigs.Features = i.ToString();
                    continue;
                }
                foreach (var item in ProductesBufBD.SupplierConfigs.productAttributeKeys)
                {
                    if (ds[i].ToString() == item.KeySupplier)
                    {
                        item.KeySupplier = i.ToString();
                        break;
                    }
                }
            }
            return ProductesBufBD;
        }
        public void CSVParser(ProFileSupplier ProductesBufBD)
        {
            List<Product> ProductesAdd = new List<Product>();
            List<Product> ProductesUpdate = new List<Product>();
            List<Product> ProductesAll = new List<Product>();
            List<AttributeEntity> AttributeEntityUpdate = new List<AttributeEntity>();
            List<Product> ProductsList = Connection.context.GetSupplierIdProduct(ProductesBufBD.Id);
            List<AttributeEntity> Attributes = Connection.context.GetTables("Attribute", new Model.AttributeEntity());
            ProductesBufBD = Connection.context.GetTables("Supplier", new Model.ProFileSupplier()).FirstOrDefault(x => x.SupplierName == ProductesBufBD.SupplierName);
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            using (TextFieldParser parser = new TextFieldParser(ProductesBufBD.Connection))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(";");
                string[] fields1 = parser.ReadFields();
                //var aaa = parser.ReadToEnd().Split('\n');
                ProductesBufBD = CSVConverKeySupplier(ProductesBufBD, fields1);
                while (!parser.EndOfData)
                {
                    //Processing row
                    Product productCSV = CSVParserOneProduct(ProductesBufBD, parser.ReadFields());
                    Product ProductUpdate = Connection.context.SearchProduct(productCSV, ProductsList );
                    if (ProductUpdate != null)
                    {
                        if (ProductUpdate.Equals(productCSV))
                        {
                            // Ничегоне делать
                            Console.WriteLine($"{productCSV.VendorId} - Без изменений");
                        }
                        else
                        {
                            // Update
                            productCSV.Id = ProductUpdate.Id;
                            productCSV.CreatedAt = ProductUpdate.CreatedAt;
                            productCSV.UpdatedAt = DateTime.Now;
                            ProductesUpdate.Add(productCSV);
                            Console.WriteLine($"{productCSV.VendorId} - Продукт изменён");
                        }
                    }
                    else
                    {

                        // Insert
                        Console.WriteLine($"{productCSV.VendorId} - Продукта добавлен");
                        foreach (var itemss in productCSV.Attributes)
                        {
                            bool Check = true;
                            foreach (var item in AttributeEntityUpdate)
                            {
                                if (itemss.AttributeEntity == item.Id)
                                {
                                    Check = false;
                                    item.Rating++;
                                    break;
                                }
                            }

                            if (Check)
                            {
                                AttributeEntity attributeBD = Attributes.FirstOrDefault(x => x.Id == itemss.AttributeEntity);
                                attributeBD.Rating++;
                                AttributeEntityUpdate.Add(attributeBD);
                            }

                        }
                        ProductesAdd.Add(productCSV);
                    }

                    ProductesAll.Add(productCSV);

                }

                Connection.context.InsertManyRecord("Product", ProductesAdd);
                Connection.context.UpdateAttribute("Attribute", AttributeEntityUpdate);
                Connection.context.UpdateProductTest(ProductesUpdate);
                // IsDelete (true or false
                ProductesUpdate = new List<Product>();
                ProductsList = Connection.context.GetTables("Product", new Model.Product());
                foreach (var itemlist in ProductsList.Where(x => x.SupplierId == ProductesBufBD.Id.ToString()))
                {
                    bool CheckAtt = true;
                    foreach (var itemall in ProductesAll)
                    {
                        if (itemlist.VendorId == itemall.VendorId)
                        {
                            CheckAtt = false;
                            break;
                        }
                    }
                    if (CheckAtt)
                    {
                        if (!itemlist.IsDeleted)
                        {
                            itemlist.IsDeleted = true;
                            ProductesUpdate.Add(itemlist);
                            Console.WriteLine($"{itemlist.VendorId} - Заморозка"); // УТ000001744653656344746
                        }
                    }
                    else
                    {
                        if (itemlist.IsDeleted)
                        {
                            itemlist.IsDeleted = false;
                            ProductesUpdate.Add(itemlist);
                            Console.WriteLine($"{itemlist.VendorId} - Разморозка");
                        }
                    }

                }
                Connection.context.UpdateProductTest(ProductesUpdate);
            }
        }
        public Product XLSXParserOneProduct(ProFileSupplier ProductesBufBD, DataRow ds, List<AttributeEntity> Attributes)
        {
            List<AttributeEntity> attributeEntitiesUpdate = new List<AttributeEntity>();
            Product product = new Product();
            product.Id = ObjectId.GenerateNewId().ToString();
            product.SupplierId = ProductesBufBD.Id;
            product.Title = (ProductesBufBD.SupplierConfigs.Title == null ? null : ds.ItemArray[Convert.ToInt32(ProductesBufBD.SupplierConfigs.Title)].ToString());
            product.TitleLong = (ProductesBufBD.SupplierConfigs.TitleLong == null ? null : ds.ItemArray[Convert.ToInt32(ProductesBufBD.SupplierConfigs.TitleLong)].ToString());
            product.Description = (ProductesBufBD.SupplierConfigs.Description == null ? null : ds.ItemArray[Convert.ToInt32(ProductesBufBD.SupplierConfigs.Description)].ToString());
            product.Vendor = (ProductesBufBD.SupplierConfigs.Vendor == null ? null : ds.ItemArray[Convert.ToInt32(ProductesBufBD.SupplierConfigs.Vendor)].ToString());
            product.VendorId = (ProductesBufBD.SupplierConfigs.VendorId == null ? null : ProductesBufBD.SourceSettings.Prefix + ds.ItemArray[Convert.ToInt32(ProductesBufBD.SupplierConfigs.VendorId)].ToString());
            product.Image360 = (ProductesBufBD.SupplierConfigs.Image360 == null ? null : ds.ItemArray[Convert.ToInt32(ProductesBufBD.SupplierConfigs.Image360)].ToString());
            //product.Categories = (ProductesBufBD.SupplierConfigs.CategoriesStart == null ? null : (string)ds.Rows[i][Convert.ToInt32(ProductesBufBD.SupplierConfigs.CategoriesStart)]);
            product.TitleLong = (ProductesBufBD.SupplierConfigs.TitleLong == null ? null : ds.ItemArray[Convert.ToInt32(ProductesBufBD.SupplierConfigs.TitleLong)].ToString());
            product.Images = new List<string>();
            string[] Images = ProductesBufBD.SupplierConfigs.Images.Split(";", StringSplitOptions.RemoveEmptyEntries);
            foreach (var Image in Images)
            {
                if (!string.IsNullOrEmpty(ds.ItemArray[Convert.ToInt32(Image)].ToString()))
                {
                    product.Images.Add(ds.ItemArray[Convert.ToInt32(Image)].ToString());
                }                          
            }
            product.Attributes = new List<AttributeProduct>();
            foreach (var item in ProductesBufBD.SupplierConfigs.productAttributeKeys)
            {
                AttributeProduct AttributeProducts = new AttributeProduct();
                AttributeProducts.AttributeEntity = item.AttributeIdBD;
                AttributeProducts.Value = ds.ItemArray[Convert.ToInt32(item.KeySupplier)].ToString();         
                product.Attributes.Add(AttributeProducts);
                AttributeEntity AttributeProduct = Connection.context.GetDataAttributeByID("Attribute", item.AttributeIdBD);
                bool Check = true;
                foreach (var AttributeValue in AttributeProduct.AllValue)
                {
                    if (AttributeValue == AttributeProducts.Value)
                    {
                        Check = false;
                    }
                }
                if (Check)
                {
                    AttributeProduct.AllValue.Add(AttributeProducts.Value);
                    attributeEntitiesUpdate.Add(AttributeProduct);
                }
            }
                Connection.context.UpdateAttribute("Attribute", attributeEntitiesUpdate);
            return product;
        }
        public ProFileSupplier XLSXConverKeySupplier(ProFileSupplier ProductesBufBD, DataRow ds)
        {
            string ImagesSupplier = "";
            for (int i = 0; i < ds.ItemArray.Length; i++)
            {
                if (ds[i].ToString() == ProductesBufBD.SupplierConfigs.Title)
                {
                    ProductesBufBD.SupplierConfigs.Title = i.ToString();
                    continue;
                }
                if (ds[i].ToString() == ProductesBufBD.SupplierConfigs.TitleLong)
                {
                    ProductesBufBD.SupplierConfigs.TitleLong = i.ToString();
                    continue;
                }
                if (ds[i].ToString() == ProductesBufBD.SupplierConfigs.Description)
                {
                    ProductesBufBD.SupplierConfigs.Description = i.ToString();
                    continue;
                }
                if (ds[i].ToString() == ProductesBufBD.SupplierConfigs.Vendor)
                {
                    ProductesBufBD.SupplierConfigs.Vendor = i.ToString();
                    continue;
                }
                if (ds[i].ToString() == ProductesBufBD.SupplierConfigs.VendorId)
                {
                    ProductesBufBD.SupplierConfigs.VendorId = i.ToString();
                    continue;
                }
                if (!string.IsNullOrEmpty(ProductesBufBD.SupplierConfigs.Images))
                {
                    string[] Images = ProductesBufBD.SupplierConfigs.Images.Split(";");
                    foreach (string Image in Images)
                    {
                        if (ds[i].ToString() == Image)
                        {
                            ImagesSupplier += i+ ";";
                            continue;
                        }
                    }
                }
                if (ds[i].ToString() == ProductesBufBD.SupplierConfigs.Images)
                {
                    ProductesBufBD.SupplierConfigs.Images = i.ToString();
                    continue;
                }
                if (ds[i].ToString() == ProductesBufBD.SupplierConfigs.Image360)
                {
                    ProductesBufBD.SupplierConfigs.Image360 = i.ToString();
                    continue;
                }
                if (ds[i].ToString() == ProductesBufBD.SupplierConfigs.Features)
                {
                    ProductesBufBD.SupplierConfigs.Features = i.ToString();
                    continue;
                }
                foreach (var item in (ProductesBufBD.SupplierConfigs.productAttributeKeys == null ? new List<ProductAttributeKey>(): ProductesBufBD.SupplierConfigs.productAttributeKeys))
                {
                    if (ds[i].ToString() == item.KeySupplier)
                    {
                        item.KeySupplier = i.ToString();
                        break;
                    }
                }
            }
            ProductesBufBD.SupplierConfigs.Images = ImagesSupplier;
            return ProductesBufBD;
        }
        public void XLSXBindingAttributeKey(List<ProductAttributeKey> productAttributeKeys, List<AttributeEntity> attributeEntities)
        {
            foreach (var AttributeKey in productAttributeKeys)
            {
                foreach (var Attribute in attributeEntities)
                {
                    if (AttributeKey.AttributeBDName == Attribute.NameAttribute)
                    {
                        AttributeKey.AttributeIdBD = Attribute.Id;
                        break;
                    }
                }
            }
        }
        public void XLSXParse(ProFileSupplier ProductesBufBD, ILogger<ProductRepository> logger, string xlsxdata)
        {

            List<Product> ProductesAdd = new List<Product>();
            List<Product> ProductesUpdate = new List<Product>();
            List<Product> ProductesAll = new List<Product>();
            List<AttributeEntity> AttributeEntityUpdate = new List<AttributeEntity>();
            List<Product> ProductsList = Connection.context.GetSupplierIdProduct(ProductesBufBD.Id);
            ProductsList = ProductsList.Where(x => x.SupplierId == ProductesBufBD.Id).ToList();
            List<AttributeEntity> Attributes = Connection.context.GetTables("Attribute", new Model.AttributeEntity());
            //ProductesBufBD = Connection.context.GetTables("Supplier", new Model.ProFileSupplier()).FirstOrDefault(x => x.SupplierName == ProductesBufBD.SupplierName);
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            XLSXBindingAttributeKey(ProductesBufBD.SupplierConfigs.productAttributeKeys, Attributes);
            using (var stream = File.Open(ProductesBufBD.Connection, FileMode.Open, FileAccess.ReadWrite))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    DataSet ds1 = new DataSet();
                    ds1 = reader.AsDataSet();
                    DataTable ds = ds1.Tables[0];
                    List<int> Attributcol = new List<int>();
                    ProductesBufBD = XLSXConverKeySupplier(ProductesBufBD, ds.Rows[Convert.ToInt32(ProductesBufBD.SupplierConfigs.Input)-1]);
                    for (int i = 0; i < ds.Rows.Count ; i++)
                    {
                        if ((Convert.ToInt32(ProductesBufBD.SupplierConfigs.Input) - 1) < i)
                        {
                            Product productCSV = XLSXParserOneProduct(ProductesBufBD, ds.Rows[i] , Attributes);
                            Product ProductUpdate = Connection.context.SearchProduct(productCSV, ProductsList);
                            if (!string.IsNullOrEmpty(productCSV.VendorId) && !string.IsNullOrEmpty(productCSV.Title))
                            {


                                if (ProductUpdate != null)
                                {
                                    if (ProductUpdate.Equals(productCSV))
                                    {
                                        // Ничегоне делать
                                        logger.LogInformation($"{ProductesBufBD.SupplierName} - {productCSV.VendorId}-Без изменений");

                                    }
                                    else
                                    {
                                        // Update
                                        productCSV.Id = ProductUpdate.Id;
                                        productCSV.CreatedAt = ProductUpdate.CreatedAt;
                                        productCSV.UpdatedAt = DateTime.Now;
                                        ProductesUpdate.Add(productCSV);
                                        logger.LogInformation($"{ProductesBufBD.SupplierName} - {productCSV.VendorId} - Продукт изменён");

                                    }
                                }
                                else
                                {
                                    // Insert
                                    logger.LogInformation($"{ProductesBufBD.SupplierName} - {productCSV.VendorId} - Продукта добавлен");
                                    foreach (var itemss in productCSV.Attributes)
                                    {
                                        bool Check = true;
                                        foreach (var item in AttributeEntityUpdate)
                                        {
                                            if (itemss.AttributeEntity == item.Id)
                                            {
                                                Check = false;
                                                item.Rating++;
                                                break;
                                            }
                                        }
                                        if (Check)
                                        {
                                            AttributeEntity attributeBD = Attributes.FirstOrDefault(x => x.Id == itemss.AttributeEntity);
                                            attributeBD.Rating++;
                                            AttributeEntityUpdate.Add(attributeBD);
                                        }
                                    }
                                    ProductesAdd.Add(productCSV);
                                }
                                ProductesAll.Add(productCSV);
                            }
                            else
                            {
                                logger.LogInformation($"{ProductesBufBD.SupplierName} - {productCSV.VendorId}| {productCSV.Title} - Продукт пропущен");
                            }
                        }
                    }
                    Connection.context.InsertManyRecord("Product", ProductesAdd);
                    Connection.context.UpdateAttribute("Attribute", AttributeEntityUpdate);
                    Connection.context.UpdateProductTest(ProductesUpdate);
                    // IsDelete (true or false
                    //ProductesUpdate = new List<Product>();
                    //ProductsList = Connection.context.GetTables("Product", new Model.Product());
                    //foreach (var itemlist in ProductsList.Where(x => x.SupplierId == ProductesBufBD.Id.ToString()))
                    //{
                    //    bool CheckAtt = true;
                    //    foreach (var itemall in ProductesAll)
                    //    {
                    //        if (itemlist.VendorId == itemall.VendorId)
                    //        {
                    //            CheckAtt = false;
                    //            break;
                    //        }
                    //    }
                    //    if (CheckAtt)
                    //    {
                    //        if (!itemlist.IsDeleted)
                    //        {
                    //            itemlist.IsDeleted = true;
                    //            ProductesUpdate.Add(itemlist);
                    //            Console.WriteLine($"{itemlist.VendorId} - Заморозка");
                    //        }
                    //    }
                    //    else
                    //    {
                    //        if (itemlist.IsDeleted)
                    //        {
                    //            itemlist.IsDeleted = false;
                    //            ProductesUpdate.Add(itemlist);
                    //            Console.WriteLine($"{itemlist.VendorId} - Разморозка");
                    //        }
                    //    }
                    //}
                    //Connection.context.UpdateProductTest(ProductesUpdate);
                }
            }
        }
        private List<string> GetImagesHttpClientJson(string data, ProFileSupplier proFileSupplier)
        {
            JToken? BufXml2;
            JArray start = new JArray();
            List<string> Images = new List<string>();
            List<AttributeProduct> attributeProductReturn = new List<AttributeProduct>();
            if (proFileSupplier.SupplierConfigs.ImageConfig.ImageInput != null)
            {
                var jobj = JObject.Parse(data);
                start = (JArray)jobj[proFileSupplier.SupplierConfigs.ImageConfig.ImageInput];
            }
            else
            {

                start = JArray.Parse(data);
            }
            foreach (var item in start)
            {

                if (proFileSupplier.SupplierConfigs.ImageConfig.ImageName != null)
                {
                    BufXml2 = item.SelectToken(proFileSupplier.SupplierConfigs.ImageConfig.ImageName);
                    if (BufXml2 != null)
                    {
                        Images.Add(BufXml2.ToString());
                    }
                }
            }
            return Images;
        }

        private List<string> GetDocumentHttpClientJson(string json, ProFileSupplier proFileSupplier)
        {
            List<Document> Documents = Connection.context.GetDocumentSearch(proFileSupplier.Id);
            List<Document> AddDocuments = new List<Document>();
            List<string> DocumentsProduct = new List<string>();
            JArray start = new JArray();
            if (proFileSupplier.SupplierConfigs.Input != null)
            {
                var jobj = JObject.Parse(json);
                start = (JArray)jobj[proFileSupplier.SupplierConfigs.Input];
            }
            else
            {

                start = JArray.Parse(json);
            }
            foreach (var item in start)
            {
                var Docs = (proFileSupplier.SupplierConfigs.DocumentsStart == null ? null : (JArray)item[proFileSupplier.SupplierConfigs.DocumentsStart]);
                if (Docs != null)
                {
                    foreach (var Doc in Docs)
                    {
                        Document document = new Document();
                        document.Id = ObjectId.GenerateNewId().ToString();
                        document.SupplierId = proFileSupplier.Id;
                        document.Keywords = new List<string>();
                        document.IsDeleted = false;
                        var Jtoken = Doc.SelectToken("nakjhgfdsame");/////////
                        if (proFileSupplier.SupplierConfigs.Documents.Type != null)
                        {
                            Jtoken = Doc.SelectToken(proFileSupplier.SupplierConfigs.Documents.Type);
                            if (Jtoken != null)
                            {
                                document.Type = Jtoken.ToString();
                                if (Documents.Where(x => x.Type == document.Type).Count() >= 1)
                                {
                                    DocumentsProduct.Add(Documents.Where(x => x.Type == document.Type).FirstOrDefault().Id);
                                    continue;
                                }
                            }
                        }
                        if (proFileSupplier.SupplierConfigs.Documents.Url != null)
                        {
                            Jtoken = Doc.SelectToken(proFileSupplier.SupplierConfigs.Documents.Url);
                            if (Jtoken != null)
                            {
                                document.Url = Jtoken.ToString();
                            }
                        }
                        if (proFileSupplier.SupplierConfigs.Documents.CertId != null)
                        {
                            Jtoken = Doc.SelectToken(proFileSupplier.SupplierConfigs.Documents.CertId);
                            if (Jtoken != null)
                            {
                                document.CertId = Jtoken.ToString();
                            }
                        }
                        if (proFileSupplier.SupplierConfigs.Documents.CertNumber != null)
                        {
                            Jtoken = Doc.SelectToken(proFileSupplier.SupplierConfigs.Documents.CertNumber);
                            if (Jtoken != null)
                            {
                                document.CertNumber = Jtoken.ToString();
                            }
                        }
                        if (proFileSupplier.SupplierConfigs.Documents.CertDescr != null)
                        {
                            Jtoken = Doc.SelectToken(proFileSupplier.SupplierConfigs.Documents.CertDescr);
                            if (Jtoken != null)
                            {
                                document.CertDescr = Jtoken.ToString();
                            }
                        }
                        if (proFileSupplier.SupplierConfigs.Documents.File != null)
                        {
                            Jtoken = Doc.SelectToken(proFileSupplier.SupplierConfigs.Documents.File);
                            if (Jtoken != null)
                            {
                                document.File = Jtoken.ToString();
                            }
                        }
                        if (proFileSupplier.SupplierConfigs.Documents.CertOrganizNumber != null)
                        {
                            Jtoken = Doc.SelectToken(proFileSupplier.SupplierConfigs.Documents.CertOrganizNumber);
                            if (Jtoken != null)
                            {
                                document.CertOrganizNumber = Jtoken.ToString();
                            }
                        }
                        if (proFileSupplier.SupplierConfigs.Documents.CertOrganizDescr != null)
                        {
                            Jtoken = Doc.SelectToken(proFileSupplier.SupplierConfigs.Documents.CertOrganizDescr);
                            if (Jtoken != null)
                            {
                                document.CertOrganizDescr = Jtoken.ToString();
                            }
                        }
                        if (proFileSupplier.SupplierConfigs.Documents.BlankNumber != null)
                        {
                            Jtoken = Doc.SelectToken(proFileSupplier.SupplierConfigs.Documents.BlankNumber);
                            if (Jtoken != null)
                            {
                                document.BlankNumber = Jtoken.ToString();
                            }
                        }
                        if (proFileSupplier.SupplierConfigs.Documents.StartDate != null)
                        {
                            Jtoken = Doc.SelectToken(proFileSupplier.SupplierConfigs.Documents.StartDate);
                            if (Jtoken != null)
                            {
                                document.StartDate = Convert.ToDateTime(Jtoken.ToString());
                            }
                        }
                        if (proFileSupplier.SupplierConfigs.Documents.EndDate != null)
                        {
                            Jtoken = Doc.SelectToken(proFileSupplier.SupplierConfigs.Documents.EndDate);
                            if (Jtoken != null)
                            {
                                document.EndDate = Convert.ToDateTime(Jtoken.ToString());
                            }
                        }

                        //TODO: Keywords как заполнять (List) 
                        if (proFileSupplier.SupplierConfigs.Documents.Keywords != null)
                        {
                            Jtoken = Doc.SelectToken(proFileSupplier.SupplierConfigs.Documents.Keywords);
                            if (Jtoken != null)
                            {
                                // document.Keywords = Jtoken.ToString();
                            }
                        }
                        Documents.Add(document);
                        AddDocuments.Add(document);
                        DocumentsProduct.Add(document.Id);
                    }
                }
            }
            Connection.context.InsertManyRecord("Document", AddDocuments);
            return DocumentsProduct;
        }
        private List<AttributeProduct> JSONParserAttributeHttpClient(ProFileSupplier ProductesBufBD, string json, List<AttributeEntity> Attributes)
        {
            List<Product> ProductesBD = new List<Product>();
            List<AttributeProduct> attributeProductReturn = new List<AttributeProduct>();
            List<Model.AttributeEntity> AttributesAdd = new List<Model.AttributeEntity>();
   
                JArray start = new JArray();
            if (ProductesBufBD.SupplierConfigs.AttributesStart != null)
            {
                var jobj = JObject.Parse(json);
                var tkn = jobj.SelectToken(ProductesBufBD.SupplierConfigs.Input);
                start = (JArray)jobj[ProductesBufBD.SupplierConfigs.Input];
            }
            else
            {
                start = JArray.Parse(json);
            }
     
                foreach (var aItemm in start)
                {
                    var collection = aItemm.SelectToken(ProductesBufBD.SupplierConfigs.AttributesParam.AttributesInputURL);
                    foreach (var aItem in collection)
                    {


                        AttributeEntity attributeBD = new AttributeEntity();
                        attributeBD.Rating = 0;
                    attributeBD.SupplierId = ProductesBufBD.Id;
                        attributeBD.Id = ObjectId.GenerateNewId().ToString();
                        attributeBD.AllValue = new List<string>();
                        bool Check = true;
                        bool CheckList = true;



                        var BufXml2 = aItem.SelectToken(ProductesBufBD.SupplierConfigs.AttributesParam.NameAttribute);
                        if (BufXml2 != null)
                        {
                            attributeBD.NameAttribute = BufXml2.ToString();
                        }


                        foreach (var AttributeKeys in (ProductesBufBD.SupplierConfigs.productAttributeKeys == null ? new List<ProductAttributeKey>() : ProductesBufBD.SupplierConfigs.productAttributeKeys))
                        {
                            if (attributeBD.NameAttribute == AttributeKeys.KeySupplier)
                            {
                                CheckList = false;
                                break;
                            }
                        }
                        if (CheckList)
                        {
                            foreach (AttributeEntity item in Attributes)
                            {
                                if (item.NameAttribute == attributeBD.NameAttribute)
                                {
                                    Check = false;
                                    break;
                                }
                                else
                                {
                                    foreach (var PossibleAttributeNameOne in (item.PossibleAttributeName == null ? new List<string>() : item.PossibleAttributeName))
                                    {
                                        if (PossibleAttributeNameOne == attributeBD.NameAttribute)
                                        {
                                            Check = false;
                                            goto GotThis;
                                        }
                                    }
                                }
                            }
                        GotThis:

                            if (Check)
                            {
                                //Connection.context.InsertRecord("Attribute", attributeBD);
                                AttributesAdd.Add(attributeBD);
                                Attributes.Add(attributeBD);
                                //Attributes.Add(attributeBD);
                            }
                        }




                    AttributeProduct NewAttribute = new AttributeProduct();
                    bool checkAttribut = true;

                    if (ProductesBufBD.SupplierConfigs.AttributesParam.Value != null)
                    {
                        BufXml2 = aItem.SelectToken(ProductesBufBD.SupplierConfigs.AttributesParam.Value);
                        if (BufXml2 != null)
                        {
                            NewAttribute.Value = BufXml2.ToString();
                        }
                        else
                        {
                            if (aItem.ToString() == ProductesBufBD.SupplierConfigs.AttributesParam.Value ? true : false)
                            {
                                NewAttribute.Value = aItem.ToString();
                            }
                        }
                    }


                    foreach (Model.AttributeEntity Attribut in Attributes)
                    {
                        var a = aItem.SelectToken(ProductesBufBD.SupplierConfigs.AttributesParam.NameAttribute);
                        if (Attribut.NameAttribute == (a == null ? null : a.ToString()))
                        {
                            NewAttribute.AttributeEntity = Attribut.Id;
                            checkAttribut = false;
                            bool AttributValue = true;
                            foreach (var Value in Attribut.AllValue)
                            {
                                if (Value == NewAttribute.Value)
                                {
                                    AttributValue = false;
                                }
                            }
                            if (AttributValue)
                            {
                                // Доавить
                                Attribut.AllValue.Add(NewAttribute.Value);
                            }
                            break;
                        }
                    }



                    if (ProductesBufBD.SupplierConfigs.AttributesParam.Unit != null)
                    {
                        BufXml2 = aItem.SelectToken(ProductesBufBD.SupplierConfigs.AttributesParam.Unit);
                        if (BufXml2 != null)
                        {
                            NewAttribute.Unit = BufXml2.ToString();
                        }
                    }
                    if (ProductesBufBD.SupplierConfigs.AttributesParam.Type != null)
                    {
                        BufXml2 = aItem.SelectToken(ProductesBufBD.SupplierConfigs.AttributesParam.Type);
                        if (BufXml2 != null)
                        {
                            NewAttribute.Type = BufXml2.ToString();
                        }
                    }

                    attributeProductReturn.Add(NewAttribute);


                }
                }

            //Connection.context.AttributeIdUpdate( AttributesAdd);
            Connection.context.InsertManyRecord("Attribute", AttributesAdd); //AttributeIdUpdate
            return attributeProductReturn;

            
        }   
        private Product XMLParserOneProduct(ProFileSupplier ProductesBufBD, XmlNode aItem, List<AttributeEntity> Attributes)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "EnergoMix");
            Product Buf = new Product();
            List<AttributeEntity> attributeEntitiesUpdate = new List<AttributeEntity>();
            Buf.SupplierId = ProductesBufBD.Id.ToString();
            Buf.Id = ObjectId.GenerateNewId().ToString();
            Buf.Features = new List<string>();
            Buf.Attributes = new List<AttributeProduct>();
            Buf.Images = new List<string>();
            XmlNode? BufXml2 = aItem.SelectSingleNode("nakjhgfdsame");/////////
            if (aItem.NamespaceURI != "")
            {
                aItem.InnerXml = aItem.InnerXml.Replace(aItem.NamespaceURI, "");
            }

            if (ProductesBufBD.SupplierConfigs.Title != null)
            {
                XmlNode? BufXml22 = aItem.SelectSingleNode(ProductesBufBD.SupplierConfigs.Title);
                if (BufXml22 != null)
                {
                    Buf.Title = BufXml22.InnerText;
               }          
            }
            if (ProductesBufBD.SupplierConfigs.Vendor != null)
            {
                BufXml2 = aItem.SelectSingleNode(ProductesBufBD.SupplierConfigs.Vendor);
                if (BufXml2 != null)
                {
                    Buf.Vendor = BufXml2.InnerText;
                }        
            }
            if (ProductesBufBD.SupplierConfigs.VendorId != null)
            {
                BufXml2 = aItem.SelectSingleNode(ProductesBufBD.SupplierConfigs.VendorId);
                if (BufXml2 != null)
                {
                    Buf.VendorId = BufXml2.InnerText;
                }         
            }
            if (ProductesBufBD.SupplierConfigs.TitleLong != null)
            {
                BufXml2 = aItem.SelectSingleNode(ProductesBufBD.SupplierConfigs.TitleLong);
                if (BufXml2 != null)
                {
                    Buf.TitleLong = BufXml2.InnerText;
                }
            }
            if (ProductesBufBD.SupplierConfigs.Description != null)
            {
                BufXml2 = aItem.SelectSingleNode(ProductesBufBD.SupplierConfigs.Description);
                if (BufXml2 != null)
                {
                    Buf.Description = BufXml2.InnerText;
                }              
            }
            if (ProductesBufBD.SupplierConfigs.Image360 != null)
            {
                BufXml2 = aItem.SelectSingleNode(ProductesBufBD.SupplierConfigs.Image360);
                if (BufXml2 != null)
                {
                    Buf.Image360 = BufXml2.InnerText;
                } 
            }
            if (ProductesBufBD.SupplierConfigs.Images != null)
            {
                XmlNodeList? Param = (XmlNodeList)aItem.SelectNodes(ProductesBufBD.SupplierConfigs.Images); ///
                for (int i = 0; i < Param.Count; i++)
                {
                    Buf.Images.Add(ParserImage(Param[i].InnerText));
                }
            }
            //if (ProductesBufBD.SupplierConfigs.ImageConfig.ImageUrl != null)
            //{
            //    var supplierHeader = JsonConvert.DeserializeObject<List<HeaderContent>>(ProductesBufBD.SourceSettings.Header);///////////
            //    foreach (var item in supplierHeader)
            //    {
            //        client.DefaultRequestHeaders.Add(item.HeaderName, item.HeaderValue);
            //    }
            //    string response = client.GetStringAsync(ProductesBufBD.SupplierConfigs.ImageConfig.ImageUrl).Result;
            //    Buf.Attributes.AddRange(GetAttributeHttpClientJson(response, ProductesBufBD, Attributes));
            //}
            if (ProductesBufBD.SupplierConfigs.Features != null)
            {
                XmlNodeList? Param15 = (XmlNodeList)aItem.SelectNodes(ProductesBufBD.SupplierConfigs.Features);
                for (int i = 0; i < Param15.Count; i++)
                {
                    Buf.Features.Add(Param15[i].InnerText);
                }
            }
            if (ProductesBufBD.SupplierConfigs.AttributesStart != null)
            {


                if (ProductesBufBD.SupplierConfigs.AttributesURL != null)
                {

                    //var supplierHeader = JsonConvert.DeserializeObject<List<HeaderContent>>(ProductesBufBD.SourceSettings.Header);///////////
                    //foreach (var item in supplierHeader)
                    //{
                    //    client.DefaultRequestHeaders.Add(item.HeaderName, item.HeaderValue);
                    //}
                    //string response = client.GetStringAsync(ProductesBufBD.SupplierConfigs.AttributesURL).Result;
                    //Buf.Attributes.AddRange(GetAttributeHttpClientJson(response, ProductesBufBD, Attributes));
                }
                else
                {
                    XmlNodeList? AttributesStart = (XmlNodeList)aItem.SelectNodes(ProductesBufBD.SupplierConfigs.AttributesStart);

                    foreach (XmlNode item in AttributesStart)
                    {
                        AttributeProduct NewAttribute = new AttributeProduct();

                        if (ProductesBufBD.SupplierConfigs.AttributesParam.Value != null)
                        {

                            BufXml2 = item.SelectSingleNode(ProductesBufBD.SupplierConfigs.AttributesParam.Value);
                            if (BufXml2 != null)
                            {
                                NewAttribute.Value = BufXml2.InnerText;
                            }
                            else
                            {
                                if (item.Name == ProductesBufBD.SupplierConfigs.AttributesParam.Value ? true : false)
                                {
                                    NewAttribute.Value = item.InnerText;
                                }
                            }

                        }
                        if (ProductesBufBD.SupplierConfigs.AttributesParam.NameAttribute != null)
                        {
                            bool checkAttribut = true;
                            foreach (AttributeEntity Attribut in Attributes)
                            {

                                if (Attribut.NameAttribute == item.SelectSingleNode(ProductesBufBD.SupplierConfigs.AttributesParam.NameAttribute).InnerText)
                                {
                                    NewAttribute.AttributeEntity = Attribut.Id;
                                    checkAttribut = false;
                                    bool AttributValue = true;
                                    foreach (var Value in Attribut.AllValue)
                                    {
                                        if (Value == NewAttribute.Value)
                                        {
                                            AttributValue = false;
                                        }
                                    }
                                    if (AttributValue)
                                    {
                                        // Доавить
                                        Attribut.AllValue.Add(NewAttribute.Value);
                                    }
                                    break;
                                }
                                else
                                {
                                    if (Attribut.PossibleAttributeName != null)
                                    {
                                        foreach (var PossibleAttributeNameOne in Attribut.PossibleAttributeName)
                                        {
                                            if (PossibleAttributeNameOne == item.SelectSingleNode(ProductesBufBD.SupplierConfigs.AttributesParam.NameAttribute).InnerText)
                                            {
                                                NewAttribute.AttributeEntity = Attribut.Id;
                                                checkAttribut = false;
                                                bool AttributValue = true;
                                                foreach (var Value in Attribut.AllValue)
                                                {
                                                    if (Value == NewAttribute.Value)
                                                    {
                                                        AttributValue = false;
                                                    }
                                                }
                                                if (AttributValue)
                                                {
                                                    // Доавить
                                                    Attribut.AllValue.Add(NewAttribute.Value);
                                                }
                                                goto LoopEnd;
                                            }
                                        }
                                    }
                                }

                            }
                        }
                    LoopEnd:
                        if (ProductesBufBD.SupplierConfigs.AttributesParam.Unit != null)
                        {
                            BufXml2 = item.SelectSingleNode(ProductesBufBD.SupplierConfigs.AttributesParam.Unit);
                            if (BufXml2 != null)
                            {
                                NewAttribute.Unit = BufXml2.InnerText;
                            }
                        }
                        if (ProductesBufBD.SupplierConfigs.AttributesParam.Type != null)
                        {
                            BufXml2 = item.SelectSingleNode(ProductesBufBD.SupplierConfigs.AttributesParam.Type);
                            if (BufXml2 != null)
                            {
                                NewAttribute.Type = BufXml2.InnerText;
                            }
                        }


                        Buf.Attributes.Add(NewAttribute);
                    }
                }

            }
            //Documents
            // ???

            //Packages
            if (ProductesBufBD.SupplierConfigs.PackagesStart != null)
            {
                XmlNodeList? PackagesList = (XmlNodeList)aItem.SelectNodes(ProductesBufBD.SupplierConfigs.PackagesStart);
                try
                {
                    foreach (XmlNode item in PackagesList)
                    {
                        Package package = new Package();

                        if (ProductesBufBD.SupplierConfigs.Packages.Barcode != null)
                        {
                            BufXml2 = item.SelectSingleNode(ProductesBufBD.SupplierConfigs.Packages.Barcode);
                            if (BufXml2 != null)
                            {
                                package.Barcode = BufXml2.InnerText;
                            }
                        }
                        if (ProductesBufBD.SupplierConfigs.Packages.Type != null)
                        {
                            BufXml2 = item.SelectSingleNode(ProductesBufBD.SupplierConfigs.Packages.Type);
                            if (BufXml2 != null)
                            {
                                package.Weight = (float)Convert.ToDouble(BufXml2.InnerText);
                            }
                        }
                        if (ProductesBufBD.SupplierConfigs.Packages.Volume != null)
                        {
                            BufXml2 = item.SelectSingleNode(ProductesBufBD.SupplierConfigs.Packages.Volume);
                            if (BufXml2 != null)
                            {
                                package.Volume = (float)Convert.ToDouble(BufXml2.InnerText);
                            }
                        }
                        if (ProductesBufBD.SupplierConfigs.Packages.PackQty != null)
                        {
                            BufXml2 = item.SelectSingleNode(ProductesBufBD.SupplierConfigs.Packages.PackQty);
                            if (BufXml2 != null)
                            {
                                package.PackQty = Convert.ToInt32(BufXml2.InnerText);
                            }
                        }
                        if (ProductesBufBD.SupplierConfigs.Packages.Height != null)
                        {
                            BufXml2 = item.SelectSingleNode(ProductesBufBD.SupplierConfigs.Packages.Height);
                            if (BufXml2 != null)
                            {
                                package.Height = Convert.ToInt32(BufXml2.InnerText);
                            }
                        }
                        if (ProductesBufBD.SupplierConfigs.Packages.Width != null)
                        {
                            BufXml2 = item.SelectSingleNode(ProductesBufBD.SupplierConfigs.Packages.Width);
                            if (BufXml2 != null)
                            {
                                package.Width = Convert.ToInt32(BufXml2.InnerText);
                            }
                        }
                        if (ProductesBufBD.SupplierConfigs.Packages.Length != null)
                        {
                            BufXml2 = item.SelectSingleNode(ProductesBufBD.SupplierConfigs.Packages.Length);
                            if (BufXml2 != null)
                            {
                                package.Length = Convert.ToInt32(BufXml2.InnerText);
                            }
                        }
                        if (ProductesBufBD.SupplierConfigs.Packages.Depth != null)
                        {
                            BufXml2 = item.SelectSingleNode(ProductesBufBD.SupplierConfigs.Packages.Depth);
                            if (BufXml2 != null)
                            {
                                package.Depth = Convert.ToInt32(BufXml2.InnerText);
                            }
                        }
                        if (ProductesBufBD.SupplierConfigs.Packages.Weight != null)
                        {
                            BufXml2 = item.SelectSingleNode(ProductesBufBD.SupplierConfigs.Packages.Weight);
                            if (BufXml2 != null)
                            {
                                package.Weight = (float)Convert.ToDouble(BufXml2.InnerText);
                            }
                        }



                        Buf.Packages.Add(package);
                    }
                }
                catch
                {
                    var PackagesStart = aItem.SelectSingleNode(ProductesBufBD.SupplierConfigs.PackagesStart);

                    //TODO:доделать 
                    XmlNodeList Barcodes = PackagesStart.SelectNodes(ProductesBufBD.SupplierConfigs.Packages.Barcode == null ? "" : ProductesBufBD.SupplierConfigs.Packages.Barcode);
                    XmlNodeList Types = PackagesStart.SelectNodes(ProductesBufBD.SupplierConfigs.Packages.Type == null ? "" : ProductesBufBD.SupplierConfigs.Packages.Type);
                    XmlNodeList Volumes = PackagesStart.SelectNodes(ProductesBufBD.SupplierConfigs.Packages.Volume == null ? "" : ProductesBufBD.SupplierConfigs.Packages.Volume);
                    XmlNodeList PackQtys = PackagesStart.SelectNodes(ProductesBufBD.SupplierConfigs.Packages.PackQty == null ? "" : ProductesBufBD.SupplierConfigs.Packages.PackQty);
                    XmlNodeList Heights = PackagesStart.SelectNodes(ProductesBufBD.SupplierConfigs.Packages.Height == null ? "" : ProductesBufBD.SupplierConfigs.Packages.Height);
                    XmlNodeList Widths = PackagesStart.SelectNodes(ProductesBufBD.SupplierConfigs.Packages.Width == null ? "" : ProductesBufBD.SupplierConfigs.Packages.Width);
                    XmlNodeList Lengths = PackagesStart.SelectNodes(ProductesBufBD.SupplierConfigs.Packages.Length == null ? "" : ProductesBufBD.SupplierConfigs.Packages.Length);
                    XmlNodeList Depths = PackagesStart.SelectNodes(ProductesBufBD.SupplierConfigs.Packages.Depth == null ? "" : ProductesBufBD.SupplierConfigs.Packages.Depth);
                    XmlNodeList Weights = PackagesStart.SelectNodes(ProductesBufBD.SupplierConfigs.Packages.Weight == null ? "" : ProductesBufBD.SupplierConfigs.Packages.Weight);



                    if (!string.IsNullOrEmpty(ProductesBufBD.SupplierConfigs.Packages.Barcode))
                    {

                    }
               
                    bool Check = true;
                    for (int i = 0; Check == true; i++)
                    {
                        Package package = new Package();
                        if (Barcodes.Count > i)
                        {
                            package.Barcode = Barcodes[i].ToString();
                        }
                        if (Types.Count > i)
                        {
                            package.Type = Types[i].ToString();
                        }
                        if (Volumes.Count > i)
                        {
                            package.Volume = (float)Convert.ToDouble(Volumes[i].ToString());
                        }
                        if (PackQtys.Count > i)
                        {
                            package.PackQty = Convert.ToInt32(PackQtys[i].ToString());
                        }
                        if (Heights.Count > i)
                        {
                            package.Height = Convert.ToInt32(Heights[i].ToString());
                        }
                        if (Widths.Count > i)
                        {
                            package.Width = Convert.ToInt32(Widths[i].ToString());
                        }
                        if (Lengths.Count > i)
                        {
                            package.Length = Convert.ToInt32(Lengths[i].ToString());
                        }
                        if (Depths.Count > i)
                        {
                            package.Depth = Convert.ToInt32(Depths[i].ToString());
                        }
                        if (Weights.Count > i)
                        {
                            package.Weight = (float)Convert.ToDouble(Weights[i].ToString());
                        }

                        if (!string.IsNullOrEmpty(package.Barcode) ||
                            !string.IsNullOrEmpty(package.Type) 

                            )
                        {

                        }
                    }
                }
            }

            ////Category
            if (ProductesBufBD.SupplierConfigs.CategoriesProduct != null)
            {
                BufXml2 = aItem.SelectSingleNode(ProductesBufBD.SupplierConfigs.CategoriesProduct);
                List<Category> bufCategory = Connection.context.GetTables("Categories", new Category());
                bool check1 = true;
                Category CategorysId = XMLPatserCategoryRecursion(BufXml2.InnerText, bufCategory);
                if (CategorysId != null)
                {
                    Buf.Categories = CategorysId.Id;
                }

            }
            Buf.UpdatedAt = DateTime.Now;
            Buf.CreatedAt = DateTime.Now;
            Buf.IsDeleted = false;
            //Connection.context.UpdateAttribute("Attribute", attributeEntitiesUpdate);
            return Buf;
        }
        public Category XMLPatserCategoryRecursion(string BufXml2, List<Category> bufCategory)
        {
            foreach (var item in bufCategory)
            {
                if (BufXml2 == item.VenderId)
                {
                    return item;
                }
                else
                {
                    if (item.SubCategories != null)
                    {
                        Category Buf = XMLPatserCategoryRecursion(BufXml2, item.SubCategories);
                        if (Buf != null)
                        {
                            return Buf;
                        }
                    }

                }
            }
            return null;
        }
        public void XMLParserAttribute(ProFileSupplier ProductesBufBD , XmlDocument xDoc)
        {
            List<Product> ProductesBD = new List<Product>();
            List<AttributeEntity> Attributes = new List<AttributeEntity>();
            List<AttributeEntity> AttributesAdd = new List<AttributeEntity>();
            Attributes = Connection.context.GetTables("Attribute", new AttributeEntity());
            var a = xDoc.GetElementsByTagName(ProductesBufBD.SupplierConfigs.Input);
            foreach (XmlNode? aItem in a)
            {
                if (aItem.NamespaceURI != "")
                {
                    aItem.InnerXml = aItem.InnerXml.Replace(aItem.NamespaceURI, "");
                }
                XmlNodeList? Param = (XmlNodeList)aItem.SelectNodes(ProductesBufBD.SupplierConfigs.AttributesStart);
                foreach (XmlNode? aItems in Param)
                {
                    AttributeEntity attributeBD = new AttributeEntity();
                    attributeBD.Rating = 0;
                    attributeBD.SupplierId = ProductesBufBD.Id;
                    attributeBD.Id = ObjectId.GenerateNewId().ToString();
                    attributeBD.AllValue = new List<string>();
                    attributeBD.PossibleAttributeName = new List<string>();
                    bool Check = true;
                    bool CheckList = true;
                    if (ProductesBufBD.SupplierConfigs.AttributesParam.NameAttribute != null)
                    {
                        XmlNode? BufXml2 = aItems.SelectSingleNode(ProductesBufBD.SupplierConfigs.AttributesParam.NameAttribute);
                        if (BufXml2 != null)
                        {
                            attributeBD.NameAttribute = BufXml2.InnerText;
                        }
                    }
                    foreach (var AttributeKeys in (ProductesBufBD.SupplierConfigs.productAttributeKeys == null ? new List<ProductAttributeKey>() : ProductesBufBD.SupplierConfigs.productAttributeKeys))
                    {
                        if (attributeBD.NameAttribute == AttributeKeys.KeySupplier)
                        {
                            CheckList = false;
                            break;
                        }
                    }
                    if (CheckList)
                    {
                        foreach (AttributeEntity item in Attributes)
                        {
                            if (item.NameAttribute == attributeBD.NameAttribute)
                            {
                                Check = false;
                                break;
                            }
                            else
                            {
                                foreach (var PossibleAttributeNameOne in (item.PossibleAttributeName == null ? new List<string>() : item.PossibleAttributeName))
                                {
                                    if (PossibleAttributeNameOne == attributeBD.NameAttribute)
                                    {
                                        Check = false;
                                        goto GotThis;
                                    }
                                }
                            }
                        }
                        GotThis: 
                        if (Check)
                        {
                            AttributesAdd.Add(attributeBD);
                            Attributes.Add(attributeBD);
                        }
                    }

                }
            }
            Connection.context.InsertManyRecord("Attribute", AttributesAdd);
        }
        public void XMLParserCategory(ProFileSupplier ProductesBufBD)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(ProductesBufBD.Connection);
            List<Model.Category> CategorysAll = Connection.context.GetTables("Categories", new Model.Category());
            if (ProductesBufBD.SupplierConfigs.CategoriesStart != null)
            {


                var a = xDoc.GetElementsByTagName(ProductesBufBD.SupplierConfigs.CategoriesStart);

                foreach (var items in ProductesBufBD.SupplierConfigs.Categories)
                {
                    //List<Model.Category> Categories = db.GetTables("Categories", new Model.Category());
                    foreach (XmlNode item in a)
                    {
                        Category attributeBD = new Category();

                        attributeBD.Id = ObjectId.GenerateNewId().ToString();

                        if (item.Attributes.Count == 1)
                        {
                            bool Check = true;
                            foreach (var itemsss in CategorysAll)
                            {
                                if (item.Attributes.GetNamedItem(items.VenderId).InnerXml == itemsss.VenderId)
                                {
                                    Check = false;
                                    break;
                                }
                            }
                            if (Check)
                            {



                                if (items.VenderId != null)
                                {
                                    XmlNode? BufXml2 = item.Attributes.GetNamedItem(items.VenderId);
                                    if (BufXml2 != null)
                                    {
                                        attributeBD.VenderId = BufXml2.InnerText;
                                    }
                                }

                                if (items.Description != null)
                                {
                                    XmlNode? BufXml2 = item.Attributes.GetNamedItem(items.Description);
                                    if (BufXml2 != null)
                                    {
                                        attributeBD.Description = BufXml2.InnerText;
                                    }
                                }

                                if (items.Title != null)
                                {
                                    XmlNode? BufXml2 = item.Attributes.GetNamedItem(items.Title);
                                    if (BufXml2 != null)
                                    {
                                        attributeBD.Title = BufXml2.InnerText;
                                    }
                                    else
                                    {
                                        if (items.Title == ProductesBufBD.SupplierConfigs.CategoriesStart ? true : false)
                                        {
                                            attributeBD.Title = item.InnerText;
                                        }
                                    }
                                }
                                Connection.context.InsertRecord("Categories", attributeBD);
                                CategorysAll.Add(attributeBD);
                            }
                        }
                        else
                        {
                            if (items.SubCategories != null)
                            {
                                bool Check = true;
                                Category CategorysId = XMLPatserCategoryRecursion(item.Attributes.GetNamedItem(items.SubCategories).InnerXml, CategorysAll);


                                if (items.VenderId != null)
                                {
                                    XmlNode? BufXml2 = item.Attributes.GetNamedItem(items.VenderId);
                                    if (BufXml2 != null)
                                    {
                                        attributeBD.VenderId = BufXml2.InnerText;
                                    }
                                }

                                if (items.Description != null)
                                {
                                    XmlNode? BufXml2 = item.Attributes.GetNamedItem(items.Description);
                                    if (BufXml2 != null)
                                    {
                                        attributeBD.Description = BufXml2.InnerText;
                                    }
                                }

                                if (items.Title != null)
                                {
                                    XmlNode? BufXml2 = item.Attributes.GetNamedItem(items.Title);
                                    if (BufXml2 != null)
                                    {
                                        attributeBD.Title = BufXml2.InnerText;
                                    }
                                    else
                                    {
                                        if (items.Title == ProductesBufBD.SupplierConfigs.CategoriesStart ? true : false)
                                        {
                                            attributeBD.Title = item.InnerText;
                                        }
                                    }
                                }
                                if (CategorysId.SubCategories == null)
                                {
                                    CategorysId.SubCategories = new List<Category>();
                                }
                                if (XMLPatserCategoryRecursion(attributeBD.VenderId, CategorysAll) == null)
                                {
                                    CategorysId.SubCategories.Add(attributeBD);
                                    Connection.context.UpdateCategory("Categories", CategorysId);
                                }
                            }

                        }
                    }
                }
            }


        }
        public bool XMLParser(ProFileSupplier ProductesBufBD, ILogger<ProductRepository> logger , string xmldata)
        {
            Log logEntity = new Log();
            logEntity.id = ObjectId.GenerateNewId().ToString();
            logEntity.SupplierId = ProductesBufBD.Id;
            logEntity.Date = DateTime.Now;
            logEntity.Status = "Данные обрабатываются...";
            logEntity.Result = "В процессе";
            Connection.context.InsertRecord("Log", logEntity);




            List<string> InfoProduct = new List<string>();
            InfoProduct.Add($"\n---------Информация о поставщике---------\n");
            InfoProduct.Add($"Время импорта: {DateTime.Now}\n");
            InfoProduct.Add($"ID - поставщика: {ProductesBufBD.Id}\n");
            InfoProduct.Add($"Поставщик: {ProductesBufBD.SupplierName}\n");
            InfoProduct.Add($"Путь до файла на сервере: {ProductesBufBD.Connection} \n\n");
            InfoProduct.Add($"---------РЕЗУЛЬТАТ ИМПОРТА---------\n");

            try
            {

            

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            List<Product> ProductesAdd = new List<Product>();
            List<Product> ProductesUpdate = new List<Product>();
            List<Product> ProductesAll = new List<Product>();
            List<AttributeEntity> AttributeEntityUpdate = new List<AttributeEntity>();
            XmlDocument xDoc = new XmlDocument();
                foreach (var AttributeKeys in (ProductesBufBD.SupplierConfigs.productAttributeKeys == null ? new List<ProductAttributeKey>() : ProductesBufBD.SupplierConfigs.productAttributeKeys))
                {
                    xmldata = xmldata.Replace(AttributeKeys.KeySupplier, AttributeKeys.AttributeBDName);
                }

                if (xmldata != null)
                {
                    xDoc.InnerXml = xmldata;
                }
                else
                {
                    xDoc.Load(ProductesBufBD.Connection);
                }
                //xDoc.Load(xmldata);

                if (ProductesBufBD.SupplierConfigs.AttributesURL == null)
            {
                XMLParserAttribute(ProductesBufBD , xDoc);
            }         
            XMLParserCategory(ProductesBufBD);
            List<Model.AttributeEntity> Attributes = new List<Model.AttributeEntity>();
            Attributes = Connection.context.GetTables("Attribute", new Model.AttributeEntity());
            ProFileSupplier ProFileSuppliers = Connection.context.GetTables("Supplier", new Model.ProFileSupplier()).FirstOrDefault(x => x.SupplierName == ProductesBufBD.SupplierName);
            XmlElement? xRoot = xDoc.DocumentElement;
                List<Product> ProductsList = Connection.context.GetSupplierIdProduct(ProFileSuppliers.Id);
                //xDoc.InnerXml = RemoveAllNamespaces(xDoc.InnerXml);
                XmlNodeList ListXML = xDoc.GetElementsByTagName(ProductesBufBD.SupplierConfigs.Input);
            foreach (XmlNode item in ListXML)
            {


                Product ProductXML = XMLParserOneProduct(ProFileSuppliers, item, Attributes);
                Product ProductUpdate = Connection.context.SearchProduct(ProductXML, ProductsList);
                if (ProductUpdate != null)
                {
                    if (ProductUpdate.Equals(ProductXML))
                    {
                        // Ничегоне делать           
                        InfoProduct.Add($"{ProFileSuppliers.SupplierName} - {ProductXML.VendorId}-Без изменений");
                        logger.LogInformation($"{ProFileSuppliers.SupplierName} - {ProductXML.VendorId}-Без изменений");
                    }
                    else
                    {
                        // Update
                        ProductXML.Id = ProductUpdate.Id;
                        ProductXML.CreatedAt = ProductUpdate.CreatedAt;
                        ProductXML.UpdatedAt = DateTime.Now;
                        ProductesUpdate.Add(ProductXML);
                        InfoProduct.Add($" {ProFileSuppliers.SupplierName} - {ProductXML.VendorId}-Изменён");
                        logger.LogInformation($" {ProFileSuppliers.SupplierName} - {ProductXML.VendorId}-Изменён");
                    }
                }
                else
                {

                    // Insert
                    foreach (var itemss in ProductXML.Attributes)
                    {
                        bool Check = true;
                        foreach (var itemm in AttributeEntityUpdate)
                        {
                            if (itemss.AttributeEntity == itemm.Id)
                            {
                                Check = false;
                                itemm.Rating++;
                                break;
                            }
                        }

                        if (Check)
                        {
                            AttributeEntity attributeBD = Attributes.FirstOrDefault(x => x.Id == itemss.AttributeEntity);
                            attributeBD.Rating++;
                            AttributeEntityUpdate.Add(attributeBD);
                        }

                    }
                    InfoProduct.Add($"{ProFileSuppliers.SupplierName} - {ProductXML.VendorId} - Добавлен\n");

                    logger.LogInformation($"{ProFileSuppliers.SupplierName} - {ProductXML.VendorId} - Добавлен\n");

                    ProductesAdd.Add(ProductXML);
                }

                ProductesAll.Add(ProductXML);

                 Connection.context.InsertRecord("Product", ProductXML);
                }
            
            Connection.context.UpdateAttribute("Attribute", AttributeEntityUpdate);
            Connection.context.UpdateProductTest(ProductesUpdate);
            //Connection.context.InsertManyRecord("Product", ProductesAdd);
            //InfoProduct.Add($"Продуктов обновлено: {ProductesUpdate.Count}\n");
            //logger.LogInformation($"Продуктов обновлено: {ProductesUpdate.Count}\n");
            //logEntity.Data = InfoProduct;
            //logEntity.TotalAdded = ProductesAdd.Count;
            //logEntity.TotalEdit = ProductesUpdate.Count;

            //InfoProduct.Add($"---Статус продукта---");
            //ProductsList = Connection.context.GetTables("Product", new Model.Product());
            //ProductesUpdate = new List<Product>();
            //foreach (var itemlist in ProductsList.Where(x => x.SupplierId == ProductesBufBD.Id))
            //{
            //    bool CheckAtt = true;
            //    foreach (var itemall in ProductesAll)
            //    {
            //        if (itemlist.VendorId == itemall.VendorId)
            //        {
            //            CheckAtt = false;
            //            break;
            //        }
            //    }
            //    if (CheckAtt)
            //    {
            //        if (!itemlist.IsDeleted)
            //        {
            //            itemlist.IsDeleted = true;
            //            ProductesUpdate.Add(itemlist);
            //            //Console.WriteLine($"{itemlist.VendorId} - Заморозка"); // УТ000001744653656344746
            //            InfoProduct.Add($"{DateTime.Now} - {itemlist.VendorId}-Заморозка");
            //            logger.LogInformation($"{DateTime.Now} - {itemlist.VendorId}-Заморозка");
            //        }
            //    }
            //    else
            //    {
            //        if (itemlist.IsDeleted)
            //        {
            //            itemlist.IsDeleted = false;
            //            ProductesUpdate.Add(itemlist);
            //            //Console.WriteLine($"{itemlist.VendorId} - Разморозка");
            //            InfoProduct.Add($"{DateTime.Now} - {itemlist.VendorId}-Разморозка");
            //            logger.LogInformation($"{DateTime.Now} - {itemlist.VendorId}-Разморозка");
            //        }
            //    }

            //}
            //Connection.context.UpdateProductTest( ProductesUpdate);


            InfoProduct.Add($"Продуктов добавлено: {ProductesAdd.Count}\n");
            logger.LogInformation($"Продуктов добавлено: {ProductesAdd.Count}\n");

            InfoProduct.Add($"Продуктов заморожено: {ProductesUpdate.Count}\n");
            logger.LogInformation($"Продуктов заморожено: {ProductesUpdate.Count}\n");
            logEntity.TotalAdded = ProductesAdd.Count;
            logEntity.Status = "Импорт завершился";
            logEntity.Result = "Выполненно";
            logEntity.TotalFreezed = ProductesUpdate.Count;
            
            Connection.context.UpdateLog("Log", logEntity);
            DownloadImage();
            if (ProductesAll.Count == (ProductesBufBD.SourceSettings.CountPage == null ? 0 : Convert.ToInt32(ProductesBufBD.SourceSettings.CountPage)))
            {
                return true;
            }
            else
            {
                return false;
            }
            }
            catch
            {
                logEntity.Status = "Ошибка импорта";
                logEntity.Result = "Ошибка";
                Connection.context.UpdateLog("Log", logEntity);
                return false;
            }

        }
        public void JsonParserDocument(ProFileSupplier ProductesBufBD, string json)
        {
            List<Document> Documents = Connection.context.GetDocumentSearch(ProductesBufBD.Id);
            List<Document> AddDocuments = new List<Document>();
            JArray start = new JArray();
            if (ProductesBufBD.SupplierConfigs.Input != null)
            {
                var jobj = JObject.Parse(json);
                start = (JArray)jobj[ProductesBufBD.SupplierConfigs.Input];
            }
            else
            {

                start = JArray.Parse(json);
            }
            foreach (var item in start)
            {
                var Docs = (ProductesBufBD.SupplierConfigs.DocumentsStart == null ? null : (JArray)item[ProductesBufBD.SupplierConfigs.DocumentsStart]);
                if (Docs != null)
                {
                    foreach (var Doc in Docs)
                    {
                        Document document = new Document();
                        document.Id = ObjectId.GenerateNewId().ToString();
                        document.SupplierId = ProductesBufBD.Id;
                        document.Keywords = new List<string>();
                        document.IsDeleted = false;
                        var Jtoken = Doc.SelectToken("nakjhgfdsame");/////////
                        if (ProductesBufBD.SupplierConfigs.Documents.Type != null)
                        {
                            Jtoken = Doc.SelectToken(ProductesBufBD.SupplierConfigs.Documents.Type);
                            if (Jtoken != null)
                            {
                                document.Type = Jtoken.ToString();
                                if (Documents.Where(x => x.Type == document.Type).Count() >= 1)
                                {
                                    continue;
                                }
                            }
                        }
                        if (ProductesBufBD.SupplierConfigs.Documents.Url != null)
                        {
                            Jtoken = Doc.SelectToken(ProductesBufBD.SupplierConfigs.Documents.Url);
                            if (Jtoken != null)
                            {
                                document.Url = Jtoken.ToString();
                            }
                        }
                        if (ProductesBufBD.SupplierConfigs.Documents.CertId != null)
                        {
                            Jtoken = Doc.SelectToken(ProductesBufBD.SupplierConfigs.Documents.CertId);
                            if (Jtoken != null)
                            {
                                document.CertId = Jtoken.ToString();
                            }
                        }
                        if (ProductesBufBD.SupplierConfigs.Documents.CertNumber != null)
                        {
                            Jtoken = Doc.SelectToken(ProductesBufBD.SupplierConfigs.Documents.CertNumber);
                            if (Jtoken != null)
                            {
                                document.CertNumber = Jtoken.ToString();
                            }
                        }
                        if (ProductesBufBD.SupplierConfigs.Documents.CertDescr != null)
                        {
                            Jtoken = Doc.SelectToken(ProductesBufBD.SupplierConfigs.Documents.CertDescr);
                            if (Jtoken != null)
                            {
                                document.CertDescr = Jtoken.ToString();
                            }
                        }
                        if (ProductesBufBD.SupplierConfigs.Documents.File != null)
                        {
                            Jtoken = Doc.SelectToken(ProductesBufBD.SupplierConfigs.Documents.File);
                            if (Jtoken != null)
                            {
                                document.File = Jtoken.ToString();
                            }
                        }
                        if (ProductesBufBD.SupplierConfigs.Documents.CertOrganizNumber != null)
                        {
                            Jtoken = Doc.SelectToken(ProductesBufBD.SupplierConfigs.Documents.CertOrganizNumber);
                            if (Jtoken != null)
                            {
                                document.CertOrganizNumber = Jtoken.ToString();
                            }
                        }
                        if (ProductesBufBD.SupplierConfigs.Documents.CertOrganizDescr != null)
                        {
                            Jtoken = Doc.SelectToken(ProductesBufBD.SupplierConfigs.Documents.CertOrganizDescr);
                            if (Jtoken != null)
                            {
                                document.CertOrganizDescr = Jtoken.ToString();
                            }
                        }
                        if (ProductesBufBD.SupplierConfigs.Documents.BlankNumber != null)
                        {
                            Jtoken = Doc.SelectToken(ProductesBufBD.SupplierConfigs.Documents.BlankNumber);
                            if (Jtoken != null)
                            {
                                document.BlankNumber = Jtoken.ToString();
                            }
                        }
                        if (ProductesBufBD.SupplierConfigs.Documents.StartDate != null)
                        {
                            Jtoken = Doc.SelectToken(ProductesBufBD.SupplierConfigs.Documents.StartDate);
                            if (Jtoken != null)
                            {
                                document.StartDate = Convert.ToDateTime(Jtoken.ToString());
                            }
                        }
                        if (ProductesBufBD.SupplierConfigs.Documents.EndDate != null)
                        {
                            Jtoken = Doc.SelectToken(ProductesBufBD.SupplierConfigs.Documents.EndDate);
                            if (Jtoken != null)
                            {
                                document.EndDate = Convert.ToDateTime(Jtoken.ToString());
                            }
                        }

                        //TODO: Keywords как заполнять (List) 
                        if (ProductesBufBD.SupplierConfigs.Documents.Keywords != null)
                        {
                            Jtoken = Doc.SelectToken(ProductesBufBD.SupplierConfigs.Documents.Keywords);
                            if (Jtoken != null)
                            {
                                // document.Keywords = Jtoken.ToString();
                            }
                        }
                        Documents.Add(document);
                        AddDocuments.Add(document);
                    }
                }
            }
            Connection.context.InsertManyRecord("Document", AddDocuments);
        }
        public bool ParseJson(ProFileSupplier ProductesBufBD , ILogger<ProductRepository> logger, string json)
        {
            
            Log logEntity = new Log();
            logEntity.id = ObjectId.GenerateNewId().ToString();
            logEntity.SupplierId = ProductesBufBD.Id;
            logEntity.Date = DateTime.Now;
            logEntity.Status = "Данные обрабатываются...";
            logEntity.Result = "В процессе";
            Connection.context.InsertRecord("Log", logEntity);



            List<string> InfoProduct = new List<string>();
            InfoProduct.Add($"\n---------Информация о поставщике---------\n");
            InfoProduct.Add($"Время импорта: {DateTime.Now}\n");
            InfoProduct.Add($"ID - поставщика: {ProductesBufBD.Id}\n");
            InfoProduct.Add($"Поставщик: {ProductesBufBD.SupplierName}\n");
            InfoProduct.Add($"Путь до файла на сервере: {ProductesBufBD.Connection} \n\n");
            InfoProduct.Add($"---------РЕЗУЛЬТАТ ИМПОРТА---------\n");

            try
            {
                foreach (var AttributeKeys in (ProductesBufBD.SupplierConfigs.productAttributeKeys == null ? new List<ProductAttributeKey>() : ProductesBufBD.SupplierConfigs.productAttributeKeys))
                {
                    json = json .Replace(AttributeKeys.KeySupplier, AttributeKeys.AttributeBDName);
                }

                List<Product> ProductesAdd = new List<Product>();
                List<Product> ProductesUpdate = new List<Product>();
                List<Product> ProductesAll = new List<Product>();
                List<AttributeEntity> AttributeEntityUpdate = new List<AttributeEntity>();
                List<AttributeEntity> AttributeUpdate = new List<AttributeEntity>();

                List<AttributeEntity> Attributes = Connection.context.GetTables("Attribute", new Model.AttributeEntity()); //InsertManyRecord
                ProFileSupplier ProFileSuppliers = Connection.context.GetTables("Supplier", new Model.ProFileSupplier()).FirstOrDefault(x => x.SupplierName == ProductesBufBD.SupplierName);
                List<Product> ProductsList = Connection.context.GetSupplierIdProduct(ProFileSuppliers.Id);
              
                if (json == null)
                {
                    using (TextFieldParser parser = new TextFieldParser(ProductesBufBD.Connection))
                    {
                        json = parser.ReadToEnd();
                    }

                }
                foreach (var AttributeKeys in (ProductesBufBD.SupplierConfigs.productAttributeKeys == null ? new List<ProductAttributeKey>() : ProductesBufBD.SupplierConfigs.productAttributeKeys))
                {
                    json = json.ToString().Replace(AttributeKeys.KeySupplier, AttributeKeys.AttributeBDName);
                }
                logger.LogInformation($"{ProductesBufBD.SupplierName} - Начало парсинга Атрибутов");
                if (ProductesBufBD.SupplierConfigs.AttributesURL == null)
                {
                    JsonParserAttribute(ProductesBufBD, json, Attributes);
                }
                logger.LogInformation($"{ProductesBufBD.SupplierName} - Конец парсинга Атрибутов");



                logger.LogInformation($"{ProductesBufBD.SupplierName} - Начало парсинга Документов");
                if (ProductesBufBD.SupplierConfigs.DocumentsURL == null)
                {
                    JsonParserDocument(ProductesBufBD, json);
                }
                logger.LogInformation($"{ProductesBufBD.SupplierName} - Конец парсинга Документов");

                JArray start = new JArray();
                if (ProductesBufBD.SupplierConfigs.Input != null)
                {
                    var jobj = JObject.Parse(json);
                    var tkn = jobj.SelectToken(ProductesBufBD.SupplierConfigs.Input);
                    start = (JArray)jobj[ProductesBufBD.SupplierConfigs.Input];
                }
                else
                {

                    start = JArray.Parse(json);
                }
                foreach (var ProductAllJson in start)
                {
                    Product ProductXML = JsonParserOneProduct(ProFileSuppliers, ProductAllJson, Attributes);
                    Product ProductUpdate = Connection.context.SearchProduct(ProductXML, ProductsList);
                    if (ProductXML.Title != null)
                    {


                        if (ProductUpdate != null)
                        {
                            if (ProductUpdate.Equals(ProductXML))
                            {
                                // Ничегоне делать
                                //Console.WriteLine($"{ProductXML.VendorId} - Без изменений");
                                InfoProduct.Add($"{ProFileSuppliers.SupplierName} - {ProductXML.VendorId} - Без изменений\n");
                                logger.LogInformation($"{ProFileSuppliers.SupplierName} - {ProductXML.VendorId}-Без изменений");
                            }
                            else
                            {
                                // Update
                                ProductXML.Id = ProductUpdate.Id;
                                ProductXML.CreatedAt = ProductUpdate.CreatedAt;
                                ProductXML.UpdatedAt = DateTime.Now;
                                ProductesUpdate.Add(ProductXML);
                                InfoProduct.Add($"{ProFileSuppliers.SupplierName} - {ProductXML.VendorId} - Изменён\n");
                                logger.LogInformation($" {ProFileSuppliers.SupplierName} - {ProductXML.VendorId}-Изменён");
                            }
                        }
                        else
                        {

                            // Insert
                            //Console.WriteLine($"{ProductXML.VendorId} - Продукта добавлен");
                            ProductesAdd.Add(ProductXML);
                            InfoProduct.Add($"{ProFileSuppliers.SupplierName} - {ProductXML.VendorId} - Добавлен");
                            foreach (var itemss in ProductXML.Attributes)
                            {
                                bool Check = true;
                                foreach (var item in AttributeEntityUpdate)
                                {
                                    if (itemss.AttributeEntity == item.Id)
                                    {
                                        Check = false;
                                        item.Rating++;
                                        break;
                                    }
                                }

                                if (Check)
                                {
                                    AttributeEntity attributeBD = Attributes.FirstOrDefault(x => x.Id == itemss.AttributeEntity);
                                    attributeBD.Rating++;
                                    AttributeEntityUpdate.Add(attributeBD);

                                }

                            }
                            logger.LogInformation($"{ProFileSuppliers.SupplierName}  - {ProductXML.VendorId} - Добавлен");
                        }
                    }
                    ProductesAll.Add(ProductXML);

                }

                Connection.context.InsertManyRecord("Product", ProductesAdd);
                Connection.context.UpdateAttribute("Attribute", AttributeEntityUpdate);
                Connection.context.UpdateProductTest(ProductesUpdate);
                //logEntity.Data = InfoProduct;
                //logEntity.TotalAdded = ProductesAdd.Count;
                //logEntity.TotalEdit = ProductesUpdate.Count;
                //ProductsList = Connection.context.GetTables("Product", new Model.Product());
                //ProductesUpdate = new List<Product>();
                //foreach (var itemlist in ProductsList.Where(x => x.SupplierId == ProductesBufBD.Id))
                //{

                //    bool CheckAtt = true;
                //    foreach (var itemall in ProductesAll)
                //    {
                //        if (itemlist.VendorId == itemall.VendorId)
                //        {
                //            CheckAtt = false;
                //            break;
                //        }
                //    }
                //    if (CheckAtt)
                //    {
                //        if (!itemlist.IsDeleted)
                //        {
                //            itemlist.IsDeleted = true;
                //            ProductesUpdate.Add(itemlist);
                //            //Console.WriteLine($"{itemlist.VendorId} - Заморозка");// УТ000001744653656344746
                //            InfoProduct.Add($"{DateTime.Now} - {itemlist.VendorId}-Заморозка");
                //        }
                //    }
                //    else
                //    {
                //        if (itemlist.IsDeleted)
                //        {
                //            itemlist.IsDeleted = false;
                //            ProductesUpdate.Add(itemlist);
                //            //Console.WriteLine($"{itemlist.VendorId} - Разморозка");
                //            InfoProduct.Add($"{DateTime.Now} - {itemlist.VendorId}-Разморозка");

                //        }
                //    }

                //}
                //Connection.context.UpdateProductTest(ProductesUpdate);

                logEntity.TotalAdded = ProductesAdd.Count;
                logEntity.Status = "Импорт завершился";
                logEntity.Result = "Выполненно";
                logEntity.TotalFreezed = ProductesUpdate.Count;
                Connection.context.UpdateLog("Log", logEntity);
                //Thread downloadThread = new Thread(() => DownloadImage());
                //downloadThread.Start();
                DownloadImage();
                InfoProduct.Add($"---------РЕЗУЛЬТАТ ИМПОРТА---------\n");
                InfoProduct.Add($"Продуктов добавлено: {ProductesAll.Count}\n");

                InfoProduct.Add($"Продуктов обновлено: {ProductesUpdate.Count}\n");
                if (ProductesAll.Count == (ProductesBufBD.SourceSettings.CountPage == null ? 0 : Convert.ToInt32(ProductesBufBD.SourceSettings.CountPage)))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                logEntity.Status = "Ошибка импорта";
                logEntity.Result = "Ошибка";
                Connection.context.UpdateLog("Log", logEntity);
                return false;
            }
        }
        public void JsonParserAttribute(ProFileSupplier ProductesBufBD, string json , List<AttributeEntity> Attributes)
        {
            List<Product> ProductesBD = new List<Product>();
            List<Model.AttributeEntity> AttributesAdd = new List<Model.AttributeEntity>();
            using (StreamReader reader = new StreamReader(ProductesBufBD.Connection))
            {
                JArray start = new JArray();
                //var json = reader.ReadToEnd();
                if (ProductesBufBD.SupplierConfigs.Input != null)
                {
                    var jobj = JObject.Parse(json);
                    var tkn = jobj.SelectToken(ProductesBufBD.SupplierConfigs.Input);
                    start = (JArray)jobj[ProductesBufBD.SupplierConfigs.Input];
                }
                else
                {

                    start = JArray.Parse(json);
                }
                foreach (var aItem in start)
                {
                    var Param = (ProductesBufBD.SupplierConfigs.AttributesStart == null ? null : (JArray)aItem[ProductesBufBD.SupplierConfigs.AttributesStart]);
                    if (Param != null)
                    {
                        foreach (var aItems in Param)
                        {
                            AttributeEntity attributeBD = new AttributeEntity();
                            attributeBD.Rating = 0;
                            attributeBD.SupplierId = ProductesBufBD.Id;
                            attributeBD.Id = ObjectId.GenerateNewId().ToString();
                            attributeBD.AllValue = new List<string>();
                            bool Check = true;
                            bool CheckList = true;



                            var BufXml2 = aItems.SelectToken(ProductesBufBD.SupplierConfigs.AttributesParam.NameAttribute);
                            if (BufXml2 != null)
                            {
                                attributeBD.NameAttribute = BufXml2.ToString();
                            }


                            foreach (var AttributeKeys in (ProductesBufBD.SupplierConfigs.productAttributeKeys == null ? new List<ProductAttributeKey>() : ProductesBufBD.SupplierConfigs.productAttributeKeys))
                            {
                                if (attributeBD.NameAttribute == AttributeKeys.KeySupplier)
                                {
                                    CheckList = false;
                                    break;
                                }
                            }
                            if (CheckList)
                            {
                                foreach (AttributeEntity item in Attributes)
                                {
                                    if (item.NameAttribute == attributeBD.NameAttribute)
                                    {
                                        Check = false;
                                        break;
                                    }
                                    else
                                    {
                                        foreach (var PossibleAttributeNameOne in (item.PossibleAttributeName == null ? new List<string>() : item.PossibleAttributeName))
                                        {
                                            if (PossibleAttributeNameOne == attributeBD.NameAttribute)
                                            {
                                                Check = false;
                                                goto GotThis;
                                            }
                                        }
                                    }
                                }
                            GotThis:

                                if (Check)
                                {
                                    //Connection.context.InsertRecord("Attribute", attributeBD);
                                    AttributesAdd.Add(attributeBD);
                                    Attributes.Add(attributeBD);
                                    //Attributes.Add(attributeBD);
                                }
                            }
                        }
                    }
                    
                }

                Connection.context.InsertManyRecord("Attribute", AttributesAdd);


            }
        }
        public Product JsonParserOneProduct(ProFileSupplier ProductesBufBD, JToken aItem, List<Model.AttributeEntity> Attributes)
        {
            var client = new HttpClient();
            List<Document> Documents = Connection.context.GetDocumentSearch(ProductesBufBD.Id);
            client.DefaultRequestHeaders.Add("User-Agent", "EnergoMix");
            Product Buf = new Product();
            Buf.SupplierId = ProductesBufBD.Id.ToString();
            Buf.Id = ObjectId.GenerateNewId().ToString();
            Buf.Features = new List<string>();
            Buf.Attributes = new List<Model.AttributeProduct>();
            Buf.Images = new List<string>();
            Buf.Documents = new List<string>();
            var BufXml2 = aItem.SelectToken("nakjhgfdsame");/////////

            if (ProductesBufBD.SupplierConfigs.Title != null)
            {
                var BufXml22 = aItem.SelectToken(ProductesBufBD.SupplierConfigs.Title);
                if (BufXml22 != null)
                {
                    Buf.Title = BufXml22.ToString();
                }
            }
            if (ProductesBufBD.SupplierConfigs.Vendor != null)
            {
                BufXml2 = aItem.SelectToken(ProductesBufBD.SupplierConfigs.Vendor);
                if (BufXml2 != null)
                {
                    Buf.Vendor = BufXml2.ToString();
                }              
            }
            if (ProductesBufBD.SupplierConfigs.VendorId != null)
            {
                BufXml2 = aItem.SelectToken(ProductesBufBD.SupplierConfigs.VendorId);
                if (BufXml2 != null)
                {
                    Buf.VendorId = BufXml2.ToString();
                }
            }
            if (ProductesBufBD.SupplierConfigs.TitleLong != null)
            {
                BufXml2 = aItem.SelectToken(ProductesBufBD.SupplierConfigs.TitleLong);
                if (BufXml2 != null)
                {
                    Buf.TitleLong = BufXml2.ToString();
                }
             
            }
            if (ProductesBufBD.SupplierConfigs.Description != null)
            {
                BufXml2 = aItem.SelectToken(ProductesBufBD.SupplierConfigs.Description);
                if (BufXml2 != null)
                {
                    Buf.Description = BufXml2.ToString();
                }           
            }
            if (ProductesBufBD.SupplierConfigs.Image360 != null)
            {
                BufXml2 = aItem.SelectToken(ProductesBufBD.SupplierConfigs.Image360);
                if (BufXml2 != null)
                {
                    Buf.Image360 = BufXml2.ToString();
                }             
            }

            if (ProductesBufBD.SupplierConfigs.Images != null)
            {
                var Param = aItem.SelectTokens(ProductesBufBD.SupplierConfigs.Images); ///
                if (Param != null)
                {
                    foreach (var item in Param)
                    {
                        //Buf.Images.Add(ParserImage(Param[i].InnerText));
                        Buf.Images.Add(ParserImage(item.ToString()));
                    }
                }
            }
            if (ProductesBufBD.SupplierConfigs.ImageConfig !=null)
            {
                if (ProductesBufBD.SupplierConfigs.ImageConfig.ImageUrl != null)
                {
                    var supplierHeader = JsonConvert.DeserializeObject<List<HeaderContent>>(ProductesBufBD.SourceSettings.Header);///////////
                    foreach (var item in supplierHeader)
                    {
                        client.DefaultRequestHeaders.Add(item.HeaderName, item.HeaderValue);
                    }
                    string response = client.GetStringAsync(ProductesBufBD.SupplierConfigs.ImageConfig.ImageUrl).Result;
                    Buf.Images.AddRange(GetImagesHttpClientJson(response, ProductesBufBD));
                }
                if (ProductesBufBD.SupplierConfigs.Features != null)
                {

                    var tyrtr = aItem.SelectTokens(ProductesBufBD.SupplierConfigs.Features);
                    if (tyrtr != null)
                    {
                        foreach (var item in tyrtr)
                        {
                            Buf.Features.Add(item.ToString());
                        }
                    }

                }
            }
            if (ProductesBufBD.SupplierConfigs.DocumentsStart != null || ProductesBufBD.SupplierConfigs.DocumentsURL != null)
            {
                if (ProductesBufBD.SupplierConfigs.DocumentsURL != null)
                {
                    // TODO: Получение документов из URL
                    string Url = string.Format(ProductesBufBD.SupplierConfigs.DocumentsURL, Buf.VendorId);
                    string response = client.GetStringAsync(Url).Result;
                    Buf.Documents.AddRange(GetDocumentHttpClientJson(response, ProductesBufBD));

                }
                else
                {
                    var DocumentsStart = aItem.SelectToken(ProductesBufBD.SupplierConfigs.DocumentsStart);
                    if (DocumentsStart != null)
                    {
                        foreach (var Document in DocumentsStart)
                        {
                            string Type = "";
                            if (ProductesBufBD.SupplierConfigs.Documents.Type != null)
                            {
                                BufXml2 = Document.SelectToken(ProductesBufBD.SupplierConfigs.Documents.Type);
                                if (BufXml2 != null)
                                {
                                    Type = BufXml2.ToString();
                                }
                            }
                            foreach (var item in Documents)
                            {
                                if (Type == item.Type)
                                {
                                    Buf.Documents.Add(item.Id);
                                    break;
                                }
                            }
                        }
                    }
                     
                }
            }



                if (ProductesBufBD.SupplierConfigs.AttributesStart != null || ProductesBufBD.SupplierConfigs.AttributesURL != null)
            {
                if (ProductesBufBD.SupplierConfigs.AttributesURL != null)
                {
                    if (ProductesBufBD.SourceSettings.Header != null)
                    {
                        var supplierHeader = JsonConvert.DeserializeObject<List<HeaderContent>>(ProductesBufBD.SourceSettings.Header);
                        foreach (var item in supplierHeader)
                        {
                            client.DefaultRequestHeaders.Add(item.HeaderName, item.HeaderValue);
                        }

                    }
                    if (ProductesBufBD.SourceSettings.MethodType == "GET")
                    {                  
                        string Url = string.Format(ProductesBufBD.SupplierConfigs.AttributesURL, Buf.VendorId);
                        string response = client.GetStringAsync(Url).Result;
                        Buf.Attributes.AddRange(JSONParserAttributeHttpClient(ProductesBufBD, response, Attributes));
                    }
                    else
                    {
                        string Url = string.Format(ProductesBufBD.SupplierConfigs.AttributesURL, Buf.VendorId);
                        var bodyData = new StringContent(ProductesBufBD.SourceSettings.Body, Encoding.UTF8, "application/json");
                        var response = client.PostAsync($"{Url}", bodyData).Result;
                        string json = response.Content.ReadAsStringAsync().Result;
                        Buf.Attributes.AddRange(JSONParserAttributeHttpClient(ProductesBufBD, json, Attributes));
                    }
                }
                else
                {



                    var AttributesStart = aItem.SelectToken(ProductesBufBD.SupplierConfigs.AttributesStart);
                    if (AttributesStart != null)
                    {


                        foreach (var item in AttributesStart)
                        {
                            GoodWareMixApi.Model.AttributeProduct NewAttribute = new GoodWareMixApi.Model.AttributeProduct();
                            



                            if (ProductesBufBD.SupplierConfigs.AttributesParam.Value != null)
                            {
                                BufXml2 = item.SelectToken(ProductesBufBD.SupplierConfigs.AttributesParam.Value);
                                if (BufXml2 != null)
                                {
                                    NewAttribute.Value = BufXml2.ToString();
                                }
                                else
                                {
                                    if (item.ToString() == ProductesBufBD.SupplierConfigs.AttributesParam.Value ? true : false)
                                    {
                                        NewAttribute.Value = item.ToString();
                                    }
                                }
                            }





                            bool checkAttribut = true;
                            foreach (Model.AttributeEntity Attribut in Attributes)
                            {
                                var a = item.SelectToken(ProductesBufBD.SupplierConfigs.AttributesParam.NameAttribute);
                                if (Attribut.NameAttribute == (a == null ? null : a.ToString()))
                                {
                                    NewAttribute.AttributeEntity = Attribut.Id;
                                    checkAttribut = false;
                                    bool AttributValue = true;
                                    foreach (var Value in Attribut.AllValue)
                                    {
                                        if (Value == NewAttribute.Value)
                                        {
                                            AttributValue = false;
                                        }
                                    }
                                    if (AttributValue)
                                    {
                                        // Доавить
                                        Attribut.AllValue.Add(NewAttribute.Value);
                                    }
                                    break;
                                }
                            }



                            if (ProductesBufBD.SupplierConfigs.AttributesParam.Unit != null)
                            {
                                BufXml2 = item.SelectToken(ProductesBufBD.SupplierConfigs.AttributesParam.Unit);
                                if (BufXml2 != null)
                                {
                                    NewAttribute.Unit = BufXml2.ToString();
                                }
                            }
                            if (ProductesBufBD.SupplierConfigs.AttributesParam.Type != null)
                            {
                                BufXml2 = item.SelectToken(ProductesBufBD.SupplierConfigs.AttributesParam.Type);
                                if (BufXml2 != null)
                                {
                                    NewAttribute.Type = BufXml2.ToString();
                                }
                            }

                            if (!String.IsNullOrEmpty(NewAttribute.Value))
                            {
                                Buf.Attributes.Add(NewAttribute);
                            }     
                        }
                    }
                }
            }
            //Documents
            //if (ProductesBufBD.SupplierConfigs.DocumentsStart != null || ProductesBufBD.SupplierConfigs.Documents.Url != null)
            //{
            //    if (ProductesBufBD.SupplierConfigs.Documents.Url != null)
            //    {

            //        var supplierHeader = JsonConvert.DeserializeObject<List<HeaderContent>>(ProductesBufBD.SupplierConfigs.Documents.Url);
            //        foreach (var item in supplierHeader)
            //        {
            //            client.DefaultRequestHeaders.Add(item.HeaderName, item.HeaderValue);
            //        }
            //        string Url = string.Format(ProductesBufBD.SupplierConfigs.AttributesURL, Buf.VendorId);
            //        string response = client.GetStringAsync(Url).Result;
            //        Buf.Attributes.AddRange(JSONParserAttributeHttpClient(ProductesBufBD, response, Attributes));
            //    }
            //    else
            //    {



            //        var AttributesStart = aItem.SelectToken(ProductesBufBD.SupplierConfigs.AttributesStart);
            //        foreach (var item in AttributesStart)
            //        {
            //            GoodWareMixApi.Model.AttributeProduct NewAttribute = new GoodWareMixApi.Model.AttributeProduct();
            //            bool checkAttribut = true;
            //            foreach (Model.AttributeEntity Attribut in Attributes)
            //            {
            //                var a = item.SelectToken(ProductesBufBD.SupplierConfigs.AttributesParam.NameAttribute);
            //                if (Attribut.NameAttribute == (a == null ? null : a.ToString()))
            //                {
            //                    NewAttribute.AttributeEntity = Attribut.Id;
            //                    checkAttribut = false;
            //                    bool AttributValue = true;
            //                    foreach (var Value in Attribut.AllValue)
            //                    {
            //                        if (Value == NewAttribute.Value)
            //                        {
            //                            AttributValue = false;
            //                        }
            //                    }
            //                    if (AttributValue)
            //                    {
            //                        // Доавить
            //                        Attribut.AllValue.Add(NewAttribute.Value);
            //                    }
            //                    break;
            //                }
            //            }


            //            if (ProductesBufBD.SupplierConfigs.AttributesParam.Value != null)
            //            {
            //                BufXml2 = item.SelectToken(ProductesBufBD.SupplierConfigs.AttributesParam.Value);
            //                if (BufXml2 != null)
            //                {
            //                    NewAttribute.Value = BufXml2.ToString();
            //                }
            //                else
            //                {
            //                    if (item.ToString() == ProductesBufBD.SupplierConfigs.AttributesParam.Value ? true : false)
            //                    {
            //                        NewAttribute.Value = item.ToString();
            //                    }
            //                }
            //            }
            //            if (ProductesBufBD.SupplierConfigs.AttributesParam.Unit != null)
            //            {
            //                BufXml2 = item.SelectToken(ProductesBufBD.SupplierConfigs.AttributesParam.Unit);
            //                if (BufXml2 != null)
            //                {
            //                    NewAttribute.Unit = BufXml2.ToString();
            //                }
            //            }
            //            if (ProductesBufBD.SupplierConfigs.AttributesParam.Type != null)
            //            {
            //                BufXml2 = item.SelectToken(ProductesBufBD.SupplierConfigs.AttributesParam.Type);
            //                if (BufXml2 != null)
            //                {
            //                    NewAttribute.Type = BufXml2.ToString();
            //                }
            //            }

            //            Buf.Attributes.Add(NewAttribute);
            //        }
            //    }
            //}
            // ???

            //Packages
            //if (ProductesBufBD.SupplierConfigs.PackagesStart != null)
            //{
            //    var PackagesStart = aItem.SelectTokens(ProductesBufBD.SupplierConfigs.PackagesStart);
            //    foreach (var item in PackagesStart)
            //    {
            //        Package package = new Package();


            //        if (ProductesBufBD.SupplierConfigs.Packages.Barcode != null)
            //        {
            //            BufXml2 = item.SelectToken(ProductesBufBD.SupplierConfigs.Packages.Barcode);
            //            if (BufXml2 != null)
            //            {
            //                package.Barcode = BufXml2.ToString();
            //            }
            //        }
            //        if (ProductesBufBD.SupplierConfigs.Packages.Type != null)
            //        {
            //            BufXml2 = item.SelectToken(ProductesBufBD.SupplierConfigs.Packages.Type);
            //            if (BufXml2 != null)
            //            {
            //                package.Weight = (float)Convert.ToDouble(BufXml2.ToString());
            //            }
            //        }
            //        if (ProductesBufBD.SupplierConfigs.Packages.Volume != null)
            //        {
            //            BufXml2 = item.SelectToken(ProductesBufBD.SupplierConfigs.Packages.Volume);
            //            if (BufXml2 != null)
            //            {
            //                package.Volume = (float)Convert.ToDouble(BufXml2.ToString());
            //            }
            //        }
            //        if (ProductesBufBD.SupplierConfigs.Packages.PackQty != null)
            //        {
            //            BufXml2 = item.SelectToken(ProductesBufBD.SupplierConfigs.Packages.PackQty);
            //            if (BufXml2 != null)
            //            {
            //                package.PackQty = Convert.ToInt32(BufXml2.ToString());
            //            }
            //        }
            //        if (ProductesBufBD.SupplierConfigs.Packages.Height != null)
            //        {
            //            BufXml2 = item.SelectToken(ProductesBufBD.SupplierConfigs.Packages.Height);
            //            if (BufXml2 != null)
            //            {
            //                package.Height = Convert.ToInt32(BufXml2.ToString());
            //            }
            //        }
            //        if (ProductesBufBD.SupplierConfigs.Packages.Width != null)
            //        {
            //            BufXml2 = item.SelectToken(ProductesBufBD.SupplierConfigs.Packages.Width);
            //            if (BufXml2 != null)
            //            {
            //                package.Width = Convert.ToInt32(BufXml2.ToString());
            //            }
            //        }
            //        if (ProductesBufBD.SupplierConfigs.Packages.Length != null)
            //        {
            //            BufXml2 = item.SelectToken(ProductesBufBD.SupplierConfigs.Packages.Length);
            //            if (BufXml2 != null)
            //            {
            //                package.Length = Convert.ToInt32(BufXml2.ToString());
            //            }
            //        }
            //        if (ProductesBufBD.SupplierConfigs.Packages.Depth != null)
            //        {
            //            BufXml2 = item.SelectToken(ProductesBufBD.SupplierConfigs.Packages.Depth);
            //            if (BufXml2 != null)
            //            {
            //                package.Depth = Convert.ToInt32(BufXml2.ToString());
            //            }
            //        }
            //        if (ProductesBufBD.SupplierConfigs.Packages.Weight != null)
            //        {
            //            BufXml2 = item.SelectToken(ProductesBufBD.SupplierConfigs.Packages.Weight);
            //            if (BufXml2 != null)
            //            {
            //                package.Weight = (float)Convert.ToDouble(BufXml2.ToString());
            //            }
            //        }


            //        if (Buf.Packages != null )
            //        {
            //            Buf.Packages = new List<Package>();
            //        }

            //        Buf.Packages.Add(package);
            //    }
            //}
            Buf.UpdatedAt = DateTime.Now;
            Buf.CreatedAt = DateTime.Now;
            Buf.IsDeleted = false;
            return Buf;
        }
        public List<string> JSONParserInternalCode(string connection, ILogger<ProductRepository> logger, string prefix)
        {
            List<string> loggerList = new List<string>();
            List<Product> ProductUpdate = new List<Product>();
            List<Product> ProductAll = Connection.context.GetTables("Product", new Product());
            using (StreamReader reader = new StreamReader(connection)) //UpdateProductInternalCode
            {
                JArray start = new JArray();
                var json = reader.ReadToEnd();
                start = JArray.Parse(json);
                foreach (var OnejToken in start)
                {
                    Product NewInternalCode = new Product();
                    NewInternalCode.VendorId =  OnejToken.SelectToken("codeAlpha").ToString(); // selectToken
                    //logger.LogInformation($"Токен найден: {NewInternalCode.VendorId}");
                    //loggerList.Add($"Токен найден: {NewInternalCode.VendorId}");
                    Product SearchProduct = Connection.context.SearchProductInternalCode(NewInternalCode.VendorId, ProductAll , prefix); // findProduct
                    //logger.LogInformation($"Продукт найден: {NewInternalCode.VendorId}");
                    //loggerList.Add($"Продукт найден: {NewInternalCode.VendorId}");
                    if (SearchProduct != null)
                    {
                        SearchProduct.InternalCode = OnejToken.SelectToken("code").ToString();//Internal code insert

                        
                        ProductUpdate.Add(SearchProduct);
                        logger.LogInformation($"Продукту присовен Internal Code: {SearchProduct.InternalCode}");
                        loggerList.Add($"Продукту присовен Internal Code: {SearchProduct.InternalCode}");
                    }
                }
                logger.LogInformation($"Всего: {ProductUpdate.Count}");
                loggerList.Add($"Всего: {ProductUpdate.Count}");
                Connection.context.UpdateProductInternalCode(ProductUpdate); // update 
            }
            return loggerList;
        }
        public async Task<List<string>> JSONParserInternalCodeScheduler(string connection, ILogger<ProductRepository> logger, string prefix, ProFileSupplier currentSupplier)
        {
            List<string> loggerList = new List<string>();
            List<Product> ProductUpdate = new List<Product>();
            List<Product> ProductAll = Connection.context.GetSupplierIdProduct(currentSupplier.Id);
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync($"{connection}");
            string json = await response.Content.ReadAsStringAsync();
            var start = JsonConvert.DeserializeObject<InternalCodeAPI>(json);
            foreach (var OnejToken in start.data)
            {                                                    
                Product SearchProduct = Connection.context.SearchProductInternalCode(OnejToken.codeAlpha, ProductAll, prefix);
                if (SearchProduct != null && SearchProduct.InternalCode == null)
                {
                    SearchProduct.InternalCode = OnejToken.code;//Internal code insert
                    ProductUpdate.Add(SearchProduct);
                    logger.LogInformation($"Продукту присовен Internal Code: {SearchProduct.InternalCode}");
                    loggerList.Add($"Продукту присовен Internal Code: {SearchProduct.InternalCode}");
                }
            }

            logger.LogInformation($"Всего изменённых InternalCode: {ProductUpdate.Count}");          
            loggerList.Add($"Всего изменённых InternalCode: {ProductUpdate.Count}");
            Connection.context.UpdateProductInternalCode(ProductUpdate); // update 
            return loggerList;
        }
        
        public string ParserImage(string url)
        {
            //string wwwPath = this.environment.WebRootPath;
            //string path = Path.Combine(wwwPath, "Images");
            //if (!Directory.Exists(path))
            //{
            //    Directory.CreateDirectory(path);
            //}
            //HttpClient httpClient = new HttpClient();
            //HttpResponseMessage response = httpClient.GetAsync($"{url}").Result;
            //var Arrbyte = response.Content.ReadAsByteArrayAsync().Result;
            //DirectoryInfo d = new DirectoryInfo(path); //Assuming Test is your Folder
            string[] Name = url.Split('/');
            //FileInfo[] Files = d.GetFiles(Name[Name.Count() - 1]); //Getting Text files
            //if (Files.Count() == 0)
            //{
            //    File.WriteAllBytes($"{path}/{Name[Name.Count() - 1]}", Arrbyte);
            //}
            Urls.Add(url);
            return Name[Name.Count() - 1];
        }

        public void DownloadImage()
        {
            string wwwPath = this.environment.WebRootPath;
            string path = Path.Combine(wwwPath, "Images");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            HttpClient httpClient = new HttpClient();
            foreach (var item in Urls)
            {
                
                HttpResponseMessage response = httpClient.GetAsync($"{item}").Result;
                var Arrbyte = response.Content.ReadAsByteArrayAsync().Result;
                DirectoryInfo d = new DirectoryInfo(path); //Assuming Test is your Folder
                string[] Name = item.Split('/');
                FileInfo[] Files = d.GetFiles(Name[Name.Count() - 1]); //Getting Text files
                if (Files.Count() == 0)
                {
                    File.WriteAllBytes($"{path}/{Name[Name.Count() - 1]}", Arrbyte);
                }
            }
        }
    }
}
