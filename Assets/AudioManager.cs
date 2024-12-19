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
    private void EnsureSingleAudioListener()
    {
        AudioListener[] listeners = FindObjectsOfType<AudioListener>();
        for (int i = 1; i < listeners.Length; i++) // Bắt đầu từ index 1 để giữ lại listener đầu tiên
        {
            Destroy(listeners[i]); // Xóa các AudioListener dư thừa
        }
    }
    private void PlayMusicForCurrentScene()
    {
        EnsureSingleAudioListener();
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