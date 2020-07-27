using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using CoffeeApp.Helpers;
using CoffeeApp.Models;
using Microsoft.AspNet.SignalR;

namespace CoffeeApp.Hubs
{
    public class CoffeeHub : Hub<ICoffeeClient>
    {
        private static readonly OrderChecker _orderChecker = new OrderChecker(new Random());
        // This method gets called by the client right after a new order has been placed
        public async Task GetUpdateForOrder(Order order)
        {
            // I first allowed other clients to know there is a new order, we use Clients property of the Hub base class
            // which gives access to the Clients object with this I can call function on clients
            await Clients.Others.NewOrder(order); 
            UpdateInfo result;
            do
            {
                result = _orderChecker.GetUpdate(order);
                await Task.Delay(700);
                if(!result.New) continue;

                await Clients.Caller.ReceiveOrderUpdate(result);
            } while (!result.Finished); 

            await Clients.Caller.Finished(order);

        }

        public override Task OnConnected()
        {
            if (Context.QueryString["group"] == "allUpdates")
                Groups.Add(Context.ConnectionId, "allUpdateReceivers");
            return base.OnConnected();
        }
    }
}