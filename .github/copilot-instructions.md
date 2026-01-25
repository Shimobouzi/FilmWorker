# Copilot Instructions — FilmWorker (Unity / C#)

## 前提（確定事項）
- Unity ベース / C#。コードは原則 `Assets/Script/` 配下に置く。
- このリポジトリでは Unity プロジェクトは `FilmWorker/` 配下（`FilmWorker/Assets/...`）にある。以降の `Assets/...` 表記は **Unityプロジェクトルート基準**。
- Asset の移動/リネームは参照切れ（Scene/Prefab/SerializeField）を起こし得るため慎重に行う。
- Unity バージョン: **Unity 6.000.0.58f1**

## 出力スタイル
- 指示や説明は簡潔に書く（冗長な書き方はしない）。

## 開発ルール（追記・優先）
- 開発は Unity 3D（Unityエディタ操作が必要な変更は、その前提で提案する）。
- **`.unity` ファイルは積極的に参照して情報を得るが、編集はしない**（必要な配線変更がある場合は、変更手順だけ提示する）。
- ほかのスクリプトで共通利用しそうなカウント/状態/変数は `Assets/Script/System/FWDB.cs` 側に集約する方針。
  - 現状 `FWDB.cs` のクラス名は `FWDB` で、`DontDestroyOnLoad` のシングルトンとしてカウントを保持している。
- シーン遷移は当面 **`FWSceneManager` を使用する**（`Assets/Script/System/FWSceneManager.cs`）。
  - 現状 `MenuManager.cs` は `FWSceneManager.Instance.LoadTeamSelectScene()` を呼ぶが、`FWSceneManager` 側に同メソッドは未実装のため、実装するか呼び出し先を既存メソッドへ合わせる。
- `GameManager` は「ステージ進行に関する機能」を持つ。複数シーンにまたがって使いそうな機能を置く想定（APIを断定せず、まず前提確認）。
- `MenuManager.cs` はメニュー画面のUI管理（タイトル/オプションのパネル切替、選択状態、開始/終了等）。

## フォルダ責務（現状の構造）
- `Assets/Script/System/`：ゲーム基盤（例：Scene/DB/JSON/Replay）
- `Assets/Script/Player/`：プレイヤー制御・入力・リプレイ入力/記録
- `Assets/Script/Class/`：データクラス（例：`ReplayData`）
- `Assets/Script/System/Sound/`：サウンド（※後述の通り変更禁止）

## 重要なプロジェクト固有ルール
- **Sound には干渉しないこと**  
  `Assets/Script/System/Sound/` および `Assets/Resources/sound_data.csv` に関する変更・リファクタ・新規追加をしない。
- `GameManager` はこれから実装するため、既存の設計や API を断定しない。提案が必要な場合は「前提確認」を優先する。
- リプレイ記録は **`InputRecorder` → `ReplayData` → JSON 保存** が前提。  
  保存形式/フィールド変更は後方互換（既存の保存データ）に影響する可能性があるため、変更時は仕様書も更新する。

## 実装前の調査（このリポでは必須）
変更を入れる前に、少なくとも以下を確認してから実装すること：
- 仕様: `.docs/specs.md` の該当セクション（ターン制、入力割当、リプレイ編集/停止指示）
- 関連コード: `Assets/Script/Player/`, `Assets/Script/System/` の該当クラス（既存責務を崩さない）
- 既存のPrefab/Scene参照（参照切れを起こす変更は避け、必要なら移行手順も提示）
- **十分に調査してから実装すること**（関連クラス/入力/Prefab/Sceneの影響範囲を把握し、変更理由と影響点を明確にしてから着手）

## Scene参照（必須）
- Scene内のオブジェクト構成を変更した場合は `.docs/specs.md` に追記する。
  - 現状リポ内にあるSceneは `Main1.unity` と `SampleScene.unity`。

## Unityエディタでの操作が必要な場合
- `*.inputactions` の編集（Action/Binding追加削除）は、原則 **Unityの Input Actions Editor** で行う（整合性のため）。

## 仕様ドキュメント（必読）
- 仕様の正: `.docs/specs.md`  
  実装を変えるときは、該当セクションを必ず更新する。
  - 仕様変更、仕様解釈の統合（会話内で確定した解釈を含む）があった場合も、必ず `.docs/specs.md` に反映してから実装する。

## リプレイJSON（仕様上の前提 / これから実装）
- `.docs/specs.md` では `InputRecorder` / `JsonController` を前提としているが、現状このリポには未実装。
- 実装する場合の保存先は `Application.persistentDataPath/Replays/`、ファイル名は `replay{n}.json`（仕様）。