using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

enum TileType
{
    WALL = 0,
    FLOOR = 1,
    WATER = 2,
    DRUG = 3,
    VIRUS = 4,
}

public class Level : MonoBehaviour
{
    // fields/variables you may adjust from Unity's interface
    public int width = 16;   // size of level (default 16 x 16 blocks)
    public int length = 16;
    public float storey_height = 2.5f;   // height of walls
    public float virus_speed = 3.0f;     // virus velocity
    public GameObject fps_prefab;        // these should be set to prefabs as provided in the starter scene
    public GameObject virus_prefab;
    public GameObject water_prefab;
    public GameObject house_prefab;
    public GameObject text_box;
    public GameObject scroll_bar;

    // fields/variables accessible from other scripts
    internal GameObject fps_player_obj;   // instance of FPS template
    internal float player_health = 1.0f;  // player health in range [0.0, 1.0]
    internal int num_virus_hit_concurrently = 0;            // how many viruses hit the player before washing them off
    internal bool virus_landed_on_player_recently = false;  // has virus hit the player? if yes, a timer of 5sec starts before infection
    internal float timestamp_virus_landed = float.MaxValue; // timestamp to check how many sec passed since the virus landed on player
    internal bool drug_landed_on_player_recently = false;   // has drug collided with player?
    internal bool player_is_on_water = false;               // is player on water block
    internal bool player_entered_house = false;             // has player arrived in house?

    // fields/variables needed only from this script
    private Bounds bounds;                   // size of ground plane in world space coordinates 
    private float timestamp_last_msg = 0.0f; // timestamp used to record when last message on GUI happened (after 7 sec, default msg appears)
    private int function_calls = 0;          // number of function calls during backtracking for solving the CSP
    private int num_viruses = 0;             // number of viruses in the level
    private List<int[]> pos_viruses;         // stores their location in the grid

    // feel free to put more fields here, if you need them e.g, add AudioClips that you can also reference them from other scripts
    // for sound, make also sure that you have ONE audio listener active (either the listener in the FPS or the main camera, switch accordingly)
    public AudioClip home_enter;
    public AudioClip virus_sound;
    public AudioClip health_loss;
    public AudioClip water_sound;
    public AudioClip drug_sound;
    private AudioSource audio_source;
    GameObject main_cam;

    public Button restart_level;
    public Button new_level;
    private List<TileType>[,] old_grid;
    private int[] restart_arr;
    
    // a helper function that randomly shuffles the elements of a list (useful to randomize the solution to the CSP)
    private void Shuffle<T>(ref List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    private void RestartLevel(List<TileType>[,] grid, int[] restart_var)
    {
        GameObject[] gameObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject go in gameObjects)
        {
            if (go.name.Contains("COVID") || go.name.Contains("DRUG") || go.name.Contains("GRAVE") 
                || go.name.Contains("WATER") || go.name.Contains("WALL") || go.name.Contains("HOUSE"))
            {
                Destroy(go);
            }
        }
        function_calls = 0;
        player_health = 1.0f;
        num_virus_hit_concurrently = 0;
        virus_landed_on_player_recently = false;
        timestamp_virus_landed = float.MaxValue;
        drug_landed_on_player_recently = false;
        player_is_on_water = false;
        player_entered_house = false;
        restart_level.gameObject.SetActive(false);
        text_box.GetComponent<Text>().text = "Find your home!";

        DrawDungeon(grid, restart_var);
    }

    // Use this for initialization
    IEnumerator Start()
    {
        // initialize internal/private variables
        restart_arr = new int[] { };
        restart_level.gameObject.SetActive(false);
        new_level.gameObject.SetActive(false);
        main_cam = GameObject.FindGameObjectWithTag("MainCamera");
        main_cam.GetComponent<AudioListener>().enabled = false;
        bounds = GetComponent<Collider>().bounds; 
        timestamp_last_msg = 0.0f;
        function_calls = 0;
        num_viruses = 0;
        player_health = 1.0f;
        num_virus_hit_concurrently = 0;
        virus_landed_on_player_recently = false;
        timestamp_virus_landed = float.MaxValue;
        drug_landed_on_player_recently = false;
        player_is_on_water = false;
        player_entered_house = false;        

        // initialize 2D grid
        List<TileType>[,] grid = new List<TileType>[width, length];
        // useful to keep variables that are unassigned so far
        List<int[]> unassigned = new List<int[]>();

        // will place x viruses in the beginning (at least 1). x depends on the sise of the grid (the bigger, the more viruses)        
        num_viruses = width * length / 25 + 1; // at least one virus will be added
        pos_viruses = new List<int[]>();
        // create the wall perimeter of the level, and let the interior as unassigned
        // then try to assign variables to satisfy all constraints
        // *rarely* it might be impossible to satisfy all constraints due to initialization
        // in this case of no success, we'll restart the random initialization and try to re-solve the CSP
        audio_source = gameObject.AddComponent<AudioSource>();
        audio_source.clip = health_loss;
        bool success = false;        
        while (!success)
        {
            for (int v = 0; v < num_viruses; v++)
            {
                while (true) // try until virus placement is successful (unlikely that there will no places)
                {
                    // try a random location in the grid
                    int wr = Random.Range(1, width - 1);
                    int lr = Random.Range(1, length - 1);

                    // if grid location is empty/free, place it there
                    if (grid[wr, lr] == null)
                    {
                        grid[wr, lr] = new List<TileType> { TileType.VIRUS };
                        pos_viruses.Add(new int[2] { wr, lr });
                        break;
                    }
                }
            }

            for (int w = 0; w < width; w++)
                for (int l = 0; l < length; l++)
                    if (w == 0 || l == 0 || w == width - 1 || l == length - 1)
                        grid[w, l] = new List<TileType> { TileType.WALL };
                    else
                    {
                        if (grid[w, l] == null) // does not have virus already or some other assignment from previous run
                        {
                            // CSP will involve assigning variables to one of the following four values (VIRUS is predefined for some tiles)
                            List<TileType> candidate_assignments = new List<TileType> { TileType.WALL, TileType.FLOOR, TileType.WATER, TileType.DRUG };
                            Shuffle<TileType>(ref candidate_assignments);

                            grid[w, l] = candidate_assignments;
                            unassigned.Add(new int[] { w, l });
                        }
                    }

            // YOU MUST IMPLEMENT this function!!!
            success = BackTrackingSearch(grid, unassigned);
            if (!success)
            {
                Debug.Log("Could not find valid solution - will try again");
                unassigned.Clear();
                grid = new List<TileType>[width, length];
                function_calls = 0; 
            }
            yield return null;
        }
        DrawDungeon(grid, restart_arr);
    }

    // one type of constraint already implemented for you
    bool DoWeHaveTooManyInteriorWallsORWaterORDrug(List<TileType>[,] grid)
    {
        int[] number_of_assigned_elements = new int[] { 0, 0, 0, 0, 0 };
        for (int w = 0; w < width; w++)
            for (int l = 0; l < length; l++)
            {
                if (w == 0 || l == 0 || w == width - 1 || l == length - 1)
                    continue;
                if (grid[w, l].Count == 1)
                    number_of_assigned_elements[(int)grid[w, l][0]]++;
            }

        if ((number_of_assigned_elements[(int)TileType.WALL] > num_viruses * 10) ||
             (number_of_assigned_elements[(int)TileType.WATER] > (width + length) / 4) ||
             (number_of_assigned_elements[(int)TileType.DRUG] >= num_viruses / 2))
            return true;
        else
            return false;
    }

    // another type of constraint already implemented for you
    bool DoWeHaveTooFewWallsORWaterORDrug(List<TileType>[,] grid)
    {
        int[] number_of_potential_assignments = new int[] { 0, 0, 0, 0, 0 };
        for (int w = 0; w < width; w++)
            for (int l = 0; l < length; l++)
            {
                if (w == 0 || l == 0 || w == width - 1 || l == length - 1)
                    continue;
                for (int i = 0; i < grid[w, l].Count; i++)
                    number_of_potential_assignments[(int)grid[w, l][i]]++;
            }

        if ((number_of_potential_assignments[(int)TileType.WALL] < (width * length) / 4) ||
             (number_of_potential_assignments[(int)TileType.WATER] < num_viruses / 4) ||
             (number_of_potential_assignments[(int)TileType.DRUG] < num_viruses / 4))
            return true;
        else
            return false;
    }

    // *** YOU NEED TO COMPLETE THIS FUNCTION  ***
    // must return true if there are three (or more) interior consecutive wall blocks either horizontally or vertically
    // by interior, we mean walls that do not belong to the perimeter of the grid
    // e.g., a grid configuration: "FLOOR - WALL - WALL - WALL - FLOOR" is not valid
    bool TooLongWall(List<TileType>[,] grid)
    {
        /*** implement the rest ! */

        for (int w = 1; w < width - 1; w++)
        {
            for (int l = 1; l < length - 1; l++)
            {
                if (grid[w, l].Count == 1 && grid[w, l].Contains(TileType.WALL))
                {
                    if (w+2 < width - 1 && grid[w + 1, l].Count == 1 && grid[w + 1, l].Contains(TileType.WALL)
                        && grid[w + 2, l].Count == 1 && grid[w + 2, l].Contains(TileType.WALL) 
                        || l + 2 < length - 1 && grid[w, l + 1].Count == 1 && grid[w , l + 1].Contains(TileType.WALL)
                        && grid[w, l + 2].Count == 1 && grid[w, l + 2].Contains(TileType.WALL) )
                    {
                        return true;
                    }
                }
            }
        }
        //CHANGE TO FALSE
        return false;
    }

    // *** YOU NEED TO COMPLETE THIS FUNCTION  ***
    // must return true if there is no WALL adjacent to a virus 
    // adjacency means left, right, top, bottom, and *diagonal* blocks
    bool NoWallsCloseToVirus(List<TileType>[,] grid)
    {
        /*** implement the rest ! */
        for (int w = 1; w < width - 1; w++)
            for (int l = 1; l < length - 1; l++)
            {
                if (!grid[w, l].Contains(TileType.VIRUS))
                    continue;
                for (int i = -1; i < 2; i++)
                    for (int j = -1; j < 2; j++)
                    {
                        if (grid[w + i, l + j].Contains(TileType.WALL))
                            return false;
                    }

            }
        //CHANGE TO TRUE
        return true;
    }


    // check if attempted assignment is consistent with the constraints or not
    bool CheckConsistency(List<TileType>[,] grid, int[] cell_pos, TileType t)
    {
        int w = cell_pos[0];
        int l = cell_pos[1];

        List<TileType> old_assignment = new List<TileType>();
        old_assignment.AddRange(grid[w, l]);
        grid[w, l] = new List<TileType> { t };

		// note that we negate the functions here i.e., check if we are consistent with the constraints we want
        bool areWeConsistent = !DoWeHaveTooFewWallsORWaterORDrug(grid) && !DoWeHaveTooManyInteriorWallsORWaterORDrug(grid) 
                            && !TooLongWall(grid) && !NoWallsCloseToVirus(grid);

        grid[w, l] = new List<TileType>();
        grid[w, l].AddRange(old_assignment);
        return areWeConsistent;
    }


    // *** YOU NEED TO COMPLETE THIS FUNCTION  ***
    // implement backtracking 
    bool BackTrackingSearch(List<TileType>[,] grid, List<int[]> unassigned)
    {
        // if there are too many recursive function evaluations, then backtracking has become too slow (or constraints cannot be satisfied)
        // to provide a reasonable amount of time to start the level, we put a limit on the total number of recursive calls
        // if the number of calls exceed the limit, then it's better to try a different initialization
        if (function_calls++ > 10000)       
            return false;

        // we are done!
        if (unassigned.Count == 0)
            return true;

        /*** implement the rest ! */
        int index = Random.Range(0, unassigned.Count - 1);
        int[] pos = unassigned[index];

        foreach (TileType tile in grid[pos[0], pos[1]])
        {
            if (CheckConsistency(grid, pos, tile))
            {
                List<TileType> old_assignment = new List<TileType>();
                old_assignment.AddRange(grid[pos[0], pos[1]]);
                grid[pos[0], pos[1]] = new List<TileType> { tile };
                unassigned.RemoveAt(index);
                bool result = BackTrackingSearch(grid, unassigned);
                if (result) return true;
                unassigned.Insert(index, pos);
                grid[pos[0], pos[1]] = old_assignment;
            }
        }
        return false;
    }
    private int Manhattan(int[] player, int[] house)
    {
        return Mathf.Abs(player[0] - house[0]) + Mathf.Abs(player[1] - house[1]);
    }

    // g cost, h cost, Grid position, walls moved through (list of positions)
    struct Vertex
    {
        public int h_score;
        public float g_score;
        public int[] pos;
        public List<int[]> walls;

        public Vertex(int h_score, float g_score, int[] pos, List<int[]> walls)
        {
            this.h_score = h_score;
            this.g_score = g_score;
            this.pos = pos;
            this.walls = walls;
        }
    }

    // places the primitives/objects according to the grid assignents
    // you will need to edit this function (see below)
    // restart_var: wr, lr, wee, lee, max_dist
    void DrawDungeon(List<TileType>[,] solution, int[] restart_var)
    {
        GetComponent<Renderer>().material.color = Color.grey; // ground plane will be grey

        // place character at random position (wr, lr) in terms of grid coordinates (integers)
        // make sure that this random position is a FLOOR tile (not wall, drug, or virus)
        int wr = 0;
        int lr = 0;
        int wee = -1;
        int lee = -1;
        int max_dist = -1;
        if (restart_var.Length < 3)
        {
            while (true) // try until a valid position is sampled
            {
                wr = Random.Range(1, width - 1);
                lr = Random.Range(1, length - 1);

                if (solution[wr, lr][0] == TileType.FLOOR)
                {
                    float x = bounds.min[0] + (float)wr * (bounds.size[0] / (float)width);
                    float z = bounds.min[2] + (float)lr * (bounds.size[2] / (float)length);
                    fps_player_obj = Instantiate(fps_prefab);
                    fps_player_obj.name = "PLAYER";

                    // character is placed above the level so that in the beginning, he appears to fall down onto the maze
                    fps_player_obj.transform.position = new Vector3(x + 0.5f, 2.0f * storey_height, z + 0.5f);
                    break;
                }
            }

            // place an exit from the maze at location (wee, lee) in terms of grid coordinates (integers)
            // destroy the wall segment there - the grid will be used to place a house
            // the exist will be placed as far as away from the character (yet, with some randomness, so that it's not always located at the corners)
            while (true) // try until a valid position is sampled
            {
                if (wee != -1)
                    break;
                for (int we = 0; we < width; we++)
                {
                    for (int le = 0; le < length; le++)
                    {
                        // skip corners
                        if (we == 0 && le == 0)
                            continue;
                        if (we == 0 && le == length - 1)
                            continue;
                        if (we == width - 1 && le == 0)
                            continue;
                        if (we == width - 1 && le == length - 1)
                            continue;

                        if (we == 0 || le == 0 || wee == length - 1 || lee == length - 1)
                        {
                            // randomize selection
                            if (Random.Range(0.0f, 1.0f) < 0.1f)
                            {
                                int dist = System.Math.Abs(wr - we) + System.Math.Abs(lr - le);
                                if (dist > max_dist) // must be placed far away from the player
                                {
                                    wee = we;
                                    lee = le;
                                    max_dist = dist;
                                }
                            }
                        }
                    }
                }
            }


            // *** YOU NEED TO COMPLETE THIS PART OF THE FUNCTION  ***
            // implement an algorithm that checks whether
            // all paths between the player at (wr,lr) and the exit (wee, lee)
            // are blocked by walls. i.e., there's no way to get to the exit!
            // if this is the case, you must guarantee that there is at least 
            // one accessible path (any path) from the initial player position to the exit
            // by removing a few wall blocks (removing all of them is not acceptable!)
            // this is done as a post-processing step after the CSP solution.
            // It might be case that some constraints might be violated by this
            // post-processing step - this is OK.

            /*** implement what is described above ! */
            int WALL_COST = width * length;
            const int FLOOR_COST = 1;
            List<int[]> remove_walls = new List<int[]>();
            int[] house_pos = { wee, lee };
            int[] src_pos = { wr, lr };

            Vertex[,] nodes = new Vertex[width, length];
            for (int wi = 0; wi < width; wi++)
                for (int len = 0; len < length; len++)
                {
                    int[] position = { wi, len };
                    List<int[]> wall_list = new List<int[]>();
                    if (solution[wi, len][0] == TileType.WALL) wall_list.Add(position);
                    // g cost, h cost, Grid position, walls moved through (list of positions)
                    Vertex node = new Vertex(Manhattan(position, house_pos), Mathf.Infinity, position, wall_list);
                    nodes[wi, len] = node;
                }

            // h, g, pos, walls, node number
            List<int[]> temp = new List<int[]>();
            Vertex source = new Vertex(Manhattan(src_pos, house_pos), 0, src_pos, temp);
            List<Vertex> priority = new List<Vertex>();
            priority.Add(source);

            while (priority.Count != 0)
            {
                Vertex current = priority[0];
                int index = 0;
                for (int i = 0; i < priority.Count; i++)
                {
                    if (current.g_score + current.h_score > priority[i].g_score + priority[i].h_score)
                    {
                        index = i;
                        current = priority[i];
                    }
                }
                if (current.pos[0] == house_pos[0] && current.pos[1] == house_pos[1])
                {
                    remove_walls.AddRange(current.walls);
                    break;
                }

                priority.RemoveAt(index);

                for (int i = -1; i < 2; i++)
                {
                    int[] neighbor_pos = { current.pos[0] + i, current.pos[1] };
                    TileType neighbor_tile = solution[neighbor_pos[0], neighbor_pos[1]][0];
                    int path_cost = neighbor_tile == TileType.WALL ? WALL_COST : FLOOR_COST;
                    Vertex neighbor_node = nodes[neighbor_pos[0], neighbor_pos[1]];
                    List<int[]> temp_walls = new List<int[]>(current.walls);
                    temp_walls.AddRange(neighbor_node.walls);

                    int g_score = (int)current.g_score + path_cost;
                    if (neighbor_pos.SequenceEqual(house_pos) || (neighbor_pos[0] != 0 && neighbor_pos[0] != width - 1 && neighbor_pos[1] != 0 && neighbor_pos[1] != length - 1
                            && g_score < neighbor_node.g_score))
                    {

                        int dup_node = -1;
                        for (int k = 0; k < priority.Count; k++)
                        {
                            if (priority[k].pos.SequenceEqual(neighbor_pos))
                            {
                                dup_node = k;
                                break;
                            }
                        }
                        if (dup_node > -1)
                        {
                            priority.RemoveAt(dup_node);
                        }
                        Vertex new_vert = new Vertex(neighbor_node.h_score, g_score, neighbor_pos, temp_walls);
                        priority.Add(new_vert);
                        nodes[neighbor_pos[0], neighbor_pos[1]] = new_vert;
                    }
                }

                for (int j = -1; j < 2; j++)
                {
                    int[] neighbor_pos = { current.pos[0], current.pos[1] + j };
                    TileType neighbor_tile = solution[neighbor_pos[0], neighbor_pos[1]][0];
                    int path_cost = neighbor_tile == TileType.WALL ? WALL_COST : FLOOR_COST;
                    Vertex neighbor_node = nodes[neighbor_pos[0], neighbor_pos[1]];
                    List<int[]> temp_walls = new List<int[]>(current.walls);
                    temp_walls.AddRange(neighbor_node.walls);

                    int g_score = (int)current.g_score + path_cost;
                    if (neighbor_pos.SequenceEqual(house_pos) || (neighbor_pos[0] != 0 && neighbor_pos[0] != width - 1 && neighbor_pos[1] != 0 && neighbor_pos[1] != length - 1
                            && g_score < neighbor_node.g_score))
                    {

                        int dup_node = -1;
                        for (int k = 0; k < priority.Count; k++)
                        {
                            if (priority[k].pos.SequenceEqual(neighbor_pos))
                            {
                                dup_node = k;
                                break;
                            }
                        }
                        if (dup_node > -1)
                        {
                            priority.RemoveAt(dup_node);
                        }
                        Vertex new_vert = new Vertex(neighbor_node.h_score, g_score, neighbor_pos, temp_walls);
                        priority.Add(new_vert);
                        nodes[neighbor_pos[0], neighbor_pos[1]] = new_vert;
                    }
                }
            }

            foreach (int[] wall in remove_walls)
            {
                solution[wall[0], wall[1]] = new List<TileType>() { TileType.FLOOR };
            }

            old_grid = solution;
            restart_arr = new int[] {wr, lr, wee, lee};
        } else
        {
            wr = restart_var[0];
            lr = restart_var[1];
            wee = restart_var[2];
            lee = restart_var[3];
            float x = bounds.min[0] + (float)wr * (bounds.size[0] / (float)width);
            float z = bounds.min[2] + (float)lr * (bounds.size[2] / (float)length);
            fps_player_obj = Instantiate(fps_prefab);
            fps_player_obj.name = "PLAYER";

            // character is placed above the level so that in the beginning, he appears to fall down onto the maze
            fps_player_obj.transform.position = new Vector3(x + 0.5f, 2.0f * storey_height, z + 0.5f);
        }
        // the rest of the code creates the scenery based on the grid state 
        // you don't need to modify this code (unless you want to replace the virus
        // or other prefabs with something else you like)
        int w = 0;
        for (float x = bounds.min[0]; x < bounds.max[0]; x += bounds.size[0] / (float)width - 1e-6f, w++)
        {
            int l = 0;
            for (float z = bounds.min[2]; z < bounds.max[2]; z += bounds.size[2] / (float)length - 1e-6f, l++)
            {
                if ((w >= width) || (l >= width))
                    continue;

                float y = bounds.min[1];
                //Debug.Log(w + " " + l + " " + h);
                if ((w == wee) && (l == lee)) // this is the exit
                {
                    GameObject house = Instantiate(house_prefab, new Vector3(0, 0, 0), Quaternion.identity);
                    house.name = "HOUSE";
                    house.transform.position = new Vector3(x + 0.5f, y, z + 0.5f);
                    if (l == 0)
                        house.transform.Rotate(0.0f, 270.0f, 0.0f);
                    else if (w == 0)
                        house.transform.Rotate(0.0f, 0.0f, 0.0f);
                    else if (l == length - 1)
                        house.transform.Rotate(0.0f, 90.0f, 0.0f);
                    else if (w == width - 1)
                        house.transform.Rotate(0.0f, 180.0f, 0.0f);

                    house.AddComponent<BoxCollider>();
                    house.GetComponent<BoxCollider>().isTrigger = true;
                    house.GetComponent<BoxCollider>().size = new Vector3(3.0f, 3.0f, 3.0f);
                    house.AddComponent<House>();
                }
                else if (solution[w, l][0] == TileType.WALL)
                {
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.name = "WALL";
                    cube.transform.localScale = new Vector3(bounds.size[0] / (float)width, storey_height, bounds.size[2] / (float)length);
                    cube.transform.position = new Vector3(x + 0.5f, y + storey_height / 2.0f, z + 0.5f);
                    cube.GetComponent<Renderer>().material.color = new Color(0.6f, 0.8f, 0.8f);
                }
                else if (solution[w, l][0] == TileType.VIRUS)
                {
                    GameObject virus = Instantiate(virus_prefab, new Vector3(0, 0, 0), Quaternion.identity);
                    virus.name = "COVID";
                    virus.transform.position = new Vector3(x + 0.5f, y + Random.Range(1.0f, storey_height / 2.0f), z + 0.5f);

                    //GameObject virus = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    //virus.GetComponent<Renderer>().material.color = new Color(0.5f, 0.0f, 0.0f);
                    //virus.name = "ENEMY";
                    //virus.transform.position = new Vector3(x + 0.5f, y + Random.Range(1.0f, storey_height / 2.0f), z + 0.5f);
                    //virus.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
                    //virus.AddComponent<BoxCollider>();
                    //virus.GetComponent<BoxCollider>().size = new Vector3(1.2f, 1.2f, 1.2f);
                    //virus.AddComponent<Rigidbody>();
                    //virus.GetComponent<Rigidbody>().useGravity = false;

                    virus.AddComponent<Virus>();
                    virus.GetComponent<Rigidbody>().mass = 10000;
                }
                else if (solution[w, l][0] == TileType.DRUG)
                {
                    GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    capsule.name = "DRUG";
                    capsule.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
                    capsule.transform.position = new Vector3(x + 0.5f, y + Random.Range(1.0f, storey_height / 2.0f), z + 0.5f);
                    capsule.GetComponent<Renderer>().material.color = Color.magenta;
                    capsule.AddComponent<Problem>();
                }
                else if (solution[w, l][0] == TileType.WATER)
                {
                    GameObject water = Instantiate(water_prefab, new Vector3(0, 0, 0), Quaternion.identity);
                    water.name = "WATER";
                    water.transform.localScale = new Vector3(0.5f * bounds.size[0] / (float)width, 1.0f, 0.5f * bounds.size[2] / (float)length);
                    water.transform.position = new Vector3(x + 0.5f, y + 0.1f, z + 0.5f);

                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.name = "WATER_BOX";
                    cube.transform.localScale = new Vector3(bounds.size[0] / (float)width, storey_height / 20.0f, bounds.size[2] / (float)length);
                    cube.transform.position = new Vector3(x + 0.5f, y, z + 0.5f);
                    cube.GetComponent<Renderer>().material.color = Color.grey;
                    cube.GetComponent<BoxCollider>().size = new Vector3(1.1f, 20.0f * storey_height, 1.1f);
                    cube.GetComponent<BoxCollider>().isTrigger = true;
                    cube.AddComponent<Water>();
                }
            }
        }
    }

    

    // *** YOU NEED TO COMPLETE THIS PART OF THE FUNCTION JUST TO ADD SOUNDS ***
    // YOU MAY CHOOSE ANY SHORT SOUNDS (<2 sec) YOU WANT FOR A VIRUS HIT, A VIRUS INFECTION,
    // GETTING INTO THE WATER, AND REACHING THE EXIT
    // note: you may also change other scripts/functions to add sound functionality,
    // along with the functionality for the starting the level, or repeating it
    void Update()
    {
        if (player_health < 0.001f) // the player dies here
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            text_box.GetComponent<Text>().text = "Failed!";
            restart_level.gameObject.SetActive(true);
            restart_level.onClick.RemoveAllListeners();
            restart_level.onClick.AddListener(delegate { RestartLevel(old_grid, restart_arr); });

            if (fps_player_obj != null)
            {
                GameObject grave = GameObject.CreatePrimitive(PrimitiveType.Cube);
                grave.name = "GRAVE";
                grave.transform.localScale = new Vector3(bounds.size[0] / (float)width, 2.0f * storey_height, bounds.size[2] / (float)length);
                grave.transform.position = fps_player_obj.transform.position;
                grave.GetComponent<Renderer>().material.color = Color.black;
                Object.Destroy(fps_player_obj);                
            }
            main_cam.GetComponent<AudioListener>().enabled = true;

            return;
        }
        if (player_entered_house) // the player suceeds here, variable manipulated by House.cs
        {
            if (virus_landed_on_player_recently)
                text_box.GetComponent<Text>().text = "Washed it off at home! Success!!!";
            else
                text_box.GetComponent<Text>().text = "Success!!!";
            
            main_cam.GetComponent<AudioListener>().enabled = true;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Object.Destroy(fps_player_obj);
            new_level.gameObject.SetActive(true);

            return;
        }

        if (Time.time - timestamp_last_msg > 7.0f) // renew the msg by restating the initial goal
        {
            text_box.GetComponent<Text>().text = "Find your home!";            
        }

        // virus hits the players (boolean variable is manipulated by Virus.cs)
        if (virus_landed_on_player_recently)
        {
            float time_since_virus_landed = Time.time - timestamp_virus_landed;
            if (time_since_virus_landed > 5.0f)
            {
                player_health -= Random.Range(0.25f, 0.5f) * (float)num_virus_hit_concurrently;
                player_health = Mathf.Max(player_health, 0.0f);
                audio_source.PlayOneShot(health_loss, 1F);
                if (num_virus_hit_concurrently > 1)
                    text_box.GetComponent<Text>().text = "Ouch! Infected by " + num_virus_hit_concurrently + " viruses";
                else
                    text_box.GetComponent<Text>().text = "Ouch! Infected by a virus";
                timestamp_last_msg = Time.time;
                timestamp_virus_landed = float.MaxValue;
                virus_landed_on_player_recently = false;
                num_virus_hit_concurrently = 0;
            }
            else
            {
                if (num_virus_hit_concurrently == 1)
                    text_box.GetComponent<Text>().text = "A virus landed on you. Infection in " + (5.0f - time_since_virus_landed).ToString("0.0") + " seconds. Find water or drug!";
                else
                    text_box.GetComponent<Text>().text = num_virus_hit_concurrently + " viruses landed on you. Infection in " + (5.0f - time_since_virus_landed).ToString("0.0") + " seconds. Find water or drug!";
            }
        }

        // drug picked by the player  (boolean variable is manipulated by Drug.cs)
        if (drug_landed_on_player_recently)
        {
            if (player_health < 0.999f || virus_landed_on_player_recently)
                text_box.GetComponent<Text>().text = "Phew! New drug helped!";
            else
                text_box.GetComponent<Text>().text = "No drug was needed!";
            timestamp_last_msg = Time.time;
            player_health += Random.Range(0.25f, 0.75f);
            player_health = Mathf.Min(player_health, 1.0f);
            drug_landed_on_player_recently = false;
            timestamp_virus_landed = float.MaxValue;
            virus_landed_on_player_recently = false;
            num_virus_hit_concurrently = 0;
        }

        // splashed on water  (boolean variable is manipulated by Water.cs)
        if (player_is_on_water)
        {
            if (virus_landed_on_player_recently)
                text_box.GetComponent<Text>().text = "Phew! Washed it off!";
            timestamp_last_msg = Time.time;
            timestamp_virus_landed = float.MaxValue;
            virus_landed_on_player_recently = false;
            num_virus_hit_concurrently = 0;
        }

        // update scroll bar (not a very conventional manner to create a health bar, but whatever)
        scroll_bar.GetComponent<Scrollbar>().size = player_health;
        if (player_health < 0.5f)
        {
            ColorBlock cb = scroll_bar.GetComponent<Scrollbar>().colors;
            cb.disabledColor = new Color(1.0f, 0.0f, 0.0f);
            scroll_bar.GetComponent<Scrollbar>().colors = cb;
        }
        else
        {
            ColorBlock cb = scroll_bar.GetComponent<Scrollbar>().colors;
            cb.disabledColor = new Color(0.0f, 1.0f, 0.25f);
            scroll_bar.GetComponent<Scrollbar>().colors = cb;
        }

        /*** implement the rest ! */
    }
}

   


    