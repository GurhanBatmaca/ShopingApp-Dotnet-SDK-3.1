using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using shopapp.business.Abstact;
using shopapp.webui.EmailServices;
using shopapp.webui.Extentions;
using shopapp.webui.Identity;
using shopapp.webui.Models;

namespace shopapp.webui.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class AccountController: Controller
    {
        private UserManager<User> _userManager;
        private SignInManager<User> _singInManager;
        private IEmailSender _emailSender;
        private ICartService _cartService;

        public AccountController(UserManager<User> userManager,SignInManager<User> singInManager,IEmailSender emailSender,ICartService cartService)
        {
            this._userManager = userManager;
            this._singInManager = singInManager;
            this._emailSender = emailSender;
            this._cartService = cartService;
        }
        public IActionResult Login(string ReturnUrl=null)
        {
            return View(new LoginModel()
            {
                ReturnUrl = ReturnUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {

            if(!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if(user == null)
            {
                ModelState.AddModelError("","Bu kullanıcı adı ile daha önce hesap oluşturulmamış.");
                return View(model);
            }

            if(!await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError("","Lütfen email hesabınıza gelen link ile üyeliğinizi onaylayın.");
                return View(model);
            }

            var result = await _singInManager.PasswordSignInAsync(user,model.Password,true,false);

            if(result.Succeeded)
            {
                return Redirect(model.ReturnUrl??"~/");
            }

            ModelState.AddModelError("","Girilen kullancı adı veya parola yanlış.");

            return View(model);
        }
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new User()
            {
                FirsName = model.FirsName,
                LastName = model.LastName,
                UserName = model.UserName,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user,model.Password);

            if(result.Succeeded)
            {
                // generate token
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var url = Url.Action("ConfirmEmail","Account",new {
                    userId = user.Id,
                    token = code
                });

                // email
                await _emailSender.SendEmailAsync(model.Email,"Hesabınızı onaylayınız.",$"Lütfen mail hesabınızı onaylamak için linke <a href='https://localhost:5001{url}'>tıklayınız.</a>");

                return RedirectToAction("Login","Account");
            }

            ModelState.AddModelError("","Bilinmeyen bir hata oldu lütfen tekrar deneyiniz.");

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _singInManager.SignOutAsync();
            
            TempData.Put("message",new AlertMessage
            {
                Title = "Oturum Kapatıldı.",
                Message = "Hesabınızdan güvenli bir şekilde çıkış yapıldı.",
                AlertType = "warning"
            });
            
            return Redirect("~/");
        }

        public async Task<IActionResult> ConfirmEmail(string userId,string token)
        {       

            if(userId == null || token == null)    
            {
                TempData.Put("message",new AlertMessage
                {
                    Title = "Geçersiz token.",
                    Message = "Geçersiz token.",
                    AlertType = "danger"
                });
                 
                return View();
            }

            var user = await _userManager.FindByIdAsync(userId);

            if(user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user,token);
                if(result.Succeeded)
                {
                    _cartService.InitializeCart(userId);
                    
                    TempData.Put("message",new AlertMessage
                    {
                        Title = "Hesabınız onaylandı.",
                        Message = "Hesabınız onaylandı.",
                        AlertType = "success"
                    });
                    
                    return View();
                }
                
            }

            TempData.Put("message",new AlertMessage
            {
                Title = "Hesabınız onaylanmadı.",
                Message = "Hesabınız onaylanmadı.",
                AlertType = "warning"
            });
           
            return View();
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }      

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string Email)
        {
            if(string.IsNullOrEmpty(Email))
            {
                return View();
            }

            var user = await _userManager.FindByEmailAsync(Email);

            if(user == null)
            {
                return View();
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);           
            var url = Url.Action("ResetPassword","Account",new {
                userId = user.Id,
                token = code
            });

            // email
            await _emailSender.SendEmailAsync(Email,"Reset Password.",$"Lütfen parolanızı yenilemek için linke <a href='https://localhost:5001{url}'>tıklayınız.</a>");

            return View();
        }

        public IActionResult ResetPassword(string userId,string token)
        {
            if(userId == null || token == null)
            {
                return RedirectToAction("Home","Index");
            }

            var model = new ResetPasswordModel {Token = token};

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {

            if(!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if(user == null)
            {
                return RedirectToAction("Home","Index");
            }

            var result = await _userManager.ResetPasswordAsync(user,model.Token,model.Password);

            if(result.Succeeded)
            {
                return RedirectToAction("Login","Account");
            }

            return View(model);
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}