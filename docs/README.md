# Retimana 設計ドキュメント索引

Retimana プロジェクトの設計書・技術ドキュメントは以下に整理されています。
新規ドキュメントは適切なフォルダ配下に追加してください。

## ディレクトリ構成

```
docs/
├── README.md                       # 本ファイル（索引）
├── retimana_concept.md             # ブランドコンセプト
│
├── features/                       # 画面・機能の設計書
│   ├── home.md
│   ├── area-select.md
│   ├── i18n.md
│   └── gtfs-ingestion.md
│
├── architecture/                   # 横断的な技術設計
│   ├── retimana_claudecode.md      # 技術スタック・全体像
│   ├── retimana_map_design.md      # Leaflet マップ設計
│   └── odpt-api.md                 # ODPT API 利用ガイド
│
├── design/                         # ビジュアルデザイン素材
│   ├── home/                       # Home画面の地図・ロゴ
│   └── areaselect/                 # 地方選択画面の地図
│
└── odpt/                           # ODPT 外部資料（取得物）
    ├── api/api_documents.md        # 公式API仕様の完全コピー
    └── Sample/                     # 京都市営バス・京都バスのGTFS-JP ZIP
```

## トップレベル

| ファイル | 内容 |
|---------|------|
| [retimana_concept.md](retimana_concept.md) | ブランドコンセプト・名前の由来・サービス特徴・UI/UX設計方針・キャッチコピー・ロードマップ |

## 画面・機能（features/）

各画面・機能ごとの設計書。実装ファイル（`src/Retimana.Web/...`）への参照を含む。

| ファイル | 内容 |
|---------|------|
| [features/home.md](features/home.md) | Home画面（タイトル + スタートボタン + 言語切替 + 地図風背景） |
| [features/area-select.md](features/area-select.md) | 地方選択画面（日本地図 + 47都道府県リスト） |
| [features/i18n.md](features/i18n.md) | 多言語対応（IStringLocalizer + .resx、言語別フォント切替、4言語対応） |
| [features/gtfs-ingestion.md](features/gtfs-ingestion.md) | GTFS-JP データの週次取り込み（Azure Function Timer Trigger） |

## 横断的な技術設計（architecture/）

複数機能・将来も影響する技術選定・統合設計。

| ファイル | 内容 |
|---------|------|
| [architecture/retimana_claudecode.md](architecture/retimana_claudecode.md) | 技術スタック全体・コアロジック・MVP スコープの引き継ぎ書 |
| [architecture/retimana_map_design.md](architecture/retimana_map_design.md) | Leaflet + Stamen Toner Lite + クリームフィルタの本番マップ設計 |
| [architecture/odpt-api.md](architecture/odpt-api.md) | ODPT API 利用ガイド（認証・エンドポイント・バス関連RDFタイプ・コード例） |

## デザイン素材（design/）

| パス | 内容 |
|------|------|
| [design/home/](design/home/) | Home画面の地図風背景PNG + ロゴSVG |
| [design/areaselect/](design/areaselect/) | 地方選択画面の日本地図PNG・京都エリア画像 |

## 外部資料（odpt/）

ODPT から取得した一次資料の保管場所。
GitHub にコミットしているのは ZIP のみ（解凍済みフォルダは `.gitignore`）。

| パス | 内容 |
|------|------|
| [odpt/api/api_documents.md](odpt/api/api_documents.md) | ODPT API 公式仕様（version 4.15）の完全コピー |
| [odpt/Sample/](odpt/Sample/) | 京都市営バス・京都バス株式会社のGTFS-JPサンプルZIP |

## 設計書のひな型

各ファイルには以下を含めることが推奨されます（CLAUDE.md の規約）:
- 概要
- 画面・UI（対応するファイル）
- ソース構成（ファイル一覧と役割）
- 主要な関数・メソッド（引数・戻り値・役割）
- データモデル
- 外部依存
- 更新履歴

## ドキュメント間の参照

- 同フォルダ内: 相対パスで直接（例: `[i18n.md](i18n.md)`）
- 別フォルダ: `../` で上に出てから（例: features → architecture: `(../architecture/odpt-api.md)`）
- ソースコード参照: `../../src/Retimana.Web/...` 形式
