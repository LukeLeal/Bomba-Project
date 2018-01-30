using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour, IZOrder {

    // Dados da bomba
    int power; // Tiles além do centro ocupado pela explosão (min 1)
    Boneco owner; // Boneco dono da bomba. 
    int zOrder;
    int state; // 1: Ticking; 2: Not ticking; 11: Explosion
    GridController gc;

    public const int Ticking = 1;
    public const int NotTicking = 2;
    public const int Exploding = 11;

    Coroutine tickCR;

    public int Power {
        get { return power; }

        set { power = value; }
    }

    public int ZOrder {
        get { return zOrder; }
        set {
            GetComponent<Renderer>().sortingOrder = value;
            zOrder = value;
        }
    }

    // Use this for initialization
    void Start () {
		
	}

    // Update is called once per frame
    void Update () {
		
	}

    /// <summary>
    /// Posiciona e liga a bomba
    /// </summary>
    /// <param name="b"> Boneco que criou a bomba. </param>
    public void setup(Boneco b) {
        gc = GridController.instance;
        owner = b;
        power = b.FirePower;
        transform.position = gc.centerPosition(b.transform.position);
        ZOrder = GridController.ZObjects;
        state = Ticking;
        GetComponent<AudioSource>().Play();
        tickCR = StartCoroutine(tick());
    }

    /// <summary>
    /// Tempo até explodir. Terá mudanças quando o estado NotTicking for implementado.
    /// </summary>
    IEnumerator tick() {
        // #sdds animação
        yield return new WaitForSeconds(2f);
        state = Exploding;
        GetComponent<Collider2D>().enabled = false; // Desativa o próprio collider pra não interferir nos cálculos
        explode();
    }

    /// <summary>
    /// Alguma explosão causou a explosão dessa bomba.
    /// </summary>
    public IEnumerator forceExplode() {
        if (state != Exploding) { // Pra garantir que não vai explodir múltiplas vezes por motivos diversos :P

            state = Exploding;
            if (tickCR != null) {
                StopCoroutine(tickCR);
            }
            yield return new WaitForSeconds(0.12f);
            GetComponent<Collider2D>().enabled = false; // Desativa o próprio collider pra não interferir nos cálculos
            explode();
        }
    }

    /// <summary>
    /// Cria os rastros da explosão nas direções possíveis e o seu centro.
    /// </summary>
    void explode() {
        // Cria as explosões pra cada lado
        createExplosion(Vector2.up);
        createExplosion(Vector2.right);
        createExplosion(Vector2.down);
        createExplosion(Vector2.left);

        // Cria o centro da explosão
        Explosion e = Instantiate(Resources.Load<Explosion>("Prefabs/Explosion"), transform.position, Quaternion.identity); 
        e.setup(owner, true);

        owner.BombsUsed--;    
        Destroy(gameObject); // rip bomb
    }

    // Cria os objetos das explosões em uma direção, se possível
    void createExplosion(Vector2 dir) {
        IZOrder zo;
        int range = calculateRange(dir, out zo);

        Vector2 curPos = (Vector2) transform.position + dir;
        while (range > 0) {
            if (range == 1) {
                
                //IZOrder zo = gc.tileMainContent(curPos);
                // Comofas pro item
                if(zo != null) {
                    // Cria uma pseudo-explosão no tile ocupado pelo outro objeto. Única função é ativar um trigger (se houver) no objeto.
                    Explosion spriteLess = Instantiate(Resources.Load<Explosion>("Prefabs/Explosion"), curPos, Quaternion.identity);
                    spriteLess.GetComponent<SpriteRenderer>().enabled = false;
                    spriteLess.setup(owner, false);
                    break;
                }
            } 
            Explosion e = Instantiate(Resources.Load<Explosion>("Prefabs/Explosion"), curPos, Quaternion.identity);
            e.setup(owner, false);
            curPos += dir;
            
            range--;
        }
    }

    /// <summary>
    /// Define o alcance da explosão na determinada direção
    /// Atenção (16/01/2018): Hit.point no curExpPos dá ruim. Deve ser por causa do tamanho / posição do collider da explosão.
    /// </summary>
    /// <param name="dir"> Direção (e.g. Vector2.up) </param>
    /// <returns> Alcance em tiles </returns>
    int calculateRange(Vector2 dir, out IZOrder zo) {
        List<RaycastHit2D> hits = new List<RaycastHit2D>(Physics2D.RaycastAll(transform.position, dir, Power));
        Vector2 lastExpPos = Vector2.positiveInfinity;

        foreach(RaycastHit2D hit in hits) {

            // Apenas atravessa explosões se não houver duas seguidas.
            if (hit.collider.CompareTag("Explosion")) {
                Vector2 curExpPos = gc.centerPosition(hit.collider.gameObject.transform.position);
                if (Vector2.Distance(curExpPos, lastExpPos) == 1) {
                    // Retorna distância entre a bomba e a tile antes das explosões
                    zo = null;
                    return (int)Mathf.Clamp(Vector2.Distance(gc.centerPosition(transform.position), curExpPos) - 2, 0, power);
                }
                lastExpPos = curExpPos;
                continue;
            }

            // Considera apenas os elementos que estão na camada de objetos.
            zo = hit.collider.gameObject.GetComponent<MonoBehaviour>() as IZOrder;
            if (zo != null && zo.ZOrder != GridController.ZObjects) {
                if (zo.gameObject.tag != "Item") {
                    continue;
                } 
            }

            // Retorna distância dos dois centros (própria bomba e objeto atingido). 
            return (int)Vector2.Distance(transform.position, gc.centerPosition(hit.point,dir)) ;
        }
        zo = null;
        return Power; // Se não tem nada no caminho, range máximo
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if(collision.GetComponent<Collider2D>().CompareTag("Explosion")) {
            Destroy(collision.gameObject); // Tira a pseudo-explosão. Única função dela era fazer essa bomba explodir.
            StartCoroutine(forceExplode()); 
        } 
    }

}
