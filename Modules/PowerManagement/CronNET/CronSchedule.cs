using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FOG.Modules.PowerManagement.CronNET
{

    public class CronSchedule
    {
        private static readonly Regex DividedRegex = new Regex(@"(\*/\d+)");
        private static readonly Regex RangeRegex = new Regex(@"(\d+\-\d+)\/?(\d+)?");
        private static readonly Regex WildRegex = new Regex(@"(\*)");
        private static readonly Regex ListRegex = new Regex(@"(((\d+,)*\d+)+)");
        private static readonly Regex ValidationRegex = new Regex(DividedRegex + "|" + RangeRegex + "|" + WildRegex + "|" + ListRegex);

        private readonly string _expression;
        public List<int> Minutes;
        public List<int> Hours;
        public List<int> DaysOfMonth;
        public List<int> Months;
        public List<int> DaysOfWeek;


        public CronSchedule()
        {
            
        }

        public CronSchedule(string expressions)
        {
            _expression = expressions;
            Generate();
        }


        private bool IsValid()
        {
            return IsValid(_expression);
        }

        public bool IsValid(string expression)
        {
            var matches = ValidationRegex.Matches(expression);
            return matches.Count > 0;
        }

        public bool IsTime(DateTime dateTime)
        {
            return Minutes.Contains(dateTime.Minute) &&
                   Hours.Contains(dateTime.Hour) &&
                   DaysOfMonth.Contains(dateTime.Day) &&
                   Months.Contains(dateTime.Month) &&
                   DaysOfWeek.Contains((int)dateTime.DayOfWeek);
        }

        private void Generate()
        {
            if (!IsValid()) return;

            var matches = ValidationRegex.Matches(_expression);

            generate_minutes(matches[0].ToString());
            generate_hours(matches.Count > 1 ? matches[1].ToString() : "*");
            generate_days_of_month(matches.Count > 2 ? matches[2].ToString() : "*");
            generate_months(matches.Count > 3 ? matches[3].ToString() : "*");
            generate_days_of_weeks(matches.Count > 4 ? matches[4].ToString() : "*");
        }

        private void generate_minutes(string match)
        {
            Minutes = generate_values(match, 0, 60);
        }

        private void generate_hours(string match)
        {
            Hours = generate_values(match, 0, 24);
        }

        private void generate_days_of_month(string match)
        {
            DaysOfMonth = generate_values(match, 1, 32);
        }

        private void generate_months(string match)
        {
            Months = generate_values(match, 1, 13);
        }

        private void generate_days_of_weeks(string match)
        {
            DaysOfWeek = generate_values(match, 0, 7);
        }

        private List<int> generate_values(string configuration, int start, int max)
        {
            if (DividedRegex.IsMatch(configuration)) return divided_array(configuration, start, max);
            if (RangeRegex.IsMatch(configuration)) return range_array(configuration);
            if (WildRegex.IsMatch(configuration)) return wild_array(configuration, start, max);
            if (ListRegex.IsMatch(configuration)) return list_array(configuration);

            return new List<int>();
        }

        private List<int> divided_array(string configuration, int start, int max)
        {
            if (!DividedRegex.IsMatch(configuration))
                return new List<int>();

            var ret = new List<int>();
            var split = configuration.Split("/".ToCharArray());
            var divisor = int.Parse(split[1]);

            for (var i = start; i < max; ++i)
                if (i % divisor == 0)
                    ret.Add(i);

            return ret;
        }

        private static List<int> range_array(string configuration)
        {
            if (!RangeRegex.IsMatch(configuration))
                return new List<int>();

            var ret = new List<int>();
            var split = configuration.Split("-".ToCharArray());
            var start = int.Parse(split[0]);
            var end = 0;
            if (split[1].Contains("/"))
            {
                split = split[1].Split("/".ToCharArray());
                end = int.Parse(split[0]);
                var divisor = int.Parse(split[1]);

                for (var i = start; i < end; ++i)
                    if (i % divisor == 0)
                        ret.Add(i);
                return ret;
            }
            else
                end = int.Parse(split[1]);

            for (var i = start; i <= end; ++i)
                ret.Add(i);

            return ret;
        }

        private List<int> wild_array(string configuration, int start, int max)
        {
            if (!WildRegex.IsMatch(configuration))
                return new List<int>();

            var ret = new List<int>();

            for (var i = start; i < max; ++i)
                ret.Add(i);

            return ret;
        }

        private static List<int> list_array(string configuration)
        {
            if (!ListRegex.IsMatch(configuration))
                return new List<int>();

            var split = configuration.Split(",".ToCharArray());
            return split.Select(int.Parse).ToList();
        }

        public override string ToString()
        {
            return _expression;
        }
    }
}
