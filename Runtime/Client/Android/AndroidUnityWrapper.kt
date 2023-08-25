package io.port6.android.unitywrapper

import android.app.Activity
import android.content.Intent
import android.content.Context
import android.util.Log
import android.bluetooth.*
import android.bluetooth.le.*
import com.unity3d.player.UnityPlayer
import io.port6.sdk.*
import java.util.Base64


object AndroidUnityWrapper {

    private val TAG = "AndroidUnityWrapper"

    private val context: Context get() = UnityPlayer.currentActivity.applicationContext
    private val activity: Activity = UnityPlayer.currentActivity

    private val encoder = Base64.getEncoder()

    private var activeWatch: Watch? = null

    private var name: String = ""

    private val watchCallback = object: Watch.WatchCallback() {

        override fun onRawMessage(data: ByteArray) {
            val encoded = encoder.encodeToString(data)
            UnityPlayer.UnitySendMessage("TouchSdkGameObject", "OnTouchSdkMessage", encoded)
        }

        override fun onDisconnect() {
            synchronized(this) {
                activeWatch = null
            }
            watchConnector.startScan()
            UnityPlayer.UnitySendMessage("TouchSdkGameObject", "OnTouchSdkDisconnect", "")
        }
    }

    private val watchConnectorCallback = object: WatchConnector.Callback() {
        override fun onWatchConnected(watch: Watch) {
            synchronized(this) {
                if (activeWatch == null) {
                    Log.d(TAG, "connecting to ${watch.name}")
                    watch.setListener(watchCallback)
                    activeWatch = watch
                    watchConnector.stopScan()
                } else {
                    Log.d(TAG, "already connected to ${activeWatch?.name}, disconnecting ${watch.name}")
                    watch.disconnect() // make sure we are only connected to one watch
                }
            }
        }
    }

    private val watchConnector: WatchConnector = WatchConnector(context, watchConnectorCallback)

    fun onDevice(device: BluetoothDevice) {
        watchConnector.connect(device)
    }

    fun vibrate(length: Int, amplitude: Float) {
        activeWatch?.triggerHaptics(length, amplitude)
    }

    fun cancelVibration() {
        activeWatch?.triggerHaptics(0, 0f)
    }

    fun requestGestureDetection(gesture: Int) {
        activeWatch?.requestGestureDetection(gesture)
    }

    fun connect(nameFilter: String, useCompanionDeviceMode: Boolean) {
        val intent = Intent(activity, HelperActivity::class.java)
        if (useCompanionDeviceMode) {
            if (activity is CompanionDeviceActivity) {
                (activity as CompanionDeviceActivity).connect()
            } else {
                intent.putExtra(HelperActivity.EXTRA_USE_COMPANION_DEVICE, true)
                activity.startActivity(intent)
            }
        } else {
            name = nameFilter
            // Start the helper activity, which will ensure that we have
            // the necessary permissions before calling startScan of this object.
            activity.startActivity(intent)

        }
    }

    fun disconnect() {
        watchConnector.stopScan()
        synchronized(this) {
            activeWatch?.disconnect()
        }
    }

    fun startScan() {
        watchConnector.startScan(name)
    }

}
