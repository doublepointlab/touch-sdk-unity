package io.port6.android.unitywrapper

import androidx.activity.ComponentActivity
import android.app.Activity
import android.os.Bundle
import android.util.Log

import io.port6.sdk.*

class HelperActivity: ComponentActivity() {

    override fun onCreate(state: Bundle?) {
        super.onCreate(state)
        val deviceLauncher = CompanionDeviceHelper.createLauncher(this) {
            AndroidUnityWrapper.onDevice(it)
            finish()
        }

        PermissionHelper.createLauncher(this) {

            // After permissions are granted, request device association
            AndroidUnityWrapper.startScan()
            finish()
            //CompanionDeviceHelper.associate(this, deviceLauncher)
        }.also {
            PermissionHelper.requestPermissions(it)
        }
    }
}
