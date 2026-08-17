# HANDOFF — ComponentStatesLab (uno-component-states skill test harness)
Updated: 2026-08-17

## Where we are

Purpose-built harness for testing the **`uno-component-states`** skill, which turns a
populated Uno/WinUI component into its Loading / Empty / Error states while holding the
component's visual identity fixed. The harness is **built and runtime-verified**; the
skill has **not been run against it yet**. That is the whole of the next session.

The skill itself was recovered this session. It was authored in a cloud session and
pushed only to a remote branch, which is why it could not be found locally at first:

- Origin: `mtmattei/Uno-Builds`, branch `claude/uno-component-states-q9lh2y`, commit `0b35fe70`
- Worktree of that branch: `C:\Users\Platform006\Workflow\Uno-Builds-states` (**temporary — delete once testing is done**)
- **Now installed globally, byte-identical**, at `~/.claude/skills/uno-component-states/`
  (`SKILL.md` + `evals/evals.json`). Skill linter: 69 skills, 0 failing, this one clean.
  It registers and is invocable from any session.

Kept verbatim on purpose: its two "this repo's apps" references are slightly odd outside
Uno-Builds, but an identical copy keeps global results comparable to the branch version.

## Harness design

Two pages, chosen to exercise both halves of the skill's Phase 4 claim ("follow the app's
existing state mechanism, don't invent one"):

**MainPage — a house pattern exists.** An account-header widget is wired through MVUX
`FeedView` with all four templates (Value/Progress/None/Error). It is **not** a target; it
exists so the skill can discover that FeedView is how this app expresses async state.
Three targets sit alongside it with zero state handling:

| # | Target | Kind |
|---|--------|------|
| 1 | Performance metric card | Toolkit `Card`, plain model data |
| 2 | Sector filter bar | Toolkit `ChipGroup` over `IListFeed<Sector>` |
| 3 | Holdings list | `ItemsRepeater` in a `CardContentControl` over `IListFeed<Holding>` |

**BarePage — nothing established.** Target 4 is a bespoke signal widget hand-built from
`Border`/`Grid`/`ItemsRepeater` with strength bars: no Toolkit container, no FeedView, no
VisualStateManager, no `IsLoading` flag anywhere on the page.

`PortfolioService` reads `LAB_MODE` = `data` (default) | `slow` (6s delay) | `empty` |
`error`, so generated states can be driven at runtime rather than argued about.

## Last verified state

- **Build**: `dotnet build -c Release` succeeds, **0 warnings / 0 errors**, `net10.0-desktop`.
- **Runtime**: verified by launching the Release exe and capturing with
  `PrintWindow(PW_RENDERFULLCONTENT)`. Both pages render correctly — all four targets
  populated, chips fit without overflow, holdings rows aligned, signal bars proportional.
  Captures in the session scratchpad: `csl-main3.png`, `csl-bare.png`.
- **Git**: `master`, last commit `76a42c8`, clean tree. Not pushed anywhere (local only).
- **Uno.Sdk**: template pinned **`6.8.0-dev.12`** (a dev SDK, not stable).

## Environment gotchas hit this session

- The **`uno-app` MCP is unavailable** unless the session root *is* the project folder.
  A session rooted at `C:\Users\Platform006` never registers it, and `/mcp` will not fix
  it. **Start the next session rooted at `C:\Users\Platform006\ComponentStatesLab`.**
- `Process.MainWindowHandle` reads **0** for this Uno Skia desktop window even when the
  window is visible. Enumerate top-level windows by PID instead — `Capture-Lab.ps1` in the
  session scratchpad already does this, and samples the bitmap because `PrintWindow` can
  return TRUE over a blank capture.
- PowerShell tool calls **do not share state**; any `Add-Type` interop must be re-declared
  in the same call or kept in a script file.
- Records with an `Id` property need `partial` or MVUX fails the build with `KE0001`.
- Toolkit `Card` slots take **plain values**, not `TextBlock` children — passing elements
  renders the ViewModel's `ToString()` into the slot.
- `utu:Chip` needs `CanRemove="False"` explicitly; `FilterChipGroupStyle` shows a remove ✕ otherwise.

## Known cosmetic gap (deliberately not chased)

The performance `Card` sizes to its content instead of stretching to the column width.
Two fix attempts (`utu:AutoLayout.CounterAlignment`, then `HorizontalAlignment` +
`HorizontalContentAlignment`) did not move it, so it was left alone per the two-cycle
rule. It does not impair the test — a content-sized tile is a legitimate footprint for the
skill to preserve, and "did the footprint stay identical?" is still checkable.

## Next actions (in order)

1. **Run the skill on target 3 first** (holdings list, MainPage). Richest structure, so the
   skeleton logic gets the hardest workout, and the FeedView house pattern is right there
   to be detected. Invoke as
   `/uno-component-states ComponentStatesLab/Presentation/MainPage.xaml`.
2. **Score against the skill's own claims**, not taste: did the shell stay pixel-stable
   (padding, corner radius, typography, density)? Did it reuse Material tokens instead of
   literals? Did it reach for `FeedView` because the app already uses it? Is Empty visually
   distinct from Error? Is any CTA wired to a real command?
3. **Run it on target 4** (`BarePage.xaml`) — the interesting contrast. With no house
   pattern, does it pick a sane mechanism and justify the choice, or invent an architecture?
4. **Verify the generated states actually render**: relaunch under `LAB_MODE=slow`, then
   `empty`, then `error`, capturing each. The shell must not shift between them.
5. Run targets 1 and 2 (Card, ChipGroup) if 1–4 leave anything unresolved.
6. **Decide the two open questions below**, then delete the `Uno-Builds-states` worktree:
   `cd C:\Users\Platform006\Workflow\Uno-Builds; git worktree remove ../Uno-Builds-states`

## Open questions

- Should the global copy's two "this repo's apps" lines be genericized? Deferred until
  after a comparison run, so branch and global versions stay identical during testing.
- Does `uno-component-states` belong in `~/.claude/skills` permanently, or merged into
  `Uno-Builds` `main` as a project skill? Currently both.
- Unrelated but surfaced here: this template pinned **`Uno.Sdk 6.8.0-dev.12`**, which is a
  public dev SDK. That is directly relevant to the *other* stalled project — UnoPreviewsMcp's
  blocking step 1 is "confirm which public dev `Uno.Sdk` actually has Previews", and this
  app is a ready-made throwaway to check the Previews panel on.

## Relaunch

```powershell
# MUST be the session root, or the uno-app MCP will not register
cd C:\Users\Platform006\ComponentStatesLab

dotnet build -c Release        # expect 0 warnings / 0 errors
```

Runtime verification, in preference order:

1. `uno_app_start` via the App MCP (needs the session rooted here **and** a signed-in Uno
   Platform account — signed out gives `toolCount: 0` with a healthy-looking server).
2. Fallback used this session: launch the Release exe, then
   `pwsh -File tools\Capture-Lab.ps1 -ProcessId <pid> -OutputPath <png>` (committed).

Drive states with the env var before launching: `$env:LAB_MODE = "empty"` (or `slow`, `error`, `data`).
