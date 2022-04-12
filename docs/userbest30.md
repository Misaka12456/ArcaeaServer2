## user/best30

| arguments    | description                                                                     | optional                                        |
|:-------------|:--------------------------------------------------------------------------------|-------------------------------------------------|
| user         | user name or 9-digit user code                                                  | true when usercode is not null, otherwise false |
| withrecent   | boolean. if true, will reply with recent_score                                  | true                                            |
| withsonginfo | boolean. if true, will reply with songinfo                                      | true                                            |

#### Example

+ `{apiurl}/botarcapi/user/best30?user=ToasterKoishi&withrecent=true&withsonginfo=true`

###### Return data (Edited for readability)

```json
{
  "status": 0,
  "content": {
    "best30_avg": 12.707672500000001,
    "recent10_avg": 12.836982499999998,
    "account_info": {
      "code": "062596721",
      "name": "ToasterKoishi",
      "user_id": 4,
      "is_mutual": false,
      "is_char_uncapped_override": false,
      "is_char_uncapped": true,
      "is_skill_sealed": false,
      "rating": 1274,
      "join_date": 1487816563340,
      "character": 12
    },
    "best30_list": [
      {
        "score": 9956548,
        "health": 100,
        "rating": 13.082740000000001,
        "song_id": "grievouslady",
        "modifier": 0,
        "difficulty": 2,
        "clear_type": 1,
        "best_clear_type": 5,
        "time_played": 1614911430950,
        "near_count": 7,
        "miss_count": 3,
        "perfect_count": 1440,
        "shiny_perfect_count": 1376
      },
      {
        "score": 9884488,
        "health": 100,
        "rating": 12.92244,
        "song_id": "tempestissimo",
        "modifier": 0,
        "difficulty": 3,
        "clear_type": 1,
        "best_clear_type": 5,
        "time_played": 1591566895228,
        "near_count": 20,
        "miss_count": 8,
        "perfect_count": 1512,
        "shiny_perfect_count": 1372
      }    
    ],
    "best30_songinfo": [
      {
        "id": "grievouslady",
        "title_localized": {
          "en": "Grievous Lady"
        },
        "artist": "Team Grimoire vs Laur",
        "bpm": "210",
        "bpm_base": 210.0,
        "set": "yugamu",
        "set_friendly": "Vicious Labyrinth",
        "world_unlock": false,
        "remote_dl": true,
        "side": 1,
        "time": 141,
        "date": 1509667208,
        "version": "1.5",
        "difficulties": [
          {
            "ratingClass": 0,
            "chartDesigner": "迷路第一層",
            "jacketDesigner": "シエラ",
            "jacketOverride": false,
            "realrating": 65,
            "totalNotes": 956
          },
          {
            "ratingClass": 1,
            "chartDesigner": "迷路第二層",
            "jacketDesigner": "シエラ",
            "jacketOverride": false,
            "realrating": 93,
            "totalNotes": 1194
          },
          {
            "ratingClass": 2,
            "chartDesigner": "迷路深層",
            "jacketDesigner": "シエラ",
            "jacketOverride": false,
            "realrating": 113,
            "totalNotes": 1450
          }
        ]
      },
      {
        "id": "tempestissimo",
        "title_localized": {
          "en": "Tempestissimo"
        },
        "artist": "t+pazolite",
        "bpm": "231",
        "bpm_base": 231.0,
        "set": "vs",
        "set_friendly": "Black Fate",
        "world_unlock": false,
        "remote_dl": true,
        "side": 1,
        "time": 137,
        "date": 1590537605,
        "version": "3.0",
        "difficulties": [
          {
            "ratingClass": 0,
            "chartDesigner": "Prelude - Ouverture",
            "jacketDesigner": "シエラ",
            "jacketOverride": false,
            "realrating": 65,
            "totalNotes": 919
          },
          {
            "ratingClass": 1,
            "chartDesigner": "Convergence - Intermezzo",
            "jacketDesigner": "シエラ",
            "jacketOverride": false,
            "realrating": 95,
            "totalNotes": 1034
          },
          {
            "ratingClass": 2,
            "chartDesigner": "Onslaught - Crescendo",
            "jacketDesigner": "シエラ",
            "jacketOverride": false,
            "realrating": 106,
            "totalNotes": 1254
          },
          {
            "ratingClass": 3,
            "chartDesigner": "Finale - The Tempest",
            "jacketDesigner": "シエラ",
            "jacketOverride": true,
            "realrating": 115,
            "totalNotes": 1540
          }
        ]
      }
    ],
    "recent_score": {
      "user_id": 4,
      "score": 9979350,
      "health": 100,
      "rating": 11.59675,
      "song_id": "melodyoflove",
      "modifier": 0,
      "difficulty": 2,
      "clear_type": 1,
      "best_clear_type": 3,
      "time_played": 1647570474485,
      "near_count": 2,
      "miss_count": 1,
      "perfect_count": 928,
      "shiny_perfect_count": 833
    },
    "recent_songinfo": {
      "id": "melodyoflove",
      "title_localized": {
        "en": "A Wandering Melody of Love",
        "ja": "迷える音色は恋の唄"
      },
      "artist": "からとPαnchii少年 feat.はるの",
      "bpm": "165",
      "bpm_base": 165.0,
      "set": "omatsuri",
      "set_friendly": "Sunset Radiance",
      "world_unlock": false,
      "remote_dl": true,
      "side": 0,
      "time": 134,
      "date": 1566432002,
      "version": "2.3",
      "difficulties": [
        {
          "ratingClass": 0,
          "chartDesigner": "恋のToaster",
          "jacketDesigner": "シエラ",
          "jacketOverride": false,
          "realrating": 35,
          "totalNotes": 422
        },
        {
          "ratingClass": 1,
          "chartDesigner": "恋のToaster",
          "jacketDesigner": "シエラ",
          "jacketOverride": false,
          "realrating": 75,
          "totalNotes": 670
        },
        {
          "ratingClass": 2,
          "chartDesigner": "恋のToaster",
          "jacketDesigner": "シエラ",
          "jacketOverride": false,
          "realrating": 97,
          "totalNotes": 931
        }
      ]
    }
  }
}
```
