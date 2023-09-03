using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using shopapp.business.Abstact;
using shopapp.data.Abstract;
using shopapp.entity;
using shopapp.webui.Models;

namespace shopapp.webui.Controllers
{
    public class HomeController: Controller
    {
        private IProductService _productService;
        public HomeController(IProductService productService)
        {
            this._productService=productService;
        }
        public IActionResult Index()
        {

            var productViewModel = new ProductListViewModel()
            {
                Products = _productService.GetHomePageProducts()
            };

            return View(productViewModel);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View("MyView");
        }
    }
}