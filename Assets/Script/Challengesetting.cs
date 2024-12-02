using UnityEngine;

[CreateAssetMenu(fileName = "ChallengeSettings", menuName = "GameSettings/ChallengeSettings")]
public class ChallengeSettings : ScriptableObject
{
    public int monsterPerWave;
    public int numberOfWaves;
    public float waveDuration;
}
