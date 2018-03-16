using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegularBlock : MonoBehaviour, IZOrder {

    bool isExploding = false;
    int zOrder;
    string itemName; // Nome do item (se houver) que aparecerá quando o bloco for explodido.

    public int ZOrder {
        get { return zOrder; }
        set {
            GetComponent<Renderer>().sortingOrder = value;
            zOrder = value;
        }
    }

    public string ItemName {
        get { return itemName; }
        set { itemName = value; }
    }

    // Use this for initialization
    void Awake () {
		zOrder = GetComponent<Renderer>().sortingOrder;
        ItemName = "";
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    IEnumerator exploding() {
        yield return new WaitForSeconds(Explosion.ExplosionTime);
        if(ItemName != "") {
            Item i = Instantiate(Resources.Load<Item>("Prefabs/" + ItemName), GridController.instance.centerPosition(transform.position), 
                Quaternion.identity);
            i.name = ItemName;
        }
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.CompareTag("Explosion")) {
            Destroy(collider.gameObject); // Tira a pseudo-explosão. Única função dela era fazer esse objeto explodir.
            if (!isExploding) {
                isExploding = true;
                GetComponent<SpriteRenderer>().color = Color.red;
                StartCoroutine(exploding());
            }
        }
    }
}
