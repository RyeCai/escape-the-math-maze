using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    private GameObject fps_player_obj;
    // private Level level;
    private float radius_of_search_for_player;
    public float monster_speed;
    public float view_angle;
    // Start is called before the first frame update
    void Start()
    {
        // GameObject level_obj = GameObject.FindGameObjectWithTag("Level");
        // level = level_obj.GetComponent<Level>(); 
        // if (level == null)
        // {
        //     Debug.LogError("Internal error: could not find the Level object - did you remove its 'Level' tag?");
        //     return;
        // }
        fps_player_obj = GameObject.FindGameObjectWithTag("PLAYER");
        radius_of_search_for_player = 10.0f;
    }

    // Update is called once per frame
    void Update()
    {
        // raycast cone and check if player is in it, make walls block vision and maybe other obstacles as well
        bool inVisionCone = true;
        if(inVisionCone){
            // move animation hopping
            Vector3 v = fps_player_obj.transform.position - transform.position;
            float dist = v.magnitude;
            // collision checker, currently can get player stuck
            if (radius_of_search_for_player >= dist)
            {
                Vector3 direction = v / dist;
                Vector3 new_pos = new Vector3(direction.x * monster_speed * Time.deltaTime, 0, direction.z * monster_speed * Time.deltaTime);
                new_pos += transform.position;
                new_pos.Set(new_pos.x, transform.position.y, new_pos.z);
                transform.position = new_pos;
            }
        }else{
            // move randomly
        }
    }

//     public Vector3 DirFromAngle(float degrees, bool angleIsGlobal){
//         if(!angleIsGlobal){
//             degrees += transform.eulerAngles.y;
//         }
//         return new Vector3(Mathf.Sin(degrees * Mathf.Deg2Rad),0,Mathf.Cos(degress * Mathf.Deg2Rad));
//     }
}