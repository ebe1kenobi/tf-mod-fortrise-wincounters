# WinCounters
<img width="1074" height="670" alt="image" src="https://github.com/user-attachments/assets/af27e9d0-f4f2-4d5a-be63-4b0702fc9d8d" />

Tracks wins and per-player stats across a game night: wins today, all-time total,
kills, deaths, self-kills, and who killed whom. Results are written to one file per
day and per game mode, and can be synced with a web app.

A mod for **FortRise 5** (>= 5.3.3). The FortRise 4 version (`tf-mod-fortrise-wincounters`) is no longer maintained: fixes and new features only land in this repository.

## Installation

1. Install FortRise 5 and start the game through `FortRise.exe`.
2. Copy `release/wincounters` (or the shipped folder) into `<TowerFall>/FortRise/Mods/`.

**No mod is required.** The **Archer** mod is used if it is there, to put real player
names on the stats instead of `P1`..`P8` — it took that job over from CustomName, which
this mod no longer looks for. Without Archer everything still works; the keys are just
`P1`, `P2`, and so on.

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

<img width="700" height="338" alt="image" src="https://github.com/user-attachments/assets/599a3d1e-ef73-4095-873a-5d72ee8cb522" />

| Setting | Purpose |
|---------|---------|
| Enable | turn counting on |
| Display total win after today win | show the all-time total next to today's wins |
| Reset today counter | reset today's counters (handy when a player joins mid-evening) |
| Use Online stat (need a config file) | also keep the stats in a Google spreadsheet |

## Online stats

Off by default, and **entirely optional**: with it off, everything above works from the
local files alone. Turned on, the same figures are additionally read from and written to
a Google spreadsheet of your own — which is what lets two people on two machines share
one set of counters.

You supply the spreadsheet: nothing is hosted for you, and no data leaves the machine
until you have set one up.

**1. Create the spreadsheet.** Three columns — `id`, `date`, `value` — and no rows. The
mod fills it.

<img width="345" height="472" alt="image" src="https://github.com/user-attachments/assets/e106e04c-13ae-4b5a-94d5-ac40dd820d1f" />

**2. Add the Apps Script.** In the spreadsheet, `Extensions > Apps Script`, and paste the
contents of [script/appscript.js](script/appscript.js). Deploy it as a **web app**, and
copy the deployment URL.

<img width="538" height="305" alt="image" src="https://github.com/user-attachments/assets/f3fa62ec-81d6-4fb0-a5b6-fc6d10e1bd28" />

**3. Point the mod at it.** Edit `settings.json` in
`<TowerFall>/FortRise/Mods/tf-mod-fortrise-wincounters/`, keeping the two placeholders
— the mod replaces `[#ID#]` and `[#DATE#]` on every call:

```json
{
    "appliWebUrl": "https://script.google.com/macros/s/---your-script---/exec?id=[#ID#]&date=[#DATE#]"
}
```

**4. Tick `Use Online stat`** in the mod settings.

### What happens when it goes wrong

**Nothing fatal.** An earlier version of this page warned that the game would crash with
the setting on and no URL configured; it does not, and never should — every path is
guarded:

| Situation | What the mod does |
|-----------|-------------------|
| `settings.json` missing, unreadable, or with no URL | logs one line, skips the network entirely, saves locally |
| Server unreachable, or silent | gives up after **15 seconds**, saves locally |
| Reply unusable | same, and the popup shows `ERROR - STATS / NOT LOADED` |

The local save is the one that always happens: the online copy is a mirror, never the
source of truth. The counters you see on screen come from the file on disk.

The fifteen seconds are worth knowing about. The call is made on the game thread when a
versus match starts, so the game is **frozen** while it waits. Fifteen is already a
compromise: `HttpWebRequest` defaults to a hundred seconds, and an unreachable server
used to lock the game up for that long.

## Build / deployment

| Script | Purpose |
|--------|---------|
| `script/release.bat` | build, then assemble into `release/` |
| `script/deploy.bat` | copy `release/` into the TowerFall `Mods` folder |
| `script/release_deploy.bat` | both, one after the other |

Paths (game folder, module name) are set in `script/config.bat`.
