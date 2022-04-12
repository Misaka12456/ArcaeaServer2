## song/info

| arguments  | description                                                                | optional                                        |
|:-----------|:---------------------------------------------------------------------------|-------------------------------------------------|
| songid     | sid in Arcaea songlist                                                     | true when songname is not null, otherwise false |

#### Example

+ `{apiurl}/botarcapi/song/info?songname=infinity`

###### Return data

```json
{
  "status": 0,
  "content": {
    "id": "infinityheaven",
    "title_localized": {
      "en": "Infinity Heaven"
    },
    "artist": "HyuN",
    "bpm": "160",
    "bpm_base": 160.0,
    "set": "base",
    "set_friendly": "Arcaea",
    "world_unlock": false,
    "remote_dl": false,
    "side": 0,
    "time": 154,
    "date": 1491868800,
    "version": "1.0",
    "difficulties": [
      {
        "ratingClass": 0,
        "chartDesigner": "Nitro",
        "jacketDesigner": "Tagtraume",
        "jacketOverride": false,
        "realrating": 15,
        "totalNotes": 336
      },
      {
        "ratingClass": 1,
        "chartDesigner": "Nitro",
        "jacketDesigner": "Tagtraume",
        "jacketOverride": false,
        "realrating": 55,
        "totalNotes": 545
      },
      {
        "ratingClass": 2,
        "chartDesigner": "Nitro",
        "jacketDesigner": "Tagtraume",
        "jacketOverride": false,
        "realrating": 75,
        "totalNotes": 853
      },
      {
        "ratingClass": 3,
        "chartDesigner": "Nitr∞",
        "jacketDesigner": "Tagtraume",
        "jacketOverride": true,
        "realrating": 96,
        "totalNotes": 986
      }
    ]
  }
}
```

