export function rewriteRelativeUrls(hostElement, linkBaseHref, imageBaseHref) {
    if (!hostElement) {
        return;
    }

    rewriteRelativeLinks(hostElement, linkBaseHref);
    rewriteRelativeImages(hostElement, imageBaseHref);
}

export function getInnerHtml(hostElement) {
    return hostElement?.innerHTML ?? '';
}

export async function copyPlainTextToClipboard(text) {
    await navigator.clipboard.writeText(text);
}

export async function copyRichTextToClipboard(html, plainText) {
    if (navigator.clipboard?.write && globalThis.ClipboardItem) {
        const clipboardItem = new ClipboardItem({
            'text/html': new Blob([html], { type: 'text/html' }),
            'text/plain': new Blob([plainText], { type: 'text/plain' })
        });

        await navigator.clipboard.write([clipboardItem]);
        return;
    }

    await copyPlainTextToClipboard(plainText);
}

function rewriteRelativeLinks(hostElement, linkBaseHref) {
    if (!linkBaseHref) {
        return;
    }

    const baseUrl = new URL(linkBaseHref, window.location.origin);
    for (const link of hostElement.querySelectorAll('a[href]')) {
        const href = link.getAttribute('href');
        if (!href || !isRelativeUrl(href)) {
            continue;
        }

        const resolved = new URL(href, baseUrl);
        link.setAttribute('href', `${resolved.pathname}${resolved.search}${resolved.hash}`);
    }
}

function rewriteRelativeImages(hostElement, imageBaseHref) {
    if (!imageBaseHref) {
        return;
    }

    const baseUrl = new URL(imageBaseHref, window.location.origin);
    for (const image of hostElement.querySelectorAll('img[src]')) {
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
