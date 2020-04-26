using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleController : MonoBehaviour
{
    [SerializeField] private GameObject background = default;
    [SerializeField] private Text logoText = default;
    [SerializeField] private GameObject tapText = default;
    [SerializeField] private AudioSource titleBgm = default;
    [SerializeField] private AudioSource tapSound = default;

    private void Start()
    {
        background.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/background-logo");
        logoText.color = Color.clear;
        tapText.SetActive(false);
        
        // ロゴの表示
        StartCoroutine(DisplayLogo());
    }

    IEnumerator DisplayLogo()
    {
        yield return new WaitForSeconds(1);

        /* ロゴの表示 */

        float _transparent = 0;

        // フェードイン
        while (_transparent <= 1)
        {
            _transparent += 0.04f;
            logoText.color = new Color(0, 0, 0, _transparent);
            yield return new WaitForFixedUpdate();
        }

        yield return new WaitForSeconds(2);

        // フェードアウト
        while (_transparent >= 0)
        {
            _transparent -= 0.025f;
            logoText.color = new Color(0, 0, 0, _transparent);
            yield return new WaitForFixedUpdate();
        }

        yield return new WaitForSeconds(1);

        background.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/background-title");
        tapText.SetActive(true);
        titleBgm.Play();
    }


    private float tapTransparent = 0.1f; // テキストの透明度
    private float flashSpeed = 0.01f; // 点滅スピード

    private void FixedUpdate()
    {
        if (tapText.activeSelf == true)
        {
            // Tapテキストの点滅
            if (tapTransparent <= 0 || tapTransparent >= 1)
            {
                flashSpeed *= -1;
            }
            tapTransparent += flashSpeed;
            tapText.GetComponent<Text>().color = new Color(0.4f, 0.4f, 0.4f, tapTransparent);

            // タップされたらバトルシーンへ
            if (Input.touchCount > 0 || Input.GetMouseButtonDown(0))
            {
                StartCoroutine(GoToBattleScene());
                enabled = false;
            }
        }
    }

    IEnumerator GoToBattleScene()
    {
        tapSound.Play();

        yield return new WaitForSeconds(3);

        SceneManager.LoadScene("BattleScene");
    }
}
