# 地方選択（AreaSelect）画面

## 概要
Home画面でスタートボタンを押した直後に表示される、利用エリア（地方ブロック）を選ぶ画面。
最初の粒度は **北海道・東北・関東・中部・近畿・中国・四国・九州** の地方ブロック単位とする。
将来的に選択した地方の中で都市/市町村を絞り込めるようにすることを想定（例: `/area/kansai`）。

## 画面・UI（対応するファイル）
- [Retimana/Components/Pages/AreaSelect.razor](../Retimana/Components/Pages/AreaSelect.razor) — マークアップ（`/area` ルート）
- [Retimana/Components/Pages/AreaSelect.razor.css](../Retimana/Components/Pages/AreaSelect.razor.css) — スコープ付きCSS

### レイアウト
- 画面全体（`100dvh`）を flex column で上から積む
- 上部: タイトル「地方を選ぶ」
- 中央: 日本地図画像（地方ブロックがパステルカラーで塗り分けられたピクセル風）
- 背景: 共通のクリーム色 `--retimana-bg`

### 地図表示（SVG + 画像）
- 元デザイン:
  - [docs/design/areaselect/japanmap.png](design/areaselect/japanmap.png) — 背景込みの原案（クリーム+波模様）
  - [docs/design/areaselect/japanmap_transparent.png](design/areaselect/japanmap_transparent.png) — 背景透過版（採用）
- 採用アセット: [Retimana/wwwroot/images/japanmap.png](../Retimana/wwwroot/images/japanmap.png)（1536×1024, RGBA透過）
  - 透過版を採用することで、ページの背景（クリーム/ダーク）が地図の外側で素直に出る
- 構造:
  - `<svg viewBox="0 0 1536 1024" preserveAspectRatio="xMidYMid meet">`
  - 中に `<image>` でPNGを埋め込み
  - その上に `<g class="area-labels">` 空グループ（後で各地方名テキストを画像座標で配置）
- viewBoxを画像の実ピクセル数と一致させているため、地方名テキストを置く際に画像上のピクセル位置を直接座標として指定できる
- 配色（パステル）:
  - 北海道: セージグリーン
  - 東北: セージグリーン
  - 関東: ライトブルー
  - 中部: ライトブルー / ラベンダー
  - 近畿: ライトラベンダー
  - 中国: ライトイエロー
  - 四国: ライトグリーン
  - 九州: ピーチオレンジ
- 海部分にHomeと同様の波模様・ドット質感

## ソース構成（ファイル一覧と役割）
| ファイル | 役割 |
|----------|------|
| [Components/Pages/AreaSelect.razor](../Retimana/Components/Pages/AreaSelect.razor) | `/area` ルートのページコンポーネント |
| [Components/Pages/AreaSelect.razor.css](../Retimana/Components/Pages/AreaSelect.razor.css) | スコープ付きCSS（レイアウト・タイトル・地図画像のサイズ） |
| [wwwroot/images/japanmap.png](../Retimana/wwwroot/images/japanmap.png) | 日本地図画像 |

## 主要な関数・メソッド（引数・戻り値・役割）
現時点ではC#ロジックなし（静的UIのみ）。地方クリックでの遷移処理は未実装。

## データモデル
なし（将来的に「地方コード」「都市リスト」を導入予定）。

## 外部依存
なし。

## 今後の拡張
- 各地方ブロックをクリック可能にする（SVG `<area>` または SVG `<polygon>` オーバーレイで領域判定）
- 選択時のホバー/フォーカス状態（地方を少し明るくする等）
- 多言語対応（英語/中国語/韓国語ラベル）
- 戻るボタン（Home画面へ）
- フェーズ1のMVPでは京都のみ対応のため、近畿以外は disabled 表示にすることを検討

## 更新履歴
- 2026-05-20: 初版作成（タイトル + 日本地図画像表示のみ、Home画面のスタートボタンから遷移）
- 2026-05-20: 地図を `<img>` から `<svg>` + `<image>` 構造に変更。viewBoxを画像の実ピクセルサイズ(1536×1024)に合わせ、地方名テキストや反応領域を画像座標で重ねられる土台にした
- 2026-05-20: 地図画像を背景透過版（japanmap_transparent.png）に差し替え。ページ背景がそのまま地図の周囲に見えるようにした
- 2026-05-20: タイトルと地図alt textを多言語化（`IStringLocalizer<SharedResources>` 経由、`AreaSelect.Title` / `AreaSelect.MapAlt`）。詳細は [docs/i18n.md](i18n.md) 参照
