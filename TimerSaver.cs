/*
 * autor: Kogan Anatolii
 * e-mail: a.kogan@gooligames.com
 */
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Gooligames.Timers
{
    public static class TimerSaver
    {
        public const string DirectoryName = "TimersData";
        public const string FileTimerValuesEnding = "Value.timer";
        public const string FileStopDateEnding = "StopDate.timer";

        private static DirectoryInfo _directory = null;
        public static DirectoryInfo Directory
        {
            get => _directory ??= System.IO.Directory.CreateDirectory($"{Application.persistentDataPath}/{DirectoryName}");
        }

        public static void SaveTimer(this TimerController timer)
        {
            var pathWithValues = $"{Directory.FullName}/{timer.key}{FileTimerValuesEnding}";
            var pathWithStopDate = $"{Directory.FullName}/{timer.key}{FileStopDateEnding}";

            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(pathWithValues, FileMode.Create))
            {
                formatter.Serialize(stream, timer.values);
            }

            using (FileStream stream = new FileStream(pathWithStopDate, FileMode.Create))
            {
                formatter.Serialize(stream, new TimerStopDate(DateTime.Now));
            }
        }

        public static void SetSaveValues(this TimerController timer)
        {
            var timerValues = GetTimerStopedValue(timer.key);
            var interval = GetLustTimerStopData(timer.key).AddDays(timerValues.days).AddHours(timerValues.hours).AddMinutes(timerValues.minutes).AddSeconds(timerValues.seconds)
                - DateTime.Now;

            timer.values = new TimerData(interval);
        }

        public static TimerData GetTimerStopedValue(string key)
        {
            var pathWithValues = $"{Directory.FullName}/{key}{FileTimerValuesEnding}";

            if (File.Exists(pathWithValues))
            {
                using (FileStream stream = new FileStream(pathWithValues, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    return (TimerData)formatter.Deserialize(stream);
                }
            }

            return TimerData.Empty;
        }

        public static DateTime GetLustTimerStopData(string key)
        {
            var pathWithStopDate = $"{Directory.FullName}/{key}{FileStopDateEnding}";

            if (File.Exists(pathWithStopDate))
            {
                using (FileStream stream = new FileStream(pathWithStopDate, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    var stopdate = (TimerStopDate)formatter.Deserialize(stream);
                    return DateTime.Now.SetStopData(stopdate);
                }
            }

            return DateTime.Now;
        }

        public static bool DeleteSave(string key)
        {
            var pathWithValues = $"{Directory.FullName}/{key}{FileTimerValuesEnding}";
            var pathWithStopDate = $"{Directory.FullName}/{key}{FileStopDateEnding}";

            if (File.Exists(pathWithValues))
            {
                File.Delete(pathWithValues);
                File.Delete(pathWithStopDate);

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <returns>true - if timer save by this key exists</returns>
        public static bool TimerSaveExists(string key)
        {
            return File.Exists($"{Directory.FullName}/{key}{FileTimerValuesEnding}") || File.Exists($"{Directory.FullName}/{key}{FileStopDateEnding}");
        }

        [Serializable]
        public struct TimerStopDate
        {
            public int seconds;
            public int minutes;
            public int hours;
            public int days;

            public TimerStopDate(DateTime stopTime)
            {
                days = stopTime.Day;
                hours = stopTime.Hour;
                minutes = stopTime.Minute;
                seconds = stopTime.Second;
            }
        }

        private static DateTime SetStopData(this DateTime data, TimerStopDate stopDate)
        {
            data = new DateTime(DateTime.Now.Year, DateTime.Now.Month, stopDate.days, stopDate.hours, stopDate.minutes, stopDate.seconds);
            return data;
        }
    }
}