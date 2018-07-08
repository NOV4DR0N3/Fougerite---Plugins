using Fougerite;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// System Timer Easy - version 1.1
/// * Timer Once
/// * Timer Repeat
/// </summary>

namespace TimerEdit
{
    public class TimerEditStart
    {

        //timer
        private static GameObject LoadTime;

        //active
        public static void Init()
        {

            // clear olds timers actives
            TimerEvento[] timersAtual = UnityEngine.Object.FindObjectsOfType<TimerEvento>();
            foreach (TimerEvento obj2 in timersAtual)
            {
                GameObject.Destroy(obj2);
            }

            LoadTime = new GameObject();
            LoadTime.AddComponent<TimerEvento>();
            UnityEngine.Object.DontDestroyOnLoad(LoadTime);
        }

        //desactive
        public static void DeInitialize()
        {
            TimerEvento.ClearTimer();
            GameObject.Destroy(LoadTime);
        }
    }

    public class TimerEvento : MonoBehaviour
    {
        private static bool WarnBug = false;
        private static Dictionary<float, TimerAction> listTimers = new Dictionary<float, TimerAction>();
        public float TimerAtual;
        public float antiCrach = 0;
        public static float timerSync = 0;

        public class TimerAction
        {
            public Action acao;
            public int repeat;
            public float time;

            public TimerAction(float time_, int repeat_, Action acao_)
            {
                time = time_;
                repeat = repeat_;
                acao = acao_;
            }
        }

        private void Update()
        {

            if (listTimers.Count > 0 && UnityEngine.Time.realtimeSinceStartup - antiCrach >= 1)
            {
                try
                {
                    if (!WarnBug && listTimers.Count > 1000)
                    {
                        WarnBug = true;
                        Logger.LogDebug("[TimerEvento] Error WarnBug!\n[TimerEvento] Error WarnBug!\n[TimerEvento] Error WarnBug!\n");
                    }

                    //antiCrach - Reduces the milliseconds update rate to seconds
                    antiCrach = UnityEngine.Time.realtimeSinceStartup;

                    //Validates the timer to activate
                    for (int i = 0; i < listTimers.Count; i++)
                    {
                        var time = listTimers.ElementAt(i);
                        if (time.Key < UnityEngine.Time.realtimeSinceStartup)
                        {
                            try
                            {
                                time.Value.acao();
                            }
                            catch (Exception ex)
                            {
                                Logger.LogDebug("[TimerEvento] Error in active Action! " + ex);
                            }


                            if (time.Value.repeat == -1)
                            {
                                AntiExistente(time.Value);
                            }
                            else if (time.Value.repeat > 0)
                            {
                                time.Value.repeat--;
                                AntiExistente(time.Value);
                            }

                            listTimers.Remove(time.Key);
                        }

                    }
                }
                catch (Exception ex)
                {
                    Logger.LogDebug("[TimerEvento] Error Update! " + ex);
                }

            }

        }

        //Responsible for adding the timers without repeating Key
        public static void AntiExistente(TimerAction callback)
        {
            try
            {
                timerSync = UnityEngine.Time.realtimeSinceStartup + callback.time;
                if (listTimers.ContainsKey(timerSync))
                {
                    callback.time += 0.05001f;
                    AntiExistente(callback);
                }
                else
                {
                    //debug    UnityEngine.Debug.Log("timer current: " + UnityEngine.Time.realtimeSinceStartup.ToString() + "  timer event: " + timerSync.ToString());
                    listTimers.Add(timerSync, callback);
                }
            }
            catch (Exception ex)
            {
                Logger.LogDebug("[TimerEvento] Error AntiExistente! " + ex);
            }

        }

        //Create timer  dalay=seconds  callback=action/function
        public static void Once(float delay, Action callback)
        {
            try
            {
                AntiExistente(new TimerAction(delay, 0, callback));
            }
            catch (Exception ex)
            {
                Logger.LogDebug("[TimerEvento] Error Once! " + ex);
            }

        }

        //Create timer with repetitions  dalay=seconds  reps=0(infinit)  callback=action/function
        public static void Repeat(float delay, int reps, Action callback)
        {
            try
            {
                if (reps == 0)
                {
                    reps = -1;//infinit
                }
                AntiExistente(new TimerAction(delay, reps, callback));
            }
            catch (Exception ex)
            {
                Logger.LogDebug("[TimerEvento] Error Repeat! " + ex);
            }

        }

        public static void ClearTimer()
        {
            listTimers.Clear();
        }

    }
}