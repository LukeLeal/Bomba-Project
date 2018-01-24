using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour, IZOrder {

    int zOrder;
    bool isExploding;
    //string name;

    public int ZOrder {
        get { return zOrder; }
        set {
            GetComponent<Renderer>().sortingOrder = value;
            zOrder = value;
        }
    }

    // Use this for initialization
    void Start () {
        zOrder = GetComponent<Renderer>().sortingOrder;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    /* RegularBlocks precisará ter um esquema que guarda o nome do item que terá dentro dele.
     * Ao explodir, o bloco instancia um prefab desse item no lugar que ele tava.
     * */

    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.GetComponent<Collider2D>().tag == "Explosion") {
            if (!isExploding) {
                isExploding = true;
                // Deleta o item e deixa a explosão ficar em cima. [/temp]
                // Faz a animação de item explodindo [final]
            }
        }
    }
}
