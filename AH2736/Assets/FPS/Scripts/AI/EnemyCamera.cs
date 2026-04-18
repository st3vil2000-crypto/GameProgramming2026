//using log4net.Util;
using Unity.FPS.AI;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.UIElements;
using static Unity.FPS.AI.EnemyMobile;
using static Unity.FPS.AI.EnemyTurret;

namespace Unity.FPS.AI
{
    [RequireComponent(typeof(EnemyController))]
    public class EnemyCamera : MonoBehaviour

    {
        public enum AIState
        {
            Scan,
            Attack,
            Alarm,
        }

        public Transform TurretPivot;
        public Transform TurretAimPoint;
        public Animator Animator;
        public float AimRotationSharpness = 5f;
        public float LookAtRotationSharpness = 2.5f;
        public float DetectionFireDelay = 1f;
        public float AimingTransitionBlendTime = 1f;

        [Tooltip("The random hit damage effects")]
        public ParticleSystem[] RandomHitSparks;

        public ParticleSystem[] OnDetectVfx;
        public AudioClip OnDetectSfx;

        public AIState AiState { get; private set; }

        EnemyController m_EnemyController;
        Health m_Health;
        Quaternion m_RotationWeaponForwardToPivot;
        float m_TimeStartedDetection;
        float m_TimeLostDetection;
        Quaternion m_PreviousPivotAimingRotation;
        Quaternion m_PivotAimingRotation;

        const string k_AnimOnDamagedParameter = "OnDamaged";
        const string k_AnimIsActiveParameter = "IsActive";

        void Start()
        {
            AiState = AIState.Scan;
        }

        void UpdateCurrentAiState()
        {
            switch (AiState)
            {
                case AIState.Scan:
                    // Kevyt "etsintä" – ei tee vielä mitään aggressiivista 
                    m_PivotAimingRotation = Quaternion.Slerp(
                        m_PreviousPivotAimingRotation,
                        TurretPivot.rotation,
                        LookAtRotationSharpness * Time.deltaTime
                    );
                    Debug.Log("Camera Scan");
                    break;

                case AIState.Attack:
                    bool mustShoot = Time.time > m_TimeStartedDetection + DetectionFireDelay;

                    Vector3 directionToTarget =
                        (m_EnemyController.KnownDetectedTarget.transform.position - TurretAimPoint.position).normalized;

                    Quaternion offsettedTargetRotation =
                        Quaternion.LookRotation(directionToTarget) * m_RotationWeaponForwardToPivot;

                    m_PivotAimingRotation = Quaternion.Slerp(
                        m_PreviousPivotAimingRotation,
                        offsettedTargetRotation,
                        (mustShoot ? AimRotationSharpness : LookAtRotationSharpness) * Time.deltaTime
                    );

                    if (mustShoot)
                    {
                        Vector3 correctedDirectionToTarget =
                            (m_PivotAimingRotation * Quaternion.Inverse(m_RotationWeaponForwardToPivot))
        *
                            Vector3.forward;

                        m_EnemyController.TryAtack(TurretAimPoint.position + correctedDirectionToTarget);
                    }
                    Debug.Log("Camera Attack");
                    break;

                case AIState.Alarm:
                    // Alarm-tila: esim. nopeampi kääntyminen tai "hälytystila" 
                    m_PivotAimingRotation = Quaternion.Slerp(
                        m_PreviousPivotAimingRotation,
                        TurretPivot.rotation,
                        AimRotationSharpness * 2f * Time.deltaTime
                    );
                    Debug.Log("Camera Alarm");
                    break;
            }
        }
        void OnDetectedTarget()
        {
            if (AiState == AIState.Scan)
            {
                AiState = AIState.Alarm;
            }

            for (int i = 0; i < OnDetectVfx.Length; i++)
            {
                OnDetectVfx[i].Play();
            }

            if (OnDetectSfx)
            {
                AudioUtility.CreateSFX(OnDetectSfx, transform.position,
                AudioUtility.AudioGroups.EnemyDetection, 1f);
            }
            Animator.SetBool(k_AnimIsActiveParameter, true);
            m_TimeStartedDetection = Time.time;
            // Siirtyy attackiin pienen viiveen jälkeen 
            Invoke(nameof(SetAttackState), DetectionFireDelay);
            Debug.Log("Camera Target Detect");
        }

        void SetAttackState()
        {
            if (AiState == AIState.Alarm)
            {
                AiState = AIState.Attack;
            }
        }
    }
}
