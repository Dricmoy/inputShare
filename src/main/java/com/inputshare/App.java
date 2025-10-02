package com.inputshare;

import com.github.kwhat.jnativehook.GlobalScreen;
import com.github.kwhat.jnativehook.NativeHookException;
import com.github.kwhat.jnativehook.keyboard.NativeKeyEvent;
import com.github.kwhat.jnativehook.keyboard.NativeKeyListener;
import com.github.kwhat.jnativehook.mouse.NativeMouseEvent;
import com.github.kwhat.jnativehook.mouse.NativeMouseListener;
import com.github.kwhat.jnativehook.mouse.NativeMouseMotionListener;
import com.github.kwhat.jnativehook.mouse.NativeMouseWheelEvent;
import com.github.kwhat.jnativehook.mouse.NativeMouseWheelListener;

public class App implements NativeKeyListener, NativeMouseListener, NativeMouseMotionListener, NativeMouseWheelListener {
    public static void main(String[] args) {
        try {
            // Disable verbose logging from JNativeHook
            java.util.logging.Logger logger = java.util.logging.Logger.getLogger(GlobalScreen.class.getPackage().getName());
            logger.setLevel(java.util.logging.Level.OFF);

            // Register the native hook (start listening globally)
            GlobalScreen.registerNativeHook();

            App listener = new App();

            // Add listeners for keyboard + mouse + scroll events
            GlobalScreen.addNativeKeyListener(listener);
            GlobalScreen.addNativeMouseListener(listener);
            GlobalScreen.addNativeMouseMotionListener(listener);
            GlobalScreen.addNativeMouseWheelListener(listener);

            System.out.println("JNativeHook started. Listening for keyboard + mouse + scroll events...");
        } catch (NativeHookException ex) {
            System.err.println("Error: " + ex.getMessage());
            ex.printStackTrace();
            System.exit(1);
        }
    }

    // --------------------
    // Keyboard events
    // --------------------
    @Override
    public void nativeKeyPressed(NativeKeyEvent e) {
        System.out.println("Key Pressed: " + NativeKeyEvent.getKeyText(e.getKeyCode()));
    }

    @Override
    public void nativeKeyReleased(NativeKeyEvent e) { }

    @Override
    public void nativeKeyTyped(NativeKeyEvent e) { }

    // --------------------
    // Mouse click events
    // --------------------
    @Override
    public void nativeMouseClicked(NativeMouseEvent e) {
        System.out.println("Mouse Clicked: Button " + e.getButton());
    }

    @Override
    public void nativeMousePressed(NativeMouseEvent e) {
        switch (e.getButton()) {
            case NativeMouseEvent.BUTTON1:
                System.out.println("Left Mouse Button Pressed");
                break;
            case NativeMouseEvent.BUTTON2:
                System.out.println("Right Mouse Button Pressed");
                break;
            case NativeMouseEvent.BUTTON3:
                System.out.println("Middle Mouse Button Pressed");
                break;
            default:
                System.out.println("Unknown Mouse Button Pressed: " + e.getButton());
        }
    }

    @Override
    public void nativeMouseReleased(NativeMouseEvent e) {
        System.out.println("Mouse Released: Button " + e.getButton());
    }

    // --------------------
    // Mouse movement events
    // --------------------
    @Override
    public void nativeMouseMoved(NativeMouseEvent e) {
        System.out.println("Mouse Moved to: (" + e.getX() + ", " + e.getY() + ")");
    }

    @Override
    public void nativeMouseDragged(NativeMouseEvent e) {
        System.out.println("Mouse Dragged to: (" + e.getX() + ", " + e.getY() + ")");
    }

    // --------------------
    // Mouse wheel (scroll) events
    // --------------------
    @Override
    public void nativeMouseWheelMoved(NativeMouseWheelEvent e) {
        System.out.println("Mouse Wheel Moved: Scroll Amount = " + e.getScrollAmount()
                + ", Wheel Rotation = " + e.getWheelRotation());
    }
}
