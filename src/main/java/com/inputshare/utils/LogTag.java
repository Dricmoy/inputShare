package com.inputshare.utils;

public enum LogTag {
    INFO("INFO"),
    KEYBOARD("KEYBOARD"),
    MOUSE("MOUSE"),
    WHEEL("WHEEL"),
    NETWORK("NETWORK"),
    CLIPBOARD("CLIPBOARD"),
    SYSTEM("SYSTEM");

    private final String label;

    LogTag(String label) {
        this.label = label;
    }

    public String label() {
        return label;
    }

    @Override
    public String toString() {
        return label;
    }
}
