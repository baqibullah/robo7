# RoboLink

Connection to BC ROBO 7 device via smbclient.

## Project layout

- `RoboLink.Core`: Pure logic. File names, file bodies, validation.
- `RoboLink.Web`: Web UI, database, and the background service that does the handshake.
- `RoboLink.Tests`: Unit tests for Core.

## Files Structure

- `TubeOrder.cs`: Record holding everything together about one tube.
- `DatBody.cs`: One per tube and it has two parts, name and a body.
  - `DatFileName.cs`: Builds a long comma separated string that describes the DAT filename.
  - `DatBody.cs`: DatBody + LabelTemplate, the label layout: STR/BAR/LIN/BOX these lines describe what to print and where.
- `ControlFileName.cs`: One record for the shared part of the DEF/OK/NG names (host, robo, month, day, tray). It builds the DEF
  name, parses OK/NG names, and `From(order)` lets us match a reply to its order.

## Run

```shell
dotnet build
dotnet test
```
