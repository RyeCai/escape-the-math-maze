using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;
public class MazeGenerator : MonoBehaviour
{
    public GameObject fps_prefab;
    public GameObject snowman_prefab;
    public GameObject door_prefab;
    public GameObject invis_prefab;
    public GameObject heal_prefab;
    public GameObject holder;
    public GameObject sleigh_prefab;
    public GameObject wall_prefab;
    private float wall_height;
    private float wall_length;
    private GameObject[] presents;
    private GameObject[] snowmen;
    private GameObject[] powerups;

    public int size;
    internal int remaining_problems;
    private bool[,] visited;
    private bool[,] v_wall;
    private bool[,] h_wall;
    private int[] start;
    private int exit_side;
    private int[] door_pos;
    private GameObject door_wall;
    private bool opened;
    Stack<int[]> stack = new Stack<int[]>();
    
    void FillMaze()
    {
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                v_wall[i, j] = true;
                h_wall[i, j] = true;
                visited[i, j] = false;
            }
        }
        
        for (int e = 0; e < size; e++)
        {
            v_wall[size, e] = true;
            h_wall[e, size] = true;
        }
    }
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

    bool ValidNeighbor(int[] neighbor)
    {
        int x = neighbor[0];
        int y = neighbor[1];
        return x >= 0 && x < size && y >= 0 && y < size && !visited[x, y];
    }

    void RemoveWall(int[] cur, int[] next)
    {
        int x_diff = next[0] - cur[0];
        int y_diff = next[1] - cur[1];

        if (x_diff == 1)
        {
            v_wall[next[0], next[1]] = false;
        } else if (x_diff == -1)
        {
            v_wall[cur[0], cur[1]] = false;
        } else if (y_diff == 1)
        {
            h_wall[next[0], next[1]] = false;
        } else if (y_diff == -1)
        {
            h_wall[cur[0], cur[1]] = false;
        }
    }

    void DepthFirst()
    {
        int[] cur_cell;
        stack.Push(start);
        while(stack.Count != 0)
        {
            cur_cell = stack.Pop();
            int x = cur_cell[0];
            int y = cur_cell[1];
            visited[x, y] = true;
            int[][] neighs = new int[][] { new int[]{ x + 1, y }, new int[] { x - 1, y }, new int[] { x, y + 1 }, new int[] { x, y - 1 } };
            List<int[]> neighbors = new List<int[]>();
            for (int i=0; i < 4; i++)
            {
                int[] neighbor = neighs[i];
                if (ValidNeighbor(neighbor))
                {
                    neighbors.Add(neighbor);
                }
            }
            if (neighbors.Count > 0)
            {

                stack.Push(cur_cell);
                Shuffle<int[]>(ref neighbors);
                int[] next = neighbors[0];
                stack.Push(next);
                RemoveWall(cur_cell, next);
            }
        }
    }

    void BuildMaze()
    {
        for (int i = 0; i<size+1; i++)
        {
            for (int j = 0; j < size+1; j++)
            {
                GameObject wall;
                if (h_wall[i, j])
                {
                    
                    if (i == door_pos[0] && j == door_pos[1])
                    {
                        wall = Instantiate(door_prefab, new Vector3(i * wall_length, wall_height / 2, j * wall_length - wall_length / 2), Quaternion.identity);
                        wall.name = "Door";
                        door_wall = wall;
                    } else
                    {
                        wall = Instantiate(wall_prefab, new Vector3(i * wall_length, wall_height / 2, j * wall_length - wall_length / 2), Quaternion.identity);
                        wall.name = string.Format("HWALL {0}, {1}", i, j);
                        wall.transform.SetParent(holder.transform);
                    }
                }
                if (v_wall[i,j])
                {
                    if (i == door_pos[0] && j == door_pos[1])
                    {
                        wall = Instantiate(door_prefab, new Vector3(i * wall_length - wall_length / 2, wall_height / 2, j * wall_length), Quaternion.identity);
                        wall.name = "Door";
                        door_wall = wall;
                    } else
                    {
                        wall = Instantiate(wall_prefab, new Vector3(i * wall_length - wall_length / 2, wall_height / 2, j * wall_length), Quaternion.identity);
                        wall.name = string.Format("VWALL {0}, {1}", i, j);
                        wall.transform.SetParent(holder.transform);
                    }
                    wall.transform.Rotate(0.0f, 90.0f, 0.0f);
                }
                
            }
        }
    }

    // Randomly chooses positions that are spaced out between each other and not on the player
    int[][] RandomPositions(int num, float min_dist)
    {
        int min_distance = 0;
        int[][] positions = new int[num][];
        while (min_distance < min_dist)
        {
            min_distance = int.MaxValue;
            for (int i = 0; i < num; i++)
            {
                int x = Random.Range(0, size - 1);
                int y = Random.Range(0, size - 1);
                while (Mathf.Abs(x-start[0]) + Mathf.Abs(y - start[1]) < min_dist/2)
                {
                    x = Random.Range(0, size - 1);
                    y = Random.Range(0, size - 1);
                }
                positions[i] = new int[] { x, y };
            }
            for (int i = 0; i < num; i++)
            {
                for (int j = i + 1; j < num; j++)
                {
                    int cur_dist = Mathf.Abs(positions[i][0] - positions[j][0]) + Mathf.Abs(positions[i][1] - positions[j][1]);
                    min_distance = Mathf.Min(min_distance, cur_dist);
                }
            }
        }
        return positions;
    }

    void SpawnPowerups()
    {
        int[][] positions = RandomPositions(6, size/4);

        for (int i=0; i<2; i++)
        {
            Instantiate(invis_prefab, new Vector3(positions[i][0], 2.2f, positions[i][1]), Quaternion.identity);
        }
        for (int i=2; i<6; i++)
        {
            Instantiate(heal_prefab, new Vector3(positions[i][0] * wall_length, 2.2f, positions[i][1] * wall_length), Quaternion.identity);
        }
    }

    void MovePlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("PLAYER");
        player.transform.position = new Vector3(start[0] * wall_length, wall_length, start[1] * wall_length); 
    }


    void MovePresents()
    {
        int[][] positions = RandomPositions(presents.Length, size/4);

        for (int i=0; i<presents.Length; i++)
        {
            GameObject present = presents[i];
            float height = present.transform.localScale[1];
            present.transform.position = new Vector3(positions[i][0] * wall_length, height / 2, positions[i][1] * wall_length);
        }
    }

    void MoveSnowmen()
    {
        int[][] positions = RandomPositions(snowmen.Length, size/2);

        for (int i = 0; i < snowmen.Length; i++)
        {
            GameObject snowman = snowmen[i];
            snowman.transform.position = new Vector3(positions[i][0] * wall_length, 3, positions[i][1] * wall_length);
        }
    }

    void MoveSleigh()
    {
        float sleigh_width = sleigh_prefab.transform.localScale.y;
        // 0 Top/North, 1 Right/East, 2 Bottom/South, 3 Left/West
        switch (exit_side)
        {
            case 0:
                door_pos = new int[] { Random.Range(0, size), size };
                sleigh_prefab.transform.position = new Vector3(door_pos[0] * wall_length , sleigh_prefab.transform.localScale[1] / 2, size * wall_length - wall_length / 2 + sleigh_width);
                //h_wall[door_pos[0], door_pos[1]] = false;
                break;
            case 1:
                door_pos = new int[] { size, Random.Range(0, size) };
                sleigh_prefab.transform.position = new Vector3(size * wall_length - wall_length / 2 + sleigh_width, sleigh_prefab.transform.localScale[1] / 2, door_pos[1] * wall_length );
                sleigh_prefab.transform.localRotation = Quaternion.Euler(-90.0f, 90.0f, 0.0f);
                //v_wall[door_pos[0], door_pos[1]] = false;
                break;
            case 2:
                door_pos = new int[] { Random.Range(0, size), 0 };
                sleigh_prefab.transform.position = new Vector3(door_pos[0] * wall_length, sleigh_prefab.transform.localScale[1] / 2, -sleigh_width - wall_length / 2);
                //h_wall[door_pos[0], door_pos[1]] = false;
                break;
            case 3:
                door_pos = new int[] { 0, Random.Range(0, size) };
                sleigh_prefab.transform.position = new Vector3(-sleigh_width/2 - wall_length / 2, sleigh_prefab.transform.localScale[1] / 2, door_pos[1] * wall_length);
                sleigh_prefab.transform.localRotation = Quaternion.Euler(-90.0f, -90.0f, 0.0f);
                //v_wall[door_pos[0], door_pos[1]] = false;
                break;
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {

        presents = GameObject.FindGameObjectsWithTag("probmaze");
        snowmen = GameObject.FindGameObjectsWithTag("Enemy");
        remaining_problems = presents.Length;
        wall_length = wall_prefab.transform.localScale[0];
        wall_height = wall_prefab.transform.localScale[1];
        visited = new bool[size, size];
        h_wall = new bool[size+1, size+1];
        v_wall = new bool[size+1, size+1];
        opened = false;
        FillMaze();
        start = new int[] { Random.Range(0, size-1), Random.Range(0, size-1) };
        exit_side = Random.Range(0, 3);       
        DepthFirst();
        MoveSleigh();
        MovePlayer();
        BuildMaze();
        MovePresents();
        SpawnPowerups();
        MoveSnowmen();
    }

    private void Update()
    {
        presents = GameObject.FindGameObjectsWithTag("probmaze");
        remaining_problems = presents.Length;
        if (remaining_problems == 0 && !opened)
        {
            Vector3 d_pos = door_wall.transform.position;
            if (d_pos[1] < -wall_height / 2) opened = true;
            door_wall.transform.position = new Vector3(d_pos[0], d_pos[1] - Time.deltaTime * 0.4f, d_pos[2]);
        } 
    }
}
