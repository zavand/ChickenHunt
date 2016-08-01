using System.Web.Mvc;
using ChickenHunt.Website.Controllers.Templates.Actions.SignUp;

namespace ChickenHunt.Website.Controllers.Templates
{
    public partial class TemplatesController
    {
        public ActionResult SignUp(Route r)
        {
            var m = GetModel<Model, Route, TemplatesController>(r, this);
            PrepareModel(m, r);
            return View(m);
        }

        void PrepareModel(Model m, Route r)
        {
        }

        [HttpPost]
        public ActionResult SignUp(ModelPost post, Route r, FormCollection c)
        {
            var m = GetModel<Model, Route, TemplatesController>(r, this);
            PrepareModel(m, r);
            m.Post = post;

            if (ModelState.IsValid)
            {
                return RedirectToAction<Actions.SignUp.Route>(r);
            }
            return View(m);
        }
    }
}

namespace ChickenHunt.Website.Controllers.Templates.Actions.SignUp
{
    public class Route : TemplatesRoute
    {
        public const string ActionName = "SignUp";

        public Route()
        {
            Action = ActionName;
        }

        public static Route Create(string name, string email)
        {
            return new Route() {Name=name,Email=email};
        }

        public string Email { get; set; }

        public string Name { get; set; }
    }

    public class Model : TemplatesModel<Route, TemplatesController>
    {
        public ModelPost Post { get; set; }

        public Model()
        {
            Post = new ModelPost();
        }
    }

    public class ModelPost
    {

    }
}