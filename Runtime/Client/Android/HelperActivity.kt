package io.port6.android.unitywrapper

import androidx.activity.ComponentActivity
import android.app.Activity
import android.content.Intent
import android.os.Bundle

import io.port6.sdk.*
import io.port6.android.unitywrapper.AndroidUnityWrapper

class HelperActivity: ComponentActivity() {

    override fun onCreate(state: Bundle?) {
        super.onCreate(state)

        val useCompanionDeviceMode = intent.getBooleanExtra(EXTRA_USE_COMPANION_DEVICE, false)

        val deviceLauncher = CompanionDeviceHelper.createLauncher(this) {
            AndroidUnityWrapper.onDevice(it)
            finish()
        }

        PermissionHelper.createLauncher(this) {

            // After permissions are granted, request device association
            if (useCompanionDeviceMode) {
                CompanionDeviceHelper.associate(this, deviceLauncher)
            } else {
                AndroidUnityWrapper.startScan()
                finish()
            }
        }.also {
            PermissionHelper.requestPermissions(it)
        }
    }

    companion object {
        val EXTRA_USE_COMPANION_DEVICE = "foobar_use_companion_device"
    }
}
