using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rainManager : MonoBehaviour
{
    private GameManager _GameManager;
    public bool isRain;

    // Start is called before the first frame update
    void Start()
    {
        _GameManager = FindObjectOfType(typeof(GameManager)) as GameManager;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            _GameManager.OnOffRain(isRain);
        }
    }
}
