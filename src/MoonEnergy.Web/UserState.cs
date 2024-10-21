namespace MoonEnergy;

// Fake the user's data. This would normally come from a database.
public class UserState
{
    public UserState(string customerName)
    {
        CustomerNumber = 1234567;
        CustomerName = customerName;
        
        PostalCode = "1234AA";
        HouseNumber = 1;

        var monthlyUsage = GenerateUsage(11, 250, 350);
        var thisMonthUsage = GenerateUsage(DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month), 5, 15);
        monthlyUsage.Add(thisMonthUsage.Sum());

        Electricity = new ElecProduct
        {
            InstallmentAmountCurrent = 50,
            InstallmentAmountIdeal = 70,
            MontlyUsage = monthlyUsage.ToArray(),
            ThisMonthUsage =thisMonthUsage.ToArray()
        };
    }
    public int CustomerNumber { get; init; }
    public string CustomerName { get; init; }
    public string PostalCode { get; init; }
    public int HouseNumber { get; init; }

    public ElecProduct Electricity { get; init; }

    public class ElecProduct
    {
        public required int InstallmentAmountCurrent { get; set; }
        public required int InstallmentAmountIdeal { get; set; }
        public required int[] MontlyUsage { get; set; }
        public required int[] ThisMonthUsage { get; set; }
    }

    static List<int> GenerateUsage(int count, int min, int max)
    {
        var random = new Random();
        var usageNumbers = new List<int>();

        for (var i = 0; i < count; i++)
        {
            usageNumbers.Add(random.Next(min, max));
        }

        return usageNumbers;
    }
}