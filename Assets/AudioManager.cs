using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;

    public AudioClip musicMenu;
    public AudioClip musicMap;

    private static AudioManager instance; // Singleton instance

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // Destroy duplicate AudioManager
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject); // Prevent destruction on scene load
    }

    private void Start()
    {
        PlayMusicForCurrentScene();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForCurrentScene();
    }

    private void PlayMusicForCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        if (currentSceneName == "Menu")
        {
            if (musicSource.clip != musicMenu)
            {
                musicSource.clip = musicMenu;
                musicSource.Play();
            }
        }
        else
        {
            if (musicSource.clip != musicMap)
            {
                musicSource.clip = musicMap;
                musicSource.Play();
            }
        }
    }
}
