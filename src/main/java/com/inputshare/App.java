package com.inputshare;

import com.inputshare.input.NativeHookManager;
import com.inputshare.logger.GuiLogger;

public class App {
    public static void main(String[] args) {
        if (shouldEnableLogger(args)) {
            GuiLogger.init();
        }

        NativeHookManager.initialize(); // responsible for all input detection for keyboard and mouse
    }

    private static boolean shouldEnableLogger(String[] args) {
        for (String arg : args) {
            if (arg.equalsIgnoreCase("--enable-logger")) {
                return true;
            }
        }
        return false;
    }
}
