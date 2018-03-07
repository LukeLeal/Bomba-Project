using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour, IZOrder {

    int zOrder;
    bool isExploding = false;

    public int ZOrder {
        get { return zOrder; }
        set {
            GetComponent<Renderer>().sortingOrder = value;
            zOrder = value;
        }
    }

    // Use this for initialization
    void Start() {
        zOrder = GetComponent<Renderer>().sortingOrder;
    }

    // Update is called once per frame
    void Update() {

    }

    IEnumerator exploding() {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    // Ao ser atingido por uma explosão, items param de bloquear explosões e se tornam explosões.
    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.CompareTag("Explosion")) {
            if (!isExploding) {
                Destroy(collider.gameObject); // Tira a pseudo-explosão. Única função dela era fazer esse objeto explodir.
                gameObject.tag = "Explosion";
                isExploding = true;
                GetComponent<SpriteRenderer>().color = Color.red;
                StartCoroutine(exploding());
            }
        } else if (collider.CompareTag("Bomb")) {
            Destroy(gameObject);
        }
    }
}
