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

    private val watchCallback = object: Watch.WatchCallback() {

        override fun onRawMessage(data: ByteArray) {
            val encoded = encoder.encodeToString(data)
            UnityPlayer.UnitySendMessage("TouchSdkGameObject", "OnTouchSdkMessage", encoded)
        }

        override fun onDisconnect() {
            UnityPlayer.UnitySendMessage("TouchSdkGameObject", "OnTouchSdkDisconnect", null)
        }

    }

    private val watchConnectorCallback = object: WatchConnector.Callback() {
        override fun onWatchConnected(watch: Watch) {
            Log.i(TAG, "Found watch")
            watch.setListener(watchCallback)
        }
    }

    private val watchConnector = WatchConnector(context, watchConnectorCallback)

    fun onDevice(device: BluetoothDevice) {
        Log.d(TAG, "got $device")
        watchConnector.connect(device)
    }

    fun vibrate(length: Int, amplitude: Float) {}

    fun cancelVibration() {}

    fun requestGestureDetection(gesture: Int) {}

    fun connect() {

        val intent = Intent(activity, HelperActivity::class.java)
        activity.startActivity(intent)
    }

    fun disconnect() {}

    fun startScan() {
        watchConnector.startScan()

    }

}
