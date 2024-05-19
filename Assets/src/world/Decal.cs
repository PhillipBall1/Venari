using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Decal : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(DeleteSelf());
    }

    private IEnumerator DeleteSelf()
    {
        yield return new WaitForSeconds(15);
        Destroy(gameObject);
    }
}
