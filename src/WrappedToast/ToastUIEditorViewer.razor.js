import {
    copyPlainTextToClipboard,
    copyRichTextToClipboard,
    disposeToastUiInstance,
    getToastUiHtml,
    getToastUiInstance,
    initializeToastUiInstance,
    setToastUiElementStyle
} from './toastui-loader.js';

const viewerInstances = new WeakMap();
const viewerMarkdown = new WeakMap();

export async function initialize(viewerElement, options) {
    const resolvedOptions = {
        height: "100%",
        initialEditType: 'wysiwyg',
        viewer: true,
        ...(options ?? {})
    };

    resolvedOptions.el = viewerElement;

    await initializeToastUiInstance(
        viewerElement,
        viewerInstances,
        'viewer',
        resolvedOptions,
        (instanceOptions) => new globalThis.toastui.Editor.factory(instanceOptions),
        () => viewerMarkdown.set(viewerElement, '')
    );
}

export function getMarkdown(viewerElement) {
    return viewerMarkdown.get(viewerElement) ?? '';
}

export function getHTML(viewerElement) {
    return getToastUiHtml(
        viewerElement,
        getToastUiInstance(viewerElement, viewerInstances, 'viewer')
    );
}

export function setMarkdown(viewerElement, markdown) {
    viewerMarkdown.set(viewerElement, markdown);
    getToastUiInstance(viewerElement, viewerInstances, 'viewer').setMarkdown(markdown);
}

export async function copyMarkdownToClipboard(viewerElement) {
    await copyPlainTextToClipboard(getMarkdown(viewerElement));
}

export async function copyHtmlToClipboard(viewerElement) {
    await copyRichTextToClipboard(getHTML(viewerElement), getMarkdown(viewerElement));
}

export function setElementStyle(viewerElement, styles) {
    setToastUiElementStyle(viewerElement, styles);
}

export function dispose(viewerElement) {
    disposeToastUiInstance(viewerElement, viewerInstances, () => viewerMarkdown.delete(viewerElement));
}
