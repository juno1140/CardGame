using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [SerializeField] private int deck_num = 15; // カードの裏面
    [SerializeField] private Sprite card_back = default; // カードの裏面
    [SerializeField] private GameObject PlayerDeck = default; // プレイヤーのデッキ
    [SerializeField] private GameObject EnemyDeck = default;  // 敵プレイヤーのデッキ
    [SerializeField] private UIController uIController = default; // UI制御スクリプト

    // カードリスト
    public Dictionary<string, List<GameObject>> Cards = new Dictionary<string, List<GameObject>>()
    {
        ["player"] = new List<GameObject>(),
        ["enemy"] = new List<GameObject>()
    };
    // デッキの位置
    private readonly Dictionary<string, Vector3> DeckPosition = new Dictionary<string, Vector3>()
    {
        ["player"] = new Vector3(0, 0.7f, -14f),
        ["enemy"] = new Vector3(0, 0.7f, 14f)
    };
    // デッキの向き
    private readonly Dictionary<string, Quaternion> DeckRotation = new Dictionary<string, Quaternion>()
    {
        ["player"] = Quaternion.Euler(90f, 0, 0) * Quaternion.AngleAxis(180, Vector3.up),
        ["enemy"] = Quaternion.Euler(90f, 180f, 0) * Quaternion.AngleAxis(180, Vector3.up)
    };
    // 場の位置
    private readonly Dictionary<string, Vector3> FieldPosition = new Dictionary<string, Vector3>()
    {
        ["player"] = new Vector3(0, 0.7f, -5f),
        ["enemy"] = new Vector3(0, 0.7f, 5f)
    };
    // 場のカードの有無
    public Dictionary<string, GameObject> fieldCardObj = new Dictionary<string, GameObject>()
    {
        ["player"] = null,
        ["enemy"] = null
    };
    // ドロップゾーンの位置
    private readonly Dictionary<string, Vector3> DropPosition = new Dictionary<string, Vector3>()
    {
        ["player"] = new Vector3(8, 0.7f, -10f),
        ["enemy"] = new Vector3(-8, 0.7f, 10f)
    };
    // ドロップゾーンのカードリスト
    public Dictionary<string, List<GameObject>> DropCardList = new Dictionary<string, List<GameObject>>()
    {
        ["player"] = new List<GameObject>(),
        ["enemy"] = new List<GameObject>()
    };

    private void Start()
    {
        // デッキの生成
        SetDeck("player"); // プレイヤー
        SetDeck("enemy"); // 敵

        // ドローパネルを有効にする
        uIController.SwitchDrawFlag(true);
    }

    /// <summary>
    /// デッキの設定
    /// </summary>
    /// <param name="player">プレイヤー</param>
    private void SetDeck(string player)
    {
        // Assetsの内からカードPrefabの総数を取得
        UnityEngine.Object[] prefabs = Resources.LoadAll("Prefabs/Cards");

        // オブジェクトの生成と各種設定
        for (int num = 0; num < deck_num; num++)
        {
            // カードのPrefabをランダムに取得
            GameObject card = (GameObject)Resources.Load("Prefabs/Cards/" + UnityEngine.Random.Range(1, prefabs.Length + 1));

            Cards[player].Add(Instantiate(card, DeckPosition[player], DeckRotation[player])); // 生成
            Cards[player][num].GetComponent<SpriteRenderer>().sprite = card_back; // 裏面の設定
            Cards[player][num].name = card.name; // 名前の設定([Cloneを消す])
            Cards[player][num].transform.parent = player == "player" ? PlayerDeck.transform : EnemyDeck.transform; // 親の設定
            Cards[player][num].tag = player;
        }
        // デッキとドロップカード枚数のUIを設定
        uIController.setDeckNum(player, deck_num);
        uIController.setDropNum(player, 0);
    }
    
    /// <summary>
    /// ターン処理
    /// </summary>
    public IEnumerator Turn()
    {
        // 自分の場にカードが出ている場合、エラーを出す
        if (fieldCardObj["player"])
        {
            Debug.LogError("自分の場にカードが存在します。");
            yield break;
        }

        // [player]カードをドロー
        yield return DrawCard("player");

        // プレイヤーが負けるまでバトルを続ける
        while (fieldCardObj["player"])
        {
            // [enemy]場にカードがなければカードをドロー
            if (!fieldCardObj["enemy"] && uIController.getDeckNum("enemy") != 0)
            {
                yield return DrawCard("enemy");
            }
            
            // カードの情報を変数に設定
            CardController playerCard = fieldCardObj["player"].GetComponent<CardController>();
            CardController enemyCard = fieldCardObj["enemy"].GetComponent<CardController>();
            // 場に出たとき効果を持っていれば発動する
            yield return playerCard.AppearanceEffect();
            yield return enemyCard.AppearanceEffect();
            
            // バトルフェーズ
            yield return BattlePhase();

            // 勝敗確認
            if (CheckResult() != 0)
            {
                yield break;
            }
        }

        // ドローパネルを有効にする
        uIController.SwitchDrawFlag(true);
    }

    /// <summary>
    /// 山札からカードを場に出す
    /// </summary>
    /// <param name="player">プレイヤー</param>
    public IEnumerator DrawCard(string player)
    {
        if (Cards[player].Count != 0)
        {
            // デッキの先頭を変数に代入し、デッキから削除する。
            fieldCardObj[player] = Cards[player][0];
            Cards[player].RemoveAt(0);

            // デッキ枚数を減らす
            uIController.decrementDeckNum(player);

            // 場に出すアニメーション(①〜④)
            // ①上に上げる
            while (fieldCardObj[player].transform.position.y <= 5)
            {
                fieldCardObj[player].transform.position += new Vector3(0, 0.5f, 0);
                yield return new WaitForFixedUpdate();
            }
            // ②前に出す
            float posDir = player == "player" ? 1 : -1; // プレイヤーによって移動方向が異なる
            while ((float)Math.Round(fieldCardObj[player].transform.position.z, 1, MidpointRounding.AwayFromZero) != FieldPosition[player].z)
            {
                fieldCardObj[player].transform.position += new Vector3(0, 0, 0.5f * posDir);
                yield return new WaitForFixedUpdate();
            }
            // ③カードを反転する
            float time = 0;
            float angle = 180 * Time.deltaTime;
            while (time < 1.0f)
            {
                // Time.deltaTimeを足していく。(徐々にずれていくため、小数第2位で四捨五入)
                time = (float)Math.Round(time + Time.deltaTime, 2 , MidpointRounding.AwayFromZero);

                //回転させる
                fieldCardObj[player].transform.rotation *= Quaternion.AngleAxis(angle, Vector3.up);

                //もしtimeが0.5以上の場合、表示を表面に変える
                if (time >= 0.5f)
                {
                    fieldCardObj[player].GetComponent<SpriteRenderer>().sprite =　Resources.Load<Sprite>("Images/" + fieldCardObj[player].name);
                }

                yield return new WaitForFixedUpdate();
            }
            // ④下に下ろす
            while (fieldCardObj[player].transform.position.y >= FieldPosition[player].y)
            {
                fieldCardObj[player].transform.position += new Vector3(0, -0.5f, 0);
                yield return new WaitForFixedUpdate();
            }

            // カード情報を設定
            uIController.setFieldCardInfo(player);
            uIController.displayFieldCardInfo(player, true);
        }
    }

    /// <summary>
    /// バトルフェーズの処理
    /// </summary>
    private IEnumerator BattlePhase()
    {
        // 場にカードがなければエラーを出力
        if (!fieldCardObj["player"] || !fieldCardObj["enemy"])
        {
            Debug.LogError("場にカードがありません！");
            yield return null;
        }

        // それぞれのプレイヤーカードに付与されるスクリプトを取得
        CardController playerCard = fieldCardObj["player"].GetComponent<CardController>();
        CardController enemyCard = fieldCardObj["enemy"].GetComponent<CardController>();

        // プレイヤーが勝ちパターン
        if ((playerCard.rspType == CardController.RSPType.Rock && enemyCard.rspType == CardController.RSPType.Scissors) ||
            (playerCard.rspType == CardController.RSPType.Scissors && enemyCard.rspType == CardController.RSPType.Paper) ||
            (playerCard.rspType == CardController.RSPType.Paper && enemyCard.rspType == CardController.RSPType.Rock))
        {
            yield return GoToDrop("enemy");
        }
        // 引き分けパターン
        else if ((playerCard.rspType == CardController.RSPType.Rock && enemyCard.rspType == CardController.RSPType.Rock) ||
                 (playerCard.rspType == CardController.RSPType.Scissors && enemyCard.rspType == CardController.RSPType.Scissors) ||
                 (playerCard.rspType == CardController.RSPType.Paper && enemyCard.rspType == CardController.RSPType.Paper))
        {
            // パワーの比較
            if (playerCard.power == enemyCard.power)
            { // 同等の場合、引き分け
                // DrawWin効果確認
                if (playerCard.battleEffect.Contains("DrawWin") && !enemyCard.battleEffect.Contains("DrawWin"))
                { // プレイヤーのみがDrawWinの場合、プレイヤーの勝ち
                    yield return GoToDrop("enemy");
                }
                else if (!playerCard.battleEffect.Contains("DrawWin") && enemyCard.battleEffect.Contains("DrawWin"))
                { // 相手プレイヤーのみがDrawWinの場合、プレイヤーの負け
                    yield return GoToDrop("player");
                }
                else
                { // お互いのカードを捨て場に送る
                    StartCoroutine(GoToDrop("player"));
                    yield return GoToDrop("enemy");
                }
            }
            else if (playerCard.power > enemyCard.power)
            { // プレイヤーが勝ち
                yield return GoToDrop("enemy");
            }
            else
            { // プレイヤーが負け
                yield return GoToDrop("player");
            }
            
        }
        // プレイヤーが負けパターン
        else if ((playerCard.rspType == CardController.RSPType.Rock && enemyCard.rspType == CardController.RSPType.Paper) ||
                 (playerCard.rspType == CardController.RSPType.Scissors && enemyCard.rspType == CardController.RSPType.Rock) ||
                 (playerCard.rspType == CardController.RSPType.Paper && enemyCard.rspType == CardController.RSPType.Scissors))
        {
            yield return GoToDrop("player");
        }
        else
        {
            Debug.LogError("Battle中に思わぬエラーです！");
        }
    }

    /// <summary>
    /// 場のカードをドロップゾーンへ送る
    /// </summary>
    /// <param name="player">プレイヤー</param>
    public IEnumerator GoToDrop(string player)
    {
        // 場にカードがなければエラーを出す。
        if (!fieldCardObj[player])
        {
            Debug.LogError("場にカードが出ていません");
            yield return null;
        }

        // ドロップゾーンへ移動するアニメーション
        // ①上に上げる
        while ((float)Math.Round(fieldCardObj[player].transform.position.y, 1, MidpointRounding.AwayFromZero) < 3)
        {
            fieldCardObj[player].transform.position += new Vector3(0, 0.1f, 0);
            yield return new WaitForFixedUpdate();
        }
        // ②ドロップゾーンに向けて移動
        float posDir = player == "player" ? 1 : -1; // プレイヤーによって移動方向が異なる
        Vector3 endPos = DropPosition[player] + new Vector3(0, 2.3f, 0); // 終着点
        while (fieldCardObj[player].transform.position != endPos)
        {
            fieldCardObj[player].transform.position += new Vector3(0.32f * posDir, 0, -0.2f * posDir);
            yield return new WaitForFixedUpdate();
        }
        // ③下に下ろす
        while (fieldCardObj[player].transform.position.y > DropPosition[player].y)
        {
            fieldCardObj[player].transform.position += new Vector3(0, -0.2f, 0);
            yield return new WaitForFixedUpdate();
        }

        // ドロップゾーンの設定
        if (DropCardList[player].Count != 0)
        {
            // カードを透明にする
            DropCardList[player][DropCardList[player].Count - 1].GetComponent<SpriteRenderer>().enabled = false;
        }
        DropCardList[player].Add(fieldCardObj[player]); // ドロップカードリストに追加
        uIController.incrementDropNum(player); // ドロップカード枚数のUIを設定

        // 場のカードを無(null)にする
        fieldCardObj[player] = null;
    }

    /// <summary>
    /// プレイヤー勝敗チェック
    /// </summary>
    /// <returns>
    /// 0: 続行
    /// 1: プレイヤー勝ち
    /// 2: プレイヤー負け
    /// 3: 引き分け
    /// </returns>
    private int CheckResult()
    {
        if (uIController.getDeckNum("player") == 0 && fieldCardObj["player"] == null
            && uIController.getDeckNum("enemy") == 0 && fieldCardObj["enemy"] == null)
        { // お互いにデッキも場のカードもない場合は引き分け
            uIController.ShowResult(3);
            return 3;
        }
        else if (uIController.getDeckNum("player") == 0 && fieldCardObj["player"] == null)
        { // プレイヤーのデッキと場にカードがない場合はプレイヤーの負け
            uIController.ShowResult(2);
            return 2;
        }
        else if (uIController.getDeckNum("enemy") == 0 && fieldCardObj["enemy"] == null)
        { // 相手プレイヤーのデッキと場にカードがない場合はプレイヤーの勝ち
            uIController.ShowResult(1);
            return 1;
        }
        else
        {
            return 0;
        }
    }

    public void ReturnTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
