using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class Status : MonoBehaviour
{

    public int health;
    public Canvas LoseScreen;
    public Canvas indicator;
    public int maxHealth;
    public Image[] hearts;
    public Sprite heart;
    public Sprite emptyHeart;
    private float waitTime;
    private float powerUpDuration;
    // Start is called before the first frame update
    void Start()
    {
        powerUpDuration = 0.0f;
        StaticData.health = 3;
        waitTime = 0;
        for(int i = 0; i < hearts.Length; i++){
            hearts[i].sprite = heart;
        }
        LoseScreen.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {   
        StaticData.health = health;
        if(StaticData.health <= 0){
            LoseScreen.enabled = true;
        }
        waitTime -= Time.deltaTime;
        for(int i = 0; i < hearts.Length; i++){
            if(i < StaticData.health){
                hearts[i].sprite = heart;
            }else{
                hearts[i].sprite = emptyHeart;
            }
            if(i < maxHealth){
                hearts[i].enabled = true;
            }else{
                hearts[i].enabled = false;
            }
        }
        powerUpDuration -= Time.deltaTime;
        if(powerUpDuration <= 0.0f){
            StaticData.invisible = false;
            indicator.enabled = false;
        }
    }
    void OnCollisionEnter(Collision collision){
        Debug.Log(collision.gameObject.tag);
        if(waitTime <= 0){
            if(collision.gameObject.tag == "Enemy"){
                for(int i = hearts.Length-1; i > -1; i--){
                    if(hearts[i].enabled && hearts[i].sprite == heart){
                        hearts[i].sprite = emptyHeart;
                        health--;
                        waitTime = 2;
                        break;
                    }
                }
            }
        }
        if(collision.gameObject.tag == "Invisible"){
            indicator.enabled = true;
            StaticData.invisible = true;
            powerUpDuration = 5.0f;
            Destroy(collision.gameObject);
        }
    }
}
