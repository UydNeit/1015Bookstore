﻿using _1015bookstore.viewmodel.Catalog.Products;
using _1015bookstore.viewmodel.Comon;
using _1015bookstore.viewmodel.System.Users;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace _1015bookstore.websiteadmin.Service
{
    public class ProductAPIClient : IProductAPIClient
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProductAPIClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task<ResponseAPI<List<ProductViewModel>>> GetProduct(string session)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("https://localhost:7139");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", session);
            var response = await client.GetAsync($"/api/product/admin-getall");
            var body = await response.Content.ReadAsStringAsync();
            var products = JsonSerializer.Deserialize<List<ProductViewModel>>(body);
            return new ResponseAPI<List<ProductViewModel>>
            {
                Data = products,
                Status = response.StatusCode == System.Net.HttpStatusCode.OK ? true : false,
            };
        }

        public async Task<ResponseAPI<ProductViewModel>> GetProductById(string session, int product_id)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("https://localhost:7139");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", session);

            var response = await client.GetAsync($"/api/product/{product_id}");
            var body = await response.Content.ReadAsStringAsync();
            var product = JsonSerializer.Deserialize<ProductViewModel>(body);

            return new ResponseAPI<ProductViewModel>
            {
                Data = product,
                Status = response.StatusCode == System.Net.HttpStatusCode.OK ? true : false,
            };
        }

        public async Task<ResponseAPI<string>> CraeteProduct(ProductCreateRequest request, string session, Guid user_id)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("https://localhost:7139");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", session);

            var requestContent = new MultipartFormDataContent();
            if (request.sImage_pathThumbnail != null)
            {
                byte[] data;
                using (var br = new BinaryReader(request.sImage_pathThumbnail.OpenReadStream()))
                {
                    data = br.ReadBytes((int)request.sImage_pathThumbnail.OpenReadStream().Length);
                }
                ByteArrayContent bytes = new ByteArrayContent(data);
                requestContent.Add(bytes, "sImage_pathThumbnail", request.sImage_pathThumbnail.FileName);
            }

            requestContent.Add(new StringContent(request.sProduct_name.ToString()), "sProduct_name");
            requestContent.Add(new StringContent(request.vProduct_price.ToString()), "vProduct_price");
            requestContent.Add(new StringContent(request.iProduct_waranty.ToString()), "iProduct_waranty");
            requestContent.Add(new StringContent(request.iProduct_quantity.ToString()), "iProduct_quantity");
            requestContent.Add(new StringContent(request.sProduct_description.ToString()), "sProduct_description");
            requestContent.Add(new StringContent(request.sProduct_brand == null?"": request.sProduct_brand.ToString()), "sProduct_brand");
            requestContent.Add(new StringContent(request.sProduct_brand == null ? "" : request.sProduct_brand.ToString()), "sProduct_madein");
            requestContent.Add(new StringContent(request.dtProduct_mfgdate == null ? "" : request.dtProduct_mfgdate.ToString()), "dtProduct_mfgdate");
            requestContent.Add(new StringContent(request.sProduct_suppiler == null ? "" : request.sProduct_suppiler.ToString()), "sProduct_suppiler");
            requestContent.Add(new StringContent(request.sProduct_author == null ? "" : request.sProduct_author.ToString()), "sProduct_author");
            requestContent.Add(new StringContent(request.sProduct_nop == null ? "" : request.sProduct_nop.ToString()), "sProduct_nop");
            requestContent.Add(new StringContent(request.sProduct_nop == null ? "" : request.sProduct_nop.ToString()), "c");

            var response = await client.PostAsync($"/api/product?gCreator_id={user_id}", requestContent);
            
            return new ResponseAPI<string>
            {
                Message = await response.Content.ReadAsStringAsync(),
                Status = response.StatusCode == System.Net.HttpStatusCode.Created ? true : false,
            };
        }
    }
}
