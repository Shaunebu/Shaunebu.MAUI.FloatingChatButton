# Android Runtime Validation Checklist

Use a Release build of `FloatingChatButton.Demo` unless a step explicitly asks for Debug. Record device model, Android version, display size, font scale, orientation, and whether TalkBack is enabled.

| Area | Steps | Expected Result |
| --- | --- | --- |
| Launch | Install and launch the demo. | App opens without crash; floating chat button appears above page content near the lower-right visible area. |
| Open and close | Tap the floating button, then tap the overlay. | Chat expands and collapses; latest requested state wins; button remains visible. |
| Rapid tapping | Tap the button 10 times quickly. | No crash, no stuck overlay, no impossible size; final visual state matches final `IsExpanded` value. |
| Drag to edges | Drag the collapsed button to left, right, top, and bottom areas, releasing after each drag. | Button remains fully reachable and snaps to the nearest horizontal edge with finite coordinates. |
| System bars | Drag near status and navigation bars. | Button does not disappear behind bars or become impossible to grab. |
| Portrait and landscape | Rotate from portrait to landscape and back while collapsed and while expanded. | Bounds are recalculated; control remains visible and usable. |
| Small device | Run on a small phone-sized emulator/device. | Expanded panel fits within visible bounds; collapsed button remains reachable. |
| Large device | Run on a tablet/foldable-sized emulator/device. | Expanded panel respects maximum dimensions and does not stretch excessively. |
| Display and font scale | Test default, large, and largest font/display scale. | Text remains readable; input and send button remain usable without overlapping. |
| Keyboard | Expand chat, focus the entry, type text, hide keyboard. | Keyboard does not permanently obscure the control; layout recovers after hiding. |
| Send messages | Type whitespace and tap Send, then type normal text and tap Send. | Whitespace-only input is ignored; normal text is trimmed, added as outgoing, and entry clears. |
| Programmatic add | Add incoming and outgoing messages from the demo/view model. | CollectionView updates and scrolls to the latest message while loaded and expanded. |
| Replace collection | Replace `Messages` with a new collection. | Old collection changes no longer affect the UI; new collection changes update the UI. |
| Navigate away/back | Navigate away from the page and return repeatedly. | No duplicate messages, duplicate event handling, stuck animations, or crashes. |
| Activity recreation | Enable "Don't keep activities" or rotate/recreate the activity. | App restores without leaked old page behavior; control initializes in a reachable position. |
| Background/resume | Background the app while collapsed and expanded, then resume. | No crash; control remains interactive; active animations do not resume into a corrupt state. |
| Theme | Toggle light/dark theme. | Text, bubbles, overlay, and button remain legible with acceptable contrast. |
| TalkBack | Enable TalkBack and navigate to the collapsed button, input, and send button. | Elements have meaningful labels/hints; activation opens/collapses/sends predictably. |
| Multiple instances | Place two controls on a test page with separate message collections. | Each instance keeps its own messages and drag state; no cross-instance message leakage. |
| Memory inspection | Navigate to/from a page with the control 25 times while monitoring memory allocations. | Old pages and controls are collectible; no steadily increasing retained control/page instances. |
| Release behavior | Run all core gestures in Release mode. | Behavior matches Debug; no linker/trimming-related missing resources or crashes. |

## Demo Scenario Mapping

`FloatingChatButton.Demo` starts on a runtime validation dashboard. Open the listed scenario for each checklist area and record the result below. Scenarios use the packaged control surface (`IsExpanded`, `Messages`, `PrimaryColor`, `BotIcon`, layout placement, and `Dispose()` where manual disposal is demonstrated) and expose diagnostics such as expansion state, message count, translation, size, lifecycle event, collection callbacks, load/unload cycles, request count, current theme, and platform information.

| Checklist Area | Demo Scenario |
| --- | --- |
| Launch | Dashboard plus Scenario 1: Basic expand and collapse |
| Open and close | Scenario 1: Basic expand and collapse |
| Rapid tapping | Scenario 2: Rapid transition interruption |
| Drag to edges | Scenario 3: Dragging and edge snapping |
| System bars | Scenario 3: Dragging and edge snapping; Scenario 15: Orientation and layout resizing |
| Portrait and landscape | Scenario 15: Orientation and layout resizing; Scenario 2 while rotating during a transition |
| Small device | Scenario 11: Small-container bounds |
| Large device | Scenario 3 tablet preset; Scenario 15 on tablet/foldable emulator |
| Display and font scale | Scenario 10: Large font and accessibility text |
| Keyboard | Scenario 4: Keyboard and message sending |
| Send messages | Scenario 4: Keyboard and message sending |
| Programmatic add | Scenario 5: Programmatic message insertion |
| Replace collection | Scenario 6: Message collection replacement |
| Navigate away/back | Scenario 8: Navigation lifecycle |
| Activity recreation | Scenario 8: Navigation lifecycle; Scenario 15 after enabling activity recreation |
| Background/resume | Scenario 1 and Scenario 15 while collapsed and expanded |
| Theme | Scenario 9: Theme changes |
| TalkBack | Scenario 10: Large font and accessibility text |
| Multiple instances | Scenario 7: Multiple independent control instances |
| Memory inspection | Scenario 14: Memory-leak navigation loop plus Android Studio profiler |
| Release behavior | Run Scenarios 1-15 from a Release build |

## Runtime Result Table

Do not mark validation complete until these rows are filled from real device or emulator runs.

| Date | Device / Emulator | Android Version | Build | Scenario(s) | Result | Notes / Evidence |
| --- | --- | --- | --- | --- | --- | --- |
|  |  |  | Release | 1-15 | Not run |  |
