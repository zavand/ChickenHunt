using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using ChickenHunt.Website.Controllers.Sign.Actions.ResetPassword;
using ChickenHunt.Website.DataLayer;

namespace ChickenHunt.Website.Controllers.Sign
{
    public partial class SignController
    {
        public ActionResult ResetPassword(Route r)
        {
            var m = GetModel<Model, Route, SignController>(r, this);
            PrepareModel(m, r);
            return View(m);
        }

        void PrepareModel(Model m, Route r)
        {
            m.Hunter = _dataStorage.GetHunterByResetPasswordCode(r.ResetPasswordCode);
        }

        [HttpPost]
        public ActionResult ResetPassword(ModelPost post, Route r, FormCollection c)
        {
            var m = GetModel<Model, Route, SignController>(r, this);
            PrepareModel(m, r);
            m.Post = post;

            if (post.Password != post.PasswordRetype)
            {
                ModelState.AddModelError(nameof(ModelPost.Password), "Passwords don't match");
            }
            if (ModelState.IsValid)
            {
                _dataStorage.UpdatePassword(m.Hunter.ID, post.Password);
                
                // Change reset code so previous one can not be used.
                _dataStorage.UpdateResetPasswordCode(m.Hunter.Email,Guid.NewGuid().ToString());

                return RedirectToAction<Actions.In.Route>(r);
            }
            return View(m);
        }
    }
}

namespace ChickenHunt.Website.Controllers.Sign.Actions.ResetPassword
{
    public class Route : SignRoute
    {
        public const string ActionName = "ResetPassword";

        public Route()
        {
            Action = ActionName;
        }

        public static Route Create(string resetPasswordCode)
        {
            return new Route() { ResetPasswordCode = resetPasswordCode};
        }

        public string ResetPasswordCode { get; set; }

        public override string GetUrl()
        {
            return base.GetUrl() + "/{ResetPasswordCode}";
        }
    }

    public class Model : SignModel<Route, SignController>
    {
        public ModelPost Post { get; set; }
        public ChickenCandidate Hunter { get; set; }

        public Model()
        {
            Post = new ModelPost();
        }
    }

    public class ModelPost
    {
        [Required]
        public string Password { get; set; }
        [Required]
        public string PasswordRetype { get; set; }
    }
}