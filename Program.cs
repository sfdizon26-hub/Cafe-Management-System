using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace CafeMenu_SalesManagement
{
    class CafeEntity
    {
        public string Name { get; set; }

        public CafeEntity() { Name = string.Empty; }

        public CafeEntity(string name) { Name = name; }
    }

    interface IPrintable { void PrintDetails(); }
    interface IRecordable { void RecordTransaction(); }

    class Sale : CafeEntity, IPrintable, IRecordable
    {
        public int SaleId { get; private set; }
        private static int nextId = 1;

        public static void InitializeNextId()
        {
            string salesFile = "sales.txt";
            if (File.Exists(salesFile))
            {
                string[] lines = File.ReadAllLines(salesFile);
                if (lines.Length > 0)
                {
                    string lastLine = lines[lines.Length - 1];
                    int start = lastLine.IndexOf('#') + 1;
                    int end = lastLine.IndexOf(" -");
                    if (start > 0 && end > start)
                    {
                        string idStr = lastLine.Substring(start, end - start);
                        if (int.TryParse(idStr, out int lastId)) nextId = lastId + 1;
                    }
                }
            }
        }

        public int Quantity { get; set; }
        public double Total { get; set; }
        public DateTime Date { get; set; }
        public string CashierName { get; set; }

        public Sale(string itemName, int quantity, double total, DateTime date, string cashier) : base(itemName)
        {
            Quantity = quantity;
            Total = total;
            Date = date;
            CashierName = cashier;
            SaleId = nextId++;
        }

        public void PrintDetails()
        {
            Console.WriteLine($"Sale #{SaleId}: {Date} - {Name} x{Quantity} = ₱{Total}");
        }

        public void RecordTransaction()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Sale #{SaleId} recorded successfully!");
            Console.ResetColor();
            string record = $"Sale #{SaleId} - {Date} - {Name} x{Quantity} = ₱{Total} (By: {CashierName}) | [PENDING]";
            File.AppendAllText("sales.txt", record + Environment.NewLine);
        }
    }

    class CartItem
    {
        public string Name { get; set; }
        public int Qty { get; set; }
        public double Price { get; set; }
        public double Total => Price * Qty;
    }

    class Account
    {
        public string Role { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public double Salary { get; set; } 

        public override string ToString()
        {
            if (Role.ToLower() == "boss" || Role.ToLower() == "owner")
            {
                return $"{Role}|{Username}|{Password}";
            }
            return $"{Role}|{Username}|{Password}|{Salary}";
        }
    }

    class IngredientInfo
    {
        public int Qty { get; set; }
        public string Unit { get; set; }

        public IngredientInfo() { Qty = 0; Unit = "pieces"; }
        public IngredientInfo(int q, string u) { Qty = q; Unit = u; }
    }


    static class UI
    {
        public static void DrawHeader(string title)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine($"║ {title.PadRight(60)} ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        public static void DrawWidget(string title, string content, ConsoleColor color)
        {
            Console.ForegroundColor = color;

            string boxTop = $"┌───────────────────────────────┐";
            string boxBottom = $"└───────────────────────────────┘";

            string formattedTitle = $"{title}".PadRight(29);
            string formattedContent = $"{content}".PadRight(29);

            Console.WriteLine(boxTop);
            Console.WriteLine($"│ {formattedTitle} │");
            Console.WriteLine($"│ {formattedContent} │");
            Console.WriteLine(boxBottom);

            Console.ResetColor();
        }


        public static void DrawAlert(string message)
        {
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($" [!!!] ALERT: {message} ");
            Console.ResetColor();
        }
    }

    class Inventory
    {
        public const string InventoryFile = "inventory.txt";
        private Dictionary<string, Dictionary<string, IngredientInfo>> ingredients;

        public IReadOnlyDictionary<string, Dictionary<string, IngredientInfo>> IngredientsData => ingredients;

        public Inventory()
        {
            ingredients = new Dictionary<string, Dictionary<string, IngredientInfo>>();
            LoadIngredients();
        }

        public void AddOrUpdateNewIngredient(string category, string item, int qty, string unit)
        {
            if (!ingredients.ContainsKey(category))
            {
                ingredients[category] = new Dictionary<string, IngredientInfo>();
            }
            if (ingredients[category].ContainsKey(item))
            {
                ingredients[category][item].Qty += qty;
                if (!string.IsNullOrWhiteSpace(unit))
                {
                    if (string.IsNullOrWhiteSpace(ingredients[category][item].Unit))
                        ingredients[category][item].Unit = unit;
                }
            }
            else
            {
                ingredients[category][item] = new IngredientInfo(qty, unit);
            }
        }


        private void LoadIngredients()
        {
            if (!File.Exists(InventoryFile) || new FileInfo(InventoryFile).Length == 0)
            {
                ingredients["Raw Materials"] = new Dictionary<string, IngredientInfo>
            {
                { "Coffee Beans", new IngredientInfo(1000, "grams") },
                { "Matcha Powder", new IngredientInfo(1000, "grams") },
                { "Cocoa Powder", new IngredientInfo(1000, "grams") },
                { "Sugar", new IngredientInfo(5000, "grams") }
            };
                ingredients["Dairy"] = new Dictionary<string, IngredientInfo>
            {
                { "Milk", new IngredientInfo(5000, "ml") }
            };

                ingredients["Packaging"] = new Dictionary<string, IngredientInfo>
            {
                { "Cups", new IngredientInfo(500, "pieces") },
                { "Lids", new IngredientInfo(500, "pieces") },
                { "Straws", new IngredientInfo(500, "pieces") },
            };
                SaveInventory();
            }
            else
            {
                var lines = File.ReadAllLines(InventoryFile);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split('|');
                    if (parts.Length == 4 && int.TryParse(parts[2], out int qty))
                    {
                        string cat = parts[0];
                        string name = parts[1];
                        string unit = parts[3];
                        if (!ingredients.ContainsKey(cat)) ingredients[cat] = new Dictionary<string, IngredientInfo>();
                        ingredients[cat][name] = new IngredientInfo(qty, unit);
                    }
                    else if (parts.Length == 3 && int.TryParse(parts[2], out int qty2))
                    {
                        string cat = parts[0];
                        string name = parts[1];
                        string unit = "pieces"; 
                        if (!ingredients.ContainsKey(cat)) ingredients[cat] = new Dictionary<string, IngredientInfo>();
                        ingredients[cat][name] = new IngredientInfo(qty2, unit);
                    }
                }
            }
        }

        public void SaveInventory()
        {
            var lines = new List<string>();
            foreach (var cat in ingredients)
            {
                foreach (var item in cat.Value)
                {
                    lines.Add($"{cat.Key}|{item.Key}|{item.Value.Qty}|{item.Value.Unit}");
                }
            }
            File.WriteAllLines(InventoryFile, lines);
        }

        public void ViewStocks()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n=== INGREDIENT STOCKS ===");
            foreach (var cat in ingredients)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\n[{cat.Key}]");
                Console.WriteLine("{0,-20} | {1,12} | {2,8}", "Item", "Qty", "Unit");
                Console.WriteLine(new string('-', 50));
                foreach (var item in cat.Value)
                {
                    if (item.Value.Qty < 50) Console.ForegroundColor = ConsoleColor.Red;
                    else Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("{0,-20} | {1,12:N0} | {2,8}", item.Key, item.Value.Qty, item.Value.Unit);
                }
                Console.ResetColor();
            }
        }

        public bool IsItemAvailable(string menuItemName, Dictionary<string, Dictionary<string, int>> recipes)
        {
            if (!recipes.ContainsKey(menuItemName)) return false;
            var requiredIngredients = recipes[menuItemName];
            foreach (var req in requiredIngredients)
            {
                string ingredientName = req.Key;
                int amountNeeded = req.Value;
                bool ingredientFound = false;

                foreach (var cat in ingredients)
                {
                    if (cat.Value.ContainsKey(ingredientName))
                    {
                        ingredientFound = true;
                        if (cat.Value[ingredientName].Qty < amountNeeded) return false;
                        break;
                    }
                }
                if (!ingredientFound) return false;
            }
            return true;
        }

        public bool DeductIngredients(string menuItemName, int quantity, Dictionary<string, Dictionary<string, int>> recipes)
        {
            if (!recipes.ContainsKey(menuItemName)) return true;
            var requiredIngredients = recipes[menuItemName];

            foreach (var req in requiredIngredients)
            {
                string ingredientName = req.Key;
                int totalNeeded = req.Value * quantity;
                bool found = false;

                foreach (var cat in ingredients)
                {
                    if (cat.Value.ContainsKey(ingredientName))
                    {
                        found = true;
                        if (cat.Value[ingredientName].Qty < totalNeeded)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Error: Not enough {ingredientName}!");
                            Console.ResetColor();
                            return false;
                        }
                        break;
                    }
                }
                if (!found) return false;
            }

            foreach (var req in requiredIngredients)
            {
                string ingredientName = req.Key;
                int totalNeeded = req.Value * quantity;

                foreach (var cat in ingredients)
                {
                    if (cat.Value.ContainsKey(ingredientName))
                    {
                        cat.Value[ingredientName].Qty -= totalNeeded;
                        break;
                    }
                }
            }
            SaveInventory();
            return true;
        }

        public void RestockItem(string role)
        {
            if (role != "owner" && role != "boss")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Access denied!");
                Console.ResetColor();
                return;
            }

            Console.Write("Enter Category: "); string cat = Console.ReadLine().Trim();
            if (!ingredients.ContainsKey(cat)) { Console.WriteLine("Category not found."); return; }

            Console.Write("Enter Item: "); string item = Console.ReadLine().Trim();
            if (!ingredients[cat].ContainsKey(item)) { Console.WriteLine("Item not found."); return; }

            Console.Write("Enter Qty to Add: ");
            if (!int.TryParse(Console.ReadLine(), out int qty) || qty <= 0)
            {
                Console.WriteLine("Invalid Qty. Must be a positive number.");
                return;
            }

            Console.Write("Enter Cost (₱): ");
            if (!double.TryParse(Console.ReadLine(), out double cost))
            {
                Console.WriteLine("Invalid Cost");
                return;
            }

            ingredients[cat][item].Qty += qty;
            SaveInventory();

            File.AppendAllText("expenses.txt", $"{DateTime.Now} | Restock: {item} | ₱{cost:N2}\n");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Stock Updated & Expense Recorded.");
            Console.ResetColor();
        }

        public bool DeleteIngredient(string category, string itemName)
        {
            if (!ingredients.ContainsKey(category)) return false;
            if (!ingredients[category].ContainsKey(itemName)) return false;

            ingredients[category].Remove(itemName);
            if (ingredients[category].Count == 0) ingredients.Remove(category);
            SaveInventory();
            return true;
        }

        public bool DeleteIngredientInteractive()
        {
            var list = new List<(string category, string name)>();
            foreach (var cat in ingredients)
            {
                foreach (var it in cat.Value)
                {
                    list.Add((cat.Key, it.Key));
                }
            }

            if (list.Count == 0)
            {
                Console.WriteLine("No ingredients to delete.");
                return false;
            }

            Console.WriteLine("\nSelect ingredient to DELETE:");
            for (int i = 0; i < list.Count; i++)
            {
                var info = ingredients[list[i].category][list[i].name];
                Console.WriteLine($"{i + 1}. [{list[i].category}] {list[i].name} - {info.Qty} {info.Unit}");
            }
            Console.Write("Enter number (or 0 to cancel): ");
            if (!int.TryParse(Console.ReadLine(), out int sel) || sel < 0 || sel > list.Count)
            {
                Console.WriteLine("Invalid selection.");
                return false;
            }
            if (sel == 0) { Console.WriteLine("Cancelled."); return false; }

            var chosen = list[sel - 1];
            Console.Write($"Are you sure you want to delete '{chosen.name}' from '{chosen.category}'? (y/n): ");
            if (Console.ReadLine().ToLower() == "y")
            {
                DeleteIngredient(chosen.category, chosen.name);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Deleted.");
                Console.ResetColor();
                return true;
            }
            Console.WriteLine("Cancelled.");
            return false;
        }
    }

    enum TimeFrame { Daily, Weekly, Monthly, Yearly }

    class Program
    {
        const string AccountsFileName = "accounts.txt";
        const string ReportsFile = "reports.txt";
        const string ExpensesFile = "expenses.txt";
        const string MenuFile = "menu.txt";
        const string RecipesFile = "recipes.txt";

        static Dictionary<string, Account> accounts = new Dictionary<string, Account>();
        public static Inventory inventory;

        static Dictionary<string, Dictionary<string, int>> ItemRecipes;

        static string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                if (char.IsControl(key.KeyChar))
                {
                    if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                    {
                        password = password.Substring(0, (password.Length - 1));
                        Console.Write("\b \b");
                    }
                }
                else if (key.Key != ConsoleKey.Enter)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
            } while (key.Key != ConsoleKey.Enter);

            Console.WriteLine();
            return password;
        }

        public static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            Sale.InitializeNextId();
            EnsureFileExists(AccountsFileName);
            EnsureFileExists("sales.txt");
            EnsureFileExists(Inventory.InventoryFile);
            EnsureFileExists(ReportsFile);
            EnsureFileExists(ExpensesFile);
            EnsureFileExists(MenuFile);
            EnsureFileExists(RecipesFile);

            LoadAccounts();
            Dictionary<string, double> ItemPrices = LoadMenu();
            LoadRecipes();

            List<Sale> sales = new List<Sale>();
            inventory = new Inventory();

            if (!AdminSystemUnlock())
            {
                return;
            }

            while (true)
            {
                Console.Clear();
                UI.DrawHeader("CAFE MANAGEMENT SYSTEM: ROLE SELECT");
                Console.WriteLine("\nWho is using the system?");
                Console.WriteLine("1. Boss / Admin (Dashboard)");
                Console.WriteLine("2. Cashier");
                Console.WriteLine("3. Barista");
                Console.WriteLine("4. Customer (Self-Order)");
                Console.WriteLine("5. Shutdown System");
                Console.Write("\nSelect Role: ");

                string choice = Console.ReadLine();

                if (choice == "5")
                {
                    Console.WriteLine("Shutting down...");
                    break;
                }

                switch (choice)
                {
                    case "1":
                        if (VerifyLogin("boss")) BossDashboard(ItemPrices);
                        break;
                    case "2":
                        if (VerifyLogin("cashier")) CashierMenu(ItemPrices, sales);
                        break;
                    case "3":
                        if (VerifyLogin("barista")) BaristaMenu();
                        break;
                    case "4":
                        CustomerKiosk(ItemPrices, sales);
                        break;
                    default:
                        Console.WriteLine("Invalid Selection.");
                        Console.ReadKey();
                        break;
                }
            }
        }

        static bool AdminSystemUnlock()
        {
            while (true)
            {
                Console.Clear();
                DisplayWelcome();

                WriteCentered("(System Locked)", ConsoleColor.White);
                Console.WriteLine();
                WriteCentered("--- ADMIN UNLOCK REQUIRED ---", ConsoleColor.Red);
                Console.WriteLine();

                WriteCentered("Username: ", ConsoleColor.White);
                Console.SetCursorPosition((Console.WindowWidth - 10) / 2, Console.CursorTop - 1);
                Console.Write("Username: ");
                string u = Console.ReadLine();

                Console.Write(new string(' ', (Console.WindowWidth - 10) / 2) + "Password: ");
                string p = ReadPassword();

                if (accounts.TryGetValue(u, out var acc) && (acc.Role.ToLower() == "boss" || acc.Role.ToLower() == "owner") && acc.Password == p)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n");
                    WriteCentered("ACCESS GRANTED. SYSTEM UNLOCKED.", ConsoleColor.Green);
                    Console.ResetColor();
                    System.Threading.Thread.Sleep(1500);
                    return true;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    WriteCentered("ACCESS DENIED. BOSS ONLY.", ConsoleColor.Red);
                    Console.ResetColor();
                    Console.ReadKey();
                }
            }
        }


        static bool BossUnlockSequence()
        {
            while (true)
            {
                Console.Clear();
                DisplayWelcome();
                CenterText("(System Locked)");
                WriteCentered("--- ADMIN UNLOCK REQUIRED ---", ConsoleColor.Red);
                Console.WriteLine();

                Console.Write(new string(' ', (Console.WindowWidth - 10) / 2) + "Username: ");
                string u = Console.ReadLine();

                Console.Write(new string(' ', (Console.WindowWidth - 10) / 2) + "Password: ");
                string p = ReadPassword();

                if (accounts.TryGetValue(u, out var acc) && (acc.Role.ToLower() == "boss" || acc.Role.ToLower() == "owner") && acc.Password == p)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n");
                    WriteCentered("ACCESS GRANTED. SYSTEM UNLOCKED.", ConsoleColor.Green);
                    Console.ResetColor();
                    System.Threading.Thread.Sleep(1500);
                    return true;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    WriteCentered("ACCESS DENIED. BOSS ONLY.", ConsoleColor.Red);
                    Console.ResetColor();
                    Console.ReadKey();
                }
            }
        }

        static bool VerifyLogin(string requiredRole)
        {
            Console.Clear();
            UI.DrawHeader($"{requiredRole.ToUpper()} LOGIN");

            Console.Write("Username: ");
            string u = Console.ReadLine().Trim();

            Console.Write("Password: ");
            string p = ReadPassword().Trim();

            if (accounts.TryGetValue(u, out var acc))
            {
                if (acc.Password.Trim() == p &&
                    (acc.Role.Trim() == requiredRole ||
                     acc.Role.Trim() == "boss" ||
                     acc.Role.Trim() == "owner"))
                {
                    return true;
                }
            }

            Console.WriteLine("Invalid Credentials.");
            Console.ReadKey();
            return false;
        }

        static double CalculateTotalStaffSalary()
        {
            LoadAccounts();

            double total = 0;
            foreach (var acc in accounts.Values)
            {
                string role = acc.Role.ToLower();
                if (role == "cashier" || role == "barista" || role == "staff")
                {
                    total += acc.Salary;
                }
            }
            return total;
        }


        static void BossDashboard(Dictionary<string, double> priceList)
        {
            bool inDashboard = true;

            while (inDashboard)
            {
                Console.Clear();
                UI.DrawHeader("EXECUTIVE DASHBOARD: BOSS");

                double totalSales = CalculateTotalSales();
                double totalExpenses = CalculateTotalExpenses();
                double totalStaffSalary = CalculateTotalStaffSalary(); 
                double netProfit = totalSales - (totalExpenses + totalStaffSalary);

                string[] alerts = File.Exists(ReportsFile) ? File.ReadAllLines(ReportsFile) : new string[0];

                Console.WriteLine($"\n--- FINANCIAL SNAPSHOT as of {DateTime.Now:yyyy-MM-dd} ---");
                UI.DrawWidget("TOTAL REVENUE", $"₱{totalSales:N2}", ConsoleColor.Green);
                UI.DrawWidget("EXPENSES", $"₱{totalExpenses:N2}", ConsoleColor.Red);
                UI.DrawWidget("STAFF SALARY", $"₱{totalStaffSalary:N2}", ConsoleColor.Yellow);
                UI.DrawWidget("NET PROFIT", $"₱{netProfit:N2}", netProfit >= 0 ? ConsoleColor.Cyan : ConsoleColor.Red);

                if (alerts.Length > 0)
                {
                    Console.WriteLine("\n--- 📩 URGENT ALERTS ---");
                    foreach (var alert in alerts) UI.DrawAlert(alert);
                }

                Console.WriteLine("\n--- MANAGEMENT FUNCTIONS ---");
                Console.WriteLine("1. Manage Menu (Prices & Items)");
                Console.WriteLine("2. Manage Inventory (View/Add/Restock)");
                Console.WriteLine("3. View Sales Report");
                Console.WriteLine("4. Clear Alerts");
                Console.WriteLine("5. Register New Staff");
                Console.WriteLine("6. Remove Staff");
                Console.WriteLine("7. Logout");
                Console.Write("\nSelect Function: ");

                string input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        BossMenuManagement(priceList);
                        break;

                    case "2":
                        BossInventoryManagement();
                        break;

                    case "3":
                        ViewSalesReport();
                        break;

                    case "4":
                        File.WriteAllText(ReportsFile, "");
                        Console.WriteLine("Cleared.");
                        Console.ReadKey();
                        break;

                    case "5":
                        Register();
                        break;

                    case "6":
                        RemoveStaff(AccountsFileName);
                        break;

                    case "7":
                        inDashboard = false;
                        break;

                    default:
                        Console.WriteLine("Invalid selection.");
                        Console.ReadKey();
                        break;
                }
            }
        }

        static void RemoveStaff(string fileName)
        {
            Console.Clear();
            UI.DrawHeader("REMOVE STAFF");

            if (!File.Exists(fileName))
            {
                Console.WriteLine("No accounts file found.");
                Console.ReadKey();
                return;
            }

            LoadAccounts();
            var staffAccounts = accounts.Values
                                        .Where(acc => acc.Role.ToLower() != "boss" && acc.Role.ToLower() != "owner")
                                        .ToList();

            if (staffAccounts.Count == 0)
            {
                Console.WriteLine("No staff found to remove.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Staff Accounts:");
            for (int i = 0; i < staffAccounts.Count; i++)
            {
                var acc = staffAccounts[i];
                Console.WriteLine($"{i + 1}. {acc.Username} ({acc.Role}) - Salary: ₱{acc.Salary:N2}");
            }

            Console.Write("\nSelect staff number to remove: ");
            if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 1 || choice > staffAccounts.Count)
            {
                Console.WriteLine("Invalid selection.");
                Console.ReadKey();
                return;
            }

            string usernameToRemove = staffAccounts[choice - 1].Username;

            accounts.Remove(usernameToRemove);

            var updatedLines = accounts.Values.Select(acc => acc.ToString()).ToList();
            File.WriteAllLines(fileName, updatedLines);

            LoadAccounts();

            Console.WriteLine($"\nStaff '{usernameToRemove}' removed successfully! Dashboard salary total will be updated.");
            Console.ReadKey();
        }


        static void BossInventoryManagement()
        {
            bool managing = true;
            while (managing)
            {
                Console.Clear();
                UI.DrawHeader("INVENTORY MANAGEMENT");
                Console.WriteLine();
                inventory.ViewStocks();
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("1. Restock EXISTING Ingredient");
                Console.WriteLine("2. Add NEW Ingredient");
                Console.WriteLine("3. Delete Ingredient");
                Console.WriteLine("4. Back to Dashboard");
                string c = Console.ReadLine();

                if (c == "1") { inventory.RestockItem("owner"); Console.ReadKey(); }
                else if (c == "2") { AddIngredientAndStock(); Console.ReadKey(); }
                else if (c == "3") { inventory.DeleteIngredientInteractive(); Console.ReadKey(); }
                else if (c == "4") managing = false;
            }
        }

        static void AddIngredientAndStock()
        {
            Console.Clear();
            UI.DrawHeader("ADD NEW INGREDIENT");

            Console.Write("Enter Category (e.g., Raw Materials, Dairy, Syrup): ");
            string category = Console.ReadLine().Trim();

            Console.Write("Enter Ingredient Name: ");
            string ingName = Console.ReadLine().Trim();

            if (inventory.IngredientsData.Values.Any(dict => dict.ContainsKey(ingName)))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nError: Ingredient '{ingName}' already exists. Use 'Restock Existing Ingredient' instead.");
                Console.ResetColor();
                return;
            }

            Console.Write("Enter unit (grams / ml / pieces): ");
            string unit = Console.ReadLine().Trim().ToLower();
            if (unit != "grams" && unit != "ml" && unit != "pieces")
            {
                Console.WriteLine("Invalid unit. Choose one of: grams / ml / pieces");
                return;
            }

            Console.Write("Enter starting quantity (in chosen unit) [e.g., 5000]: ");
            if (!int.TryParse(Console.ReadLine(), out int qty) || qty <= 0)
            {
                Console.WriteLine("Invalid quantity. Must be a positive number.");
                return;
            }

            inventory.AddOrUpdateNewIngredient(category, ingName, qty, unit);
            inventory.SaveInventory();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nSUCCESS: Ingredient '{ingName}' added to inventory under '{category}' with {qty:N0} {unit} stock.");
            Console.ResetColor();
        }

        static void BossMenuManagement(Dictionary<string, double> priceList)
        {
            bool managing = true;
            while (managing)
            {
                Console.Clear();
                UI.DrawHeader("MENU MANAGEMENT");
                ViewMenu(priceList);
                Console.WriteLine("\n1. Add Item\n2. Update Price\n3. Remove Item\n4. View Recipe\n5. Back");
                string c = Console.ReadLine();
                switch (c)
                {
                    case "1": AddItem(priceList); break;
                    case "2": UpdatePrice(priceList); break;
                    case "3": DeleteItem(priceList); break;
                    case "4": ViewRecipes(); break;
                    case "5": managing = false; break;
                }
            }
        }

        static void CashierMenu(Dictionary<string, double> priceList, List<Sale> sales)
        {
            bool active = true;
            while (active)
            {
                Console.Clear();
                UI.DrawHeader("CASHIER TERMINAL");
                Console.WriteLine("1. Record New Sale");
                Console.WriteLine("2. Check Item Availability");
                Console.WriteLine("3. View Inventory (Read-Only)");
                Console.WriteLine("4. Report Issue");
                Console.WriteLine("5. Logout");
                string c = Console.ReadLine();
                switch (c)
                {
                    case "1": RecordSale(priceList, sales, "Cashier"); break;
                    case "2": CheckAvailability(priceList); break;
                    case "3": Console.Clear(); inventory.ViewStocks(); Console.ReadKey(); break;
                    case "4": ReportIssueToBoss(); break;
                    case "5": active = false; break;
                }
            }
        }

        static void CustomerKiosk(Dictionary<string, double> priceList, List<Sale> sales)
        {
            bool ordering = true;
            while (ordering)
            {
                Console.Clear();
                UI.DrawHeader("SELF-ORDER");
                Console.WriteLine("Welcome! Please place your order.\n");
                RecordSale(priceList, sales, "Kiosk-Customer");
                Console.WriteLine("\nOrder another? (y/n)");
                if (Console.ReadLine().ToLower() != "y")
                {
                    Console.Clear();
                    UI.DrawWidget("ORDER SENT", "PLEASE WAIT", ConsoleColor.Green);
                    Console.ReadKey();
                    ordering = false;
                }
            }
        }

        static void BaristaMenu()
        {
            bool active = true;
            while (active)
            {
                Console.Clear();
                UI.DrawHeader("BARISTA STATION: KITCHEN DISPLAY");
                List<string> allLines = File.Exists("sales.txt") ? File.ReadAllLines("sales.txt").ToList() : new List<string>();

                Console.WriteLine("\n--- ⏳ PENDING ORDERS ---");
                int count = 0;
                foreach (var line in allLines)
                {
                    if (line.Contains("[PENDING]")) { Console.ForegroundColor = ConsoleColor.Yellow; Console.WriteLine(line); count++; }
                }
                Console.ResetColor();
                if (count == 0) Console.WriteLine("(No pending orders)");

                Console.WriteLine("\n1. Mark Order COMPLETE\n2. View Recipes\n3. Check Inventory\n4. Refresh\n5. Logout");
                string c = Console.ReadLine();
                if (c == "1") MarkOrderComplete(allLines);
                else if (c == "2") ViewRecipes();
                else if (c == "3") { Console.Clear(); inventory.ViewStocks(); Console.ReadKey(); }
                else if (c == "5") active = false;
            }
        }

        static void RecordSale(Dictionary<string, double> priceList, List<Sale> sales, string operatorName)
        {
            List<CartItem> currentCart = new List<CartItem>();
            bool ordering = true;
            double grandTotal = 0;

            while (ordering)
            {
                Console.Clear();
                UI.DrawHeader($"NEW ORDER: {operatorName.ToUpper()}");

                if (currentCart.Count > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("--- BASKET ---");
                    foreach (var c in currentCart) Console.WriteLine($" > {c.Name,-20} x{c.Qty} = ₱{c.Total:N2}");
                    Console.WriteLine($" SUBTOTAL: ₱{currentCart.Sum(x => x.Total):N2}\n----------------------");
                    Console.ResetColor();
                }

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("{0,-5} | {1,-25} | {2,10} | {3,-12}", "No", "Item Name", "Price", "Status");
                Console.WriteLine(new string('=', 60));
                Console.ResetColor();

                int i = 1;
                var keys = priceList.Keys.ToList();
                foreach (var item in priceList)
                {
                    bool isAvailable = inventory.IsItemAvailable(item.Key, ItemRecipes);
                    Console.Write(String.Format("{0,-5} | {1,-25} | {2,10} | ", i++, item.Key, "₱" + item.Value.ToString("N2")));
                    if (isAvailable) { Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine("AVAILABLE"); }
                    else { Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine("OUT OF STOCK"); }
                    Console.ResetColor();
                }
                Console.WriteLine(new string('-', 60));

                Console.WriteLine("[0] FINALIZE & PAY");
                Console.Write("\nEnter item number: ");
                string input = Console.ReadLine();
                if (input == "0") { if (currentCart.Count > 0) ordering = false; continue; }

                if (!int.TryParse(input, out int choice) || choice < 1 || choice > keys.Count) continue;
                string itemName = keys[choice - 1];

                if (!inventory.IsItemAvailable(itemName, ItemRecipes)) { UI.DrawAlert("OUT OF STOCK"); Console.ReadKey(); continue; }

                Console.Write($"Quantity: ");
                if (!int.TryParse(Console.ReadLine(), out int qty) || qty <= 0) continue;

                currentCart.Add(new CartItem { Name = itemName, Qty = qty, Price = priceList[itemName] });
            }

            foreach (var item in currentCart)
            {
                if (!Program.inventory.DeductIngredients(item.Name, item.Qty, ItemRecipes)) continue;
                grandTotal += item.Total;
                Sale sale = new Sale(item.Name, item.Qty, item.Total, DateTime.Now, operatorName);
                sale.RecordTransaction();
                sales.Add(sale);
            }

            string receipt = "\n";
            receipt += "==========================================\n";
            receipt += "                CAFE RECEIPT              \n";
            receipt += "==========================================\n";
            receipt += $"Date: {DateTime.Now:M/d/yyyy hh:mm:ss tt}\n";
            receipt += $"Served By: {operatorName}\n";
            receipt += "------------------------------------------\n";
            receipt += $"{"Item",-20} {"Qty",5} {"Total",10}      \n";
            receipt += "------------------------------------------\n";

            foreach (var item in currentCart)
            {
                receipt += $"{item.Name,-20} {item.Qty,5} {"₱" + item.Total.ToString("N2"),10}\n";
            }

            receipt += "==========================================\n";

            const int TotalWidth = 32;
            string totalLabel = "GRAND TOTAL:";
            string totalValue = "₱" + grandTotal.ToString("N2");

            int paddingSpace = TotalWidth - totalLabel.Length - totalValue.Length;

            receipt += $"{totalLabel}{string.Empty.PadLeft(paddingSpace)}{totalValue}\n";

            receipt += "===========================================\n";
            receipt += "         THANK YOU FOR YOUR ORDER!         \n";
            receipt += "===========================================\n\n";


            File.AppendAllText("all_receipts.txt", receipt);
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(receipt);
            Console.ResetColor();
            Console.WriteLine("(Receipt saved). Press any key...");
            Console.ReadKey();
        }

        static void MarkOrderComplete(List<string> allLines)
        {
            Console.Write("Enter Sale ID to Complete: ");
            string id = Console.ReadLine();
            bool found = false;
            List<string> updates = new List<string>();
            foreach (var line in allLines)
            {
                if (line.StartsWith($"Sale #{id}") && line.Contains("[PENDING]"))
                {
                    updates.Add(line.Replace("[PENDING]", "[COMPLETED] ✅"));
                    found = true;
                }
                else updates.Add(line);
            }
            if (found) { File.WriteAllLines("sales.txt", updates); Console.WriteLine("Completed!"); }
            else Console.WriteLine("Order not found.");
            Console.ReadKey();
        }

        static void ViewRecipes()
        {
            Console.Clear();
            UI.DrawHeader("MENU RECIPES");
            Console.WriteLine();
            foreach (var r in ItemRecipes) 
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[{r.Key}]");
                Console.ResetColor();

                foreach (var ing in r.Value)
                {
                    string unit = "units";

                    foreach (var cat in inventory.IngredientsData.Values)
                    {
                        if (cat.ContainsKey(ing.Key))
                        {
                            unit = cat[ing.Key].Unit;
                            break;
                        }
                    }
                    Console.WriteLine($" - {ing.Key}: {ing.Value} {unit}");
                }
                Console.WriteLine(new string('-', 30));
            }
            Console.ReadKey();
        }

        static void CheckAvailability(Dictionary<string, double> priceList)
        {
            Console.Clear();
            UI.DrawHeader("AVAILABILITY CHECK");
            foreach (var item in priceList)
            {
                bool avail = inventory.IsItemAvailable(item.Key, ItemRecipes);
                Console.WriteLine($"{item.Key,-20} : " + (avail ? "AVAILABLE" : "OUT OF STOCK"));
            }
            Console.ReadKey();
        }

        static void ReportIssueToBoss()
        {
            Console.Write("Issue: ");
            File.AppendAllText(ReportsFile, $"[CASHIER ALERT] {DateTime.Now}: {Console.ReadLine()}\n");
            Console.WriteLine("Sent."); Console.ReadKey();
        }

        static void ViewMenu(Dictionary<string, double> priceList)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n{0,-5} | {1,-25} | {2,10} | {3,-12}", "No", "Item Name", "Price", "Status");
            Console.WriteLine(new string('=', 60));
            Console.ResetColor();
            int i = 1;
            foreach (var item in priceList)
            {
                bool avail = inventory.IsItemAvailable(item.Key, ItemRecipes);
                Console.Write(String.Format("{0,-5} | {1,-25} | {2,10} | ", i++, item.Key, "₱" + item.Value.ToString("N2")));
                if (avail) { Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine("AVAILABLE"); }
                else { Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine("OUT OF STOCK"); }
                Console.ResetColor();
            }
        }

        static void AddItem(Dictionary<string, double> priceList)
        {
            Console.Write("Name of new item: ");
            string name = Console.ReadLine().Trim();

            if (priceList.ContainsKey(name))
            {
                Console.WriteLine($"Item '{name}' already exists in the menu.");
                Console.ReadKey();
                return;
            }

            Console.Write("Price: ");
            if (!double.TryParse(Console.ReadLine(), out double price) || price <= 0)
            {
                Console.WriteLine("Invalid price. Must be a positive number.");
                Console.ReadKey();
                return;
            }

            priceList[name] = price;
            SaveMenu(priceList);

            Console.Write("Add recipe/ingredients now? (y/n): ");
            if (Console.ReadLine().ToLower() == "y")
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n--- Defining Recipe for {name} ---");
                Console.ResetColor();

                var recipe = BuildRecipeForNewItem();
                if (recipe.Count > 0)
                {
                    ItemRecipes[name] = recipe;
                    SaveRecipes();
                }
                else
                {
                    Console.WriteLine("Recipe creation cancelled or failed.");
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Item '{name}' added successfully to menu and recipes.");
            Console.ResetColor();
            Console.ReadKey();
        }

        static Dictionary<string, int> BuildRecipeForNewItem()
        {
            var recipe = new Dictionary<string, int>();

            while (true)
            {
                Console.Write("\nEnter ingredient name (or type 'done'): ");
                string ingName = Console.ReadLine().Trim();

                if (ingName.ToLower() == "done" || string.IsNullOrWhiteSpace(ingName))
                    break;

                bool exists = inventory.IngredientsData.Values.Any(dict => dict.ContainsKey(ingName));

                if (!exists)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: Ingredient '{ingName}' is not in the inventory. Please add it via the Inventory Management menu first.");
                    Console.ResetColor();
                    Console.ReadKey();
                    return new Dictionary<string, int>();
                }

                string unit = "units";
                var cat = inventory.IngredientsData.First(d => d.Value.ContainsKey(ingName));
                unit = cat.Value[ingName].Unit;

                Console.Write($"Enter amount needed per serving for {ingName} ({unit}): ");

                if (!int.TryParse(Console.ReadLine(), out int amount) || amount <= 0)
                {
                    Console.WriteLine("Invalid amount. Try again.");
                    continue;
                }

                recipe[ingName] = amount;
            }

            return recipe;
        }


        static void UpdatePrice(Dictionary<string, double> priceList)
        {
            Console.Write("Name: "); string name = Console.ReadLine();
            if (priceList.ContainsKey(name))
            {
                Console.Write("New Price: ");
                if (double.TryParse(Console.ReadLine(), out double p))
                {
                    priceList[name] = p;
                    SaveMenu(priceList);
                }
            }
        }

        static void DeleteItem(Dictionary<string, double> priceList)
        {
            Console.Write("Name to remove: ");
            string name = Console.ReadLine();
            if (priceList.Remove(name))
            {
                ItemRecipes.Remove(name);
                SaveMenu(priceList);
                SaveRecipes();
                Console.WriteLine($"Item and recipe for '{name}' deleted.");
            }
            else
            {
                Console.WriteLine("Item not found.");
            }
            Console.ReadKey();
        }

        static double CalculateTotalSales()
        {
            double total = 0;
            if (File.Exists("sales.txt"))
            {
                foreach (var line in File.ReadAllLines("sales.txt"))
                {
                    if (line.Contains("₱"))
                    {
                        try
                        {
                            string money = line.Split('₱')[1].Split(' ')[0].Replace(",", "");
                            if (double.TryParse(money, out double amount)) total += amount;
                        }
                        catch { }
                    }
                }
            }
            return total;
        }

        static double CalculateTotalExpenses()
        {
            double total = 0;
            if (File.Exists("expenses.txt"))
            {
                foreach (var line in File.ReadAllLines("expenses.txt"))
                {
                    if (line.Contains("₱"))
                    {
                        try
                        {
                            int idx = line.IndexOf('₱');
                            string part = line.Substring(idx + 1).Trim();
                            var cleaned = new string(part.TakeWhile(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());
                            cleaned = cleaned.Replace(",", "");
                            if (double.TryParse(cleaned, out double amount)) total += amount;
                        }
                        catch { }
                    }
                }
            }
            return total;
        }

        static void ViewSalesReport()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=================================");
                Console.WriteLine("        SALES REPORT MENU");
                Console.WriteLine("=================================");
                Console.WriteLine("1) Weekly Sales Report");
                Console.WriteLine("2) Monthly Sales Report");
                Console.WriteLine("3) Yearly Sales Report");
                Console.WriteLine("4) Back");
                Console.Write("Choose: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        GenerateSalesReport(TimeFrame.Weekly);
                        break;
                    case "2":
                        GenerateSalesReport(TimeFrame.Monthly);
                        break;
                    case "3":
                        GenerateSalesReport(TimeFrame.Yearly);
                        break;
                    case "4":
                        return;
                    default:
                        Console.WriteLine("Invalid option...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        static void GenerateSalesReport(TimeFrame tf)
        {
            Console.Clear();

            if (!File.Exists("sales.txt"))
            {
                Console.WriteLine("No sales data found.");
                Console.ReadKey();
                return;
            }

            var allSalesLines = File.ReadAllLines("sales.txt").ToList();
            var filteredSalesLines = new List<string>();
            DateTime now = DateTime.Now;

            // Filter sales based on timeframe
            foreach (var line in allSalesLines)
            {
                var parts = line.Split('-');
                if (parts.Length < 3) continue;

                string dateTimePart = parts[1].Trim();
                if (!DateTime.TryParse(dateTimePart, out DateTime dt)) continue;

                bool include = false;
                switch (tf)
                {
                    case TimeFrame.Weekly:
                        // Current week (Monday-Sunday)
                        var diff = (7 + (dt.DayOfWeek - DayOfWeek.Monday)) % 7;
                        var weekStart = dt.AddDays(-diff).Date;
                        var weekEnd = weekStart.AddDays(6);
                        if (dt.Date >= weekStart && dt.Date <= weekEnd) include = true;
                        break;

                    case TimeFrame.Monthly:
                        if (dt.Month == now.Month && dt.Year == now.Year) include = true;
                        break;

                    case TimeFrame.Yearly:
                        if (dt.Year == now.Year) include = true;
                        break;
                }

                if (include) filteredSalesLines.Add(line);
            }

            if (filteredSalesLines.Count == 0)
            {
                Console.WriteLine("No sales found for the selected period.");
                Console.ReadKey();
                return;
            }

            // Count items and calculate dynamic column width
            var itemCount = new Dictionary<string, int>();
            int itemColumnWidth = 4; // Minimum width
            foreach (var line in filteredSalesLines)
            {
                var parts = line.Split('-');
                if (parts.Length < 3) continue;
                string itemPart = parts[2].Trim();
                string[] itemSplit = itemPart.Split('x');
                if (itemSplit.Length < 2) continue;
                string itemName = itemSplit[0].Trim();
                if (itemName.Length > itemColumnWidth) itemColumnWidth = itemName.Length;
            }

            // Build table header
            string header = "| DATE       | TIME     | " + "ITEM".PadRight(itemColumnWidth) + " | QTY |";
            string separator = new string('-', header.Length);

            var tableBuilder = new StringBuilder();
            tableBuilder.AppendLine(separator);
            tableBuilder.AppendLine(header);
            tableBuilder.AppendLine(separator);

            double totalSales = 0;

            foreach (var line in filteredSalesLines)
            {
                var parts = line.Split('-');
                if (parts.Length < 3) continue;

                string dateTimePart = parts[1].Trim();
                if (!DateTime.TryParse(dateTimePart, out DateTime dt)) continue;
                string dateStr = dt.ToString("MM/dd/yyyy");
                string timeStr = dt.ToString("hh:mm tt");

                string itemPart = parts[2].Trim();
                string[] itemSplit = itemPart.Split('x');
                if (itemSplit.Length < 2) continue;
                string itemName = itemSplit[0].Trim();
                string qtyStr = itemSplit[1].Split('=')[0].Trim();
                int qty = 0;
                int.TryParse(qtyStr, out qty);

                // --- FIXED PRICE PARSING ---
                double price = 0;
                string[] priceSplit = itemPart.Split('=');
                if (priceSplit.Length > 1)
                {
                    string priceText = priceSplit[1].Trim(); // e.g., "₱90 (By: Cashier) | [COMPLETED]"
                    string firstPart = priceText.Split(' ')[0]; // "₱90"
                    firstPart = firstPart.Replace("₱", "").Replace(",", "").Trim(); // "90"
                    double.TryParse(firstPart, out price);
                }
                totalSales += price;
                // ----------------------------

                tableBuilder.AppendLine($"| {dateStr,-10} | {timeStr,-8} | {itemName.PadRight(itemColumnWidth)} | {qty,-3} |");

                if (!itemCount.ContainsKey(itemName)) itemCount[itemName] = 0;
                itemCount[itemName] += qty;
            }

            // Best-selling
            var best = itemCount.OrderByDescending(x => x.Value).FirstOrDefault();
            string bestSellingItem = best.Key ?? "N/A";
            int bestSellingQty = best.Value;

            tableBuilder.AppendLine(separator);
            tableBuilder.AppendLine($"TOTAL SALES: ₱{totalSales:N2}");
            tableBuilder.AppendLine($"BEST-SELLING: {bestSellingItem} ({bestSellingQty})");
            tableBuilder.AppendLine(separator);

            string table = tableBuilder.ToString();

            // Report header for weekly/monthly/yearly
            string periodHeader = "";
            string fileName = "";
            switch (tf)
            {
                case TimeFrame.Weekly:
                    periodHeader = "WEEKLY SALES REPORT";
                    fileName = "weekly_report.txt";
                    break;
                case TimeFrame.Monthly:
                    periodHeader = $"MONTH: {now:MMMM}";
                    fileName = "monthly_report.txt";
                    break;
                case TimeFrame.Yearly:
                    periodHeader = $"YEAR: {now:yyyy}";
                    fileName = "yearly_report.txt";
                    break;
            }

            string reportText =
        $@"========================================
              SALES REPORT
========================================
{periodHeader}
----------------------------------------
{table}
========================================
";

            File.WriteAllText(fileName, reportText);

            Console.WriteLine(reportText);
            Console.WriteLine($"Report saved to: {fileName}");
            Console.WriteLine("Press any key to return...");
            Console.ReadKey();
        }


        static void EnsureFileExists(string fileName)
        {
            if (!File.Exists(fileName)) File.WriteAllText(fileName, string.Empty);
        }

        static void LoadAccounts()
        {
            accounts.Clear();
            if (!File.Exists(AccountsFileName)) return;

            var lines = File.ReadAllLines(AccountsFileName);

            if (lines.Length == 0)
            {
                File.WriteAllText(AccountsFileName, "boss|admin|1234" + Environment.NewLine);
                lines = File.ReadAllLines(AccountsFileName);
            }

            foreach (var rawLine in lines)
            {
                if (string.IsNullOrWhiteSpace(rawLine)) continue;

                var parts = rawLine.Split('|');

                if (parts.Length < 3) continue;

                string role = parts[0].Trim();
                string username = parts[1].Trim();
                string password = parts[2].Trim();
                double salary = 0.0;

                if (parts.Length >= 4)
                {
                    double parsed;
                    if (double.TryParse(parts[3].Trim(), out parsed)) salary = parsed;
                }

                accounts[username] = new Account
                {
                    Role = role,
                    Username = username,
                    Password = password,
                    Salary = salary
                };
            }
        }

        static void SaveAccountToFile(Account acc)
        {
            File.AppendAllLines(AccountsFileName, new[] { acc.ToString() });
        }


        static Dictionary<string, double> LoadMenu()
        {
            var menu = new Dictionary<string, double>();
            if (!File.Exists(MenuFile) || new FileInfo(MenuFile).Length == 0)
            {
                menu = new Dictionary<string, double>
            {
                { "Hot Coffee", 90 }, { "Iced Coffee", 100 }, { "Cafe Latte", 120 },
                { "Iced Latte", 130 }, { "Brown Sugar Latte", 140 }, { "Sweetened Milk Coffee", 110 },
                { "Hot Matcha Latte", 130 }, { "Iced Matcha Latte", 140 }, { "Matcha Milk", 120 },
                { "Hot Chocolate", 110 }, { "Iced Chocolate", 120 }, { "Chocolate Milk", 100 }
            };
                SaveMenu(menu);
            }
            else
            {
                foreach (var line in File.ReadAllLines(MenuFile))
                {
                    var parts = line.Split('|');
                    if (parts.Length == 2 && double.TryParse(parts[1], out double price))
                    {
                        menu[parts[0]] = price;
                    }
                }
            }
            return menu;
        }

        static void SaveMenu(Dictionary<string, double> menu)
        {
            var lines = new List<string>();
            foreach (var item in menu) lines.Add($"{item.Key}|{item.Value}");
            File.WriteAllLines(MenuFile, lines);
        }

        static void LoadRecipes()
        {
            ItemRecipes = new Dictionary<string, Dictionary<string, int>>();
            if (!File.Exists(RecipesFile) || new FileInfo(RecipesFile).Length == 0)
            {
                ItemRecipes = new Dictionary<string, Dictionary<string, int>>
            {
                { "Hot Coffee", new Dictionary<string, int> { { "Coffee Beans", 15 }, { "Cups", 1 }, { "Lids", 1 } } },
                { "Iced Coffee", new Dictionary<string, int> { { "Coffee Beans", 15 }, { "Sugar", 10 }, { "Cups", 1 }, { "Lids", 1 }, { "Straws", 1 } } },
                { "Cafe Latte", new Dictionary<string, int> { { "Coffee Beans", 20 }, { "Milk", 150 }, { "Cups", 1 }, { "Lids", 1 } } },
                { "Iced Latte", new Dictionary<string, int> { { "Coffee Beans", 20 }, { "Milk", 120 }, { "Sugar", 10 }, { "Cups", 1 }, { "Lids", 1 }, { "Straws", 1 } } },
                { "Brown Sugar Latte", new Dictionary<string, int> { { "Coffee Beans", 20 }, { "Milk", 150 }, { "Sugar", 25 }, { "Cups", 1 }, { "Lids", 1 }, { "Straws", 1 } } },
                { "Sweetened Milk Coffee", new Dictionary<string, int> { { "Coffee Beans", 15 }, { "Milk", 50 }, { "Sugar", 15 }, { "Cups", 1 }, { "Lids", 1 } } },
                { "Hot Matcha Latte", new Dictionary<string, int> { { "Matcha Powder", 15 }, { "Milk", 200 }, { "Sugar", 10 }, { "Cups", 1 }, { "Lids", 1 } } },
                { "Iced Matcha Latte", new Dictionary<string, int> { { "Matcha Powder", 15 }, { "Milk", 150 }, { "Sugar", 15 }, { "Cups", 1 }, { "Lids", 1 }, { "Straws", 1 } } },
                { "Matcha Milk", new Dictionary<string, int> { { "Matcha Powder", 10 }, { "Milk", 250 }, { "Cups", 1 }, { "Lids", 1 }, { "Straws", 1 } } },
                { "Hot Chocolate", new Dictionary<string, int> { { "Cocoa Powder", 30 }, { "Milk", 200 }, { "Sugar", 15 }, { "Cups", 1 }, { "Lids", 1 } } },
                { "Iced Chocolate", new Dictionary<string, int> { { "Cocoa Powder", 30 }, { "Milk", 150 }, { "Sugar", 15 }, { "Cups", 1 }, { "Lids", 1 }, { "Straws", 1 } } },
                { "Chocolate Milk", new Dictionary<string, int> { { "Cocoa Powder", 15 }, { "Milk", 250 }, { "Sugar", 10 }, { "Cups", 1 }, { "Lids", 1 }, { "Straws", 1 } } }
            };
                SaveRecipes();
            }
            else
            {
                foreach (var line in File.ReadAllLines(RecipesFile))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split('|');
                    if (parts.Length == 2)
                    {
                        string name = parts[0];
                        var ingList = new Dictionary<string, int>();
                        foreach (var raw in parts[1].Split(';'))
                        {
                            var split = raw.Split('=');
                            if (split.Length == 2 && int.TryParse(split[1], out int qty)) ingList[split[0]] = qty;
                        }
                        ItemRecipes[name] = ingList;
                    }
                }
            }
        }

        static void SaveRecipes()
        {
            List<string> lines = new List<string>();
            foreach (var item in ItemRecipes)
            {
                string line = item.Key + "|";
                List<string> parts = new List<string>();
                foreach (var ing in item.Value) parts.Add($"{ing.Key}={ing.Value}");
                line += string.Join(";", parts);
                lines.Add(line);
            }
            File.WriteAllLines(RecipesFile, lines);
        }

        static void DisplayWelcome()
        {
            Console.ForegroundColor = ConsoleColor.Green;

            string[] welcomeArt =
            {
        "████████████████████████████████████████████████████████████████████████",
        "██                                                                    ██",
        "██    ██╗    ██╗███████╗██╗      ██████╗ ██████╗ ███╗   ███╗███████╗  ██",
        "██    ██║    ██║██╔════╝██║     ██╔════╝██╔═══██╗████╗ ████║██╔════╝  ██",
        "██    ██║ █╗ ██║█████╗  ██║     ██║     ██║   ██║██╔████╔██║█████╗    ██",
        "██    ██║███╗██║██╔══╝  ██║     ██║     ██║   ██║██║╚██╔╝██║██╔══╝    ██",
        "██    ╚███╔███╔╝███████╗███████╗╚██████╗╚██████╔╝██║ ╚═╝ ██║███████╗  ██",
        "██     ╚══╝╚══╝ ╚══════╝╚══════╝ ╚═════╝ ╚═════╝ ╚═╝     ╚═╝╚══════╝  ██",
        "██                                                                    ██",
        "████████████████████████████████████████████████████████████████████████"
    };

            Console.Clear();
            Console.WriteLine();

            foreach (string line in welcomeArt)
            {
                WriteCentered(line, ConsoleColor.Green);
            }

            Console.WriteLine();
            Console.ResetColor();
        }


        static void WriteCentered(string text, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            int padding = (Console.WindowWidth - text.Length) / 2;
            if (padding < 0) padding = 0;
            Console.WriteLine(new string(' ', padding) + text);
            Console.ResetColor();
        }

        static void WriteCenteredLine(string text, ConsoleColor color = ConsoleColor.White)
        {
            if (text == null) text = "";

            int width;
            try { width = Console.WindowWidth; }
            catch { width = 80; }

            int left = (width - text.Length) / 2;
            if (left < 0) left = 0;

            Console.ForegroundColor = color;
            try
            {
                int row = Console.CursorTop;
                if (Console.CursorLeft != 0) row = Console.CursorTop + 1;

                if (row < 0) row = 0;
                if (row >= Console.BufferHeight) row = Console.BufferHeight - 1;

                Console.SetCursorPosition(left, row);
                Console.WriteLine(text);
            }
            catch
            {
                Console.WriteLine(new string(' ', left) + text);
            }
            Console.ResetColor();
        }


        static void Register()
        {
            Console.Clear();
            DisplayWelcome();
            Console.WriteLine();
            WriteCentered("*** NEW ACCOUNT REGISTRATION ***", ConsoleColor.Yellow);

            WriteCentered("Role (boss/cashier/barista): ", ConsoleColor.White);
            Console.SetCursorPosition((Console.WindowWidth - 30) / 2, Console.CursorTop - 1);
            Console.Write("Role (boss/cashier/barista): ");
            string role = Console.ReadLine().ToLower();
            if (role != "boss" && role != "cashier" && role != "barista" && role != "owner" && role != "staff") { WriteCentered("Invalid Role!", ConsoleColor.Red); Console.ReadKey(); return; }

            WriteCentered("Username: ", ConsoleColor.White);
            Console.SetCursorPosition((Console.WindowWidth - 10) / 2, Console.CursorTop - 1);
            Console.Write("Username: ");
            string u = Console.ReadLine();
            if (accounts.ContainsKey(u)) { WriteCentered("Username taken!", ConsoleColor.Red); Console.ReadKey(); return; }

            WriteCentered("Password: ", ConsoleColor.White);
            Console.SetCursorPosition((Console.WindowWidth - 10) / 2, Console.CursorTop - 1);
            Console.Write("Password: ");
            string p = ReadPassword();

            double salary = 0.0;
            if (role != "boss" && role != "owner")
            {
                Console.Write("Enter salary for this staff (numeric, e.g. 1500): ");
                string salInput = Console.ReadLine();
                if (!double.TryParse(salInput, out salary))
                {
                    salary = 0.0;
                }
            }
            else
            {
                salary = 0.0;
            }

            var acc = new Account { Role = role, Username = u, Password = p, Salary = salary };
            accounts[u] = acc;
            SaveAccountToFile(acc);

            Console.WriteLine($"\nRegistered salary for {u}: ₱{salary:N2}");
            WriteCentered("Account Created!", ConsoleColor.Cyan);
            Console.ReadKey();
        }

        static void CenterText(string text)
        {
            int screenWidth = Console.WindowWidth;
            int textWidth = text.Length;
            int leftPadding = (screenWidth - textWidth) / 2;
            Console.SetCursorPosition(leftPadding, Console.CursorTop);
            Console.WriteLine(text);
        }
    }
}