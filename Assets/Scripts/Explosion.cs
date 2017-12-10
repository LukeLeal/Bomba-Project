using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour {



	// Use this for initialization
	void Start () {
        StartCoroutine(exploding()); // beta
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    IEnumerator exploding() {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }
}
