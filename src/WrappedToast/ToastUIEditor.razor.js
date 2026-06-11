import {
    copyPlainTextToClipboard,
    copyRichTextToClipboard,
    disposeToastUiInstance,
    getToastUiHtml,
    getToastUiInstance,
    initializeToastUiInstance,
    setToastUiElementStyle
} from './toastui-loader.js';

const editorInstances = new WeakMap();

export async function initialize(editorElement, options) {
    const resolvedOptions = {
        height: "100%",
        initialEditType: 'wysiwyg',
        ...(options ?? {})
    };

    resolvedOptions.el = editorElement;

    await initializeToastUiInstance(
        editorElement,
        editorInstances,
        'editor',
        resolvedOptions,
        (instanceOptions) => new globalThis.toastui.Editor(instanceOptions)
    );
}

export function getMarkdown(editorElement) {
    return getToastUiInstance(editorElement, editorInstances, 'editor').getMarkdown();
}

export function getHTML(editorElement) {
    return getToastUiHtml(
        editorElement,
        getToastUiInstance(editorElement, editorInstances, 'editor')
    );
}

export function setMarkdown(editorElement, markdown) {
    getToastUiInstance(editorElement, editorInstances, 'editor').setMarkdown(markdown);
}

export function insertText(editorElement, text) {
    getToastUiInstance(editorElement, editorInstances, 'editor').insertText(text);
}

export function replaceSelection(editorElement, text, start, end) {
    const instance = getToastUiInstance(editorElement, editorInstances, 'editor');
    instance.replaceSelection(text, start, end);
}

export function getSelectedText(editorElement, start, end) {
    return getToastUiInstance(editorElement, editorInstances, 'editor').getSelectedText(start, end);
}

export function getSelection(editorElement) {
    return getToastUiInstance(editorElement, editorInstances, 'editor').getSelection();
}

export function setSelection(editorElement, start, end) {
    getToastUiInstance(editorElement, editorInstances, 'editor').setSelection(start, end);
}

export function isMarkdownMode(editorElement) {
    return getToastUiInstance(editorElement, editorInstances, 'editor').isMarkdownMode();
}

export function changeMode(editorElement, mode) {
    getToastUiInstance(editorElement, editorInstances, 'editor').changeMode(mode);
}

export function focus(editorElement) {
    getToastUiInstance(editorElement, editorInstances, 'editor').focus();
}

export function moveCursorToEnd(editorElement) {
    getToastUiInstance(editorElement, editorInstances, 'editor').moveCursorToEnd(true);
}

export function moveCursorToStart(editorElement) {
    getToastUiInstance(editorElement, editorInstances, 'editor').moveCursorToStart(true);
}

export async function copyMarkdownToClipboard(editorElement) {
    await copyPlainTextToClipboard(getMarkdown(editorElement));
}

export async function copyHtmlToClipboard(editorElement) {
    await copyRichTextToClipboard(getHTML(editorElement), getMarkdown(editorElement));
}

export function setElementStyle(editorElement, styles) {
    setToastUiElementStyle(editorElement, styles);
}

export function dispose(editorElement) {
    disposeToastUiInstance(editorElement, editorInstances);
}
