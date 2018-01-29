using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour, IZOrder {

    int zOrder;
    bool isExploding = false;
    //string name;

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

    /* RegularBlocks precisará ter um esquema que guarda o nome do item que terá dentro dele.
     * Ao explodir, o bloco instancia um prefab desse item no lugar que ele tava.
     * */

    IEnumerator exploding() {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Explosion")) {
            Destroy(collision.gameObject); // Tira a pseudo-explosão. Única função dela era fazer esse objeto explodir.
            if (!isExploding) {
                Debug.Log("Item expl");
                gameObject.tag = "Explosion";
                isExploding = true;
                GetComponent<SpriteRenderer>().color = Color.red;
                StartCoroutine(exploding());
            }
        }
    }
}
