# Retimana - マップデザイン仕様

## デザインコンセプト
- ベージュ・アイボリー系のドットパターン背景
- 道路：白〜クリーム色でくっきり表示
- 建物：グリーン系ではっきり色分け
- バスマーカー：ブランドカラー（未定・後で差し替え）
- ピクセル・ドット感のあるユニークなスタイル
- シンプルで「バスの位置だけが目に入る」設計

---

## Leaflet 実装

### 基本セットアップ
```html
<!-- Leaflet CSS / JS -->
<link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css"/>
<script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>

<div id="map" style="width:100%; height:100vh;"></div>
```

```javascript
const map = L.map('map', {
    center: [35.0116, 135.7681],  // 京都駅
    zoom: 16,
    zoomControl: true,
    attributionControl: true
});
```

---

## タイルレイヤー

### 推奨：Stadia Maps（Stamen Toner Lite）
ベージュ系に近いシンプルなスタイル。無料枠あり。

```javascript
L.tileLayer('https://tiles.stadiamaps.com/tiles/stamen_toner_lite/{z}/{x}/{y}{r}.png', {
    maxZoom: 20,
    attribution: '© Stadia Maps © Stamen Design © OpenMapTiles © OpenStreetMap'
}).addTo(map);
```

### 代替：MapTiler Basic
```javascript
L.tileLayer('https://api.maptiler.com/maps/basic/{z}/{x}/{y}.png?key=YOUR_KEY', {
    maxZoom: 20,
    attribution: '© MapTiles © OpenStreetMap'
}).addTo(map);
```

---

## CSSスタイル（ドット・ベージュ感を出す）

```css
/* 地図全体にベージュフィルター */
.leaflet-tile {
    filter: sepia(0.25) saturate(0.7) brightness(1.05) contrast(0.95);
    image-rendering: pixelated;
}

/* 地図コンテナ背景色 */
.leaflet-container {
    background: #F5F0E8;
}

/* ポップアップのスタイル */
.leaflet-popup-content-wrapper {
    background: #F5F0E8;
    border: 2px solid #C8C0B0;
    border-radius: 4px;
    font-family: monospace;
}
```

---

## カラーパレット

```css
:root {
    /* 背景 */
    --map-bg:           #F5F0E8;  /* アイボリー */
    --map-dot:          #DDD8CC;  /* ドット色 */

    /* 道路 */
    --road-main:        #F0EBE0;  /* メイン道路 */
    --road-sub:         #E8E2D6;  /* 細道 */

    /* 建物 */
    --building-fill:    #C8D4B8;  /* 建物塗り */
    --building-stroke:  #A8BC90;  /* 建物枠線 */
    --building-label:   #4A5C38;  /* 建物ラベル */

    /* UI下部パネル */
    --panel-bg:         #EBE4D8;
    --panel-border:     #C8C0B0;
    --panel-text:       #5F5850;
    --panel-muted:      #A09888;

    /* バスマーカー（仮・後でブランドカラーに差し替え） */
    --bus-primary:      #888078;  /* メインバス */
    --bus-secondary:    #A09888;  /* サブバス */

    /* のりばピン */
    --stop-pin:         #888078;  /* 現在地のりば */
}
```

---

## マーカー実装

### のりば（現在地）ピン
```javascript
const stopIcon = L.divIcon({
    className: '',
    html: `
        <div style="
            width: 16px; height: 16px;
            background: var(--stop-pin);
            border: 3px solid #F5F0E8;
            border-radius: 2px;
            position: relative;
        ">
            <div style="
                position: absolute; top: -6px; left: 50%;
                transform: translateX(-50%);
                width: 6px; height: 6px;
                background: var(--stop-pin);
                border-radius: 1px;
            "></div>
        </div>
    `,
    iconSize: [16, 22],
    iconAnchor: [8, 22]
});
```

### バスマーカー（接近中）
```javascript
function createBusMarker(routeName, isRecommended) {
    return L.divIcon({
        className: '',
        html: `
            <div style="
                background: ${isRecommended ? 'var(--bus-primary)' : 'var(--bus-secondary)'};
                color: #F5F0E8;
                font-family: monospace;
                font-size: 10px;
                font-weight: bold;
                padding: 4px 6px;
                border-radius: 3px;
                border: 2px solid #F5F0E8;
                white-space: nowrap;
                box-shadow: 2px 2px 0 rgba(0,0,0,0.2);
            ">${routeName}</div>
        `,
        iconSize: [40, 24],
        iconAnchor: [20, 12]
    });
}
```

---

## バス位置のリアルタイム更新

```javascript
const busMarkers = {};

async function updateBusPositions(destinationStopId) {
    const res = await fetch(`/api/buses?destination=${destinationStopId}`);
    const buses = await res.json();

    // 古いマーカーを削除
    Object.keys(busMarkers).forEach(id => {
        if (!buses.find(b => b.id === id)) {
            map.removeLayer(busMarkers[id]);
            delete busMarkers[id];
        }
    });

    buses.forEach(bus => {
        const icon = createBusMarker(bus.routeName, bus.isRecommended);

        if (busMarkers[bus.id]) {
            // 既存マーカーを移動（アニメーション）
            busMarkers[bus.id].setLatLng([bus.lat, bus.lng]);
            busMarkers[bus.id].setIcon(icon);
        } else {
            // 新規マーカーを追加
            busMarkers[bus.id] = L.marker([bus.lat, bus.lng], { icon })
                .addTo(map)
                .bindPopup(`
                    <b>${bus.routeName}</b><br>
                    ${bus.destination}行き<br>
                    あと約${bus.etaMinutes}分
                `);
        }
    });
}

// 30秒ごとに更新
setInterval(() => updateBusPositions(currentDestinationId), 30000);
```

---

## 現在地追跡

```javascript
let userMarker = null;

navigator.geolocation.watchPosition((pos) => {
    const latlng = [pos.coords.latitude, pos.coords.longitude];

    if (!userMarker) {
        userMarker = L.circleMarker(latlng, {
            radius: 8,
            fillColor: '#F5F0E8',
            color: '#888078',
            weight: 3,
            fillOpacity: 1
        }).addTo(map);
    } else {
        userMarker.setLatLng(latlng);
    }
}, null, {
    enableHighAccuracy: true,
    maximumAge: 10000,
    timeout: 5000
});
```

---

## 下部パネル（バス一覧）HTML構造

```html
<div id="bus-panel" style="
    position: fixed; bottom: 0; left: 0; right: 0;
    background: var(--panel-bg);
    border-top: 2px solid var(--panel-border);
    padding: 12px;
    font-family: monospace;
    z-index: 1000;
    max-height: 40vh;
    overflow-y: auto;
">
    <!-- おすすめ便（強調） -->
    <div class="bus-item recommended" style="
        border: 2px solid var(--bus-primary);
        border-radius: 4px;
        padding: 10px 12px;
        margin-bottom: 8px;
        display: flex;
        align-items: center;
        justify-content: space-between;
    ">
        <div>
            <span class="route-badge" style="
                background: #D85A30;
                color: #F5F0E8;
                font-size: 10px;
                padding: 2px 6px;
                border-radius: 3px;
                margin-right: 6px;
            ">206</span>
            <span style="font-size: 13px; color: var(--panel-text);">京都駅行き</span>
        </div>
        <div style="text-align: right;">
            <span style="font-size: 18px; font-weight: bold; color: var(--panel-text);">3分</span>
            <div style="font-size: 10px; color: var(--panel-muted);">A2のりば・徒歩2分</div>
        </div>
    </div>
</div>
```

---

## ズームレベルの推奨値

| シーン | ズームレベル |
|--------|------------|
| バス停周辺を見る | 17 |
| 街全体を把握 | 15〜16 |
| バスが近づいた（1分以内） | 17〜18 |
| 複数のりばを比較 | 15 |

```javascript
// ETAに応じてズームを自動調整
function adjustZoomByEta(etaMinutes) {
    if (etaMinutes <= 2) map.setZoom(17);
    else if (etaMinutes <= 5) map.setZoom(16);
    else map.setZoom(15);
}
```

---

## 今後の課題（色未定）
- バスマーカーの色：ブランドカラー確定後に `--bus-primary` を更新
- のりばピンの色：同上
- ロゴカラーと地図カラーの統一

---

*Retimana - Realis Tempus Mappa Navigare*
