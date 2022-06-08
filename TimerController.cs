/*
 * autor: Kogan Anatolii
 * e-mail: a.kogan@gooligames.com
 */
using System;
using System.Collections.Generic;
using Task = System.Threading.Tasks.Task;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

namespace Gooligames.Timers
{
    public sealed class TimerController
    {
        public static List<TimerController> runningTimers = new List<TimerController>(0);

        public string key { get; private set; } = "simple";

        public TimerData values = new TimerData();

        public Action OnValueChangedEvent;
        public Action OnFinishedEvent;

        private CancellationTokenSource _currentCancellationToken = null;

        private TimerController(string key)
        {
            this.key = key;
        }

        public static TimerController GetTimerByKey(string key)
        {
            foreach (var timer in runningTimers)
            {
                if (timer.key == key)
                {
                    return timer;
                }
            }

            return new TimerController(key);
        }

        /// <summary>
        /// Start timer from DateTime.Now to endTime
        /// </summary>
        /// <returns>true - timer finished; false - timer stopped, but not finished</returns>
        public async Task<bool> Run(DateTime endTime)
        {
            TimeSpan interval = endTime - DateTime.Now;
            values = new TimerData(interval);
            this.SaveTimer();

            return await Continue();
        }

        /// <summary>
        /// If timer hadn't existed, start timer with 1sec value!
        /// </summary>
        /// <returns>true - timer finished; false - timer stopped, but not finished</returns>
        public async Task<bool> Continue()
        {
            this.SetSaveValues();

            _currentCancellationToken?.Dispose();
            _currentCancellationToken = new CancellationTokenSource();
            var isTimerFinished = await TimerStateAsyncUpdate(_currentCancellationToken.Token);
            runningTimers.Remove(this);

            if (isTimerFinished == false)
            {
                this.SaveTimer();
            }
            else
            {
                TimerSaver.DeleteSave(key);
            }

            return isTimerFinished;
        }

        public void Stop()
        {
            _currentCancellationToken?.Cancel();
        }

        private async Task<bool> TimerStateAsyncUpdate(CancellationToken token)
        {
            runningTimers.Add(this);
            while (CheckForRemainingTime(ref values.seconds))
            {
                try
                {
                    await Task.Delay(1000, token);
                }
                catch (TaskCanceledException)
                {
                    //token have be respond for Delay(), but if application have stoped, there is TaskCanceledException...
                }


                if (token.IsCancellationRequested)
                {
                    return false;
                }

                values.seconds--;
                OnValueChangedEvent?.Invoke();
            }

            OnFinishedEvent?.Invoke();
            runningTimers.Remove(this);
            TimerSaver.DeleteSave(key);

            return true;
        }

        /// <summary>
        /// Equates seconds to 60, if seconds == 0 and another time parameters don't equal 0
        /// </summary>
        /// <returns>seconds > 0</returns>
        private bool CheckForRemainingTime(ref int seconds)
        {
            if (seconds <= 0)
            {
                if (values.minutes > 0)
                {
                    values.minutes--;
                    seconds += 59;
                }
                else
                {
                    if (values.hours > 0)
                    {
                        values.hours--;
                        values.minutes += 59;
                        seconds += 59;
                    }
                    else
                    {
                        if (values.days > 0)
                        {
                            values.days--;
                            values.hours += 23;
                            values.minutes += 59;
                            seconds += 59;
                        }
                    }
                }
            }

            return seconds > 0;
        }

        ~TimerController()
        {
            _currentCancellationToken?.Dispose();
        }

        #region "Helpers"
        /// <returns>value in "00:00:00" (hours : min : sec) format</returns>
        public string GetStringValue()
        {
            return $"{values.hours:00}:{values.minutes:00}:{values.seconds:00}";
        }

        public static float ConvertAndGetValue(in TimerData values, TimeType timeType)
        {
            float sec = values.days * 86400.0f + values.hours * 3600.0f + values.minutes * 60.0f + values.seconds;
            return timeType switch
            {
                TimeType.Sec => sec,
                TimeType.Min => sec / 60.0f,
                TimeType.Hours => sec / 3600.0f, //60 * 60
                TimeType.Days => sec / 86400.0f, //60 * 60 * 24
                _ => sec
            };
        }

        public float ConvertAndGetValue(TimeType timeType)
        {
            return ConvertAndGetValue(values, timeType);
        }
        #endregion

        public enum TimeType
        {
            Sec,
            Min,
            Hours,
            Days
        }
    }

    [Serializable]
    public struct TimerData
    {
        public int seconds;
        public int minutes;
        public int hours;
        public int days;

        public TimerData(TimeSpan interval)
        {
            days = interval.Days > 0 ? interval.Days : 0;
            hours = interval.Hours > 0 ? interval.Hours : 0;
            minutes = interval.Minutes > 0 ? interval.Minutes : 0;
            seconds = interval.Seconds > 0 ? interval.Seconds : 0;
        }

        public TimerData(int days, int hours, int minutes, int seconds)
        {
            this.days = days;
            this.hours = hours;
            this.minutes = minutes;
            this.seconds = seconds;
        }

        public static TimerData Empty => new TimerData(0, 0, 0, 1);
    }
}
