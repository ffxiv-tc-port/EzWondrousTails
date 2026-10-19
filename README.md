# EzWondrousTails

[MidoriKami/EzWondrousTails](https://github.com/MidoriKami/EzWondrousTails)（原 `daemitus/WondrousTailsSolver`）的台服移植版。
在天書奇談面板上顯示各格貼出後的中獎機率的插件。

## 功能

- 讀取目前天書奇談盤面，計算貼出 1／2／3 條連線的機率
- 計算花費 2 奇想點「重新貼」（取下已經貼上的印花進行隨機重貼）之後的平均機率，協助判斷是否值得重新貼
- 純唯讀計算，不會替你操作

與 **DailyDuty** 的天書奇談模組不重疊：DailyDuty 追蹤「這週還有沒有格子可貼／書過期沒／
要不要去找庫洛」，本插件只做面板機率計算，兩者可同時使用。

## 台服版的修改

- 「重新貼平均」改成精確枚舉所有可能盤面，不再是隨機取樣的估計值
- 「重新貼平均」只在已貼 3～7 張印花時顯示，對上遊戲實際開放重新貼的張數
- 新增唯讀 IPC（`WondrousTailsSolver.GetLineChances`、`WondrousTailsSolver.ApiVersion`），別的插件可以查任一盤面的連線機率
- 介面繁體中文化（術語依台服官方用語：天書奇談／印花／故事線／奇想點／重新貼）

## 台服（TC）分支說明

- 分支基準：上游最後一個仍在 Dalamud API 13 的版本。上游其後已改到新版 API 並封存（archived），
  不會再有更新，也不要 rebase 到 `master`。
- `KamiToolKit` 子模組改指 `ffxiv-tc-port/KamiToolKit` 分支 `tc-dailyduty-api13-pin`
  ——上游釘的版本在這個 Dalamud 版本上有相容性問題，org fork 已修好（與 DailyDuty 共用同一份）。
- 介面文字走 `LanguageChineseTraditional.ini`（英文原文當 key）。

## 安裝

在 Dalamud 設定的「自訂插件庫」加入
`https://raw.githubusercontent.com/ffxiv-tc-port/DalamudPluginsTC/main/repo.json`
並啟用，再從插件列表安裝。

原作者：[daemitus](https://github.com/daemitus)、[MidoriKami](https://github.com/MidoriKami)
