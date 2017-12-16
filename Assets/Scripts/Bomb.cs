using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour {

    // Dados da bomba
    int power; // Tiles além do centro ocupado pela explosão (min 1)
    Boneco owner; // Boneco dono da bomba. 

    int state; // 1: Ticking; 2: Not ticking; 11: Explosion
    public const int Ticking = 1;
    public const int NotTicking = 2;
    public const int Exploding = 11;

    Coroutine tickCR;

    public int Power {
        get {
            return power;
        }

        set {
            power = value;
        }
    }

    // Use this for initialization
    void Start () {
		
	}

    // Update is called once per frame
    void Update () {
		
	}

    // Posiciona e Liga a bomba
    public void setup(Boneco b) {
        owner = b;
        power = b.FirePower;
        transform.position = GridController.instance.centerPosition(b.transform.position);
        GetComponent<SpriteRenderer>().sortingOrder = GridController.LObjects;
        state = Ticking;
        tickCR = StartCoroutine(tick());
    }

    // Tempo até explodir. Terá mudanças quando o estado NotTicking for implementado.
    IEnumerator tick() {
        // #sdds animação
        yield return new WaitForSeconds(2f);
        state = Exploding;
        GetComponent<Collider2D>().enabled = false; // Desativa o próprio collider pra não interferir nos cálculos
        explode();
    }

    // Alguma explosão causou a explosão dessa bomba.
    public IEnumerator forceExplode() {
        if (state != Exploding) { // Pra garantir que não vai explodir múltiplas vezes por motivos diversos :P

            state = Exploding;
            if (tickCR != null) {
                StopCoroutine(tickCR);
            }
            GetComponent<Collider2D>().enabled = false; // Desativa o próprio collider pra não interferir nos cálculos
            yield return new WaitForSeconds(0.12f);
            explode();
        }
    }

    // Cria a explosão central. Ela vai criando o resto.
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
        Destroy(gameObject); 
    }

    // Cria os objetos das explosões em uma direção, se possível
    void createExplosion(Vector2 dir) {
        int range = calculateRange(dir);

        Vector2 curPos = (Vector2) transform.position + dir;
        while (range > 0) {
            if (range == 1) {
                // Caso do último. Vê se tem objeto no local ou não. 
                GameObject go = GridController.instance.tileMainContent(curPos);
                if(go != null) {
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

    // Define o alcance da explosão
    int calculateRange(Vector2 dir) {
        List<RaycastHit2D> hits = new List<RaycastHit2D>(Physics2D.RaycastAll(transform.position, dir, Power));

        foreach(RaycastHit2D hit in hits) {
            // Considera apenas aqueles que estão na camada de objetos.
            if (hit.collider.gameObject.GetComponent<Renderer>().sortingOrder != GridController.LObjects) {
                continue; 
            }

            if (hit.collider.tag == "Explosion") {
                // ATENÇÃO (12/12/17): Deve ter um jeito melhor de pegar o centro do GO do hit, mas foi o que consegui. 
                // hit.point e hit.centroid tavam dando ruim nesse caso.

                Vector2 pos = GridController.instance.centerPosition(hit.collider.gameObject.transform.position);
                GameObject go = GridController.instance.tileMainContent((pos + dir));
                if (go != null) {
                    if(go.tag == "Explosion") {
                        // Rastro não atravessa duas explosões vizinhas.
                        return (int)Vector2.Distance(transform.position, GridController.instance.centerPosition(hit.point));
                    }
                }
                continue; // Apenas uma explosão. Rastro atravessa.
            }

            // Retorna distância dos dois centros (própria bomba e objeto atingido). 
            return (int)Vector2.Distance(transform.position, GridController.instance.centerPosition(hit.point)) ;
        }
        return Power; // Se não tem nada no caminho, range máximo
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if(collision.GetComponent<Collider2D>().tag == "Explosion") {
            Destroy(collision.gameObject); // Tira a pseudo-explosão. Única função dela era fazer essa bomba explodir.
            StartCoroutine(forceExplode()); 
        }
        
    }

}
