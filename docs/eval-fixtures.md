# Component-states eval fixtures

Three Uno apps for testing the `uno-component-states` skill. Each pairs a
*populated-only* component with a different stack, so a run proves the skill
works beyond MVUX + Material rather than only on its home turf.

| Fixture | Stack | Component | What it stresses |
|---|---|---|---|
| `ProductCardAppv2` | MVUX + Material + Toolkit | Product card (`MainPage.xaml`) | Reference **failure** case — generated externally, states are four separate screens |
| `FluentStatesProbe` | Plain MVVM (INPC) + Fluent | Recent Orders card (`MainPage.xaml`) | Flag-bound visibility; Fluent token vocabulary |
| `StationBoardLab` | Plain MVVM + custom tokens | `Controls/DepartureBoard.xaml` | Standalone `UserControl`; hand-rolled design system; an existing Refresh button |
| `PartsFinderLab` | Plain MVVM + Fluent | Whole page, 4 regions (`MainPage.xaml`) | Phase 0 gate: 2 regions must get NO states; empty-kind taxonomy |
| `InventoryCardLab` | MVUX + Fluent | Stock levels card (built from nothing) | The build-first-then-transform path; no component to start from |

## How to run one

Reset the fixture's component to its populated-only state (`git checkout` the
component, view-model, and service files), then hand a fresh agent a plain
request — "add loading, empty and error states to X" — with **no** mention of
the skill or the grading criteria. Routing is part of what is being measured.

## Grading

**Static self-check** (from the skill's Phase 5) — each is a grep:

1. Retry/CTA binds to a command that resolves in that template's DataContext
2. The shell sits outside the state host; every state renders inside it
3. Loading is a skeleton mirroring the populated layout, not a lone spinner
4. The swap container carries the footprint pin
5. Absence returns a value (no `?? throw`); the service has a forced-mode switch

**Runtime** — drive each state, then prove the two things markup cannot show:

- **Retry fires**: drive to Error, flip the mode file healthy while the app
  runs, invoke Retry, assert a *new* line in the trace log and recovery to data.
- **Footprint**: compare the state host's arranged bounds across states; equal
  numbers pass, "looks about right" does not.

Each fixture's service reads a mode file on every call and appends to a trace
log (paths in the service source; modes are `slow` / `empty` / `error`).

## Results so far (2026-08-17)

- `ProductCardAppv2` — 0/5 static. Dead Retry (DataContext is the exception),
  no shared shell, spinner loading, no pin, empty throws so `NoneTemplate` is
  unreachable. Captures in the fixture folder.
- `FluentStatesProbe` — 5/5 static, Retry proven. Caught one skill defect: the
  skeleton pulse used a `RepeatBehavior="Forever"` storyboard, measured at
  15-21% of one core idle (`Stop()` does not reclaim it). Skill now mandates a
  `DispatcherTimer` pulse.
- `StationBoardLab` — 5/5 static, plus all five rules added that day: reload
  keeps data visible, re-entry guard (two rapid clicks produced one call),
  flash delay, `LiveSetting` announcements, timer pulse. Host measured
  402x208 in both data and error.
- `PartsFinderLab` — mixed page; correctly gave states to only 2 of 4 regions, two empty variants by cause, warehouse Retry proven. Agent confirmed Phase 0 drove the skip decisions.
- `InventoryCardLab` — build-from-scratch path. Populated card and states landed as two separate commits (the states commit touches only the templates), Retry proven, 378x295 in data and error. Surfaced that FeedView has no hook for the skeleton-flash delay; skill now says to put it in the data layer or declare the gap.

## Framework issues filed from this work

- [uno.extensions#3141](https://github.com/unoplatform/uno.extensions/issues/3141) — `{Binding Refresh}` inside `FeedView.ErrorTemplate` binds against the thrown `Exception`, so Retry renders enabled and does nothing. Workaround is the `ElementName` form.
- [uno#24098](https://github.com/unoplatform/uno/issues/24098) — a `RepeatBehavior="Forever"` storyboard keeps consuming ~17% of a core while its target is `Collapsed`. Isolated repro: `PulseCpuRepro` (run the exe with no args / `--pulse` / `--pulse-hide` / `--pulse-stop` and sample CPU).
