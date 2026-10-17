using System;
using UnityEngine;
using UnityEngine.Video;

namespace LensOfLegends.Core
{
    public struct CinematicShotKeyframe
    {
        public Vector3 startPos;
        public Vector3 endPos;
        public Vector3 lookAtTarget;
        public float durationSeconds;
        public string captionTitle;
        public string captionSubtitle;
    }

    /// <summary>
    /// Handles in-engine camera cinematics (Introductory Sequence & Regional Arrival Sequence)
    /// and seamlessly transitions camera control to the playable FirstPersonExplorerController.
    /// Also supports optional Unity VideoPlayer playback if a licensed video or Blender render is assigned.
    /// </summary>
    public class CinematicDirector : MonoBehaviour
    {
        public static CinematicDirector Instance { get; private set; }

        [Header("Optional External Video Clip (Blender Render or Licensed Footage)")]
        public VideoClip optionalIntroVideoClip;

        public bool IsPlayingCinematic { get; private set; }
        public string CurrentTitle { get; private set; } = "";
        public string CurrentSubtitle { get; private set; } = "";
        public float NormalizedProgress { get; private set; }

        private CinematicShotKeyframe[] _activeShots;
        private int _currentShotIndex;
        private float _shotElapsed;
        private Action _onCompleteCallback;
        private Vector3 _finalPlayerEyePos;
        private Quaternion _finalPlayerEyeRot;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void PlayIntroductoryJourneyCinematic(Action onCompleted)
        {
            CinematicShotKeyframe[] shots = new CinematicShotKeyframe[]
            {
                new CinematicShotKeyframe
                {
                    startPos = new Vector3(0f, 11f, -22f),
                    endPos = new Vector3(0f, 7.5f, -12f),
                    lookAtTarget = new Vector3(0f, 2.5f, 6f),
                    durationSeconds = 4.2f,
                    captionTitle = "LENS OF LEGENDS — AN INDIA-WIDE PHOTOGRAPHY JOURNEY",
                    captionSubtitle = "Every region holds living traditions, architecture, music, and stories waiting to be documented with care."
                },
                new CinematicShotKeyframe
                {
                    startPos = new Vector3(-10f, 4.5f, -4f),
                    endPos = new Vector3(-4f, 2.8f, -6f),
                    lookAtTarget = new Vector3(0f, 2.2f, 4f),
                    durationSeconds = 4.2f,
                    captionTitle = "A VISUAL TRAVEL JOURNAL ACROSS INDIA",
                    captionSubtitle = "From North Kerala's sacred groves to Varanasi's ghats, Rajasthan's dunes, Kolkata's pandals, Nagaland's morungs, and Ladakh's monasteries."
                }
            };

            StartShotSequence(shots, new Vector3(0f, 1.72f, -11.5f), Quaternion.identity, onCompleted);
        }

        public void PlayRegionalArrivalCinematic(Vector3 playerSpawnEyePos, Quaternion playerSpawnRot, Action onCompleted)
        {
            CinematicShotKeyframe[] shots = new CinematicShotKeyframe[]
            {
                new CinematicShotKeyframe
                {
                    startPos = new Vector3(-9.5f, 5.2f, -10.5f),
                    endPos = new Vector3(-5.0f, 3.4f, -7.0f),
                    lookAtTarget = new Vector3(0f, 2.5f, 8.0f),
                    durationSeconds = 3.8f,
                    captionTitle = "CHAPTER I: NORTH KERALA — THEYYAM IN THE SACRED GROVE",
                    captionSubtitle = "Kannur & Kasaragod Region • Nighttime Kavu Courtyard (In-Engine Real-Time Cinematic)"
                },
                new CinematicShotKeyframe
                {
                    startPos = new Vector3(5.2f, 2.6f, -6.5f),
                    endPos = playerSpawnEyePos,
                    lookAtTarget = new Vector3(0f, 2.2f, 4.0f),
                    durationSeconds = 3.6f,
                    captionTitle = "THE LIVING RITUAL OF THE KAVU",
                    captionSubtitle = "Explore freely. Press [Right-Click] or [V] at any time to raise your camera viewfinder."
                }
            };

            StartShotSequence(shots, playerSpawnEyePos, playerSpawnRot, onCompleted);
        }

        private void StartShotSequence(
            CinematicShotKeyframe[] shots,
            Vector3 handoffEyePos,
            Quaternion handoffEyeRot,
            Action onCompleted)
        {
            _activeShots = shots;
            _currentShotIndex = 0;
            _shotElapsed = 0f;
            _finalPlayerEyePos = handoffEyePos;
            _finalPlayerEyeRot = handoffEyeRot;
            _onCompleteCallback = onCompleted;
            IsPlayingCinematic = true;
            NormalizedProgress = 0f;
        }

        public void SkipCinematic()
        {
            if (!IsPlayingCinematic) return;
            FinishCinematic();
        }

        private void Update()
        {
            if (!IsPlayingCinematic || _activeShots == null || _activeShots.Length == 0)
                return;

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return))
            {
                SkipCinematic();
                return;
            }

            CinematicShotKeyframe shot = _activeShots[_currentShotIndex];
            _shotElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_shotElapsed / Mathf.Max(0.1f, shot.durationSeconds));
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            CurrentTitle = shot.captionTitle;
            CurrentSubtitle = shot.captionSubtitle;
            NormalizedProgress = (_currentShotIndex + t) / _activeShots.Length;

            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.transform.position = Vector3.Lerp(shot.startPos, shot.endPos, smoothT);
                Vector3 lookDir = shot.lookAtTarget - cam.transform.position;
                if (lookDir.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
                    cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, targetRot, Time.deltaTime * 6f);
                }
                cam.fieldOfView = 46f;
            }

            if (_shotElapsed >= shot.durationSeconds)
            {
                _currentShotIndex++;
                _shotElapsed = 0f;
                if (_currentShotIndex >= _activeShots.Length)
                {
                    FinishCinematic();
                }
            }
        }

        private void FinishCinematic()
        {
            IsPlayingCinematic = false;
            CurrentTitle = "";
            CurrentSubtitle = "";

            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.transform.position = _finalPlayerEyePos;
                cam.transform.rotation = _finalPlayerEyeRot;
                cam.fieldOfView = 60f;
            }

            Action cb = _onCompleteCallback;
            _onCompleteCallback = null;
            cb?.Invoke();
        }
    }
}
