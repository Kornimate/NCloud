﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NCloud.Services;
using NCloud.Users;
using NCloud.ViewModels;

namespace NCloud.Controllers
{
    public class DriveController : Controller
    {
        private readonly ICloudService service;
        private readonly UserManager<CloudUser> userManager;

        public DriveController(ICloudService service,UserManager<CloudUser> userManager)
        {
            this.service = service;
            this.userManager = userManager;
        }

        // GET: DriveController
        public ActionResult Index()
        {
            var result = service.GetCurrentUserIndexData();
            return View(new DriveIndexViewModel(result.Item1,result.Item2));
        }

        // GET: DriveController/Details/5
        public async Task<ActionResult> Details(int parentId)
        {
            CloudUser user = await userManager.GetUserAsync(HttpContext.User);
            return View(new DriveDetailsViewModel(service.GetCurrentDeptData(parentId,user)));
        }

        // GET: DriveController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: DriveController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: DriveController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: DriveController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: DriveController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: DriveController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}