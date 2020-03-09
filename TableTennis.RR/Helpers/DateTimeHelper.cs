using System;
using System.Collections.Generic;
using System.Globalization;

namespace TableTennis.RR.Helpers
{
    public static class DateTimeHelper
    {
        public static IEnumerable<int> GetPast90DaysYYYYMMDD()
        {
            var now = DateTime.UtcNow;
            var iteratorTime = now;

            // 0 - 89 for safety. For more info: https://betsapi.com/docs/events/search.html
            for (var i = 0; i < 89; i++)
            {
                iteratorTime = iteratorTime.AddDays(-1);
                var res = iteratorTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
                yield return int.Parse(res);
            }
        }
    }
}