# Retimana - Claude Code 引き継ぎドキュメント

## プロジェクト概要
京都の観光客向けリアルタイムバス案内 PWA（Webアプリ）

## 名前の由来
```
Re  →  Realis    ラテン語「本物・リアル」
Ti  →  Tempus    ラテン語「時間・タイミング」
ma  →  Mappa     ラテン語「地図」
na  →  Navigare  ラテン語「向かう・進む」
```

## ドメイン
retimana.com（さくらインターネット）

---

## 技術スタック

### バックエンド
- **言語**: C#
- **フレームワーク**: ASP.NET Core (.NET 9)
- **リアルタイム配信**: SignalR
- **GTFSパース**: Google.Protobuf + GtfsRealtimeBindings
- **静的GTFS読み込み**: CsvHelper
- **決済**: Stripe Payment Links
- **ホスティング**: さくらインターネット

### フロントエンド
- **地図**: Leaflet + OpenStreetMap
- **地図スタイル**: ベージュ・アイボリー系ドットパターン
- **リアルタイム更新**: SignalR クライアント
- **PWA**: manifest.json + Service Worker

### データソース
- **静的データ**: GTFS-JP（京都市交通局）
- **リアルタイム**: GTFS-RT（ODPT API）
- **施設検索**: OpenStreetMap Nominatim API
- **データ提供**: 公共交通オープンデータセンター（ODPT）

---

## コアコンセプト

### 既存アプリとの差別化
- バス停名を知らなくても使える
- 「行き先（施設名・観光スポット）」を選ぶだけ
- リアルタイムのバス位置が主役
- 「どれに乗ればいいか」だけを表示
- 乗り間違い防止（逆方向のバスは表示しない）

### メインフロー
```
施設名を検索（例:「清水寺」）
    ↓
現在地から最寄りバス停 Top3 を自動抽出
    ↓
各バス停に来る「目的地行きのバス」をリアルタイム表示
    ↓
「今すぐ乗れる便」を強調表示（徒歩時間 vs バス到着時間）
```

---

## データ処理の核心ロジック

### 施設 → バス停の逆引き
```csharp
// 1. 施設名 → 緯度経度（Nominatim API）
// 2. 緯度経度 → 最寄りバス停 Top3（GTFS stops.txt + Haversine）
// 3. バス停 → 目的地行きのtrip一覧（GTFS stop_times.txt）
// 4. trip → リアルタイム位置（GTFS-RT VehiclePosition）
// 5. 到着予測（GTFS-RT TripUpdate）
```

### 距離計算
```csharp
public List<NearbyStop> FindNearestStops(double lat, double lng, int topN = 3)
{
    return allStops
        .Select(s => new {
            Stop = s,
            DistanceM = Haversine(lat, lng, s.Latitude, s.Longitude)
        })
        .Where(x => x.DistanceM <= 500)
        .OrderBy(x => x.DistanceM)
        .Take(topN)
        .Select(x => new NearbyStop {
            Stop = x.Stop,
            DistanceM = x.DistanceM,
            WalkingMinutes = Math.Ceiling(x.DistanceM / 80.0)
        })
        .ToList();
}
```

### GTFS-RT パース（C#）
```csharp
using TransitRealtime;

byte[] data = await http.GetByteArrayAsync(url);
FeedMessage feed = FeedMessage.Parser.ParseFrom(data);

foreach (var entity in feed.Entity)
{
    if (entity.Vehicle != null)
    {
        var lat = entity.Vehicle.Position.Latitude;
        var lng = entity.Vehicle.Position.Longitude;
        var tripId = entity.Vehicle.Trip.TripId;
    }
}
```

---

## ODPT API

### エンドポイント
```
https://api-public.odpt.org/api/v4/gtfs/realtime/
```

### 認証
```
?acl:consumerKey=YOUR_API_KEY
```

### 必要な NuGet パッケージ
```
Google.Protobuf
GtfsRealtimeBindings
CsvHelper
Microsoft.AspNetCore.SignalR
Stripe.net
```

---

## 表示UI仕様

### 画面1：行き先選択
- 検索ボックス（施設名・観光スポット）
- よく使う行き先リスト（アイコン付き）
- 現在地ボタン

### 画面2：バス位置表示
- 上部：Leafletマップ（ベージュ系ドットスタイル）
- 下部：バス一覧（到着順）
  - 系統番号バッジ（赤）
  - 行き先名（大きく）
  - 到着時間（分）
  - のりば名・徒歩時間
  - 「余裕 / 急ぎ足 / 間に合わない」判定

### 地図スタイル
- 背景：ベージュ・アイボリー系ドットパターン
- 道路：白〜クリーム色
- 建物：グリーン系
- バスマーカー：ブランドカラー（未定）
- のりばピン：グリーン系

---

## 収益モデル
1. **ユーザー投げ銭**: Stripe Payment Links（多通貨・Alipay・WeChat Pay対応）
2. **スポンサー**: 観光協会・ホテル・観光施設
3. **自治体連携**: 地域版OEM提供

---

## ライセンス・クレジット表記（必須）
```
本アプリで利用するバスデータは、公共交通オープンデータセンター
（ODPT）において提供されるものです。京都市交通局により提供された
データを元にしていますが、必ずしも正確・完全なものとは限りません。
本アプリの表示内容について、京都市交通局への直接のお問い合わせは
ご遠慮ください。

お問い合わせ：[メールアドレス]
データ取得日時：[動的表示]
```

---

## フェーズ1 MVP スコープ（まず作るもの）
- [ ] ASP.NET Core プロジェクト作成
- [ ] 京都市バス GTFS-JP の取り込み（stops.txt / trips.txt / stop_times.txt）
- [ ] Nominatim API で施設名 → 緯度経度
- [ ] 最寄りバス停 Top3 抽出ロジック
- [ ] GTFS-RT ポーリング（30秒間隔）
- [ ] Leaflet マップ表示
- [ ] バス位置リアルタイム表示
- [ ] 行き先選択UI
- [ ] PWA 設定（manifest.json + Service Worker）
- [ ] Stripe Payment Links 設置

---

*Retimana - Realis Tempus Mappa Navigare*
*「本物のリアルタイム地図で、目的地へ進もう」*
