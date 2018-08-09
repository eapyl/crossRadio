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
* `-l`, `--list` - show list of radio stations;
* `-p {id}`, `--play {id}` - play radio using ID;
* `-pa`, `--pause` - pause playing;
* `-st`, `--start` - start playing after pause;
* `-vu`, `--volumeUp` - increase volume;
* `-vd`, `--volumeDown` - decrease volume;
* `-s`, `--stop` - Stop playing and exit.
* `-db {uri}`, `--database {uri}` - Change a link to database with radio stations (default - https://github.com/eapyl/radio-stations/blob/master/db.json).

All descriptions of station are hosted at [GitHub](https://github.com/eapyl/radio-stations/blob/master/db.json).


