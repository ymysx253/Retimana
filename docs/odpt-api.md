# ODPT API 利用ガイド

公共交通オープンデータセンター（ODPT）API を Retimana から使うための要点まとめ。
完全な仕様は [docs/odpt/api/api_documents.md](odpt/api/api_documents.md)（fetched from developer.odpt.org/documents, version 4.15）を参照。

## 概要
- 公共交通オープンデータ協議会が提供する RESTful API
- 鉄道・バス・航空機・船・自転車シェアの公開データを統一的に取得
- 全レスポンスは **JSON-LD** 形式（通常の JSON として扱える、Linked Data としても扱える）
- データ形式は **RDF ベースのトリプル**（主語・述語・目的語）
- バージョン: v4.15（2025-11-28 時点）

## 認証

### API キー取得
1. https://developer.odpt.org/ で開発者登録
2. アクセストークン (Primary / Secondary Consumer Key) が発行される

### 認証方法
すべてのリクエストにクエリパラメータ `acl:consumerKey` を付与する。
```
?acl:consumerKey=YOUR_PRIMARY_OR_SECONDARY_KEY
```

### Retimana での管理
**`appsettings.json` には書かない**（コミット時に漏れる）。`dotnet user-secrets` を使う:

```bash
cd Retimana
dotnet user-secrets init
dotnet user-secrets set "Odpt:ConsumerKey" "YOUR_KEY"
```

本番では環境変数 / Azure Key Vault / シークレットマネージャから供給。

### ⚠️ ハマりどころ: `:` のURLエンコード
ASP.NET の `QueryHelpers.AddQueryString` や `HttpClient` のクエリビルダーは `acl:consumerKey` の `:` を `%3A` にエスケープすることがあり、API が拒否する可能性がある。**URL は文字列補間で組み立てる**こと:

```csharp
// ❌ 危険（コロンがエスケープされる可能性）
var url = QueryHelpers.AddQueryString(baseUrl, "acl:consumerKey", key);

// ✅ 推奨
var url = $"{baseUrl}?acl:consumerKey={key}";
```

## エンドポイント

### ベース URL
- 通常 API: `https://api.odpt.org/api/v4/`
- File API: `https://api.odpt.org/api/v4/files/`
- ⚠️ データセットによっては別ホストの場合あり（仕様書 1.3.1 留意点）

### 4種類の API パターン

| API | パス | 用途 |
|-----|------|------|
| **データ検索API** | `/v4/{rdf_type}?...` | 種別+フィルタで検索（リアルタイム情報など） |
| **データダンプAPI** | `/v4/{rdf_type}.json?...` | 種別の全データを一括取得（駅一覧など、HTTP 301 リダイレクトされる） |
| **データ取得API** | `/v4/datapoints/{data_uri}?...` | 特定 IDのデータ1件を取得 |
| **地物情報検索API** | `/v4/places/{rdf_type}?...` | 緯度・経度・半径で空間検索（**最寄りバス停の取得に有用**） |

### HTTPステータスコード
- `200` 正常
- `400` パラメータ不正
- `401` `acl:consumerKey` 誤り
- `402` 課金が必要（一部 datapoints API）
- `403` 権限なし
- `404` 該当データなし
- `500` サーバ内部エラー
- `503` サービス利用不可

### 出力上限
1リクエストの返却件数には上限がある（[FAQ 参照](https://developer.odpt.org/faq#api-output-limit)）。
超過時は上限以下にフィルタされた結果が返るので、絞り込みクエリで再取得する。

---

## バス関連データ種別（Retimana のメインターゲット）

### `odpt:Bus`（バス車両のリアルタイム位置）
- バス車両の運行情報（現在位置に相当）
- 主要プロパティ:
  - `odpt:busroutePattern` — 運行系統 ID
  - `odpt:fromBusstopPole` / `odpt:toBusstopPole` — 直近通過バス停 / 次の到着バス停
  - `odpt:fromBusstopPoleTime` — 直近バス停の通過時刻
  - `odpt:operator` — 事業者
  - `odpt:occupancyStatus` — 混雑度（2021-03 追加）

### `odpt:BusstopPole`（バス停の標柱情報）
- バス停単位ではなく**標柱（のりば）単位**で管理される
- 位置情報（lat/lng）を持つ
- 多言語対応の名前: `odpt:busstopPoleTitle.ja` / `.en` 等
- 主要プロパティ:
  - `geo:lat` / `geo:long` — 緯度経度（WGS84）
  - `odpt:platformNumber` — のりば番号（2023-11 追加）
  - `odpt:busroutePattern` — 通過する系統のリスト

### `odpt:BusroutePattern`（運行系統）
- 「206系統 京都駅→清水寺」のような**特定方向の系統**を表す
- `odpt:busroute` — 路線 ID（同じ路線の往復が同じ busroute を持つ）
- `odpt:busstopPoleOrder` — 停車順

### `odpt:BusTimetable`（バス時刻表）
- 1便（trip）単位の時刻表
- `odpt:busTimetableObject[]` に各バス停の発着時刻が入る

### `odpt:BusstopPoleTimetable`（バス停の時刻表）
- 特定バス停発の時刻表（バス停目線）
- 曜日区分・行先で複数存在

### `odpt:BusroutePatternFare`（運賃）
- 系統内の乗車標柱・降車標柱ペアごとの運賃

### 関係性
```
odpt:Bus
  ├── odpt:busroutePattern → odpt:BusroutePattern
  │                            ├── odpt:busroute (路線)
  │                            └── odpt:busstopPoleOrder → [odpt:BusstopPole...]
  │
  ├── odpt:fromBusstopPole → odpt:BusstopPole
  └── odpt:toBusstopPole → odpt:BusstopPole

odpt:BusTimetable
  └── odpt:busTimetableObject[] → 各 BusstopPole + 発着時刻
```

---

## ファイルダウンロード API（GTFS-JP ZIP 取得）

### エンドポイント
```
GET https://api.odpt.org/api/v4/files/odpt/{OperatorPath}/{FileName}
    ?date={YYYYMMDD}
    &acl:consumerKey={KEY}
```

### 京都市営バス GTFS-JP の例
```
https://api.odpt.org/api/v4/files/odpt/KyotoMunicipalTransportation/Kyoto_City_Bus_GTFS.zip?date=20260518&acl:consumerKey=YOUR_KEY
```

### `date` パラメータの扱い
- データ公開日（YYYYMMDD）を指定する
- 公開頻度は事業者ごとに異なる（多くは月1回〜不定期）
- **最新日を取得する標準的なエンドポイントは仕様書に明示されていない** ので、以下のいずれかで対応:
  - a. `appsettings.json` に最新公開日を手動設定
  - b. CKAN カタログ（https://ckan.odpt.org/）の事業者ページで公開日を確認
  - c. 直近数日を試行（ブルートフォース）

### ⚠️ 利用条件
GTFS-JP データは**週1回程度の取得**が利用条件で要求されている（事業者ごとに条件が異なる）。アプリ起動毎にDLするのは NG。

---

## 典型的なコード例

### 京都駅周辺の最寄りバス停を取得（地物情報検索API）
```csharp
var lat = 34.985858;   // 京都駅
var lon = 135.758767;
var radius = 500;       // 500m 以内
var key = config["Odpt:ConsumerKey"];

var url = $"https://api.odpt.org/api/v4/places/odpt:BusstopPole" +
          $"?lat={lat}&lon={lon}&radius={radius}&acl:consumerKey={key}";

var stops = await httpClient.GetFromJsonAsync<List<BusstopPole>>(url);
```

### 特定路線の現在走行中バスを取得（データ検索API）
```csharp
var routePatternId = "odpt.BusroutePattern:KyotoMunicipalTransportation.206.1";
var url = $"https://api.odpt.org/api/v4/odpt:Bus" +
          $"?odpt:busroutePattern={routePatternId}&acl:consumerKey={key}";

var buses = await httpClient.GetFromJsonAsync<List<Bus>>(url);
```

### GTFS-JP ZIP をダウンロード
```csharp
var url = $"https://api.odpt.org/api/v4/files/odpt/KyotoMunicipalTransportation/Kyoto_City_Bus_GTFS.zip" +
          $"?date={dataDate}&acl:consumerKey={key}";

using var stream = await httpClient.GetStreamAsync(url);
using var archive = new ZipArchive(stream);
archive.ExtractToDirectory(extractPath, overwriteFiles: true);
```

---

## 多言語対応プロパティの命名規則
- `dc:title` — 日本語名のみ（従来互換のため）
- `odpt:xxxTitle` — 多言語マップ（`{"ja": "…", "en": "…"}`）

例:
```json
"dc:title": "京都駅前",
"odpt:busstopPoleTitle": {
  "ja": "京都駅前",
  "en": "Kyoto Station"
}
```

Retimana の多言語UIには `odpt:busstopPoleTitle` 系を優先的に使う。

---

## レスポンス共通フィールド

| フィールド | 説明 |
|-----------|------|
| `@context` | JSON-LD のコンテキスト URL |
| `@id` | 固有識別子（ucode）。`urn:ucode:_` で始まる |
| `@type` | データ種別。例: `odpt:BusstopPole` |
| `owl:sameAs` | 識別子の別名。`odpt.BusstopPole:京都市バス.京都駅前.XXX.1` のような人間可読 |
| `dc:date` | データ生成日時（ISO 8601） |
| `dct:valid` | データ保証期限（ISO 8601） |

`owl:sameAs` はパラメータで指定する ID として使える（`urn:ucode:` の生IDよりこちらが扱いやすい）。

---

## ライセンス・クレジット

データ利用には **「公共交通オープンデータ基本ライセンス」**（事業者によっては個別ライセンス）への同意が必要。表示義務:

> 本サービスで利用するバスデータは、公共交通オープンデータセンター（ODPT）において提供されるものです。
> 京都市交通局により提供されたデータを元にしていますが、必ずしも正確・完全なものとは限りません。
> 本サービスの表示内容について、京都市交通局への直接のお問い合わせはご遠慮ください。

詳細は [https://developer.odpt.org/terms](https://developer.odpt.org/terms) と事業者ごとの利用条件を確認。

---

## 参考リンク

- API 仕様書（公式）: https://developer.odpt.org/documents
- API 補足: https://developer.odpt.org/api_addendum
- FAQ: https://developer.odpt.org/faq
- データカタログ: https://ckan.odpt.org/
- フォーラム: https://developer-forum.odpt.org/
- 利用規約: https://developer.odpt.org/terms

## 当プロジェクトでの参照

- 取得済み完全仕様: [docs/odpt/api/api_documents.md](odpt/api/api_documents.md)（version 4.15）
- 京都市営バス GTFS サンプル: [docs/odpt/Sample/Kyoto_City_Bus_GTFS-20260518/](odpt/Sample/Kyoto_City_Bus_GTFS-20260518/)（agency / stops / routes / trips / stop_times / calendar / shapes 等）

## 更新履歴
- 2026-05-25: 初版作成（ODPT API v4.15 ベース、Retimana の Kyoto MVP 向け要点抽出）
