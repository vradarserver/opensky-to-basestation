# opensky-to-basestation
A command-line utility that fetches state vectors from the Open Sky network and converts
them into a BaseStation format network feed.

## Command-line parameters

**Usage**: \<command> [options]

### Commands

| Command            | Description |
| -                  | -           |
| ```-rebroadcast``` | Periodically fetches OpenSky state and rebroadcasts it in BaseStation format on the local network |

### OpenSky Options
| Option              | Value    | Description |
| -                   | -        | -           |
| ```-user```         | text     | OpenSky network username (defaults to anonymous access) |
| ```-password```     | text     | OpenSky network password |
| ```-anonRootUrl```  | url      | Root URL for anonymous OpenSky API calls (defaults to ```https://opensky-network.org/api```) |
| ```-userRootUrl```  | url      | Root URL for logged-in OpenSky API calls (defaults to ```https://{user}:{password}@opensky-network.org/api```) |
| ```-anonInterval``` | seconds  | Seconds between fetches for anonymous users (defaults to ```10```) |
| ```-userInterval``` | seconds  | Seconds between fetches for logged-in users (defaults to ```5```) |
| ```-icao24```       | hex-list | Hyphen-separated ICAOs to fetch from OpenSky (defaults to all aircraft) |
| ```-lamin```        | degrees  | Lower bound for latitude (defaults to no bounding box) |
| ```-lamax```        | degrees  | Upper bound for latitude |
| ```-lomin```        | degrees  | Lower bound for longitude |
| ```-lomax```        | degrees  | Upper bound for longitude |

### Network Server Options

| Option      | Value      | Description |
| -           | -          | -           |
| ```-port``` | 1024-65535 | The port to listen to for incoming connections (defaults to ```20003```) |

### Diagnostic Options
| Option              | Value    | Description |
| -                   | -        | -           |
| ```-jsonFileName``` | filename | The full path to a file that the OpenSky JSON will be saved to |
