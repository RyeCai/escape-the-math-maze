using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// functionality of a water/soap (whatever) puddle
// checks if the player is on water/soap puddle
public class Water : MonoBehaviour
{
    private GameObject fps_player_obj;
    private Level level;
    private AudioSource source;
    private AudioClip water_sound;

    void Start()
    {
        GameObject level_obj = GameObject.FindGameObjectWithTag("Level");
        level = level_obj.GetComponent<Level>();
        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        water_sound = level.water_sound;
        source.clip = water_sound;
        if (level == null)
        {
            Debug.LogError("Internal error: could not find the Level object - did you remove its 'Level' tag?");
            return;
        }
        fps_player_obj = level.fps_player_obj;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "PLAYER")
        {
            source.PlayOneShot(water_sound, 1F);
            level.player_is_on_water = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "PLAYER")
        {
            
            level.player_is_on_water = false;
        }
    }
}