using eCommerce.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;

namespace eCommerce.Helpers
{
    public static class CartHelper
    {
        public static int GetCartCount(ISession session)
        {
            var cart = session.GetObject<List<CartItem>>("cart");

            if (cart == null)
                return 0;

            return cart.Sum(item => item.Quantity);
        }
    }
}
