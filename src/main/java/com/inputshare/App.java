package com.inputshare;

import com.inputshare.input.NativeHookManager;
import com.inputshare.utils.GuiLogger;

public class App {
    public static void main(String[] args) {
        boolean enableLogger = false;
        for (String arg : args) {
            if (arg.equalsIgnoreCase("--enable-logger")) {
                enableLogger = true;
                break;
            }
        }
        if (enableLogger) {
            GuiLogger.init();
        }

        NativeHookManager.initialize(); // responsible for all input detection for keyboard and mouse
    }
}
