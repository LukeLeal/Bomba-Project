using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour {

    public const float ExplosionTime = 0.5f;
    Boneco owner;
    bool center;

    public int Layer {
        get { return gameObject.layer; }
    }

    /// <summary>
    /// Faz ajustes e definições finais antes de ligar a explosão
    /// </summary>
    /// <param name="b"> Boneco que soltou a bomba que causou essa explosão. </param>
    ///                     - atm faz nada, mas depois poderá ser usado pra dar a kill pra ele. 
    /// <param name="center"> Centro das explosões causada pela bomba. Responsável pelo som. </param>
    public void setup(Boneco b, bool center) {
        owner = b;
        GetComponent<Renderer>().sortingOrder = Layer;
        this.center = center;
        if (center) {
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

    /// <summary>
    /// Controla a duração da explosão
    /// </summary>
    /// Dá pra ter um nome melhor. Talvez trocar esse ou os outros métodos na Bomb, Soft-Block e tals. 
    IEnumerator exploding() {
        yield return new WaitForSeconds(ExplosionTime);

        if (center && GetComponent<AudioSource>() != null) {
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<Collider2D>().enabled = false;
            while (GetComponent<AudioSource>().isPlaying) {
                yield return null;
            }
        } 

        Destroy(gameObject); 
    }

}
