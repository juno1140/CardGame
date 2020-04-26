using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardController : MonoBehaviour
{
    /* ジャンケン種別 */
    public enum RSPType
    {
        Rock,     // グー
        Scissors, // チョキ
        Paper    // パー
    }

    /* カードプロパティ */
    public string cardName; // カード名
    public RSPType rspType; // ジャンケン種別変数
    public int power = 1; // パワー

    public List<string> battleEffect = new List<string>(); // バトル効果リスト

    private GameController gameController;
    private UIController uIController;

    private void Start()
    {
        gameController = GameObject.Find("GameMaster").GetComponent<GameController>();
        uIController = GameObject.Find("Canvas").GetComponent<UIController>();
    }

    public string GetEffectSentence(int cardNum)
    {
        string effectSen = "";

        switch (cardNum)
        {
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
                break;
            case 6:
                effectSen = "場に出たとき効果を発動する。相手のキャラカードのパワーを-1にする。";
                break;
            case 7:
                effectSen = "場に出たとき効果を発動する。次のキャラカードのパワーを+1にする。";
                break;
            case 8:
                effectSen = "場に出たとき効果を発動する。次のキャラカードは効果にDrawWinを得る。";
                break;
            case 9:
                effectSen = "場に出たとき効果を発動する。相手の場のカードのバトル効果を無効にする。";
                break;
            default:
                break;
        }

        // バトル効果の記述
        foreach (string eff in battleEffect)
        {
            // 文があれば改行する
            if (effectSen != "")
            {
                effectSen += "\r\n";
            }
            effectSen += "※" + eff;
        }

        // 効果を持たない場合
        if (effectSen == "")
        {
            effectSen = "効果なし";
        }

        return effectSen;
    }

    /* 場に出たときの効果 */
    public bool appearanceEffectFlg = true; // 効果を一度使ったらfalseにする
    public IEnumerator AppearanceEffect()
    {
        // 効果を一度使っていたら無効
        if (!appearanceEffectFlg) { yield break; }

        switch (int.Parse(gameObject.name))
        {
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
                break;
            case 6:
                yield return PowerDown();
                break;
            case 7:
                yield return NextCardPowerIncrement();
                break;
            case 8:
                yield return NextCardAddEffectDrawWin();
                break;
            case 9:
                yield return EffectInvalidation();
                break;
            default:
                Debug.LogError("そのカードNoは存在しません。");
                break;
        }

        // フラグをOFFにする
        appearanceEffectFlg = false;
    }

    /// <summary>
    /// 相手の場のカードのパワーを-1にする
    /// </summary>
    private IEnumerator PowerDown()
    {
        // ターゲットとなるプレイヤーの設定
        string targetPlayer = gameObject.tag == "player" ? "enemy" : "player";

        // 対象プレイヤーの場にカードが出ていなければ無効
        if (gameController.fieldCardObj[targetPlayer] == null) { yield break; }

        // 場のカードのパワーを-1
        gameController.fieldCardObj[targetPlayer].GetComponent<CardController>().power--;

        // UIに反映させる
        uIController.setFieldCardPower(targetPlayer, -1);
    }

    /// <summary>
    /// 次の自分のカードのパワーを+1にする
    /// </summary>
    private IEnumerator NextCardPowerIncrement()
    {
        if (gameController.Cards[gameObject.tag].Count != 0)
        {
            gameController.Cards[gameObject.tag][0].GetComponent<CardController>().power++;
        }
        yield break;
    }

    /// <summary>
    /// 次の自分のカードのバトル効果にDrawWinを付与する
    /// </summary>
    private IEnumerator NextCardAddEffectDrawWin()
    {
        if (gameController.Cards[gameObject.tag].Count != 0)
        {
            CardController card = gameController.Cards[gameObject.tag][0].GetComponent<CardController>();

            if (!card.battleEffect.Contains("DrawWin"))
            {
                gameController.Cards[gameObject.tag][0].GetComponent<CardController>().battleEffect.Add("DrawWin");
            }
        }
        yield break;
    }

    /// <summary>
    /// 相手の場のカードのバトル効果を無効にする
    /// </summary>
    private IEnumerator EffectInvalidation()
    {
        // ターゲットとなるプレイヤーの設定
        string targetPlayer = gameObject.tag == "player" ? "enemy" : "player";

        // 対象プレイヤーの場にカードが出ていなければ無効
        if (gameController.fieldCardObj[targetPlayer] == null) { yield break; }

        // バトル効果を全てクリアする
        gameController.fieldCardObj[targetPlayer].GetComponent<CardController>().battleEffect.Clear();

        // UIに反映させる
        uIController.setFieldCardInfo(targetPlayer);
    }
}
