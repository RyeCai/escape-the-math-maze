﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// functionality of virus
// animates color and size of the virus
// and attacks the player if the player is near the virus (check the code)
public class Virus : MonoBehaviour
{
    private GameObject fps_player_obj;
    private Level level;
    private float radius_of_search_for_player;
    private float virus_speed;
    private GameObject audio_holder;
    private AudioSource source;
    private AudioClip virus_sound;

    void Start ()
    {
        GameObject level_obj = GameObject.FindGameObjectWithTag("Level");
        level = level_obj.GetComponent<Level>();
        audio_holder = GameObject.FindGameObjectWithTag("MainCamera");
        source = audio_holder.gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        virus_sound = level.virus_sound;
        source.clip = virus_sound;
        if (level == null)
        {
            Debug.LogError("Internal error: could not find the Level object - did you remove its 'Level' tag?");
            return;
        }
        fps_player_obj = level.fps_player_obj;
        Bounds bounds = level.GetComponent<Collider>().bounds;
        radius_of_search_for_player = (bounds.size.x + bounds.size.z) / 10.0f;
        virus_speed = level.virus_speed;
    }

    // *** YOU NEED TO COMPLETE THIS PART OF THE FUNCTION TO ANIMATE THE VIRUS ***
    // so that it moves towards the player when the player is within radius_of_search_for_player
    // a simple strategy is to update the position of the virus
    // so that it moves towards the direction d=v/||v||, where v=(fps_player_obj.transform.position - transform.position)
    // with rate of change (virus_speed * Time.deltaTime)
    // make also sure that the virus y-coordinate position does not go above the wall height
    void Update()
    {
        float y_position = Mathf.Min(transform.position.y, level.storey_height);
        transform.position = new Vector3(transform.position[0], y_position, transform.position[2]);
        if (level.player_health < 0.001f || level.player_entered_house)
            return;
        Color redness = new Color
        {
            r = Mathf.Max(1.0f, 0.25f + Mathf.Abs(Mathf.Sin(2.0f * Time.time)))
        };
        if ( transform.childCount > 0)
            transform.GetChild(0).GetComponent<MeshRenderer>().material.color = redness;
        else
            transform.GetComponent<MeshRenderer>().material.color = redness;
        transform.localScale = new Vector3(
                               0.9f + 0.2f * Mathf.Abs(Mathf.Sin(4.0f * Time.time)), 
                               0.9f + 0.2f * Mathf.Abs(Mathf.Sin(4.0f * Time.time)), 
                               0.9f + 0.2f * Mathf.Abs(Mathf.Sin(4.0f * Time.time))
                               );
        /*** implement the rest ! */
        Vector3 v = fps_player_obj.transform.position - transform.position;
        float dist = v.magnitude;
        if (radius_of_search_for_player >= dist)
        {
            
            Vector3 direction = v / dist;
            Vector3 new_pos = new Vector3(direction.x * virus_speed * Time.deltaTime, 0, direction.z * virus_speed * Time.deltaTime);
            new_pos += transform.position;
            new_pos.Set(new_pos.x, y_position, new_pos.z);
            transform.position = new_pos;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "PLAYER")
        {
            source.PlayOneShot(virus_sound, 1F);
            if (!level.virus_landed_on_player_recently)
                level.timestamp_virus_landed = Time.time;
            level.num_virus_hit_concurrently++;
            level.virus_landed_on_player_recently = true;
            Destroy(gameObject);
        }
    }
    
}