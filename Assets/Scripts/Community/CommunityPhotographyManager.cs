using UnityEngine;
using LensOfLegends.Core;
using LensOfLegends.Gallery;

namespace LensOfLegends.Community
{
    /// <summary>
    /// High-level MonoBehaviour service wrapping ICommunityDataSource.
    /// Defaults to OfflineDemoCommunityDataSource so the community interface works reliably offline.
    /// </summary>
    public class CommunityPhotographyManager : MonoBehaviour
    {
        public static CommunityPhotographyManager Instance { get; private set; }

        public ICommunityDataSource DataSource { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            DataSource = new OfflineDemoCommunityDataSource();
        }

        public void PublishPhotoToDemoFeed(PhotoMetadata photo)
        {
            if (photo == null || DataSource == null) return;
            if (photo.isPublishedToDemoCommunity)
            {
                if (GameStateManager.Instance != null)
                    GameStateManager.Instance.ShowToastNotification("This photograph is already shared in the local Community Demo feed.");
                return;
            }

            DataSource.PublishPlayerPhotograph(photo, null);
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.RecordCommunityPublishReward();
                GameStateManager.Instance.ShowToastNotification(
                    "Published to Local Community Demo Feed! (+50 Photographer XP)");
            }
        }
    }
}
