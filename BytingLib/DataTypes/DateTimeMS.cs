namespace BytingLib
{
    public struct DateTimeMS
    {
        public long MS { get; set; }

        const string FormatDate = "yyyy-MM-dd";
        const string FormatTime = "HH-mm-ss";
        const string FormatFractions = "fff";

        public DateTimeMS()
        {
            MS = 0;
        }

        public DateTimeMS(DateTime dateTime)
        {
            dateTime = dateTime.ToUniversalTime();

            MS = new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
        }

        public DateTimeMS(long ms)
        {
            MS = ms;
        }

        public static DateTimeMS Now => new DateTimeMS(DateTime.UtcNow);

        public override string ToString()
        {
            // TODO: check if utcdatetime is needed
            return DateTimeOffset.FromUnixTimeMilliseconds(MS).ToString($"{FormatDate}_{FormatTime}-{FormatFractions}");
        }

        public string ToShortString()
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(MS).ToString($"{FormatDate}_{FormatTime}");
        }

        public string ToDateString()
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(MS).ToString(FormatDate);
        }
        /// <summary>Excluding fractions of a second.</summary>
        public string ToShortTimeString()
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(MS).ToString(FormatTime);
        }
        /// <summary>Including fractions of a second.</summary>
        public string ToLongTimeString()
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(MS).ToString($"{FormatTime}-{FormatFractions}");
        }

        public override int GetHashCode()
        {
            return MS.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            if (obj is DateTimeMS dateTimeMS)
            {
                return dateTimeMS.MS == MS;
            }
            return false;
        }

        public DateTime GetDateTime(DateTimeKind kind = DateTimeKind.Utc)
        {
            switch (kind)
            {
                case DateTimeKind.Unspecified:
                    return DateTimeOffset.FromUnixTimeMilliseconds(MS).DateTime;
                case DateTimeKind.Utc:
                    return DateTimeOffset.FromUnixTimeMilliseconds(MS).UtcDateTime;
                case DateTimeKind.Local:
                    return DateTimeOffset.FromUnixTimeMilliseconds(MS).LocalDateTime;
                default:
                    throw new NotImplementedException();
            }
        }

        public static bool operator ==(DateTimeMS d1, DateTimeMS d2)
        {
            return d1.MS == d2.MS;
        }
        public static bool operator !=(DateTimeMS d1, DateTimeMS d2)
        {
            return d1.MS != d2.MS;
        }
        public static bool operator <(DateTimeMS d1, DateTimeMS d2)
        {
            return d1.MS < d2.MS;
        }
        public static bool operator >(DateTimeMS d1, DateTimeMS d2)
        {
            return d1.MS > d2.MS;
        }
        public static bool operator <=(DateTimeMS d1, DateTimeMS d2)
        {
            return d1.MS <= d2.MS;
        }
        public static bool operator >=(DateTimeMS d1, DateTimeMS d2)
        {
            return d1.MS >= d2.MS;
        }
    }
}
