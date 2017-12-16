using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour {

    Boneco owner;

	// Use this for initialization
	void Start () {

    }
	
	// Update is called once per frame
	void Update () {

	}

    public void setup(Boneco b, bool playSFX) {
        owner = b;

        // O som da explosão deve vir do centro dela.
        if (playSFX) {
            AudioSource player = gameObject.AddComponent<AudioSource>();
            player.playOnAwake = false;
            AudioClip sfx = (AudioClip)Resources.Load("Sounds/SFX/Explosion v1");
            if(sfx != null) {
                player.clip = sfx;
                player.Play();
            }
        }
        
        StartCoroutine(exploding());
    }

    IEnumerator exploding() {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject); // Detalhe: Isso corta o som também.
    }

}
