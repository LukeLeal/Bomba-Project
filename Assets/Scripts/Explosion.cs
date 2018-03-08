using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour, IZOrder {

    Boneco owner;
    int zOrder;
    bool pseudo; // pseudo-explosões são criadas em tiles já ocupadas por outro objeto. Única função é ativar um trigger (se houver) nele.
        // Ao triggerar o outro objeto, sua colisão deve deixar de existir.     
    // ATENÇÃO (07/03/18): Não estou satisfeito com a pseudo-explosão. Mudanças Soon tm. 
    //  Talvez interagindo diretamente com o objeto a ser explodido ao invés de depender de colliders seja melhor.

    public int ZOrder {
        get { return zOrder; }
        set {
            GetComponent<Renderer>().sortingOrder = value;
            zOrder = value;
        }
    }

    public bool Pseudo {
        get { return pseudo; }
    }

    // Use this for initialization
    void Start () {

    }
	
	// Update is called once per frame
	void Update () {

	}

    /// <summary>
    /// Faz ajustes e definições finais antes de ligar a explosão
    /// </summary>
    /// <param name="b"> Boneco que soltou a bomba que causou essa explosão. 
    ///                     - atm faz nada, mas depois poderá ser usado pra dar a kill pra ele. </param>
    /// <param name="center"> Centro das explosões causada pela bomba. Responsável pelo som. </param>
    /// <param name="pseudo"> Se será apenas uma pseudo-explosão. Ver definição lá em cima. </param>
    public void setup(Boneco b, bool center, bool pseudo) {
        owner = b;
        zOrder = GetComponent<Renderer>().sortingOrder;

        if (center) {
            AudioSource player = gameObject.AddComponent<AudioSource>();
            player.playOnAwake = false;
            AudioClip sfx = (AudioClip)Resources.Load("Sounds/SFX/Explosion v1");
            if(sfx != null) {
                player.clip = sfx;
                player.Play();
            }
        }

        if (pseudo) {
            this.pseudo = pseudo;
            GetComponent<SpriteRenderer>().enabled = false;
        }
        
        StartCoroutine(exploding());
    }

    IEnumerator exploding() {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject); // Detalhe: Isso corta o som também.
    }

}
