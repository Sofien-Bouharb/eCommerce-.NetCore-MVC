using eCommerce.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;

namespace eCommerce.Helpers
{
    public static class CartHelper
    {
        private const string CartKey = "cart";

        // -----------------------------
        // GET CART
        // -----------------------------
        public static List<CartItem> GetCart(ISession session)
        {
            var cart = session.GetObject<List<CartItem>>(CartKey);
            if (cart == null)
            {
                cart = new List<CartItem>();
                session.SetObject(CartKey, cart);
            }
            return cart;
        }

        // -----------------------------
        // SAVE CART
        // -----------------------------
        public static void SaveCart(ISession session, List<CartItem> cart)
        {
            session.SetObject(CartKey, cart);
        }

        // -----------------------------
        // ADD OR INCREASE QUANTITY
        // -----------------------------
        public static void AddToCart(ISession session, int productId, int quantity = 1)
        {
            var cart = GetCart(session);

            var existing = cart.FirstOrDefault(c => c.ProductId == productId);

            if (existing != null)
                existing.Quantity += quantity;
            else
                cart.Add(new CartItem { ProductId = productId, Quantity = quantity });

            SaveCart(session, cart);
        }

        // -----------------------------
        // REMOVE ITEM COMPLETELY
        // -----------------------------
        public static void RemoveFromCart(ISession session, int productId)
        {
            var cart = GetCart(session);
            cart.RemoveAll(c => c.ProductId == productId);
            SaveCart(session, cart);
        }

        // -----------------------------
        // TOGGLE ITEM (add/remove)
        // -----------------------------
        public static bool ToggleItem(ISession session, int productId)
        {
            var cart = GetCart(session);
            var item = cart.FirstOrDefault(c => c.ProductId == productId);

            if (item != null)
            {
                cart.Remove(item);
                SaveCart(session, cart);
                return false; // removed
            }
            else
            {
                cart.Add(new CartItem { ProductId = productId, Quantity = 1 });
                SaveCart(session, cart);
                return true; // added
            }
        }

        // -----------------------------
        // UPDATE QUANTITY
        // -----------------------------
        public static void UpdateQuantity(ISession session, int productId, int newQty)
        {
            var cart = GetCart(session);

            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            if (item == null) return;

            if (newQty <= 0)
                cart.Remove(item);
            else
                item.Quantity = newQty;

            SaveCart(session, cart);
        }

        // -----------------------------
        // CLEAR CART
        // -----------------------------
        public static void ClearCart(ISession session)
        {
            session.Remove(CartKey);
        }

        // -----------------------------
        // GET CART COUNT (your existing feature)
        // -----------------------------
        public static int GetCartCount(ISession session)
        {
            var cart = session.GetObject<List<CartItem>>(CartKey);

            if (cart == null)
                return 0;

            return cart.Sum(item => item.Quantity);
        }
    }
}
