﻿using _1015bookstore.application.Common;
using _1015bookstore.application.Helper;
using _1015bookstore.data.EF;
using _1015bookstore.data.Entities;
using _1015bookstore.data.Enums;
using _1015bookstore.utility.Exceptions;
using _1015bookstore.viewmodel.Catalog.Carts;
using _1015bookstore.viewmodel.Catalog.Products;
using _1015bookstore.viewmodel.Comon;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

namespace _1015bookstore.application.Catalog.Products
{
    public class ProductService : IProductService
    {
        private readonly _1015DbContext _context;
        private readonly IStorageService _storageService;
        private readonly IRemoveUnicode _removeUnicode;

        public ProductService(_1015DbContext context, IStorageService storageService, IRemoveUnicode removeUnicode)
        {
            _context = context;
            _storageService = storageService;
            _removeUnicode = removeUnicode;
        }

        public async Task AddViewcount(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) throw new _1015Exception($"Cannot find a product with id: {id}");

            product.view_count += 1;
            await _context.SaveChangesAsync();
        }

        public async Task<ResponseService<ProductViewModel>> Create(ProductCreateRequest request)
        {
            var product = new Product()
            {
                name = request.name,
                alias = _removeUnicode.Removeunicode(request.name),
                price = request.price,
                start_count = 0,
                review_count = 0,
                buy_count = 0,
                flashsale = false,
                like_count = 0,
                waranty = request.waranty,
                quanity = request.quanity,
                view_count = 0,
                description = request.description,
                brand = request.brand,
                madein = request.madein,
                mfgdate = request.mfgdate,
                suppiler = request.suppiler,
                author = request.author,
                nop = request.nop,
                yop = request.yop,
                createdby = "Hệ thống",
                datecreated = DateTime.Now,
                updatedby = "Hệ thống",
                dateupdated = DateTime.Now,
                status = ProductStatus.Normal
            };

            
            ProductImage productImage = null;

            //Save image
            if (request.ThumbnailImage != null)
            {
                
                productImage = new ProductImage()
                {
                    caption = "Thumbnail image",
                    createdate = DateTime.Now,
                    sizeimage = request.ThumbnailImage.Length,
                    imagepath = await this.SaveFile(request.ThumbnailImage),
                    is_default = true,
                    sortorder = 1
                };

                product.productimages = new List<ProductImage>()
                {
                    productImage
                };
            }
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return new ResponseService<ProductViewModel>
            {
                CodeStatus = 201,
                Status = true,
                Message = "Success",
                Data = new ProductViewModel
                {
                    id = product.id,
                    name = product.name,
                    price = product.price,
                    start_count = product.start_count,
                    review_count = product.review_count,
                    buy_count = product.buy_count,
                    flashsale = product.flashsale,
                    like_count = product.like_count,
                    waranty = product.waranty,
                    quanity = product.quanity,
                    view_count = product.view_count,
                    description = product.description,
                    brand = product.brand,
                    madein = product.madein,
                    mfgdate = product.mfgdate,
                    supplier = product.suppiler,
                    author = product.author,
                    nop = product.nop,
                    yop = product.yop,
                    status = product.status,
                    pathThumbnailImage = productImage == null ? null : productImage.imagepath,
                },
            };
        
        }

        public async Task<int> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null) throw new _1015Exception($"Cannot find a product with id: {id}");

            var images = _context.ProductImages.Where(i => i.product_id == id);
            foreach (var image in images)
            {
                await _storageService.DeleteFileAsync(image.imagepath);
            }

            product.status = ProductStatus.Delete;

            return await _context.SaveChangesAsync();
        }

        public async Task<PagedResult<ProductViewModel>> GetProductByKeyWordPaging(GetProductByKeyWordPagingRequest request)
        {
            var query = from p in _context.Products
                        join pic in _context.ProductInCategory on p.id equals pic.product_id
                        join pimg in _context.ProductImages on p.id equals pimg.product_id into ppimg
                        from pimg in ppimg.DefaultIfEmpty()
                        where p.status != ProductStatus.Delete
                        select new { p, pic, pimg };

            if (!string.IsNullOrEmpty(request.keyword))
                query = query.Where(x => x.p.alias.Contains(_removeUnicode.Removeunicode(request.keyword)));

            int totalRow = await query.CountAsync();

            var data = await query.Skip((request.pageindex - 1) * request.pagesize).Take(request.pagesize)
            .Select(x => new ProductViewModel()
            {
                id = x.p.id,
                name = x.p.name,
                price = x.p.price,
                start_count = x.p.start_count,
                review_count = x.p.review_count,
                buy_count = x.p.buy_count,
                flashsale = x.p.flashsale,
                like_count = x.p.like_count,
                waranty = x.p.waranty,
                quanity = x.p.quanity,
                view_count = x.p.view_count,
                description = x.p.description,
                brand = x.p.brand,
                madein = x.p.madein,
                mfgdate = x.p.mfgdate,
                supplier = x.p.suppiler,
                author = x.p.author,
                nop = x.p.nop,
                yop = x.p.yop,
                status = x.p.status,
                pathThumbnailImage = x.pimg.imagepath
            }).ToListAsync();
            var pagedResult = new PagedResult<ProductViewModel>()
            {
                totalrecord = totalRow,
                items = data
            };
            return pagedResult;
        }

        public async Task<ResponseService<ProductViewModel>> GetById(int id)
        {
            var query = from p in _context.Products
                        join pimg in _context.ProductImages on p.id equals pimg.product_id into ppimg
                        from pimg in ppimg.DefaultIfEmpty()
                        where p.status != ProductStatus.Delete
                        select new { p, pimg };

            var product = await query.FirstOrDefaultAsync(x => x.p.id == id);

            if (product == null)
                return new ResponseService<ProductViewModel> {
                    CodeStatus = 400,
                    Status = false,
                    Message = $"Cannot find a product with id: {id}",
                };

            var data =  new ProductViewModel
            {
                id = product.p.id,
                name = product.p.name,
                price = product.p.price,
                start_count = product.p.start_count,
                review_count = product.p.review_count,
                buy_count = product.p.buy_count,
                flashsale = product.p.flashsale,
                like_count = product.p.like_count,
                waranty = product.p.waranty,
                quanity = product.p.quanity,
                view_count = product.p.view_count,
                description = product.p.description,
                brand = product.p.brand,
                madein = product.p.madein,
                mfgdate = product.p.mfgdate,
                supplier = product.p.suppiler,
                author = product.p.author,
                nop = product.p.nop,
                yop = product.p.yop,
                status = product.p.status,
                pathThumbnailImage = product.pimg == null ? null : product.pimg.imagepath,
            };

            return new ResponseService<ProductViewModel>
            {
                CodeStatus = 200,
                Status = true,
                Message = "Success",
                Data = data,
            };
        }

        public async Task<ResponseService> UpdataQuanity(int id, int addedQuantity)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return new ResponseService()
                {
                    CodeStatus = 400,
                    Status = false,
                    Message = $"Cannot find a product with id: {id}",
                };
            
            if (product.status == ProductStatus.OOS && addedQuantity < 0)
                return new ResponseService()
                {
                    CodeStatus = 400,
                    Status = false,
                    Message = $"Cannot update because product is out of stock",
                };

            if (product.quanity + addedQuantity >= 1)
                product.quanity += addedQuantity;
            else if (product.quanity + addedQuantity == 0)
            {
                product.quanity = 0;
                product.status = ProductStatus.OOS;
            }
            else
            {
                return new ResponseService()
                {
                    CodeStatus = 400,
                    Status = false,
                    Message = $"Cannot update because product is out of stock",
                };
            }

            if (await _context.SaveChangesAsync() > 0)
            {
                return new ResponseService()
                {
                    CodeStatus = 200,
                    Status = true,
                    Message = $"Success",
                };
            }
            return new ResponseService()
            {
                CodeStatus = 500,
                Status = false,
                Message = $"Cannot update a product with id: {id}",
            };
        }

        public async Task<ResponseService> Update(ProductUpdateRequest request)
        {
            var product = await _context.Products.FindAsync(request.id);

            if (product == null)
                return new ResponseService()
                {
                    CodeStatus = 400,
                    Status = false,
                    Message = $"Cannot find a product with id: {request.id}",
                };

            product.name = request.name;
            product.price = request.price;
            product.waranty = request.waranty;
            product.quanity = request.quanity;
            product.description = request.description;
            product.brand = request.brand;
            product.madein = request.madein;
            product.mfgdate = request.mfgdate;
            product.suppiler = request.suppiler;
            product.author = request.author;
            product.nop = request.nop;
            product.yop = request.yop;

            //Save image
            if (request.ThumbnailImage != null)
            {
                var thumbnailImage = await _context.ProductImages.FirstOrDefaultAsync(i => i.is_default == true && i.product_id == request.id);
                if (thumbnailImage != null)
                {
                    thumbnailImage.sizeimage = request.ThumbnailImage.Length;
                    thumbnailImage.imagepath = await this.SaveFile(request.ThumbnailImage);
                    _context.ProductImages.Update(thumbnailImage);
                }
            }

            if(await _context.SaveChangesAsync() > 0)
            {
                return new ResponseService()
                {
                    CodeStatus = 200,
                    Status = true,
                    Message = $"Success",
                };
            }
            return new ResponseService()
            {
                CodeStatus = 500,
                Status = false,
                Message = $"Cannot update a product with id: {request.id}",
            };
        }

        public async Task<ResponseService> UpdatePrice(int id, decimal newPrice)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return new ResponseService()
                {
                    CodeStatus = 400,
                    Status = false,
                    Message = $"Cannot find a product with id: {id}",
                };
            product.price = newPrice;
            if (await _context.SaveChangesAsync() > 0)
            {
                return new ResponseService()
                {
                    CodeStatus = 200,
                    Status = true,
                    Message = $"Success",
                };
            }
            return new ResponseService()
            {
                CodeStatus = 500,
                Status = false,
                Message = $"Cannot update a product with id: {id}",
            };
        }

        private async Task<string> SaveFile(IFormFile file)
        {
            var originalFileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim();
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
            await _storageService.SaveFileAsync(file.OpenReadStream(), fileName);
            return fileName;
        }

        public async Task<List<ProductViewModel>> GetAll()
        {
            var query = from p in _context.Products
                        join pimg in _context.ProductImages on p.id equals pimg.product_id into ppimg
                        from pimg in ppimg.DefaultIfEmpty()
                        where p.status != ProductStatus.Delete
                        select new { p, pimg };

            var data = await query.Select(e => new ProductViewModel
            {
                id = e.p.id,
                name = e.p.name,
                price = e.p.price,
                start_count = e.p.start_count,
                review_count = e.p.review_count,
                buy_count = e.p.buy_count,
                flashsale = e.p.flashsale,
                like_count = e.p.like_count,
                waranty = e.p.waranty,
                quanity = e.p.quanity,
                view_count = e.p.view_count,
                description = e.p.description,
                brand = e.p.brand,
                madein = e.p.madein,
                mfgdate = e.p.mfgdate,
                supplier = e.p.suppiler,
                author = e.p.author,
                nop = e.p.nop,
                yop = e.p.yop,
                status = e.p.status,
                pathThumbnailImage = e.pimg.imagepath
            }).ToListAsync();
            return data;
        }

        public async Task<PagedResult<ProductViewModel>> GetProductByCategoryId(GetProductByCategoryPagingRequest request)
        {
            var query = from p in _context.Products
                        join pic in _context.ProductInCategory on p.id equals pic.product_id
                        join pimg in _context.ProductImages on p.id equals pimg.product_id into ppimg
                        from pimg in ppimg.DefaultIfEmpty()
                        where p.status != ProductStatus.Delete
                        select new { p, pic, pimg };

            if (request.category_ids.Count > 0)
            {
                query = query.Where(p => request.category_ids.Contains(p.pic.category_id));
            }

            int totalRow = await query.CountAsync();

            var data = await query.Skip((request.pageindex - 1) * request.pagesize).Take(request.pagesize)
            .Select(x => new ProductViewModel()
            {
                id = x.p.id,
                name = x.p.name,
                price = x.p.price,
                start_count = x.p.start_count,
                review_count = x.p.review_count,
                buy_count = x.p.buy_count,
                flashsale = x.p.flashsale,
                like_count = x.p.like_count,
                waranty = x.p.waranty,
                quanity = x.p.quanity,
                view_count = x.p.view_count,
                description = x.p.description,
                brand = x.p.brand,
                madein = x.p.madein,
                mfgdate = x.p.mfgdate,
                supplier = x.p.suppiler,
                author = x.p.author,
                nop = x.p.nop,
                yop = x.p.yop,
                status = x.p.status,
                pathThumbnailImage = x.pimg.imagepath,
            }).ToListAsync();
            var pagedResult = new PagedResult<ProductViewModel>()
            {
                totalrecord = totalRow,
                items = data
            };
            return pagedResult;
        }
    }
}
