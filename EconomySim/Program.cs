using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;

namespace Idek1 {

    public static class Program {

        // The Law of Supply and Demand

        /// <summary>
        /// Represents the current iteration we are on in the simulation.
        /// </summary>
        private static int Iteration { get; set; } = 0;

        /// <summary>
        /// The list of all currently available products in the simulation.
        /// </summary>
        public static LinkedList<Product> Products { get; } = new LinkedList<Product>();

        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions() {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
        private const string SerializerProductsFile = "Products.json";

        public static void Main(string[] programArgs) {
RESET:
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("Economy Simulator by FireController#1847");
            Console.WriteLine("Version 0.1.0");
#if DEBUG
            Console.WriteLine("#### DEVELOPMENT VERSION ####");
#endif

            // Perform initial product print
            if (!File.Exists(SerializerProductsFile) || !Program.LoadProducts()) {
                // Load default products list
                Product exampleProduct = new Product("Example Product") {
                    Price = 450,
                    Supply = 200,
                    Demand = 200
                };
                Products.AddLast(exampleProduct);
            }
            Program.PrintProducts("Pre-Iteration Report");

            // Start the main simulation loop
            string? response;
            int skipCounter = 0;
            bool stopSimulation = false;
            do {
                // Accept input
                if (skipCounter <= 0) {
                    do {
                        Console.WriteLine("Enter your command or press enter to perform the next iteration.");
                        Console.Write("> ");
                        response = Console.ReadLine()?.Trim().ToLower();
                        if (string.IsNullOrWhiteSpace(response)) {
                            response = null;
                        }
                        if (response != null) {
                            string[] args = response.Split(" ", StringSplitOptions.RemoveEmptyEntries & StringSplitOptions.RemoveEmptyEntries);

                            // See command
                            if (response.Contains("see")) {
                                LinkedListNode<Product>? product = ArgumentHelpers.ValidateProduct(ref args, 1);
                                if (product == null) {
                                    continue;
                                }
                                Console.WriteLine();
                                Console.WriteLine(product.ValueRef.ToString());
                                Console.WriteLine();
                            } else if (response.Contains("create")) {
                                if (args.Length <= 1 || string.IsNullOrEmpty(args[1])) {
                                    Console.WriteLine("Please provide the type of item you'd like to create ('product').");
                                    Console.WriteLine();
                                    continue;
                                }
                                if (args[1].Contains("product")) {
                                    Console.Write("Product Name? ");
                                    string? name = Console.ReadLine();
                                    if (string.IsNullOrEmpty(name)) {
                                        Console.WriteLine("Invalid product name!");
                                        Console.WriteLine();
                                        continue;
                                    }
                                    Console.Write("Starting price? ");
                                    int startingPrice;
                                    try {
                                        string? startingPriceStr = Console.ReadLine();
                                        if (string.IsNullOrEmpty(startingPriceStr)) {
                                            Console.WriteLine("Invalid starting price!");
                                            Console.WriteLine();
                                            continue;
                                        }
                                        startingPrice = Convert.ToInt32(startingPriceStr);
                                    } catch {
                                        Console.WriteLine("Starting price must be a valid whole number (ex $4.50 would be 450)!");
                                        Console.WriteLine();
                                        continue;
                                    }
                                    Console.Write("Starting supply? ");
                                    int startingSupply;
                                    try {
                                        string? startingSupplyStr = Console.ReadLine();
                                        if (string.IsNullOrEmpty(startingSupplyStr)) {
                                            Console.WriteLine("Invalid starting supply!");
                                            Console.WriteLine();
                                            continue;
                                        }
                                        startingSupply = Convert.ToInt32(startingSupplyStr);
                                    } catch {
                                        Console.WriteLine("Starting supply must be a valid whole number!");
                                        Console.WriteLine();
                                        continue;
                                    }
                                    Console.Write("Starting demand? ");
                                    int startingDemand;
                                    try {
                                        string? startingDemandStr = Console.ReadLine();
                                        if (string.IsNullOrEmpty(startingDemandStr)) {
                                            Console.WriteLine("Invalid starting demand!");
                                            Console.WriteLine();
                                            continue;
                                        }
                                        startingDemand = Convert.ToInt32(startingDemandStr);
                                    } catch {
                                        Console.WriteLine("Starting demand must be a valid whole number!");
                                        Console.WriteLine();
                                        continue;
                                    }

                                    // Make the product
                                    Product newProduct = new Product(name) {
                                        Price = startingPrice,
                                        Supply = startingSupply,
                                        Demand = startingDemand
                                    };
                                    Products.AddLast(newProduct);

                                    Console.WriteLine("Succcessfully created a new product!");
                                    Console.WriteLine();
                                    Console.WriteLine(newProduct);
                                    Console.WriteLine();
                                    continue;
                                } else {
                                    Console.WriteLine("Please provide a valid item type ('product').");
                                    Console.WriteLine();
                                    continue;
                                }
                            } else if (response.Contains("save")) {
                                Console.WriteLine("Saving current products list...");
                                Program.SaveProducts();
                            } else if (response.Contains("load")) {
                                Console.Write("Are you sure you want to overwrite your current products list? (y/n) ");
                                string? res2 = Console.ReadLine();
                                if (res2 != null && res2.Trim().ToLower().Contains("y")) {
                                    if (Program.LoadProducts()) {
                                        Program.PrintProducts("Load Report");
                                    }
                                } else {
                                    Console.WriteLine("Load cancelled.");
                                    Console.WriteLine();
                                    continue;
                                }
                            } else if (response.Contains("reset")) {
                                Console.WriteLine("\u001b[31;1mResetting the simulator will delete your current save. This action is irreversible.\u001b[0m");
                                Console.Write("Are you sure you want to reset the simulator? (y/n) ");
                                string? res2 = Console.ReadLine();
                                if (res2 != null && res2.Trim().ToLower().Contains("y")) {
                                    Console.WriteLine();

                                    // Delete save files
                                    File.Delete(SerializerProductsFile);
                                    Console.WriteLine("Deleted save files.");
                                    Thread.Sleep(50);

                                    // Reset program state
                                    Console.WriteLine("Clearing products list...");
                                    Thread.Sleep(75);
                                    Product.NextId = 1;
                                    Products.Clear();

                                    // Clear console
                                    Console.WriteLine("Finishing up...");
                                    Thread.Sleep(500);
                                    Console.Clear();
                                    Thread.Sleep(1000);

                                    // Reset
                                    goto RESET;
                                } else {
                                    Console.WriteLine("Reset cancelled.");
                                    Console.WriteLine();
                                    continue;
                                }
                            } else if (response.Contains("update")) {
                                LinkedListNode<Product>? product = ArgumentHelpers.ValidateProduct(ref args, 1);
                                if (product == null) {
                                    continue;
                                }
                                if (args.Length <= 2 || string.IsNullOrEmpty(args[2])) {
                                    Console.WriteLine("Please provide the property you would like to update ('price', 'supply', or 'demand')");
                                    Console.WriteLine();
                                    continue;
                                }
                                if (args.Length <= 3 || string.IsNullOrEmpty(args[3])) {
                                    Console.WriteLine("Please provide the value to update with. Ensure it is a whole number (ex $4.50 would be 450).");
                                    Console.WriteLine();
                                    continue;
                                }
                                int updateval;
                                try {
                                    updateval = Convert.ToInt32(args[3]);
                                } catch {
                                    Console.WriteLine("Invalid value to update with. Ensure it is a whole number (ex $4.50 would be 450).");
                                    Console.WriteLine();
                                    continue;
                                }
                                if (args[2].Contains("price")) {
                                    product.ValueRef.UpdatePrevious();
                                    product.ValueRef.UpdatePrice(updateval);
                                    Console.WriteLine("Price updated.");
                                } else if (args[2].Contains("supply")) {
                                    product.ValueRef.UpdatePrevious();
                                    product.ValueRef.UpdateSupply(updateval);
                                    Console.WriteLine("Price updated.");
                                } else if (args[2].Contains("demand")) {
                                    product.ValueRef.UpdatePrevious();
                                    product.ValueRef.UpdateDemand(updateval);
                                    Console.WriteLine("Price updated.");
                                } else {
                                    Console.WriteLine("Unknown property to update. Please provide 'price', 'supply,' or 'demand.'");
                                    Console.WriteLine();
                                    continue;
                                }
                                Console.WriteLine();
                                Console.WriteLine(product.ValueRef.ToString());
                                Console.WriteLine();
                                continue;
                            } else if (response.Contains("set")) {
                                LinkedListNode<Product>? product = ArgumentHelpers.ValidateProduct(ref args, 1);
                                if (product == null) {
                                    continue;
                                }
                                if (args.Length <= 2 || string.IsNullOrEmpty(args[2])) {
                                    Console.WriteLine("Please provide the property you would like to set ('price', 'supply', or 'demand')");
                                    Console.WriteLine();
                                    continue;
                                }
                                if (args.Length <= 3 || string.IsNullOrEmpty(args[3])) {
                                    Console.WriteLine("Please provide the value to set to. Ensure it is a whole number (ex $4.50 would be 450).");
                                    Console.WriteLine();
                                    continue;
                                }
                                int updateval;
                                try {
                                    updateval = Convert.ToInt32(args[3]);
                                } catch {
                                    Console.WriteLine("Invalid value to set to. Ensure it is a whole number (ex $4.50 would be 450).");
                                    Console.WriteLine();
                                    continue;
                                }
                                if (args[2].Contains("price")) {
                                    product.ValueRef.UpdatePrevious();
                                    product.ValueRef.Price = updateval;
                                    Console.WriteLine("Price updated.");
                                } else if (args[2].Contains("supply")) {
                                    product.ValueRef.UpdatePrevious();
                                    product.ValueRef.Supply = updateval;
                                    Console.WriteLine("Price updated.");
                                } else if (args[2].Contains("demand")) {
                                    product.ValueRef.UpdatePrevious();
                                    product.ValueRef.Demand = updateval;
                                    Console.WriteLine("Price updated.");
                                } else {
                                    Console.WriteLine("Unknown property to update. Please provide 'price', 'supply,' or 'demand.'");
                                    Console.WriteLine();
                                    continue;
                                }
                                Console.WriteLine();
                                Console.WriteLine(product.ValueRef.ToString());
                                Console.WriteLine();
                                continue;
                            } else if (response.Contains("add")) {
                                LinkedListNode<Product>? product = ArgumentHelpers.ValidateProduct(ref args, 1);
                                if (product == null) {
                                    continue;
                                }
                                if (args.Length <= 2 || string.IsNullOrEmpty(args[2])) {
                                    Console.WriteLine("Please provide the property you would like to add ('price', 'supply', or 'demand')");
                                    Console.WriteLine();
                                    continue;
                                }
                                if (args.Length <= 3 || string.IsNullOrEmpty(args[3])) {
                                    Console.WriteLine("Please provide the value to add. Ensure it is a whole number (ex $4.50 would be 450).");
                                    Console.WriteLine();
                                    continue;
                                }
                                int updateval;
                                try {
                                    updateval = Convert.ToInt32(args[3]);
                                } catch {
                                    Console.WriteLine("Invalid value to add. Ensure it is a whole number (ex $4.50 would be 450).");
                                    Console.WriteLine();
                                    continue;
                                }
                                if (args[2].Contains("price")) {
                                    product.ValueRef.UpdatePrevious();
                                    product.ValueRef.UpdatePrice(product.ValueRef.Price + updateval);
                                    Console.WriteLine("Price updated.");
                                } else if (args[2].Contains("supply")) {
                                    product.ValueRef.UpdatePrevious();
                                    product.ValueRef.UpdateSupply(product.ValueRef.Supply + updateval);
                                    Console.WriteLine("Price updated.");
                                } else if (args[2].Contains("demand")) {
                                    product.ValueRef.UpdatePrevious();
                                    product.ValueRef.UpdateDemand(product.ValueRef.Demand + updateval);
                                    Console.WriteLine("Price updated.");
                                } else {
                                    Console.WriteLine("Unknown property to update. Please provide 'price', 'supply,' or 'demand.'");
                                    Console.WriteLine();
                                    continue;
                                }
                                Console.WriteLine();
                                Console.WriteLine(product.ValueRef.ToString());
                                Console.WriteLine();
                                continue;
                            } else if (response.Contains("sub")) {
                                LinkedListNode<Product>? product = ArgumentHelpers.ValidateProduct(ref args, 1);
                                if (product == null) {
                                    continue;
                                }
                                if (args.Length <= 2 || string.IsNullOrEmpty(args[2])) {
                                    Console.WriteLine("Please provide the property you would like to subtract ('price', 'supply', or 'demand')");
                                    Console.WriteLine();
                                    continue;
                                }
                                if (args.Length <= 3 || string.IsNullOrEmpty(args[3])) {
                                    Console.WriteLine("Please provide the value to subtract. Ensure it is a whole number (ex $4.50 would be 450).");
                                    Console.WriteLine();
                                    continue;
                                }
                                int updateval;
                                try {
                                    updateval = Convert.ToInt32(args[3]);
                                } catch {
                                    Console.WriteLine("Invalid value to add. Ensure it is a whole number (ex $4.50 would be 450).");
                                    Console.WriteLine();
                                    continue;
                                }
                                if (args[2].Contains("price")) {
                                    product.ValueRef.UpdatePrevious();
                                    product.ValueRef.UpdatePrice(product.ValueRef.Price - updateval);
                                    Console.WriteLine("Price updated.");
                                } else if (args[2].Contains("supply")) {
                                    product.ValueRef.UpdatePrevious();
                                    product.ValueRef.UpdateSupply(product.ValueRef.Supply - updateval);
                                    Console.WriteLine("Price updated.");
                                } else if (args[2].Contains("demand")) {
                                    product.ValueRef.UpdatePrevious();
                                    product.ValueRef.UpdateDemand(product.ValueRef.Demand - updateval);
                                    Console.WriteLine("Price updated.");
                                } else {
                                    Console.WriteLine("Unknown property to update. Please provide 'price', 'supply,' or 'demand.'");
                                    Console.WriteLine();
                                    continue;
                                }
                                Console.WriteLine();
                                Console.WriteLine(product.ValueRef.ToString());
                                Console.WriteLine();
                                continue;
                            } else if (response.Contains("skip")) {
                                if (args.Length <= 1 || string.IsNullOrEmpty(args[1])) {
                                    Console.WriteLine("Please provide the number of iterations you would like to perform.");
                                    Console.WriteLine();
                                    continue;
                                }
                                try {
                                    skipCounter = Convert.ToInt32(args[1]) - 1;
                                } catch {
                                    Console.WriteLine("Invalid number.");
                                    Console.WriteLine();
                                    continue;
                                }
                                Console.WriteLine($"Performing {skipCounter + 1} iterations...");
                                response = null;
                                continue;
                            } else if ((new string[] { "stop", "exit", "quit" }).Contains(response)) {
                                Console.Write("Do you want to save your current products list? (y/n) ");
                                string? res2 = Console.ReadLine();
                                if (res2 != null && res2.Trim().ToLower().Contains("y")) {
                                    Program.SaveProducts();
                                }
                                stopSimulation = true;
                                break;
                            } else {
                                Console.WriteLine("Unknown command.");
                                Console.WriteLine();
                            }
                        }
                    } while (response != null);
                } else {
                    skipCounter--;
                }

                // Perform iteration
                if (!stopSimulation) {
                    // Increase the iteration
                    Console.WriteLine();
                    Console.WriteLine("==========================");
                    Console.WriteLine($"\u001b[1mIteration {++Iteration}\u001b[0m");

                    // Perform product demand updates and display updated products
                    Console.WriteLine($"You currently have {Products.Count} product(s) in your inventory.");
                    Console.WriteLine();
                    foreach (Product product in Products) {
                        // For right now, it's every product updated
                        product.UpdatePrevious();
                        if (product.Demand != 0 && product.Supply != 0) {
                            product.UpdateSupply(product.Supply - 1);
                            product.UpdateDemand(product.Demand - 1);
                        }
                        Console.WriteLine(product.ToString());
                        Console.WriteLine();
                    }
                }
            } while (!stopSimulation);

            // Close the simulator
            Console.Write(Environment.NewLine + "Press any key to close the simulator.");
            Console.ReadKey(true);
        }

        public static void SaveProducts() {
            JsonArray arr = new JsonArray();
            LinkedListNode<Product>? node = Products.First;
            while (node != null) {
                arr.Add(JsonSerializer.SerializeToDocument(node.ValueRef, SerializerOptions));
                node = node.Next;
            }
            string db = arr.ToJsonString(SerializerOptions);
            File.WriteAllText(SerializerProductsFile, db);
            Console.WriteLine($"Successfully saved {arr.Count} product(s).");
            Console.WriteLine();
        }

        public static bool LoadProducts() {
            Console.WriteLine("Loading products...");
            try {
                string db = File.ReadAllText(SerializerProductsFile);
                JsonArray? arr = JsonSerializer.Deserialize<JsonArray>(db, SerializerOptions);
                if (arr != null) {
                    Products.Clear();
                    int failed = 0;
                    for (int i = 0; i < arr.Count; i++) {
                        Product? product = JsonSerializer.Deserialize<Product>(arr[i], SerializerOptions);
                        if (product == null) {
                            failed++;
                            continue;
                        }
                        Products.AddLast(product);
                        if (i == (arr.Count - 1)) {
                            Product.NextId = product.Id + 1;
                        }
                    }
                    if (failed > 0) {
                        Console.WriteLine($"Failed to load {failed} product(s).");
                    }
                    Console.WriteLine($"Successfully loaded {Products.Count} product(s).");
                    return true;
                } else {
                    Console.WriteLine("Internal error loading products.");
                    return false;
                }
            } catch {
                Console.WriteLine("Internal error loading products.");
                return false;
            }
        }

        public static void PrintProducts(string title = "Product Report") {
            Console.WriteLine();
            Console.WriteLine("==========================");
            Console.WriteLine($"\u001b[1m{title}\u001b[0m");
            Console.WriteLine($"You currently have {Products.Count} product(s) in your inventory.");
            Console.WriteLine();
            foreach (Product product in Products) {
                Console.WriteLine(product.ToString());
                Console.WriteLine();
            }
        }

    }

    public static class ArgumentHelpers {

        public static LinkedListNode<Product>? ValidateProduct(ref string[] args, int index) {
            if (args.Length <= 1 || string.IsNullOrEmpty(args[index])) {
                Console.WriteLine("Please provide the ID of the product you would like to modify or see.");
                Console.WriteLine();
                return null;
            }
            LinkedListNode<Product>? product = null;
            try {
                int id = Convert.ToInt32(args[index]);
                LinkedListNode<Product> node = Program.Products.First;
                while (node != null) {
                    if (node.Value.Id == id) {
                        product = node;
                        break;
                    }
                    node = node.Next;
                }
            } catch { }
            if (product == null) {
                Console.WriteLine("Invalid ID or invalid product.");
                Console.WriteLine();
                return null;
            }
            return product;
        }

    }

}