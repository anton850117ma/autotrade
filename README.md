# autotrade

## Introduction

這是一個為許哥專門設計的自動交易程式。

## Usage

目錄下有個設定檔`Settings.json`，包含的各項目與預設值請參考下方幾個區域。

- Login: 登入區塊
  - host: 伺服器域名或地址
  - port: 連線埠號
  - debug: 是否開啟 API 的紀錄功能(`true/false`)
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
  - T30: T30 檔案網址
    - TSE: 上市盤前檔網址
    - OTC: 上櫃盤前檔網址
  - Info: 個股資訊網址
    - url: 個股資訊網址
    - update: 是否要更新個股資本額資訊(`true/false`)

```json
{
  "Urls": {
    "T30": {
      "TSE": "http://download.pscnet.com.tw/download/ap/T30/ASCT30S_",
      "OTC": "http://download.pscnet.com.tw/download/ap/T30/ASCT30O_"
    },
    "Info": {
      "url": "https://mops.twse.com.tw/mops/web/t146sb05",
      "update": false
    }
  }
}
```

- Paths: 檔案位址相關
  - Records: 股票紀錄檔讀寫位址
  - LogDir: 日誌檔目錄

```json
{
  "Paths": {
    "Records": "Data/Records.json",
    "LogDir": "Logs"
  }
}
```

- Rules: 各項條件判斷規則
  - Exclude: 排除條件類別
    - IDLength: 股票代號長度
      - enabled: 是否啟用此規則(`true/false`)
      - length: 代號長度大於此值會被排除
      - fund: 是否排除基金(`true/false`)
    - NotDayTrade: 當沖註記()
      - enabled: 是否啟用此規則(`true/false`)
    - FullCash: 全額交割(用交易方式判斷)
      - enabled: 是否啟用此規則(`true/false`)
    - Disposed: 處置註記
      - enabled: 是否啟用此規則(`true/false`)

```json
{
  "Rules": {
    "Exclude": {
      "IDLength": {
        "enabled": true,
        "length": 4,
        "fund": true
      },
      "NotDayTrade": {
        "enabled": true
      },
      "FullCash": {
        "enabled": true
      },
      "Disposed": {
        "enabled": true
      },
      "Capital": {
        "enabled": true,
        "amount": "10000000000"
      },
      "TradeAmount": {
        "enabled": true,
        "total": 200
      },
      "TradePrice": {
        "enabled": true,
        "price": {
          "min": 10,
          "max": 400
        }
      }
    },
    "Buy": {
      "NowPrice": {
        "enabled": true,
        "time": {
          "start": "09:00:00",
          "end": "09:30:00"
        },
        "stock": {
          "amount": 0
        },
        "price": {
          "close": {
            "factor": 1.0666
          },
          "now": {
            "compare": ">="
          }
        },
        "order": {
          "cost": 400000,
          "timeinforce": "ROD"
        }
      }
    },
    "Sell": {
      "NowPrice1": {
        "enabled": true,
        "time": {
          "start": "09:00:00",
          "end": "13:30:00"
        },
        "stock": {
          "amount": 1
        },
        "price": {
          "max": {
            "factor": 0.9667
          },
          "now": {
            "compare": "<="
          }
        },
        "order": {
          "cost": -1,
          "timeinforce": "ROD"
        }
      },
      "NowPrice2": {
        "enabled": true,
        "time": {
          "start": "10:00:00",
          "end": "13:30:00"
        },
        "stock": {
          "amount": 1
        },
        "price": {
          "bull": {
            "factor": 1
          },
          "now": {
            "compare": "<"
          }
        },
        "order": {
          "cost": -1,
          "timeinforce": "ROD"
        }
      }
    }
  }
}
```
