﻿using _1015bookstore.viewmodel.Catalog.Products;
using _1015bookstore.viewmodel.Comon;

namespace _1015bookstore.websiteadmin.Service
{
    public interface IProductAPIClient
    {
        Task<ResponseAPI<List<ProductViewModel>>> GetProduct(string session);
        Task<ResponseAPI<ProductViewModel>> GetProductById(string session, int product_id);
        Task<ResponseAPI<string>> CraeteProduct(ProductCreateRequest request, string session, Guid user_id);
    }
}
