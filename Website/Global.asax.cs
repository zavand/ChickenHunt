using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using ChickenHunt.Website.Common;
using ChickenHunt.Website.DataLayer;
using log4net;
using Zavand.Web.Mvc.Manana.Framework;

namespace ChickenHunt.Website
{
    public class MvcApplication : BaseApplication
    {
        public MvcApplication()
        {
            IsLocalizationSupported = false;
        }
        protected void Application_Start()
        {
            #region Autofac

            var builder = new ContainerBuilder();

            builder.RegisterModule<LoggingModule>();
            builder.Register(c =>
            {
                var ds = new DataStorage(WebConfigurationManager.AppSettings["ChickenHuntConnectionString"], "chicken_hunt");
                return ds;
            }).As<IDataStorage>();

            // Register your MVC controllers.
            builder.RegisterControllers(typeof(MvcApplication).Assembly);
            var container = builder.Build();

            container.Resolve<IDataStorage>().Init();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            #endregion

            var log = container.Resolve<ILog>();
            log.Info("Starting...");

            BundleConfig.RegisterBundles(BundleTable.Bundles);
#if !DEBUG
            BundleTable.EnableOptimizations = true;
#endif

            ApplicationStart();

            log.Info("Started.");
        }

        public override void RegisterRoutes(RouteCollection routes)
        {
            base.RegisterRoutes(routes);

            routes.MapRoute<Website.Controllers.Home.Actions.Index.Route>(IsLocalizationSupported);
            routes.MapRoute<Website.Controllers.Home.Actions.Chicken.Route>(IsLocalizationSupported);
            routes.MapRoute<Website.Controllers.Home.Actions.Chicken2.Route>(IsLocalizationSupported);

            routes.MapRoute<Website.Controllers.Sign.Actions.In.Route>(IsLocalizationSupported);
            routes.MapRoute<Website.Controllers.Sign.Actions.Up.Route>(IsLocalizationSupported);
            routes.MapRoute<Website.Controllers.Sign.Actions.Out.Route>(IsLocalizationSupported);
            routes.MapRoute<Website.Controllers.Sign.Actions.ForgotPassword.Route>(IsLocalizationSupported);
            routes.MapRoute<Website.Controllers.Sign.Actions.ForgotPasswordConfirm.Route>(IsLocalizationSupported);
            routes.MapRoute<Website.Controllers.Sign.Actions.ResetPassword.Route>(IsLocalizationSupported);

            routes.MapRoute<Website.Controllers.Templates.Actions.SignUp.Route>(IsLocalizationSupported);
            routes.MapRoute<Website.Controllers.Templates.Actions.ForgotPassword.Route>(IsLocalizationSupported);

            routes.MapRoute<Website.Controllers.Hunter.Actions.Index.Route>(IsLocalizationSupported);
        }
    }
}
