# Home画面

## 概要
アプリ起動時の入口画面。中央に「スタート」ボタンを置き、その背後に地図画像（クリーム/ドット系の街並み）を敷くことで、観光客が「これからバスで街を移動する」イメージを直感的に持てる入口画面とする。
[retimana_concept.md](../retimana_concept.md) の「UI/UX設計方針（スマホファースト・シンプル）」に従い、操作要素はスタートボタン1つだけに絞る。

地図はデザイン画像をそのまま使用し、その上に **SVGオーバーレイ層** を載せる構成にすることで、後からバスアイコンをCSS `offset-path` や `<animateMotion>` で道路に沿って動かせる。

## 画面・UI（対応するファイル）
- [Retimana/Components/Pages/Home.razor](../../src/Retimana.Web/Components/Pages/Home.razor) — マークアップ（`/` ルート）
- [Retimana/Components/Pages/Home.razor.css](../../src/Retimana.Web/Components/Pages/Home.razor.css) — スコープ付きCSS（中央配置・ボタン装飾）
- [Retimana/Components/MapBackground.razor](../../src/Retimana.Web/Components/MapBackground.razor) — 地図画像 + バスアニメーション用SVG
- [Retimana/Components/MapBackground.razor.css](../../src/Retimana.Web/Components/MapBackground.razor.css) — SVG配置・ダークモード時の画像フィルタ

### レイアウト
- `.home` を `position: relative` + `min-height: 100dvh` の flex 中央寄せ（縦方向に積む）
- `MapBackground` を `position: absolute / inset: 0` で全面に敷く（`z-index: 0`）
- `.home-content` ラッパー（`z-index: 1`）に **タイトル + スタートボタン** を縦に積み、間に `gap: clamp(2.5rem, 8vh, 4.5rem)` の余白
- ラッパー全体が画面中央に来るため、タイトルが中央より上・スタートボタンが中央より下に自然配置される
- `.language-switcher`（`z-index: 1`）は画面下部に `position: absolute; bottom: 1.5rem; left: 50%; transform: translateX(-50%)` で配置

### 言語切替プルダウン
- マークアップ: `<form method="get" action="/set-language">` の中に `<select>` を配置（日本語/English/中文/한국어 の4言語）
- 動作: `onchange="this.form.submit()"` で GET 送信 → サーバーが `.AspNetCore.Culture` クッキーをセット → `returnUrl` にリダイレクト
- Blazor インタラクティビティ不要（静的SSRで動く）
- スタイル: 半透明クリーム背景 + `backdrop-filter: blur(4px)` で地図背景越しに馴染ませる
- ダークモード: 半透明ダーク背景 + 明るい文字色に切替
- 多言語対応の全体構成は [i18n.md](i18n.md) を参照

### タイトル「Retimana」（ロゴSVG）
- 元アセット: [docs/design/home/logo/retimana_logo.svg](../design/home/logo/retimana_logo.svg)（ピン型アイコン + Georgia serif の "Retimana" ワードマーク、暖色ブラウン）
- 採用パス: [Retimana/wwwroot/images/retimana_logo.svg](../../src/Retimana.Web/wwwroot/images/retimana_logo.svg)
- セマンティクス: `<h1>` 内に `<img alt="Retimana">` を入れる（SEOとアクセシビリティを両立）
- サイズ: `width: clamp(15rem, 60vw, 22rem)` で画面幅に応じて伸縮、`height: auto` でアスペクト維持
- ダークモード: `filter: invert(1) hue-rotate(180deg) brightness(0.92)` で暗色用に色反転（明度反転 + 色相戻しで暖色を保持）
- 別バージョンに [retimana_logo_with_tagline.svg](../design/home/logo/retimana_logo_with_tagline.svg)（"Realis · Tempus · Mappa · Navigare" 入り）あり。必要なら差し替え可能

### 背景（地図画像 + SVGレイヤー）
- 採用方式: SVG内に `<image>` で地図PNGを埋め込み、その上に `<g class="map-buses">` レイヤーを置く
- 地図画像: [Retimana/wwwroot/images/home-bg.png](../../src/Retimana.Web/wwwroot/images/home-bg.png)（元: [docs/design/home/home.png](../design/home/home.png) のコピー）
- viewBox: `0 0 384 256`、`preserveAspectRatio="xMidYMid slice"` で画面全体を覆う（cover相当）
- 採用理由:
  - デザイン画像の品質をそのまま保てる（手書きSVGでは再現困難）
  - 画像とアニメーションが同一座標系（viewBox）に乗るので、道路に沿った動きを正確に設計できる
  - 画像は静的アセットとして Service Worker でキャッシュ可能、Leafletタイル取得のような通信コストはない

### ダークモード
- 地図画像（`.map-image`）に CSS `filter: brightness(0.3) saturate(0.55) hue-rotate(-10deg)` を適用して暗色化
- 別画像を用意せず一枚で両モードに対応

### ボタンスタイル
画像内のスタートボタン（旧版）に合わせた暖色グレーの角丸長方形。

| 要素 | ライト | ダーク |
|------|--------|--------|
| 背景色 | `#8B7D6E`（暖色系グレー） | `#C8BBA8`（明るい暖色グレー） |
| 文字色 | `#EDE5D5`（クリーム） | `#2A2622` |
| 角丸 | `10px` | `10px` |
| 影 | `0 4px 10px rgba(60,50,40,0.18)` | 同上 |

### 全体カラートークン
ライトモード／ダークモードを `prefers-color-scheme` で切り替える。色トークンは [Retimana/wwwroot/app.css](../../src/Retimana.Web/wwwroot/app.css) の `:root` に集約。

| トークン | ライト | ダーク | 用途 |
|----------|--------|--------|------|
| `--retimana-bg` | `#F5F0E8`（クリーム） | `#2A2622`（暖色系ダーク） | 画面背景 |
| `--retimana-text` | `#5F5850` | `#E8E0D0` | 本文テキスト |
| `--retimana-accent` | `#5F5850` | `#E8E0D0` | 共通アクセント（他画面で使用） |
| `--retimana-accent-text` | `#F5F0E8` | `#2A2622` | アクセント上の文字色 |

## ソース構成（ファイル一覧と役割）
| ファイル | 役割 |
|----------|------|
| [Components/Pages/Home.razor](../../src/Retimana.Web/Components/Pages/Home.razor) | `/` ルートのページコンポーネント |
| [Components/Pages/Home.razor.css](../../src/Retimana.Web/Components/Pages/Home.razor.css) | Home画面専用のスコープ付きCSS（ボタン中心配置、ボタンスタイル） |
| [Components/MapBackground.razor](../../src/Retimana.Web/Components/MapBackground.razor) | 地図画像とバスアニメーション用SVGレイヤー |
| [Components/MapBackground.razor.css](../../src/Retimana.Web/Components/MapBackground.razor.css) | SVG配置とダークモード時の画像フィルタ |
| [Components/App.razor](../../src/Retimana.Web/Components/App.razor) | `<html lang="ja">`、`color-scheme` メタタグを設定 |
| [wwwroot/app.css](../../src/Retimana.Web/wwwroot/app.css) | 全体のカラートークン定義、`body` 背景・フォント |
| [wwwroot/images/home-bg.png](../../src/Retimana.Web/wwwroot/images/home-bg.png) | 地図背景画像 |
| [wwwroot/images/retimana_logo.svg](../../src/Retimana.Web/wwwroot/images/retimana_logo.svg) | アプリ名ロゴ |

## 主要な関数・メソッド（引数・戻り値・役割）
現時点ではC#ロジックなし（静的UIのみ）。スタートボタンのクリック時遷移先は未定。

## データモデル
なし。

## 外部依存
なし。

## 今後の拡張（バスアニメーション）
- 地図画像の道路位置に合わせて、`map-buses` レイヤー内に非表示の `<path id="route-1">` を定義
- バスアイコン `<g class="bus">` を作り、CSS `offset-path: path('...')` または SVG `<animateMotion>` で道路に沿ってループ走行
- 複数路線で同時走行・速度違い・夜は本数を減らすなどの演出を、すべて同一座標系で実装可能
- viewBox が画像と一致しているため、画像上の任意の地点をピクセル単位で指定できる

## 更新履歴
- 2026-05-19: 初版作成（スタートボタンのみのシンプル構成、ナイトモード対応）
- 2026-05-19: 地図風背景画像（home-bg.png）を追加、ボタンスタイルをデザインに合わせて調整
- 2026-05-19: 一度SVGで地図全体を手書き再現を試みたが画像品質に届かず破棄。地図は画像、SVGはバスアニメーション用オーバーレイ層に分離する構成に変更
- 2026-05-19: アプリ名「Retimana」を中央上に追加、スタートボタンを中央下に配置（flexの縦並びで自然に上下分割）
- 2026-05-20: 画面下に言語切替プルダウン（日本語/English/中文/한국어）を設置（UIのみ、切替ロジックは未配線）
- 2026-05-20: スタートボタンを `<button>` から `<a href="area">` に変更し、`/area`（地方選択画面）へ遷移するように配線
- 2026-05-20: 多言語対応を導入。表示テキストを `IStringLocalizer<SharedResources>` 経由に変更、言語切替プルダウンを `/set-language` エンドポイントに配線して実動作させた
