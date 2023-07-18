package io.port6.android.unitywrapper

import androidx.activity.ComponentActivity
import android.app.Activity
import android.os.Bundle
import android.util.Log

import io.port6.sdk.*

class HelperActivity: ComponentActivity() {

    override fun onCreate(state: Bundle?) {
        super.onCreate(state)

        PermissionHelper.createLauncher(this) {

            // After permissions are granted, request device association
            AndroidUnityWrapper.startScan()
            finish()
        }.also {
            PermissionHelper.requestPermissions(it)
        }
    }
}
