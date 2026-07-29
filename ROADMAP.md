# Roadmap

## Before Next NuGet Release

* Run the sample on Android hardware or emulator and verify drag, snap, expand/collapse, keyboard interaction, TalkBack, and navigation away/back.
* Run the sample on iOS simulator or device and verify safe areas, keyboard interaction, VoiceOver, and scene navigation.
* Profile Release builds before making any frame-rate or performance claims.
* Review the manual validation results and decide whether the `1.1.0` package can move from internal prerelease to public release.

## Non-Breaking Improvements

* Add optional bindable colors/templates for incoming and outgoing message bubbles.
* Add MAUI handler or device tests for the actual control once a reliable test host is available.
* Add UI automation IDs for the bubble, message list, input, and send button.
* Add XML documentation examples for common XAML and MVVM usage.
* Broaden public API compatibility checks to compare both Android and iOS assemblies.

## Future Breaking Candidates

* Remove the public `PlatformClass1` placeholder types from platform folders.
* Consider changing the package root namespace to match the package ID only in a major version.
* Consider replacing `ObservableCollection<ChatMessage>` with a more flexible collection interface if XAML usability and compatibility can be preserved.
* Consider exposing expanded width/height and edge padding as validated bindable properties, since they are documented design concepts but not currently public API.
