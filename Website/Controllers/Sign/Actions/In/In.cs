using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Web.Security;
using ChickenHunt.Website.Controllers.Sign.Actions.In;
using ChickenHunt.Website.DataLayer;

namespace ChickenHunt.Website.Controllers.Sign
{
    public partial class SignController
    {
        public ActionResult In(Route r)
        {
            var m = GetModel<Model, Route, SignController>(r, this);
            PrepareModel(m, r);
            return View(m);
        }

        void PrepareModel(Model m, Route r)
        {
        }

        [HttpPost]
        public ActionResult In(ModelPost post, Route r, FormCollection c)
        {
            var m = GetModel<Model, Route, SignController>(r, this);
            PrepareModel(m, r);
            m.Post = post;

            if (ModelState.IsValid)
            {
                try
                {
                    string token = _dataStorage.GetToken(post.Email, post.Password);
                    if (string.IsNullOrEmpty(token))
                    {
                        token = _dataStorage.GenerateToken(post.Email, post.Password);
                    }
                    var hunter = _dataStorage.GetHunterByToken(token);

                    var cc = FormsAuthentication.GetAuthCookie(hunter.Name, true);
                    var t1 = FormsAuthentication.Decrypt(cc.Value);
                    // Add token
                    var t = new FormsAuthenticationTicket(t1.Version, hunter.Name, t1.IssueDate, t1.Expiration, t1.IsPersistent, token, t1.CookiePath);
                    cc.Value = FormsAuthentication.Encrypt(t);
                    Response.AppendCookie(cc);

                    if (!string.IsNullOrEmpty(r.ReturnUrl))
                    {
                        return Redirect(r.ReturnUrl);
                    }
                    return RedirectToAction<Home.Actions.Index.Route>(r);
                }
                catch (InvalidCredentialsException)
                {
                    ModelState.AddModelError("","Invalid credentials");
                }

            }
            return View(m);
        }
    }
}

namespace ChickenHunt.Website.Controllers.Sign.Actions.In
{
    public class Route : SignRoute
    {
        public const string ActionName = "In";

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
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}