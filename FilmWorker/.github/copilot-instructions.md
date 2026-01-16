# Copilot Instructions — FilmWorker (Unity / C#)

## 前提（確定事項）
- Unity ベース / C#。コードは原則 `Assets/Script/` 配下に置く。
- Asset の移動/リネームは参照切れ（Scene/Prefab/SerializeField）を起こし得るため慎重に行う。
- Unity バージョン: **Unity 6.000.0.58f1**

## 出力スタイル
- 指示や説明は簡潔に書く（冗長な書き方はしない）。

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
- ゲーム挙動の実装時は `Assets/Scenes/Stage.unity` を参照し、必要なら配線（参照フィールド等）も更新する。
- Scene内のオブジェクト構成を変更した場合は `.docs/specs.md` に追記する。

## Unityエディタでの操作が必要な場合
- `*.inputactions` の編集（Action/Binding追加削除）は、原則 **Unityの Input Actions Editor** で行う（整合性のため）。

## 仕様ドキュメント（必読）
- 仕様の正: `.docs/specs.md`  
  実装を変えるときは、該当セクションを必ず更新する。
  - 仕様変更、仕様解釈の統合（会話内で確定した解釈を含む）があった場合も、必ず `.docs/specs.md` に反映してから実装する。

## リプレイJSON（実装上の正）
- リプレイの JSON 保存/読込は `Assets/Script/System/JsonController.cs` が担当。
- 保存先は `Application.persistentDataPath/Replays/`、ファイル名は `replay{n}.json`。