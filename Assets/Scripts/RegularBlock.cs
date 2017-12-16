using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegularBlock : MonoBehaviour {

    bool isExploding = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    IEnumerator exploding() {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.GetComponent<Collider2D>().tag == "Explosion") {
            if (!isExploding) {
                isExploding = true;
                Destroy(collision.gameObject); // Tira a pseudo-explosão. Única função dela era fazer essa bomba explodir.
                GetComponent<SpriteRenderer>().color = Color.red;
                StartCoroutine(exploding());
            }
        }
    }
}
