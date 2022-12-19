using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    private float powerUpDuration = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        powerUpDuration -= Time.deltaTime;
        if(powerUpDuration <= 0.0f){
            StaticData.invisible = false;
        }
    }
    void OnCollisionEnter(Collision collision){
        if(collision.gameObject.tag == "Invisible"){
            StaticData.invisible = true;
            powerUpDuration = 5.0f;
            Destroy(collision.gameObject);
        }
    }
}
