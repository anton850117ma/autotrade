# autotrade

## Description

這是一個為許哥專門設計的自動交易程式。

## Usage

此程式的介面是個空殼，請勿使用。在開啟程式後，只須等到其自行結束即可。

### Settings

目錄下有個設定檔`Settings.json`，此設定檔必須存在於執行檔所在的目錄底下，且必須唯一。

以下說明包含的各項目與預設值請參考下方幾個區域。

- Login: 登入相關設定
  - host: 伺服器域名或地址
  - port: 連線埠號
  - debug: 是否開啟 API 的紀錄功能(`true / false`)，固定是存在名為`Logs`的資料夾
  - username: 登入帳號
  - password: 登入密碼
  - subcomp: 券商分公司代號
  - account: 券商帳號
  - time: 時間相關
    - download: 可下載盤前檔的時間(`HH:MM:SS`)
    - begin: 登入時間(`HH:MM:SS`)
    - end: 登出時間(`HH:MM:SS`)

**NOTE**: 
- 只要時間是在`登入時間`以後，就會登入。
- 只要時間超過`登出時間`，程式就會自動登出並結束。

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

- Urls: 網路資源位址相關設定
  - T30: T30 檔案網址
    - TSE: 上市盤前檔網址
    - OTC: 上櫃盤前檔網址
  - Info: 個股資訊
    - url: 查詢網址
    - update: 是否只更新個股資本額資訊而不交易(`true / false`)

**NOTE**: 
- 由於觀測站的限制，1分鐘只能查詢20次，更新幾千個股票會非常久，因此將更新資本額和交易分開。
- 由於初始化時盤前檔是必要的，因此必須在能取得盤前檔後才能更新資本額。
- 請透過`update`來決定是更新資本額或交易:
  - `true`: 更新資本額
  - `false`: 交易

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

- Paths: 檔案位址相關設定
  - Records: 股票紀錄檔讀寫路徑
  - LogDir: 日誌檔目錄

**NOTE**: 
- `Records`的路徑可以不存在，程式會在交易結束後在指定的路徑上產生檔案。
- 由於條件裡有交易量這項指標，因此如果沒有紀錄檔同時又開啟交易量的條件，會造成不訂閱任何股票而沒有交易。
- 不過為了防止符合條件股票數超過API訂閱數上限，程式會在結束前自動去訂閱每隻股票以更新交易量，這可能需要幾秒鐘完成。
- 只有在程式自動結束時才會更新每隻股票的交易量，手動關閉視窗只會登出並儲存現有紀錄。

```json
{
  "Paths": {
    "Records": "Data/Records.json",
    "LogDir": "Logs"
  }
}
```

- Rules: 各項判斷條件設定

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
### Records

程式會在開始時嘗試讀取設定檔中的`Records`路徑，用來初始化股票和下單紀錄。結束前，程式會將收到的報價和下單紀錄回寫到該路徑。

### Logs

執行程式時，程式會使用`LogDir`的值來創建存放日誌的目錄，並將日誌寫入該目錄。

**NOTE**:
如果`debug`設置為`true`，雷影的API也會寫日誌，並固定存放在名為Logs資料夾下。