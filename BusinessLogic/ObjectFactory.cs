using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace das_api.BusinessLogic
{
    public class ObjectFactory
    {

        public static DashConnect dashConnect = null;

        public static void start()
        {
            if(dashConnect == null)
            {
                dashConnect = new DashConnect();
                dashConnect.login();
            }
        }
    }
}
