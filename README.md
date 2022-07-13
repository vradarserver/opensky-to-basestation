# opensky-to-basestation
A command-line utility that fetches state vectors from the Open Sky network and converts
them into a BaseStation format network feed.

# OpenSky Rate Limits

As of July 2022 OpenSky have introduced rate limits that make it useless for aircraft tracking programs
such as Virtual Radar Server. Anonymous users are now limited to 8 minutes of tracking calls and registered
users get just under an hour and a half of tracking calls per day.

OpenSky is no longer a viable source of ADSB data.

## How to install

1. Download the .NET Core 3.1 runtime for your platform:

   https://dotnet.microsoft.com/download/dotnet-core/3.1

   Choose the **.NET CORE RUNTIME** option. You don't need the SDK version unless you intend building the program.

2. Either download the pre-compiled binaries from the releases page or compile your own build:

   https://github.com/vradarserver/opensky-to-basestation/releases

3. Unzip the pre-compiled binaries and copy to a folder of your choosing.

### Windows

To run the application under Windows:

```opensky-to-basestation -rebroadcast```

### Other platforms

To run the application under Linux and OSX:

```dotnet opensky-to-basestation.dll -rebroadcast```


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

| Option            | Value      | Description |
| -                 | -          | -           |
| ```-port```       | 1024-65535 | The port to listen to for incoming connections (defaults to ```20003```) |
| ```-tickleSecs``` | seconds    | Seconds between aircraft list tickles, 0 to switch off (defaults to ```25```) |

### Diagnostic Options
| Option              | Value    | Description |
| -                   | -        | -           |
| ```-jsonFileName``` | filename | The full path to a file that the OpenSky JSON will be saved to |

## Compilation Pre-requisites

You will need the latest version of Visual Studio 2019 Community edition. When you choose the components to install you need
to tick the option for **.NET desktop development**. In the **Individual Components** tab make sure that the
```.NET Core 3.1 SDK``` is ticked.

Once .NET Core 3.1 is no longer the bleeding edge you might see the option called ```.NET Core 3.1 Runtime``` instead.
