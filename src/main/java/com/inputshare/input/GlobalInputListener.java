package com.inputshare.input;

import com.github.kwhat.jnativehook.keyboard.*;
import com.github.kwhat.jnativehook.mouse.*;

public class GlobalInputListener implements
        NativeKeyListener,
        NativeMouseListener,
        NativeMouseMotionListener,
        NativeMouseWheelListener {

    // --------------------
    // Keyboard events
    // --------------------
    @Override
    public void nativeKeyPressed(NativeKeyEvent e) {
        System.out.println("Key Pressed: " + NativeKeyEvent.getKeyText(e.getKeyCode()));
    }

    @Override
    public void nativeKeyReleased(NativeKeyEvent e) {}

    @Override
    public void nativeKeyTyped(NativeKeyEvent e) {}

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
            case NativeMouseEvent.BUTTON1 -> System.out.println("Left Mouse Button Pressed");
            case NativeMouseEvent.BUTTON2 -> System.out.println("Right Mouse Button Pressed");
            case NativeMouseEvent.BUTTON3 -> System.out.println("Middle Mouse Button Pressed");
            default -> System.out.println("Unknown Mouse Button Pressed: " + e.getButton());
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
