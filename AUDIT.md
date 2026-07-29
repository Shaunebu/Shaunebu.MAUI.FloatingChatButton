# Production Readiness Audit

Audit date: 2026-07-29

## Overall Assessment

Production-readiness level after the first pass: **ready for a patch-release candidate, pending device validation and real tests**.

Continuation status: **ready for internal prerelease validation, not yet ready for public release**. Real automated tests now exist and pass, but Android/iOS runtime, accessibility, and performance validation have not been executed on emulator, simulator, or device.

Post-audit compatibility update: the repository now builds with SDK `10.0.301` and multi-targets `net9.0-android`, `net9.0-ios`, `net10.0-android`, and `net10.0-ios`. The NuGet package includes both .NET 9 and .NET 10 Android/iOS library assets while preserving the `1.1.0` package version and existing public API baseline.

The repository is small and understandable, with one MAUI control project and one sample app. Before remediation, the biggest risks were a shared mutable `Messages` default, unobserved `async void` animation flows, weak lifecycle cleanup, inaccurate README/package documentation, and an SDK-selection problem that caused restore/build failures on a machine with newer preview SDKs.

## Baseline

Environment before changes:

* Default SDK selected: `11.0.100-preview.6.26359.118`.
* Project target frameworks: `net9.0-android;net9.0-ios`.
* Library package references: `CommunityToolkit.Maui` 12.1.0, `CommunityToolkit.Mvvm` 8.4.0, `Microsoft.Maui.Controls` 9.0.90.
* Demo package references: `CommunityToolkit.Maui` 12.1.0, `CommunityToolkit.Mvvm` 8.4.0, `Microsoft.Maui.Controls` 9.0.90, `Microsoft.Extensions.Logging.Debug` 9.0.0.
* Test projects: none.
* CI workflows: none.

Baseline command results:

| Command | Result |
| --- | --- |
| `dotnet restore FloatingChatButton.sln` | Failed with `NETSDK1147`; .NET 11 preview SDK could not resolve Android workload for `net9.0-android`. |
| `dotnet build FloatingChatButton.sln --configuration Release --no-restore` | Failed with `NETSDK1147` for Android and iOS workloads. |
| `dotnet test FloatingChatButton.sln --configuration Release --no-restore` | Failed with `NETSDK1147`; no test project exists. |
| `dotnet build FloatingChatButton.Demo/FloatingChatButton.Demo.csproj --configuration Release --no-restore` | Failed with `NETSDK1147`. |
| `dotnet pack FloatingChatButton/FloatingChatButton.csproj --configuration Release --no-restore` | Failed with `NETSDK1147`. |

## Findings

### Critical

| ID | File / Member | Explanation | Impact | Correction | Compatibility | Fixed now |
| --- | --- | --- | --- | --- | --- | --- |
| CRIT-001 | `FloatingChatButton.csproj`, repository root | No `global.json`; repository selected .NET 11 preview SDK for .NET 9 MAUI targets. | Clean restore/build failed on this machine. | Added `global.json` pinning SDK `9.0.313` with `latestFeature` roll-forward. | Internal, non-breaking | Yes |
| CRIT-002 | `FloatingChatButton.MessagesProperty` | Bindable property used `new ObservableCollection<ChatMessage>()` as a shared mutable default. | Multiple control instances could share messages and retain unexpected state. | Switched to `defaultValueCreator` and null coercion. | Behavioral correction | Yes |

### High

| ID | File / Member | Explanation | Impact | Correction | Compatibility | Fixed now |
| --- | --- | --- | --- | --- | --- | --- |
| HIGH-001 | `FloatingChatButton` expand/collapse internals | `async void` transition methods could overlap and hide exceptions. | Rapid taps could leave inconsistent layout/visibility state. | Added observed task-based transition runner, versioning, and animation aborts. | Behavioral correction | Yes |
| HIGH-002 | `FloatingChatButton` lifecycle | No unload/handler-changing cleanup for animations and collection subscriptions. | Navigation or handler recreation could retain work longer than needed. | Added `Loaded`, `Unloaded`, `OnHandlerChanging`, and idempotent `Dispose()`. | Additive, non-breaking | Yes |
| HIGH-003 | `Messages` replacement | Replacing `Messages` had no explicit subscription transfer. | Old collections could keep event handlers after replacement once collection handling was introduced. | Added unsubscribe/subscribe logic and automatic scroll on changes. | Behavioral correction | Yes |
| HIGH-004 | README public API examples | README used namespace and properties that do not compile against current API. | Consumers following docs would fail at compile time. | Rewrote examples around the registered XAML URI and actual properties. | Documentation | Yes |

### Medium

| ID | File / Member | Explanation | Impact | Correction | Compatibility | Fixed now |
| --- | --- | --- | --- | --- | --- | --- |
| MED-001 | Drag and snap logic | Coordinates were not consistently clamped for small containers, resize, or invalid values. | Button could become hard to reach after resize or unusual layout. | Added finite-size checks and coordinate clamping. | Behavioral correction | Yes |
| MED-002 | `ViewExtensions.ResizeTo` | No argument validation and used possibly invalid measured dimensions as animation starts. | Invalid animation inputs could fail unpredictably. | Added null/range validation, animation abort, and safer start dimensions. | Behavioral correction | Yes |
| MED-003 | Converters | `ConvertBack` threw `NotImplementedException`. | Tooling or accidental two-way binding could crash. | Return `Binding.DoNothing`. | Behavioral correction | Yes |
| MED-004 | Dependencies | CommunityToolkit packages were only used for one converter and a registration wrapper. | Unnecessary dependency and analyzer failure in the demo. | Added local `InvertedBoolConverter`; removed Toolkit/MVVM references. | Internal, non-breaking | Yes |
| MED-005 | Demo layout | Floating control was inside `ScrollView`. | Drag bounds reflected scroll content instead of visible page. | Moved control to root overlay `Grid`. | Sample-only | Yes |

### Low

| ID | File / Member | Explanation | Impact | Correction | Compatibility | Fixed now |
| --- | --- | --- | --- | --- | --- | --- |
| LOW-001 | Public XML docs | Public classes and members were sparsely documented. | Poor IntelliSense and XML doc warnings once docs are enabled. | Added XML docs to public API touched in this pass. | Documentation | Yes |
| LOW-002 | Platform placeholders | `PlatformClass1` is public template noise. | Pollutes public API for compiled platform targets. | Documented placeholders; deferred removal to a major version. | Future breaking candidate | No |
| LOW-003 | `.editorconfig` | Several compiler/analyzer diagnostics are disabled globally with placeholder comments. | Can hide real issues. | Documented for follow-up; did not change globally in this pass. | Internal | No |

### Documentation

| ID | File / Member | Explanation | Impact | Correction | Compatibility | Fixed now |
| --- | --- | --- | --- | --- | --- | --- |
| DOC-001 | `README.md` | Claimed "60 FPS", "optimized performance", and undocumented sizing/color properties. | Unsupported release claims and broken examples. | Replaced with verifiable wording and actual API table. | Documentation | Yes |
| DOC-002 | `README.md` | Embedded very large base64 screenshot. | Bloats source and package readme. | Extracted to `docs/assets/floating-chat-expanded.png` and referenced relatively. | Documentation | Yes |
| DOC-003 | Repository docs | Missing audit, changelog, roadmap, and license files. | Harder to review and release responsibly. | Added `AUDIT.md`, `CHANGELOG.md`, `ROADMAP.md`, and `LICENSE`. | Documentation | Yes |

### Testing

| ID | File / Member | Explanation | Impact | Correction | Compatibility | Fixed now |
| --- | --- | --- | --- | --- | --- | --- |
| TEST-001 | Solution | No test project exists. | `dotnet test` does not validate behavior beyond build. | Documented as release blocker; deterministic test seams should be added next. | Testing | No |
| TEST-002 | Device testing | No automated device/UI coverage exists. | Dragging, keyboard, safe area, and screen-reader behavior are not proven. | Documented required Android/iOS validation. | Testing | No |

### Future or Breaking Candidates

| ID | Candidate | Reason |
| --- | --- | --- |
| BREAK-001 | Remove public `PlatformClass1` placeholders. | They are template artifacts, but removing public types is breaking. |
| BREAK-002 | Align root namespace with package ID. | Current namespace is `FloatingChatButton`; changing it would break source/XAML consumers. |
| BREAK-003 | Expose expanded size and edge padding as bindable properties. | README previously documented these concepts, but adding them needs design and tests. |
| BREAK-004 | Consider collection interface broadening. | `ObservableCollection<ChatMessage>` is XAML-friendly but less flexible than `IList`/`IEnumerable`; changing it would be breaking. |

## Changes Implemented

| File | Reason | Compatibility | Coverage |
| --- | --- | --- | --- |
| `global.json` | Pin .NET 9 SDK selection for stable restore/build. | Internal, non-breaking | Restore/build verification |
| `FloatingChatButton/Controls/FloatingChatButton.xaml.cs` | Per-instance messages, collection replacement handling, lifecycle cleanup, animation reentrancy control, coordinate clamping, accessible defaults, send behavior. | Behavioral correction/additive | Build verification; device tests still needed |
| `FloatingChatButton/Controls/FloatingChatButton.xaml` | Add send-button handler/name, local inverted converter, compiled binding metadata. | Internal, non-breaking | XAML compilation |
| `FloatingChatButton/Converters/*.cs` | XML docs, safe `ConvertBack`, local inverted bool converter. | Behavioral correction/additive | Build verification |
| `FloatingChatButton/Extensions/ViewExtensions.cs` | Validate arguments and abort previous resize animations. | Behavioral correction | Build verification |
| `FloatingChatButton/Models/ChatMessage.cs` | XML docs and non-null default text. | Behavioral correction | Build verification |
| `FloatingChatButton/MainProgram.cs` | Null guard and dependency-free registration hook. | Behavioral correction | Demo build |
| `FloatingChatButton/FloatingChatButton.csproj` | Package metadata, XML docs, symbols, readme/assets, dependency cleanup. | Internal, non-breaking | Pack and package inspection |
| `FloatingChatButton.Demo/*` | Remove swallowed exception, use package registration, improve overlay sample layout. | Sample-only | Android/iOS demo builds |
| `README.md`, `CHANGELOG.md`, `ROADMAP.md`, `AUDIT.md`, `LICENSE` | Release documentation. | Documentation | Package inspection/read review |
| `.github/workflows/ci.yml` | Restore/build/test/pack validation without publishing. | CI-only | Not executed locally |

## Final Verification

| Command | Result |
| --- | --- |
| `dotnet --version` | `9.0.313` |
| `dotnet restore FloatingChatButton.sln` | Succeeded |
| `dotnet build FloatingChatButton/FloatingChatButton.csproj --configuration Release --no-restore` | Succeeded, 0 warnings, 0 errors; created `.nupkg` and `.snupkg` |
| `dotnet build FloatingChatButton.Demo/FloatingChatButton.Demo.csproj --configuration Release --no-restore` | Succeeded, 0 warnings, 0 errors |
| `dotnet build FloatingChatButton.sln --configuration Release --no-restore` | Succeeded, 0 warnings, 0 errors |
| `dotnet test FloatingChatButton.sln --configuration Release --no-restore` | Succeeded as build validation; no test projects were present |
| `dotnet build FloatingChatButton/FloatingChatButton.csproj --configuration Release --framework net9.0-android --no-restore` | Succeeded, 0 warnings, 0 errors |
| `dotnet build FloatingChatButton/FloatingChatButton.csproj --configuration Release --framework net9.0-ios --no-restore` | Succeeded, 0 warnings, 0 errors |
| `dotnet build FloatingChatButton.Demo/FloatingChatButton.Demo.csproj --configuration Release --framework net9.0-android --no-restore` | Succeeded, 0 warnings, 0 errors |
| `dotnet build FloatingChatButton.Demo/FloatingChatButton.Demo.csproj --configuration Release --framework net9.0-ios --no-restore` | Succeeded, 0 warnings, 0 errors |
| `dotnet pack FloatingChatButton/FloatingChatButton.csproj --configuration Release --no-restore` | Succeeded; created `Shaunebu.MAUI.Controls.FloatingChatButton.1.0.0.nupkg` and `.snupkg` |

Package inspection confirmed:

* `README.md`
* `docs/assets/floating-chat-expanded.png`
* `lib/net9.0-android35.0/FloatingChatButton.dll`
* `lib/net9.0-android35.0/FloatingChatButton.xml`
* `lib/net9.0-ios18.0/FloatingChatButton.dll`
* `lib/net9.0-ios18.0/FloatingChatButton.xml`
* MIT license expression in nuspec
* Symbols package generated

Public API comparison:

* Preserved existing public control, model, converter, extension, bindable-property, and placeholder type names.
* Added `FloatingChatButton.Controls.FloatingChatButton.Dispose()`.
* Added `FloatingChatButton.Converters.InvertedBoolConverter`.
* Changed `ViewExtensions.ResizeTo` nullable annotation from `Easing` to `Easing?`; source-compatible for callers.
* Removed NuGet dependency exposure for CommunityToolkit packages; no public API type from those packages remains in signatures.

## Remaining Findings

* Validate drag/snap/keyboard/safe-area behavior on Android emulator or hardware.
* Validate expand/collapse, safe areas, keyboard, scene lifecycle, and VoiceOver on iOS simulator or hardware.
* Profile Release builds before making performance claims.
* Inspect behavior under large message collections before adding virtualization or limits.
* Review whether `PlatformClass1` placeholders should be removed in a major version.
* The CI workflow was added but not executed locally.

## Release Recommendation

Initial recommendation: **ready for patch-release candidate after device validation and tests**.

Use a patch version rather than reusing `1.0.0`, because the changes are compatible corrections to lifecycle, package metadata, docs, and behavior. Do not publish until Android and iOS runtime checks and at least focused unit tests are added.

## Continuation Results

### Regression Review

| Prior remediation | Status | Notes |
| --- | --- | --- |
| Per-instance `Messages` initialization | Confirmed correct | Covered by `MessageCollectionObserverTests.NullCollectionCreatesSafePerInstanceCollection`. |
| `Messages` collection replacement | Corrected further | Extracted `MessageCollectionObserver`; replacement unsubscribes the old collection and observes the new one only while active. |
| Collection event subscription cleanup | Corrected further | `Unloaded` now deactivates collection forwarding; `Loaded` reactivates without duplicate subscriptions; `Dispose()` unsubscribes. |
| `Loaded` and `Unloaded` handling | Corrected further | Fixed regression where collection changes could auto-scroll after unload. |
| `OnHandlerChanging` cleanup | Confirmed correct | Still aborts transient state and animations; requires handler recreation validation on device. |
| `Dispose()` implementation | Confirmed correct | Idempotent; consumers normally do not call it when MAUI owns the visual tree. After disposal, the control should not be reused. |
| Animation cancellation and reentrancy | Confirmed at coordination level | `TransitionCoordinator` tests verify latest version wins; actual animation engine behavior still needs runtime validation. |
| Drag coordinate clamping | Corrected further | Extracted layout helper; infinity now normalizes before edge selection. |
| Edge snapping | Confirmed at calculation level | Covered for normal, equal-distance, out-of-range, tiny-container, NaN, and infinity inputs. |
| Send-button behavior | Confirmed at normalization level | Whitespace normalizes to empty and is ignored by control; UI click path still needs runtime validation. |
| Accessibility defaults | Requires runtime validation | Semantics compile, but TalkBack/VoiceOver must be tested manually. |
| `ResizeTo` validation | Requires runtime validation | Argument validation builds; real MAUI animation behavior remains device/runtime work. |
| `InvertedBoolConverter` | Confirmed correct | Unit tests cover true, false, null, unexpected values, and `ConvertBack`. |
| CommunityToolkit dependency removal | Confirmed correct | Library and demo build without Toolkit package references. |
| Demo layout changes | Requires runtime validation | Android/iOS builds pass; visual behavior must be manually checked. |
| Package metadata | Confirmed correct | Pack and package inspection pass. |
| CI workflow | Edited, not executed | CI now runs real tests, checks for zero tests, inspects package contents, and uploads artifacts. |

### Automated Tests

Test project: `FloatingChatButton.Tests`.

Latest local result: **55 tests discovered, 55 passed, 0 failed, 0 skipped**.

Coverage areas:

* Message collection ownership, replacement, add/remove/replace/reset events, unload deactivation, repeated load/unload activation, and disposal.
* Coordinate clamping, snap target selection, invalid number handling, very small containers, and orientation-size recalculation.
* Expanded width/height calculations for zero, normal, maximum, NaN, and infinity inputs.
* Outgoing message text normalization.
* `InvertedBoolConverter` convert/convert-back paths.
* `ChatMessage` defaults and current null-assignment behavior.
* `UseFloatingChatButton()` null validation and repeated registration.
* Transition versioning and cancellation invariants.
* Android public API baseline for hand-authored public surface, excluding generated `Resource`.

Important untested areas:

* Actual MAUI `FloatingChatButton` visual tree instantiation under a platform handler.
* Real animations, focus behavior, keyboard interaction, and CollectionView scrolling.
* Android TalkBack and iOS VoiceOver behavior.
* Memory graph/device leak validation.
* Performance profiling.

### Public API Review

Added public APIs:

* `FloatingChatButton.Controls.FloatingChatButton.Dispose()`
* `FloatingChatButton.Converters.InvertedBoolConverter`

Removed public APIs:

* None.

Changed public APIs:

* `ViewExtensions.ResizeTo` now annotates the `Easing` parameter as nullable; this is source-compatible.
* Package dependency surface no longer includes CommunityToolkit packages.

API recommendation:

* Keep `Dispose()` public because `FloatingChatButton` explicitly implements `IDisposable`; document that normal MAUI consumers do not need to call it and should not reuse the control after disposal.
* Keep `InvertedBoolConverter` public because MAUI XAML resource construction expects a public type. Treat it as additive API and include it in the baseline.

Baseline result:

* `FloatingChatButton.Tests/PublicApi.Shipped.txt` was added.
* `PublicApiBaselineTests.AndroidAssemblyPublicApiMatchesBaseline` passes against `bin/Release/net9.0-android/FloatingChatButton.dll`.

### Semantic Version

Recommended version: **`1.1.0`**.

Reasoning:

* `1.0.1` is too small because the release intentionally includes additive public API (`Dispose()` and `InvertedBoolConverter`), plus new externally visible send behavior and package/test/CI assets.
* `2.0.0` is not required because no confirmed breaking changes were introduced or required.
* `1.1.0` matches SemVer for compatible fixes plus additive public API.

### Runtime Validation Status

| Area | Status |
| --- | --- |
| Build validation | Completed locally. |
| Unit-test validation | Completed locally: 55 passed. |
| Android runtime validation | Not executed; checklist added at `docs/testing/android-validation.md`. |
| iOS runtime validation | Not executed; checklist added at `docs/testing/ios-validation.md`. |
| Accessibility validation | Not executed on TalkBack or VoiceOver. |
| Performance validation | Not executed; no frame-rate claims should be made. |

### Continuation Verification

| Command | Result |
| --- | --- |
| `dotnet --version` | `9.0.313` |
| `dotnet restore FloatingChatButton.sln` | Succeeded |
| `dotnet build FloatingChatButton.sln -c Release --no-restore` | Succeeded, 0 warnings, 0 errors |
| `dotnet test FloatingChatButton.sln -c Release --no-build` | Succeeded; 55 discovered, 55 passed, 0 failed, 0 skipped |
| `dotnet build FloatingChatButton/FloatingChatButton.csproj -c Release -f net9.0-android --no-restore` | Succeeded, 0 warnings, 0 errors |
| `dotnet build FloatingChatButton/FloatingChatButton.csproj -c Release -f net9.0-ios --no-restore` | Succeeded, 0 warnings, 0 errors |
| `dotnet build FloatingChatButton.Demo/FloatingChatButton.Demo.csproj -c Release -f net9.0-android --no-restore` | Succeeded, 0 warnings, 0 errors |
| `dotnet build FloatingChatButton.Demo/FloatingChatButton.Demo.csproj -c Release -f net9.0-ios --no-restore` | Succeeded, 0 warnings, 0 errors |
| `dotnet pack FloatingChatButton/FloatingChatButton.csproj -c Release --no-restore` | Succeeded; created `Shaunebu.MAUI.Controls.FloatingChatButton.1.1.0.nupkg` and `.snupkg` |

Package inspection for `1.1.0` confirmed:

* `README.md`
* `docs/assets/floating-chat-expanded.png`
* `lib/net9.0-android35.0/FloatingChatButton.dll`
* `lib/net9.0-android35.0/FloatingChatButton.xml`
* `lib/net9.0-ios18.0/FloatingChatButton.dll`
* `lib/net9.0-ios18.0/FloatingChatButton.xml`
* `Shaunebu.MAUI.Controls.FloatingChatButton.nuspec`
* nuspec version `1.1.0`

### Updated Release Recommendation

Recommendation: **ready for internal prerelease** using version **`1.1.0`**.

Do not publish publicly until Android and iOS runtime validation, accessibility checks, and at least basic memory-leak inspection are completed.
