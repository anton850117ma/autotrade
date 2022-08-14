# autotrade

## Introduction

這是一個為許哥專門設計的自動交易程式。

## Usage

目錄下有個設定檔`Settings.json`，包含的各項目與預設值請參考下方幾個區域。

- Login: 登入區塊
  - host: 伺服器域名或地址
  - port: 連線埠號
  - debug: 是否開啟 API 的紀錄功能(`true / false`)
  - timeout: 回報斷線重連間隔時間(`秒數`)
  - username: 登入帳號
  - password: 登入密碼
  - subcomp: 券商分公司代號
  - account: 券商帳號
  - time: 時間相關
    - download: 可下載盤前檔的時間(`HH:MM:SS`)
    - begin: 登入時間(`HH:MM:SS`)
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
      "download": "08:30:00",
      "begin": "09:00:00",
      "end": "17:00:00"
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
    - update: 是否要更新個股資本額資訊(`true / false`)

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

- Rules: 各項判斷條件

  - Exclude: 排除條件類別

    - **IDLength**: 股票代號長度過長
      - enabled: 是否啟用此條件(`true / false`)
      - length: 代號長度大於此值會被排除(`整數`)
      - fund: 是否排除基金(`true / false`)
    - **NotDayTrade**: 無當沖註記
      - enabled: 是否啟用此條件(`true / false`)
    - **FullCash**: 是全額交割(用交易方式判斷)
      - enabled: 是否啟用此條件(`true / false`)
    - **Disposed**: 有處置註記
      - enabled: 是否啟用此條件(`true / false`)
    - **Capital**: 資本額過大
      - enabled: 是否啟用此條件(`true / false`)
      - amount: 大於等於此值會被排除(`字串`)
    - **TradeAmount**: 昨日交易量過低
      - enabled: 是否啟用此條件(`true / false`)
      - total: 小於此值會被排除(`整數`)
    - **TradePrice**: 昨日收盤價過低或過高
      - enabled: 是否啟用此條件(`true / false`)
      - price: 價格區間(區間外的會被排除)
        - min: 最小價格(`整數或小數`)
        - max: 最大價格(`整數或小數`)

  - Buy: 買進條件類別

    - **NowPrice**: 最新成交價大於等於昨日收盤價 x106.66%且無庫存

      - enabled: 是否啟用此條件(`true / false`)
      - time: 時間區間(區間內才會觸發)
        - begin: 開始時間(`HH:MM:SS`)
        - end: 結束時間(`HH:MM:SS`)
      - stock: 庫存規則
        - amount: 庫存量大於此值將不觸發(`整數`)
      - price: 價格規則
        - now: 最新成交價
          - compare: 比較方式(`> / >= / == / <= / < / !=`)
        - close: 昨日收盤價乘以下方倍數
          - factor: 倍數(`整數或小數`)
      - order: 下單方式
        - cost: 最高金額(`正整數`)
        - timeinforce: 下單時間(`R / I / F`，分別代表ROD、IOC、FOK)

  - Sell: 賣出條件類別

    - **NowPrice1**: 最新成交價小於等於目前最高價 x96.67且有庫存

      - enabled: 是否啟用此條件(`true / false`)
      - time: 時間區間(區間內才會觸發)
        - begin: 開始時間(`HH:MM:SS`)
        - end: 結束時間(`HH:MM:SS`)
      - stock: 庫存規則
        - amount: 庫存量小於此值將不觸發(`整數`)
      - price: 價格規則
        - now: 最新成交價
          - compare: 比較方式(`> / >= / == / <= / < / !=`)
        - max: 目前最高價乘以下方倍數
          - factor: 倍數(`整數或小數`)
      - order: 下單方式
        - cost: 最高金額(`整數`，負數代表所有庫存)
        - timeinforce: 下單時間(`R / I / F`，分別代表ROD、IOC、FOK)

    - **NowPrice2**: 最新成交價不等於漲停價且有庫存

      - enabled: 是否啟用此條件(`true / false`)
      - time: 時間區間(區間內才會觸發)
        - begin: 開始時間(`HH:MM:SS`)
        - end: 結束時間(`HH:MM:SS`)
      - stock: 庫存規則
        - amount: 庫存量小於此值將不觸發(`整數`)
      - price: 價格規則
        - now: 最新成交價
          - compare: 比較方式(`> / >= / == / <= / < / !=`)
        - bull: 今日漲停價乘以下方倍數
          - factor: 倍數(`整數或小數`)
      - order: 下單方式
        - cost: 最高金額(`整數`，負數代表所有庫存)
        - timeinforce: 下單時間(`R / I / F`，分別代表ROD、IOC、FOK)

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
          "min": 10.0,
          "max": 400.0
        }
      }
    },
    "Buy": {
      "NowPrice": {
        "enabled": true,
        "time": {
          "begin": "09:00:00",
          "end": "09:30:00"
        },
        "stock": {
          "amount": 0
        },
        "price": {
          "now": {
            "compare": ">="
          },
          "close": {
            "factor": 1.0666
          }
        },
        "order": {
          "cost": 400000,
          "timeinforce": "R"
        }
      }
    },
    "Sell": {
      "NowPrice1": {
        "enabled": true,
        "time": {
          "begin": "09:00:00",
          "end": "13:30:00"
        },
        "stock": {
          "amount": 1
        },
        "price": {
          "now": {
            "compare": "<="
          },
          "max": {
            "factor": 0.9667
          }
        },
        "order": {
          "cost": -1,
          "timeinforce": "R"
        }
      },
      "NowPrice2": {
        "enabled": true,
        "time": {
          "begin": "10:30:00",
          "end": "13:30:00"
        },
        "stock": {
          "amount": 1
        },
        "price": {
          "now": {
            "compare": "<"
          },
          "bull": {
            "factor": 1
          }
        },
        "order": {
          "cost": -1,
          "timeinforce": "R"
        }
      }
    }
  }
}
```
