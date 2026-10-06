using System;
using System.Collections.Generic;
using System.Linq;

namespace Sinapse.Util
{
    public static class DateTimeUtil
    {
        public static String GetMesColumn(Int32 Mes)
        {
            switch (Mes)
            {
                case 1: return "JANEIRO";
                case 2: return "FEVEREIRO";
                case 3: return "MARCO";
                case 4: return "ABRIL";
                case 5: return "MAIO";
                case 6: return "JUNHO";
                case 7: return "JULHO";
                case 8: return "AGOSTO";
                case 9: return "SETEMBRO";
                case 10: return "OUTUBRO";
                case 11: return "NOVEMBRO";
                case 12: return "DEZEMBRO";
                default: throw new Exception($"Mes {Mes} informado inválido");
            }
        }
        public static int GetIdade(DateTime Nascimento)
        {
            DateTime zeroTime = new DateTime(1, 1, 1);
            TimeSpan span = DateTime.Now - Nascimento;
            int years = (zeroTime + span).Year - 1;

            return years;
        }
        public static int GetWeekNumberOfMonth(DateTime date)
        {
            date = date.Date;
            DateTime firstMonthDay = new DateTime(date.Year, date.Month, 1);
            DateTime firstMonthMonday = firstMonthDay.AddDays((DayOfWeek.Monday + 7 - firstMonthDay.DayOfWeek) % 7);
            if (firstMonthMonday > date)
            {
                firstMonthDay = firstMonthDay.AddMonths(-1);
                firstMonthMonday = firstMonthDay.AddDays((DayOfWeek.Monday + 7 - firstMonthDay.DayOfWeek) % 7);
            }
            return (date - firstMonthMonday).Days / 7 + 1;
        }
        public static DateTime GetLastDayOfMonth(DateTime date)
        {
            int lastDay = DateTime.DaysInMonth(date.Year, date.Month);
            return new DateTime(date.Year, date.Month, lastDay);
        }
        public static DateTime GetLastDayOfMonth(int year, int month)
        {
            int lastDay = DateTime.DaysInMonth(year, month);
            return new DateTime(year, month, lastDay);
        }
        public static TimeSpan GetTimeSpanFromDate(DateTime date)
        {
            string dateString = date.ToString("HH:mm:ss");
            return TimeSpan.Parse(dateString);
        }
        public static double RemoveSeconds(double hour)
        {
            TimeSpan hourTimeSpan = TimeSpan.FromHours(hour);
            return hourTimeSpan.TotalHours - (Convert.ToDouble(hourTimeSpan.Seconds) / 3600);
        }
        public static int CountDiasUteis(DateTime d1, DateTime d2, params DateTime[] bankHolidays)
        {
            List<DateTime> datas = new List<DateTime>();

            for (DateTime i = d1.Date; i <= d2.Date; i = i.AddDays(1))
            {
                datas.Add(i);
            }

            return datas.Count(d => d.DayOfWeek != DayOfWeek.Saturday &&
                                    d.DayOfWeek != DayOfWeek.Sunday &&
                                    !bankHolidays.Contains(d));
        }
        public static DateTime CastDataTratado(String value)
        {
            try
            {
                return Convert.ToDateTime(value);
            }
            catch
            {
                var arr = value.Split('/');
                var ultimoDia = DateTime.DaysInMonth(Convert.ToInt32(arr[2]), Convert.ToInt32(arr[1]));
                return new DateTime(Convert.ToInt32(arr[2]), Convert.ToInt32(arr[1]), ultimoDia);
            }
        }
        public static DateTime GetProximaDataPorDia(DateTime DataReferencia, Int32 Dia)
        {
            DateTime temp = DataReferencia;

            while (true)
            {
                temp = temp.AddDays(1);

                if (Dia > DateTime.DaysInMonth(temp.Year, temp.Month))
                    Dia = DateTime.DaysInMonth(temp.Year, temp.Month);

                if (Dia == temp.Day)
                {
                    return temp;
                }
            }
        }
        public static DateTime GetNextDayofTheWeek(DayOfWeek dia_semana)
        {

            var dia = DateTime.Today.AddDays(1);
            while (dia.DayOfWeek != dia_semana)
            {
                dia = dia.AddDays(1);
            }

            return dia;

        }
        public static DateTime GetLastDayofTheWeek(DayOfWeek dia_semana)
        {
            var dia = DateTime.Today;
            while (dia.DayOfWeek != dia_semana)
            {
                dia = dia.AddDays(-1);
            }

            return dia;
        }
        public static Boolean PeriodoCoincidente(DateTime inicio, DateTime termino, DateTime inicioPeriodo, DateTime terminoPeriodo)
        {
            return (inicioPeriodo <= inicio && inicio <= terminoPeriodo) || (inicioPeriodo <= termino && termino <= terminoPeriodo);
        }
        public static IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }
    }
}
