/**
 * SimpleTextEditor - WYSIWYG JavaScript Interop Module
 * Handles contenteditable operations for WYSIWYG mode
 */

/**
 * Executes a document command on the contenteditable element
 * @param {string} command - The command to execute (bold, italic, etc.)
 * @param {string|null} value - Optional value for the command
 */
export function execCommand(command, value = null) {
    document.execCommand(command, false, value);
}

/**
 * Gets the HTML content from a contenteditable element
 * @param {HTMLElement} element - The contenteditable element
 * @returns {string} The HTML content
 */
export function getHtml(element) {
    if (!element) return '';
    return element.innerHTML;
}

/**
 * Sets the HTML content of a contenteditable element
 * @param {HTMLElement} element - The contenteditable element
 * @param {string} html - The HTML content to set
 */
export function setHtml(element, html) {
    if (!element) return;
    element.innerHTML = html || '';
}

/**
 * Inserts HTML at the current cursor position
 * @param {string} html - The HTML to insert
 */
export function insertHtml(html) {
    document.execCommand('insertHTML', false, html);
}

/**
 * Applies text alignment to current selection or paragraph
 * @param {string} alignment - 'left', 'center', or 'right'
 */
export function alignText(alignment) {
    const command = alignment === 'center' ? 'justifyCenter' :
        alignment === 'right' ? 'justifyRight' : 'justifyLeft';
    document.execCommand(command, false, null);
}

/**
 * Checks if a command is active in the current selection
 * @param {string} command - The command to check
 * @returns {boolean} True if the command is active
 */
export function queryCommandState(command) {
    return document.queryCommandState(command);
}

/**
 * Focuses the contenteditable element
 * @param {HTMLElement} element - The contenteditable element
 */
export function focus(element) {
    if (element) {
        element.focus();
    }
}

/**
 * Creates a link from the current selection
 * @param {string} url - The URL for the link
 */
export function createLink(url) {
    document.execCommand('createLink', false, url);
}

/**
 * Inserts an image at the current cursor position
 * @param {string} src - The image source URL
 */
export function insertImage(src) {
    document.execCommand('insertImage', false, src);
}

/**
 * Formats the current block as a heading
 * @param {string} level - The heading level (h1, h2, h3, etc.)
 */
export function formatBlock(level) {
    document.execCommand('formatBlock', false, level);
}

/**
 * Inserts a horizontal rule
 */
export function insertHorizontalRule() {
    document.execCommand('insertHorizontalRule', false, null);
}

/**
 * Toggles ordered list
 */
export function insertOrderedList() {
    document.execCommand('insertOrderedList', false, null);
}

/**
 * Toggles unordered list
 */
export function insertUnorderedList() {
    document.execCommand('insertUnorderedList', false, null);
}

/**
 * Indents the current selection (for blockquote)
 */
export function indent() {
    document.execCommand('indent', false, null);
}

/**
 * Outdents the current selection
 */
export function outdent() {
    document.execCommand('outdent', false, null);
}
