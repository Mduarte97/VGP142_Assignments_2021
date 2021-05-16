using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManger : MonoBehaviour
{
    public int startingLives;
    public Transform spawnLocation;

    
    // Start is called before the first frame update
    void Start()
    {
        GameManager.instance.currentLevel = GetComponent<LevelManger>();
    }

   
}
