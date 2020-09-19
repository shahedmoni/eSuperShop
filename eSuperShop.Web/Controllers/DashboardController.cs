﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eSuperShop.BusinessLogic;
using eSuperShop.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace eSuperShop.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IVendorDashboardCore _sellerDashboard;
        private readonly IVendorCore _vendor;

        public DashboardController(IVendorDashboardCore sellerDashboard, IVendorCore vendor)
        {
            _sellerDashboard = sellerDashboard;
            _vendor = vendor;
        }

        [Authorize(Roles = "admin, sub-admin")]
        public IActionResult Index()
        {
            return View();
        }

        //seller dashboard
        [Authorize(Roles = "Seller")]
        public IActionResult Seller()
        {
            var model = _sellerDashboard.GetDetails(User.Identity.Name);
            ViewBag.ThemeType = new SelectList(_vendor.ThemeDdl(), "value", "label");

            return View(model.Data);
        }

        [Authorize(Roles = "Seller")]
        public IActionResult ChangeTheme(int vendorId, StoreTheme theme)
        {
            var model = _vendor.ThemeChange(vendorId, theme);
            return Json(model);
        }


    }
}
