using System.Text.RegularExpressions;

namespace SmsClient
{
    public class Validation
    {
        public static bool IsValidMobileNumber(string Number)
        {
            Regex regex = new Regex(@"^\d{10}$");

            if (regex.IsMatch(Number))
                return true;

            return false;
        }
    }
}
