using UnityEngine;
using UnityEngine.UI;

public class MudarCorButao : MonoBehaviour
{
    public Button meuButao;
    private bool active = true;

    void Start()
    {
        meuButao.onClick.AddListener(TrocarCor);
    }

    void TrocarCor()
    {
        if (active)
        {
            meuButao.GetComponent<Image>().color = Color.red;
            active = false;
        }
        else
        {
            meuButao.GetComponent<Image>().color = Color.green;
            active = true;
        }
    }
}