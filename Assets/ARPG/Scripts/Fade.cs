using UnityEngine;
using UnityEngine.UI; // Necessário para manipular imagens
using System.Collections;

public class FadeEffect : MonoBehaviour
{ 
    public GameObject Painel;
    public Image fadeImage;
    public float duracao = 2.0f;

    void Start()
    {
        // Começa com a tela preta e faz o Fade In (fica transparente)
        StartCoroutine(ExecutarFadeIn());
    }

    IEnumerator ExecutarFadeIn()
    {
        float tempo = 0;
        Color cor = fadeImage.color;

        // FADE OUT: De Transparente (0) para Opaco (1)
        tempo = 0;
        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            cor.a = Mathf.Lerp(0, 1, tempo / duracao);
            fadeImage.color = cor;
            yield return null;
        }
    }
}
