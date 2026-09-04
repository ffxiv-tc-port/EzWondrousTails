# EzWondrousTails

在天書奇談面板上顯示各格貼出後的中獎機率的插件。

## 功能

- 讀取目前天書奇談盤面，計算貼出 1／2／3 條連線的機率
- 計算花費 2 奇想點重新貼一格之後的平均機率變化，協助判斷是否值得重新貼

與 **DailyDuty** 的天書奇談模組不重疊：DailyDuty 追蹤「這週還有沒有格子可貼／書過期沒／
要不要去找庫洛」，本插件只做面板機率計算，兩者可同時使用。

## 台服（TC）分支說明

本插件是 [MidoriKami/EzWondrousTails](https://github.com/MidoriKami/EzWondrousTails)
（原 `daemitus/WondrousTailsSolver`）針對**台服官方繁中版**（Dalamud API 13）維護的 fork，
由 [ffxiv-tc-port](https://github.com/ffxiv-tc-port) 發佈。

- 分支基準：上游最後一個仍在 `Dalamud.NET.Sdk/13.0.0` 的 commit `21cbd7d`（2025-08-08）。
  上游其後已推進到 API14 並封存（archived），不會再有更新，也不要 rebase 到 `master`。
- `KamiToolKit` 子模組改指 `ffxiv-tc-port/KamiToolKit` 分支 `tc-dailyduty-api13-pin`
  ——上游釘的版本在本 pin 上有相容性問題，org fork 已修好（與 DailyDuty 共用同一份）。
- 介面文字走 `LanguageChineseTraditional.ini`（英文原文當 key），術語依台服官方用語：
  天書奇談／印花／故事線／奇想點／重新貼。

## 安裝

在 Dalamud 設定的「自訂插件庫」加入
`https://raw.githubusercontent.com/ffxiv-tc-port/DalamudPluginsTC/main/repo.json`
並啟用，再從插件列表安裝。
