# Changelog
## 0.0.0-beta
Added support to different gesture models including release and surface taps.
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
