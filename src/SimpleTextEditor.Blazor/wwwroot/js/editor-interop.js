/**
 * SimpleTextEditor - JavaScript Interop Module
 * Handles text selection, cursor position, and undo/redo functionality
 */

/**
 * Gets the current text selection in the editor
 * @param {HTMLTextAreaElement} textarea - The editor textarea element
 * @returns {Object} Selection info with start, end, and selectedText
 */
export function getSelection(textarea) {
    if (!textarea) return { start: 0, end: 0, selectedText: '' };

    return {
        start: textarea.selectionStart,
        end: textarea.selectionEnd,
        selectedText: textarea.value.substring(textarea.selectionStart, textarea.selectionEnd)
    };
}

/**
 * Sets the cursor position or selection in the editor
 * @param {HTMLTextAreaElement} textarea - The editor textarea element
 * @param {number} start - Start position
 * @param {number} end - End position (optional, defaults to start)
 */
export function setSelection(textarea, start, end) {
    if (!textarea) return;

    end = end !== undefined ? end : start;
    textarea.focus();
    textarea.setSelectionRange(start, end);
}

/**
 * Inserts text at the current cursor position or wraps selected text
 * @param {HTMLTextAreaElement} textarea - The editor textarea element
 * @param {string} before - Text to insert before selection
 * @param {string} after - Text to insert after selection
 * @param {boolean} newLineBefore - Whether to insert a newline before
 * @returns {string} The new textarea value
 */
export function insertText(textarea, before, after, newLineBefore) {
    if (!textarea) return '';

    const start = textarea.selectionStart;
    const end = textarea.selectionEnd;
    const value = textarea.value;
    const selectedText = value.substring(start, end);

    let prefix = before || '';
    let suffix = after || '';

    // Add newline before if needed and not at start of line
    if (newLineBefore && start > 0 && value[start - 1] !== '\n') {
        prefix = '\n' + prefix;
    }

    const newText = prefix + selectedText + suffix;
    const newValue = value.substring(0, start) + newText + value.substring(end);

    textarea.value = newValue;

    // Position cursor after the inserted prefix (before selected text or at end of insertion)
    const newCursorPos = start + prefix.length + selectedText.length + suffix.length;
    textarea.setSelectionRange(newCursorPos, newCursorPos);
    textarea.focus();

    // Trigger input event for Blazor binding
    textarea.dispatchEvent(new Event('input', { bubbles: true }));

    return newValue;
}

/**
 * Wraps selected text with before/after strings, or inserts at cursor
 * @param {HTMLTextAreaElement} textarea - The editor textarea element
 * @param {string} before - Text to insert before selection
 * @param {string} after - Text to insert after selection
 * @returns {string} The new textarea value
 */
export function wrapSelection(textarea, before, after) {
    if (!textarea) return '';

    const start = textarea.selectionStart;
    const end = textarea.selectionEnd;
    const value = textarea.value;
    const selectedText = value.substring(start, end);

    before = before || '';
    after = after || '';

    const newText = before + selectedText + after;
    const newValue = value.substring(0, start) + newText + value.substring(end);

    textarea.value = newValue;

    // Keep the text selected
    const newStart = start + before.length;
    const newEnd = newStart + selectedText.length;
    textarea.setSelectionRange(newStart, newEnd);
    textarea.focus();

    // Trigger input event for Blazor binding
    textarea.dispatchEvent(new Event('input', { bubbles: true }));

    return newValue;
}

/**
 * Gets the current line number (1-indexed)
 * @param {HTMLTextAreaElement} textarea - The editor textarea element
 * @returns {number} Current line number
 */
export function getCurrentLine(textarea) {
    if (!textarea) return 1;

    const value = textarea.value.substring(0, textarea.selectionStart);
    return (value.match(/\n/g) || []).length + 1;
}

/**
 * Scrolls the preview to sync with editor position
 * @param {HTMLElement} editor - The editor element
 * @param {HTMLElement} preview - The preview element
 */
export function syncScroll(editor, preview) {
    if (!editor || !preview) return;

    const percentage = editor.scrollTop / (editor.scrollHeight - editor.clientHeight);
    preview.scrollTop = percentage * (preview.scrollHeight - preview.clientHeight);
}

/**
 * Toggles fullscreen mode for the editor container
 * @param {HTMLElement} container - The editor container element
 * @param {boolean} enable - Whether to enable fullscreen
 */
export function toggleFullscreen(container, enable) {
    if (!container) return;

    if (enable) {
        container.classList.add('ste-fullscreen');
        document.body.style.overflow = 'hidden';
    } else {
        container.classList.remove('ste-fullscreen');
        document.body.style.overflow = '';
    }
}

/**
 * Focuses the editor textarea
 * @param {HTMLTextAreaElement} textarea - The editor textarea element
 */
export function focus(textarea) {
    if (textarea) {
        textarea.focus();
    }
}
