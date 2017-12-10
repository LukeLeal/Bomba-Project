using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour {

    int power; // Tiles além do centro ocupado pela explosão (min 1)
    // bool pierce; // Explosão não é limitada por blocos destrutíveis.
    Boneco owner; // Boneco dono da bomba. 

    int state; // 1: Ticking; 2: Not ticking; 11: Explosion
    public const int Ticking = 1;
    public const int NotTicking = 2;
    public const int Exploding = 11;

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
        transform.position = GridController.instance.centerPosition(b.transform.position);
        GetComponent<SpriteRenderer>().sortingOrder = 3;
        state = Ticking;
        StartCoroutine(tick());
    }

    // Tempo até explodir. Terá mudanças quando o estado NotTicking for implementado.
    IEnumerator tick() {
        // #sdds animação
        yield return new WaitForSeconds(2f);
        state = Exploding;
        explode();
    }

    // Cria a explosão central. Ela vai criando o resto.
    void explode() {
        GetComponent<Collider2D>().enabled = false; // Desativa o próprio collider pra não interferir nos cálculos

        // Cria as explosões pra cada lado
        createExplosion(Vector2.up);
        createExplosion(Vector2.right);
        createExplosion(Vector2.down);
        createExplosion(Vector2.left);

        Explosion e = Instantiate(Resources.Load<Explosion>("Prefabs/Explosion"), transform.position, Quaternion.identity); // Centro
        owner.BombsUsed--;    
        Destroy(gameObject); // Aparentemente tanto faz se destrói o gameObject antes de fazer o resto. hmm
    }

    // Cria os objetos das explosões
    void createExplosion(Vector2 dir) {
        int range = calculateRange(dir);

        Vector2 curPos = (Vector2) transform.position + dir;
        while (range > 0) {
            if (range == 1) {
                // Caso do último. Vê se tem objeto no local ou não. 
            } else {
                Explosion e = Instantiate(Resources.Load<Explosion>("Prefabs/Explosion"), curPos, Quaternion.identity);
                curPos += dir;
            }
            range--;
        }
    }

    // Define o alcance da explosão
    int calculateRange(Vector2 dir) {
        // Raycast
        List<RaycastHit2D> hits = new List<RaycastHit2D>(Physics2D.RaycastAll(transform.position, dir, 5));

        // Ordena os hits de acordo com a direção
        if (dir == Vector2.up) {
            hits.Sort((h1, h2) => h1.point.y.CompareTo(h2.point.y)); // y Crescente
        } else if (dir == Vector2.down) {
            hits.Sort((h1, h2) => h2.point.y.CompareTo(h1.point.y)); // y Decrescente
        } else if (dir == Vector2.right) {
            hits.Sort((h1, h2) => h1.point.x.CompareTo(h2.point.x)); // x Crescente
        } else if (dir == Vector2.left) {
            hits.Sort((h1, h2) => h2.point.x.CompareTo(h1.point.x)); // x Decrescente
        } else {
            // VISH
        }

        foreach(RaycastHit2D hit in hits) {
            // Vê se é ignorável ou não (tm só bonecos). Calcula distância em tiles no primeiro que não for.
            if (hit.collider.tag == "Player" || hit.collider.tag == "Explosion") {
                continue;
            }
            // Retorna distância dos dois centros. Deve dar certo nos tiles. Ai tira 1 ou sei la -q
            return (int)Vector2.Distance(transform.position, GridController.instance.centerPosition(hit.point)) ;
        }

        return 5; // Power; // Se não tem nada no caminho, range máximo
    }
}
