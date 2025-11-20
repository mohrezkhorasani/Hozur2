namespace BBBDashboard.MiddleWare.ext
{
public static class DateExtensions
{
    public static string ToPersianDate(this DateTime date)
    {
        try
        {
            var pc = new System.Globalization.PersianCalendar();
            return $"{pc.GetYear(date)}/{pc.GetMonth(date):D2}/{pc.GetDayOfMonth(date):D2} {date:HH:mm}";
        }
        catch
        {
            return date.ToString("yyyy/MM/dd HH:mm");
        }
    }
}
}
