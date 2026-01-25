# FilmWorker 仕様メモ（実装参照用）

このドキュメントは「仕様の正」として扱い、実装変更に合わせて更新する。

---

## 0. 重要ルール（開発制約）
- **Sound には干渉しない**（`Assets/Script/System/Sound/`、`Assets/Resources/sound_data.csv` を変更しない）
- `GameManager` はこれから実装（現時点で挙動/APIは未確定）
- Rigidbody は使わない（リプレイのズレ防止・決定論寄りの挙動を重視）

---

## 1. コンセプト / テーマ
- 昔の映画フィルム（ビデオテープ/映写機）をモチーフにした **アクション×パズル**
- プレイヤー自身のプレイを「録画」し、「編集」して、「過去の自分（ゴースト）」として活用する
- 編集行為そのものがゲーム体験になる設計

キーワード:
- （アクション）レミングス的（多数の行動/誘導・仕掛けの発想）
- ビデオテープ/フィルム（装飾/UI世界観）

---

## 2. ゲーム構成（ターン制）
本作の基本ループは **2つのターン**を繰り返す。

### 2.0 ターン進行（確認事項）
- 行動 → 編集 → 行動（ここで **リプレイが出現**し、プレイヤーは **スタート位置から**やり直す。ステージはリセット扱い）
- Stage開始（シーン開始）時に、保存済みリプレイJSON（persistentDataPath/Replays）を全消去する

### 2.1 行動ターン（Action Turn）
共通ルール:
- **Action ボタン**: 自身の行動開始 + 記憶（録画）開始を示す  
  - Action を押すまでプレイヤーは動けない
- 行動中は入力を記憶する（リプレイ生成の元データ）
- **Cut ボタン**: 映画のカット＝行動および記録の終了を示す

補足:
- `Action` と `Cut` は同じボタンを状態で使い分ける（開始待ちでは Action、行動中は Cut）。

#### 2.1.1 1度目の行動ターン
- Action ボタン → 自身の行動 → 行動の記憶 → Cut ボタン

#### 2.1.2 2度目の行動ターン
- Action ボタン → 自身の行動 → 行動の記憶 → **Replay 映像たちへの指示** → Cut ボタン
- Replay 映像たちへの指示: **停止指示のみ**（将来的に増やす可能性あり）
- ステージのゴールに到達した場合、その周回は完了し **2度目の行動ターンは発生しない**
  - 暫定実装: `Stage.unity` の `Goal`（GameObject名）に近づくとクリア扱い
    - `Goal` は Trigger コライダーを持ち、実装はコライダー重なり判定（無ければ距離判定）で検出する

### 2.2 編集ターン（Edit Turn）
- Replay 映像（録画データ）を編集するターン
- 編集項目:
  - カット（始点/終点の決定: `startTime` / `endTime`）
  - 倍速化（`speed`: 0.5〜2.0）
  - ループさせるか否か（`loop`）

#### 2.2.1 編集UI（暫定実装）
- ランタイムで簡易UIを生成し、Edit中のみ表示する（Scene配線不要）
- UI操作はマウス無しを想定し、キー/パッドで調整できる
  - Speed: `W/S`（Pad: D-Pad Up/Down）
  - Start: `A/D`（Pad: D-Pad Left/Right）
  - End: `J/L`（Pad: L/R Shoulder）
  - Loop: `Space`（Pad: South button）
  - 次の行動ターンへ: `Action`（既存のターン進行通り）

---

## 3. ステージ/ルール要素（仕様）
- ステージの形状が重要（搭載する機能から逆算して設計する）
- **時間制限はなし**（現仕様）
- 基本方針: **Replay機能を使わなければゴールにたどりつけない**ステージ設計を主とする
- 使えるフィルム（リプレイ/テープ）の本数は制限する  
  - 初期は「余力がある」状態（厳しすぎない設定）から開始
- 触れてはいけないもの（危険物/即失敗/ダメージ等：詳細未確定）
- 最初に思考ターン（開始直後に計画を立てる時間/モード：仕様詳細未確定）
- 爆弾を止める（何をどう止めるか：仕様詳細未確定）
- ショートカット（ギミック/ルート）
- 動く床（Moving Platform）
- 複数ステージを実装予定（段階的に機能を増やして難易度を上げる）

---

## 4. 入力（Input）/ コアシステム設計（Unity前提）
### 4.1 入力アーキテクチャ（実装構造として既存）
- Input System アセット: `Assets/InputSystem_Actions.inputactions`
- 入力提供インターフェース: `Assets/Script/Player/IInputProvider.cs`
- 実装:
  - 人間操作: `Assets/Script/Player/HumanInputProvider.cs`
  - リプレイ入力: `Assets/Script/Player/ReplayInputProvider.cs`
- `PlayerController` は入力元を意識せず **`IInputProvider` のみ**を参照する（方針）

### 4.2 移動システム（仕様）
- Rigidbody を使用しない
- Transform + Collider(2D) + Raycast/BoxCast 等で自前物理を組む（決定論重視）
- 目的: リプレイ再生時のズレを減らす

※ 具体実装の確定は `Assets/Script/Player/PlayerController.cs` を正とする。

### 4.3 操作割り当て（確定）
#### キーボード（マウス使用なし）
- 移動: WASD
- ジャンプ: Space
- UI決定: Space（UI InputAction: `UI/Submit`）
- 行動ターン開始: Enter（Player InputAction: `Action`）
- 行動ターン終了（カット）: Enter（Player InputAction: `Cut`）
- ゴースト停止指示: J（Player InputAction: `Stop`）
- 将来アクション追加キー（予定順）: U → H → K

#### コントローラー
- 移動: 左スティック
- ジャンプ: A
- UI決定: ZR（= Gamepad rightTrigger, UI InputAction: `UI/Submit`）
- 行動ターン開始: ZR（Player InputAction: `Action`）
- 行動ターン終了（カット）: ZR（Player InputAction: `Cut`）
- ゴースト停止指示: B（Player InputAction: `Stop`）
- 将来アクション追加ボタン（予定順）: Y → X

※ UI中は `UI` ActionMap を有効、ゲームプレイ中は `Player` ActionMap を有効にするなど、
同時有効による入力衝突を避ける運用を前提とする。

---

## 6. ゴーストキャラ（過去映像）
- Replay用プレイヤー（例: `Assets/Prefab/ReplayPlayer.prefab`）を使用する想定
- ゴースト再生は `ReplayCharacter` / `ReplayInputProvider` を中心に設計する

### 6.1 ゴーストへ行う「指示」（仕様）
- 指示可能なのは **行動ターン（2度目）**のみ
- 現時点での指示は **停止/再開（トグル）** のみ（将来的に増やす可能性あり）
- 指示入力:
  - Input System のアクション名: **`Stop`**
- 対象選択（暫定）:
  - 対象選択: **プレイヤー中心の半径（距離判定）**で範囲内にいるゴーストを候補とする
    - 暫定の指示可能半径: **`3.0f`**
    - ※「近づけば指示できる」という方式自体が **仮（暫定）**。必要に応じて UI/照準/別方式へ変更し得る
  - 候補が複数いる場合:
    - 原則: **最も近いゴースト**
    - 距離が同一（同率）の場合: **生成順が早いゴーストを優先**（先に生成されたもの）
- 指示の効果:
  - **停止**: ゴーストの移動更新だけでなく、**見た目/アニメーションも停止**する
  - **再開**: 停止中のゴーストを通常再生へ戻す（同じ `Stop` 入力でトグル）
  - ループしない場合、リプレイが終端に到達したらゴーストを破壊する（終了後に残さない）
  - 範囲内に対象がいない場合: 何もしない（no-op）
- 表示:
  - 現在の「指示対象ゴースト」を **強調表示**する
    - 3D の場合は「メッシュ複製 + 少し拡大 + 単色マテリアル（FrontCull）」でアウトライン表示する（シェーダ非依存）
  - 対象が変わったら強調表示も追従し、範囲外になったら解除する

### 6.2 ゴーストの再生開始（現行実装）
- 行動ターン開始時に、存在する全てのゴーストを「最初（編集済み startTime）」へ戻して再生し直す
- スタート位置もプレイヤー開始位置へリセットする

---

## 7. リプレイ保存（実装上の正）
- 保存/読込担当: `Assets/Script/System/JsonController.cs`
- 保存先: `Application.persistentDataPath/Replays/`
- ファイル名: `replay{n}.json`

---

## 8. Scene内オブジェクト構成（現状: Stage.unity）
※実装・配線の正は `Assets/Scenes/Stage.unity` を参照。

### 8.1 ルート（抜粋）
- `Main Camera`（Orthographic、URP追加コンポーネントあり）
- `Global Light 2D`
- `Player`（PrefabInstance: `Assets/Prefab/Player.prefab`）
- `Goal`（暫定: クリア判定用マーカー）
- `GameManager`（管理オブジェクト）
  - `ReplayManager`（`replayPrefab` は `Assets/Prefab/ReplayPlayer.prefab` が設定済み）
  - `JsonController`
  - `GameManager`
  - `TurnController`
- `GroundP` / `Ground`（地形スプライト）
- `WallP` / `Wall`（壁スプライト）

### 8.2 TurnController のScene配線（現状）
- `player` / `recorder` は参照が入っている
- `replayManager` は参照が入っている（同オブジェクトの `ReplayManager`）
- `actionAction` / `cutAction` / `stopAction` は未設定（Input Actions Editorで作成した `Player/Action` `Player/Cut` `Player/Stop` を割り当てる）
  - ただし実装側で `HumanInputProvider.actionAction` を起点に ActionMap から `Action/Cut/Stop` を探索するため、未設定でも動く設計にしている（配線は推奨）。

### 8.3 レイヤー注意（要確認）
- `PlayerController.groundLayer` は Prefab で `Layer 6(Ground) + Layer 7(Wall)` を含むようにしている
- `Stage.unity` の `Ground` は `Layer 6`、`Wall` は `Layer 7` になっている
  - 接地判定が意図どおりか、Layerを揃える/groundLayerを修正する

---

## 9. Prefab構成（抜粋）
### 9.1 Player.prefab
- Root: `Player`
  - `BoxCollider2D`
  - `HumanInputProvider`（`InputActionReference` を Inspector で参照）
  - `PlayerController`
  - `InputRecorder`
  - 子オブジェクトに見た目（silhouette）

### 9.2 ReplayPlayer.prefab
- Root: `ReplayPlayer`
  - `BoxCollider2D`
  - `PlayerController`
  - 子オブジェクトに見た目（silhouette）
  - `ReplayCharacter` は生成時に追加される（`ReplayManager` が `AddComponent` する）

---

## 10. Script構成（現状: Assets/Script）
- `Assets/Script/System/`
  - `TurnController`（ターン進行：Action/Cut、2度目以降Stop指示、編集→ゴースト生成）
  - `ReplayManager`（リプレイ生成/管理、最寄りゴースト探索、生成順）
  - `JsonController`（リプレイJSON保存/読込）
  - `GameManager`（現状はシングルトン枠のみ）
  - `FWSceneManager`（シーン遷移ユーティリティ）
  - `FWDB`（`KakoimaDB`：カウント保持）
- `Assets/Script/Player/`
  - `PlayerController`（自前2D物理＋移動）
  - `IInputProvider` / `HumanInputProvider` / `ReplayInputProvider`（入力抽象と実装）
  - `InputRecorder`（入力記録→ReplayData生成→保存）
  - `ReplayCharacter`（ゴースト再生、停止/再開、アウトライン強調）
- `Assets/Script/Class/`
  - `ReplayData`（frames + start/end/speed + loop）
- `Assets/Script/Shader/`
  - `OutlineHighlighter`（2D擬似アウトライン）