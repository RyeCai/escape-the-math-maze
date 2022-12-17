using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolAndSeek : MonoBehaviour
{
    UnityEngine.AI.NavMeshAgent agent;
    public Transform[] points;
    private int index;
    private Vector3 target;
    private GameObject fps_player_obj;
    private float radius_of_search_for_player;
    private Transform sight;
    private Vector3 distToPlayer;
    private Vector3 dirToPlayer;
    private Vector3 forwardVector;
    private float angle;
    // Start is called before the first frame update
    void Start()
    {
        // foreach(Transform child in transform){
        //     if(child.name == "Sight"){
        //         sight = child;
        //         break;
        //     }
        // }
        fps_player_obj = GameObject.FindGameObjectWithTag("PLAYER");
        radius_of_search_for_player = 30.0f;
        index = 0;
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();    
        target = points[index].position;
        agent.SetDestination(target);
    }

    // Update is called once per frame
    void Update()
    {
        distToPlayer = transform.position - fps_player_obj.transform.position;
        distToPlayer.y = 0;
        dirToPlayer = fps_player_obj.transform.position - transform.position;
        dirToPlayer.Normalize();
        var dot = Vector3.Dot(dirToPlayer, transform.forward);
        // sight.transform.LookAt(fps_player_obj.transform);
        // Physics.Raycast(sight.transform.position, sight.transform.forward, 100)
        Debug.Log(dot);
        RaycastHit hit;
        // if player is in sight
        if(distToPlayer.magnitude < radius_of_search_for_player && dot > 0.707 && Physics.Raycast(transform.position, dirToPlayer, out hit) && hit.collider.tag == "PLAYER"){
            target = fps_player_obj.transform.position;
            agent.SetDestination(target);
            Debug.Log("sighted");
        }else{
            Vector3 dist = transform.position - target;
            dist.y = 0;
            if(dist.magnitude < 1){
                index = (index + 1) % points.Length;
                target = points[index].position;
                agent.SetDestination(target);
            }
        }      
    }


}
