# GitHub CoPilot Instructions for Operable Roof (Continued)

## Mod Overview and Purpose

The "Operable Roof (Continued)" mod is an update of DRILLED_HEAD's original mod that introduces operable or retractable roofs into RimWorld, enhancing the game's architectural and automation possibilities. Inspired by the idea that even with futuristic technology there are limitations, this mod allows players to control their roof structures dynamically. It is especially useful for gameplay scenarios involving secret bases or high-tech facilities where even the ceiling can be controlled.

## Key Features and Systems

- **Buildable Roofs**: Adds new buildable roofs with balanced stats for each roof type available in the game, with special integration for the "Save Our Ship 2" mod.
- **Roof Controllers**: Features roof controllers with visual indicators showing which roofs are connected to them. Includes roof-size indicators when selected.
- **Size Customization**: Allows for changing the roof-size per roof-pillar, providing customizable coverage.
- **Automation**: Provides options to automatically close and open roofs based on time of day and environmental temperature, enhancing automation and in-game strategies.
- **Advanced Control Tables**: Additional structures that can manage larger areas (up to 30x30) but at a higher resource cost.

## Coding Patterns and Conventions

- **C# Structures**: 
  - Usage of classes with methods encapsulating game logic for roof management (`Open`, `Close`, `checkAutomaticRules`).
  - Utilization of members for UI elements like `DrawExtraSelectionOverlays` and `GetInspectString`.

- **Dialog Management**:
  - Use of `Dialog_FloatRangeSlider.cs` for handling user interactions via sliders.

- **Naming Conventions**:
  - Pascal casing for class names (e.g., `Operable_Roof_Controller`) and methods.
  - Descriptive method names for intuitive understanding of functionalities.

## XML Integration

- **Defs**: 
  - `Roofs.xml` and `Buildings_Misc.xml` define additional roof and building behaviors and stats.
  - XML files are structured under `Defs` or `TranslationTemplate` for localization purposes.
  
- **XML Editing Instructions**: To alter roof types, users can modify the `Roofs.xml` file located in the `Defs` directory. Toggle between working and non-working elements using `label ~~~~~~~` for testing changes.

## Harmony Patching

The mod does not explicitly mention Harmony patches, but integration might be necessary if interaction with core game functionalities is needed. Use Harmony to modify existing game functions or introduce postfix/prefix methods alongside game logic in C#.

## Suggestions for Copilot

- **Automation Features**:
  - Suggest code snippets for handling time-based logic (`Tick`) or temperature-based triggers for roof automation.
- **UI Elements**:
  - Offer auto-completion for extending Gizmo-related functionalities, helping with `GetGizmos` and selection overlays.
- **Error Handling**:
  - Generate patterns for exception handling to manage potential bugs or logic errors during mod operation.
- **XML Template Assistance**:
  - Provide XML editing suggestions for defining new building or roof properties (`ThingDef`, `RoofDef`).

This document aims to aid developers integrating Copilot into their workflow to quickly develop and iterate on new features for the "Operable Roof (Continued)" mod in RimWorld.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

