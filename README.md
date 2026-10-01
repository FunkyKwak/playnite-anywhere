# Playnite Anywhere <img align="left" width="130" height="80" src="/docs/images/icon.png" alt="Extension icon">

<br/>

Access your Playnite library from any device on your local network.

> [!WARNING]
> The functionalities are currently very limited, but it suits my own needs.
> Anyway I would be happy to add new functionalities for anyone need, don't hesitate to submit a feature request in a [new issue](https://github.com/FunkyKwak/playnite-anywhere/issues/new).


Playnite Anywhere is a Playnite extension that adds a lightweight web interface to your Playnite library. Once installed, it starts a small web server directly inside Playnite, allowing you to browse your games from a phone, tablet, laptop, or any other device connected to the same local network.

![screenshot-main](/docs/images/screenshot-main.png)

## Features

### ✓ Browse your Playnite library

View your Playnite games directly from your web browser, including:

* Game covers from your existing Playnite library
* Game names
* Favorite status
* Game source
* Completion status

No external game database or cover service is required.

### ✓ Group your games

You can group your library by:

* **None** — display all games in a single grid
* **Source** — group games by their Playnite source
* **Progress** — group games by completion status
* **Platform** — group games by platform

Groups can be collapsed and expanded.

Your grouping preference and the collapsed/expanded state of each group are saved automatically.

### ✓ Use it from any device

Open Playnite Anywhere from any browser on your local network:

```text
http://YOUR-PC-IP:32650
```

### ✓ Easy to setup

No additional web server, database, Docker container, or other software is required, just install the plugin and it works.


## Installation

1. Download the [latest `.pext` release](https://github.com/FunkyKwak/playnite-anywhere/releases/latest).
2. Double-clic on the `.pext` file.
5. Restart Playnite when requested.

Once Playnite is running, Playnite Anywhere automatically starts its web server. Go open it in your web browser, you can find the link in the plugin settings.


![Téléchargements GitHub](https://img.shields.io/github/downloads/FunkyKwak/playnite-anywhere/total)

### (Optional) Distant web server - Make it work even when Playnite is off
1. Create and run a container stack using the [docker-compose](/PlayniteAnywhere.Backend/docker-compose.yml) file provided here
2. In the plugin settings
    - choose "Serveur Web distant"
    - set your the url adress of your server, with port (for example `http://192.168.1.10:8080`)
3. Restart playnite

Now everytime you start Playnite, the plugin will sync the games infos to your external web server. 
Go open it in your web browser, you can find the link in the plugin settings : it's the url you just set in the above step 2.


## Requirements

Playnite must be running for Playnite Anywhere to be accessible.

Find the local IP address of the computer running Playnite. You can find the address in the plugin settings.

```text
http://YOUR-PC-IP:32650
```

Directly on the computer running Playnite, you can also open:
[http://localhost:32650](http://localhost:32650)


## Security & network access

Playnite Anywhere 0.1 is designed for local network use.

The web server listens on the computer's network interfaces, so other devices on the same LAN can access it.

This initial release does not provide:
- Authentication
- HTTPS
- Internet/external access protection
- User accounts

For this reason, **do not expose port 32650 directly to the Internet**.

## Current limitations

the current version is intentionally focused on browsing your library.

The following features are not currently available:
- Launching games remotely
- Game search
- Game details
- Screenshots
- Playtime statistics
- Advanced filtering
- Favorites filtering
- QR code generation
- Authentication
- HTTPS
- Internet access

These may be considered for future releases. **Do not hesitate to submit feature request via [a new GitHub issue](https://github.com/FunkyKwak/playnite-anywhere/issues/new), by now I'm the only user so I won't do much if not requested.**

## Troubleshooting
### The page does not load

Make sure:

- Playnite is running.
- Playnite Anywhere is installed and enabled.
- You are using the correct IP address of the computer running Playnite.
- Port 32650 is not blocked by Windows Firewall.
- Both devices are connected to the same local network.

Try accessing [http://localhost:32650](http://localhost:32650) on the Playnite computer first.

If this works but another device cannot connect, the issue is likely related to the local network or Windows Firewall.

### Some covers are missing

Playnite Anywhere uses the covers already stored by Playnite.

If a game does not have a cover in Playnite, Playnite Anywhere cannot display one.

