# Component States — Rules Card (Uno Studio web variant)

Condensed from `uno-component-states` SKILL.md for one-shot generation agents.
Paste this with the generation prompt. Every rule is a hard requirement.

## The one rule everything follows from

The populated component is the design specification. Loading, Empty, and
Error are **the same component on a different day** — same card, same
footprint, same tokens. Only the data region changes.

Self-check before finishing: if the populated state shows a card and the
other states show bare centered content on the page background, the output
is wrong. All four states render **inside** the same shell — the card,
its padding, corner radius, and header exist exactly once, with the
`FeedView` (or state switch) *inside* it, never wrapped around it.

## Hard rules

1. **Retry must use ElementName.** Inside `FeedView.ErrorTemplate` the
   DataContext is the thrown `Exception`, so the obvious binding is a
   silently dead button:

   ```xml
   <!-- DEAD: resolves to null against the Exception -->
   <Button Content="Retry" Command="{Binding Refresh}" />

   <!-- WORKS: name the FeedView, bind by ElementName -->
   <mvux:FeedView x:Name="ProductFeed" Source="{Binding Product}">
   <Button Content="Retry" Command="{Binding Refresh, ElementName=ProductFeed}" />
   ```

2. **Loading is a skeleton, never a default spinner.** Mirror the populated
   layout: an image placeholder the same size as the image, bars sized like
   the real text lines at their real positions, built from `Border`/
   `Rectangle` filled with a low-emphasis surface brush and a small corner
   radius. No `ProgressRing` unless the app already uses spinners elsewhere.
   No "Loading…" text.

3. **Pin the footprint.** Set `MinHeight` on the container that swaps
   between states, matching the populated data region's height, and center
   the Empty/Error content inside it. Without the pin the component
   collapses the moment data is absent.

4. **Empty must be reachable and must not throw.** The service exposes a
   mode it re-reads on every call (debug flag, env var, or file) forcing
   `slow` / `empty` / `error`. In MVUX, the model returns null/absent for
   empty — `FeedView` then shows `NoneTemplate`. Throwing on "no data"
   makes Empty render as Error and leaves `NoneTemplate` dead code.

5. **Empty is not an error.** Neutral tone, secondary text style, optional
   icon. A CTA only if the app actually supports the action, wired to a
   real command — never a dead button.

6. **Error is an accent, not a repaint.** Existing semantic error brush on
   the icon/message only; the shell keeps its identity. Short human message,
   never raw exception text. Retry per rule 1, omitted only when there is
   genuinely nothing to retry.

7. **Existing resources only.** `ThemeResource`/`StaticResource` brushes,
   text styles, and radii the app already defines. No literal hex, no
   literal font sizes.

## Order of work (even in one pass)

Design the populated component completely first — it is the spec. Then
derive the three states from it by swapping only the data region. States
designed alongside the component degrade into four unrelated screens; that
is the failure mode this card exists to prevent.
