import {
    disposeToastUiInstance,
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

    const instance = getToastUiInstance(editorElement, editorInstances, 'editor');
    const toBlob = (str) => new Blob([new TextEncoder().encode(str)]);

    return {
        // Return the markdown as a Blob to avoid issues with large content and JS interop string size limits.
        getMarkdown: () => toBlob(instance.getMarkdown()),
        getHTML: () => toBlob(instance.getHTML()),
        getSelectedText: (start, end) => toBlob(instance.getSelectedText(start, end)),

        setHTML: (html, cursorToEnd = true) => instance.setHTML(html, cursorToEnd),
        setMarkdown: (markdown, cursorToEnd = true) => instance.setMarkdown(markdown, cursorToEnd),
        insertText: (text) => instance.insertText(text),
        replaceSelection: (text, start, end) => instance.replaceSelection(text, start, end),

        isMarkdownMode: () => instance.isMarkdownMode(),
        isViewer: () => instance.isViewer(),
        isWysiwygMode: () => instance.isWysiwygMode(),

        blur: () => instance.blur(),
        show: () => instance.show(),
        hide: () => instance.hide(),
        focus: () => instance.focus(),

        getHeight: () => instance.getHeight(),
        setHeight: (height) => instance.setHeight(height),
        getMinHeight: () => instance.getMinHeight(),
        setMinHeight: (minHeight) => instance.setMinHeight(minHeight),

        getScrollTop: () => instance.getScrollTop(),
        setScrollTop: (value) => instance.setScrollTop(value),

        changeMode: (mode, withoutFocus = false) => instance.changeMode(mode, withoutFocus),
        getCurrentPreviewStyle: () => instance.getCurrentPreviewStyle(),
        changePreviewStyle: (style) => instance.changePreviewStyle(style),

        getSelection: () => instance.getSelection(),
        setSelection: (start, end) => instance.setSelection(start, end),
        deleteSelection: (start, end) => instance.deleteSelection(start, end),
        moveCursorToEnd: (focus = true) => instance.moveCursorToEnd(focus),
        moveCursorToStart: (focus = true) => instance.moveCursorToStart(focus),

        convertPosToMatchEditorMode: (start, end, mode) => {
            const args = [start];
            if (end !== undefined && end !== null) {
                args.push(end);
            }
            if (mode !== undefined && mode !== null) {
                if (args.length === 1) {
                    args.push(start);
                }
                args.push(mode);
            }
            return instance.convertPosToMatchEditorMode(...args);
        },


        exec: (name, payload) => {
            if (payload === undefined || payload === null) {
                instance.exec(name);
                return;
            }
            instance.exec(name, payload);
        },
        getRangeInfoOfNode: (pos) => {
            if (pos === undefined || pos === null) {
                return instance.getRangeInfoOfNode();
            }
            return instance.getRangeInfoOfNode(pos);
        },

        setElementStyle: (styles) => setToastUiElementStyle(editorElement, styles),
        setPlaceholder: (placeholder) => instance.setPlaceholder(placeholder),
        insertToolbarItem: (indexInfo, item) => instance.insertToolbarItem(indexInfo, item),
        removeToolbarItem: (itemName) => instance.removeToolbarItem(itemName),

        off: (type) => instance.off(type),
        removeHook: (type) => instance.removeHook(type),
        replaceWithWidget: (start, end, text) => instance.replaceWithWidget(start, end, text),
        reset: () => instance.reset(),
        dispose: () => disposeToastUiInstance(editorElement, editorInstances)
    };
}
