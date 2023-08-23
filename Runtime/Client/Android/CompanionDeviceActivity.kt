package io.port6.android.unitywrapper

import android.app.Activity
import android.content.Intent
import android.bluetooth.BluetoothDevice
import android.bluetooth.le.ScanFilter
import android.bluetooth.le.ScanResult
import android.companion.AssociationRequest
import android.companion.BluetoothLeDeviceFilter
import android.companion.CompanionDeviceManager
import android.companion.DeviceFilter
import android.content.Context
import android.content.IntentSender
import android.os.ParcelUuid
import android.util.Log
import androidx.activity.ComponentActivity
import androidx.activity.result.ActivityResultLauncher
import androidx.activity.result.IntentSenderRequest
import androidx.activity.result.contract.ActivityResultContracts
import java.lang.ClassCastException
import java.util.regex.Pattern

import com.unity3d.player.UnityPlayerActivity

import io.port6.sdk.*

class CompanionDeviceActivity: UnityPlayerActivity() {

    override fun onActivityResult(requestCode: Int, resultCode: Int, data: Intent?) {
        when (requestCode) {
            SELECT_DEVICE_REQUEST_CODE -> when(resultCode) {
                Activity.RESULT_OK -> {
                    // The user chose to pair the app with a Bluetooth device.
                    val scanResult: ScanResult? = data?.getParcelableExtra(CompanionDeviceManager.EXTRA_DEVICE)
                    scanResult?.device?.also {
                        AndroidUnityWrapper.onDevice(scanResult.device)
                    }
                }
            }
            else -> super.onActivityResult(requestCode, resultCode, data)
        }
    }

    companion object {
        const val SELECT_DEVICE_REQUEST_CODE = 42
    }

    fun connect() {
        Log.d("CompanionDeviceActivity", "connect")

        val deviceFilter = BluetoothLeDeviceFilter.Builder()
        .setScanFilter(ScanFilter.Builder().setServiceUuid(ParcelUuid(TouchSdkProfile.INTERACTION_SERVICE)).build())
        .build()

        val pairingRequest = AssociationRequest.Builder().addDeviceFilter(deviceFilter).build()

        val deviceManager = applicationContext.getSystemService(Context.COMPANION_DEVICE_SERVICE) as CompanionDeviceManager

        deviceManager.associate(pairingRequest,
            object : CompanionDeviceManager.Callback() {
                // Called when a device is found. Launch the IntentSender so the user
                // can select the device they want to pair with.
                override fun onDeviceFound(chooserLauncher: IntentSender) {
                    startIntentSenderForResult(chooserLauncher,
                        SELECT_DEVICE_REQUEST_CODE, null, 0, 0, 0)
                }

                override fun onFailure(error: CharSequence?) {
                    // Handle the failure.
                }
            },
            null
        )

    }
}
