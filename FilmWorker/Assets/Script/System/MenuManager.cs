using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// タイトル画面のUIパネルおよびボタンの処理を管理するクラス
/// </summary>
public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _titlePanel;

    [SerializeField]
    private GameObject _optionPanel;

    private void Start()
    {
        // 初期状態ではタイトルパネルを表示、オプションパネルを非表示にする
        SetPanelVisibility(isTitleVisible: true, isOptionVisible: false);
        EventSystem.current.SetSelectedGameObject(null);
        _titlePanel.transform.Find("Start").GetComponent<Button>().Select();
    }

    /// <summary>
    /// スタートボタンが押された時に呼ばれる
    /// チーム選択シーンへ遷移する
    /// </summary>
    public void OnStart()
    {
        FBSceneManager.Instance.LoadTeamSelectScene();
    }

    /// <summary>
    /// オプションボタンが押された時に呼ばれる
    /// オプションパネルを表示し、タイトルパネルを非表示にする
    /// </summary>
    public void OnOption()
    {
        SetPanelVisibility(isTitleVisible: false, isOptionVisible: true);
        EventSystem.current.SetSelectedGameObject(null);
        _optionPanel.transform.Find("Back").GetComponent<Button>().Select();
    }

    /// <summary>
    /// 戻るボタンが押された時に呼ばれる
    /// タイトルパネルを表示し、オプションパネルを非表示にする
    /// </summary>
    public void OnBack()
    {
        SetPanelVisibility(isTitleVisible: true, isOptionVisible: false);
        EventSystem.current.SetSelectedGameObject(null);
        _titlePanel.transform.Find("Start").GetComponent<Button>().Select();

    }

    /// <summary>
    /// 終了ボタンが押された時に呼ばれる
    /// エディタ上では再生を停止、ビルド環境ではアプリケーションを終了する
    /// </summary>
    public void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// パネルの表示・非表示を切り替える共通処理
    /// </summary>
    /// <param name="isTitleVisible">タイトルパネルを表示するかどうか</param>
    /// <param name="isOptionVisible">オプションパネルを表示するかどうか</param>
    private void SetPanelVisibility(bool isTitleVisible, bool isOptionVisible)
    {
        _titlePanel.SetActive(isTitleVisible);
        _optionPanel.SetActive(isOptionVisible);
    }
}
