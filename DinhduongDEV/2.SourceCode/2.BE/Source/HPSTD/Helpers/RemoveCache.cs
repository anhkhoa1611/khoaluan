using DevTrends.MvcDonutCaching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HPSTD.Helpers
{
    public class RemoveCache
    {
        public static void CacheOrders()
        {
            var cacheManager = new OutputCacheManager();
            cacheManager.RemoveItems("Orders");
            cacheManager.RemoveItems("OrdersProcessing");
            cacheManager.RemoveItems("OrdersTransfer");
            cacheManager.RemoveItems("OrdersTrouble");
            cacheManager.RemoveItems("OrdersPayment");
        } 
    }
}