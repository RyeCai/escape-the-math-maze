using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpManager : MonoBehaviour
{
    private float powerUpDuration = 0.0f;
    public Canvas indicator;
    // Start is called before the first frame update
    void Start()
    {
        indicator.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        powerUpDuration -= Time.deltaTime;
        if(powerUpDuration <= 0.0f){
            StaticData.invisible = false;
            indicator.enabled = false;
        }
    }
    void OnCollisionEnter(Collision collision){
        if(collision.gameObject.tag == "Invisible"){
            indicator.enabled = true;
            StaticData.invisible = true;
            powerUpDuration = 5.0f;
            Destroy(collision.gameObject);
        }
    }
}
