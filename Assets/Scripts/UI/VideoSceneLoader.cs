using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

[DisallowMultipleComponent]
public class VideoSceneLoader : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private string nextSceneName = "Castillo";
    [SerializeField] private bool playOnStart = true;

    private void Awake()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        if (videoPlayer == null)
            videoPlayer = FindFirstObjectByType<VideoPlayer>();

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += HandleVideoFinished;
        }
        else
        {
            Debug.LogWarning("VideoSceneLoader: no se encontro VideoPlayer en la escena.");
        }
    }

    private void Start()
    {
        if (playOnStart && videoPlayer != null)
            videoPlayer.Play();
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= HandleVideoFinished;
    }

    private void HandleVideoFinished(VideoPlayer source)
    {
        if (string.IsNullOrWhiteSpace(nextSceneName))
            return;

        SceneManager.LoadScene(nextSceneName);
    }
}
