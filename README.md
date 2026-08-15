# WinCounters

Tracks wins and per-player stats across a game night: wins today, all-time total,
kills, deaths, self-kills, and who killed whom. Results are written to one file per
day and per game mode, and can be synced with a web app.

A mod for **FortRise 5** (>= 5.3.3). The FortRise 4 version (`tf-mod-fortrise-wincounters`) is no longer maintained: fixes and new features only land in this repository.

## Installation

1. Install FortRise 5 and start the game through `FortRise.exe`.
2. Install the mods this one depends on first: **CustomName**.
3. Copy `release/wincounters` (or the shipped folder) into `<TowerFall>/FortRise/Mods/`.

Settings are under **Options > Mods > WinCounters**.
Data and log files live in `<TowerFall>/FortRise/Saves/WinCounters/` and `<TowerFall>/FortRise/Logs/`.

## Usage

Counters are drawn on the end-of-match screen, under each archer.

| Input | Effect |
|-------|--------|
| **Left upper shoulder** (Alt2) on the end-of-match screen | open the detailed stats popup |
| Up / Down | scroll the table |
| A / B | close the popup |

The popup lists, per player: wins, kills, deaths, self-kills, plus who killed them
(`BY`) and who they killed (`VS`). Each figure reads `3(21)`: the night, then the
all-time total in brackets.

### Why the table scrolls

A player is not a line. One who was killed by three different people takes four, and
eight players with a full history overflowed the panel - the extra lines were drawn
outside it and wandered across the screen, because Monocle does not clip.

The lines are therefore **built once** when the popup opens, into a list, and drawing
is only a window sliding over it. That is what makes the height knowable before
anything is drawn, and scrolling is just a matter of moving the window. The earlier
version laid itself out *while* rendering, stacking Y offsets as it went, so nothing
knew how tall the table was until it was already too late.

The scrollbar on the right appears only when there is something further down.

Two distinct messages can replace the table:

- `NO STATS FOUND / FOR THOSE PLAYERS`: nothing recorded yet for this team and this
  mode. That is the normal case for a first game.
- `ERROR - STATS / NOT LOADED`: the server is unreachable or the reply is unusable.
  Counters then start from zero - a banner also says so at the bottom of the screen
  for a few seconds.

## Files produced

One file per day, per team **and per game mode**:

```
<TowerFall>/FortRise/Saves/WinCounters/2026-08-05-Respawn-DAVID-ERIC-wincounters.json
```

The mode comes first in the name, so switching mode starts from fresh counters,
which avoids mixing stats from two different rule sets. Modes added by a mod show up
under their real name (`Respawn`, `PlayTag`...) rather than a number.

Contents (format `v4`): `version`, `date`, `mode`, `matchsResults` (the final score
of every match of the day), `todayWin` / `totalWin`, and `today` / `total` with the
per-player breakdown.

## Settings

| Setting | Purpose |
|---------|---------|
| Enable | turn counting on |
| Display total win after today win | show the all-time total next to today's wins |
| Reset today counter | reset today's counters (handy when a player joins mid-evening) |
| Use Online stat (need a config file) | sync with the web app |

The game will crash if online is set to true and no serveur settings or url in settings.json.

`settings.json`, shipped with the mod, holds the web app URL. Network calls have a
5 second guard: if the server is unreachable the game carries on and the local save
still happens.

## Online Settings

The Use online Stat save the data online on a spreadsheet (you need to create and configure it)

create the spreadsheet (3 column id date value, no data!)
<img width="345" height="472" alt="image" src="https://github.com/user-attachments/assets/e106e04c-13ae-4b5a-94d5-ac40dd820d1f" />

create the AppsScript (copy the content of script/appscript.js)
<img width="538" height="305" alt="image" src="https://github.com/user-attachments/assets/f3fa62ec-81d6-4fb0-a5b6-fc6d10e1bd28" />

create the file settings.json with the url of the script deployed in the "TowerFall\Mods\tf-mod-fortrise-wincounters" directory

settings.json

{
    "appliWebUrl": "https://script.google.com/macros/s/---yoursscript-----/exec?id=[#ID#]&date=[#DATE#]"
}


## Build / deployment

| Script | Purpose |
|--------|---------|
| `script/release.bat` | build, then assemble into `release/` |
| `script/deploy.bat` | copy `release/` into the TowerFall `Mods` folder |
| `script/release_deploy.bat` | both, one after the other |

Paths (game folder, module name) are set in `script/config.bat`.
