using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Resolvedor : MonoBehaviour
{
    public GerenciadorGrade gerenciador;
    public Color corAndado;
    public float velocidade = 2f;

    private Coroutine moverPorCaminhoCoroutine;


    public void ResolverLabirinto()
    {
        Node[,] grade = gerenciador.GetGrade(); // Pega a grade do problema

        // Implementacao do A*, faca sua implementacao aqui
        StartCoroutine(AEstrela(gerenciador.posicaoInicio, gerenciador.posicaoDestino, (caminho) =>
        {
            // Chama a funcao de mover por caminho
            StartCoroutine(MoverPorCaminhoCoroutine(caminho));
        }));

    }

    private IEnumerator AEstrela(Vector2 posicaoInicio, Vector2 posicaoDestino, Action <List <Node>> callback = null)
    {
        Node[,] grade = gerenciador.GetGrade();
        List<Node> listaEspera = new List<Node>();
        Dictionary<Vector2, Node> processados = new Dictionary<Vector2, Node>();


        Node nodeInicial = grade[(int)posicaoInicio.x, (int)posicaoInicio.y];
        Node nodeFinal = grade[(int)posicaoDestino.x, (int)posicaoDestino.y];

        listaEspera.Add(nodeInicial);

        //enquanto a lista de espera n�o for vazia
        while (listaEspera.Count != 0)
        {
            //pegar primeiro n� na lista de espera, que tem o menor f
            Node nodeAtual = listaEspera[0];

            listaEspera.RemoveAt(0);
            processados.Add(nodeAtual.posicao, nodeAtual);
            ColorirTile(Color.red, nodeAtual.gameObject); // Colorindo o tile que foi processado    
            yield return new WaitForSeconds(0.04f); // Espera um frame para processar o proximo tile

            //verificar se � o nodeFinal
            if (nodeAtual.posicao == nodeFinal.posicao)
            {
                List<Node> caminho = new List<Node>();
                Node noAux = nodeAtual;
                caminho.Add(nodeAtual);
                while (noAux.pai != null)
                {
                    caminho.Add(noAux.pai);
                    noAux = noAux.pai;
                }

                caminho.Reverse();

                callback?.Invoke(caminho); // Chama o callback se ele existir
                yield break;
            }

            //gerar os filhos(vizinhos) e calcular custo F pra cada um
            List<Node> vizinhos = new List<Node>();
            vizinhos = GetVizinhos(nodeAtual, grade);

            foreach (Node vizinho in vizinhos)
            {
                if (!vizinho.ehAndavel || processados.ContainsKey(vizinho.posicao)) // Se � movimento v�lido ou j� possui a key
                    continue;

                float custoMovimento = (vizinho.direcao == direcao.Reto) ? 1f : 1.41f;
                float novoCustoG = nodeAtual.custoG + custoMovimento; // Soma do custo do caminho

                bool estaNaLista = listaEspera.Exists(n => n.posicao == vizinho.posicao); // Verificando se existe a posi��o na lista de espera

                if (vizinho.pai == null || novoCustoG < vizinho.custoG)
                {
                    vizinho.pai = nodeAtual;
                    vizinho.custoG = novoCustoG;
                    vizinho.custoH = Vector2.Distance(vizinho.posicao, posicaoDestino);

                    if (!estaNaLista){ 
                        listaEspera.Add(vizinho);
                        ColorirTile(Color.blue, vizinho.gameObject); // Colorindo o tile que foi processado    
                        yield return new WaitForSeconds(0.04f); // Espera um frame para processar o proximo tile
                    }
                }

                //Texto do custo do tile
                if (vizinho.ehAndavel && vizinho.costText != null)
                {
                    string custoFTruncado = (Mathf.Floor(vizinho.custoF* 100f) / 100f).ToString();
                    string custoGTruncado = (Mathf.Floor(vizinho.custoG * 100f) / 100f).ToString();
                    string custoHTruncado = (Mathf.Floor(vizinho.custoH * 100f) / 100f).ToString();
                    string pesoTruncado = (Mathf.Floor(vizinho.peso * 100f) / 100f).ToString();
                    vizinho.costText.text = custoGTruncado + "\n" + custoFTruncado + "\n" + custoHTruncado + "\n" + pesoTruncado;
                }
            }

            listaEspera.Sort((node1, node2) => node1.custoF.CompareTo(node2.custoF));
        }

        LigarPopup();
        yield break; // Caso nao encontre caminho
    }

    /*private List<Node> FindPath(HashSet<Node> processados, Node nodeFinal){
        List<Node> caminho = new List<Node>();



        caminho.Add();


    }*/

    private List<Node> GetVizinhos(Node node, Node[,] grade)
    { // Funcao auxiliar para achar os vizinhos
        List<Node> vizinhos = new List<Node>();
        int x = (int)node.posicao.x;
        int y = (int)node.posicao.y;

        // Checa os vizinhos nas 4 direcoes
        bool esquerda = x > 0;
        bool direita = x < grade.GetLength(0) - 1;
        bool cima = y < grade.GetLength(1) - 1;
        bool baixo = y > 0;

        bool reto = esquerda ^ baixo && esquerda ^ cima && direita ^ baixo && direita ^ cima;

        //Horizontal/Vertical
        if (esquerda) vizinhos.Add(grade[x - 1, y]); // Esquerda
        if (direita) vizinhos.Add(grade[x + 1, y]); // Direita
        if (baixo) vizinhos.Add(grade[x, y - 1]); // Baixo
        if (cima) vizinhos.Add(grade[x, y + 1]); // Cima
        if (reto) vizinhos[vizinhos.Count - 1].direcao = direcao.Reto;

        //Diagonais
        if (esquerda && cima) vizinhos.Add(grade[x - 1, y + 1]); // Esquerda-Cima
        if (esquerda && baixo) vizinhos.Add(grade[x - 1, y - 1]); //Esquerda-Baixo
        if (direita && cima) vizinhos.Add(grade[x + 1, y + 1]); //Direita-Cima
        if (direita && baixo) vizinhos.Add(grade[x + 1, y - 1]); //Direita-Baixo
        if (!reto) vizinhos[vizinhos.Count - 1].direcao = direcao.Diagonal;

        return vizinhos;
    }

    private void ColorirTile(Color cor, GameObject tile)
    {
        tile.GetComponent<SpriteRenderer>().color = cor;
    }

    public void ResetarPosicaoResolvedor()
    {
        StopAllCoroutines();
        moverPorCaminhoCoroutine = null;
        transform.position = (Vector3)gerenciador.posicaoInicio + gerenciador.transform.position;
    }

    private IEnumerator MoverPorCaminhoCoroutine(List<Node> caminho)
    {
        if (moverPorCaminhoCoroutine != null) yield break; // Quebrando pois o jogador ja esta andando
        if (caminho == null || caminho.Count == 0)
        { // Quebrando pois o caminho nao existe, ou eh impossivel
            Debug.LogWarning("Caminho Impossivel");
            LigarPopup();
            yield break;
        }

        for (int i = 0; i < caminho.Count; i++)
        {
            Node nodoAtual = caminho[i];

            ColorirTile(corAndado, nodoAtual.gameObject);

            if (i < caminho.Count - 1)
            {
                Node proximoNodo = caminho[i + 1];
                Vector2 posicaoAtual = nodoAtual.transform.position; // Pegando a posicao relativa ao mundo
                Vector2 posicaoDestino = proximoNodo.transform.position; // Pegando a posicao relativa ao mundo

                yield return StartCoroutine(MoverParaPosicaoCoroutine(posicaoAtual, posicaoDestino));
            }
        }
        moverPorCaminhoCoroutine = null;
    }

    private IEnumerator MoverParaPosicaoCoroutine(Vector3 posicaoAtual, Vector3 posicaoDestino)
    {
        float tempo = 0f;
        float distancia = Vector3.Distance(posicaoAtual, posicaoDestino);

        while (tempo < distancia / velocidade)
        {
            tempo += Time.deltaTime;
            float interpolacao = tempo / (distancia / velocidade);
            transform.position = Vector3.Lerp(posicaoAtual, posicaoDestino, interpolacao);
            yield return null;
        }

        transform.position = posicaoDestino;
    }

    private void instaciaTexto(Node nodes) { 
    }

    private void LigarPopup()
    {
        if (gerenciador.popup != null)
        {
            gerenciador.popup.SetActive(true);
        }
    }
}