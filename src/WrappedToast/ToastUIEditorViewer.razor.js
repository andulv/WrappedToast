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
const viewerLinkBaseHrefs = new WeakMap();
const viewerImageBaseHrefs = new WeakMap();

export async function initialize(viewerElement, options) {
    const resolvedOptions = {
        height: "100%",
        initialEditType: 'wysiwyg',
        viewer: true,
        ...(options ?? {})
    };

    const linkBaseHref = resolvedOptions.linkBaseHref;
    const imageBaseHref = resolvedOptions.imageBaseHref;
    delete resolvedOptions.linkBaseHref;
    delete resolvedOptions.imageBaseHref;
    resolvedOptions.el = viewerElement;
    setLinkBaseHref(viewerElement, linkBaseHref);
    setImageBaseHref(viewerElement, imageBaseHref);

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
    rewriteRelativeLinks(viewerElement);
    rewriteRelativeImages(viewerElement);
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
    disposeToastUiInstance(viewerElement, viewerInstances, () => {
        viewerMarkdown.delete(viewerElement);
        viewerLinkBaseHrefs.delete(viewerElement);
        viewerImageBaseHrefs.delete(viewerElement);
    });
}

export function setLinkBaseHref(viewerElement, linkBaseHref) {
    if (!linkBaseHref) {
        viewerLinkBaseHrefs.delete(viewerElement);
        return;
    }

    viewerLinkBaseHrefs.set(viewerElement, linkBaseHref);
}

export function setImageBaseHref(viewerElement, imageBaseHref) {
    if (!imageBaseHref) {
        viewerImageBaseHrefs.delete(viewerElement);
        return;
    }

    viewerImageBaseHrefs.set(viewerElement, imageBaseHref);
}

function rewriteRelativeLinks(viewerElement) {
    const linkBaseHref = viewerLinkBaseHrefs.get(viewerElement);
    if (!linkBaseHref) {
        return;
    }

    const baseUrl = new URL(linkBaseHref, window.location.origin);
    for (const link of viewerElement.querySelectorAll('a[href]')) {
        const href = link.getAttribute('href');
        if (!href || !isRelativeUrl(href)) {
            continue;
        }

        const resolved = new URL(href, baseUrl);
        link.setAttribute('href', `${resolved.pathname}${resolved.search}${resolved.hash}`);
    }
}

function rewriteRelativeImages(viewerElement) {
    const imageBaseHref = viewerImageBaseHrefs.get(viewerElement);
    if (!imageBaseHref) {
        return;
    }

    const baseUrl = new URL(imageBaseHref, window.location.origin);
    for (const image of viewerElement.querySelectorAll('img[src]')) {
        const src = image.getAttribute('src');
        if (!src || !isRelativeUrl(src)) {
            continue;
        }

        const resolved = new URL(src, baseUrl);
        image.setAttribute('src', `${resolved.pathname}${resolved.search}${resolved.hash}`);
    }
}

function isRelativeUrl(href) {
    return !href.startsWith('/')
        && !href.startsWith('#')
        && !href.startsWith('//')
        && !/^[a-z][a-z0-9+.-]*:/i.test(href);
}
