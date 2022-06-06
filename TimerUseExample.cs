/*
 * autor: Kogan Anatolii
 * e-mail: a.kogan@gooligames.com
 */
using System;
using UnityEngine;
using UnityEngine.UI;
using Gooligames.Timers;

namespace Gooligames.Tests
{
    public class TimerUseExample : MonoBehaviour
    {
        public string timerKey = "test";

        public int minutes = 1;
        public int seconds = 50;

        public Text text = null;

        private TimerController _timer = null;

        private void Awake()
        {
            Initilize();
        }

        [ContextMenu("Delete Save")]
        public void DeleteSave()
        {
            TimerSaver.DeleteSave(timerKey);
        }

        [ContextMenu("Restart Timer")]
        public void RestartTimer()
        {
            _timer.Stop();
            DeleteSave();
            MainFoo();
        }

        private void Initilize()
        {
            _timer = TimerController.GetTimerByKey(timerKey);

            _timer.OnValueChangedEvent += DisplayTimerValue;
            _timer.OnFinishedEvent += OnTimerFinished;

            DisplayTimerValue();

            MainFoo();
        }

        private async void MainFoo()
        {
            bool res;
            if (TimerSaver.TimerSaveExists(timerKey) == false)
            {
                res = await _timer.Run(DateTime.Now.AddMinutes(minutes).AddSeconds(seconds));
            }
            else
            {
                res = await _timer.Continue();
            }

            if (res == true)
            {
                //logic after timer finished
            }
            else
            {
                //logic after timer stopped, but didn't finished
            }
        }

        private void DisplayTimerValue()
        {
            text.text = _timer.GetStringValue();
        }

        private void OnTimerFinished()
        {
            Debug.Log("Timer Finished");
        }
    }
}