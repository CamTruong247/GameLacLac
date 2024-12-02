using System;
using UnityEngine;
using TMPro; // Nếu dùng InputField, thay bằng UnityEngine.UI
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Openchallenge : NetworkBehaviour
{
    public TMP_InputField monsterPerWaveInput;
    public TMP_InputField numberOfWavesInput;
    public TMP_InputField waveDurationInput;

    public ChallengeSettings challengeSettings; // Gắn ChallengeSettings asset vào đây

    public void OnClick()
    {
        if (int.TryParse(monsterPerWaveInput.text, out int monsterPerWave) &&
            int.TryParse(numberOfWavesInput.text, out int numberOfWaves) &&
            float.TryParse(waveDurationInput.text, out float waveDuration))
        {
            // Giới hạn giá trị
            if (monsterPerWave <= 16 && numberOfWaves <= 10 && waveDuration <= 40f)
            {
                // Lưu dữ liệu vào ChallengeSettings
                challengeSettings.monsterPerWave = monsterPerWave;
                challengeSettings.numberOfWaves = numberOfWaves;
                challengeSettings.waveDuration = waveDuration;

                // Chuyển scene
                NetworkManager.Singleton.SceneManager.LoadScene("Challenge", LoadSceneMode.Single);

            }
            else
            {
                Debug.LogError("Giá trị vượt quá giới hạn!");
            }
        }
        else
        {
            Debug.LogError("Vui lòng nhập đúng định dạng số!");
        }
    }
}
