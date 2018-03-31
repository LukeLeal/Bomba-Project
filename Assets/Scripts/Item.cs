using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour, IDestructible {

    bool isExploding = false;

    public int Layer {
        get { return gameObject.layer; }
    }

    // Use this for initialization
    void Start() {
        GetComponent<Renderer>().sortingOrder = Layer;
    }

    // Update is called once per frame
    void Update() {

    }

    // ATENÇÃO (28/03/2018): Item sendo explodido deverá instanciar uma Explosion em seu lugar, com sprite customizado. Soon tm
    /// <summary>
    /// Item tem sua destruição forçada por um agente externo.
    /// </summary>
    /// <param name="position"> Posição onde a destruição deve ocorrer. </param>
    public void forceDestruction(Vector2 position) {
        if (!isExploding) {
            gameObject.tag = "Explosion";
            isExploding = true;
            GetComponent<SpriteRenderer>().color = Color.red;
            StartCoroutine(exploding());
        }
    }

    IEnumerator exploding() {
        yield return new WaitForSeconds(Explosion.ExplosionTime);
        Destroy(gameObject);
    }

    // Ao ser atingido por uma explosão, items param de bloquear explosões e se tornam explosões.
    void OnTriggerEnter2D(Collider2D collider) {
        //if (collider.CompareTag("Explosion")) {
        //    if (!isExploding) {
        //        Destroy(collider.gameObject); // Tira a pseudo-explosão. Única função dela era fazer esse objeto explodir.
        //        gameObject.tag = "Explosion";
        //        isExploding = true;
        //        GetComponent<SpriteRenderer>().color = Color.red;
        //        StartCoroutine(exploding());
        //    }
        //} else 
        if (collider.CompareTag("Bomb")) {
            Destroy(gameObject);
        }
    }
}
