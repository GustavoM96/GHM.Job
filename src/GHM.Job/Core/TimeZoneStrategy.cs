namespace GHM.Job;

public class NowTimeZoneStrategy : ITimeZoneStrategy
{
    public DateTime Now => DateTime.Now;
}

public class UtcTimeZoneStrategy : ITimeZoneStrategy
{
    public DateTime Now => DateTime.UtcNow;
}

public class UtcAddingHoursTimeZoneStrategy : ITimeZoneStrategy
{
    private readonly int _addhours;

    public UtcAddingHoursTimeZoneStrategy(int addhours)
    {
        _addhours = addhours;
    }

    public DateTime Now => DateTime.UtcNow.AddHours(_addhours);
}
