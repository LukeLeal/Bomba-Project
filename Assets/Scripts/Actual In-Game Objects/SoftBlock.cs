using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Soft-Blocks são os blocos destrutíveis por explosões. Podem revelar items quando destruidos.
/// </summary>
public class SoftBlock : MonoBehaviour, IDestructible {

    bool isExploding = false;
    string itemName; // Nome do item (se houver) que aparecerá quando o bloco for explodido.

    public int Layer {
        get { return gameObject.layer; }
    }

    public string ItemName {
        get { return itemName; }
        set { itemName = value; }
    }

    // Use this for initialization
    void Awake () {
		GetComponent<Renderer>().sortingOrder = Layer;
        ItemName = "";
    }

    /// <summary>
    /// (IDestructible): Bloco tem sua destruição forçada por um agente externo.
    /// </summary>
    /// <param name="position"> Posição onde a destruição deve ocorrer. </param>
    public void forceDestruction(Vector2 position) {
        if (!isExploding) {
            isExploding = true;
            GetComponent<SpriteRenderer>().color = Color.red;
            StartCoroutine(exploding());
        }
    }

    /// <summary>
    /// Controla a duração da destruição do bloco. Quando termina, cria item no lugar (se houver).
    /// </summary>
    IEnumerator exploding() {
        yield return new WaitForSeconds(Explosion.ExplosionTime);
        if(ItemName != "") {
            Item i = Instantiate(Resources.Load<Item>("Prefabs/Items/" + ItemName), GridController.instance.centerPosition(transform.position), 
                Quaternion.identity);
            i.name = ItemName;
        }
        Destroy(gameObject);
    }

}
