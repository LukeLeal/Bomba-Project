﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour {

    // Dados da bomba
    int power; // Tiles além do centro ocupado pela explosão (min 1)

    int state; // 1: Ticking; 2: Not ticking; 11: Explosion
    Boneco owner; // Boneco dono da bomba. 
    GridController gc;
    string sfxPath = "Sounds/SFX/Bomb/"; // Caminho pros sound effects.
    string explosionPath = "Prefabs/Explosions/Explosion"; // Caminho pros prefabs das explosões

    public const int Ticking = 1;
    public const int NotTicking = 2;
    public const int Exploding = 11;

    Coroutine tickCR; // Corotina que controla o tempo até a explosão
    Coroutine slideCR; // Corotina que controla o movimento terrestre

    public int Power {
        get { return power; }

        set { power = value; }
    }

    public int Layer {
        get { return gameObject.layer; }
        set {
            GetComponent<Renderer>().sortingOrder = value;
            gameObject.layer = value;
        }
    }

    // Use this for initialization
    void Start () {
		
	}

    // Update is called once per frame
    void Update () {

        // Testes de chute de bomba
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
        GetComponent<Renderer>().sortingOrder = Layer;
        state = Ticking;
        GetComponent<AudioSource>().clip = (AudioClip) Resources.Load(sfxPath + "PlaceBomb");
        GetComponent<AudioSource>().Play();
        tickCR = StartCoroutine(tick());
    }

    #region Ticking & pre-explosion
    /// <summary>
    /// Tempo até explodir. Terá mudanças quando o estado NotTicking for implementado (provavelmente usando deltaTime).
    /// </summary>
    IEnumerator tick() {
        // #sdds animação
        yield return new WaitForSeconds(2.5f);
        state = Exploding;
        if (slideCR != null) {
            StopCoroutine(slideCR);
        }
        explode();
    }

    /// <summary>
    /// Bomba para tudo e tem sua explosão forçada por um agente externo
    /// 
    /// triggerPos existe pq num caso muito específico o trigger já tava destruido ao regular a posição, dando ruim.
    /// 
    /// ATENÇÃO (1º/03/18): Parar a coroutine do slideCR não para imediatamente o translate já feito nela.
    ///     Por causa disso, tive que meter dois centerPos (antes e depois do wait) pra garantir que a bomba 
    ///     explodirá no local certo e sem movimentos extremamente bruscos. Deve haver uma solução melhor, tho...
    /// </summary>
    /// <param name="trigger"> Objeto cujo a colisão causou a explosão dessa bomba. </param>
    public IEnumerator forceExplode(GameObject trigger) {
        if (state != Exploding) { 
            Vector2 triggerPos = gc.centerPosition(trigger.transform.position);
            state = Exploding;
            if (tickCR != null) {
                StopCoroutine(tickCR);
            }
            if (slideCR != null) {
                StopCoroutine(slideCR);
            }
            transform.position = triggerPos;
            
            yield return new WaitForSeconds(0.075f); // No jogo original (SB5) é algo entre 0.07 e 0.08 segs
            transform.position = triggerPos;
            explode();
            Destroy(trigger);
        }
    }
#endregion

    #region Sequência de criação da explosão
    /// <summary>
    /// Cria os rastros da explosão nas direções possíveis e o seu centro.
    /// </summary>
    void explode() {
        // Preparação pré-explosão
        GetComponent<Collider2D>().enabled = false; // Desativa o próprio collider pra não interferir nos cálculos
        transform.position = gc.centerPosition(transform.position);

        // Cria as explosões pra cada lado
        createExplosions(Vector2.up);
        createExplosions(Vector2.right);
        createExplosions(Vector2.down);
        createExplosions(Vector2.left);

        // Cria o centro da explosão
        Explosion center = Instantiate(Resources.Load<Explosion>(explosionPath + "Center"), transform.position, Quaternion.identity);
        center.setup(owner, true, false);

        owner.BombsUsed--;
        Destroy(gameObject);
    }

    /// <summary>
    /// Se possível, cria os objetos das explosões em uma direção
    /// ATENÇÃO (07/03/18): Não estou satisfeito com a pseudo-explosão. Mudanças Soon tm. 
    ///     Talvez interagindo diretamente com o objeto a ser explodido ao invés de depender de colliders seja melhor.
    /// </summary>
    /// <param name="dir"> Direção do rastro da explosão a ser criado (e.g. Vector2.up) </param>
    void createExplosions(Vector2 dir) {
        GameObject go;
        string direction;
        if (dir == Vector2.up) {
            direction = "Up";
        } else if (dir == Vector2.right) {
            direction = "Right";
        } else if (dir == Vector2.down) {
            direction = "Down";
        } else {
            direction = "Left";
        }

        int range = calculateRange(dir, out go);

        Vector2 curPos = (Vector2)transform.position + dir;
        while (range > 0) {
            if (range == 1) {

                if (go != null) {
                    // Cria pseudo explosão no tile já ocupado por outro objeto
                    Explosion pseudoExplosion = Instantiate(Resources.Load<Explosion>(explosionPath + direction), curPos, Quaternion.identity);
                    pseudoExplosion.setup(owner, false, true);
                    break;
                } else {
                    Explosion end = Instantiate(Resources.Load<Explosion>(explosionPath + direction + "End"), curPos, Quaternion.identity);
                    end.setup(owner, false, false);
                    break;
                }
            }
            Explosion e = Instantiate(Resources.Load<Explosion>(explosionPath + direction), curPos, Quaternion.identity);
            e.setup(owner, false, false);
            curPos += dir;

            range--;
        }
    }

    /// <summary>
    /// Define o alcance da explosão na determinada direção
    /// </summary>
    /// <param name="dir"> Direção a ser calculada (e.g. Vector2.up) </param>
    /// <param name="go"> Objeto (exceto Explosion) que determina o fim do alcance </param>
    /// <returns> Alcance em tiles </returns>
    int calculateRange(Vector2 dir, out GameObject go) {
        
        RaycastHit2D[] hits = 
            Physics2D.BoxCastAll(transform.position, new Vector2(0.9f, 0.9f), 0f, dir, power, LayerMask.GetMask("Surface", "Objects"));

        Vector2 lastExplosionPos = Vector2.positiveInfinity;

        foreach (RaycastHit2D hit in hits) {
            // Apenas atravessa explosões se não houver duas seguidas.
            if (hit.collider.CompareTag("Explosion")) {
                Vector2 curExplosionPos = gc.centerPosition(hit.collider.gameObject.transform.position);
                if (Vector2.Distance(curExplosionPos, lastExplosionPos) == 1) {
                    // Retorna distância entre a bomba e a tile antes das explosões
                    go = null;
                    return (int)Mathf.Clamp(Vector2.Distance(gc.centerPosition(transform.position), curExplosionPos) - 2, 0, power);
                }
                lastExplosionPos = curExplosionPos;
                continue;
            }

            go = hit.collider.gameObject;
            // Retorna distância dos dois centros de tiles (própria bomba e colisão). 
            return (int)Vector2.Distance(transform.position, gc.centerPosition(hit.point, dir));
        }
        go = null;
        return Power; // Se não tem nada no caminho, range máximo
    }

    #endregion

    #region Kick Stuff
    /// <summary>
    /// Inicia o processo de movimento da bomba (se possível) devido a chute do Boneco
    /// </summary>
    /// <param name="dir"> Direção (e.g. Vector2.up) </param>
    public void wasKicked(Vector2 dir) {
        if (state != Exploding && gameObject.layer == GridController.Objects && slideCR == null && possibleSlide(dir)) {
            GetComponent<AudioSource>().clip = (AudioClip)Resources.Load(sfxPath + "KickBomb");
            GetComponent<AudioSource>().Play();
            slideCR = StartCoroutine(Slide(dir));
        }
    }

    /// <summary>
    /// Computa o movimento terrestre da bomba.
    /// </summary>
    IEnumerator Slide(Vector2 dir) {
        transform.position = gc.centerPosition(transform.position); // Just in case...

        while (possibleSlide(dir)) {
            float moveConst = Time.deltaTime * 7; // BETA. 
            transform.Translate(dir * moveConst);
            yield return null;
        }
        transform.position = gc.centerPosition(transform.position);
        slideCR = null;
    }

    /// <summary>
    /// Verifica se o slide (movimento por chute) é possível até a tile à frente
    /// </summary>
    /// <param name="dir"> Direção (e.g. Vector2.up) </param>
    /// <returns> Movimento possível ou não </returns>
    bool possibleSlide(Vector2 dir) {

        List<GameObject> contents = gc.tileContent((Vector2)transform.position + dir, GridController.Objects, GridController.Bonecos);

        if (contents.Count > 0) {

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

    void OnTriggerEnter2D(Collider2D collider) {
        if(collider.CompareTag("Explosion")) {
            if(collider.gameObject.GetComponent<Explosion>() != null && collider.gameObject.GetComponent<Explosion>().Pseudo) {
                collider.enabled = false;
            }
            StartCoroutine(forceExplode(collider.gameObject));
        } 
    }

}
