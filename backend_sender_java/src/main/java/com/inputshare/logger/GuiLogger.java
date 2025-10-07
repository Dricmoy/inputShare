package com.inputshare.logger;

import com.inputshare.enums.LogTag;

import javax.swing.*;
import javax.swing.text.DefaultCaret;
import java.awt.*;
import java.util.EnumMap;
import java.util.Map;

public class GuiLogger {
    private static JFrame frame;
    private static JTextArea logArea;
    private static boolean initialized = false;

    private static final Map<LogTag, JCheckBox> tagFilters = new EnumMap<>(LogTag.class);

    public static void init() {
        if (initialized) return;
        initialized = true;

        SwingUtilities.invokeLater(() -> {
            frame = new JFrame("InputShare Logger GUI");
            frame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
            frame.setSize(800, 500);
            frame.setLayout(new BorderLayout());

            // --- Toolbar with Filters and Controls ---
            JPanel toolbar = new JPanel(new FlowLayout(FlowLayout.LEFT));

            JCheckBox toggleAll = new JCheckBox("Toggle All", true);
            toggleAll.addActionListener(e -> {
                boolean selected = toggleAll.isSelected();
                tagFilters.values().forEach(cb -> cb.setSelected(selected));
            });
            toolbar.add(toggleAll);

            for (LogTag tag : LogTag.values()) {
                JCheckBox box = new JCheckBox(tag.label(), true);
                tagFilters.put(tag, box);
                toolbar.add(box);
            }

            JButton clearBtn = new JButton("Clear");
            clearBtn.addActionListener(e -> clear());
            toolbar.add(clearBtn);

            frame.add(toolbar, BorderLayout.NORTH);

            // --- Log Area ---
            logArea = new JTextArea();
            logArea.setEditable(false);
            logArea.setFont(new Font("Consolas", Font.PLAIN, 13));

            DefaultCaret caret = (DefaultCaret) logArea.getCaret();
            caret.setUpdatePolicy(DefaultCaret.ALWAYS_UPDATE);

            JScrollPane scrollPane = new JScrollPane(logArea);
            frame.add(scrollPane, BorderLayout.CENTER);

            frame.setVisible(true);
        });
    }

    public static void log(LogTag tag, String message) {
        if (!initialized) return; // don’t auto-init
        JCheckBox filter = tagFilters.get(tag);
        if (filter != null && !filter.isSelected()) return;

        SwingUtilities.invokeLater(() -> {
            if (logArea != null) {
                logArea.append("[" + tag.label() + "] " + message + "\n");
            }
        });
    }


    public static void clear() {
        SwingUtilities.invokeLater(() -> {
            if (logArea != null) logArea.setText("");
        });
    }
}
