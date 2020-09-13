﻿using AutoMapper;
using eSuperShop.Data;

namespace eSuperShop.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        public UnitOfWork(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;

            Attribute = new AttributeRepository(_db, _mapper);
            Brand = new BrandRepository(_db, _mapper);
            Registration = new RegistrationRepository(_db, _mapper);
            Slider = new SliderRepository(_db, _mapper);
            Specification = new SpecificationRepository(_db, _mapper);
            Catalog = new CatalogRepository(_db, _mapper);
            Warehouse = new WarehouseRepository(_db, _mapper);
        }

        public IAttributeRepository Attribute { get; }
        public IBrandRepository Brand { get; }
        public IRegistrationRepository Registration { get; }
        public ISliderRepository Slider { get; }
        public ISpecificationRepository Specification { get; }
        public ICatalogRepository Catalog { get; }
        public IWarehouseRepository Warehouse { get; }

        public void Dispose()
        {
            _db.Dispose();
        }


        public int SaveChanges()
        {
            return _db.SaveChanges();
        }
    }
}