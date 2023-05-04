using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetIT.Domain.Extensions
{
    public static class DateTimeExtension
    {
        // Метод для округления даты по тикам до секунд
        public static DateTime Round(this DateTime dateTime, TimeSpan interval)
        {
            var halfIntervalTicks = (interval.Ticks + 1) >> 1;

            return dateTime.AddTicks(halfIntervalTicks - ((dateTime.Ticks + halfIntervalTicks) % interval.Ticks));
        }
    }
}
