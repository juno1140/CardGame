using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    // スクリプト
    [SerializeField] private GameController gameController = default;

    // Player情報
    [SerializeField] private Text playerDeckNum = default;
    [SerializeField] private Text playerDropNum = default;
    [SerializeField] private Text enemyDeckNum = default;
    [SerializeField] private Text enemyDropNum = default;

    // フィールドカード情報
    [SerializeField] private GameObject playerFieldCardinfo = default;
    [SerializeField] private GameObject enemyFieldCardinfo = default;
    [SerializeField] private Image playerFieldCardRSPType = default;
    [SerializeField] private Image enemyFieldCardRSPType = default;
    [SerializeField] private Text playerFieldCardName = default;
    [SerializeField] private Text enemyFieldCardName = default;
    [SerializeField] private Text playerFieldCardPower = default;
    [SerializeField] private Text enemyFieldCardPower = default;
    [SerializeField] private Text playerFieldCardEffect = default;
    [SerializeField] private Text enemyFieldCardEffect = default;

    // タッチパネル
    [SerializeField] private GameObject PlayerDeckPanel = default;
    [SerializeField] private Text TapText = default;

    // Resultテキスト
    [SerializeField] private Text resultText = default;

    // デッキの枚数取得
    public int getDeckNum(string player)
    {
        if (player == "player")
        {
            return int.Parse(playerDeckNum.text.Substring(5));
        }
        else
        {
            return int.Parse(enemyDeckNum.text.Substring(5));
        }
    }
    // デッキの枚数を設定
    public void setDeckNum(string player, int num)
    {
        if (player == "player")
        {
            playerDeckNum.text = "Deck:" + num.ToString();
        }
        else
        {
            enemyDeckNum.text = "Deck:" + num.ToString();
        }
    }
    // ドロップカードの枚数取得
    public int getDropNum(string player)
    {
        if (player == "player")
        {
            return int.Parse(playerDropNum.text.Substring(5));
        }
        else
        {
            return int.Parse(enemyDropNum.text.Substring(5));
        }
    }
    // ドロップカードの枚数を設定
    public void setDropNum(string player, int num)
    {
        if (player == "player")
        {
            playerDropNum.text = "Drop:" + num.ToString();
        }
        else
        {
            enemyDropNum.text = "Drop:" + num.ToString();
        }
    }
    // デッキの枚数を1減らす
    public void decrementDeckNum(string player)
    {
        int num = getDeckNum(player) - 1;
        setDeckNum(player, num);
    }
    // ドロップカードの枚数を1増やす
    public void incrementDropNum(string player)
    {
        int num = getDropNum(player) + 1;
        setDropNum(player, num);
    }

    // フィールドカード情報の表示・非表示切替
    public void displayFieldCardInfo(string player, bool b)
    {
        
        if (player == "player")
        {
            playerFieldCardinfo.SetActive(b);
        }
        else
        {
            enemyFieldCardinfo.SetActive(b);
        }
    }

    // フィールドカード情報を設定
    public void setFieldCardInfo(string player)
    {
        // フィールドカードの設定
        CardController fieldCard = gameController.fieldCardObj[player].GetComponent<CardController>();

        Image fieldCardRSPType;
        Text fieldCardName;
        Text fieldCardPower;
        Text fieldCardEffect;

        if (player == "player")
        { // player
            fieldCardRSPType = playerFieldCardRSPType;
            fieldCardName = playerFieldCardName;
            fieldCardPower = playerFieldCardPower;
            fieldCardEffect = playerFieldCardEffect;
        }
        else
        { // enemy
            fieldCardRSPType = enemyFieldCardRSPType;
            fieldCardName = enemyFieldCardName;
            fieldCardPower = enemyFieldCardPower;
            fieldCardEffect = enemyFieldCardEffect;
        }

        // RSPTypeの設定
        switch (fieldCard.rspType)
        {
            case CardController.RSPType.Rock:
                fieldCardRSPType.sprite = Resources.Load<Sprite>("Images/rock");
                break;
            case CardController.RSPType.Scissors:
                fieldCardRSPType.sprite = Resources.Load<Sprite>("Images/scissors");
                break;
            case CardController.RSPType.Paper:
                fieldCardRSPType.sprite = Resources.Load<Sprite>("Images/paper");
                break;
            default:
                Debug.Log("RSPTypeエラーです！");
                break;
        }
        // Nameの設定
        fieldCardName.text = fieldCard.cardName;
        // Powerの設定
        fieldCardPower.text = fieldCard.power.ToString();
        // Effectの設定
        fieldCardEffect.text = fieldCard.GetEffectSentence(int.Parse(fieldCard.name));
    }

    // 場のカードのパワーを取得
    public int getFieldCardPower(string player)
    {
        if (player == "player")
        {
            return int.Parse(playerFieldCardPower.text);
        }
        else
        {
            return int.Parse(enemyFieldCardPower.text);
        }
    }
    // 場のカードのパワーを設定
    public void setFieldCardPower(string player, int num)
    {
        if (player == "player")
        {
            playerFieldCardPower.text = (getFieldCardPower(player) + num).ToString();
        }
        else
        {
            enemyFieldCardPower.text = (getFieldCardPower(player) + num).ToString();
        }
    }

    /// <summary>
    /// ドローパネルを有効・無効の設定
    /// </summary>
    /// <param name="b">ON(true)\OFF(false)</param>
    public void SwitchDrawFlag(bool b)
    {
        PlayerDeckPanel.SetActive(b);
    }

    private float transparent = 0.9f;
    private float flashSpeed = 0.01f;

    private void FixedUpdate()
    {
        // タップテキストのアニメーション
        if (PlayerDeckPanel.activeSelf == true)
        {
            if (transparent <= 0 || transparent >= 1)
            {
                flashSpeed *=  -1;
            }
            transparent -= flashSpeed;
            TapText.color = new Color(1f, 0.3f, 0f, transparent);
        }
    }

    // パネルタップ時の処理
    public void onClickButton(GameObject obj)
    {
        SwitchDrawFlag(false);

        if (obj.name == "PlayerDeckPanel")
        {
            StartCoroutine(gameController.Turn());
        }
        else
        {
            Debug.Log("思わぬエラーです！");
        }
    }

    /// <summary>
    /// 結果を表示する
    /// </summary>
    /// <param name="result">
    /// 1: プレイヤー勝ち
    /// 2: プレイヤー負け
    /// 3: 引き分け
    /// </param>
    public void ShowResult(int result)
    {
        if (result == 1)
        {
            resultText.text = "You Win!!";
            resultText.color = Color.red;
        }
        else if (result == 2)
        {
            resultText.text = "You Lose!!";
            resultText.color = Color.blue;
        }
        else
        {
            resultText.text = "Draw";
            resultText.color = Color.blue;
        }

        // テキストを表示する
        resultText.transform.parent.gameObject.SetActive(true);

        
    }
}