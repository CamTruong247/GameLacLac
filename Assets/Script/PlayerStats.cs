
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerStats : NetworkBehaviour
{
    [SerializeField] private Image healthbar;
    [SerializeField] private GameObject healingEffect;
    public float health = 100f;
    private float maxHealth = 100f;
    public SkillData healSkillData;

    private float money;

    public void Money(float money)
    {
        this.money += money;
        PlayerPrefs.GetFloat("Money",this.money);
    }

    private void Awake()
    {
        
        if (IsOwner)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey("Money"))
        {
            money = PlayerPrefs.GetFloat("Money");
        }
        // Nếu kỹ năng hồi máu tự động đã mở khóa, bắt đầu hồi máu mỗi 10 giây
        if (healSkillData.isSkillUnlocked)
        {
            StartCoroutine(AutoHeal());
        }
    }

    private IEnumerator AutoHeal()
    {
        while (true)
        {
            // Đợi 10 giây
            yield return new WaitForSeconds(10f);

            // Hồi phục 50% máu (50% của maxHealth)
            Heal(20f);
        }
    }

    private void Heal(float healAmount)
    {
        float newHealth = Mathf.Min(health + healAmount, maxHealth);

        // Nếu lượng máu thay đổi, cập nhật lại
        if (newHealth != health)
        {
            // Cập nhật lượng máu mới vào server
            UpdateHealthServerRpc(-(healAmount));  // Truyền giá trị âm để tăng máu
            health = newHealth;  // Cập nhật giá trị health trên client
            SpawnHealingEffect();
        }
    }
    private GameObject healingEffectInstance;
    private void SpawnHealingEffect()
    {
        // Kiểm tra nếu prefab healingEffectPrefab không phải là null
        if (healingEffect != null)
        {
            // Instantiate prefab healingEfct tại vị trí của người chơi
            healingEffectInstance = Instantiate(healingEffect, transform.position, Quaternion.identity);
        }
        StartCoroutine(Deplay());
    }
    private IEnumerator Deplay()
    {

        yield return new WaitForSeconds(0.5f);

        if (healingEffectInstance != null)
        {
            Debug.Log("Destroying healing effect instance.");
            Destroy(healingEffectInstance); // Hủy instance của healing effect
        }

    }
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        //Debug.Log(money);
        UpdateUIPlayerServerRpc();
        HealthBarServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void HealthBarServerRpc()
    {
        HealthBarClientRpc();
    }

    [ClientRpc]
    private void HealthBarClientRpc()
    {
        healthbar.fillAmount = health / maxHealth;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateHealthServerRpc(float health)
    {
        // Chỉ giảm máu (khi bị tấn công hoặc nhận thiệt hại)
        UpdateHealthClientRpc(health);
    }

    [ClientRpc]
    public void UpdateHealthClientRpc(float health)
    {

        this.health -= health;
        // Giảm máu dựa trên giá trị từ server
        
       // Debug.Log("Updated Health: " + this.health); // Kiểm tra giá trị máu khi cập nhật

        // Kiểm tra nếu máu <= 0 thì hủy nhân vật
        if (this.health <= 0)
        {
            // Debug.Log("Player health reached 0, destroying the object.");
            gameObject.transform.GetChild(0).gameObject.SetActive(false);
            gameObject.transform.GetChild(1).gameObject.SetActive(false);
            gameObject.transform.GetChild(2).gameObject.SetActive(false);
            gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
            gameObject.GetComponent<SummonPet>().enabled = false;
            //gameObject.GetComponent<AuraEffect>().enabled = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void HealthServerRpc(float health)
    {
        // Chỉ giảm máu (khi bị tấn công hoặc nhận thiệt hại)
        HealthClientRpc(health);
    }

    [ClientRpc]
    public void HealthClientRpc(float health)
    {
        // Giảm máu dựa trên giá trị từ server
        this.health += health;
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateUIPlayerServerRpc()
    {
        UpdateUIPlayerClientRpc();
    }

    [ClientRpc]
    private void UpdateUIPlayerClientRpc()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        if (currentSceneName == "Menu")
        {
            this.health = 100;
            gameObject.transform.GetChild(0).gameObject.SetActive(true);
            gameObject.transform.GetChild(1).gameObject.SetActive(true);
            gameObject.transform.GetChild(2).gameObject.SetActive(true);
            gameObject.GetComponent<BoxCollider2D>().isTrigger = false;
            gameObject.GetComponent<SummonPet>().enabled = true;
            //gameObject.GetComponent<AuraEffect>().enabled = true;
        }
    }
}
