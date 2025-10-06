package com.inputshare.input;

import com.github.kwhat.jnativehook.keyboard.*;
import com.github.kwhat.jnativehook.mouse.*;
import com.inputshare.utils.GuiLogger;
import com.inputshare.utils.LogTag;

public class GlobalInputListener implements
        NativeKeyListener,
        NativeMouseListener,
        NativeMouseMotionListener,
        NativeMouseWheelListener {

    @Override
    public void nativeKeyPressed(NativeKeyEvent e) {
        GuiLogger.log(LogTag.KEYBOARD, "Key Pressed: " + NativeKeyEvent.getKeyText(e.getKeyCode()));
    }

    @Override
    public void nativeKeyReleased(NativeKeyEvent e) {}

    @Override
    public void nativeKeyTyped(NativeKeyEvent e) {}

    @Override
    public void nativeMouseClicked(NativeMouseEvent e) {
        GuiLogger.log(LogTag.MOUSE, "Mouse Clicked: Button " + e.getButton());
    }

    @Override
    public void nativeMousePressed(NativeMouseEvent e) {
        String buttonName = switch (e.getButton()) {
            case NativeMouseEvent.BUTTON1 -> "Left Mouse Button Pressed";
            case NativeMouseEvent.BUTTON2 -> "Right Mouse Button Pressed";
            case NativeMouseEvent.BUTTON3 -> "Middle Mouse Button Pressed";
            default -> "Unknown Mouse Button Pressed: " + e.getButton();
        };
        GuiLogger.log(LogTag.MOUSE, buttonName);
    }

    @Override
    public void nativeMouseReleased(NativeMouseEvent e) {
        GuiLogger.log(LogTag.MOUSE, "Mouse Released: Button " + e.getButton());
    }

    @Override
    public void nativeMouseMoved(NativeMouseEvent e) {
        GuiLogger.log(LogTag.MOUSE, "Mouse Moved to: (" + e.getX() + ", " + e.getY() + ")");
    }

    @Override
    public void nativeMouseDragged(NativeMouseEvent e) {
        GuiLogger.log(LogTag.MOUSE, "Mouse Dragged to: (" + e.getX() + ", " + e.getY() + ")");
    }

    @Override
    public void nativeMouseWheelMoved(NativeMouseWheelEvent e) {
        GuiLogger.log(LogTag.WHEEL, "Mouse Wheel Moved: Scroll Amount = " + e.getScrollAmount()
                + ", Wheel Rotation = " + e.getWheelRotation());
    }
}
