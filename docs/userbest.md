## user/best

| arguments    | description                                                                | optional                                        |
|:-------------|:---------------------------------------------------------------------------|-------------------------------------------------|
| user         | user name or 9-digit user code                                             | true when usercode is not null, otherwise false |
| usercode     | 9-digit user code                                                          | true when user is not null, otherwise false     |
| songname     | any song name for fuzzy querying                                           | true when songid is not null, otherwise false   |
| songid       | sid in Arcaea songlist                                                     | true when songname is not null, otherwise false |
| difficulty   | accept format are 0/1/2/3 or pst/prs/ftr/byn or past/present/future/beyond | false                                           |
| withrecent   | boolean. if true, will reply with recent_score                             | true                                            |
| withsonginfo | boolean. if true, will reply with songinfo                                 | true                                            |

#### Example

+ `{apiurl}/botarcapi/user/best?user=ToasterKoishi&songid=ifi&difficulty=ftr&withrecent=true&withsonginfo=true`

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
    "record": {
      "score": 9979257,
      "health": 100,
      "rating": 12.796285000000001,
      "song_id": "ifi",
      "modifier": 0,
      "difficulty": 2,
      "clear_type": 1,
      "best_clear_type": 5,
      "time_played": 1598919831344,
      "near_count": 5,
      "miss_count": 1,
      "perfect_count": 1570,
      "shiny_perfect_count": 1466
    },
    "songinfo": [
      {
        "id": "ifi",
        "title_localized": {
          "en": "#1f1e33"
        },
        "artist": "かめりあ(EDP)",
        "bpm": "181",
        "bpm_base": 181.0,
        "set": "vs",
        "set_friendly": "Black Fate",
        "world_unlock": false,
        "remote_dl": true,
        "side": 1,
        "time": 163,
        "date": 1590537604,
        "version": "3.0",
        "difficulties": [
          {
            "ratingClass": 0,
            "chartDesigner": "夜浪",
            "jacketDesigner": "望月けい",
            "jacketOverride": false,
            "realrating": 55,
            "totalNotes": 765
          },
          {
            "ratingClass": 1,
            "chartDesigner": "夜浪",
            "jacketDesigner": "望月けい",
            "jacketOverride": false,
            "realrating": 92,
            "totalNotes": 1144
          },
          {
            "ratingClass": 2,
            "chartDesigner": "夜浪 VS 東星 \"Convergence\"",
            "jacketDesigner": "望月けい",
            "jacketOverride": false,
            "realrating": 109,
            "totalNotes": 1576
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

