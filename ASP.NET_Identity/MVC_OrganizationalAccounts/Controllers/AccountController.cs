using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IdentityModel.Services;
using System.IdentityModel.Services.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using MVC_OrganizationalAccounts.Models;

namespace MVC_OrganizationalAccounts.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult SignOut()
        {
            WsFederationConfiguration config = FederatedAuthentication.FederationConfiguration.WsFederationConfiguration;

            // 在登出後重新導向至 SignOutCallback。
            string callbackUrl = Url.Action("SignOutCallback", "Account", routeValues: null, protocol: Request.Url.Scheme);
            SignOutRequestMessage signoutMessage = new SignOutRequestMessage(new Uri(config.Issuer), callbackUrl);
            signoutMessage.SetParameter("wtrealm", config.Realm);
            FederatedAuthentication.SessionAuthenticationModule.SignOut();

            return new RedirectResult(signoutMessage.WriteQueryString());
        }

        public ActionResult SignOutCallback()
        {
            if (Request.IsAuthenticated)
            {
                // 如果使用者已驗證，則重新導向至首頁。
                return RedirectToAction("Index", "Home");
            }

            return View();
        }
    }
}
