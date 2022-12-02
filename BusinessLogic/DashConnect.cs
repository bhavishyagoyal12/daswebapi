using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using das_api.Controllers;
using daslibrary;
using Microsoft.Extensions.Logging;

namespace das_api.BusinessLogic
{
    public class JsonData
    {
        public List<string> data { get; set; }
    }
    public class DashConnect
    {

        private SocketOrderServer socketOrderServer = null;
        private SocketQuoteServer socketQuoteServer = null;
        private int traderId;
        private int waitseconds = 5;
        private string user = "IM0395";
        private string passwd = "WyethPaper321";
        private string orderserver = "4.2.100.36";
        private string quoteserver = "4.2.100.86";
        private int orderport = 7500;
        private int quoteport = 9510;

        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public DashConnect()
        {
          
        }

        private void readJsonAndSubscribeL1()
        {
            try
            {
                using (StreamReader r = new StreamReader("data.json"))
                {
                    string json = r.ReadToEnd();
                    JsonData source = JsonSerializer.Deserialize<JsonData>(json);
                    for(int i=0; i<source.data.Count; i++)
                    {
                        try
                        {
                            int timeout = 0;
                            string quotesymbol = source.data[i];
                            socketQuoteServer.WLSTAddWatch(quotesymbol);
                            while (timeout <= 10000)
                            {
                                timeout += 100;
                                System.Threading.Thread.Sleep(100);
                                if (IssueIfo.mIssueSet.ContainsKey(quotesymbol))
                                {
                                    IssueIfo quote = IssueIfo.mIssueSet[quotesymbol];
                                    Console.WriteLine("price done");
                                    break;
                                }
                            }
                            socketQuoteServer.WLSTRemoveWatch(quotesymbol);
                        }catch(Exception e)
                        {
                            _logger.Info("first error msg: subscribing level 1" + e.Message);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                _logger.Info("error msg: subscribing level 1" + e.Message);
            }
        }

        public String login()
        {
            string msg = "";
            if (socketOrderServer != null || socketQuoteServer != null)
            {
                _logger.Info("socketOrderServer is null or socketQuoteServer is null ");
                return "Already Connected";
            }
            
            //connect  the order server
            socketOrderServer = new SocketOrderServer();
            socketOrderServer.Connect(orderserver, (int)orderport);
            //Subscribe short locate events
            socketOrderServer.eventPool.Subscribe("ShortLocateRoutes", new EventHandler<StringArgs>(LocateRoutesHandler));
            socketOrderServer.eventPool.Subscribe("ShortLocateInquire", new EventHandler<LocateInquireArgs>(LocateInquireHandler));
            socketOrderServer.eventPool.Subscribe("ShortLocateOrder", new EventHandler<LocateOrderArgs>(LocateOrderHandler));

            socketOrderServer.PkgLongin(user, passwd, orderserver);
            int timeout = 0;
            while (timeout <= waitseconds * 1000) // wait waitseconds(default =3) seconds
            {
                timeout += 500;
                System.Threading.Thread.Sleep(500);
                itemTrader itemtrader = socketOrderServer.sitemTrader.FindItemByName(user);


                if (itemtrader != null)
                {
                    socketQuoteServer = new SocketQuoteServer(socketOrderServer);
                    socketQuoteServer.Connect(quoteserver, quoteport);
                    socketQuoteServer.PkgLogin(user, passwd, quoteserver);

                    traderId = itemtrader.mtrid;
                    _logger.Info("Logged Successfully");
                    msg = "Logged successfully.";

                    if (socketOrderServer.sitemAcct_Access.sMapAccid.Count > 0)
                    {
                        foreach (int acoutid in socketOrderServer.sitemAcct_Access.GetAccountIDs(traderId))
                        {
                            _logger.Info("Account IDS "+ acoutid.ToString());
                            msg += "\n Account Ids- "+(acoutid.ToString());

                            // foreach (itemPosition position in socketOrderServer.sitemPosition.Finditems(acoutid))
                            {
                                // position.
                            }

                            break;
                        }
                    }

                    msg += "\n  trader Id- "+(traderId.ToString());

                    foreach (itemAccount ia in socketOrderServer.sitemAccount.sMapAccID.Values)
                    {
                        _logger.Info("\n\r maccid and name- " + ia.mitemifo.maccid + ":" + ia.mitemifo.mname);
                        Console.WriteLine("\n\r" + ia.mitemifo.maccid + ":" + ia.mitemifo.mname);
                    }

                    foreach (itemOrder porder in socketOrderServer.sitemOrder.sordermap.Values)
                    {
                        Console.WriteLine(System.Environment.NewLine + porder.morderid + "," + porder.msecsym + ":" + porder.mqty);
                    }
                    //socketOrderServer.PkgQueryMail(traderId, ComFucs.GetIntDate(DateTime.Now.AddDays(-1)));
                    socketOrderServer.eventPool.Subscribe("HeartBeat", new EventHandler<HeartBeatArgs>(HeartBeatHandler));
                    socketQuoteServer.eventPool.Subscribe("HeartBeat", new EventHandler<HeartBeatArgs>(HeartBeatHandler));


                    //Locate routes combobox initialization
                    if (socketOrderServer.LocateRoutes.Count > 0)
                    {
                        Dictionary<string, byte> comboSource = new Dictionary<string, byte>();
                        foreach (KeyValuePair<string, byte> keyvalue in socketOrderServer.LocateRoutes)
                        {
                            Console.WriteLine(keyvalue.Key + " - " + keyvalue.Value);
                        }


                    }

                    break;


                }
            }

            if (timeout > waitseconds * 1000)
            {
                _logger.Info("Problem in User ID or password.");
                msg = "Problem in User ID or password.";
            }

            readJsonAndSubscribeL1();
            return msg;

        }
            private void HeartBeatHandler(object sender, HeartBeatArgs aea)
            { 
            }
            public void LocateRoutesHandler(object sender, StringArgs aea)
            {
            }
            public void LocateInquireHandler(object sender, LocateInquireArgs aea)
            { 
            }
            public void LocateOrderHandler(object sender, LocateOrderArgs aea)
            {
            }


        public String placeTrade(order o)
        {
            string msg = "";

            string bid = "", ask = "", high = "", low = "", last = "";
            byte exc = 0;

            int timeout = 0;

            string quotesymbol = o.symbol.Trim().ToUpper();

            for(int i=0; i<3; i++)
            {
                Boolean priceDone = false;
                socketQuoteServer.WLSTAddWatch(quotesymbol);
                while (timeout <= 10000)
                {
                    timeout += 100;
                    System.Threading.Thread.Sleep(100);
                    if (IssueIfo.mIssueSet.ContainsKey(quotesymbol))
                    {
                        IssueIfo quote = IssueIfo.mIssueSet[quotesymbol];
                        bid = quote.l1_BidPrice.ToString();
                        ask = quote.l1_AskPrice.ToString();
                        high = quote.l1_todayhigh.ToString();
                        low = quote.l1_todaylow.ToString();
                        last = quote.l1_lastPrice.ToString();
                        exc = quote.PrimExch;
                        priceDone = true;
                        Console.WriteLine("price done");
                        break;
                    }
                }
                socketQuoteServer.WLSTRemoveWatch(quotesymbol);
                if (priceDone)
                {                    
                    break;
                }
                else
                {
                    Console.WriteLine("price not done");
                }
                System.Threading.Thread.Sleep(1000);
            }
                        
            
            _logger.Info("Level1 subscribe Done bid: " + bid + " ask: " + ask + " high: " + high + " low: " + low + " last: " + last);

            itemOrder newOrder = new itemOrder();

            newOrder.mtrid = traderId;
            newOrder.msecsym = o.symbol.Trim().ToUpper();
            newOrder.mstatus = 0;
            newOrder.morderid = 0;

            newOrder.mexchange = exc;

            switch (o.side.Trim().ToUpper())
            {
                case "BUY":
                    newOrder.mstatus |= 1 << 6;
                    break;
                case "SELL":
                    break;
            }


            try
            {
                itemAccount itemA = socketOrderServer.sitemAccount.FindItem(Convert.ToInt32(o.account));
                newOrder.maccid = itemA.mitemifo.maccid;
                newOrder.mbrid = itemA.mitemifo.mbrid;
                newOrder.mrrno = itemA.mitemifo.mrrno;

                if (o.isMarket)
                {
                    newOrder.mstatus |= 1 << 9;
                    newOrder.maskprice = Convert.ToDouble(ask);
                    newOrder.mbidprice = Convert.ToDouble(bid);
                    if (o.side.Trim().ToUpper() == "BUY")
                    {
                        newOrder.mprice = Convert.ToDouble(ask);
                    }
                    if (o.side.Trim().ToUpper() == "SELL")
                    {
                        newOrder.mprice = Convert.ToDouble(bid);
                    }

                }
                else
                {
                    newOrder.mprice = Convert.ToDouble(o.price);
                }



                newOrder.mtmforce = 65535; //Day Order

                newOrder.mroute = o.route.Trim();
                newOrder.mqty = Convert.ToInt32(o.quantity);

                string errMsg = "";
                long morig = -1;

                if (itemOrder.LSendOrder(newOrder, ref errMsg, true, socketOrderServer, ref morig) == 0)
                {
                    timeout = 0;

                    while (timeout <= 5000)
                    {
                        timeout += 1000;
                        System.Threading.Thread.Sleep(1000);

                        itemOrder myorder = socketOrderServer.sitemOrder.FindItemBymorig((int)morig);
                        if (myorder != null)
                        {
                            msg = myorder.morderid.ToString();
                            // if (!(((myorder.mstatus & (1 << 4)) != 0) || ((myorder.mstatus & (1 << 1)) != 0))) continue;


                            if ((myorder.mstatus & (1 << 1)) != 0)
                                msg += " Order Executed";
                            else if ((myorder.mstatus & (1 << 2)) != 0)
                                msg += " Order Canceled";
                            else if ((myorder.mstatus & (1 << 4)) != 0)
                                msg += " Order Accepted";

                            if (msg.Trim().Length == 0) continue;

                            return msg;
                        }
                    }

                    if (timeout > 5000)
                    {
                        if (socketOrderServer.errormsg.Length > 0)
                        {
                            _logger.Info("error msg " + socketOrderServer.errormsg);
                            msg = socketOrderServer.errormsg;
                        }
                        else
                        {
                            _logger.Info("error msg: TimeOut ");
                            msg = "Time Out !";
                        }
                    }

                }
                else
                {
                    _logger.Info("error msg:  "+errMsg);
                    msg = "Error:" + errMsg;


                }



            }
            catch (Exception ex)
            {
                _logger.Info("error msg: order send failed "+ex.Message);
                msg = "Send Order fail.";

            }

            return msg;
        }

    }
}
