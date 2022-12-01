using das_api.BusinessLogic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace das_api.Controllers
{

    public class order
    {
        public int account { get; set; }
        public string symbol { get; set; }
        public double price { get; set; }
        public int quantity { get; set; }
        public string route { get; set; }
        public string side { get; set; }
        public Boolean isMarket { get; set; }
    }
    public class Response
    {
        public string data { get; set; }
    }
    [ApiController]
    [Route("[controller]")]
    public class AdminController : ControllerBase
    {
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public DashConnect dashConnect = null;
        protected readonly ILogger<AdminController> _log;
        public AdminController(ILogger<AdminController> log)
        {
            _log = log;
        }

        [HttpGet]
        [Route("Login")]
        [Produces("application/json")]
        public IActionResult Login()
        {
            _log.LogInformation("sss");
            ObjectFactory.start();
            return new OkObjectResult(new Response { data = "Logged In" });
        }

        [HttpPost]
        [Route("PlaceOrder")]
        [Produces("application/json")]
        public IActionResult PlaceOrder([FromBody] order o)
        {
            _logger.Info("data request "+o.ToString());
            if (ObjectFactory.dashConnect == null)
            {
                _logger.Info("User Currently LogOut");
                return new OkObjectResult(new Response { data = "User LogOut" });
            }
            string msg = ObjectFactory.dashConnect.placeTrade(o);
            _logger.Info("PlaceOrder Response " + msg);
            return new OkObjectResult(new Response { data = msg });
        }
        
    }
}
