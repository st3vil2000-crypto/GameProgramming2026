using UnityEngine;
using Unity.FPS.AI;
using Unity.FPS.Game;
using NUnit;


namespace AH2736
{
    public class AlarmManager : MonoBehaviour
    {
        private EnemyMobile[] allEnemies;

        void OnEnable()
        {
            // Listen for Alarm Events
            EventManager.AddListener<AreaAlarmEvent>(OnAlarmRaised);
        }

        void OnDisable()
        {
            EventManager.RemoveListener<AreaAlarmEvent>(OnAlarmRaised);
        }

        void OnAlarmRaised(AreaAlarmEvent alarm)
        {
            TriggerAlarm(alarm.info.origin, alarm.info.range);
        }
        
        void Start()
        {
            // Find all mobile enemies in the scene and hold the list
            allEnemies = FindObjectsByType<EnemyMobile>(FindObjectsSortMode.None);
        }

        // Method called when alarm is triggered
        public void TriggerAlarm(Vector3 origin, float range)
        {
            foreach (EnemyMobile enemy in allEnemies)
            {
                // bypass enemies that have been killed
                if (enemy == null) continue;

                // calculate distance from enemy to the source of the alarm
                float distance = Vector3.Distance(origin, enemy.transform.position);

                if (distance <= range)
                {
                    // call the bridging method within EnemyMobile
                    enemy.OnAlarmTriggered();

                    Debug.Log("Alarm Triggered by Alarm Manager");
                }

            }
        }

    }


}
