using System.Globalization;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using NationalBank.Models;

namespace NationalBank
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // One DbContext (one connection) for the whole menu session.
            // "using" disposes it (closes the connection) when we leave Main.
            using APPDbContext dbContext = new APPDbContext();

            try
            {
                await SeedSampleCustomersAsync(dbContext);
            }
            catch (Exception ex)
            {
                WriteLineColor($"Cannot connect to the database: {ex.Message}", ConsoleColor.Red);
                WriteLineColor("Make sure SQL Server is running and you ran Update-Database.", ConsoleColor.Red);
                return;
            }

            #region Interactive Menu
            while (true)
            {
                PrintMenu();
                int choice = ReadChoice("  Enter choice: ", 0, 5);

                if (choice == 0)
                    break; // Exit

                Console.Clear();
                try
                {
                    switch (choice)
                    {
                        case 1: await AddCustomerAsync(dbContext); break;
                        case 2: await OpenAccountAsync(dbContext); break;
                        case 3: await UpdateAccountStatusAsync(dbContext); break;
                        case 4: await RemoveAccountFromCustomerAsync(dbContext); break;
                        case 5: await ListCustomersAsync(dbContext); break;
                    }
                }
                catch (DbUpdateException ex)
                {
                    // Database refused the change: clear tracked changes so the next operation starts clean
                    dbContext.ChangeTracker.Clear();
                    WriteLineColor($"Database error: {ex.InnerException?.Message ?? ex.Message}", ConsoleColor.Red);
                }
                catch (Exception ex)
                {
                    dbContext.ChangeTracker.Clear();
                    WriteLineColor($"Unexpected error: {ex.Message}", ConsoleColor.Red);
                }

                Console.WriteLine();
                Console.Write("Press any key to return to the menu...");
                Console.ReadKey(true);
            }
            #endregion

            Console.WriteLine();
            WriteLineColor("Connection closed. Goodbye!", ConsoleColor.Yellow);
        }

        #region Menu
        private static void PrintMenu()
        {
            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("    National Bank - Management");
            Console.WriteLine("========================================");
            Console.WriteLine("  1) Add a new Customer");
            Console.WriteLine("  2) Open a new Account for a Customer");
            Console.WriteLine("  3) Update Account Status (Active / Closed)");
            Console.WriteLine("  4) Remove an Account from a Customer");
            Console.WriteLine("  5) List all Customers (with accounts)");
            Console.WriteLine("  0) Exit");
            Console.WriteLine("----------------------------------------");
        }
        #endregion

        #region 1) Add a new Customer
        private static async Task AddCustomerAsync(APPDbContext dbContext)
        {
            Console.WriteLine("--- Add New Customer ---");

            string fullName = ReadText("Full Name      : ", 100, V => V.Length >= 3 ? null : "Name must be at least 3 characters.");

            string nationalId = ReadText("National ID    : ", 20, V =>
                V.All(char.IsDigit) && V.Length >= 10 && V.Length <= 14 ? null : "National ID must be 10 to 14 digits.");
            if (await dbContext.Customers.AnyAsync(C => C.NationalId == nationalId))
            {
                WriteLineColor($"A customer with National ID '{nationalId}' already exists.", ConsoleColor.Red);
                return;
            }

            DateTime dateOfBirth = ReadDate("Date of Birth  : (yyyy-MM-dd) ");

            string email = ReadText("Email          : ", 150, V => IsValidEmail(V) ? null : "Invalid email address.");

            string phone = ReadText("Phone          : ", 20, V =>
                V.All(char.IsDigit) && V.Length >= 8 && V.Length <= 15 ? null : "Phone must be 8 to 15 digits.");

            string address = ReadText("Address        : ", 200, V => V.Length >= 3 ? null : "Address is too short.");

            Console.WriteLine("Customer Type:");
            Console.WriteLine("    1) Individual");
            Console.WriteLine("    2) Business");
            CustomerType customerType = (CustomerType)ReadChoice("  Choice: ", 1, 2);

            Customer customer = new Customer()
            {
                FullName = fullName,
                NationalId = nationalId,
                DateOfBirth = dateOfBirth,
                Email = email,
                PhoneNumber = phone,
                Address = address,
                CustomerType = customerType
            };

            dbContext.Customers.Add(customer);
            await dbContext.SaveChangesAsync();

            Console.WriteLine();
            WriteLineColor($"Customer created successfully. CustomerId = {customer.Id}", ConsoleColor.Green);
        }
        #endregion

        #region 2) Open a new Account for a Customer
        private static async Task OpenAccountAsync(APPDbContext dbContext)
        {
            Console.WriteLine("--- Open New Account ---");

            string accountNumber = ReadText("Account Number : ", 20, V =>
                V.All(Ch => char.IsLetterOrDigit(Ch) || Ch == '-') ? null : "Use letters, digits or '-' only.");

            // Joint accounts: if the account already exists, the customer is added as another owner
            Account? existingAccount = await dbContext.Accounts.FindAsync(accountNumber);

            AccountType accountType = AccountType.Savings;
            string branchCode;
            if (existingAccount is null)
            {
                Console.WriteLine("Account Type:");
                Console.WriteLine("    1) Savings");
                Console.WriteLine("    2) Current");
                Console.WriteLine("    3) Business");
                accountType = (AccountType)ReadChoice("  Choice: ", 1, 3);

                branchCode = ReadText("Branch Code    : ", 10).ToUpper();
            }
            else
            {
                branchCode = existingAccount.BranchCode;
                WriteLineColor($"Account '{accountNumber}' already exists (branch {branchCode}) -> the customer will be added as a joint owner.", ConsoleColor.Cyan);
            }

            int customerId = ReadInt("Customer Id    : ");

            Console.WriteLine("Ownership Role:");
            Console.WriteLine("    1) Primary");
            Console.WriteLine("    2) CoHolder");
            OwnershipType role = (OwnershipType)ReadChoice("  Choice: ", 1, 2);

            // Verify that the branch and the customer exist BEFORE creating anything
            WriteLineColor($"Validating branch '{branchCode}' and customer #{customerId}...", ConsoleColor.Cyan);

            if (!await dbContext.Branches.AnyAsync(B => B.Code == branchCode))
            {
                WriteLineColor($"Branch '{branchCode}' does not exist. Account was not created.", ConsoleColor.Red);
                return;
            }
            if (!await dbContext.Customers.AnyAsync(C => C.Id == customerId))
            {
                WriteLineColor($"Customer #{customerId} does not exist. Account was not created.", ConsoleColor.Red);
                return;
            }

            if (existingAccount is not null)
            {
                if (await dbContext.CustomerAccounts.AnyAsync(CA => CA.AccountNumber == accountNumber && CA.CustomerId == customerId))
                {
                    WriteLineColor($"Customer #{customerId} is already an owner of account '{accountNumber}'.", ConsoleColor.Red);
                    return;
                }
                if (role == OwnershipType.Primary &&
                    await dbContext.CustomerAccounts.AnyAsync(CA => CA.AccountNumber == accountNumber && CA.OwnershipType == OwnershipType.Primary))
                {
                    WriteLineColor($"Account '{accountNumber}' already has a Primary owner. Add this customer as CoHolder.", ConsoleColor.Red);
                    return;
                }
            }
            else
            {
                dbContext.Accounts.Add(new Account()
                {
                    AccountNumber = accountNumber,
                    AccountType = accountType,
                    BranchCode = branchCode,
                    CurrentBalance = 0m,
                    OpeningDate = DateTime.Now
                });
            }

            dbContext.CustomerAccounts.Add(new CustomerAccount()
            {
                CustomerId = customerId,
                AccountNumber = accountNumber,
                OwnershipType = role,
                OwnershipStartDate = DateTime.Now,
                AccountStatus = AccountStatus.Active
            });

            await dbContext.SaveChangesAsync(); // Account + CustomerAccount in one transaction

            string action = existingAccount is null ? "created and linked" : "linked";
            WriteLineColor($"Account '{accountNumber}' {action} to customer {customerId} as {role} owner.", ConsoleColor.Green);
        }
        #endregion

        #region 3) Update Account Status
        private static async Task UpdateAccountStatusAsync(APPDbContext dbContext)
        {
            Console.WriteLine("--- Update Account Status ---");

            string accountNumber = ReadText("Account Number : ", 20);
            int customerId = ReadInt("Customer Id    : ");

            CustomerAccount? link = await dbContext.CustomerAccounts.FindAsync(customerId, accountNumber);
            if (link is null)
            {
                WriteLineColor($"Customer #{customerId} is not linked to account '{accountNumber}'.", ConsoleColor.Red);
                return;
            }

            Console.WriteLine($"Current Status : {link.AccountStatus}");
            Console.WriteLine("New Status:");
            Console.WriteLine("    1) Active");
            Console.WriteLine("    2) Closed");
            AccountStatus newStatus = (AccountStatus)ReadChoice("  Choice: ", 1, 2);

            if (newStatus == link.AccountStatus)
            {
                WriteLineColor($"Status is already {newStatus}. Nothing changed.", ConsoleColor.Yellow);
                return;
            }

            link.AccountStatus = newStatus;
            await dbContext.SaveChangesAsync();
            WriteLineColor($"Status updated to {newStatus}.", ConsoleColor.Green);
        }
        #endregion

        #region 4) Remove an Account from a Customer
        private static async Task RemoveAccountFromCustomerAsync(APPDbContext dbContext)
        {
            Console.WriteLine("--- Remove Account From Customer ---");

            string accountNumber = ReadText("Account Number : ", 20);
            int customerId = ReadInt("Customer Id    : ");

            CustomerAccount? link = await dbContext.CustomerAccounts.FindAsync(customerId, accountNumber);
            if (link is null)
            {
                WriteLineColor($"Customer #{customerId} is not linked to account '{accountNumber}'.", ConsoleColor.Red);
                return;
            }

            dbContext.CustomerAccounts.Remove(link);
            await dbContext.SaveChangesAsync();
            Console.WriteLine();
            WriteLineColor("  Ownership link deleted.", ConsoleColor.Green);

           
            bool hasOtherOwners = await dbContext.CustomerAccounts.AnyAsync(CA => CA.AccountNumber == accountNumber);
            if (!hasOtherOwners)
            {
                Account? account = await dbContext.Accounts.FindAsync(accountNumber);
                if (account is not null)
                {
                    dbContext.Accounts.Remove(account);
                    await dbContext.SaveChangesAsync();
                    WriteLineColor($"      That was the last owner - account '{accountNumber}' was also removed.", ConsoleColor.Cyan);
                }
            }
        }
        #endregion

        #region 5) List all Customers (Eager Loading)
        private static async Task ListCustomersAsync(APPDbContext dbContext)
        {
            Console.WriteLine("--- All Customers ---");
            Console.WriteLine();

         
            List<Customer> customers = await dbContext.Customers
                .AsNoTracking()
                .Include(C => C.CustomerAccounts)
                    .ThenInclude(CA => CA.Account)
                        .ThenInclude(A => A.Branch)
                .OrderBy(C => C.Id)
                .ToListAsync();

            if (customers.Count == 0)
            {
                Console.WriteLine("  (no customers yet)");
                return;
            }

            foreach (Customer customer in customers)
            {
                Console.WriteLine($"  #{customer.Id} {customer.FullName} ({customer.CustomerType})");

                if (customer.CustomerAccounts.Count == 0)
                {
                    Console.WriteLine("      (no accounts)");
                    continue;
                }

                foreach (CustomerAccount link in customer.CustomerAccounts.OrderBy(CA => CA.AccountNumber))
                {
                    Account account = link.Account;
                    Console.WriteLine($"      {account.AccountNumber,-10} {account.AccountType,-9} Balance: {account.CurrentBalance,12:N2}   " +
                                      $"{link.OwnershipType,-9} {link.AccountStatus,-7} @ {account.Branch.Name}");
                }
            }
        }
        #endregion

        #region Data Seeding (sample customers, only if the table is empty)
       
        private static async Task SeedSampleCustomersAsync(APPDbContext dbContext)
        {
            if (await dbContext.Customers.AnyAsync())
                return;

            Customer ahmed = new Customer { FullName = "Ahmed Ali", NationalId = "29001011234567", DateOfBirth = new DateTime(1990, 1, 1), Email = "ahmed.ali@gmail.com", PhoneNumber = "01012345678", Address = "10 Nasr St, Cairo", CustomerType = CustomerType.Individual };
            Customer nile = new Customer { FullName = "Nile Trading LLC", NationalId = "1002003004", DateOfBirth = new DateTime(2010, 5, 20), Email = "info@niletrading.com", PhoneNumber = "034567890", Address = "5 Port Said St, Alexandria", CustomerType = CustomerType.Business };

            Account cur = new Account { AccountNumber = "2001-CUR", AccountType = AccountType.Current, CurrentBalance = 15400m, OpeningDate = new DateTime(2022, 3, 15), BranchCode = "CAI-01" };
            Account bus = new Account { AccountNumber = "3000-BUS", AccountType = AccountType.Business, CurrentBalance = 120000m, OpeningDate = new DateTime(2021, 9, 1), BranchCode = "ALX-01" };

            dbContext.CustomerAccounts.AddRange(
                new CustomerAccount { Customer = ahmed, Account = cur, OwnershipType = OwnershipType.Primary, OwnershipStartDate = cur.OpeningDate, AccountStatus = AccountStatus.Active },
                new CustomerAccount { Customer = nile, Account = bus, OwnershipType = OwnershipType.Primary, OwnershipStartDate = bus.OpeningDate, AccountStatus = AccountStatus.Active });

            dbContext.Transactions.AddRange(
                new Transaction { Account = cur, TransactionDate = new DateTime(2024, 1, 5), Amount = 15400m, TransactionType = TransactionType.Deposit, Note = "Opening deposit" },
                new Transaction { Account = bus, TransactionDate = new DateTime(2024, 2, 10), Amount = 120000m, TransactionType = TransactionType.Deposit, Note = null });

            await dbContext.SaveChangesAsync();
        }
        #endregion

        #region Input Helpers (never crash on bad input - ask again)
        private static string ReadText(string label, int maxLength, Func<string, string?>? validate = null)
        {
            while (true)
            {
                Console.Write(label);
                string value = (Console.ReadLine() ?? string.Empty).Trim();

                string? error =
                    value.Length == 0 ? "This field is required." :
                    value.Length > maxLength ? $"Maximum length is {maxLength} characters." :
                    validate?.Invoke(value);

                if (error is null)
                    return value;

                WriteLineColor($"  {error} Please try again.", ConsoleColor.Red);
            }
        }

        private static int ReadInt(string label)
        {
            while (true)
            {
                Console.Write(label);
                if (int.TryParse(Console.ReadLine(), out int value) && value > 0)
                    return value;

                WriteLineColor("  Please enter a valid positive number.", ConsoleColor.Red);
            }
        }

        private static int ReadChoice(string label, int min, int max)
        {
            while (true)
            {
                Console.Write(label);
                if (int.TryParse(Console.ReadLine(), out int value) && value >= min && value <= max)
                    return value;

                WriteLineColor($"  Invalid choice. Enter a number from {min} to {max}.", ConsoleColor.Red);
            }
        }

        private static DateTime ReadDate(string label)
        {
            while (true)
            {
                Console.Write(label);
                string input = (Console.ReadLine() ?? string.Empty).Trim();

                if (DateTime.TryParseExact(input, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                {
                    if (date >= DateTime.Today)
                        WriteLineColor("  Date of birth must be in the past.", ConsoleColor.Red);
                    else if (date.Year < 1900)
                        WriteLineColor("  Please enter a realistic date.", ConsoleColor.Red);
                    else
                        return date;
                }
                else
                    WriteLineColor("  Invalid date. Use the format yyyy-MM-dd (e.g. 2000-01-25).", ConsoleColor.Red);
            }
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                MailAddress address = new MailAddress(email);
                return address.Address == email && email.Contains('.');
            }
            catch
            {
                return false;
            }
        }

        private static void WriteLineColor(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }
        #endregion
    }
}
