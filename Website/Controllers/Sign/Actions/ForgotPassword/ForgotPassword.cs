using System;
using System.Web.Mvc;
using ChickenHunt.Website.Controllers.Sign.Actions.ForgotPassword;

namespace ChickenHunt.Website.Controllers.Sign
{
    public partial class SignController
    {
        public ActionResult ForgotPassword(Route r)
        {
            var m = GetModel<Model, Route, SignController>(r, this);
            PrepareModel(m, r);
            return View(m);
        }

        private void PrepareModel(Model m, Route r)
        {
        }

        [HttpPost]
        public ActionResult ForgotPassword(ModelPost post, Route r, FormCollection c)
        {
            var m = GetModel<Model, Route, SignController>(r, this);
            PrepareModel(m, r);
            m.Post = post;

            var hunter = _dataStorage.GetHunterByEmail(post.Email);
            if (hunter == null)
            {
                ModelState.AddModelError($"{nameof(Model.Post)}.{nameof(ModelPost.Email)}", "Account not found");
            }
            if (ModelState.IsValid)
            {
                var passwordResetCode = Guid.NewGuid().ToString();
                _dataStorage.UpdateResetPasswordCode(post.Email, passwordResetCode);

                SendEmail(Request, r, Templates.Actions.ForgotPassword.Route.Create(post.Email, passwordResetCode), post.Email);
                return RedirectToAction<Actions.ForgotPasswordConfirm.Route>(r);
            }
            return View(m);
        }
    }
}

namespace ChickenHunt.Website.Controllers.Sign.Actions.ForgotPassword
{
    public class Route : SignRoute
    {
        public const string ActionName = "ForgotPassword";

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
        public Model()
        {
            Post = new ModelPost();
        }

        public ModelPost Post { get; set; }
    }

    public class ModelPost
    {
        public string Email { get; set; }
    }
}