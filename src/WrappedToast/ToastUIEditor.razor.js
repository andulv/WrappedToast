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
