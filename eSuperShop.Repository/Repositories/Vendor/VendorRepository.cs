﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using eSuperShop.Data;
using JqueryDataTables.LoopsIT;
using Microsoft.EntityFrameworkCore;
using Paging.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eSuperShop.Repository
{
    public class VendorRepository : Repository, IVendorRepository
    {
        public VendorRepository(ApplicationDbContext db, IMapper mapper) : base(db, mapper)
        {
        }

        public Vendor Vendor { get; set; }
        public VendorCatalog VendorCatalog { get; set; }
        public void Add(VendorAddModel model)
        {
            Vendor = _mapper.Map<Vendor>(model);
            Db.Vendor.Add(Vendor);
        }


        public VendorModel Get(int id)
        {
            return Db.Vendor
                 .ProjectTo<VendorModel>(_mapper.ConfigurationProvider)
                 .FirstOrDefault(c => c.VendorId == id);
        }

        public VendorDashboardModel Dashboard(int id)
        {
            var dashboard = new VendorDashboardModel
            {
                VendorInfo = Db.Vendor
                    .ProjectTo<VendorInfoModel>(_mapper.ConfigurationProvider)
                    .FirstOrDefault(c => c.VendorId == id)
            };

            dashboard.Catalogs = Catalogs(id);

            return dashboard;
        }

        public bool IsExistPhone(string phone)
        {
            return Db.Vendor.Any(c => c.VerifiedPhone == phone);
        }

        public bool IsExistEmail(string email)
        {
            return Db.Vendor.Any(c => c.Email == email);
        }

        public bool IsExistStore(string store)
        {
            return Db.Vendor.Any(c => c.StoreName == store);
        }

        public bool IsExistStore(string store, int updateVendorId)
        {
            return Db.Vendor.Any(c => c.StoreName == store && c.VendorId != updateVendorId);
        }

        public bool IsExistSlugUrl(string slugUrl)
        {
            return Db.Vendor.Any(c => c.StoreSlugUrl == slugUrl);
        }
        public bool IsExistSlugUrl(string slugUrl, int updateVendorId)
        {
            return Db.Vendor.Any(c => c.StoreSlugUrl == slugUrl && c.VendorId != updateVendorId);
        }
        public bool IsNull(int id)
        {
            return !Db.Vendor.Any(c => c.VendorId == id);
        }

        public DataResult<VendorModel> List(DataRequest request)
        {
            var list = Db.Vendor
                .ProjectTo<VendorModel>(_mapper.ConfigurationProvider);

            return list.ToDataResult(request);
        }


        public async Task<ICollection<VendorModel>> SearchAsync(string key)
        {
            return await Db.Vendor
                .Where(c => c.VerifiedPhone.Contains(key) || c.StoreName.Contains(key))
                .ProjectTo<VendorModel>(_mapper.ConfigurationProvider)
                .Take(5)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public void AssignCatalogMultiple(VendorCatalogAssignModel model)
        {
            var assignList = new List<VendorCatalog>();


            foreach (var item in model.Catalogs)
            {
                if (!IsExistVendorInCatalog(model.VendorId, item.CatalogId))
                {
                    assignList.Add(new VendorCatalog
                    {
                        VendorId = model.VendorId,
                        CatalogId = item.CatalogId,
                        CommissionPercentage = item.CommissionPercentage,
                        AssignedByRegistrationId = model.AssignedByRegistrationId
                    });
                }
            }


            if (assignList.Any())
            {
                Db.VendorCatalog.AddRange(assignList);
            }
        }

        public void UnAssignCatalog(int VendorId, int catalogId)
        {
            VendorCatalog = Db.VendorCatalog.FirstOrDefault(c => c.VendorId == VendorId && c.CatalogId == catalogId);
            Db.VendorCatalog.Remove(VendorCatalog);
        }

        public bool IsExistVendorInCatalog(int vendorId, int catalogId)
        {
            return Db.VendorCatalog.Any(c => c.VendorId == vendorId && c.CatalogId == catalogId);
        }

        public List<VendorModel> CatalogWiseList(int catalogId)
        {
            var list = Db.VendorCatalog
                .Include(c => c.Vendor)
                .Where(c => c.CatalogId == catalogId)
                .Select(c => c.Vendor)
                .ProjectTo<VendorModel>(_mapper.ConfigurationProvider);
            return list.ToList();
        }

        public void Approved(VendorApprovedModel model)
        {
            var vendor = Db.Vendor.Find(model.VendorId);
            var registration = new Registration
            {
                UserName = vendor.Email,
                Validation = true,
                Type = UserType.Seller,
                Name = vendor.AuthorizedPerson,
                Phone = vendor.VerifiedPhone,
                Email = vendor.Email
            };
            vendor.Registration = registration;
            vendor.ApprovedByRegistrationId = model.ApprovedByRegistrationId;
            vendor.ApprovedOnUtc = DateTime.UtcNow;
            vendor.IsApproved = true;

            Db.Vendor.Update(vendor);
        }

        public void Delete(int vendorId)
        {
            var vendor = Db.Vendor
                .Include(v => v.VendorCatalog)
                .FirstOrDefault(v => v.VendorId == vendorId);
            Db.Vendor.Remove(vendor);
        }

        public bool IsApproved(int vendorId)
        {
            return Db.Vendor.Find(vendorId).IsApproved;
        }

        public string GetPhone(int vendorId)
        {
            return Db.Vendor.Find(vendorId).VerifiedPhone;
        }

        public List<VendorCatalogViewModel> Catalogs(int vendorId)
        {
            var list = Db
                .VendorCatalog
                .Include(v => v.Catalog)
                .ThenInclude(c => c.ParentCatalog)
                .AsEnumerable()?.ToList()
                .Select(c => new VendorCatalogViewModel
                {
                    CatalogId = c.CatalogId,
                    CatalogName = CatalogDllFunction(c.Catalog.ParentCatalog, c.Catalog.CatalogName),
                    CommissionPercentage = c.CommissionPercentage
                }).OrderBy(l => l.CatalogName);
            return list?.ToList() ?? new List<VendorCatalogViewModel>();



        }

        public bool IsCatalogExist(int vendorId, int catalogId)
        {
            return Db.VendorCatalog.Any(v => v.CatalogId == catalogId && v.VendorId == vendorId);
        }

        public List<DDL> ThemeDdl()
        {
            var list = from StoreTheme a in Enum.GetValues(typeof(StoreTheme))
                       select
                           new DDL
                           {
                               label = a.GetDescription(),
                               value = a.ToString()
                           };
            return list.ToList();
        }

        public void ThemeChange(int vendorId, StoreTheme theme)
        {
            var vendor = Db.Vendor.Find(vendorId);
            vendor.StoreTheme = theme;
            Db.Vendor.Update(vendor);
        }

        public void SlugUrlChange(int vendorId, string slugUrl)
        {
            var vendor = Db.Vendor.Find(vendorId);
            vendor.StoreSlugUrl = slugUrl;
            Db.Vendor.Update(vendor);
        }

        public void BanarUrlChange(int vendorId, string banarUrl)
        {
            var vendor = Db.Vendor.Find(vendorId);
            vendor.StoreBannerUrl = banarUrl;
            Db.Vendor.Update(vendor);
        }

        public void StoreInfoUpdate(VendorStoreInfoUpdateModel model)
        {
            var vendor = Db.Vendor.Find(model.VendorId);

            vendor.StoreName = model.StoreName;
            vendor.StoreAddress = model.StoreAddress;
            vendor.StoreSlugUrl = model.StoreSlugUrl;
            vendor.StoreBannerUrl = model.StoreBannerUrl;
            vendor.StoreLogoUrl = model.StoreLogoUrl;
            vendor.StoreTagLine = model.StoreTagLine;
            Db.Vendor.Update(vendor);
        }

        public VendorStoreInfoUpdateModel StoreDetails(int vendorId)
        {
            return Db.Vendor
                .Where(c => c.VendorId == vendorId)
                .ProjectTo<VendorStoreInfoUpdateModel>(_mapper.ConfigurationProvider)
                .FirstOrDefault();
        }

        public PagedResult<StoreViewModel> TopStores(StoreFilterRequest request)
        {
            var stores = Db.Vendor
                .Where(v => v.IsApproved)
                .ProjectTo<StoreViewModel>(_mapper.ConfigurationProvider)
                .OrderBy(s => s.Rating).ThenBy(s => s.RatingBy)
                .GetPaged(request.Page, request.PageSize);
            return stores;
        }

        public StoreThemeViewModel StoreThemeDetails(string storeSlugUrl)
        {
            return Db.Vendor
                .Where(c => c.StoreSlugUrl == storeSlugUrl)
                .ProjectTo<StoreThemeViewModel>(_mapper.ConfigurationProvider)
                .FirstOrDefault();
        }

        string CatalogDllFunction(Catalog catalog, string cat)
        {

            if (catalog != null)
            {
                cat = CatalogDllFunction(catalog.ParentCatalog, catalog.CatalogName) + ">" + cat;
            }

            return cat;
        }
    }
}