using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using shopapp.business.Abstact;

namespace shopapp.webui.ViewComponents
{
    public class CategoriesViewComponent: ViewComponent
    {
        ICategoryService _categoryService;
        public CategoriesViewComponent(ICategoryService categoryService)
        {
            this._categoryService = categoryService;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            if(RouteData.Values["category"] != null)  // action
            {
                ViewBag.SelectedCategory = RouteData?.Values["category"];
            }

            return View(await _categoryService.GetAll());

        }
    }
}