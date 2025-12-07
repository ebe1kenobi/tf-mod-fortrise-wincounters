2025/12/07 : New format json for saved stat and new stat 

```
"{
  ""todayWin"": {
    ""P2"": 2
  },
  ""totalWin"": {
    ""P2"": 2
  },
  ""today"": {
    ""P1"": {
      ""win"": 0,
      ""kill"": 0,
      ""death"": 2,
      ""self"": 0,
      ""killFrom"": {
        ""Arrow"": 2
      },
      ""killBy"": {
        ""P2"": 2
      }
    },
    ""P2"": {
      ""win"": 2,
      ""kill"": 2,
      ""death"": 0,
      ""self"": 0,
      ""killFrom"": {},
      ""killBy"": {}
    }
  },
  ""total"": {
    ""P1"": {
      ""win"": 0,
      ""kill"": 0,
      ""death"": 2,
      ""self"": 0,
      ""killFrom"": {
        ""Arrow"": 2
      },
      ""killBy"": {
        ""P2"": 2
      }
    },
    ""P2"": {
      ""win"": 2,
      ""kill"": 2,
      ""death"": 0,
      ""self"": 0,
      ""killFrom"": {},
      ""killBy"": {}
    }
  },
  ""date"": ""2025-12-07-18""
}"
```

2025/10/20 : Need Mod "Custom Name" https://github.com/ebe1kenobi/tf-mod-fortrise-custom-name, it will now save the stat with the name instead of the color of the player. So the player can change his archer when he wants. 


Fork of the wincounters mod in the port of Bartizan Mod in fortrise https://github.com/FortRise/ExampleFortRiseMod/blob/main/BartizanMod/Core/Other.cs

2 counters, one for the current party, and another with the total of each party throw the years

![image](https://github.com/user-attachments/assets/6ae4990b-bf7d-4e4f-aa46-a0811597e28e)


In the bartizan mod, the counter reset each time we change the match settings, the players ...

* WiderSetMod supported

The total victories of each player will be displayed in the gem on the versus match result screen

Settings

![image](https://github.com/user-attachments/assets/054d5765-06ec-498a-b768-869192091d28)

the team parameter is used to save the stat in different file when we played with different team.

The stat are saved in the TowerFall directory 

![image](https://github.com/user-attachments/assets/c8487c59-1a5c-48a0-b471-631c36ad5c61)

The Use online Stat save the data online on a spreadsheet (you need to create and configure it)

1. create the spreadsheet (3 column id	date value, no data!)
2. ![image](https://github.com/user-attachments/assets/370ad798-1742-4b5e-9b78-9c3038c0b155)
3. create the AppsScript (copy the content of script/appscript.js)
4. ![image](https://github.com/user-attachments/assets/094ade8f-5878-42f5-8305-768c38bb848f)
5. create the file settings.json with the url of the script deployed in the "TowerFall\Mods\tf-mod-fortrise-wincounters" directory

settings.json

```
{
    "appliWebUrl": "https://script.google.com/macros/s/---yoursscript-----/exec?id=[#ID#]&date=[#DATE#]"
}
```



