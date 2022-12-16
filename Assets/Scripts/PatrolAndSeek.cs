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
    // Start is called before the first frame update
    void Start()
    {
        fps_player_obj = GameObject.FindGameObjectWithTag("PLAYER");
        radius_of_search_for_player = 10.0f;
        index = 0;
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();    
        target = points[index].position;
        agent.SetDestination(target);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 distToPlayer = transform.position - fps_player_obj.transform.position;
        distToPlayer.y = 0;
        if(distToPlayer.magnitude < radius_of_search_for_player){
            target = fps_player_obj.transform.position;
            agent.SetDestination(target);
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
