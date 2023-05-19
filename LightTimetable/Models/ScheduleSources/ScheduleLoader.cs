﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using LightTimetable.Properties;
using LightTimetable.Models.Utilities;


namespace LightTimetable.Models.ScheduleSources
{
    public class ScheduleLoader
    {
        public Dictionary<DateTime, List<DataItem>>? ScheduleDictionary { get; protected set; }
        public DateTime[]? AvailableDates { get; protected set; }

        public async Task InitializeScheduleAsync(DateTime startDate, DateTime endDate, IScheduleSource scheduleSource)
        {
            ScheduleDictionary = await scheduleSource.LoadDataAsync(startDate, endDate);

            if (ScheduleDictionary == null || !ScheduleDictionary.Any())
                return;

            // Initialize AvailableDates
            AvailableDates = ScheduleDictionary.Keys.ToArray();

            // Check for obsolete notes
            var oldestId = ScheduleDictionary.First().Value.First().Id;

            foreach (var obsoleteId in Settings.Default.Notes.Where(a => a.Key < oldestId))
            {
                Settings.Default.Notes.Remove(obsoleteId.Key);
            }
            Settings.Default.Save();
        }
    }
}