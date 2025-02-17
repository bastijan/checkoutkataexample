﻿using SupermarketCheckout.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SupermarketCheckout.Logic
{
    /// <summary>
    /// Scan and calculate total price
    /// </summary>
    public class ShoppingCart
    {

        // userCart is a list of products in the user's cart
        private static List<Product> userCart = new List<Product>();

        // userCartProducts is a list of products in the user's cart, just for display purposes
        private static List<string> userCartProducts = new List<string>();

        private static Product product;
        // productsList is a list of products
        private static readonly List<Product> productsList = SeedProducts.GetProducts();


        /*
         * Get product by id
         * @param id
         * If product is not found return null         
         */
        private Product GetProductById(int id)
        {
            product = productsList.FirstOrDefault(p => p.Id == id);
            return product;
        }

        public void ShowScan(string userInput)
        {
            (string productName, decimal subTotal) scanResult = Scan(userInput);

            if (scanResult.productName == null)
            {
                Console.WriteLine("Invalid input");
            }
            else
            {
                Console.WriteLine("Product {0} added to the cart!", scanResult.productName);
                Console.WriteLine("subtotal: {0}", scanResult.subTotal);
                Console.WriteLine("Your shopping cart: {0}", String.Join(", ", userCartProducts.ToArray()));
            }
            Console.Write("\r\nPress Enter to return to Main Menu");
            Console.ReadLine();
        }

        /// <summary>
        /// Scans the specified user input.
        /// </summary>
        /// <param name="userInput">The user input.</param>
        /// <returns>(string, decimal)</returns>
        public (string, decimal) Scan(string userInput)
        {
            string retString;
            bool success = false;
            decimal subTotal = 0;
            if (int.TryParse(userInput, out int userInputInt))
            {
                product = GetProductById(userInputInt);

                if (product != null)
                {
                    userCart.Add(product);
                    userCartProducts.Add(product.Name);
                    success = true;
                }
            }


            if (success)
            {
                retString = product.Name;
            }
            else
            {
                retString = null;
            }
            subTotal = GetTotal();

            return (retString, subTotal);

        }

        /// <summary>
        /// Gets the total.
        /// This method uses a LINQ query to group products by name and sum the quantity
        ///  Then it calculates the total based on the quantity and the product price
        /// If the product has a discount, it calculates the total based on the discount
        /// Math.DivRem is used to calculate the discount quantity(DiscountPackages) and the remainder(products left) :
        ///   The DiscountPackages is multiplied by the DiscountQtyPrice and adds up the remainder multiplied by the Price
        /// DiscountPackages * DiscountQtyPrice + products left * Price
        ///
        ///
        /// NOTE: The first thinks are usually to use a for loop and a switch statement to decide is a product has a discount or not,
        /// but I think that is better to use built-in power of LINQ and Math library to solve this problem
        /// in one line instead loop and switch (I hate loops because of time complexity and avoid it if it is possible)
        /// </summary>
        /// <returns>decimal</returns>
        public decimal GetTotal()
        {
            decimal total = 0;
            // group products by product Id and sum the quantity
            var sortedCart = userCart.GroupBy(x => x.Id)
                .Select(g => new
                {
                    Id = g.Key,
                    Count = g.Count(),
                    Name = g.First().Name,
                    Price = g.First().Price,
                    DiscountQty = g.First().DiscountQty,
                    DiscountQtyPrice = g.First().DiscountQtyPrice
                })
                .ToList();

            // calculate total in reduced foreach, 
            // not using a number of loops equal to number of items in cart, but just number of different products in cart
            foreach (var item in sortedCart)
            {
                if (item.DiscountQty != null)
                {
                    // calculate discount packages numbers and products left
                    int DisountPackages = Math.DivRem(item.Count, (int)item.DiscountQty, out int remainder);
                    total += (decimal)(DisountPackages * item.DiscountQtyPrice) + (remainder * item.Price);
                }
                else
                {
                    total += item.Count * item.Price;
                }

            }

            return total;
        }

        /// <summary>
        /// Shows the total.
        /// </summary>
        public void ShowTotal()
        {
            var total = GetTotal();
            Console.WriteLine("Your shopping cart total: {0}", total);
            Console.ReadKey();
        }

        /// <summary>
        /// Resets the shopping cart.
        /// </summary>
        public void ResetShoppingCart()
        {
            userCart.Clear();
        }
    }
}
