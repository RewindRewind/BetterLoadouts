# BetterLoadouts

BepInEx plugin for SPT that makes equipment loadout presets partial-apply instead of full-replace.

## What it does

Vanilla behavior: applying a loadout preset strips **all** your equipment and re-equips from the preset. Slots the preset doesn't define are left empty.

With BetterLoadouts: only slots the preset defines are touched. Everything else stays equipped.

## Example

You have a preset with just a weapon and armor. You're wearing a helmet, headset, and backpack that aren't in the preset.

- **Vanilla:** helmet, headset, and backpack get unequipped. You have to re-equip them manually.
- **BetterLoadouts:** weapon and armor swap in, helmet/headset/backpack stay on.

If the preset defines every slot, behavior is identical to vanilla.

## Install

Drop `BetterLoadouts.dll` into `BepInEx/plugins/`.

## Build

```bash
dotnet build BetterLoadouts.slnx
```

Requires the project to be cloned inside your SPT directory tree (two levels up from the project root). Override with:

```bash
dotnet build -p:TarkovDir=/path/to/spt/
```

## Compatibility

- SPT 4.0.0+
- Tarkov build 40087

## AI Disclosure

Portions of this codebase were implemented with the assistance of LLM-based tools (Claude Code). All architecture, design decisions, and direction were done by a human, the LLM was used as an implementation aid under continuous human supervision and review.
