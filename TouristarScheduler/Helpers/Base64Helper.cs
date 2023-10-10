namespace TouristarScheduler.Helpers;

public class Base64Helper
{
    public static string RepairBase64String(string base64)
    {
        var converted = base64.Replace('-', '+');
        return converted.Replace('_', '/');
    }
}