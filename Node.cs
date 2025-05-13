using TMPro;
using UnityEngine;

public enum direcao
{
    Reto,
    Diagonal
}
public class Node : MonoBehaviour
{
    public Vector2 posicao { get; set; }
    public bool ehAndavel { get; set; } = true;
    public float custoG { get; set; }
    public float custoH { get; set; } // Heuristica

    public float peso = 10;
    public float custoF => (custoG / peso) + custoH; // Advindo da formula do A*
    public Node pai { get; set; } // A refer�ncia para o noh anterior

    public direcao direcao { get; set; }

    public TMP_Text costText;
}