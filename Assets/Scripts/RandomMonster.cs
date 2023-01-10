using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMonster : MonoBehaviour
{
    public AudioSource sighted_sound;
    private Animator animation_controller;
    UnityEngine.AI.NavMeshAgent agent;
    private Vector3 target;
    private GameObject fps_player_obj;
    private float radius_of_search_for_player;
    private Vector3 distToPlayer;
    private Vector3 dirToPlayer;
    private Vector3 forwardVector;
    private float angle;
    public GameObject plane;
    private float xMax;
    private float xMin;
    private float zMax;
    private float zMin;
    private float timer;

    
    // Start is called before the first frame update
    void Start()
    {
        fps_player_obj = GameObject.FindGameObjectWithTag("PLAYER");
        radius_of_search_for_player = 10.0f;
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        xMax=64;
        xMin=0;
        zMax=64;
        zMin=0;
        target = new Vector3(Random.Range(xMin, xMax), 0, Random.Range(zMin, zMax));
        // target = RandomNavmeshLocation(8);
        // agent.SetDestination(target);
        timer = 5.0f;
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
        // if(!StaticData.invisible && distToPlayer.magnitude > 1){
            // target = fps_player_obj.transform.position;
            agent.SetDestination(fps_player_obj.transform.position);
            sighted_sound.Play();
        }else{
            agent.SetDestination(target);
            timer -= Time.deltaTime;
            Vector3 dist = transform.position - target;
            dist.y = 0;
            if(dist.magnitude < 1 || timer <= 0.0f){
                target = new Vector3(Random.Range(xMin, xMax), 0, Random.Range(zMin, zMax));
                agent.SetDestination(target);
                timer = 5.0f;
            }
        }      
    }


}
