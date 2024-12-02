using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideWinPanel : MonoBehaviour
{
    public GameObject winpanel;
    private void Start()
    {
        winpanel.SetActive(false);
    }
}
