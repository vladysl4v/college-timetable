using Newtonsoft.Json.Linq;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Linq;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

using LightTimetable.Tools;
using LightTimetable.Common;
using LightTimetable.Properties;
using LightTimetable.Models.Utilities;

using WeekDictionary =
    System.Collections.Generic.Dictionary<LightTimetable.Common.NormalDayOfWeek, System.Collections.Generic.List<
        System.Collections.Generic.Dictionary<string, string>>?>;


namespace LightTimetable.Models.ScheduleSources
{
    [ScheduleSource("КПI iм. Iгоря Сiкорського", typeof(KpiScheduleSettings))]
    public sealed class KpiScheduleSource : IScheduleSource
    {
        private readonly Calendar _calendar = new GregorianCalendar();
        private readonly IServiceProvider _serviceProvider;

        public KpiScheduleSource(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<Dictionary<DateTime, List<DataItem>>?> LoadDataAsync(DateTime startDate, DateTime endDate)
        {
            var outputData = new Dictionary<DateTime, List<DataItem>>();

            var (firstWeek, secWeek) = await LoadScheduleForWeeks(Settings.Default.StudyGroup);

            if (firstWeek == null | secWeek == null)
                return null;           

            var builder = new DataItemBuilder();
                        
            builder.AddGlobalServices(_serviceProvider);

            for (var date = startDate; date < endDate; date = date.AddDays(1))
            {
                var dataItems = new List<DataItem>();
                
                var properWeek = IsWeekPrimary(date) ? firstWeek : secWeek;

                if (!properWeek.ContainsKey(date.GetNormalDayOfWeek()))
                    continue;

                foreach (var lesson in properWeek[date.GetNormalDayOfWeek()])
                {
                    var time = TimeOnly.ParseExact(lesson["time"], "H.mm");

                    var place = lesson["place"];
                    if (place.LastOrDefault() == '-')
                    {
                        place = place[..^1];
                    }
                    builder.AddTimePeriod(date, time, new TimeSpan(1, 35, 0));

                    builder.AddBasicInformation(lesson["name"], lesson["type"], place, lesson["teacherName"]);
                    builder.AddServices();
                    dataItems.Add(builder.Build());
                }
                dataItems.Sort();

                outputData.Add(date, dataItems);
            }
            
            return outputData;
        }

        private async Task<(WeekDictionary?, WeekDictionary?)> LoadScheduleForWeeks(string groupId)
        {
            var url = "https://schedule.kpi.ua/api/schedule/lessons?" + 
                      $"groupId={groupId}";
            
            var httpClient = _serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient();

            string serializedData;
            try
            {
                serializedData = await httpClient.GetStringAsync(url);
            }
            catch
            {
                return (null, null);
            }
            
            var jsonData = JObject.Parse(serializedData);

            if (!jsonData.ContainsKey("data"))
                return (null, null);

            var scheduleFirstData = ConvertToWeekDictionary(jsonData["data"]["scheduleFirstWeek"]);
            var scheduleSecData = ConvertToWeekDictionary(jsonData["data"]["scheduleSecondWeek"]);
            
            return (scheduleFirstData, scheduleSecData);
        }

        private bool IsWeekPrimary(DateTime date)
        {
            var intPrimariness = _calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Monday) % 2;
            return intPrimariness == 1;
        }
        
        private WeekDictionary ConvertToWeekDictionary(JToken jsonData)
        {
            var scheduleData = new WeekDictionary();
            var currentDayOfWeek = NormalDayOfWeek.Monday;
            
            foreach (var pair in jsonData)
            {
                var pairsList = pair["pairs"].ToObject<List<Dictionary<string, string>>>();
                if (pairsList.Any())
                {
                    scheduleData.Add(currentDayOfWeek, pairsList);
                }
                currentDayOfWeek++;   
            }

            return scheduleData;
        }
    }
}