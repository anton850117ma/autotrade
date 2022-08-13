# autotrade

## Introduction

這是一個簡單的自動交易程式。

## Usage

相同目錄下有個設定檔`Settings.json`，預設項目和值請參考下方幾個區域，不可更改值的部分會用粗體表示。

- Login: 登入區塊
    - host: 伺服器域名或地址
    - port: 連線埠號
    - debug: 是否開啟API的紀錄功能(`true/false`)
    - timeout: 回報斷線重連間隔時間(秒)
    - username: 登入帳號
    - password: 登入密碼
    - subcomp: 券商分公司代號
    - account: 券商帳號
    - time: 時間相關
        - download: 可下載盤前檔的時間(`HH:MM:SS`)
        - start: 登入時間(`HH:MM:SS`)
        - end: 登出時間(`HH:MM:SS`)

```json
{
  "Login": {
    "host": "itstradeuat.pscnet.com.tw",
    "port": 11002,
    "debug": true,
    "timeout": 5,
    "username": "A100000261",
    "password": "AA123456",
    "subcomp": "5850",
    "account": "8888945",
    "time": {
      "download": "00:00:00",
      "start": "09:00:00",
      "end": "23:00:00"
    }
  }
}
```

- Urls: 網路資源位址相關
    - T30: T30檔案網址
        - TSE: 上市盤前檔網址
        - OTC: 上櫃盤前檔網址
    - Info: 個股資訊網址
        - url: 個股資訊網址
        - update: 是否要更新個股資本額資訊(`true/false`)

```json
"Urls": {
    "T30": {
      "TSE": "http://download.pscnet.com.tw/download/ap/T30/ASCT30S_",
      "OTC": "http://download.pscnet.com.tw/download/ap/T30/ASCT30O_"
    },
    "Info": {
      "url": "https://mops.twse.com.tw/mops/web/index",
      "update": false
    }
  }
```

- Paths: 檔案位址相關
    - Records: 股票紀錄檔讀寫位址
    - LogDir: 日誌檔目錄

```json
  "Paths": {
    "Records": "Data//Records.json",
    "LogDir": "Logs"
  }
```