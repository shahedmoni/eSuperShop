﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using eSuperShop.Data;
using JqueryDataTables.LoopsIT;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace eSuperShop.Repository
{
    public class VendorProductCategoryRepository : Repository, IVendorProductCategoryRepository
    {
        public VendorProductCategoryRepository(ApplicationDbContext db, IMapper mapper) : base(db, mapper)
        {
        }

        public VendorProductCategory VendorProductCategory { get; set; }
        public VendorProductCategoryList VendorProductCategoryList { get; set; }
        public void Add(VendorProductCategoryAddModel model)
        {
            VendorProductCategory = _mapper.Map<VendorProductCategory>(model);
            Db.VendorProductCategory.Add(VendorProductCategory);
        }

        public void Delete(int vendorProductCategoryId)
        {
            VendorProductCategory = Db.VendorProductCategory.Find(vendorProductCategoryId);
            Db.VendorProductCategory.Remove(VendorProductCategory);
        }

        public void Update(VendorProductCategoryUpdateModel model)
        {
            var vendorCategory = Db.VendorProductCategory.Find(model.VendorProductCategoryId);

            vendorCategory.VendorProductCategoryId = model.VendorProductCategoryId;
            vendorCategory.Name = model.Name;
            vendorCategory.ImageUrl = model.ImageUrl;
            vendorCategory.DisplayOrder = model.DisplayOrder;
            vendorCategory.IsActive = model.IsActive;

            Db.VendorProductCategory.Update(vendorCategory);
        }

        public bool IsExistName(string name)
        {
            return Db.VendorProductCategory.Any(c => c.Name == name);
        }

        public bool IsExistName(string name, int updateId)
        {
            return Db.VendorProductCategory.Any(c => c.Name == name && c.VendorProductCategoryId != updateId);
        }

        public bool IsNull(int vendorProductCategoryId)
        {
            return !Db.VendorProductCategory.Any(c => c.VendorProductCategoryId == vendorProductCategoryId);
        }

        public bool IsRelatedDataExist(int id)
        {
            return Db.VendorProductCategoryList.Any(c => c.VendorProductCategoryId == id);
        }

        public List<VendorProductCategoryDisplayModel> DisplayList(int vendorId)
        {
            return Db.VendorProductCategory
                .Where(c => c.VendorId == vendorId).OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
                .ProjectTo<VendorProductCategoryDisplayModel>(_mapper.ConfigurationProvider)
                .ToList();
        }

        public List<DDL> ListDdl(int vendorId)
        {
            var ddl = Db.VendorProductCategory
                .ToList().OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
                .Select(c => new DDL
                {
                    value = c.VendorProductCategoryId.ToString(),
                    label = c.Name
                });

            return ddl?.ToList() ?? new List<DDL>();
        }

        public VendorProductCategoryUpdateModel Get(int vendorProductCategoryId)
        {
            return Db.VendorProductCategory
                .ProjectTo<VendorProductCategoryUpdateModel>(_mapper.ConfigurationProvider)
                .FirstOrDefault(c => c.VendorProductCategoryId == vendorProductCategoryId);
        }

        public DataResult<ProductListVendorCategoryWiseModel> ProductList(DataRequest request, int vendorId, int vendorProductCategoryId)
        {
            return (from p in Db.Product
                    join c in Db.VendorProductCategoryList on p.ProductId equals c.ProductId into pc
                    from m in pc.DefaultIfEmpty()
                    where p.VendorId == vendorId && m.VendorProductCategoryId == vendorProductCategoryId
                    select new ProductListVendorCategoryWiseModel
                    {
                        ProductId = p.ProductId,
                        CatalogId = p.CatalogId,
                        CatalogName = p.Catalog.CatalogName,
                        BrandId = p.BrandId,
                        BrandName = p.Brand.Name,
                        Name = p.Name,
                        SlugUrl = p.SlugUrl,
                        Price = p.Price,
                        OldPrice = p.OldPrice,
                        StockQuantity = p.StockQuantity,
                        UpdatedOnUtc = p.UpdatedOnUtc,
                        IsAssign = m != null
                    })
                .OrderBy(p => p.CatalogName).ThenBy(p => p.Name)
                .ToDataResult(request);
        }

        public void PlaceAssign(VendorProductCategoryAssignModel model)
        {
            if (IsPlaceAssign(model)) return;

            VendorProductCategoryList = _mapper.Map<VendorProductCategoryList>(model);
            Db.VendorProductCategoryList.Add(VendorProductCategoryList);
        }

        public bool IsPlaceAssign(VendorProductCategoryAssignModel model)
        {
            VendorProductCategoryList = Db.VendorProductCategoryList.FirstOrDefault(c => c.VendorProductCategoryId == model.VendorProductCategoryId && c.ProductId == model.ProductId);
            return VendorProductCategoryList != null;
        }

        public void PlaceDelete(VendorProductCategoryAssignModel model)
        {
            VendorProductCategoryList = Db.VendorProductCategoryList.FirstOrDefault(c => c.VendorProductCategoryId == model.VendorProductCategoryId && c.ProductId == model.ProductId);
            Db.VendorProductCategoryList.Remove(VendorProductCategoryList);
        }

        public List<VendorCategoryProductsModel> Products(int vendorProductCategoryId)
        {
            throw new System.NotImplementedException();
        }
    }
}