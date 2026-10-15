# RoboLink

Connection to BC ROBO 7 device via smbclient.

## File formats

### DAT file name

All the order data is in the name. Parts are comma separated, in this order:

| #   | Part          | Notes                           |
| --- | ------------- | ------------------------------- |
| 1   | Host terminal | 4 digits, zero padded           |
| 2   | ROBO terminal | 4 digits, zero padded           |
| 3   | Date          | MMDD                            |
| 4   | Tray no.      | 4 digits, zero padded           |
| 5   | Last tube     | `0` = not last, `9` = last      |
| 6   | Seq no.       | 2 digits, 01 to 99              |
| 7   | Queue no.     | 4 digits, or 4 spaces if unused |
| 8   | Patient ID    | up to 20 bytes                  |
| 9   | Patient name  | up to 20 bytes                  |
| 10  | Container     | up to 20 bytes                  |
| 11  | Volume        | `XXX.X`, one decimal            |
| 12  | Department    | up to 20 bytes                  |
| 13  | Label count   | total tubes + manual labels     |
| 14  | Stocker code  | 01 to 06, or 99 = manual label  |
| 15  | RFID          | up to 15 bytes, empty if unused |
| 16  | Priority      | always `0`                      |

Example:
`0005,0001,1126,0032,0,01,1234,0123456789,Tiger Woods,HbA1c,1.0,Pediatrics,2,99,,0.dat`

### DAT file body

The label layout. One line per item, each line ends with CR:

- `STR|x^y^font^size^text^rotating^angle^reverse^bold|`
- `BAR|x^y^height^type^codetype^narrow^wide^checkdigit^direction^data|`
- `LIN|...`
- `BOX|x1^y1^x2^y2^type^fill|`

Not allowed in any value: `,` `|` `^`, CR, and the words `STR|`, `BAR|`, `LIN|`, `BOX|`.

### Control files

`DEF`, `OK` and `NG` files are 0 bytes. Only the name matters:
`host,robo,MMDD,tray,DEF.TXT` (reply: `OK.txt` or `NG.txt`). The type is matched case-insensitively because the manual writes `DEF.TXT` but `OK.txt`.

## Found on the real machine

These come from testing, not the manual:

- The working vendor sample uses **CR-only** line endings, **no BOM**, and **no trailing newline**. The manual says UTF-8 with BOM.
- Nothing happens until Initialize is pressed in the ROBO app.
- The full handshake worked by hand with `smbclient`.

## Project layout

- `RoboLink.Core`: pure logic. File names, file bodies, validation. No files, network or database.
- `RoboLink.Web`: web UI, database, and the background service that does the handshake (not built yet).
- `RoboLink.Tests`: unit tests for Core.

## Run

```shell
dotnet build
dotnet test
```
