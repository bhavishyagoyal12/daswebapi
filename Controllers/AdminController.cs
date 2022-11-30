using das_api.BusinessLogic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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

    [ApiController]
    [Route("[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ILogger<AdminController> _logger;
        public DashConnect dashConnect = null;
        public AdminController(ILogger<AdminController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("Login")]
        public String Login()
        {
            if (dashConnect == null)
            {
                dashConnect = new DashConnect(); 
            }
            else
            {
                return "Already Connected";
            }
            String msg = dashConnect.login();
            return msg;
        }

        [HttpPost]
        [Route("PlaceOrder")]
        public String PlaceOrder([FromBody] order o)
        {
            if (dashConnect == null)
            {
                dashConnect = new DashConnect();
            }
            else
            {
                return "Already Connected";
            }
            String msg = dashConnect.login();
            if (dashConnect == null)
            {
                return "User LogOut";
            }
            msg = dashConnect.placeTrade(o);
            return msg;
        }
        
    }
}
