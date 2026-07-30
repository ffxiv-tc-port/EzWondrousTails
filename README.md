# WondrousTailsSolver

A Dalamud plugin that adds row probabilities to the Wondrous Tails display.

![math](https://github.com/user-attachments/assets/d4e00d8a-d3e9-4638-839a-2d93eb0ae928)

---

**台服（TC）分支說明**

這是 [MidoriKami/EzWondrousTails](https://github.com/MidoriKami/EzWondrousTails)（原
`daemitus/WondrousTailsSolver`）針對**台服官方繁中版**（Dalamud API 13）維護的 fork，
由 [ffxiv-tc-port](https://github.com/ffxiv-tc-port) 發佈。

- 分支基準：上游最後一個仍在 `Dalamud.NET.Sdk/13.0.0` 的 commit `21cbd7d`（2025-08-08）。
  上游其後已推進到 API14 並**封存（archived）**，不會再有更新，也不要 rebase 到 `master`。
- `KamiToolKit` 子模組改指 `ffxiv-tc-port/KamiToolKit` 分支 `tc-dailyduty-api13-pin`
  ——上游釘的 `9a52139` 在我們的 API13 pin 上有 3 處 `AtkTimeline.PlayAnimation(byte …)`
  obsolete-as-error，org fork 已修好（與 DailyDuty 共用同一份）。
- 上游 csproj 參照但程式碼從未使用的 `KamiLib` 子模組已移除（上游自己後來也拿掉了）。
- 介面文字走 `LanguageChineseTraditional.ini`（英文原文當 key），術語依台服 EXD：
  天書奇談／印花／故事線／奇想點／重新貼。

安裝方式：在 Dalamud 設定的「自訂插件庫」加入
`https://raw.githubusercontent.com/ffxiv-tc-port/DalamudPluginsTC/main/repo.json` 並啟用，
再從插件列表安裝。

與 **DailyDuty** 的天書奇談模組不重疊：DailyDuty 追蹤「這週還有沒有格子可貼／書過期沒／
要不要去找庫洛」，本插件只做「目前盤面貼出 1／2／3 條故事線的機率，以及花 2 奇想點
重新貼之後的平均機率」的計算，兩者可同時使用。
