using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  Classe responsável pelos métodos e dados do boneco
/// </summary>
public class Boneco : MonoBehaviour {

    GridController gc;

    // Stats do boneco

    int firePower = 2; // Tiles além do centro ocupado pela explosão da bomba (min = 1)
    int bombsMax = 1; // Quantidade de bombas do boneco
    int bombsUsed = 0; // Quantidade de bombas em uso (max = bombsMax)
    bool hasKick = false; // Se possui ou não habilidade de chute
    bool dead = false; // Beta: Não pode soltar bomba por um período. Estado causado por explosão.
    //bool hasPunch;
    //bool hasHold; 

    /// <summary>
    /// Nível atual de velocidade. Muda de acordo com os items "SpeedUp" acumulados.
    /// </summary>
    int speedLevel = 0;

    /// <summary>
    /// Velocidades do boneco. 
    /// </summary>
    float[] speeds = { 0.061f, 0.068f, 0.076f, 0.084f, 0.09f, 0.0976f, 0.106f, 0.113f, 0.122f };
    /* Valores "completos": 0.061, 0.0677..., 0.07625, 0.08413793103448275, 0.09037037037037, 
        0.0976, 0.1060869565217391, 0.11348837209302325, 0.122 (chute)
    */


    // Movimento e animação

    Animator animator;

    /// <summary>
    /// Informações da última animação de movimento.
    /// </summary>
    string[] previousWalkAnimationState = { "Stand", "Down", "Normal", "" };
    // { Ação, Direção, Velocidade, Carregando objeto (não usado atm) }
    // Ação: Stand, Walk; Direção: Down, Left, Right, Up; Velocidade: Normal, Slow; Carregando: "", "Carry"

    /// <summary>
    /// Valores usados para alterar a velocidade da ANIMAÇÃO de andar do boneco. Controlado pelo SpeedLevel.
    /// </summary>
    float[] walkCycleMults = { 0.5f, 0.6f, 0.7f, 0.75f, 0.8f, 0.85f, 0.9f, 0.95f, 1f };

    /// <summary>
    /// Guarda o estado dos inputs de direção do boneco. 
    /// </summary>
    Vector2Int movementState = new Vector2Int(); // Desenho dos estados no "autômato" na pasta "design & etc"

    // Constantes
    public const int MinFirePower = 2;
    public const int MaxFirePower = 10;
    const string sfxPath = "Sounds/SFX/Boneco/";

    #region gets & sets

    public int Layer {
        get { return gameObject.layer; }
        set {
            GetComponentsInChildren<Renderer>()[1].sortingOrder = value;
            gameObject.layer = value;
        }
    }

    public int BombsMax {
        get { return bombsMax; }
        set { bombsMax = value; }
    }

    public int FirePower {
        get { return firePower; }
        set {
            if (value < MinFirePower) {
                firePower = MinFirePower; 
            } else if (value > MaxFirePower) {
                firePower = MaxFirePower; 
            } else { 
                firePower = value;
            }
        }
    }

    public int SpeedLevel {
        get { return speedLevel; }
        set {
            if (value < 0) {
                speedLevel = 0;
            } else if (value >= speeds.Length) {
                speedLevel = speeds.Length - 1;
            } else {
                speedLevel = value;
            }

            animator.SetFloat("WalkCycleSpeed", walkCycleMults[SpeedLevel]); // Atualiza a velocidade da animação de andar
        }
    }

    public int BombsUsed {
        get { return bombsUsed; }
        set { bombsUsed = value; }
    }

    public bool Dead {
        get { return dead; }
        set { dead = value; }
    }

    /// <summary>
    /// Retorna a posição central da tile atual do boneco.
    /// </summary>
    public Vector2 curTileCenter() {
        return gc.centerPosition(transform.position);
    }

    #endregion

    void Start () {
        gc = GridController.instance;
        GetComponentsInChildren<Renderer>()[1].sortingOrder = Layer;
        animator = GetComponentsInChildren<Animator>()[0];
        if (!gc.randomBlocks) {
            // Configurações pro modo sem blocos no mapa (usado pra testes)
            FirePower = 6;
            BombsMax = 20;
            SpeedLevel = 8;
            hasKick = true;
        } else {
            FirePower = 2;
            BombsMax = 1;
            SpeedLevel = 0;
            hasKick = false;
        }
	}
	
	// Update is called once per frame
	void Update () {

        axisInputs();
        
        // ATENÇÃO (16/03/18): Sincronizar joystick com jogador quando tiver multiplayer. Só tá assim agora pq os ports ficam de brincation.
        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.JoystickButton1)) {
            placeBomb();
        }

        #region other inputs
        // http://wiki.unity3d.com/index.php?title=Xbox360Controller
        /* PC Mode WiiU Adapter:
            0: X
            1: A
            2: B
            3: Y
            4: L
            5: R
            6: ???
            7: Z
            8: ???
        */

        // Literal Pause. Ayy lmao
        if (Input.GetKeyDown(KeyCode.JoystickButton7)) {
            Debug.Break();
        }

        if (Input.GetKeyDown(KeyCode.C)) {
            //Debug.Log(GridController.instance.grid.WorldToLocal(transform.position));
            //Debug.Log(gc.tileContent((Vector2)transform.position + Vector2.up).name);
        }
        #endregion
    }

#region Axis & Movement functions

    /// <summary>
    /// Analisa os inputs dos eixos de movimento e realiza as ações que forem possíveis (mover o boneco, chutar uma bomba).
    /// </summary>
    void axisInputs() {

        // Informações da última animação de movimento. Ver declaração de "previousWalkAnimationState"
        string[] walkAnimationState = (string[])previousWalkAnimationState.Clone();

        string xsInput = "", ysInput = "", // Qual o input em tal direção do respectivo eixo ("" = nenhum)
            xsMove = "", ysMove = ""; // Qual o movimento possível em tal direção do respectivo eixo ("" = nenhum)
        bool xObstacle = false, yObstacle = false;   // Há obstáculo que limita movimento em tal eixo

        // Análise dos inputs de movimento

        if (Input.GetAxis("Horizontal") > 0) {
            xsInput = "Right";
            if (possibleMove(Vector2.right, out xObstacle)) {
                xsMove = "Right";
            }       
            
        } else if (Input.GetAxis("Horizontal") < 0) {
            xsInput = "Left";
            if (possibleMove(Vector2.left, out xObstacle)) {
                xsMove = "Left";
            }
        }

        if (Input.GetAxis("Vertical") > 0) {
            ysInput = "Up";
            if (possibleMove(Vector2.up, out yObstacle)) {
                ysMove = "Up";
            }
        } else if (Input.GetAxis("Vertical") < 0) {
            ysInput = "Down";
            if (possibleMove(Vector2.down, out yObstacle)) {
                ysMove = "Down";
            }
        }

        // Determinação e (se possível) realização do movimento, além de sua animação.

        if (updateMovementState(xsMove != "", ysMove != "")) {
            // Há movimento

            walkAnimationState[0] = "Walk";
            walkAnimationState[2] = "Normal";

            if (movementState[0] == 1) {
                calculateMovement(xsMove, xObstacle);
                walkAnimationState[1] = xsMove;

            } else if (movementState[1] == 1) {
                calculateMovement(ysMove, yObstacle);
                walkAnimationState[1] = ysMove;
            }

        } else {
            // Não há movimento

            if (xsInput != "") {
                walkAnimationState[0] = "Walk";
                walkAnimationState[1] = xsInput;
                walkAnimationState[2] = "Slow";         

            }  else if (ysInput != "") {
                walkAnimationState[0] = "Walk";
                walkAnimationState[1] = ysInput;
                walkAnimationState[2] = "Slow";

            } else {
                walkAnimationState[0] = "Stand";
            }       
        }

        // Coisas do chute (Provavelmente dá pra limpar um pouco essa parte)

        if (hasKick) {

            if (xsMove == "" && ysMove == "") {
                if (xsInput != "") {
                    Vector2 dir = new Vector2(Mathf.Sign(Input.GetAxis("Horizontal")), 0);
                    Vector2 nextTile = curTileCenter() + dir;

                    GameObject content = gc.tileContent(nextTile);
                    if (content != null && content.CompareTag("Bomb")) {
                        content.GetComponent<Bomb>().wasKicked(dir);
                    }
                } else if (ysInput != "") {
                    Vector2 dir = new Vector2(0, Mathf.Sign(Input.GetAxis("Vertical")));
                    Vector2 nextTile = curTileCenter() + dir;

                    GameObject content = gc.tileContent(nextTile);
                    if (content != null && content.CompareTag("Bomb")) {
                        content.GetComponent<Bomb>().wasKicked(dir);
                    }
                }
            }
        }

        // Animação
        playWalkAnimation(walkAnimationState);
        previousWalkAnimationState = (string[])walkAnimationState.Clone();
    }

    /// <summary>
    /// Verifica se há alguma colisão que impede o movimento pretendido pelo boneco.
    /// </summary>
    /// <param name="dir"> Direção (e.g. Vector2.up) </param>
    /// <param name="obstacle"> Se true, indica movimento limitado. </param>
    /// <returns> Movimento possível ou não </returns>
    bool possibleMove(Vector2 dir, out bool obstacle) {
        obstacle = false;

        GameObject go = gc.tileContent((Vector2)transform.position + dir);
        if (go != null) {

            // Desenho amigável em "PossibleMove & Slide" na pasta "Design & etc".

            if (dir == Vector2.right || dir == Vector2.left) {
                if (dir.x * (curTileCenter().x - transform.position.x) <= 0) {
                    return false;
                }

            } else if (dir == Vector2.up || dir == Vector2.down) {
                if (dir.y * (curTileCenter().y - transform.position.y) <= 0) {
                    return false;
                }

            } else {
                Debug.Log("PutaVida.exception: Impossible direction");
            }
            obstacle = true;
            
        }
        return true;
    }

    /// <summary>
    /// Baseado no movementState e nos movimentos possíveis atuais, define qual será a nova direção do boneco. 
    /// Necessário para lidar com inputs de movimento diagonais.
    /// </summary>
    /// <param name="xMove"> Se há input e possibilidade de movimento horizontal. </param>
    /// <param name="yMove"> Se há input e possibilidade de movimento vertical. </param>
    /// <returns> Se há movimento ou não </returns>
    bool updateMovementState(bool xMove, bool yMove) {
        // Interação entre os estados em "MovementState - 'Automato'" na pasta "Design & etc".
        // Desenho amigável em "UpdateMovementState - Exemplo" na pasta "Design & etc".

        if (xMove && yMove) {
            if (movementState == new Vector2Int(0, 0)) { 
                movementState = new Vector2Int(1, 2);

            } else if (movementState == new Vector2Int(1, 0)) {
                movementState = new Vector2Int(2, 1);

            } else if (movementState == new Vector2Int(0, 1)) { 
                movementState = new Vector2Int(1, 2);
            } 
        
        } else if (xMove) {
            movementState = new Vector2Int(1, 0);
        } else if (yMove) {
            movementState = new Vector2Int(0, 1);
        } else {
            movementState = new Vector2Int(0, 0);
            return false; 
        }

        return true; 
    }

    /// <summary>
    /// Define qual será o movimento realizado pelo boneco de acordo com o input e a posição atual.
    /// A ideia do movimento no jogo é sempre se manter no centro de um dos eixos da tile (ou ir a ele nas "curvas").
    /// </summary>
    /// <param name="dir"> Direção (e.g. Vector2.up) </param>
    /// <param name="obstacle"> Se há algum elemento à frente que limita o movimento </param>
    void calculateMovement(string dir, bool obstacle) {
        // - ATENÇÃO (Janeiro/2018): O translate pra fazer o movimento quebra o galho, mas não acho ideal. 
        //     O boneco fica com um "Efeito Luigi". Vai um pouco mais à frente do que deve nos movimentos.
        // - Update (16/03/2018): O "Efeito Luigi" aparentemente só ocorre quando usa teclado como input.    
        //     Até onde testei, o movimento usando controle de GameCube tá 10/10. A diferença deve ser por causa do analógico,
        //     mas não deixa de ser estranho... Devo ter feito alguma bosta nas fórmulas ai, ou o teclado é RUI mesmo hue.

        float speed = speeds[SpeedLevel];
        int directionSign = 0; // Indica se o movimento eve aumentar ou diminuir o x ou y da posição do boneco. Nome beta

        if(dir == "Right" || dir == "Up") {
            directionSign = 1;
        } else if (dir == "Left" || dir == "Down") {
            directionSign = -1;
        }

        // Sdds desenho amigável.

        if (dir == "Right" || dir == "Left") {
            if (obstacle) {
                // Com obstáculo à frente, pode apenas ir até o meio do eixo corrente
                if (Mathf.Abs(transform.position.x - curTileCenter().x) > 0.1) {
                    transform.Translate(directionSign * speed, 0, 0);
                } else {
                    transform.position = new Vector2(curTileCenter().x, transform.position.y);
                }

            } else { // Horizontal sem obstáculo
                float dif = transform.position.y - curTileCenter().y;

                // Dependendo da distância do boneco ao centro do eixo vertical da tile
                if (Mathf.Abs(dif) > 0.1) {
                    // Move diagonalmente até se aproximar
                    transform.Translate(directionSign * speed, Mathf.Sign(dif) * -1 * speed, 0);

                } else if (Mathf.Abs(dif) > 0) {
                    // Coloca no centro vertical e segue o movimento horizontal
                    transform.position = new Vector2(transform.position.x, curTileCenter().y);
                    transform.Translate(directionSign * speed, 0, 0);

                } else {
                    // Apenas move horizontalmente
                    transform.Translate(directionSign * speed, 0, 0);
                }
            }

        } else if (dir == "Up" || dir == "Down") {
            if (obstacle) {
                // Com obstáculo à frente, pode apenas ir até o meio do eixo corrente
                if (Mathf.Abs(transform.position.y - curTileCenter().y) > 0.1) {
                    transform.Translate(0, directionSign * speed, 0);
                } else {
                    transform.position = new Vector2(transform.position.x, curTileCenter().y);
                }

            } else { // Vertical sem obstáculo
                float dif = transform.position.x - curTileCenter().x;

                // Dependendo da distância do boneco ao centro do eixo horizontal da tile
                if (Mathf.Abs(dif) > 0.1) {
                    // Move diagonalmente até se aproximar
                    transform.Translate(Mathf.Sign(dif) * -1 * speed, directionSign * speed, 0);

                } else if (Mathf.Abs(dif) > 0) {
                    // Coloca no centro horizontal e segue o movimento vertical
                    transform.position = new Vector2(curTileCenter().x, transform.position.y);
                    transform.Translate(0, directionSign * speed, 0);

                } else {
                    // Apenas move verticalmente
                    transform.Translate(0, directionSign * speed, 0);
                }
            }
        } else {
            Debug.Log("PutaVida.exception: Impossible direction");
        }
    }
#endregion

    /// <summary>
    /// Faz o animator iniciar a devida animação (se já não for a atual).
    /// </summary>
    /// <param name="animationInfo"> Vetor com os informações da animação que deve ocorrer (ver </param>
    void playWalkAnimation(string[] animationInfo) {

        string newAnimationName = "";
        if (animationInfo[0] == "Walk") {
            newAnimationName = animationInfo[0] + animationInfo[1] + animationInfo[2]; // Completa o nome do estado
        } else if (animationInfo[0] == "Stand") {
            newAnimationName = animationInfo[0];
        } else { 
            Debug.Log("PutaVida.exception: Impossible walk animation state");
            return;
        }

        // Só faz algo se tiver que trocar a animação
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName(newAnimationName)) {

            if (animationInfo[0] == "Walk") {
            // Lembrete: Por serem diretamente relacionados, a velocidade da animação é ajustada junto com a do movimento (ver SpeedLevel set)

                // Se já estava andando antes, porém de outra forma
                if (previousWalkAnimationState[0] == "Walk") {
                    // Precisa continuar de onde o ciclo antigo estava. Pra animação ficar fluida.                    
                    animator.Play(newAnimationName, -1, animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
                } else {
                    // Se não, começa animação nova do 0. 
                    animator.Play(newAnimationName);
                }

            } else if (animationInfo[0] == "Stand") {
                /* A animação do estado Stand não é uma animação propriamente dita. 
                 * Ela apenas guarda ordenadamente os sprites do Boneco parado nas diferentes direções.
                 * A velocidade do animator no estado sempre é 0. Assim, ñão ocorre qualquer troca dos sprites.
                 * Pra acesar a direção desejada, use a devida sample 
                 * */

                float standSpriteTime = 0f;
                if(animationInfo[1] == "Down") {
                    standSpriteTime = 0f; // 1º sprite
                } else if (animationInfo[1] == "Left") {
                    standSpriteTime = 0.25f; // 2º sprite
                } else if (animationInfo[1] == "Right") {
                    standSpriteTime = 0.5f; // 3º sprite
                } else if (animationInfo[1] == "Up"){
                    standSpriteTime = 0.75f; // 4º sprite
                } else {
                    Debug.Log("PutaVida.exception: Impossible animation direction");
                }
                animator.Play(newAnimationName, -1, standSpriteTime);
            }
        }
    }

    /// <summary>
    /// Cria bomba na tile atual se possível
    /// </summary>
    void placeBomb() {       
        if (!dead && BombsUsed < bombsMax && gc.tileContent(transform.position) == null) {
            BombsUsed++;
            Bomb b = Instantiate(Resources.Load<Bomb>("prefabs/Bomb"));
            b.setup(this);
        }
    }

    /// <summary>
    /// Realiza as ações e alterações de acordo com o item adquirido.
    /// </summary>
    void gotItem(Item item) {
        if(item == null) {
            Debug.Log("PutaVida.Exception: GO com tag Item não é Item");
            return;
        }

        GetComponent<AudioSource>().clip = (AudioClip)Resources.Load(sfxPath + "GotItem");
        GetComponent<AudioSource>().Play();
        switch (item.name) {
            case "FireUp": 
                FirePower++;
                break;

            case "BombUp":
                BombsMax++;
                break;

            case "SpeedUp":
                SpeedLevel++;
                break;

            case "Kick":
                hasKick = true;
                break;

            default:
                Debug.Log("PutaVida.Exception: ItemNotFound");
                break;
        }
        Destroy(item.gameObject);
    }

    /// <summary>
    /// BETA. Controla o tempo em que o boneco fica no estado dead (sem bombas e vermelho)
    /// </summary>
    IEnumerator die() {
        yield return new WaitForSeconds(1.5f);
        gameObject.GetComponentsInChildren<SpriteRenderer>()[1].color = Color.white;
        dead = false;
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.CompareTag("Explosion") && !dead) {
            dead = true;
            gameObject.GetComponentsInChildren<SpriteRenderer>()[1].color = Color.red; // Beta
            GetComponent<AudioSource>().clip = (AudioClip)Resources.Load(sfxPath + "Death");
            GetComponent<AudioSource>().Play();
            StartCoroutine(die());
        } else if (collider.CompareTag("Item")) {
            gotItem(collider.gameObject.GetComponent<Item>());
        }
    }

}