using System.Globalization;
using System.Text;

Console.WriteLine("Starting BrightPath Community Library Book Loan System...");

var app = new LibraryLoanApp();
app.Run();

public class LibraryLoanApp
{
    private const string DataFileName = "books.txt";

    public void Run()
    {
        Console.WriteLine("Data file location: " + GetStorageFilePath());

        int choice;
        do
        {
            DisplayMenu();

            Console.Write("Enter your choice: ");
            var input = Console.ReadLine();

            if (!int.TryParse(input, out choice))
            {
                Console.WriteLine("Invalid menu choice. Please enter a number from 1 to 7.");
                Console.WriteLine();
                continue;
            }

            try
            {
                switch (choice)
                {
                    case 1:
                        BorrowBook();
                        break;
                    case 2:
                        ViewBorrowedBooks();
                        break;
                    case 3:
                        SearchLoan();
                        break;
                    case 4:
                        UpdateLoan();
                        break;
                    case 5:
                        ReturnBook();
                        break;
                    case 6:
                        GenerateReports();
                        break;
                    case 7:
                        Console.WriteLine("Thank you for using the BrightPath Community Library system.");
                        break;
                    default:
                        Console.WriteLine("Invalid menu choice. Please enter a number from 1 to 7.");
                        Console.WriteLine();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An unexpected error occurred: " + ex.Message);
                Console.WriteLine("Please try again.");
            }

            if (choice != 7)
            {
                Console.WriteLine();
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
            }
        }
        while (choice != 7);
    }

    private static void DisplayMenu()
    {
        Console.Clear();
        Console.WriteLine("===========================");
        Console.WriteLine("COMMUNITY LIBRARY");
        Console.WriteLine("=================");
        Console.WriteLine("1. Borrow Book");
        Console.WriteLine("2. View Borrowed Books");
        Console.WriteLine("3. Search Loan");
        Console.WriteLine("4. Update Loan");
        Console.WriteLine("5. Return Book");
        Console.WriteLine("6. Generate Reports");
        Console.WriteLine("7. Exit");
    }

    private static string GetStorageFilePath()
    {
        return Path.Combine(AppContext.BaseDirectory, DataFileName);
    }

    private static void BorrowBook()
    {
        Console.Clear();
        Console.WriteLine("=== Borrow Book ===");

        try
        {
            var loans = LoadLoans();

            var loan = new LoanRecord();
            loan.LoanNumber = ReadRequiredText("Loan Number: ");

            if (loans.Any(l => string.Equals(l.LoanNumber, loan.LoanNumber, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine("Duplicate loan number found. Please use a different Loan Number.");
                return;
            }

            loan.MemberName = ReadRequiredText("Member Name: ");
            loan.BookTitle = ReadRequiredText("Book Title: ");
            loan.Author = ReadRequiredText("Author: ");
            loan.Category = ReadRequiredText("Category: ");

            loan.BorrowDate = ReadValidDate("Borrow Date (dd/MM/yyyy): ");
            loan.ReturnDate = ReadValidDate("Return Date (dd/MM/yyyy): ");

            if (DateTime.Parse(loan.ReturnDate, CultureInfo.InvariantCulture) < DateTime.Parse(loan.BorrowDate, CultureInfo.InvariantCulture))
            {
                Console.WriteLine("Return date cannot be earlier than borrow date.");
                return;
            }

            loan.ContactNumber = ReadRequiredText("Contact Number: ");

            var filePath = GetStorageFilePath();
            var recordText = loan.ToFileString() + Environment.NewLine;

            File.AppendAllText(filePath, recordText);

            Console.WriteLine();
            Console.WriteLine("Loan saved successfully.");
            Console.WriteLine("Saved to: " + filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error saving loan: " + ex.Message);
        }
    }

    private static void ViewBorrowedBooks()
    {
        Console.Clear();
        Console.WriteLine("=== View Borrowed Books ===");

        var filePath = GetStorageFilePath();

        if (!File.Exists(filePath))
        {
            Console.WriteLine("No loan records found. File does not exist yet: " + filePath);
            return;
        }

        var loans = LoadLoans();

        if (loans.Count == 0)
        {
            Console.WriteLine("There are no saved loan records.");
            return;
        }

        foreach (var loan in loans)
        {
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("Loan Number: " + loan.LoanNumber);
            Console.WriteLine("Member Name: " + loan.MemberName);
            Console.WriteLine("Book Title: " + loan.BookTitle);
            Console.WriteLine("Author: " + loan.Author);
            Console.WriteLine("Category: " + loan.Category);
            Console.WriteLine("Borrow Date: " + loan.BorrowDate);
            Console.WriteLine("Return Date: " + loan.ReturnDate);
            Console.WriteLine("Contact Number: " + loan.ContactNumber);
        }
        Console.WriteLine("----------------------------------------");
    }

    private static void SearchLoan()
    {
        Console.Clear();
        Console.WriteLine("=== Search Loan ===");

        Console.WriteLine("Search by:");
        Console.WriteLine("1. Loan Number");
        Console.WriteLine("2. Member Name");
        Console.Write("Your choice: ");
        var choice = Console.ReadLine();

        var loans = LoadLoans();

        if (loans.Count == 0)
        {
            Console.WriteLine("No loan records available to search.");
            return;
        }

        List<LoanRecord> matches = new List<LoanRecord>();

        if (choice == "1")
        {
            var loanNumber = ReadRequiredText("Enter Loan Number: ");
            matches = loans.Where(l => l.LoanNumber.Equals(loanNumber, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        else if (choice == "2")
        {
            var memberName = ReadRequiredText("Enter Member Name: ");
            matches = loans.Where(l => l.MemberName.Contains(memberName, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        else
        {
            Console.WriteLine("Invalid search option.");
            return;
        }

        if (matches.Count == 0)
        {
            Console.WriteLine("No matching loan record found.");
            return;
        }

        foreach (var loan in matches)
        {
            DisplayLoanDetails(loan);
        }
    }

    private static void UpdateLoan()
    {
        Console.Clear();
        Console.WriteLine("=== Update Loan ===");

        var loans = LoadLoans();
        if (loans.Count == 0)
        {
            Console.WriteLine("There are no loans to update.");
            return;
        }

        var loanNumber = ReadRequiredText("Enter Loan Number to update: ");
        var loan = loans.FirstOrDefault(l => l.LoanNumber.Equals(loanNumber, StringComparison.OrdinalIgnoreCase));

        if (loan == null)
        {
            Console.WriteLine("Loan number not found.");
            return;
        }

        Console.WriteLine("Select the field to update:");
        Console.WriteLine("1. Return Date");
        Console.WriteLine("2. Contact Number");
        Console.Write("Choice: ");
        var option = Console.ReadLine();

        switch (option)
        {
            case "1":
                var newReturnDate = ReadValidDate("Enter new Return Date (dd/MM/yyyy): ");
                if (DateTime.Parse(newReturnDate, CultureInfo.InvariantCulture) < DateTime.Parse(loan.BorrowDate, CultureInfo.InvariantCulture))
                {
                    Console.WriteLine("Return date cannot be earlier than borrow date.");
                    return;
                }
                loan.ReturnDate = newReturnDate;
                break;
            case "2":
                loan.ContactNumber = ReadRequiredText("Enter new Contact Number: ");
                break;
            default:
                Console.WriteLine("Invalid option. Update cancelled.");
                return;
        }

        SaveLoans(loans);
        Console.WriteLine("Loan updated successfully.");
    }

    private static void ReturnBook()
    {
        Console.Clear();
        Console.WriteLine("=== Return Book ===");

        var loans = LoadLoans();
        if (loans.Count == 0)
        {
            Console.WriteLine("No loan records available to return.");
            return;
        }

        var loanNumber = ReadRequiredText("Enter Loan Number to return: ");
        var loan = loans.FirstOrDefault(l => l.LoanNumber.Equals(loanNumber, StringComparison.OrdinalIgnoreCase));

        if (loan == null)
        {
            Console.WriteLine("Loan number not found. No record was removed.");
            return;
        }

        loans.Remove(loan);
        SaveLoans(loans);
        Console.WriteLine("Book returned successfully and loan record removed.");
    }

    private static void GenerateReports()
    {
        Console.Clear();
        Console.WriteLine("=== Generate Reports ===");

        var loans = LoadLoans();

        if (loans.Count == 0)
        {
            Console.WriteLine("No loan records available. Reports cannot be generated yet.");
            return;
        }

        var totalLoans = loans.Count;
        var categoryCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var today = DateTime.Today;

        foreach (var loan in loans)
        {
            if (!categoryCounts.ContainsKey(loan.Category.Trim()))
            {
                categoryCounts[loan.Category.Trim()] = 0;
            }
            categoryCounts[loan.Category.Trim()]++;
        }

        var mostBorrowedCategory = categoryCounts.Count > 0
            ? categoryCounts.OrderByDescending(c => c.Value).ThenBy(c => c.Key, StringComparer.OrdinalIgnoreCase).First().Key
            : "No category data";

        int overdueBooks = 0;
        int activeLoans = 0;

        foreach (var loan in loans)
        {
            if (DateTime.TryParse(loan.ReturnDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var returnDate))
            {
                if (returnDate < today)
                {
                    overdueBooks++;
                }
                else
                {
                    activeLoans++;
                }
            }
            else
            {
                // If a return date is invalid, treat it as active if it was entered during validation.
                // In a valid file all dates should be parseable.
                activeLoans++;
            }
        }

        int returnedBooks = 0;
        Console.WriteLine("Total loans: " + totalLoans);
        Console.WriteLine("Most borrowed category: " + mostBorrowedCategory);
        Console.WriteLine("Overdue books: " + overdueBooks + " (unreturned loans whose return date is before today)");
        Console.WriteLine("Active loans: " + activeLoans + " (not yet returned and not overdue)");
        Console.WriteLine("Returned books: " + returnedBooks + " (assignment requires deleting returned loans, so returned records are not kept in books.txt)");
    }

    private static List<LoanRecord> LoadLoans()
    {
        var loans = new List<LoanRecord>();
        var filePath = GetStorageFilePath();

        if (!File.Exists(filePath))
        {
            return loans;
        }

        try
        {
            using var reader = new StreamReader(filePath);
            string? line;

            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var parts = line.Split('|');
                if (parts.Length != 8)
                {
                    Console.WriteLine("Skipping malformed record: " + line);
                    continue;
                }

                var loan = new LoanRecord
                {
                    LoanNumber = parts[0],
                    MemberName = parts[1],
                    BookTitle = parts[2],
                    Author = parts[3],
                    Category = parts[4],
                    BorrowDate = parts[5],
                    ReturnDate = parts[6],
                    ContactNumber = parts[7]
                };

                loans.Add(loan);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error reading loan data: " + ex.Message);
        }

        return loans;
    }

    private static void SaveLoans(List<LoanRecord> loans)
    {
        var filePath = GetStorageFilePath();

        try
        {
            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);

            foreach (var loan in loans)
            {
                writer.WriteLine(loan.ToFileString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error saving loan data: " + ex.Message);
        }
    }

    private static string ReadRequiredText(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var value = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }

            Console.WriteLine("This field is required. Please enter a value.");
        }
    }

    private static string ReadValidDate(string prompt)
    {
        while (true)
        {
            var value = ReadRequiredText(prompt);

            if (DateTime.TryParseExact(value, new[] { "dd/MM/yyyy", "d/MM/yyyy", "dd-MM-yyyy", "yyyy-MM-dd" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                return value;
            }

            Console.WriteLine("Invalid date. Please enter a valid date in dd/MM/yyyy format.");
        }
    }

    private static void DisplayLoanDetails(LoanRecord loan)
    {
        Console.WriteLine("----------------------------------------");
        Console.WriteLine("Loan Number: " + loan.LoanNumber);
        Console.WriteLine("Member Name: " + loan.MemberName);
        Console.WriteLine("Book Title: " + loan.BookTitle);
        Console.WriteLine("Author: " + loan.Author);
        Console.WriteLine("Category: " + loan.Category);
        Console.WriteLine("Borrow Date: " + loan.BorrowDate);
        Console.WriteLine("Return Date: " + loan.ReturnDate);
        Console.WriteLine("Contact Number: " + loan.ContactNumber);
        Console.WriteLine("----------------------------------------");
    }
}

public class LoanRecord
{
    public string LoanNumber { get; set; } = "";
    public string MemberName { get; set; } = "";
    public string BookTitle { get; set; } = "";
    public string Author { get; set; } = "";
    public string Category { get; set; } = "";
    public string BorrowDate { get; set; } = "";
    public string ReturnDate { get; set; } = "";
    public string ContactNumber { get; set; } = "";

    public string ToFileString()
    {
        return LoanNumber + "|" + MemberName + "|" + BookTitle + "|" + Author + "|" + Category + "|" + BorrowDate + "|" + ReturnDate + "|" + ContactNumber;
    }
}
