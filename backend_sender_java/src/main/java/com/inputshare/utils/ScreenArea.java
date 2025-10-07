package com.inputshare.utils;
import java.awt.*;

public final class ScreenArea  {
    /*
    System.out.printf("LEFT=%d, RIGHT=%d%n", ScreenArea.LEFT, ScreenArea.RIGHT);
    System.out.printf("TOP=%d, BOTTOM=%d%n", ScreenArea.TOP, ScreenArea.BOTTOM);
    System.out.printf("MIN_WIDTH=%d, MAX_WIDTH=%d%n", ScreenArea.MIN_WIDTH, ScreenArea.MAX_WIDTH);
    System.out.printf("MIN_HEIGHT=%d, MAX_HEIGHT=%d%n", ScreenArea.MIN_HEIGHT, ScreenArea.MAX_HEIGHT);
    System.out.printf("SCREEN_AREA=%s%n", ScreenArea.RECTANGLE);
    */

    public static final Rectangle RECTANGLE;
    public static final int
            LEFT, RIGHT,
            TOP, BOTTOM,
            MIN_WIDTH, MAX_WIDTH,
            MIN_HEIGHT, MAX_HEIGHT,
            TOTAL_WIDTH, TOTAL_HEIGHT;

    static {
        // Initialise local vars
        DisplayMode displayMode;
        Rectangle bounds, scaledBounds;
        int left, right, top, bottom, minWidth, maxWidth, minHeight, maxHeight;
        left = top = minWidth = minHeight = Integer.MAX_VALUE;
        right = bottom = maxWidth = maxHeight = Integer.MIN_VALUE;
        // In a single loop process all bounds
        for (GraphicsDevice device : GraphicsEnvironment.getLocalGraphicsEnvironment().getScreenDevices()) {
            displayMode = device.getDefaultConfiguration().getDevice().getDisplayMode();
            scaledBounds = device.getDefaultConfiguration().getBounds();
            bounds = new Rectangle(scaledBounds.x, scaledBounds.y, displayMode.getWidth(), displayMode.getHeight());
            left = Math.min(left, bounds.x);
            right = Math.max(right, bounds.x + bounds.width);
            top = Math.min(top, bounds.y);
            bottom = Math.max(bottom, bounds.y + bounds.height);
            minWidth = Math.min(minWidth, bounds.width);
            maxWidth = Math.max(maxWidth, bounds.width);
            minHeight = Math.min(minHeight, bounds.height);
            maxHeight = Math.max(maxHeight, bounds.height);
        }
        TOTAL_WIDTH = right - left;
        TOTAL_HEIGHT = bottom - top;
        RECTANGLE = new Rectangle(TOTAL_WIDTH, TOTAL_HEIGHT);
        // Transfer local to immutable global vars
        LEFT = left;
        RIGHT = right;
        TOP = top;
        BOTTOM = bottom;
        MIN_WIDTH = minWidth;
        MAX_WIDTH = maxWidth;
        MIN_HEIGHT = minHeight;
        MAX_HEIGHT = maxHeight;
    }

    // Prevent instantiation
    private ScreenArea() {
    }
}
