# iOS Runtime Validation Checklist

Use a Release build of `FloatingChatButton.Demo` on simulator or device. Record device model, iOS version, size class, Dynamic Type size, orientation, and whether VoiceOver is enabled.

| Area | Steps | Expected Result |
| --- | --- | --- |
| iPhone layout | Launch on a compact iPhone simulator/device. | Floating button starts inside the visible page and remains reachable. |
| iPad layout | Launch on iPad simulator/device, including split-view if available. | Expanded panel respects maximum dimensions and does not consume the whole screen unexpectedly. |
| Portrait/landscape | Rotate while collapsed, expanded, and during a transition. | Control remains visible; latest requested transition state wins after rotation. |
| Safe areas | Test devices with home indicator, notch, and Dynamic Island. | Button and expanded panel avoid becoming inaccessible behind unsafe areas. |
| Keyboard | Expand chat, focus entry, type, send, hide keyboard. | Keyboard interaction does not corrupt layout; entry and send button remain usable. |
| Expand/collapse during keyboard | Start expand/collapse while keyboard is appearing or disappearing. | No crash, no stuck overlay, no lost input focus unless collapse intentionally hides chat. |
| Drag near safe areas | Drag the collapsed button near top notch, side edges, and bottom home indicator. | Button remains reachable and snaps to a valid horizontal edge. |
| VoiceOver | Enable VoiceOver and navigate through the collapsed button, messages, entry, and send button. | Labels and hints are meaningful; activation works predictably. |
| Dynamic Type | Test default, large, and largest accessibility text sizes. | Text remains readable; controls do not overlap or become unusable. |
| Background/foreground | Background the app while expanded and collapsed, then foreground. | No crash; animations are not left half-applied; control remains interactive. |
| Scene lifecycle | On iPad or multi-window capable simulator, create/destroy scenes if available. | No retained old scene control instance; new scene initializes correctly. |
| Navigate away/back | Navigate away from the page and return repeatedly. | Collection subscriptions do not duplicate; old page/control instances can be released. |
| Multiple instances | Place two controls on a test page with separate collections. | Messages and drag state remain isolated per instance. |
| Collection replacement | Replace the message collection while loaded, unloaded, collapsed, and expanded. | Old collection no longer drives UI; new collection is displayed and observed. |
| Rapid transition interruption | Rapidly tap the bubble and overlay. | Final visual state matches latest `IsExpanded` request; no stale animation updates reappear. |
| Memory graph | Use Xcode memory graph after repeated navigation and collection replacement. | No retained old pages, controls, gesture closures, or old message collections. |
| Release behavior | Run core gestures and send flow in Release mode. | Behavior matches Debug; no linker/trimming-related missing resources or crashes. |

## Demo Scenario Mapping

`FloatingChatButton.Demo` starts on a runtime validation dashboard. Open the listed scenario for each checklist area and record the result below. Scenarios use the packaged control surface (`IsExpanded`, `Messages`, `PrimaryColor`, `BotIcon`, layout placement, and `Dispose()` where manual disposal is demonstrated) and expose diagnostics such as expansion state, message count, translation, size, lifecycle event, collection callbacks, load/unload cycles, request count, current theme, and platform information.

| Checklist Area | Demo Scenario |
| --- | --- |
| iPhone layout | Dashboard plus Scenario 1: Basic expand and collapse |
| iPad layout | Scenario 3 tablet preset; Scenario 15 in iPad split view where available |
| Portrait/landscape | Scenario 15: Orientation and layout resizing; Scenario 2 while rotating during a transition |
| Safe areas | Scenario 3: Dragging and edge snapping; Scenario 15: Orientation and layout resizing |
| Keyboard | Scenario 4: Keyboard and message sending |
| Expand/collapse during keyboard | Scenario 4 with the keyboard visible; Scenario 2 for rapid state changes |
| Drag near safe areas | Scenario 3: Dragging and edge snapping |
| VoiceOver | Scenario 10: Large font and accessibility text |
| Dynamic Type | Scenario 10: Large font and accessibility text |
| Background/foreground | Scenario 1 and Scenario 15 while collapsed and expanded |
| Scene lifecycle | Scenario 14: Memory-leak navigation loop plus iPad scene tools where available |
| Navigate away/back | Scenario 8: Navigation lifecycle |
| Multiple instances | Scenario 7: Multiple independent control instances |
| Collection replacement | Scenario 6: Message collection replacement |
| Rapid transition interruption | Scenario 2: Rapid transition interruption |
| Memory graph | Scenario 14: Memory-leak navigation loop plus Xcode memory graph |
| Release behavior | Run Scenarios 1-15 from a Release build |

## Runtime Result Table

Do not mark validation complete until these rows are filled from real simulator or device runs.

| Date | Device / Simulator | iOS Version | Build | Scenario(s) | Result | Notes / Evidence |
| --- | --- | --- | --- | --- | --- | --- |
|  |  |  | Release | 1-15 | Not run |  |
