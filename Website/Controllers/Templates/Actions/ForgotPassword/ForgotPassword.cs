using System.Web.Mvc;
using ChickenHunt.Website.Controllers.Templates.Actions.ForgotPassword;
using Zavand.Web.Mvc.Manana.Framework;

namespace ChickenHunt.Website.Controllers.Templates
{
    public partial class TemplatesController
    {
        public ActionResult ForgotPassword(Route r)
        {
            var m = GetModel<Model, Route, TemplatesController>(r, this);
            PrepareModel(m, r);
            return View(m);
        }

        void PrepareModel(Model m, Route r)
        {
            m.Url = $"{Request.Url.Scheme}://{Request.Url.Authority}{Url.RouteUrl(r, Sign.Actions.ResetPassword.Route.Create(r.ResetPasswordCode))}";
        }
    }
}

namespace ChickenHunt.Website.Controllers.Templates.Actions.ForgotPassword
{
    public class Route : TemplatesRoute
    {
        public const string ActionName = "ForgotPassword";

        public Route()
        {
            Action = ActionName;
        }

        public static Route Create(string email, string resetPasswordCode)
        {
            return new Route() {Email=email, ResetPasswordCode = resetPasswordCode };
        }

        public string ResetPasswordCode { get; set; }

        public string Email { get; set; }
    }

    public class Model : TemplatesModel<Route, TemplatesController>
    {
        public ModelPost Post { get; set; }
        public string Url { get; set; }

        public Model()
        {
            Post = new ModelPost();
        }
    }

    public class ModelPost
    {

    }
}