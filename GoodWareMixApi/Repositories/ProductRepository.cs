using GoodWareMixApi.Filter;
using GoodWareMixApi.Interfaces;
using GoodWareMixApi.Intrefaces;
using GoodWareMixApi.Model;
using GoodWareMixApi.Model.Settings;
using GoodWareMixApi.Service;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OpenPop.Mime;
using OpenPop.Pop3;
using System.Configuration;
using System.Text;

namespace GoodWareMixApi.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly IUriService uriService;

      
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment environment;
        ParserDocuments documentService = new ParserDocuments();
        private readonly ILogger<ProductRepository> _logger;
        private readonly ISupplierRepository _supplierRepository;   

        public ProductRepository(IUriService uriService,  ILogger<ProductRepository> logger, Microsoft.AspNetCore.Hosting.IHostingEnvironment environment, ISupplierRepository supplierRepository)
        {
            this.uriService = uriService;
            _logger = logger;

            this.environment = environment;
            documentService.environment = environment;
            this._supplierRepository = supplierRepository;

        }


        public async Task<PagedResponse<List<Product>>> Get([FromQuery] PaginationFilter filter, string route)
        {
            
            var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize, filter.Search, filter.Supplier, filter.OrderBy, filter.Attributes, Convert.ToBoolean(filter.InternalCodeCheckBool));
            var pagedData = await Connection.context.GetDataProduct("Product", validFilter);
            var pagedResponse = PaginationHelper.CreatePagedReponse<Product>(pagedData.Data, validFilter, Convert.ToInt32(pagedData.Count), uriService, route);
            return pagedResponse;
        }
        public async Task<Product> Get([FromQuery] string internalCode)
        {
            Product Product = Connection.context.GetProductEntity("Product", internalCode);
            if (Product == null)
            {
                return null;
            }
            else
            {
                return Product;
            }
        }
        public async Task<Product> GetProductById(string id)
        {
            Product Product = Connection.context.GetProductEntityById("Product", id);
            if (Product == null)
            {
                return null;
            }
            else
            {
                return Product;
            }
        }
        private string[] permittedExtensions = { ".csv", ".xlsx", ".xml", ".xls", ".json", ".yml" };
        public async Task<List<string>> Post(string supplierName, IFormFile file)
        {
            List<string> InfoProduct = new List<string>();
            string wwwPath = this.environment.WebRootPath;
            string contentPath = this.environment.ContentRootPath;
            string path = Path.Combine(wwwPath, "FilesSupplier");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
            {
                // The extension is invalid ... discontinue processing the file
            }
            else
            {
                //change
                var currentSupplier =  await Connection.context.GetProfileSupplier("Supplier", supplierName);
                if (currentSupplier != null)
                {
                    using (FileStream stream = new FileStream(Path.Combine(path, file.FileName), FileMode.Create))
                    {
                        file.OpenReadStream();
                        file.CopyTo(stream);
                        
                        currentSupplier.Connection = stream.Name;
                        await Connection.context.UpdateSupplier("Supplier", currentSupplier);
                        stream.Close();
                        if (ext == ".xlsx" || ext == ".xls")
                        {
                            documentService.XLSXParse(currentSupplier, _logger, null);
                            return InfoProduct;
                        }
                        else if (ext == ".xml" || ext == ".yml")
                        {
                            documentService.XMLParser(currentSupplier, _logger , null);
    

                        }
                        else if (ext == ".json")
                        {
                     
                            documentService.ParseJson(currentSupplier, _logger, null);

                        }
                        else
                        {
                            documentService.CSVParser(currentSupplier);
                        }

                    }

                }
                else
                {
                    return InfoProduct;
                }


            }
            string pathLog = Path.Combine(wwwPath, "Logs");
            if (!Directory.Exists(pathLog))
            {
                Directory.CreateDirectory(pathLog);
            }
            // запись в файл
            using (FileStream fstream = new FileStream(Path.Combine(pathLog, $"{supplierName}.txt"), FileMode.Append, FileAccess.Write))
            {
                string fullString = "";
                fullString = String.Join("\n", InfoProduct.ToArray());
                byte[] byteArray = Encoding.UTF8.GetBytes(fullString);
                fstream.Write(byteArray);
                fstream.Close();
            }

            return InfoProduct;
        }      
        public async Task<List<string>> RunTaskAsync(ProFileSupplier supplier, IFormFile file)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "EnergoMix");
            client.Timeout = TimeSpan.FromHours(12);
            if (supplier.SourceSettings.Header != null)
            {
                var supplierHeader = JsonConvert.DeserializeObject<List<HeaderContent>>(supplier.SourceSettings.Header);
                foreach (var item in supplierHeader)
                {
                    client.DefaultRequestHeaders.Add(item.HeaderName, item.HeaderValue);
                }
            }
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            List<string> InfoProduct = new List<string>();
            ProFileSupplier proFileSupplier = new ProFileSupplier();//////////////////////////////////////////////////todo
            string wwwPath = this.environment.WebRootPath;
            string contentPath = this.environment.ContentRootPath;
            string path = Path.Combine(wwwPath, "FilesSupplier");
            var currentSupplier = await Connection.context.GetProfileSupplier("Supplier", supplier.SupplierName);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            //else
            //{
            //change


        

            if (currentSupplier != null)
                {
                    proFileSupplier = currentSupplier;
                if (currentSupplier.Source == "API")
                {


                    if (currentSupplier.Type == "XLSX")
                    {

                        documentService.XLSXParse(currentSupplier, _logger, null);
                        return InfoProduct;
                    }
                    else if (currentSupplier.Type == "XML")
                    {
                        bool Check = true;
                        for (int i = 0; true == Check; i++)
                        {
                            string Url = string.Format(supplier.SourceSettings.Url, i);
                            if (currentSupplier.SourceSettings.FileEncoding != null)
                            {



                                var coding = Encoding.GetEncoding(currentSupplier.SourceSettings.FileEncoding);
                                byte[] response = client.GetByteArrayAsync(Url).Result;
                                byte[] utf8Bytes = Encoding.Convert(coding, Encoding.UTF8, response);
                                var json = Encoding.UTF8.GetString(utf8Bytes);
                                Check = documentService.XMLParser(currentSupplier, _logger, json);
                            }
                            else
                            {

                                var response = client.GetAsync($"{Url}").Result;
                                string json = response.Content.ReadAsStringAsync().Result;
                                Check = documentService.XMLParser(currentSupplier, _logger, json);
                            }

                            if (Url == supplier.SourceSettings.Url)
                            {
                                Check = false;
                            }
                        }
                    }
                    else if (currentSupplier.Type == "JSON")
                    {
                        client.DefaultRequestHeaders.Add("Accept", "application/json");
                        bool Check = true;
                        for (int page = Convert.ToInt32(supplier.SourceSettings.StartPage); true == Check; page++)
                        {
                            string Url = string.Format(supplier.SourceSettings.Url, page);
                            if (currentSupplier.SourceSettings.FileEncoding != null)
                            {



                                var coding = Encoding.GetEncoding(currentSupplier.SourceSettings.FileEncoding);
                                byte[] response = client.GetByteArrayAsync(Url).Result;
                                byte[] utf8Bytes = Encoding.Convert(coding, Encoding.UTF8, response);
                                var json = Encoding.UTF8.GetString(utf8Bytes);
                                Check = documentService.ParseJson(currentSupplier, _logger, json);
                            }
                            else
                            {
                                var response = client.GetAsync($"{Url}").Result;
                                string json = response.Content.ReadAsStringAsync().Result;
                                Check = documentService.ParseJson(currentSupplier, _logger, json);
                            }

                            if (Url == supplier.SourceSettings.Url)
                            {
                                Check = false;
                            }
                        }

                    }
                    else
                    {
                        documentService.CSVParser(currentSupplier);
                    }
                }
                else if(currentSupplier.Source == "MAIL")
                {
                    if (currentSupplier.Type == "XLSX")
                    {

                        var hostname = "mail.sbat.ru";
                        var port = 110;
                        var username = "goodwareimport@ekt.sbat.ru";
                        var password = "Rw4Ap7eWx";
                        var useSsl = false;
                        using (Pop3Client clientPop3 = new Pop3Client())
                        {
                            UTF8Encoding encoding = new UTF8Encoding();
                            clientPop3.Connect(hostname, port, useSsl);
                            clientPop3.Authenticate(username, password);
                            int messageCount = clientPop3.GetMessageCount();
                            List<Message> allMessages = new List<Message>(messageCount);
                            for (int i = messageCount; i > 0; i--)
                            {
                                Message objMessage = clientPop3.GetMessage(i);
                                List<MessagePart> attachment = objMessage.FindAllAttachments();
                                for (int h = 0; h < attachment.Count; h++)
                                {
                                    if (clientPop3.GetMessage(i).Headers.Subject == supplier.SupplierName)
                                    {
                                        if (attachment[h].FileName.Contains(".xls"))
                                        {
                                            if (!Directory.Exists(path))
                                            {
                                                Directory.CreateDirectory(path);
                                            }
                                            File.WriteAllBytes($@"{path}\{supplier.SupplierName}.xls", attachment[h].Body);

                                            supplier.Connection = Path.Combine(path, $"{supplier.SupplierName}.xls");
                                            var responceSupplier = await _supplierRepository.Put(supplier.SupplierName, supplier);

                                            using (FileStream stream = new FileStream(Path.Combine(path, $"{supplier.SupplierName}.xls"), FileMode.Open))
                                            {
                                                var file1 = new FormFile(stream, 0, stream.Length, $"{supplier.SupplierName}", $"{supplier.SupplierName}.xls")
                                                {
                                                    Headers = new HeaderDictionary(),
                                                    ContentType = "multipart/form-data",

                                                };
                                                supplier.Connection = stream.Name;
                                                var responce2 = await _supplierRepository.Put(supplier.SupplierName, supplier);
                                                stream.Close();

                                                documentService.XLSXParse(currentSupplier, _logger, null);
                                            }

                                        }
                                        clientPop3.DeleteMessage(i);
                                        break;
                                    }
                                }
                       
                            }

                        }

                
                    }
                }
            }
            else
            {
                return InfoProduct;
            }


            //}
            string pathLog = Path.Combine(wwwPath, "Logs");
            if (!Directory.Exists(pathLog))
            {
                Directory.CreateDirectory(pathLog);
            }
            // запись в файл
            using (FileStream fstream = new FileStream(Path.Combine(pathLog, $"{supplier.SupplierName}.txt"), FileMode.Append, FileAccess.Write))
            {
                string fullString = "";
                fullString = String.Join("\n", InfoProduct.ToArray());
                byte[] byteArray = Encoding.UTF8.GetBytes(fullString);
                fstream.Write(byteArray);
                fstream.Close();
            }

            documentService.JSONParserInternalCodeScheduler($"{Connection.InternalCodeAPI}rows?prefix={proFileSupplier.SourceSettings.Prefix}", _logger, proFileSupplier.SourceSettings.Prefix, currentSupplier);

            ////
            return InfoProduct;
        }
        public async Task<ProFileSupplier> RunTaskQUARTZAsync(ProFileSupplier supplier)
        {
            var hostname = "mail.sbat.ru";
            var port = 110;
            var username = "goodwareimport@ekt.sbat.ru";
            var password = "Rw4Ap7eWx";
            var useSsl = false;
            if (supplier.Source == "API")
            {
                if (supplier.Type == "JSON")
                {
                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Add("Accept", "application/json");


                    if (supplier.SourceSettings.Header != null)
                    {

                        var supplierHeader = JsonConvert.DeserializeObject<List<HeaderContent>>(supplier.SourceSettings.Header);
                        foreach (var item in supplierHeader)
                        {
                            client.DefaultRequestHeaders.Add(item.HeaderName, item.HeaderValue);
                        }
                    }
                    else
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", "string");
                    }

                    if (supplier.SourceSettings.MethodType == "POST")
                    {
                        if (supplier.SourceSettings.Body != null)
                        {
                            var bodyData = new StringContent(supplier.SourceSettings.Body, Encoding.UTF8, "application/json");
                            var response1 = client.PostAsync($"{supplier.SourceSettings.Url}", null).Result;
                            string json = response1.Content.ReadAsStringAsync().Result;
                            string wwwPath = this.environment.WebRootPath;
                            string contentPath = this.environment.ContentRootPath;
                            string path = Path.Combine(wwwPath, "FilesSupplier");
                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }
                            await System.IO.File.WriteAllTextAsync($@"{path}\{supplier.SupplierName}.json", json);

                            supplier.Connection = Path.Combine(path, $"{supplier.SupplierName}.json");
                            var responceSupplier = await _supplierRepository.Put(supplier.SupplierName, supplier);

                            using (FileStream stream = new FileStream(Path.Combine(path, $"{supplier.SupplierName}.json"), FileMode.Open))
                            {
                                var file = new FormFile(stream, 0, stream.Length, $"{supplier.SupplierName}", $"{supplier.SupplierName}.json")
                                {
                                    Headers = new HeaderDictionary(),
                                    ContentType = "multipart/form-data",

                                };
                                supplier.Connection = stream.Name;
                                var responce2 = await _supplierRepository.Put(supplier.SupplierName, supplier);
                                stream.Close();
                                stream.Close();
                                Thread myThread = new Thread(() => RunTaskAsync(supplier, null));
                                myThread.Start();
                                return supplier;//var result = await _productRepository.RunTaskAsync(supplier.SupplierName, file);


                            }
                        }


                    }
                    if (supplier.SourceSettings.MethodType == "GET")
                    {


                        var response1 = client.GetAsync($"{supplier.SourceSettings.Url}").Result;
                        string json = response1.Content.ReadAsStringAsync().Result;
                        string wwwPath = this.environment.WebRootPath;
                        string contentPath = this.environment.ContentRootPath;
                        string path = Path.Combine(wwwPath, "FilesSupplier");
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        System.IO.File.WriteAllText($@"{path}\{supplier.SupplierName}.json", json);

                        supplier.Connection = Path.Combine(path, $"{supplier.SupplierName}.json");
                        var responceSupplier = await _supplierRepository.Put(supplier.SupplierName, supplier);

                        using (FileStream stream = new FileStream(Path.Combine(path, $"{supplier.SupplierName}.json"), FileMode.Open))
                        {
                            var file = new FormFile(stream, 0, stream.Length, $"{supplier.SupplierName}", $"{supplier.SupplierName}.json")
                            {
                                Headers = new HeaderDictionary(),
                                ContentType = "multipart/form-data",

                            };
                            supplier.Connection = stream.Name;
                            var responce2 = await _supplierRepository.Put(supplier.SupplierName, supplier);
                            stream.Close();
                            stream.Close();

                            Thread myThread = new Thread(() => RunTaskAsync(supplier, null));
                            myThread.Start();

                            return supplier;
                        }
                    }
                    HttpResponseMessage response = await client.GetAsync($"{supplier.SourceSettings.Url}");


                }
                else if (supplier.Type == "XML")
                {
                    HttpClient client = new HttpClient();
                    //TODO: comment
                    client.DefaultRequestHeaders.Add("Accept", "application/json");

                    if (supplier.SourceSettings.Header != null)
                    {
                        var supplierHeader = JsonConvert.DeserializeObject<List<HeaderContent>>(supplier.SourceSettings.Header);
                        foreach (var item in supplierHeader)
                        {
                            client.DefaultRequestHeaders.Add(item.HeaderName, item.HeaderValue);
                        }
                    }
                    else
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", "string");
                    }


                    HttpResponseMessage response = await client.GetAsync($"{supplier.SourceSettings.Url}");
                    string json = await response.Content.ReadAsStringAsync();
                    string wwwPath = this.environment.WebRootPath;
                    string contentPath = this.environment.ContentRootPath;
                    string path = Path.Combine(wwwPath, "FilesSupplier");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    System.IO.File.WriteAllText($@"{path}\{supplier.SupplierName}.xml", json);

                    supplier.Connection = Path.Combine(path, $"{supplier.SupplierName}.xml");
                    var responceSupplier = await _supplierRepository.Put(supplier.SupplierName, supplier);

                    using (FileStream stream = new FileStream(Path.Combine(path, $"{supplier.SupplierName}.xml"), FileMode.Open))
                    {
                        var file = new FormFile(stream, 0, stream.Length, $"{supplier.SupplierName}", $"{supplier.SupplierName}.xml")
                        {
                            Headers = new HeaderDictionary(),
                            ContentType = "multipart/form-data",

                        };
                        supplier.Connection = stream.Name;
                        var responce2 = await _supplierRepository.Put(supplier.SupplierName, supplier);
                        stream.Close();
                        stream.Close();

                        Thread myThread = new Thread(() => RunTaskAsync(supplier, null));
                        myThread.Start();

                        return supplier;
                    }
                }
                else if (supplier.Type == "XLSX")
                {
                    HttpClient client = new HttpClient();
                    //TODO: comment
                    //client.DefaultRequestHeaders.Add("Accept", "application/json");

                    if (supplier.SourceSettings.Header != null)
                    {
                        var supplierHeader = JsonConvert.DeserializeObject<List<HeaderContent>>(supplier.SourceSettings.Header);
                        foreach (var item in supplierHeader)
                        {
                            client.DefaultRequestHeaders.Add(item.HeaderName, item.HeaderValue);
                        }
                    }
                    else
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", "string");
                    }


                    HttpResponseMessage response = await client.GetAsync($"{supplier.SourceSettings.Url}");
                    string json = await response.Content.ReadAsStringAsync();
                    string wwwPath = this.environment.WebRootPath;
                    string contentPath = this.environment.ContentRootPath;
                    string path = Path.Combine(wwwPath, "FilesSupplier");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    File.WriteAllText($@"{path}\{supplier.SupplierName}.xls", json);

                    supplier.Connection = Path.Combine(path, $"{supplier.SupplierName}.xls");
                    var responceSupplier = await _supplierRepository.Put(supplier.SupplierName, supplier);

                    using (FileStream stream = new FileStream(Path.Combine(path, $"{supplier.SupplierName}.xls"), FileMode.Open))
                    {
                        var file = new FormFile(stream, 0, stream.Length, $"{supplier.SupplierName}", $"{supplier.SupplierName}.xls")
                        {
                            Headers = new HeaderDictionary(),
                            ContentType = "multipart/form-data",

                        };
                        supplier.Connection = stream.Name;
                        var responce2 = await _supplierRepository.Put(supplier.SupplierName, supplier);
                        stream.Close();

                        Thread myThread = new Thread(() => RunTaskAsync(supplier, null));
                        myThread.Start();

                        return supplier;
                    }
                }
            }
            else if(supplier.Source == "MAIL")
            {
                string wwwPath = this.environment.WebRootPath;
                string contentPath = this.environment.ContentRootPath;
                string path = Path.Combine(wwwPath, "FilesSupplier");
                using (Pop3Client clientPop3 = new Pop3Client())
                {
                    UTF8Encoding encoding = new UTF8Encoding();
                    clientPop3.Connect(hostname, port, useSsl);
                    clientPop3.Authenticate(username, password);
                    int messageCount = clientPop3.GetMessageCount();
                    List<Message> allMessages = new List<Message>(messageCount);
                    for (int i = messageCount; i > 0; i--)
                    {
                        Message objMessage = clientPop3.GetMessage(i);
                        List<MessagePart> attachment = objMessage.FindAllAttachments();
                        for (int h = 0; h < attachment.Count; h++)
                        {
                            if (attachment[h].FileName.Contains(".xls"))
                            {
                                //File.WriteAllBytes(Path.Combine(path, attachment[h].FileName), attachment[h].Body);
                                if (!Directory.Exists(path))
                                {
                                    Directory.CreateDirectory(path);
                                }
                                File.WriteAllBytes($@"{path}\{supplier.SupplierName}.xls", attachment[h].Body);

                                supplier.Connection = Path.Combine(path, $"{supplier.SupplierName}.xls");
                                var responceSupplier = await _supplierRepository.Put(supplier.SupplierName, supplier);

                                using (FileStream stream = new FileStream(Path.Combine(path, $"{supplier.SupplierName}.xls"), FileMode.Open))
                                {
                                    var file = new FormFile(stream, 0, stream.Length, $"{supplier.SupplierName}", $"{supplier.SupplierName}.xls")
                                    {
                                        Headers = new HeaderDictionary(),
                                        ContentType = "multipart/form-data",

                                    };
                                    supplier.Connection = stream.Name;
                                    var responce2 = await _supplierRepository.Put(supplier.SupplierName, supplier);
                                    stream.Close();

                                    Thread myThread = new Thread(() => RunTaskAsync(supplier, null));
                                    myThread.Start();
                                }

                            }
                        }
                        if (clientPop3.GetMessage(i).Headers.Subject == supplier.SupplierName)
                        {
                            clientPop3.DeleteMessage(i);
                        }
                    }

                }
            }

            return supplier;

        }
        public async Task<List<string>> BindingInternalCode(string supplierName, IFormFile file)
        {
            List<string> InfoProduct = new List<string>();
            string wwwPath = this.environment.WebRootPath;
            string contentPath = this.environment.ContentRootPath;
            string path = Path.Combine(wwwPath, "FilesInternalCodes1C");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
            {
                // The extension is invalid ... discontinue processing the file
            }
            else
            {
                //change
                var currentSupplier = await Connection.context.GetProfileSupplier("Supplier", supplierName);
                if (currentSupplier != null)
                {
                    using (FileStream stream = new FileStream(Path.Combine(path, $"1C {file.FileName}"), FileMode.Create))
                    {
                        file.OpenReadStream();
                        file.CopyTo(stream);

                        currentSupplier.Connection1C = stream.Name;
                        await Connection.context.UpdateSupplier("Supplier", currentSupplier);
                        stream.Close();
                       
                        if (ext == ".json")
                        {
                            var configFileMap = new ExeConfigurationFileMap();
                            configFileMap.ExeConfigFilename = "path to EXE";
                            Configuration config = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
                            var confstring = config.AppSettings.Settings["URLInternalCode"].Value;
                            InfoProduct = documentService.JSONParserInternalCode(confstring, _logger, currentSupplier.SourceSettings.Prefix);
                            //foreach (var item in InfoProduct)
                            //{
                            //    _logger.LogInformation($"{item}");
                            //}
                        }
                       

                    }

                }
                else
                {
                    return InfoProduct;
                }


            }
            string pathLog = Path.Combine(wwwPath, "Logs");
            if (!Directory.Exists(pathLog))
            {
                Directory.CreateDirectory(pathLog);
            }
            // запись в файл
            using (FileStream fstream = new FileStream(Path.Combine(pathLog, $"{supplierName}.txt"), FileMode.Append, FileAccess.Write))
            {
                string fullString = "";
                fullString = String.Join("\n", InfoProduct.ToArray());
                byte[] byteArray = Encoding.UTF8.GetBytes(fullString);
                fstream.Write(byteArray);
                fstream.Close();
            }

            return InfoProduct;
        }

    }
}
