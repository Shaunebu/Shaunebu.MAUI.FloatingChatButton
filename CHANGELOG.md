# Changelog

All notable changes to this project should be documented in this file.

## 1.1.0 - Released

### Fixed

* Added `global.json` to pin the repository to a stable SDK family for repeatable MAUI restore/build behavior.
* Changed `FloatingChatButton.Messages` to use a per-instance default collection instead of a shared mutable bindable-property default.
* Added collection replacement handling so old `Messages` collections are unsubscribed and replacement collections are observed.
* Added lifecycle cleanup for unload and handler replacement, including owned animation cancellation.
* Serialized expand/collapse transitions to reduce overlapping animation and rapid-tap state corruption.
* Clamped drag, snap, and resize coordinates to keep the collapsed button inside its allocated bounds.
* Normalized invalid snap coordinates before edge selection so NaN and infinity cannot select an unsafe target.
* Replaced the unused CommunityToolkit converter dependency with a local converter.
* Removed swallowed exceptions from the demo page constructor.
* Moved the demo control out of the `ScrollView` and into a root overlay layout.
* Stopped forwarding message collection changes while the control is unloaded.

### Added

* .NET 10 MAUI compatibility for Android and iOS targets while retaining .NET 9 target support.
* XML documentation generation for the package.
* NuGet package README, MIT license expression, deterministic build, symbols package, and packaged screenshot asset metadata.
* Accessible default semantic descriptions and hints for the chat control, message entry, and send button.
* Built-in send button behavior for adding local outgoing messages.
* Repository audit, roadmap, license, and CI validation workflow.
* `FloatingChatButton.Tests` with real unit tests for message lifecycle, layout math, snapping, transition coordination, converter behavior, model defaults, registration, and public API baseline validation.
* Android and iOS manual runtime-validation checklists.

### Changed

* Updated README examples to match the current public API and XAML namespace.
* Removed unsupported performance claims and references to undocumented properties.
* Removed unused CommunityToolkit.Maui and CommunityToolkit.Mvvm package references.
* Extracted small internal helpers for deterministic layout, message subscription, and transition coordination tests.

### Known Gaps

* Device/simulator interaction testing for drag, safe areas, keyboard behavior, VoiceOver, and TalkBack remains required before publishing.
* Performance profiling remains required before making frame-rate claims.
