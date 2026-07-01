const fontFamily = 'font-family:"Open Sans","Helvetica Neue",Helvetica,Arial,sans-serif';

//Style added to document when formating for print.
const toastuiStyles = '<style>'
    + '@page{margin:1.5cm}'
    + 'body{font-family:"Open Sans","Helvetica Neue",Helvetica,Arial,sans-serif;font-size:13px;line-height:1.4;color:#424242;max-width:800px;margin:20px auto;padding:0 20px}'
    + 'h1{font-size:24px;font-weight:700;color:#222;border-bottom:3px double #999;padding-bottom:7px;margin:52px 0 15px;line-height:28px}'
    + 'h2{font-size:22px;font-weight:700;color:#222;border-bottom:1px solid #dbdbdb;padding-bottom:7px;margin:20px 0 13px;line-height:23px}'
    + 'h3{font-size:20px;font-weight:700;color:#222;margin:18px 0 2px;line-height:18px}'
    + 'h4{font-size:18px;font-weight:700;color:#222;margin:18px 0 0;line-height:18px}'
    + 'p{margin:4px 0 10px;color:#222;line-height:1.4}'
    + 'a{color:#0066cc;text-decoration:underline}'
    + 'img{max-width:100%;height:auto}'
    + 'blockquote{color:#999;border-left:4px solid #e5e5e5;padding:0 16px;margin:14px 0}'
    + 'table{border-collapse:collapse;border:1px solid #ddd;margin:12px 0 14px;width:auto}'
    + 'th{background:#555;color:#fff;font-weight:300;padding:6px 14px 5px 12px;border:1px solid #ddd}'
    + 'td{padding:5px 14px 5px 12px;border:1px solid #ddd;color:#222}'
    + 'pre{background:#f4f7f8;padding:18px;margin:2px 0 8px;color:#424242;font-family:Consolas,Courier,monospace;white-space:pre-wrap;word-break:break-word;overflow-wrap:anywhere}'
    + 'code{font-family:Consolas,Courier,monospace;font-size:13px;white-space:pre-wrap;word-break:break-word;overflow-wrap:anywhere}'
    + 'ol{margin:6px 0 10px;padding-left:28px;list-style-type:decimal}'
    + 'ul{margin:6px 0 10px;padding-left:28px;list-style-type:disc}'
    + 'li{display:list-item;margin:2px 0;line-height:1.4}'
    + 'th,pre{-webkit-print-color-adjust:exact;print-color-adjust:exact}'
    + '</style>';

// Inline styles applied to elements when copying HTML to clipboard.
const inlineStyles = {
    h1: 'font-size:24px;font-weight:700;color:#222;border-bottom:3px double #999;padding-bottom:7px;margin:14px 0 15px;' + fontFamily + ';line-height:28px',
    h2: 'font-size:22px;font-weight:700;color:#222;border-bottom:1px solid #dbdbdb;padding-bottom:7px;margin:20px 0 13px;' + fontFamily + ';line-height:23px',
    h3: 'font-size:20px;font-weight:700;color:#222;margin:18px 0 2px;' + fontFamily + ';line-height:18px',
    h4: 'font-size:18px;font-weight:700;color:#222;margin:18px 0 0;' + fontFamily + ';line-height:18px',
    p: 'margin:4px 0 10px;color:#222;line-height:1.4',
    blockquote: 'color:#999;border-left:4px solid #e5e5e5;padding:0 16px;margin:14px 0',
    table: 'border-collapse:collapse;border:1px solid #ddd;margin:12px 0 14px;width:auto',
    th: 'background:#555;color:#fff;font-weight:300;padding:6px 14px 5px 12px;border:1px solid #ddd',
    td: 'padding:5px 14px 5px 12px;border:1px solid #ddd;color:#222',
    pre: 'background:#f4f7f8;padding:18px;margin:2px 0 8px;color:#424242;font-family:Consolas,Courier,monospace;white-space:pre-wrap;word-break:break-word;overflow-wrap:anywhere',
    code: 'font-family:Consolas,Courier,monospace;font-size:13px;white-space:pre-wrap;word-break:break-word;overflow-wrap:anywhere',
    ul: 'margin:6px 0 10px;padding-left:28px;line-height:1.2;list-style-type:disc',
    ol: 'margin:6px 0 10px;padding-left:28px;line-height:1.2;list-style-type:decimal',
    li: 'display:list-item;margin:2px 0;line-height:1.4'
};

function stripEditorAttributes(html) {
    const tmp = document.createElement('div');
    tmp.innerHTML = html;
    for (const el of tmp.querySelectorAll('[data-nodeid]')) {
        el.removeAttribute('data-nodeid');
    }
    for (const li of tmp.querySelectorAll('li')) {
        const paragraphs = li.querySelectorAll(':scope > p');
        if (paragraphs.length === 1) {
            const p = paragraphs[0];
            while (p.firstChild) {
                li.insertBefore(p.firstChild, p);
            }
            p.remove();
        }
    }
    return tmp.innerHTML;
}

function applyInlineStyles(html) {
    const tmp = document.createElement('div');
    tmp.innerHTML = stripEditorAttributes(html);

    for (const [tag, style] of Object.entries(inlineStyles)) {
        for (const el of tmp.querySelectorAll(tag)) {
            const existing = el.getAttribute('style');
            el.setAttribute('style', existing ? existing + ';' + style : style);
        }
    }

    for (const h1 of tmp.querySelectorAll('h1')) {
        h1.setAttribute('style', 'color:#222;font-size:24px;font-weight:bold;margin:14px 0 7px;' + fontFamily + ';line-height:28px');
        const hr = document.createElement('hr');
        hr.setAttribute('size', '3');
        hr.setAttribute('noshade', '');
        hr.setAttribute('color', '#999');
        hr.setAttribute('style', 'border:none;height:3px;background:#999;margin:0 0 15px 0');
        h1.insertAdjacentElement('afterend', hr);
    }

    for (const h2 of tmp.querySelectorAll('h2')) {
        h2.setAttribute('style', 'color:#222;font-size:22px;font-weight:bold;margin:20px 0 7px;' + fontFamily + ';line-height:23px');
        const hr = document.createElement('hr');
        hr.setAttribute('size', '1');
        hr.setAttribute('noshade', '');
        hr.setAttribute('color', '#dbdbdb');
        hr.setAttribute('style', 'border:none;height:1px;background:#dbdbdb;margin:0 0 13px 0');
        h2.insertAdjacentElement('afterend', hr);
    }

    for (const h3 of tmp.querySelectorAll('h3')) {
        h3.setAttribute('style', 'color:#222;font-size:20px;font-weight:bold;margin:18px 0 2px;' + fontFamily + ';line-height:18px');
    }

    for (const h4 of tmp.querySelectorAll('h4')) {
        h4.setAttribute('style', 'color:#222;font-size:18px;font-weight:bold;margin:18px 0 0;' + fontFamily + ';line-height:18px');
    }

    for (const blockquote of tmp.querySelectorAll('blockquote')) {
        const wrapper = document.createElement('table');
        wrapper.setAttribute('border', '0');
        wrapper.setAttribute('cellpadding', '0');
        wrapper.setAttribute('cellspacing', '0');
        wrapper.setAttribute('style', 'margin:14px 0;border:none');
        const tr = document.createElement('tr');
        const borderTd = document.createElement('td');
        borderTd.setAttribute('width', '4');
        borderTd.setAttribute('bgcolor', '#e5e5e5');
        borderTd.setAttribute('style', 'background:#e5e5e5;width:4px;font-size:0;line-height:0;padding:0;border:none');
        borderTd.innerHTML = '&nbsp;';
        const contentTd = document.createElement('td');
        contentTd.setAttribute('style', 'padding:0 16px;color:#999;border:none');
        for (const p of blockquote.querySelectorAll('p')) {
            p.setAttribute('style', 'color:#999;margin:0;line-height:1.6');
        }
        contentTd.innerHTML = blockquote.innerHTML;
        tr.appendChild(borderTd);
        tr.appendChild(contentTd);
        wrapper.appendChild(tr);
        blockquote.replaceWith(wrapper);
    }

    for (const table of tmp.querySelectorAll('table')) {
        if (!table.hasAttribute('border')) table.setAttribute('border', '1');
        if (!table.hasAttribute('cellpadding')) table.setAttribute('cellpadding', '6');
        if (!table.hasAttribute('cellspacing')) table.setAttribute('cellspacing', '0');
    }

    for (const th of tmp.querySelectorAll('th')) {
        if (!th.getAttribute('bgcolor')) th.setAttribute('bgcolor', '#555');
        for (const p of th.querySelectorAll('p')) {
            const existing = p.getAttribute('style') || '';
            p.setAttribute('style', existing.replace(/color:\s*#222/, 'color:#fff'));
        }
        wrapChildrenInFont(th, '#ffffff');
    }

    for (const pre of tmp.querySelectorAll('pre')) {
        const wrapper = document.createElement('table');
        wrapper.setAttribute('bgcolor', '#f4f7f8');
        wrapper.setAttribute('cellpadding', '18');
        wrapper.setAttribute('cellspacing', '0');
        wrapper.setAttribute('border', '0');
        wrapper.setAttribute('width', '100%');
        wrapper.setAttribute('style', 'table-layout:fixed;width:100%;border:none');
        const tr = document.createElement('tr');
        const td = document.createElement('td');
        const code = pre.querySelector('code');
        td.setAttribute('style', 'color:#424242;font-family:Consolas,Courier,monospace;padding:18px;white-space:pre-wrap;word-break:break-word;overflow-wrap:anywhere');
        if (code) {
            const codeStyle = code.getAttribute('style') || '';
            td.innerHTML = '<code style="' + codeStyle + '">' + code.innerHTML + '</code>';
        } else {
            td.textContent = pre.textContent;
        }
        tr.appendChild(td);
        wrapper.appendChild(tr);
        pre.replaceWith(wrapper);
    }

    const body = tmp.querySelector('body') || tmp;
    return body.innerHTML;
}

function wrapChildrenInFont(el, color) {
    const font = document.createElement('font');
    font.setAttribute('color', color);
    font.innerHTML = el.innerHTML;
    el.innerHTML = '';
    el.appendChild(font);
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

export function create() {
    return {
        getInnerHtml: (hostElement) => hostElement?.innerHTML ?? '',

        rewriteRelativeUrls: (hostElement, linkBaseHref, imageBaseHref) => {
            if (!hostElement) {
                return;
            }
            rewriteRelativeLinks(hostElement, linkBaseHref);
            rewriteRelativeImages(hostElement, imageBaseHref);
        },

        copyMarkdown: async (instance) => {
            const markdown = instance.getMarkdown();
            await navigator.clipboard.writeText(markdown);
        },

        copyHtml: async (instance) => {
            await navigator.clipboard.writeText(applyInlineStyles(instance.getHTML()));
        },

        copyContent: async (instance) => {
            const html = applyInlineStyles(instance.getHTML());
            const markdown = instance.getMarkdown();

            if (navigator.clipboard?.write && globalThis.ClipboardItem) {
                const clipboardItem = new ClipboardItem({
                    'text/html': new Blob([html], { type: 'text/html' }),
                    'text/plain': new Blob([markdown], { type: 'text/plain' })
                });

                await navigator.clipboard.write([clipboardItem]);
                return;
            }

            console.warn("ClipboardItem API not supported. Falling back to plain text copy.");
            await navigator.clipboard.writeText(markdown);
        },

        printContent: (instance, title) => {
            const html = stripEditorAttributes(instance.getHTML());
            const win = window.open('', '_blank');
            if (!win) {
                throw new Error('Please allow popups to print.');
            }
            win.document.write('<!DOCTYPE html><html><head><meta charset="utf-8"><title>' + (title || 'WrappedToast') + '</title>' + toastuiStyles + '</head><body>' + html + '</body></html>');
            win.document.close();
            win.print();
        }
    };
}
