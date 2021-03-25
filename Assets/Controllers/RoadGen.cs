using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadGen : MonoBehaviour
{
    public GameObject[] PrefabsRoad; // Hold various type of roads
    public GameObject prevObj;
    public List<GameObject> planes;
    public List<GameObject> removed;
    public Vector3 prevPos;
    public static int step = 25;
    public int[,] dirs = new int[,] { { step, 0 }, { -step, 0 }, { 0, step }, { 0, -step } };
    public string[] names = { "plus_x", "minus_x", "plus_z", "minus_z" };
    public HashSet<string> seen;
    public float time;
    public int y = 15;
    void Start()
    {
        time = 0f;
        seen = new HashSet<string>();
        planes = new List<GameObject>();
        prevPos = new Vector3(0, y, 0);
        prevObj = Instantiate(
            PrefabsRoad[Random.Range(0, PrefabsRoad.Length)],
            prevPos,
            Quaternion.identity
        );
        seen.Add(0 + "," + 0);
        planes.Add(prevObj);
        removed = new List<GameObject>();
    }
    void TurnOff(GameObject gameObj, string name)
    {
        for (int i = 1; i <= 4; i++)
        {
            GameObject cur_wall = gameObj.transform.GetChild(i).gameObject;
            if (cur_wall.name == name)
            {
                cur_wall.SetActive(false);
            }
        }
    }
    string Inverse(string name)
    {
        if (name == "plus_x")
        {
            return "minus_x";
        }
        else if (name == "minus_x")
        {
            return "plus_x";
        }
        else if (name == "plus_z")
        {
            return "minus_z";
        }
        else // name == "minus_z"
        {
            return "plus_z";
        }
    }
    void DestroyMaze()
    {
        for (int i = 0; i < planes.Count; i++)
        {
            Destroy(planes[i]);
        }
        for (int i = 0; i < removed.Count; i++)
        {
            Destroy(removed[i]);
        }

        seen = new HashSet<string>();
        planes = new List<GameObject>();
        removed = new List<GameObject>();
        prevPos = new Vector3(0, y, 0);
        prevObj = Instantiate(
            PrefabsRoad[Random.Range(0, PrefabsRoad.Length)],
            prevPos,
            Quaternion.identity
        );
        seen.Add(0 + "," + 0);
        planes.Add(prevObj);
    }
    int CountExistingNei(float x, float z)
    {
        int ans = 0;
        for (int d = 0; d < 4; d++)
        {
            var dx = x + dirs[d, 0];
            var dz = z + dirs[d, 1];
            if (seen.Contains(dx + "," + dz)) ans++;
        }
        return ans;
    }


    void Update()
    {
        //time += Time.deltaTime;
        if (planes.Count >= 100)
        {
            //Debug.Log(time);
            //if (time > 20)
            //{
            //    DestroyMaze();
            //    time = 0;
            //}
            return;
        }
        prevPos = planes[planes.Count - 1].transform.position;
        if (CountExistingNei(prevPos.x, prevPos.z) == 4)
        {
            //Destroy(planes[planes.Count - 1]);
            removed.Add(planes[planes.Count - 1]);
            planes.RemoveAt(planes.Count - 1);
            //seen.Remove(prevPos.x + "," + prevPos.z);
            return;
        }
            
        int d = Random.RandomRange(0, 4);
        var dx = prevPos.x + dirs[d, 0];
        var dz = prevPos.z + dirs[d, 1];
        if (seen.Contains(dx + "," + dz)) return;
        Vector3 currentPos = new Vector3(dx, prevPos.y, dz);
        seen.Add(dx + "," + dz);
        GameObject obj = Instantiate(
            PrefabsRoad[Random.Range(0, PrefabsRoad.Length)],
            currentPos,
            Quaternion.identity
        );
        TurnOff(prevObj, names[d]);
        TurnOff(obj, Inverse(names[d]));
        prevPos = currentPos;
        prevObj = obj;
        planes.Add(obj);
    }

    void OnDestroy()
    {
        Debug.Log("Destroying the old maze");
        for (int i = 0; i < planes.Count; i++)
        {
            Destroy(planes[i]);
        }
        for (int i = 0; i < removed.Count; i++)
        {
            Destroy(removed[i]);
        }
    }
}
