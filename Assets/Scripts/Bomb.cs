using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour {

    int power; // Tiles além do centro ocupado pela explosão (min 1)
    bool pierce; // Explosão não é limitada por blocos destrutíveis.

    int state; // 1: Ticking; 2: Not ticking; 11: Explosion
    public const int Ticking = 1;
    public const int NotTicking = 2;
    public const int Explosion = 11; 

	// Use this for initialization
	void Start () {
		
	}

    // Update is called once per frame
    void Update () {
		
	}

    // Posiciona e Liga a bomba
    public void setup(Vector3 pos) {
        transform.position = pos;
        GetComponent<SpriteRenderer>().sortingOrder = 3;
        Debug.Log("Ligo!");
        state = Ticking;
        StartCoroutine(tick());
    }

    // Tempo até explodir
    IEnumerator tick() {
        yield return new WaitForSeconds(2f);
        explode();
    }

    void explode() {
        state = Explosion;
        Debug.Log("BOOM!");
        Destroy(gameObject);
    }
}
