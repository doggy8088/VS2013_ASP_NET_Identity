using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using MVC_IndividualUserAccounts.Models;

namespace MVC_IndividualUserAccounts.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        public AccountController() 
        {
            IdentityManager = new AuthenticationIdentityManager(new IdentityStore());
        }

        public AccountController(AuthenticationIdentityManager manager)
        {
            IdentityManager = manager;
        }

        public AuthenticationIdentityManager IdentityManager { get; private set; }

        private Microsoft.Owin.Security.IAuthenticationManager AuthenticationManager {
            get {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                // Validate the password
                IdentityResult result = await IdentityManager.Authentication.CheckPasswordAndSignInAsync(AuthenticationManager, model.UserName, model.Password, model.RememberMe);
                if (result.Success)
                {
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    AddErrors(result);
                }
            }

            // 如果執行到這裡，發生某項失敗，則重新顯示表單
            return View(model);
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Create a local login before signing in the user
                var user = new User(model.UserName);
                var result = await IdentityManager.Users.CreateLocalUserAsync(user, model.Password);
                if (result.Success)
                {
                    await IdentityManager.Authentication.SignInAsync(AuthenticationManager, user.Id, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    AddErrors(result);
                }
            }

            // 如果執行到這裡，發生某項失敗，則重新顯示表單
            return View(model);
        }

        //
        // POST: /Account/Disassociate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Disassociate(string loginProvider, string providerKey)
        {
            string message = null;
            IdentityResult result = await IdentityManager.Logins.RemoveLoginAsync(User.Identity.GetUserId(), loginProvider, providerKey);
            if (result.Success)
            {
                message = "已移除外部登入。";
            }
            else
            {
                message = result.Errors.FirstOrDefault();
            }

            return RedirectToAction("Manage", new { Message = message });
        }

        //
        // GET: /Account/Manage
        public async Task<ActionResult> Manage(string message)
        {
            ViewBag.StatusMessage = message ?? "";
            ViewBag.HasLocalPassword = await IdentityManager.Logins.HasLocalLoginAsync(User.Identity.GetUserId());
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Manage(ManageUserViewModel model)
        {
            string userId = User.Identity.GetUserId();
            bool hasLocalLogin = await IdentityManager.Logins.HasLocalLoginAsync(userId);
            ViewBag.HasLocalPassword = hasLocalLogin;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasLocalLogin)
            {               
                if (ModelState.IsValid)
                {
                    IdentityResult result = await IdentityManager.Passwords.ChangePasswordAsync(User.Identity.GetUserName(), model.OldPassword, model.NewPassword);
                    if (result.Success)
                    {
                        return RedirectToAction("Manage", new { Message = "您的密碼已變更。" });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }
            else
            {
                // 使用者沒有本機密碼，因此，請移除遺漏 OldPassword 欄位所導致的任何驗證錯誤
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    // Create the local login info and link it to the user
                    IdentityResult result = await IdentityManager.Logins.AddLocalLoginAsync(userId, User.Identity.GetUserName(), model.NewPassword);
                    if (result.Success)
                    {
                        return RedirectToAction("Manage", new { Message = "已設定您的密碼。" });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }

            // 如果執行到這裡，發生某項失敗，則重新顯示表單
            return View(model);
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // 要求重新導向至外部登入提供者
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { loginProvider = provider, ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string loginProvider, string returnUrl)
        {
            ClaimsIdentity id = await IdentityManager.Authentication.GetExternalIdentityAsync(AuthenticationManager);
            // 如果此外部識別已連結，則加以登入
            IdentityResult result = await IdentityManager.Authentication.SignInExternalIdentityAsync(AuthenticationManager, id);
            if (result.Success) 
            {
                return RedirectToLocal(returnUrl);
            }
            else if (User.Identity.IsAuthenticated)
            {
                // 如果使用者已登入，則嘗試連結
                result = await IdentityManager.Authentication.LinkExternalIdentityAsync(id, User.Identity.GetUserId());
                if (result.Success)
                {
                    return RedirectToLocal(returnUrl);
                }
                else 
                {
                    return View("ExternalLoginFailure");
                }
            }
            else
            {
                // 否則會出現建立本機使用者的提示
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.LoginProvider = loginProvider;
                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { UserName = id.Name });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Manage");
            }
            
            if (ModelState.IsValid)
            {
                // 從外部登入提供者處取得使用者資訊
                IdentityResult result = await IdentityManager.Authentication.CreateAndSignInExternalUserAsync(AuthenticationManager, new User(model.UserName));
                if (result.Success)
                {
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    AddErrors(result);
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ExternalLoginsList(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return (ActionResult)PartialView("_ExternalLoginsListPartial", new List<AuthenticationDescription>(AuthenticationManager.GetExternalAuthenticationTypes()));
        }

        [ChildActionOnly]
        public ActionResult RemoveAccountList()
        {
            return Task.Run(async () =>
            {
                var linkedAccounts = new List<IUserLogin>(await IdentityManager.Logins.GetLoginsAsync(User.Identity.GetUserId()));
                ViewBag.ShowRemoveButton = linkedAccounts.Count > 1;
                return (ActionResult)PartialView("_RemoveAccountPartial", linkedAccounts);
            }).Result;
        }

        protected override void Dispose(bool disposing) {
            if (disposing && IdentityManager != null) {
                IdentityManager.Dispose();
                IdentityManager = null;
            }
            base.Dispose(disposing);
        }

        #region Helper
        private void AddErrors(IdentityResult result) {
            foreach (var error in result.Errors) {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUrl)
            {
                LoginProvider = provider;
                RedirectUrl = redirectUrl;
            }

            public string LoginProvider { get; set; }
            public string RedirectUrl { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                context.HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties() { RedirectUrl = RedirectUrl }, LoginProvider);
            }
        }
        #endregion
    }
}