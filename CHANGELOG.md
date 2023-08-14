# Changelog
## 0.10.0-beta
Added support for receiving stateful pinch gestures. The start and end of a pinch
are designated by a `PinchTap` and `PinchRelease` gestures, respectively. For the
sake of consistency with stateful pinch and pinch events, pinch events are now
emitted as `PinchTap`, followed immediately by `PinchRelease`.
## 0.9.0-beta
Added support to different gesture models including pinch and surface taps.
## 0.8.0-beta
This update improves the versatility and performance of the TouchSDK Unity
public interface.  WatchManager has been removed and the functionality moved to
a new MonoBehaviour, Watch, which can be accessed with the static property
Watch.Instance. This enables alternative sources for the watch data.

The sensors events were changed to basic c# events to increase performance. The
naming scheme was also unified for some events. The sensor values are also
accessible as properties of Watch.

The connection to a watch is contained in BluetoothWatchProvider. It should be
accessed through the generic interface Watch.Instance.
