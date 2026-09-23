using System;
using System.Web.Mvc;
using Alkami.Client.Framework.Mvc;
using Common.Logging;
using Alkami.Security.Common.Claims;
using Alkami.Common;

namespace TEAM3.Client.Widget.YourMonth.Controllers
{
    [ClaimsAuthorizationFilter(PermissionNames.NoPermissions)]
    public class MobileTEAM3YourMonthController : BaseController
    {
        /// <summary>
        /// Gets logger
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger<MobileTEAM3YourMonthController>();

        /// <summary>
        /// Standard widget entry route
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            try
            {
                Logger.DebugFormat("[GET] Controller/Index");
                return View("Index");
            }
            catch (Exception e)
            {
                Logger.Error("Error [GET] Controller/Index", e);
                return View("Error");
            }
        }
    }
}