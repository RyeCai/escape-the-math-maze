using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Status : MonoBehaviour
{

    public AudioSource hit_sound;
    public AudioSource invis_sound;
    public AudioSource heal_sound;
    public AudioSource lose_sound;
    public int health;
    public Canvas LoseScreen;
    public Canvas indicator;
    public int maxHealth;
    public Image[] hearts;
    public Sprite heart;
    public Sprite emptyHeart;
    // public TMP_Text time;
    private float waitTime;
    private float powerUpDuration;
    // Start is called before the first frame update
    void Start()
    {   
        // StaticData.time = 0.0f;
        // time.text = "Time: " + StaticData.time.ToString("0.00");
        powerUpDuration = 0.0f;
        StaticData.health = 3;
        waitTime = 0;
        for(int i = 0; i < hearts.Length; i++){
            hearts[i].sprite = heart;
        }
        LoseScreen.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {   
        // StaticData.time += Time.deltaTime;
        // time.text = "Time: " + StaticData.time.ToString("0.00");
        StaticData.health = health;
        if(StaticData.health <= 0){
            lose_sound.Play();
            LoseScreen.gameObject.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
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
                hit_sound.Play();
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
            invis_sound.Play();
            indicator.enabled = true;
            StaticData.invisible = true;
            powerUpDuration = 12.0f;
            Destroy(collision.gameObject);
        }
        if(collision.gameObject.tag == "Heal" && health<3){
            heal_sound.Play();
            health++;
            Destroy(collision.gameObject);
        }
    }
}
