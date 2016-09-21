using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Discuss.Models;
using System.Web.Security;

namespace Discuss.Controllers
{
        #region Untouched code
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public void RenewCurrentUser()
        {
            System.Web.HttpCookie authCookie =
                System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                FormsAuthenticationTicket authTicket = null;
                authTicket = FormsAuthentication.Decrypt(authCookie.Value);

                if (authTicket != null && !authTicket.Expired)
                {
                    FormsAuthenticationTicket newAuthTicket = authTicket;

                    if (FormsAuthentication.SlidingExpiration)
                    {
                        newAuthTicket = FormsAuthentication.RenewTicketIfOld(authTicket);
                    }
                    string userData = newAuthTicket.UserData;
                    string[] roles = userData.Split(',');

                    System.Web.HttpContext.Current.User =
                        new System.Security.Principal.GenericPrincipal(new FormsIdentity(newAuthTicket), roles);
                }
            }
        }
        #endregion
        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
        /// <summary>
        /// It handles log in request. It accepts values from login page and redirects to Home page if LogIn is succesful.
        /// </summary>
        /// <param name="model">consists values of fields on the log in page.</param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            int id = 0;
            string result1 = string.Empty;
            string fullname = string.Empty;
            string role = string.Empty;
            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);

            try
            {
                using (DISCUSSEntities dbcontext = new DISCUSSEntities())
                {
                    var userAccntModel = dbcontext.UserAccounts.Where(t => t.Username == model.Email && t.userpassword == model.Password).FirstOrDefault();
                    if (userAccntModel != null)
                    {
                        id = userAccntModel.id;
                        fullname = dbcontext.DiscussUsers.Where(t => t.id == id).Select(i => i.FirstName + " " + i.LastName).FirstOrDefault();
                        role = dbcontext.DiscussUsers.Where(t => t.id == id).Select(i => i.userrole).FirstOrDefault();
                    }

                }
            }
            catch (Exception ex)
            {
                // Implement a logging system to track what all exceptions occured.(future improvement)
                System.Console.WriteLine(ex.StackTrace);
            }
            switch (result)
            {
                // If the result is success, create new session and add following parameters to the session and also redirect to home page.
                case SignInStatus.Success:
                    Session.Add("id", id);
                    Session.Add("Name", fullname);
                    Session["Username"] = model.Email;
                    Session["AuthenticatedUser"] = true;
                    Session["Role"] = role;
                    return RedirectToAction("HomePage", "Home");
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }
    
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }


        /// <summary>
        /// Handles register new user request, creates new acount for the user and redirect to UserDetails view after successfully registering the user.
        /// </summary>
        /// <param name="model">consists values of fields submitted by the user on register page</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(UserAccount model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Username, Email = model.Username };
                var result = await UserManager.CreateAsync(user, model.userpassword);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    //After creating a new user and sign-in sucessfully, add the user UserAccount table in Database.
                    try
                    {
                        using (DISCUSSEntities dbContext = new DISCUSSEntities())
                        {
                            if (dbContext != null)
                            {
                                dbContext.UserAccounts.Add(model);
                                dbContext.SaveChanges();
                                Session.Add("Username", model.Username);

                            }
                        }
                        //Add id of the new user to the session .
                        using (DISCUSSEntities dbContext1 = new DISCUSSEntities())
                        {
                            var userDetails = dbContext1.UserAccounts.Where(t => t.Username == model.Username).FirstOrDefault();
                            if (userDetails != null)
                            {
                                var id = userDetails.id;
                                Session["id"] = id;
                            }
                        }
                        TempData["Username"] = model.Username;
                        Session["AuthenticatedUser"] = true;
                        return RedirectToAction("UserDetails");
                    }
                    catch (Exception ex)
                    {
                        model.Msg = "User already exists !!";
                    }

                }
                AddErrors(result);

            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }
       
        
        // POST: /Account/LogOff
        /// <summary>
        /// Handles Signout request. It clears the current session and redirects to Login page.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            Session.RemoveAll();
            Session["AuthenticatedUser"] = null;
            Session["Id"] = null;
            Session["Username"] = null;
            Session["Name"] = null;
            Session["Role"] = null;
            Session["GroupId"] = null;
            return RedirectToAction("LogIn", "Account");
        }


        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion

        #region  Self-defined functions
        [HttpGet]
        public ActionResult UserDetails()
        {
            ViewBag.Title = "User Details";
            string username = Convert.ToString(Session["Username"]);
            //TempData["Username"] = string.Empty;
            DiscussUser emptyModelButId = new DiscussUser();
            emptyModelButId.id = GetIdForNewUser(username);
            return View(emptyModelButId);
        }

        [HttpPost]
        public ActionResult UserDetails(DiscussUser model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            try
            {
                if (model != null)
                {
                    using (DISCUSSEntities dbcontext = new DISCUSSEntities())
                    {
                        dbcontext.DiscussUsers.Add(model);
                        dbcontext.SaveChanges();

                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.Write(ex.StackTrace);
            }
            Session["Role"] = model.userrole;
            Session["Name"] = model.FirstName + " " + model.LastName;
            return RedirectToAction("SearchGroups", "Home");
        }

        [HttpGet]
        public ActionResult ChangeUserDetails()
        {
            int id = Convert.ToInt32(Session["id"]);

            DiscussUser model = new DiscussUser();
            try
            {
                using (DISCUSSEntities dbcontext = new DISCUSSEntities())
                {
                    model = dbcontext.DiscussUsers.Where(t => t.id == id).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.StackTrace);
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult ChangeUserDetails(DiscussUser model)
        {
            if (!ModelState.IsValid)
            {
                return View("UserDetails", model);
            }
            try
            {
                if (model != null)
                {
                    using (DISCUSSEntities dbcontext = new DISCUSSEntities())
                    {
                        var model1 = dbcontext.DiscussUsers.Where(t => t.id == model.id).FirstOrDefault();
                        dbcontext.DiscussUsers.Remove(model1);
                        dbcontext.DiscussUsers.Add(model);
                        dbcontext.SaveChanges();

                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.Write(ex.StackTrace);
            }

            return RedirectToAction("HomePage", "Home");
        }
        public int GetIdForNewUser(string username)
        {
            int id = 0;
            using (DISCUSSEntities dbcontext = new DISCUSSEntities())
            {
                id = dbcontext.UserAccounts.Where(i => i.Username == username).FirstOrDefault().id;
            }

            return id;
        }
        #endregion

        #region Unused Code
      
        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }
        
        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
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
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }


        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }
        #endregion

    }
}