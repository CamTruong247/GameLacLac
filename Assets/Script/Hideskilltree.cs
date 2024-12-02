using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hideskilltree : MonoBehaviour
{
    public GameObject skilltree;
    public GameObject skilltreedescription;
    // Start is called before the first frame update
    void Start()
    {
       skilltree.SetActive(false);
        skilltreedescription.SetActive(false);
    }

    // Update is called once per frame
    public void HideSkillTree()
    {
        skilltree.SetActive(false);  // Ẩn skill tree
        skilltreedescription.SetActive(false); // Ẩn description
    }
}
