using System.Collections;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnAnnounce : Singleton<TurnAnnounce>
{
    [SerializeField] private Text _text;

    public void Announce(string text)
    {
        _text.text = text;

        StartCoroutine(Coroutine());
        return;

        IEnumerator Coroutine()
        {
            var cg = GetComponent<CanvasGroup>();
            yield return MMFade.FadeCanvasGroup(cg, 1f, 1);
            yield return new WaitForSeconds(1);
            yield return MMFade.FadeCanvasGroup(cg, 1f, 0);
        }
    }
}