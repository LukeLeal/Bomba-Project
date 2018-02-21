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
            wasKicked(Vector2.right);
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
    /// Alguma explosão causou a explosão dessa bomba.
    /// </summary>
    /// <param name="forcePosition"> Posição em que a bomba deverá permanecer para explodir. </param>
    public IEnumerator forceExplode(Vector2 forcePosition) {
        if (state != Exploding) { // Pra garantir que não vai explodir múltiplas vezes por motivos diversos :P

            state = Exploding;
            if (tickCR != null) {
                StopCoroutine(tickCR);
            }
            if (slideCR != null) {
                StopCoroutine(slideCR);
            }
            transform.position = forcePosition;

            yield return new WaitForSeconds(0.12f);
            explode();
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

            // Elementos que estão na camada de objetos e items finalizam o alcance da explosão.
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
            StartCoroutine(forceExplode(gc.centerPosition(collision.gameObject.transform.position)));
            Destroy(collision.gameObject); // TEM QUE VER ISSAQUI
        } 
    }

    #region Beta Kick Stuff

    /// <summary>
    /// Inicia o processo de movimento da bomba devido a chute. Chamado pelo Boneco
    /// </summary>
    /// <param name="dir"> Direção (e.g. Vector2.up) </param>
    public void wasKicked(Vector2 dir) {
        if(slideCR != null) {
            StopCoroutine(slideCR);
        }
        slideCR = StartCoroutine(Slide(Vector2.right));
    }

    /// <summary>
    /// Computa o movimento terrestre da bomba.
    /// </summary>
    /// <returns></returns>
    IEnumerator Slide(Vector2 dir) {
        bool obstacle;
        while (possibleMove(dir, out obstacle)) {
            bombTranslate(dir, obstacle);
            //yield return new WaitForSeconds(0.12f);
            yield return null;
        }
        
    }

    /// <summary>
    /// BETA
    /// </summary>
    /// <param name="dir"> Direção (e.g. Vector2.up) </param>
    /// <param name="obstacle"> Se true, indica movimento limitado. False whatever </param>
    /// <returns> Movimento possível ou não </returns>
    bool possibleMove(Vector2 dir, out bool obstacle) {
        obstacle = false;

        IZOrder zo = gc.tileMainContent((Vector2)transform.position + dir);
        if (zo != null) {

            if (zo.ZOrder == GridController.ZObjects) {

                // Se já tiver o mais próximo possível do obstáculo, movimento impossível
                if (dir == Vector2.right || dir == Vector2.left) {
                    if (transform.position.x - gc.centerPosition(transform.position).x == 0) {
                        return false;
                    }
                } else if (dir == Vector2.up || dir == Vector2.down) {
                    if (transform.position.y - gc.centerPosition(transform.position).y == 0) {
                        return false;
                    }
                } else {
                    Debug.Log("PutaVida.exception: Impossible direction");
                }
                obstacle = true;
            }
        }
        return true;
    }

    void bombTranslate(Vector2 dir, bool obstacle) {
        float moveConst = Time.deltaTime * 7; // BETA. 

        if (obstacle) {
            if (dir == Vector2.up || dir == Vector2.down) {
                if (Mathf.Abs(transform.position.y - gc.centerPosition(transform.position).y) <= 0.1) {
                    transform.position = gc.centerPosition(transform.position);
                    return;
                }
            } else {
                if (Mathf.Abs(transform.position.x - gc.centerPosition(transform.position).x) <= 0.1) {
                    transform.position = gc.centerPosition(transform.position);
                    return;
                }
            }
        }

        transform.Translate(dir * moveConst);

        //if (dir == Vector2.up || dir == Vector2.down) {

        //} else {

        //}
    }

    void newbombTranslate(Vector2 dir) {
        float moveConst = Time.deltaTime * 7; // BETA. 

        if (true) {
            if (dir == Vector2.up || dir == Vector2.down) {
                if (Mathf.Abs(transform.position.y - gc.centerPosition(transform.position).y) <= 0.1) {
                    transform.position = gc.centerPosition(transform.position);
                    return;
                }
            } else {
                if (Mathf.Abs(transform.position.x - gc.centerPosition(transform.position).x) <= 0.1) {
                    transform.position = gc.centerPosition(transform.position);
                    return;
                }
            }
        }

        transform.Translate(dir * moveConst);
    }



    #endregion
}
