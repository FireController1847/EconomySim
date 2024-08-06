using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Idek1 {

    /// <summary>
    /// Represents something that has a price, a supply, and a demand.
    /// </summary>
    public class Product {

        private const double Magnitude = 1.25;
        public static int NextId { get; internal set; } = 1;

        /// <summary>
        /// The name of this product.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A unique ID assigned to every product.
        /// </summary>
        [JsonInclude]
        public int Id { get; private set; }

        /// <summary>
        /// The price from the last iteration.
        /// </summary>
        [JsonIgnore]
        public int PreviousPrice { get; private set; }

        /// <summary>
        /// The price multiplied by 100.
        /// </summary>
        public int Price { get; set; }

        /// <summary>
        /// This product's price divided by 100 and with a dollar sign in front.
        /// </summary>
        /// <example>$450.00</example>
        [JsonIgnore]
        public string DisplayPrice {
            get {
                return $"${Price / 100.0:F2}";
            }
        }

        /// <summary>
        /// The supply from the last iteration.
        /// </summary>
        [JsonIgnore]
        public int PreviousSupply { get; private set; }

        /// <summary>
        /// The number of units this product has currently available for purchase.
        /// </summary>
        public int Supply { get; set; }

        /// <summary>
        /// The demand from the last iteration.
        /// </summary>
        [JsonIgnore]
        public int PreviousDemand { get; private set; }

        /// <summary>
        /// The number of units the consumers are demanding from this product.
        /// </summary>
        public int Demand { get; set; }

        /// <summary>
        /// Creates a new product with no price, supply, or demand.
        /// </summary>
        /// <param name="name">The name of this product.</param>
        public Product(string name) {
            Name = name;
            Id = NextId++;
            PreviousPrice = Price;
            PreviousSupply = Supply;
            PreviousDemand = Demand;
        }

        public void UpdatePrice(int price) {
            if (price < 1) {
                price = 1;
            }
            int priorPrice = Price;
            Price = price;

            // The price magnifier is determined by the difference in price
            // Perform on a logarithmic falloff
            double logPrice1 = Math.Log10(priorPrice);
            double logPrice2 = Math.Log10(price );
            double difference = Math.Abs(logPrice1 - logPrice2);
            double magnitude = (difference * Magnitude * 2) + 1;

            // If the price goes up, the supply will go up and the demand will go down
            // If the price goes down, the supply will go down and the demand will go up
            // If the price stays the same, then nothing changes
            if (Price > priorPrice) {
                Supply = Convert.ToInt32(Math.Round(Supply * magnitude));
                Demand = Convert.ToInt32(Math.Round(Demand / magnitude));
            } else if (Price < priorPrice) {
                Supply = Convert.ToInt32(Math.Round(Supply / magnitude));
                Demand = Convert.ToInt32(Math.Round(Demand * magnitude));
            }
        }

        public void UpdateSupply(int supply) {
            if (supply < 0) {
                supply = 0;
            }
            int priorSupply = Supply;
            Supply = supply;

            // If the supply goes up, demand stays the same and price goes down
            // If the supply goes down, demand stays the same and price goes up
            // If the price stays the same, then nothing changes
            if (Supply > priorSupply) {
                Price = Convert.ToInt32(Math.Round(Price / Magnitude));
            } else if (Supply < priorSupply) {
                Price = Convert.ToInt32(Math.Round(Price * Magnitude));
            }
        }

        public void UpdateDemand(int demand) {
            if (demand < 0) {
                demand = 0;
            }
            int priorDemand = Demand;
            Demand = demand;

            // If the demand goes up, supply stays the same and price goes up
            // If the demand goes down, supply stays the same and price goes down
            // If the demand stays the same, then nothing changes
            if (Demand > priorDemand) {
                Price = Convert.ToInt32(Math.Round(Price * Magnitude));
            } else if (Demand < priorDemand) {
                Price = Convert.ToInt32(Math.Round(Price / Magnitude));
            }
        }

        public void UpdatePrevious() {
            PreviousPrice = Price;
            PreviousSupply = Supply;
            PreviousDemand = Demand;
        }

        public override string ToString() {
            string priceUpDown = (PreviousPrice > Price ? "\u001b[31m\u2193\u001b[0m" : (PreviousPrice < Price) ? "\u001b[32m\u2191\u001b[0m" : "\u001b[1m–\u001b[0m");
            string supplyUpDown = (PreviousSupply > Supply ? "\u001b[31m\u2193\u001b[0m" : (PreviousSupply < Supply) ? "\u001b[32m\u2191\u001b[0m" : "\u001b[1m–\u001b[0m");
            string demandUpDown = (PreviousDemand > Demand ? "\u001b[31m\u2193\u001b[0m" : (PreviousDemand < Demand) ? "\u001b[32m\u2191\u001b[0m" : "\u001b[1m–\u001b[0m");
            return $"# {Name}{Environment.NewLine}" +
                   $"Id: {Id}{Environment.NewLine}" +
                   $"Price: {DisplayPrice} {priceUpDown}{Environment.NewLine}" +
                   $"Supply: {Supply} units {supplyUpDown}{Environment.NewLine}" +
                   $"Demand: {Demand} units {demandUpDown}";
        }

    }

}
