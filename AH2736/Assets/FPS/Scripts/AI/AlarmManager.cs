using UnityEngine;
using Unity.FPS.AI;
using Unity.FPS.Game;
using NUnit;


namespace AH2736
{
    // ++ Caller for Alarm System
    // + Attached as component to scene's GameManager
    // + Instructs what to do when an Alarm Event is broadcast
    // Call an OnAlarmTriggered() method
    // for all enemies within a radius of the source
    
    public class AlarmManager : MonoBehaviour
    {
        
        // + Make a list of all enemies in the scene
        private EnemyMobile[] allEnemies;

        void OnEnable()
        {
            // Listen for Alarm Events
            EventManager.AddListener<AreaAlarmEvent>(OnAlarmRaised);
        }

        void OnDisable()
        {
            // Don't listen for alarm events
            EventManager.RemoveListener<AreaAlarmEvent>(OnAlarmRaised);
        }

        void Start()
        {
            // Find all mobile enemies in the scene and hold the list
            allEnemies = FindObjectsByType<EnemyMobile>(FindObjectsSortMode.None);
        }


        // ++ Trigger for the Alarm
        // Happens somewhere out in the scene. 
        // Called by a hold area objective, security camera, story point etc...
        void OnAlarmRaised(AreaAlarmEvent alarm)
        {
            // If an alarm is raised (i.e. by Hold Area Objective)
            // then trigger the alarm at the appropriate origin
            TriggerAlarm(alarm.info.origin, alarm.info.range);
        }
        
        // ++ Method called when alarm is triggered
        // Determine which enemies are within the specified range of the alarm
        // Tell those enemies to respond.
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
