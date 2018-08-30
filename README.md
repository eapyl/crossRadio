[![Build Status](https://travis-ci.org/eapyl/crossRadio.svg?branch=master)](https://travis-ci.org/eapyl/crossRadio)

# Dotnet global tool to listen radio

> Supported OS: Windows

To install:

```
dotnet tool install --global plr
```

Listen
* using powershell:
```
start radio
```
* using cmd
```
radio
```

Common way to start listening the first radio from the list:
```
-l
-p 1
```

Supported commands:
* `{no command}` - Show status of radio;
* `-h`, `--help` - Show descriptopn of all commands;
* `-l`, `--list` - Show list of radio stations;
* `-p {id}`, `--play {id}` - Play selected station using ID of station;
* `-pa`, `--pause` - Pause playing;
* `-st`, `--start` - Start playing after pause;
* `-vu [delta]`, `--volumeUp [delta]` - Increase volume by default value or defined by [delta];
* `-vd [delta]`, `--volumeDown [delta]` - Decrease volume by default value or defined by [delta];
* `-s`, `--stop` - Stop playing and exit;
* `-db {uri}`, `--database {uri}` - Change a link to database with radio stations (default - https://github.com/eapyl/radio-stations/blob/master/db.json).

All descriptions of station are hosted at [GitHub](https://github.com/eapyl/radio-stations/blob/master/db.json).


