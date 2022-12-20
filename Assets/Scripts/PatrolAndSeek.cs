using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolAndSeek : MonoBehaviour
{
    private Animator animation_controller;
    UnityEngine.AI.NavMeshAgent agent;
    private Vector3[] points;
    private int index;
    private Vector3 target;
    private GameObject fps_player_obj;
    private float radius_of_search_for_player;
    private Vector3 distToPlayer;
    private Vector3 dirToPlayer;
    private Vector3 forwardVector;
    private float angle;
    private float xMax;
    private float xMin;
    private float zMax;
    private float zMin;
    // Start is called before the first frame update
    void Start()
    {
        xMax = Mathf.Min(transform.position.x + 10, 64);
        xMin = Mathf.Max(transform.position.x - 10, 0);
        zMax = Mathf.Min(transform.position.z + 10, 64);
        zMin = Mathf.Max(transform.position.z - 10, 0);
        fps_player_obj = GameObject.FindGameObjectWithTag("PLAYER");
        radius_of_search_for_player = 10.0f;
        index = 0;
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();    
        // target = points[index].position;
        for(int i = 0; i < 2; i++){
            points[i] = new Vector3(Random.Range(xMin, xMax), 0, Random.Range(zMin, zMax));
        }
        target = points[index];
        agent.SetDestination(target);
        animation_controller = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        distToPlayer = transform.position - fps_player_obj.transform.position;
        distToPlayer.y = 0;
        dirToPlayer = fps_player_obj.transform.position - transform.position;
        dirToPlayer.Normalize();
        var dot = Vector3.Dot(dirToPlayer, transform.forward);
        RaycastHit hit;
        // if player is in sight
        if(distToPlayer.magnitude > 1 && distToPlayer.magnitude < radius_of_search_for_player && dot > 0.707 && Physics.Raycast(transform.position, dirToPlayer, out hit) && hit.collider.tag == "PLAYER" && !StaticData.invisible){
        // if(!StaticData.invisible){
        agent.SetDestination(fps_player_obj.transform.position);
            // Debug.Log("sighted");
        }else{
            agent.SetDestination(target);
            Vector3 dist = transform.position - target;
            dist.y = 0;
            if(dist.magnitude < 1){
                index = (index + 1) % points.Length;
                target = points[index];
                agent.SetDestination(target);
            }
        }      
    }


}
