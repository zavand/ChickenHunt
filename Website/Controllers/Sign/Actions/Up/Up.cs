using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;
using System.Web.Security;
using ChickenHunt.Website.Controllers.Sign.Actions.Up;
using Zavand.Web.Mvc.Manana.Framework;

namespace ChickenHunt.Website.Controllers.Sign
{
    public partial class SignController
    {
        public ActionResult Up(Route r)
        {
            var m = GetModel<Model, Route, SignController>(r, this);
            PrepareModel(m, r);
            return View(m);
        }

        void PrepareModel(Model m, Route r)
        {
        }

        [HttpPost]
        public ActionResult Up(ModelPost post, Route r, FormCollection c)
        {
            var m = GetModel<Model, Route, SignController>(r, this);
            PrepareModel(m, r);
            m.Post = post;

            if (!_dataStorage.IsNameAvailable(post.Name))
            {
                ModelState.AddModelError(nameof(ModelPost.Name), $"Name '{post.Name}' already taken. Please choose another name.");
            }
            if (post.Email != null && !_dataStorage.IsEmailAvailable(post.Email))
            {
                ModelState.AddModelError(nameof(ModelPost.Email), $"User with email '{post.Email}' already registered.");
            }
            if (post.Email!=null && !post.Email.EndsWith("@qpaynet.com"))
            {
                ModelState.AddModelError(nameof(ModelPost.Email), "Email must end with @qpaynet.com");
            }
            if (post.Password != post.PasswordRetype)
            {
                ModelState.AddModelError(nameof(ModelPost.Password), "Passwords don't match");
            }
            if (ModelState.IsValid)
            {
                _dataStorage.InsertChickenCandidate(post.Name, post.Email, DateTime.Now, post.Password);

                string token = _dataStorage.GenerateToken(post.Email, post.Password);
                var hunter = _dataStorage.GetHunterByToken(token);

                var cc = FormsAuthentication.GetAuthCookie(hunter.Name, true);
                var t1 = FormsAuthentication.Decrypt(cc.Value);
                // Add token
                var t = new FormsAuthenticationTicket(t1.Version, hunter.Name, t1.IssueDate, t1.Expiration, t1.IsPersistent, token, t1.CookiePath);
                cc.Value = FormsAuthentication.Encrypt(t);
                Response.AppendCookie(cc);

                SendEmail(Request, r, ChickenHunt.Website.Controllers.Templates.Actions.SignUp.Route.Create(post.Name,post.Email), post.Email);
                
                if (!string.IsNullOrEmpty(r.ReturnUrl))
                {
                    return Redirect(r.ReturnUrl);
                }
                return RedirectToAction<Home.Actions.Index.Route>(r);
            }
            return View(m);
        }
        public static string RenderPartialViewToString(Controller controller, string viewName, object model)
        {
            controller.ViewData.Model = model;
            using (StringWriter sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
                ViewContext viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.ToString();
            }
        }

    }
}

namespace ChickenHunt.Website.Controllers.Sign.Actions.Up
{
    public class Route : SignRoute
    {
        public const string ActionName = "Up";

        public Route()
        {
            Action = ActionName;
        }

        public static Route Create()
        {
            return new Route();
        }
    }

    public class Model : SignModel<Route, SignController>
    {
        public ModelPost Post { get; set; }

        public Model()
        {
            Post = new ModelPost();
        }
    }

    public class ModelPost
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string PasswordRetype { get; set; }
    }
}