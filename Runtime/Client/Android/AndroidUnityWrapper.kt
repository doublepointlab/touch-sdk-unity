package io.port6.android.unitywrapper

import android.app.Activity
import android.content.Intent
import android.content.Context
import android.util.Log
import android.bluetooth.*
import android.bluetooth.le.*
import com.unity3d.player.UnityPlayer
import io.port6.sdk.*


object AndroidUnityWrapper {

    private val TAG = "AndroidUnityWrapper"

    private val context: Context get() = UnityPlayer.currentActivity.applicationContext
    private val activity: Activity = UnityPlayer.currentActivity

    private val watchCallback = object: Watch.WatchCallback() {

        override fun onSensors(sensors: SensorFrame) {}
        override fun onGesture(gesture: Int) {}
        override fun onTouch(type: Touch, coordinates: Vec2) {}
        override fun onButton(type: Int) {}
        override fun onRotary(step: Int) {}
        override fun onDisconnect() {}

    }

    private val watchConnectorCallback = object: WatchConnector.Callback() {
        override fun onWatchConnected(watch: Watch) {
            Log.i(TAG, "Found watch")
            watch.setListener(watchCallback)
        }
    }

    private val watchConnector = WatchConnector(context, watchConnectorCallback)

    fun onDevice(device: BluetoothDevice) {
        Log.d("HEHEHE", "got $device")
    }

    fun vibrate(length: Int, amplitude: Float) {}

    fun cancelVibration() {}

    fun requestGestureDetection(gesture: Int) {}

    fun connect() {
        val intent = Intent(activity, HelperActivity::class.java)
        activity.startActivity(intent)
    }

    fun disconnect() {}

}
