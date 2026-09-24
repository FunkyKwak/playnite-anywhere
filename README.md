# Playnite Anywhere <img align="left" width="130" height="80" src="/docs/images/icon.png" alt="Extension icon">

<br/>

Access your Playnite library from any device on your local network.

Playnite Anywhere is a Playnite extension that adds a lightweight web interface to your Playnite library. Once installed, it starts a small web server directly inside Playnite, allowing you to browse your games from a phone, tablet, laptop, or any other device connected to the same local network.

![screenshot-main](/docs/images/screenshot-main.png)

## Features

### 📚 Browse your Playnite library

View your Playnite games directly from your web browser, including:

* Game covers from your existing Playnite library
* Game names
* Favorite status
* Game source
* Completion status

No external game database or cover service is required.

### 🗂️ Group your games

You can group your library by:

* **None** — display all games in a single grid
* **Source** — group games by their Playnite source
* **Progress** — group games by completion status
* **Platform** — group games by platform

Groups can be collapsed and expanded.

Your grouping preference and the collapsed/expanded state of each group are saved automatically.

### 📱 Use it from any device

Open Playnite Anywhere from any browser on your local network:

```text
http://YOUR-PC-IP:32650
```

For example:

```text
http://192.168.1.10:32650
```

No additional web server, database, Docker container, or other software is required.

## Installation

1. Download the latest `.pext` release.
2. Open Playnite.
3. Go to **Add-ons → Add-ons menu → Install extension**.
4. Select the downloaded `.pext` file.
5. Restart Playnite if required.

Once Playnite is running, Playnite Anywhere automatically starts its web server.

## Getting started

Make sure Playnite is running.

Find the local IP address of the computer running Playnite. On Windows, you can find it with:

```text
ipconfig
```

Look for the **IPv4 Address** of your local network adapter.

Then open the following address from another device connected to the same network:

```text
http://YOUR-PC-IP:32650
```

For example:

```text
http://192.168.1.10:32650
```

You can also open:

```text
http://localhost:32650
```

directly on the computer running Playnite.

## Requirements

Playnite must be running for Playnite Anywhere to be accessible.

## Security & network access

Playnite Anywhere 0.1 is designed for local network use.

The web server listens on the computer's network interfaces, so other devices on the same LAN can access it.

This initial release does not provide:

Authentication
HTTPS
Internet/external access protection
User accounts

For this reason, **do not expose port 32650 directly to the Internet**.

## Current limitations

Version 0.1 is intentionally focused on browsing your library.

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

These may be considered for future releases. **Do not hesitate to submit feature request via GitHub issue, by now I'm the only user so I won't do much if not requested.**

## Troubleshooting
### The page does not load

Make sure:

Playnite is running.
Playnite Anywhere is installed and enabled.
You are using the correct IP address of the computer running Playnite.
Port 32650 is not blocked by Windows Firewall.
Both devices are connected to the same local network.

Try accessing:

http://localhost:32650

on the Playnite computer first.

If this works but another device cannot connect, the issue is likely related to the local network or Windows Firewall.

### Some covers are missing

Playnite Anywhere uses the covers already stored by Playnite.

If a game does not have a cover in Playnite, Playnite Anywhere cannot display one.

