<div align="center">
  <img src="assets/screenshot_main.png" width="300" alt="Lenovo Legion Toolkit Logo" />
  
  <h1>Lenovo Legion Toolkit</h1>
  
  <p>
    <a href="https://github.com/BartoszCichecki/LenovoLegionToolkit/actions/workflows/build.yml"><img src="https://github.com/BartoszCichecki/LenovoLegionToolkit/actions/workflows/build.yml/badge.svg?branch=master" alt="Build Status"></a>
    <a href="https://crowdin.com/project/llt"><img src="https://badges.crowdin.net/llt/localized.svg" alt="Crowdin"></a>
    <a href="https://discord.com/invite/legionseries"><img src="https://img.shields.io/discord/761178912230473768?label=Legion%20Series%20Discord" alt="Discord"></a>
  </p>
  <p>
    <b>Advanced control for Lenovo Legion, LOQ, and select IdeaPad Gaming laptops.<br>No telemetry. No bloat. Free and open source.</b>
  </p>
  <p>
    <b>Project expanded and maintained by <a href="https://github.com/jayclatty-boop">Jaycc Media</a></b>
  </p>
</div>

---

<p align="center">
  <a href="#features">Features</a> •
  <a href="#planned--future-features">Planned</a> •
  <a href="#download">Download</a> •
  <a href="#compatibility">Compatibility</a> •
  <a href="#faq">FAQ</a> •
  <a href="#contribution">Contribute</a>
  • <a href="https://github.com/jayclatty-boop/loq-toolkit/releases/latest">Jaycc Media Releases</a>
</p>

---

> **Archived on July 24, 2025**  
> This project is no longer actively maintained by the original author. It is now expanded and maintained by <b><a href="https://github.com/jayclatty-boop">Jaycc Media</a></b>. Thank you to everyone who supported, used, and contributed! Feel free to fork and continue development.

---

Lenovo Legion Toolkit (LLT) is a lightweight utility for Lenovo Legion (and similar) series laptops, providing access to select hardware features that are otherwise only available through Lenovo Vantage or Legion Zone. No background services, minimal resources, and no telemetry. Windows only.

---

## 🚀 Features

- Change power mode and battery charging mode (where supported)
- Support for Spectrum RGB, 4-zone RGB, and white backlight keyboards (on compatible models)
- Monitor dGPU activity (NVIDIA only)
- Define Actions to automate tasks (e.g., on AC power connect)
- View battery statistics
- Disable/enable Lenovo Vantage, Legion Zone, and Lenovo Hotkeys service (without uninstalling)

---

## 🛠️ Planned & Future Features

The following features are planned for future releases:

- Improved fan curve editor for custom cooling profiles
- Enhanced battery health and cycle reporting
- More granular RGB lighting effects and profiles
- Support for additional Lenovo/IdeaPad/LOQ models
- System notifications for thermal or battery events
- Export/import of user settings and profiles
- Optional dark/light theme toggle
- Integration with Windows notifications
- More CLI commands and automation options
- Community-driven plugin or extension support

*Have a feature request? Open an issue or join the community discussion!*

---

## 📦 Download

**Latest Release:** [Jaycc Media Releases](https://github.com/jayclatty-boop/loq-toolkit/releases/latest)  
**Original Releases:** [Releases page](https://github.com/BartoszCichecki/LenovoLegionToolkit/releases/latest)

**Install via [winget](https://github.com/microsoft/winget-cli):**

```sh
winget install JayccMedia.LOQToolkit
```

**Install via [Scoop](https://scoop.sh):**

```sh
scoop bucket add jayccmedia https://github.com/jayclatty-boop/scoop-bucket.git
scoop install jayccmedia/loqtoolkit
```

> **Tip:** Looking for a Vantage alternative for Linux? Check out [LenovoLegionLinux](https://github.com/johnfanv2/LenovoLegionLinux).

---

## 💻 Compatibility
LLT is made for Lenovo Legion laptops, and similar models like Ideapad Gaming, LOQ, and their Chinese variants.

- Supported: Generations 6 (MY2021), 7 (MY2022), 8 (MY2023), 9 (MY2024)
- Partial: Some features may work on Gen 5 (MY2020)
- Not supported: Devices older than Gen 6 or non-Legion models

If you get an incompatible message, see the *Contribution* section below to help expand support.

---

## ❤️ Donate
If you enjoy using Lenovo Legion Toolkit, consider supporting development:

[Donate with PayPal](https://www.paypal.com/donate/?hosted_button_id=22AZE2NBP3HTL)

<img src="LenovoLegionToolkit.WPF/Assets/Donate/paypal_qr.png" width="200" alt="PayPal QR code" />

---

## 🙏 Credits
Special thanks to:

- [ViRb3](https://github.com/ViRb3), for [Lenovo Controller](https://github.com/ViRb3/LenovoController)
- [falahati](https://github.com/falahati), for [NvAPIWrapper](https://github.com/falahati/NvAPIWrapper) and [WindowsDisplayAPI](https://github.com/falahati/WindowsDisplayAPI)
- [SmokelessCPU](https://github.com/SmokelessCPU), for 4-zone RGB and Spectrum keyboard support
- [Mario Bălănică](https://github.com/mariobalanica), [Ace-Radom](https://github.com/Ace-Radom), and all contributors

**Project expansion, maintenance, and new releases by [Jaycc Media](https://github.com/jayclatty-boop)**

Translations by:
- Bulgarian: [Ekscentricitet](https://github.com/Ekscentricitet)
- Chinese (Simplified): [凌卡Karl](https://github.com/KarlLee830), [Ace-Radom](https://github.com/Ace-Radom)
- Chinese (Traditional): [flandretw](https://github.com/flandretw)
- Dutch: Melm, [JarneStaalPXL](https://github.com/JarneStaalPXL)
- French: EliotAku, [Georges de Massol](https://github.com/jojo2massol), Rigbone, ZeroDegree
- German: Sko-Inductor, Running_Dead89
- Greek: GreatApo
- Italian: [Lampadina17](https://github.com/Lampadina17)
- Karakalpak: KarLin, Gulnaz, Niyazbek Tolibaev, Shingis Joldasbaev
- Latvian: RJSkudra
- Romanian: [Mario Bălănică](https://github.com/mariobalanica)
- Slovak: Mitschud, Newbie414
- Spanish: M.A.G.
- Polish: Mariusz Dziemianowicz
- Portugese: dvsilva
- Portuguese (Brasil): Vernon
- Russian: [Edward Johan](https://github.com/younyokel)
- Turkish: Undervolt
- Ukrainian: [Vladyslav Prydatko](https://github.com/va1dee), [Dmytro Zozulia](https://github.com/Nollasko)
- Vietnamese: Not_Nhan, Kuri, Nagidrop

Thanks to everyone who monitors and corrects translations!

---

## ❓ FAQ
---

## 🤝 Contribution
Feedback and pull requests are welcome! Please check out [CONTRIBUTING.md](CONTRIBUTING.md) before submitting.

---

<div align="center">
  <sub>Not affiliated with Lenovo. For Legion, LOQ, and select IdeaPad Gaming laptops only.</sub>
  <br><b>Maintained and expanded by <a href="https://github.com/jayclatty-boop">Jaycc Media</a></b>
</div>
