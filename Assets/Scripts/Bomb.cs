using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour, IZOrder {

    // Dados da bomba
    int power; // Tiles além do centro ocupado pela explosão (min 1)
    int zOrder;
    int state; // 1: Ticking; 2: Not ticking; 11: Explosion
    Boneco owner; // Boneco dono da bomba. 
    GridController gc;

    public const int Ticking = 1;
    public const int NotTicking = 2;
    public const int Exploding = 11;

    Coroutine tickCR; // Corotina que controla o tempo até a explosão
    Coroutine slideCR; // Corotina que controla o movimento terrestre

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

        // Teste de chute de bomba
        if (Input.GetKeyDown(KeyCode.K)) {
            wasKicked(Vector2.left);
        }

        if (Input.GetKeyDown(KeyCode.U)) {
            wasKicked(Vector2.up);
        }
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
    /// Tempo até explodir. Terá mudanças quando o estado NotTicking for implementado (provavelmente usando deltaTime).
    /// </summary>
    IEnumerator tick() {
        // #sdds animação
        yield return new WaitForSeconds(2f);
        state = Exploding;
        if (slideCR != null) {
            StopCoroutine(slideCR);
        }
        explode();
    }

    /// <summary>
    /// Bomba para tudo e tem sua explosão forçada por um agente externo
    /// 
    /// ATENÇÃO (1º/03/18): Parar a coroutine do slideCR não para imediatamente o translate já feito nela.
    ///     Por causa disso, tive que meter dois centerPos (antes e depois do wait) pra garantir que a bomba 
    ///     explodirá no local certo e sem movimentos extremamente bruscos. Deve haver uma solução melhor, tho...
    /// </summary>
    /// <param name="collision"> Colisão que causou a explosão dessa bomba. </param>
    public IEnumerator forceExplode(GameObject triggerExplosion) {
        if (state != Exploding) { // Pra garantir que não vai explodir múltiplas vezes por motivos diversos :P

            state = Exploding;
            if (tickCR != null) {
                StopCoroutine(tickCR);
            }
            if (slideCR != null) {
                StopCoroutine(slideCR);
            }
            transform.position = gc.centerPosition(triggerExplosion.transform.position);
            
            yield return new WaitForSeconds(0.12f);
            transform.position = gc.centerPosition(triggerExplosion.transform.position);
            explode();
            Destroy(triggerExplosion);
        }
    }

    /// <summary>
    /// Cria os rastros da explosão nas direções possíveis e o seu centro.
    /// </summary>
    void explode() {
        // Preparação pré-explosão
        GetComponent<Collider2D>().enabled = false; // Desativa o próprio collider pra não interferir nos cálculos
        transform.position = gc.centerPosition(transform.position);

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
    /// BETA - BoxCast
    /// Define o alcance da explosão na determinada direção
    /// Atenção (16/01/2018): Hit.point no curExpPos dá ruim. Deve ser por causa do tamanho / posição do collider da explosão.
    /// </summary>
    /// <param name="dir"> Direção (e.g. Vector2.up) </param>
    /// <returns> Alcance em tiles </returns>
    int calculateRange(Vector2 dir, out IZOrder zo) {
        List<RaycastHit2D> hits = new List<RaycastHit2D>(Physics2D.BoxCastAll(transform.position, new Vector2(0.9f, 0.9f), 0f, dir, power));
        
        Vector2 lastExplosionPos = Vector2.positiveInfinity;

        foreach (RaycastHit2D hit in hits) {

            // Apenas atravessa explosões se não houver duas seguidas.
            if (hit.collider.CompareTag("Explosion")) {
                Vector2 curExplosionPos = gc.centerPosition(hit.collider.gameObject.transform.position);
                if (Vector2.Distance(curExplosionPos, lastExplosionPos) == 1) {
                    // Retorna distância entre a bomba e a tile antes das explosões
                    zo = null;
                    return (int)Mathf.Clamp(Vector2.Distance(gc.centerPosition(transform.position), curExplosionPos) - 2, 0, power);
                }
                lastExplosionPos = curExplosionPos;
                continue;
            }

            // Elementos que estão na camada de objetos e items finalizam o alcance da explosão.
            zo = hit.collider.gameObject.GetComponent<MonoBehaviour>() as IZOrder;
            if (zo != null && zo.ZOrder != GridController.ZObjects) {
                if (zo.gameObject.tag != "Item") {
                    continue;
                }
            }

            // Retorna distância dos dois centros de tiles (própria bomba e colisão). 
            return (int)Vector2.Distance(transform.position, gc.centerPosition(hit.point, dir));
        }
        zo = null;
        return Power; // Se não tem nada no caminho, range máximo
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if(collision.GetComponent<Collider2D>().CompareTag("Explosion")) {
            StartCoroutine(forceExplode(collision.gameObject));
            //Destroy(collision.gameObject); 
        } 
    }

    #region Beta Kick Stuff

    /// <summary>
    /// Inicia o processo de movimento da bomba devido a chute. Chamado pelo Boneco
    /// </summary>
    /// <param name="dir"> Direção (e.g. Vector2.up) </param>
    public void wasKicked(Vector2 dir) {
        if(canKick(dir)) {

            slideCR = StartCoroutine(Slide(dir));
        }
    }

    /// <summary>
    /// Determina se um boneco pode ou não chutar essa bomba.
    /// if sujeito a mudanças.
    /// </summary>
    public bool canKick(Vector2 dir) {
        if (state != Exploding && ZOrder == GridController.ZObjects && slideCR == null && possibleSlide(dir)) {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Computa o movimento terrestre da bomba.
    /// </summary>
    /// <returns></returns>
    IEnumerator Slide(Vector2 dir) {
        transform.position = gc.centerPosition(transform.position); // Just in case...

        while (possibleSlide(dir)) {
            float moveConst = Time.deltaTime * 7; // BETA. 
            transform.Translate(dir * moveConst);

            //yield return new WaitForSeconds(0.12f);
            yield return null;
        }
        transform.position = gc.centerPosition(transform.position);
        slideCR = null;
    }

    /// <summary>
    /// BETA
    /// Verifica se o slide (movimento por chute) é possível até a tile à frente
    /// </summary>
    /// <param name="dir"> Direção (e.g. Vector2.up) </param>
    /// <returns> Movimento possível ou não </returns>
    bool possibleSlide(Vector2 dir) {

        List <IZOrder> zos = gc.tileContentOnZOrders((Vector2)transform.position + dir, new int[] { 1, 2 });
        if (zos.Count > 0) {

            // Só continua o movimento se estiver antes do centro da tile atual
            if (dir == Vector2.right || dir == Vector2.left) {
                if (dir.x * (transform.position.x - gc.centerPosition(transform.position).x) < 0) {
                    return true;
                } else {
                    return false; 
                }

            } else if (dir == Vector2.up || dir == Vector2.down) {
                if (dir.y * (transform.position.y - gc.centerPosition(transform.position).y) < 0) {
                    return true;
                } else {
                    return false;
                }

            } else {
                Debug.Log("PutaVida.exception: Impossible direction");
            }

        }
        return true; // Próxima tile sem obstáculos
    }

    #endregion
}
