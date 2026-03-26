using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
 
public class VideoScript : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] string cutsceneFileName = "cutscene1.mp4";

    [Header("Rewards")]
    [SerializeField] bool grantCurrencyAfterCutscene = false;
    [SerializeField] int cutsceneCurrencyReward = 1000;

    [Header("Scene Load")]
    [SerializeField] string sceneToLoad = "Hub";

    VideoPlayer video;
 
    void Awake()
    {
        video = GetComponent<VideoPlayer>();

        if (video == null)
        {
            Debug.LogWarning("VideoPlayer component is missing.");
            return;
        }

        if (string.IsNullOrWhiteSpace(cutsceneFileName))
        {
            Debug.LogWarning("Cutscene file name is empty.");
            return;
        }

        video.url = System.IO.Path.Combine(Application.streamingAssetsPath, cutsceneFileName);
        video.Play();
        video.loopPointReached += CheckOver;
    }
 
     void CheckOver(UnityEngine.Video.VideoPlayer vp)
    {
        if (grantCurrencyAfterCutscene)
            CurrencyWallet.Add(Mathf.Max(0, cutsceneCurrencyReward));

        if (string.IsNullOrWhiteSpace(sceneToLoad))
        {
            Debug.LogWarning("video not configured properly");
            return;
        }

        SceneManager.LoadScene(sceneToLoad);
    }
}