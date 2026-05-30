# GTFS-JP データ取り込み（Retimana.Functions.Gtfs）

ODPT APIからGTFS-JP ZIPを週次でダウンロードし、解凍・CSVパース・CosmosDB取り込みまでを行うAzure Function。

## 概要

- **プロジェクト**: [src/Retimana.Functions.Gtfs/](../../src/Retimana.Functions.Gtfs/)
- **トリガー**: TimerTrigger（既定: 毎週日曜 03:00 JST = 土曜 18:00 UTC）
- **データソース**: ODPT API（[architecture/odpt-api.md](../architecture/odpt-api.md) 参照）
- **保存先**: Azure CosmosDB（コンテナ `bus-data`、パーティションキー `/cityCode`）
- **対象事業者**: 京都市営バス + 京都バス株式会社

## アーキテクチャ

```
┌───────────────────────────────────┐
│ Azure Function App (Timer Trigger)│
│ Retimana.Functions.Gtfs           │
└──────────────┬────────────────────┘
               │ 週次起動
               ▼
   ┌───────────────────────┐
   │ GtfsIngestionPipeline │
   └─────────┬─────────────┘
             │
   ┌─────────┴──────────────┐
   │ for each Gtfs.Source   │
   │  (京都市営/京都バス)   │
   ▼                         ▼
1. GtfsDownloader           ──► ODPT File API (HTTPS)
   ZIP DL → /tmp/...zip
2. GtfsExtractor
   ZIP → /tmp/.../*.txt
3. GtfsCsvParser (streaming)
   CSV → GTFS POCO
4. Map to BusDataDocument
5. CosmosBulkImporter         ──► Azure CosmosDB
   バルク upsert
```

## プロジェクト構成

```
src/Retimana.Functions.Gtfs/
├── Configuration/
│   ├── GtfsSettings.cs       # データソース一覧
│   ├── OdptSettings.cs       # APIキー
│   └── CosmosSettings.cs     # Cosmos接続情報
├── Models/
│   ├── Gtfs/                 # CSV 1行 = 1 POCO（CsvHelperで読む）
│   │   ├── GtfsStop.cs
│   │   ├── GtfsRoute.cs
│   │   ├── GtfsTrip.cs
│   │   ├── GtfsStopTime.cs
│   │   └── GtfsCalendar.cs
│   └── Cosmos/               # Cosmos documents
│       ├── BusDataDocument.cs    # 基底
│       ├── StopDocument.cs       # 地理空間検索対応
│       ├── RouteDocument.cs
│       ├── TripDocument.cs
│       ├── StopTimeDocument.cs
│       └── CalendarDocument.cs
├── Services/
│   ├── GtfsDownloader.cs     # ODPT ZIP取得
│   ├── GtfsExtractor.cs      # ZIP解凍
│   ├── GtfsCsvParser.cs      # CSV ストリーミング読込
│   ├── CosmosBulkImporter.cs # Cosmos バルクupsert
│   └── GtfsIngestionPipeline.cs  # 全体オーケストレーション
├── Functions/
│   └── GtfsWeeklyUpdateFunction.cs  # TimerTrigger エントリーポイント
├── Program.cs                # DI設定
├── host.json                 # Functions ランタイム設定
└── local.settings.json       # ローカル開発設定（gitignore）
```

## 設定

### local.settings.json（ローカル開発用、gitignore）

```json
{
  "Values": {
    "Odpt:ConsumerKey": "YOUR_KEY",
    "Cosmos:Endpoint": "https://localhost:8081",
    "Cosmos:Key": "EMULATOR_KEY",
    "Cosmos:DatabaseName": "retimana",
    "Cosmos:ContainerName": "bus-data",
    "Gtfs:DownloadIntervalCron": "0 0 18 * * SUN"
  }
}
```

### Azure App Settings（本番）

`Gtfs:Sources` は **配列なので環境変数の `:` 区切り表記**:

```
Odpt__ConsumerKey                = <secret>
Cosmos__Endpoint                 = https://<account>.documents.azure.com:443/
Cosmos__Key                      = <secret>
Cosmos__DatabaseName             = retimana
Cosmos__ContainerName            = bus-data
Gtfs__DownloadIntervalCron       = 0 0 18 * * SUN
Gtfs__Sources__0__OperatorId     = kyoto-city-bus
Gtfs__Sources__0__CityCode       = kyoto
Gtfs__Sources__0__DisplayName    = 京都市交通局
Gtfs__Sources__0__ZipUrlTemplate = https://api.odpt.org/api/v4/files/odpt/KyotoMunicipalTransportation/Kyoto_City_Bus_GTFS.zip?date={date}&acl:consumerKey={apiKey}
Gtfs__Sources__0__DataDate       = 20260518
Gtfs__Sources__1__OperatorId     = kyoto-bus
Gtfs__Sources__1__CityCode       = kyoto
Gtfs__Sources__1__DisplayName    = 京都バス株式会社
Gtfs__Sources__1__ZipUrlTemplate = https://api.odpt.org/api/v4/files/odpt/KyotoBus/AllLines.zip?date={date}&acl:consumerKey={apiKey}
Gtfs__Sources__1__DataDate       = 20260516
```

### CosmosDB

- **アカウント**: Azure Cosmos DB アカウント（NoSQL API）。MVPは無料枠（1000 RU/s + 25GB）
- **データベース**: `retimana`（自動作成）
- **コンテナ**: `bus-data`（自動作成）
  - **パーティションキー**: `/cityCode`（例: `kyoto`）
  - **地理空間インデックス**: `/location/*` (GeoJSON Point)

ローカル開発: [Azure Cosmos DB Emulator](https://learn.microsoft.com/azure/cosmos-db/local-emulator) を使う。固定 Endpoint/Key で開発時は接続。

## CosmosDB ドキュメント設計

### 共通フィールド（BusDataDocument）

| フィールド | 説明 |
|-----------|------|
| `id` | `{type}:{operatorId}:{gtfsId}` 例: `stop:kyoto-city-bus:003310` |
| `cityCode` | パーティションキー。例: `kyoto` |
| `type` | `stop` / `route` / `trip` / `stopTime` / `calendar` |
| `operatorId` | `kyoto-city-bus` または `kyoto-bus` |
| `dataDate` | 元GTFSデータ公開日（YYYYMMDD） |
| `ingestedAt` | 取り込みUTC時刻 |

### StopDocument（バス停）

| フィールド | 説明 |
|-----------|------|
| `stopId` | GTFS stop_id |
| `stopName` | バス停名 |
| `latitude` / `longitude` | 緯度経度 |
| `platformCode` | のりば番号（A/B/C/D等） |
| `location` | **GeoJSON Point** （地理空間インデックス対象） |
| その他 | stop_code, stop_desc, zone_id, location_type, parent_station, wheelchair_boarding |

### RouteDocument / TripDocument / StopTimeDocument / CalendarDocument

GTFS仕様の対応フィールド + 共通フィールド。詳細は各 `.cs` 参照。

## データフロー詳細

### 1. ダウンロード（GtfsDownloader）
- URLテンプレートに `{date}` と `{apiKey}` を文字列置換
- ⚠️ **`acl:consumerKey` のコロンは絶対にURLエンコードしない**（API拒否）
- HttpClient で ZIP を `/tmp/retimana-gtfs/{operator}/{operator}-{date}.zip` に保存

### 2. 解凍（GtfsExtractor）
- `ZipFile.ExtractToDirectory` で `.zip` と同階層に展開
- 既存解凍フォルダがあれば一旦削除（クリーンな状態）

### 3. CSV パース（GtfsCsvParser）
- **ストリーミング読み**（`yield return`）でメモリ過剰消費を回避
- stop_times.txt は65万行クラス → メモリに全行載せると数百MB
- CsvHelper の `GetRecords<T>()` で1行ずつ取り出し → そのまま LINQ で `Select` して Cosmos へ

### 4. CosmosDB バルク upsert（CosmosBulkImporter）
- `CosmosClientOptions.AllowBulkExecution = true` でバルクモード有効化
- 100件単位で `Task.WhenAll` → バックプレッシャ管理
- 個別失敗はログ出力して続行（部分的にでも更新を進める）

### 5. エラーハンドリング
- 1事業者分が失敗しても他の事業者は続行
- Cosmos接続不可・APIキー不正は全停止（Function実行失敗）
- ZIP内CSV不正は CsvHelper の `BadDataFound = null` で行スキップ

## 公開日（DataDate）の運用

ODPT は GTFS データを **特定日付** で公開する仕組み。最新日を自動取得する仕様は明示されておらず、当面は `appsettings.json` の `DataDate` を手動で更新する運用。

- 公開頻度: 月1回〜不定期
- 更新方法:
  1. https://ckan.odpt.org/ で京都市営バス・京都バスのデータセットを確認
  2. 新しい日付を `appsettings.json` の `Gtfs:Sources:N:DataDate` に反映
  3. Azure App Settings 更新 → Function App 再起動（または次のTimer起動で反映）

将来的に「最新公開日を取得する仕組み」が判明したら自動化したい。

## 動作確認手順（ローカル）

1. **Azure Cosmos DB Emulator 起動**
   - Windows: スタートメニューから起動、または Docker Desktop の linux イメージ
   - 起動確認: https://localhost:8081/_explorer/index.html

2. **ODPT API キー取得**
   - https://developer.odpt.org/ で開発者登録
   - 取得した key を `local.settings.json` の `Odpt:ConsumerKey` に設定

3. **GTFS データソース設定**
   - `local.settings.json` に環境変数形式で `Gtfs__Sources__0__*` を追加
   - またはローカル用 `appsettings.json` を作成（gitignore）

4. **Function 起動**
   ```bash
   cd src/Retimana.Functions.Gtfs
   func start
   ```

5. **手動トリガー**（Timer の発火を待たずにテスト）
   ```bash
   curl -X POST http://localhost:7071/admin/functions/GtfsWeeklyUpdateFunction \
     -H "Content-Type: application/json" -d "{}"
   ```

6. **Cosmos Emulator で確認**
   - DataExplorer で `retimana > bus-data` を開いて取り込まれたドキュメントを確認

## 今後の拡張

- **GTFS-RT ポーリング**: 別プロジェクト [Retimana.Functions.Realtime](../../src/Retimana.Functions.Realtime/) で実装
- **増分更新**: 現状は全件 upsert。差分検出して変更分のみupsertすれば高速化可能
- **shapes.txt 取り込み**: 路線形状（地図上の経路線描画用）。MVPでは省略
- **translations.txt 取り込み**: 多言語対応バス停名。i18n対応強化時に
- **データバージョン管理**: 古い `dataDate` のドキュメントを世代管理 + TTLで自動削除

## 更新履歴

- 2026-05-30: 初版作成（フォルダ構成 src/ 移行、Functions.Gtfs/Realtime 2プロジェクト分離、Timer + ZIP DL → CSV → Cosmos パイプライン実装）
