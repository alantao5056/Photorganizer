# Sortify — UI Design Reference

A Windows 10/11 desktop application built with C# and WinUI 3. This document describes the interface layout, component behaviour, and state interactions of the UI prototype (`photorganizer.html`).

---

## Table of Contents

1. [Layout Overview](#1-layout-overview)
2. [Title Bar](#2-title-bar)
3. [Toolbar](#3-toolbar)
4. [HEIC Warning Banner](#4-heic-warning-banner)
5. [Progress Strip](#5-progress-strip)
6. [Stats & Filter Bar](#6-stats--filter-bar)
7. [File Table](#7-file-table)
8. [Hover Preview Card](#8-hover-preview-card)
9. [Partial Organize Dialog](#9-partial-organize-dialog)
10. [Custom Format Dialog](#10-custom-format-dialog)
11. [State Lifecycle](#11-state-lifecycle)
12. [Theme System](#12-theme-system)

---

## 1. Layout Overview

The application window is a fixed **860 × 620 px** card centred on the screen, rendered with a 16 px border radius and a layered drop shadow. All regions stack vertically in a single flex column:

```
┌─────────────────────────────────────────┐
│  Title Bar                  (42 px)     │
├─────────────────────────────────────────┤
│  Toolbar                    (≈54 px)    │
├─────────────────────────────────────────┤
│  HEIC Warning Banner   (conditional)    │
├─────────────────────────────────────────┤
│  Progress Strip        (conditional)    │
├─────────────────────────────────────────┤
│  Stats & Filter Bar         (≈34 px)    │
├─────────────────────────────────────────┤
│  File Table             (flex 1, scrollable) │
└─────────────────────────────────────────┘
```

The File Table region takes all remaining height and scrolls independently. Every other region is fixed-height and non-scrollable.

---

## 2. Title Bar

A **42 px** strip that mimics the native WinUI title bar.

| Element | Description |
|---|---|
| Traffic-light dots | Three coloured circles (red / yellow / green) in the top-left, matching macOS-style window controls used as decorative chrome in the prototype |
| App title | Centred icon + "Sortify" label, rendered in a muted colour to feel like a system element |

The title bar has **no interactive application controls**. Theme switching and all other app-level controls live in regions below it, keeping the system chrome visually distinct.

---

## 3. Toolbar

The primary action bar, sitting directly below the title bar.

### Folder Picker

A full-width (flex `1`) input-like control that opens a folder-selection dialog when clicked.

- **Before selection:** shows placeholder text `"Choose a folder…"` in a muted colour.
- **After selection:** displays the resolved folder path in accent colour using a monospace font (`JetBrains Mono`), truncated with an ellipsis if too long.
- Clicking again resets the file list and triggers a new scan.

### Folder Format Dropdown

A compact dropdown (`min-width: 152 px`) that lets the user pick the subfolder naming pattern used during organisation. Available formats:

| Format | Example output |
|---|---|
| `yyyy-MM-dd` | `2024-07-15` (default) |
| `dd-MM-yyyy` | `15-07-2024` |
| `yyyyMMdd` | `20240715` |
| `yyyy.M.d` | `2024.7.15` |
| `MM-dd-yyyy` | `07-15-2024` |

The selected format is shown in the trigger. The dropdown menu closes on outside click or on item selection. The currently selected item is highlighted in accent colour with a checkmark icon.

A **Custom…** entry at the bottom of the list (separated by a top border and rendered in accent colour) opens the [Custom Format Dialog](#10-custom-format-dialog). When a custom format is active, the trigger label displays the user-supplied pattern instead of a preset name.

### Organize Button

A primary action button (`btn-primary`) that triggers the file-organization operation.

- **Disabled** when no folder has been selected.
- **Enabled** once a folder scan completes.
- If the user has filtered down to a subset of formats (i.e. not in All-mode), clicking the button opens the **Partial Organize Dialog** before proceeding.
- During an active organize run the button is disabled again until the run finishes.

---

## 4. HEIC Warning Banner

A persistent informational banner shown between the Toolbar and the Progress Strip when HEIC files are detected but the system HEIC codec is not installed.

| Element | Description |
|---|---|
| Warning icon | Amber triangle icon inside a tinted rounded square |
| Title | "HEIC Codec Not Installed" in warning colour |
| Description | Explains that capture time will fall back to the file's last-modified date |
| "Get HEIC Codec →" link | Accent-coloured inline link to the codec download |
| Dismiss button | `✕` icon in the top-right corner; clicking it hides the banner for the session (`display: none`) |

The banner uses amber/orange tones (`#CA5010` in light mode, `#f6ad55` in dark mode) to signal a non-blocking warning.

---

## 5. Progress Strip

A slim strip that appears **only during active operations** (folder scanning or file organisation). It is hidden at all other times.

```
[ spinner ]  Scanning folder…          3 / 247 files   42%
─────────────────────────────────[=====>              ]
```

| Element | Behaviour |
|---|---|
| Spinner | A CSS-animated rotating ring, always visible while the strip is shown |
| Status label | Changes between `"Scanning folder…"` and `"Organizing files…"` depending on the current operation; shows `"Done — all files organized"` on completion |
| Detail text | Shows `"N / total files"` during organisation; empty during scanning |
| Percentage | Shows `"N%"` during organisation; empty during scanning |
| Progress bar | **Indeterminate** (animated sliding fill) during scanning; **determinate** (width driven by progress %) during organisation; gradient fill from accent blue to success green |

The strip auto-hides 3.5 seconds after the organize operation reaches 100%.

---

## 6. Stats & Filter Bar

A single-line bar that is **always visible**, even before a folder is loaded. It serves two purposes: displaying scan summary statistics, and providing format filter controls for the file table.

### Filter Chips (left group)

Four format chips act as **mutually exclusive toggles** with the All chip:

| Chip | Colour (light / dark) | Represents |
|---|---|---|
| **All** | Accent blue | Show all files regardless of format |
| **JPG** | Green (`#107C41` / `#68d391`) | JPEG images |
| **HEIC** | Blue (`#0078D4` / `#63b3ed`) | Apple HEIC images |
| **PNG** | Orange (`#CA5010` / `#f6ad55`) | PNG images |
| **MOV** | Red (`#A4262C` / `#fc8181`) | QuickTime video files |

**Interaction rules:**

- The **All** chip and the four format chips are **mutually exclusive**. When All is active, all format chips are in the `off` (unselected) state. Clicking a format chip deactivates All and activates that chip.
- Multiple format chips can be active simultaneously (e.g. JPG + MOV).
- Clicking the **All** chip at any time resets all format chips to `off` and shows every file.
- If the user deselects every individual chip one by one until none remain, the state automatically falls back to All-mode.
- The file table updates **immediately** on every chip toggle with no transition delay.

**Pre-load state:** Before a folder is scanned, all numeric counts displayed next to chip labels are hidden. They appear once the scan completes.

### Summary Chips (right of second divider)

Two read-only informational chips that appear only after a folder has been loaded:

| Chip | Meaning |
|---|---|
| **Groups** | Number of distinct date-based subfolders that will be created |
| **No EXIF** | Number of files for which no capture date could be read from metadata |

"Groups" uses the success colour; "No EXIF" uses the warning colour.

### Theme Toggle (far right)

A small pill-shaped toggle switch anchored to the right end of the stats bar, separated by a vertical divider. It switches between light and dark themes. See [Section 11](#11-theme-system) for details.

---

## 7. File Table

A scrollable table occupying all remaining vertical space. It has five columns:

| Column | Width | Content |
|---|---|---|
| File Name | 34% | Monospace filename, truncated with ellipsis; hover triggers the Preview Card |
| Format | 10% | Coloured format badge (JPG / HEIC / PNG / MOV) |
| Capture Time | 22% | Timestamp read from EXIF (`YYYY-MM-DD  HH:mm:ss`); `—` if unavailable |
| Destination Folder | 24% | Target subfolder name in the chosen date format; `(no EXIF)` with warning colour if unresolvable |
| Status | 10% | Status pill |

### Status Pills

| Pill | Colour | Meaning |
|---|---|---|
| `● Ready` | Green | File has a valid capture date and will be moved |
| `● No EXIF` | Amber | File has no EXIF date; destination is ambiguous |

### Empty State

Before any folder is selected, the table body shows a centred placeholder with a folder icon and the message `"Choose a folder to get started"`. This is replaced by the file list once scanning completes.

### Row Animations

Each row fades and slides in (`opacity 0→1`, `translateY 4px→0`) with a staggered delay capped at 450 ms total, giving the list a smooth cascading appearance on load.

### Filter Integration

Each `<tr>` carries a `data-fmt` attribute (`JPG`, `HEIC`, `PNG`, or `MOV`). The filter bar toggles row visibility by comparing this attribute against the active set. No rows are removed from the DOM; they are shown or hidden via `display` style.

---

## 8. Hover Preview Card

A floating card (`200 × ~230 px`) that appears after hovering over a filename for **500 ms**. It is positioned to the right of the hovered row by default, and flips to the left if it would overflow the viewport edge.

```
┌──────────────────────┐
│  [coloured gradient] │  ← type-specific background
│    🌄          JPG   │  ← emoji + format badge
└──────────────────────┘
  IMG_4821.jpg
  🕐  2024-07-15  09:14:32
  📁  2024-07-15
```

| Element | Detail |
|---|---|
| Thumbnail area | 136 px tall gradient block; colour varies by format type |
| Format badge | Top-left overlay showing format name in the format's accent colour |
| Emoji | A decorative emoji chosen by format and file index from a preset list |
| Play button | Circular overlay shown only for MOV files |
| Filename | Monospace, truncated |
| Capture time | Icon + timestamp; shows `—` if unavailable |
| Destination folder | Icon + folder name; rendered in success green (ready) or warning amber (no EXIF) |

The card appears with a scale + fade-in transition (`scale 0.94→1`, `opacity 0→1`) and disappears instantly when the cursor leaves the filename cell.

---

## 9. Partial Organize Dialog

A modal confirmation dialog triggered when the user clicks **Organize** while one or more format chips are inactive (i.e. not in All-mode). It prevents accidental partial operations.

```
┌────────────────────────────┐
│ ⚠  Partial Organize        │
├────────────────────────────┤
│ Not all file types are     │
│ selected. This operation   │
│ will only organize:        │
│                            │
│  [JPG · 128 files]         │
│  [MOV · 24 files]          │
│                            │
│ Files of unselected types  │
│ will be left in place.     │
│ Do you want to continue?   │
├────────────────────────────┤
│          [Cancel] [Continue]│
└────────────────────────────┘
```

- The dialog lists only the **currently active** format chips with their file counts.
- **Cancel** closes the dialog and returns to the idle state without running any operation.
- **Continue** closes the dialog and starts the organize run immediately.
- The backdrop uses a semi-transparent blur overlay. Clicking outside the dialog does not dismiss it — the user must choose an action explicitly.

---

## 10. Custom Format Dialog

A modal dialog that opens when the user selects **Custom…** from the Folder Format Dropdown. It allows free-form entry of a date format pattern and provides an interactive token reference.

```
┌─────────────────────────────────────────┐
│ ✏  Custom Folder Format                 │
│    Enter a date format pattern…         │
├─────────────────────────────────────────┤
│  Format Pattern                         │
│  ┌──────────────────┬─────────────────┐ │
│  │ yyyy/MMMM/dd     │ Preview: 2024/… │ │
│  └──────────────────┴─────────────────┘ │
│                                         │
│  ╔ Format Tokens — click to insert ═══╗ │
│  ║ Year                               ║ │
│  ║  yyyy  Full year       (2024)      ║ │
│  ║  yy    Two-digit year  (24)        ║ │
│  ║ Month                              ║ │
│  ║  MMMM  Full name       (March)     ║ │
│  ║  MMM   Abbreviated     (Mar)       ║ │
│  ║  MM    Two-digit       (03)        ║ │
│  ║  M     No leading zero (3)         ║ │
│  ║ Day                                ║ │
│  ║  dddd  Full name       (Monday)    ║ │
│  ║  ddd   Abbreviated     (Mon)       ║ │
│  ║  dd    Two-digit       (05)        ║ │
│  ║  d     No leading zero (5)         ║ │
│  ╚════════════════════════════════════╝ │
├─────────────────────────────────────────┤
│                    [Cancel]  [Apply]    │
└─────────────────────────────────────────┘
```

### Input Field

The text input accepts any combination of format tokens and literal separator characters (hyphens, dots, slashes, spaces, etc.). A **Preview** tag on the right side of the input field updates in real time as the user types, rendering the pattern against a fixed sample date (`2024-03-05`) so the output is immediately visible.

### Token Reference

A two-column grid grouped into **Year**, **Month**, and **Day** sections. Every token label is clickable — clicking it inserts the token at the current cursor position in the input field, allowing patterns to be assembled without typing.

| Group | Token | Example output |
|---|---|---|
| Year | `yyyy` | `2024` |
| Year | `yy` | `24` |
| Month | `MMMM` | `March` |
| Month | `MMM` | `Mar` |
| Month | `MM` | `03` |
| Month | `M` | `3` |
| Day | `dddd` | `Monday` |
| Day | `ddd` | `Mon` |
| Day | `dd` | `05` |
| Day | `d` | `5` |

### Actions

| Action | Behaviour |
|---|---|
| **Apply** | Validates that the input is non-empty; on success, saves the pattern, updates the dropdown trigger label, marks **Custom…** as selected (with checkmark), and closes the dialog |
| **Cancel** | Closes the dialog without changing the active format |
| **Enter key** | Equivalent to clicking Apply |
| **Escape key** | Equivalent to clicking Cancel |
| **Backdrop click** | Equivalent to clicking Cancel |

### Format Persistence

The last applied custom pattern is stored in the `savedCustomFmt` variable for the duration of the session. Re-opening the dialog pre-fills the input with this saved value and positions the cursor at the end, so the user can refine rather than re-enter the pattern from scratch.

### Validation

If the user clicks **Apply** with an empty input, an inline error message `"Pattern cannot be empty."` appears in the dialog footer without closing the dialog.

---

## 11. State Lifecycle

### Application States

```
EMPTY  ──[pick folder]──►  SCANNING  ──[scan done]──►  IDLE
                                                         │
                                              [click Organize]
                                                         │
                                          ┌──────────────▼──────────────┐
                                          │ All-mode?                   │
                                          │  Yes → run immediately      │
                                          │  No  → open Partial Dialog  │
                                          └─────────────────────────────┘
                                                         │
                                                    ORGANIZING
                                                         │
                                              [progress 100%]
                                                         │
                                                      DONE
                                              (auto-dismiss after 3.5s)
                                                         │
                                                       IDLE
```

### State-Dependent UI

| UI Element | EMPTY | SCANNING | IDLE | ORGANIZING |
|---|---|---|---|---|
| Folder path | Placeholder | Resolved path | Resolved path | Resolved path |
| Progress strip | Hidden | Visible (indeterminate) | Hidden | Visible (determinate) |
| Organize button | Disabled | Disabled | **Enabled** | Disabled |
| Stat numbers | Hidden | Hidden | Visible | Visible |
| Groups / No EXIF chips | Hidden | Hidden | Visible | Visible |
| File table | Empty state | Empty (cleared) | File rows | File rows |

### Filter State

The filter has two modes managed by the `allMode` boolean and the `active` Set:

| `allMode` | `active` Set | Visible files | All chip | Format chips |
|---|---|---|---|---|
| `true` | empty | All files | `all-on` (highlighted) | All `off` (dimmed) |
| `false` | non-empty subset | Matching formats only | `all-off` (dimmed) | Selected ones `on` |

Transitioning back to `allMode` happens in two ways: the user clicks the All chip, or the user deselects the last active format chip.

---

## 12. Theme System

The application ships with two themes toggled by the pill switch in the Stats & Filter Bar.

### Light Theme (default)

Inspired by Windows 11 Fluent Design. Uses `#f3f3f3` as the base background with white surface cards, soft shadows, and the official Microsoft accent blue (`#0078D4`). Format colours use Microsoft's semantic palette (Office green, Teams red, etc.).

### Dark Theme

A deep navy-black (`#0d0d14`) base with translucent surface layers and a subtle blue/purple radial glow in the background corners. Accent colour shifts to sky blue (`#63b3ed`).

### Theme Toggle

| State | Thumb icon | Track |
|---|---|---|
| Light | ☀️ Sun (orange stroke) | Neutral grey |
| Dark | 🌙 Moon (blue stroke) | Tinted blue |

Switching theme adds or removes the `.dark` class on `<body>`. All colour values are defined as CSS custom properties on `:root` (light) and overridden under `body.dark` (dark), so every component responds instantly with no JavaScript involvement beyond the class toggle.

### Colour Tokens

| Token | Light | Dark |
|---|---|---|
| `--bg` | `#f3f3f3` | `#0d0d14` |
| `--surface` | `rgba(255,255,255,0.85)` | `rgba(255,255,255,0.04)` |
| `--accent` | `#0078D4` | `#63b3ed` |
| `--success` | `#107C41` | `#68d391` |
| `--warning` | `#CA5010` | `#f6ad55` |
| `--text` | `#1a1a1a` | `#e8eaf0` |
| `--text-muted` | `#605E5C` | `#6b7280` |

---

## Typography

| Usage | Font | Weight |
|---|---|---|
| All body text, labels, buttons | Sora | 400 / 500 / 600 |
| File paths, timestamps, format labels, monospace data | JetBrains Mono | 400 / 500 |
