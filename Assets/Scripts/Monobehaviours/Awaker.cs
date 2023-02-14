using UnityEngine;

public class Awaker : MonoBehaviour
{
    public GameObject WorldToSpawn;
    void Awake()
    {
        Instantiate(WorldToSpawn, transform.position, Quaternion.identity, transform);
    }
}
