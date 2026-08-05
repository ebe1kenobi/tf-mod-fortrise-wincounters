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
| **Y** (Alt2) on the end-of-match screen | open the detailed stats popup |
| A / B | close the popup |

The popup lists, per player: wins, kills, deaths, self-kills, plus who killed them
(KILL BY) and who they killed (KILL FROM).

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

`settings.json`, shipped with the mod, holds the web app URL. Network calls have a
5 second guard: if the server is unreachable the game carries on and the local save
still happens.

## Build / deployment

| Script | Purpose |
|--------|---------|
| `script/release.bat` | build, then assemble into `release/` |
| `script/deploy.bat` | copy `release/` into the TowerFall `Mods` folder |
| `script/release_deploy.bat` | both, one after the other |

Paths (game folder, module name) are set in `script/config.bat`.
