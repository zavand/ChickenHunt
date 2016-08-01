using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Filters;
using System.Web.Security;
using ChickenHunt.Website.Controllers.Templates;
using ChickenHunt.Website.DataLayer;
using log4net;
using Zavand.Web.Mvc.Manana.Framework;

namespace ChickenHunt.Website.Common
{
    public class BaseWebsiteController : BaseController
    {
        protected readonly ILog Log;
        protected readonly IDataStorage _dataStorage;

        public BaseWebsiteController(ILog log, IDataStorage dataStorage)
        {
            Log = log;
            _dataStorage = dataStorage;
        }

        protected override void OnAuthentication(AuthenticationContext filterContext)
        {
            base.OnAuthentication(filterContext);

            FormsAuthenticationTicket ticket = null;
            var c = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (c != null)
            {
                ticket = FormsAuthentication.Decrypt(c.Value);
            }
            try
            {
                if (ticket != null && (CurrentHunter = _dataStorage.GetHunterByToken(ticket.UserData)) != null)
                {
                    System.Web.HttpContext.Current.User = new GenericPrincipal(new GenericIdentity(ticket.Name), null);
                }
                else
                {
                    System.Web.HttpContext.Current.User = new GenericPrincipal(new GenericIdentity(""), null);
                }
            }
            catch (Exception ex)
            {
                System.Web.HttpContext.Current.User = new GenericPrincipal(new GenericIdentity(""), null);
                Log.Error("Auth cookie decryption", ex);
            }
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            base.OnException(filterContext);
            Log.Error("Unhandled exception", filterContext.Exception);
        }


        public void SendEmail(HttpRequestBase currentRequest, BaseWebsiteRoute currentRoute, TemplatesRoute route, string toEmail)
        {
            var url = $"{currentRequest.Url.Scheme}://{currentRequest.Url.Authority}{Url.RouteUrl(currentRoute, route)}"; // TODO: check it

            var request = (HttpWebRequest) WebRequest.Create(url);
            var response = request.GetResponse();
            var msg = new MailMessage();
            using (var stream = response.GetResponseStream())
            {
                using (var sr = new StreamReader(stream))
                {
                    var s = sr.ReadToEnd();

                    s = s.Trim();
                    const string markerStart = "<title>";
                    const string markerEnd = "</title>";
                    var i1 = s.IndexOf(markerStart);
                    var i2 = s.IndexOf(markerEnd);
                    msg.Subject = s.Substring(i1 + markerStart.Length, i2 - i1 - markerStart.Length).Trim();
                    msg.Body = s;
                    msg.IsBodyHtml = true;
                }
            }

            try
            {
                Log.Info($"Sending email to '{toEmail}'...");
                var smtp = new SmtpClient();
                msg.To.Add(toEmail);
                smtp.Send(msg);
                Log.Info($"Sending email...done.");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to send email to '{toEmail}'.", ex);
            }
        }

        public ChickenCandidate CurrentHunter { get; set; }
    }

    public class BaseWebsiteRoute : BaseRoute
    {
        public static int DefaultPageSize = 25;

        public BaseWebsiteRoute()
        {
            DefaultActionName = "Index";
            DefaultControllerName = "Home";
        }

        public string ReturnUrl { get; set; }
    }

    public class BaseWebsiteModel<TRoute, TController> : BaseModel<TRoute, TController>, ILayoutModel<TRoute>
        where TRoute : BaseWebsiteRoute
        where TController : BaseWebsiteController
    {
        public override void SetupModel(TController controller, TRoute route)
        {
            base.SetupModel(controller, route);

            var rp = route as IPageableOptional;
            var mp = this as IPageableModel;
            if (rp != null && mp != null)
            {
                mp.Page = rp.Page ?? 1;
                mp.PageSize = rp.PageSize ?? BaseWebsiteRoute.DefaultPageSize;
            }

            CurrentHunter = controller.CurrentHunter;
        }

        public ChickenCandidate CurrentHunter { get; set; }
    }

    public interface ILayoutModel<out TRoute>
        where TRoute : BaseWebsiteRoute
    {
        TRoute GetRoute();
    }
}