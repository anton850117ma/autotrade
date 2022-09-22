# AutoTrade

## 簡介

這是一個為許哥專門設計的自動交易程式。

## 使用方式

此程式的介面是個空殼，請勿使用。在執行程式後，只須等到其自行結束即可。

### 設定檔

**設定檔`Settings.json`，此設定檔必須存在於程式所在的目錄或子目路底下，且必須唯一，否則程式會跳出例外視窗。**

以下說明包含的各項目與預設值請參考下方幾個區域。

#### 登入參數

- Login: 登入相關設定
  - host: 伺服器域名或地址
  - port: 連線埠號
  - debug: 是否開啟API的日誌功能(`true / false`)
  - verbose: 程式紀錄事件的最低嚴重性層級(`0 ~ 5`)
  - username: 登入帳號
  - password: 登入密碼
  - subcomp: 券商分公司代號
  - account: 券商帳號
  - time: 時間相關
    - download: 可下載盤前檔的時間(`HH:MM:SS`)
    - begin: 登入時間(`HH:MM:SS`)
    - end: 登出時間(`HH:MM:SS`)

**注意**: 

- `debug`的路徑固定為`Logs`，跟下方的`LogDir`沒有關係。
- 執行時間是在`登入時間`以後，程式就會登入。
- 執行時間超過`登出時間`，程式就會自動登出並結束。
- 如果是更新資本額，則將不會登入或登出。
- `verbose`從0到5分為6級，0嚴重性最低，5嚴重性最高。
  - 0 (Trace): 程式最詳盡的運行流程
  - 1 (Debug): 一些互動式的調查記錄
  - 2 (Info): 與交易相關的事件
  - 3 (Warning): 異常或非預期的事件
  - 4 (Error): 影響程式正常交易的錯誤事件
  - 5 (Critical): 影響程式正常運行的重大錯誤事件
  - 以0為例，只要是嚴重性0以上的資訊或事件都會被記錄在日誌裡。

```json
{
  "Login": {
    "host": "itstradeuat.pscnet.com.tw",
    "port": 11002,
    "debug": false,
    "verbose": 0,
    "username": "A100000261",
    "password": "AA123456",
    "subcomp": "5850",
    "account": "8888945",
    "time": {
      "download": "08:30:00",
      "begin": "09:00:00",
      "end": "14:00:00"
    }
  }
}
```
#### 外部網路參數

- Urls: 網路資源位址相關設定
  - T30: T30 檔案網址
    - TSE: 上市盤前檔網址
    - OTC: 上櫃盤前檔網址
  - Info: 個股資訊
    - url: 查詢網址
    - update: 是否只更新個股資本額資訊而不交易(`true / false`)

**注意**: 

- 由於觀測站的限制，1分鐘只能查詢20次，更新幾千個股票會非常久，因此將更新資本額和交易分開。
- 由於初始化時盤前檔是必要的，因此必須在能取得盤前檔後才能更新資本額。
- 請透過`update`來決定是更新資本額或交易:
  - `true`: 更新資本額
  - `false`: 交易
- 更新資本額時會自動套用下方`OnlyShare`條件來排除不必要的查詢。

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

#### 路徑參數

- Paths: 檔案位址相關設定
  - Records: 紀錄檔讀寫路徑
  - LogDir: 日誌檔目錄

**注意**: 

- `Records`的路徑可以不存在，程式會在交易結束後在指定的路徑上產生檔案。
- 由於條件裡有交易量這項指標，因此如果沒有紀錄檔同時又開啟交易量的條件，會造成不訂閱任何股票而沒有交易。
- 不過為了防止符合條件股票數超過API訂閱數上限，程式會在結束前自動去訂閱每隻股票以更新交易量，這可能需要幾十秒完成。
- 只有在程式自動結束(也就是登出時間以後)才會更新每隻股票的交易量，手動關閉視窗只會登出並儲存現有紀錄。

```json
{
  "Paths": {
    "Records": "data/Records.json",
    "LogDir": "Logs"
  }
}
```

#### 條件設定參數

- Rules: 各項判斷條件設定
  - Exclude: 排除條件類別
    - **OnlyShare**: 股票代號非一般上市櫃(如只能是4碼)
      - enabled: 是否啟用此條件(`true / false`)
      - code: 排除排列順序在此字串前的股票代號(`字串`)
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
    - **NowPrice**: 最新成交價大於等於昨日收盤價x106.66%且無庫存
      - enabled: 是否啟用此條件(`true / false`)
      - time: 時間區間(區間內才會觸發)
        - begin: 開始時間(`HH:MM:SS`)
        - end: 結束時間(`HH:MM:SS`)
      - prebull: 是否檢查昨日是否漲停(`true / false`)
      - stock: 庫存規則
        - times: 買進次數的上限值(`整數`)
      - price: 價格規則
        - now: 最新成交價
          - compare: 比較方式(`> / >= / == / <= / < / !=`)
        - close: 昨日收盤價乘以下方倍數
          - factor: 倍數(`整數或小數`)
      - order: 下單方式
        - cost: 最高金額(`正整數`)
        - timeinforce: 下單時間(`R / I / F`)
  - Sell: 賣出條件類別
    - **NowPrice1**: 最新成交價小於等於目前最高價x96.67%且有庫存
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
        - timeinforce: 下單時間(`R / I / F`)
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
        - timeinforce: 下單時間(`R / I / F`)

**注意**: 

- 買進賣出的庫存條件差異:
  - 買進: 買進是用次數限制，如果有昨日庫存，該股票的買進初始值就會是1。
  - 賣出: 賣出是用庫存量限制，如果有至少該值的庫存就會滿足該條件。
- 關於買賣方式:
  - 買進: `order`中的`cost`參數決定一次的買進價值，算式為: 漲停價 x 張數 x 1000 小於等於該值。
  - 賣出: 原理同上，若值為負數則會將全部庫存賣出。
- 關於比較方式，依照上面所列從左至右分別為: 大於、大於等於、等於、小於等於、小於、不等於。
  - 請挑選其中一個，如果不是其中任何一個或是字串中有其他符號(包含空格)，該條件永遠不滿足。
- 關於下單時間(`timeinforce`)，`R / I / F`分別代表ROD、IOC、FOK。
- 條件名稱無法更改(如`NowPrice2`)，因為已寫死在程式裡。

```json
{
  "Rules": {
    "Exclude": {
      "OnlyShare": {
        "enabled": true,
        "code": "0100  "
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
          "begin": "09:00:00",
          "end": "09:30:00"
        },
        "prebull": true,
        "stock": {
          "times": 1
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
### 記錄檔

程式會在開始時嘗試讀取設定檔中`Records`的路徑，用來更新股票的資本額、交易量和庫存。
結束前，程式會將收到的報價和下單紀錄回寫到該路徑。

#### 紀錄格式

- `symbol`: 股票代號
- `capital`: 實收資本額
- `total`: 昨日成交量
- `bull`: 昨日漲停價
- `stock`: 庫存

**注意**: 

- `昨日成交量`與`昨日漲停價`只有在程式執行到登出時間以後才會更新，以避免將今日的資料覆寫回去而造成汙染。
- 每次回寫紀錄時，都會覆寫原本的紀錄，有需要留存請先手動保存到其他位置。

```json
[
  {
    "symbol": "1101  ",
    "capital": "69368513420",
    "total": 0,
    "bull": 0.0,
    "stock": 0
  }
]
```

### 日誌檔

執行程式時，程式會使用`LogDir`的路徑來創建存放日誌的目錄，並將日誌以日期命名寫入該目錄。
如果已經有當天的日誌檔，新的日誌會被增加到原本日誌內容的下方，而不會再直接覆寫整個日誌檔。