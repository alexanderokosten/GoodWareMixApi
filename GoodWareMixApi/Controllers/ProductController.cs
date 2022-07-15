using GoodWareMixApi.Filter;
using GoodWareMixApi.Model;
using GoodWareMixApi.Service;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using GoodWareMixApi.Repositories;
using GoodWareMixApi.Interfaces;
using GoodWareMixApi.Intrefaces;
using System.Net.Http.Headers;
using System.Net.WebSockets;

using Newtonsoft.Json;
using GoodWareMixApi.Model.Settings;
using GoodWareMixApi.Quartz;
using GoodWareMixApi.Model.Entity;
using OpenPop.Pop3;
using OpenPop.Mime;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GoodWareMixApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : Controller
    {

      
   

        
        private readonly IUriService uriService;
        Microsoft.AspNetCore.Hosting.IHostingEnvironment environment;
        ParserDocuments documentService = new ParserDocuments();
        private readonly ILogger<ProductController> _logger;

        private IProductRepository _productRepository;
        private ISupplierRepository _supplierRepository;
        private ISchedulerTaskRepository _schedulerTaskRepository;
        private IHostedService _hostedService;

        public ProductController(IUriService uriService,
            Microsoft.AspNetCore.Hosting.IHostingEnvironment env, 
            ILogger<ProductController> logger, 
            IProductRepository productRepository,
            ISupplierRepository supplierRepository, 
            IHostedService hostedService, 
            ISchedulerTaskRepository schedulerTaskRepository)
        {
            this.uriService = uriService;
            this.environment = env;

            _logger = logger;
            _productRepository = productRepository;
            //documentService.environment = environment;
            _supplierRepository = supplierRepository;
            _hostedService = hostedService;
            _schedulerTaskRepository = schedulerTaskRepository;
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        //GET: api/<ProductController>
        [HttpGet()]
        public async Task<PagedResponse<List<Product>>> Get([FromQuery] PaginationFilter filter)
        {
            var route = Request.Path.Value;
            var products = await _productRepository.Get(filter, route);
            return products;

        }
        [HttpGet("startScheduler/{task}")]
        public async Task<SchedulerTask> SchedulPinTask(string task)
        {

            //TODO: QUARTZ Scheduler
            var schedullerAdd = await _schedulerTaskRepository.GetTask(task);
            if(schedullerAdd != null)
            {

                DataScheduler dataScheduler = new DataScheduler();
                dataScheduler.Start(schedullerAdd, _productRepository, _supplierRepository, _schedulerTaskRepository);

                _logger.LogInformation("Task started");
                return schedullerAdd;
            }
            else
            {
                return null;
            } 

        }
        [HttpGet("stopScheduler/{task}")]
        public async Task<SchedulerTask> SchedulerStop(string task)
        {

            //TODO: QUARTZ Scheduler
            var scheduller = await _schedulerTaskRepository.GetTask(task);
           // var result = _schedulerTaskRepository.Post(task);


            DataScheduler dataScheduler = new DataScheduler();
            dataScheduler.Pause(scheduller, _productRepository, _supplierRepository, _schedulerTaskRepository);

            _logger.LogInformation("Task stopped");
            return scheduller;




        }

        // GET api/<ProductController>/5
        [HttpGet("GetProduct")]
        public async Task<Product> Get([FromQuery]string internalCode)
        {
            var products = await _productRepository.Get(internalCode);
            if(products == null)
            {
                return null;
            }
            else
            {
                return products;
            }

        }
        [HttpPost("InternalCodeBinding/{supplierName}")]
        [RequestSizeLimit(Int32.MaxValue)]
        public async Task<ActionResult> InternalCodeBinding(string supplierName,IFormFile file)
        {
            Thread thread = new Thread(() => _productRepository.BindingInternalCode(supplierName, file));
            thread.Start();
           
            Task.Delay(5000);
            List<string> result = new List<string>();
            result.Add("Ok!");
            //var result = await _productRepository.BindingInternalCode(supplierName, file);
            return Ok();

        }
        [HttpGet("{id}")]
        public async Task<Product> GetProduct(string id)
        {
            var products = await _productRepository.GetProductById(id);
            if (products == null)
            {
                return null;
            }
            else
            {
                return products;
            }

        }
        [HttpPost]
        public async Task<ActionResult<ProFileSupplier>> RunTaskAsync([FromQuery] string name)
        {
            var supplier = _supplierRepository.Get(name);
            if (supplier.Source == "API")
            {
                if (supplier.Type == "JSON")
                {
                    HttpClient client = new HttpClient();
                    client.Timeout = TimeSpan.FromHours(12);
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
                            //var bodyData = new StringContent(supplier.SourceSettings.Body, Encoding.UTF8, "application/json"); 23523631261236
                            HttpResponseMessage response = await client.PostAsync($"{supplier.SourceSettings.Url}", null);
                            string json = await response.Content.ReadAsStringAsync();
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
                                // stream.Close();
                                stream.Close();
                                Thread myThread = new Thread(() => _productRepository.RunTaskAsync(supplier, file));
                                myThread.Start();
                                return Ok();//var result = await _productRepository.RunTaskAsync(supplier.SupplierName, file);


                            }
                        }


                    }
                    if (supplier.SourceSettings.MethodType == "GET")
                    {


                        var response = client.GetAsync($"{supplier.SourceSettings.Url}").Result;
                        string json = response.Content.ReadAsStringAsync().Result;
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
                            // stream.Close();
                            stream.Close();

                            Thread myThread = new Thread(() => _productRepository.RunTaskAsync(supplier, file));
                            myThread.Start();

                            return Ok();
                        }
                    }
                    //HttpResponseMessage response = await client.GetAsync($"{supplier.SourceSettings.Url}");


                }
                else if (supplier.Type == "XML")
                {
                    HttpClient client = new HttpClient();
                    //TODO:comment
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
                        // stream.Close();
                        stream.Close();

                        Thread myThread = new Thread(() => _productRepository.RunTaskAsync(supplier, file));
                        myThread.Start();

                        return Ok();
                    }
                }
                else if (supplier.Type == "XLSX")
                {
                    HttpClient client = new HttpClient();
                    //TODO:comment
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


                    var response = client.GetByteArrayAsync($"{supplier.SourceSettings.Url}").Result;
                    string wwwPath = this.environment.WebRootPath;
                    string contentPath = this.environment.ContentRootPath;
                    string path = Path.Combine(wwwPath, "FilesSupplier");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    System.IO.File.WriteAllBytes($@"{path}\{supplier.SupplierName}.xls", response);

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
                        // stream.Close();
                        stream.Close();

                        Thread myThread = new Thread(() => _productRepository.RunTaskAsync(supplier, file));
                        myThread.Start();

                        return Ok();
                    }

                    return Ok();
                }
            }
            else if(supplier.Source == "MAIL")
            {
                if (supplier.Type == "XLSX")
                {
                    string wwwPath = this.environment.WebRootPath;
                    string contentPath = this.environment.ContentRootPath;
                    string path = Path.Combine(wwwPath, "FilesSupplier");
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
                        List<OpenPop.Mime.Message> allMessages = new List<OpenPop.Mime.Message>(messageCount);
                        for (int i = messageCount; i > 0; i--)
                        {
                            var objMessage = clientPop3.GetMessage(i);
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
                                        System.IO.File.WriteAllBytes($@"{path}\{supplier.SupplierName}.xls", attachment[h].Body);

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

                                            Thread myThread = new Thread(() => _productRepository.RunTaskAsync(supplier, file));
                                            myThread.Start();
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

            return Ok();


        }


        private string[] permittedExtensions = { ".csv", ".xlsx",".xml",".xls",".json",".yml"};
       // POST api/<ProductController>
        [HttpPost("{supplierName}")]
        [RequestSizeLimit(Int32.MaxValue)]
        public async Task<List<string>> Post(string supplierName, IFormFile file)
        {

            var result = await _productRepository.Post(supplierName, file);


            return result;

        }

        // PUT api/<ProductController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ProductController>/5
        [HttpDelete("{id}")]
        public void Delete(string id)
        {
        }
    }
}
