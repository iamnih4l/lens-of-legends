using UnityEngine;
using LensOfLegends.Core;

namespace LensOfLegends.Regions
{
    /// <summary>
    /// Animates the procedural North Kerala Theyyam performer through ritual phases:
    /// 1. Thottam Stance & Gesture near the shrine
    /// 2. Sacred Courtyard Processional Circle around the Meleri fire
    /// 3. Kalasham Rhythmic Spin & Expressive Firelight Flourish
    /// Note: This performer is built from stylized procedural geometry as an honest placeholder
    /// for a custom sculpted/mocapped character mesh while preserving authentic North Malabar
    /// visual motifs (Thiru Mudi towering headdress, Mukhathezhuthu vermilion facial art,
    /// Uduthukettu pleated waist attire, and Chootu ceremonial fire torch).
    /// </summary>
    public class TheyyamPerformerController : MonoBehaviour
    {
        public Transform TorsoPivot;
        public Transform HeadAndMudiPivot;
        public Transform LeftArmPivot;
        public Transform RightArmPivot;
        public Transform LeftLegPivot;
        public Transform RightLegPivot;
        public Light TorchLight;

        private Vector3 _centerStagePos = new Vector3(0f, 0f, 4.0f);
        private float _ritualTimer;

        public void InitializeRig(
            Vector3 stageCenter,
            Transform torso,
            Transform headAndMudi,
            Transform leftArm,
            Transform rightArm,
            Transform leftLeg,
            Transform rightLeg,
            Light torchLight)
        {
            _centerStagePos = stageCenter;
            TorsoPivot = torso;
            HeadAndMudiPivot = headAndMudi;
            LeftArmPivot = leftArm;
            RightArmPivot = rightArm;
            LeftLegPivot = leftLeg;
            RightLegPivot = rightLeg;
            TorchLight = torchLight;
        }

        private void Update()
        {
            _ritualTimer += Time.deltaTime;
            float cycleTime = _ritualTimer % 24.0f;

            // Phase 1 (0..8s): Expressive ritual stance & rhythmic foot-stamping before the Meleri fire
            if (cycleTime < 8.0f)
            {
                float sway = Mathf.Sin(_ritualTimer * 3.2f) * 0.45f;
                transform.position = Vector3.Lerp(
                    transform.position,
                    _centerStagePos + new Vector3(sway, 0f, 0f),
                    Time.deltaTime * 4f);
                Quaternion targetRot = Quaternion.Euler(0f, 180f + Mathf.Sin(_ritualTimer * 1.8f) * 22f, 0f);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
            }
            // Phase 2 (8..18s): Sacred processional arc around the Meleri fire pit
            else if (cycleTime < 18.0f)
            {
                float phaseProgress = (cycleTime - 8.0f) / 10.0f;
                float angle = phaseProgress * Mathf.PI * 2.0f;
                float radius = 2.65f;
                Vector3 orbitPos = _centerStagePos + new Vector3(Mathf.Sin(angle) * radius, 0f, Mathf.Cos(angle) * radius);
                Vector3 moveDir = (orbitPos - transform.position);
                transform.position = Vector3.Lerp(transform.position, orbitPos, Time.deltaTime * 5f);
                if (moveDir.sqrMagnitude > 0.0005f)
                {
                    Quaternion lookRot = Quaternion.LookRotation(moveDir.normalized, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 7f);
                }
            }
            // Phase 3 (18..24s): Kalasham energetic spin and dramatic upward gesture
            else
            {
                transform.position = Vector3.Lerp(transform.position, _centerStagePos, Time.deltaTime * 5f);
                transform.Rotate(Vector3.up, 220f * Time.deltaTime, Space.World);
            }

            // Articulated limb and towering Mudi headdress animation
            float beatFreq = 5.4f;
            float stepWave = Mathf.Sin(_ritualTimer * beatFreq);
            float counterWave = Mathf.Cos(_ritualTimer * beatFreq);

            if (TorsoPivot != null)
            {
                float verticalBounce = Mathf.Abs(stepWave) * 0.09f;
                TorsoPivot.localPosition = new Vector3(0f, 1.15f + verticalBounce, 0f);
                TorsoPivot.localRotation = Quaternion.Euler(
                    Mathf.Sin(_ritualTimer * 2.7f) * 6f,
                    0f,
                    stepWave * 4.5f);
            }

            if (HeadAndMudiPivot != null)
            {
                HeadAndMudiPivot.localRotation = Quaternion.Euler(
                    Mathf.Cos(_ritualTimer * 2.7f) * 4f,
                    Mathf.Sin(_ritualTimer * 1.9f) * 10f,
                    -stepWave * 3.5f);
            }

            if (LeftArmPivot != null)
            {
                LeftArmPivot.localRotation = Quaternion.Euler(
                    -42f + stepWave * 28f,
                    -18f,
                    -25f + counterWave * 12f);
            }

            if (RightArmPivot != null)
            {
                RightArmPivot.localRotation = Quaternion.Euler(
                    -58f - stepWave * 32f,
                    18f,
                    25f - counterWave * 14f);
            }

            if (LeftLegPivot != null)
            {
                LeftLegPivot.localRotation = Quaternion.Euler(stepWave * 26f, 0f, 0f);
            }

            if (RightLegPivot != null)
            {
                RightLegPivot.localRotation = Quaternion.Euler(-stepWave * 26f, 0f, 0f);
            }

            if (TorchLight != null)
            {
                TorchLight.intensity = 2.8f + Mathf.PerlinNoise(_ritualTimer * 8f, 1.3f) * 1.4f;
            }
        }
    }
}
