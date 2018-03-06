using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour, IZOrder {

    Boneco owner;
    int zOrder;
    bool pseudo; // pseudo-explosões são criadas em tiles já ocupadas por outro objeto. Única função é ativar um trigger (se houver) nele.
        // Ao triggerar o outro objeto, sua colisão deve deixar de existir.

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

    public void setup(Boneco b, bool playSFX, bool pseudo) {
        owner = b;
        zOrder = GetComponent<Renderer>().sortingOrder;

        // O som da explosão da bomba deve vir do centro do alcance dela.
        if (playSFX) {
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
