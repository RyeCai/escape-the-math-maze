using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patrol : MonoBehaviour
{
    UnityEngine.AI.NavMeshAgent agent;
    public Transform[] points;
    private int index;
    private Vector3 target;
    // Start is called before the first frame update
    void Start()
    {
        index = 0;
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();    
        target = points[index].position;
        agent.SetDestination(target);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dist = transform.position - target;
        dist.y = 0;
        if(dist.magnitude < 1){
            index = (index + 1) % points.Length;
            target = points[index].position;
            agent.SetDestination(target);

        }      
    }


}
