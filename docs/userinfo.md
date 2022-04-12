## user/info

| arguments    | description                                                     | optional                                        |
|:-------------|:----------------------------------------------------------------|-------------------------------------------------|
| user         | user name or 9-digit user code                                  | true when usercode is not null, otherwise false |
| usercode     | 9-digit user code                                               | true when user is not null, otherwise false     |
| recent       | number, range 0-7. The number of recently played songs expected | true                                            |
| withsonginfo | boolean. if true, will reply with songinfo                      | true                                            |

#### Example

+ `{apiurl}/botarcapi/user/info?user=ToasterKoishi&recent=2&withsonginfo=true`

###### Return data

```json
{
  "status": 0,
  "content": {
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
    "recent_score": [
      {
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
      {
        "score": 9893013,
        "health": 100,
        "rating": 12.465065,
        "song_id": "aegleseeker",
        "modifier": 0,
        "difficulty": 2,
        "clear_type": 1,
        "best_clear_type": 5,
        "time_played": 1646960716486,
        "near_count": 16,
        "miss_count": 9,
        "perfect_count": 1543,
        "shiny_perfect_count": 1432
      }
    ],
    "songinfo": [
      {
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
      },
      {
        "id": "aegleseeker",
        "title_localized": {
          "en": "Aegleseeker"
        },
        "artist": "Silentroom vs Frums",
        "bpm": "234",
        "bpm_base": 234.0,
        "set": "observer",
        "set_friendly": "Esoteric Order",
        "world_unlock": false,
        "remote_dl": true,
        "side": 0,
        "time": 147,
        "date": 1620691204,
        "version": "3.6",
        "difficulties": [
          {
            "ratingClass": 0,
            "chartDesigner": "⟨RECORD START⟩ 0:",
            "jacketDesigner": "シエラ",
            "jacketOverride": false,
            "realrating": 55,
            "totalNotes": 973
          },
          {
            "ratingClass": 1,
            "chartDesigner": "⟨OBSRV⟩ boundary(?°a,?°A)",
            "jacketDesigner": "シエラ",
            "jacketOverride": false,
            "realrating": 88,
            "totalNotes": 1235
          },
          {
            "ratingClass": 2,
            "chartDesigner": "L₆ː The Void",
            "jacketDesigner": "シエラ",
            "jacketOverride": false,
            "realrating": 110,
            "totalNotes": 1568
          }
        ]
      }
    ]
  }
}
```
