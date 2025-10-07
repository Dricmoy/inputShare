package com.inputshare.input;

import com.github.kwhat.jnativehook.GlobalScreen;
import com.github.kwhat.jnativehook.NativeHookException;

import java.util.logging.Level;
import java.util.logging.Logger;

public class NativeHookManager {

    private NativeHookManager() {
        // prevent instantiation
    }

    public static void initialize() {
        disableVerboseLogging();
        registerHook();
        addListeners();
        System.out.println("✅ Native input listeners initialized successfully.");
    }

    private static void disableVerboseLogging() {
        Logger logger = Logger.getLogger(GlobalScreen.class.getPackage().getName());
        logger.setLevel(Level.OFF);
        logger.setUseParentHandlers(false);
    }

    private static void registerHook() {
        try {
            GlobalScreen.registerNativeHook();
        } catch (NativeHookException e) {
            System.err.println("❌ Failed to register native hook: " + e.getMessage());
            e.printStackTrace();
            System.exit(1);
        }
    }

    private static void addListeners() {
        GlobalInputListener listener = new GlobalInputListener();
        GlobalScreen.addNativeKeyListener(listener);
        GlobalScreen.addNativeMouseListener(listener);
        GlobalScreen.addNativeMouseMotionListener(listener);
        GlobalScreen.addNativeMouseWheelListener(listener);
    }
}
