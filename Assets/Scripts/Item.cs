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
    /// (IDestructible): Item tem sua destruição forçada por um agente externo.
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

    /// <summary>
    /// Controla a duração da explosão
    /// </summary>
    IEnumerator exploding() {
        yield return new WaitForSeconds(Explosion.ExplosionTime);
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.CompareTag("Bomb")) {
            Destroy(gameObject);
        }
    }
}
