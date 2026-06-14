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

    return {
        getMarkdown: () => instance.getMarkdown(),
        getHTML: () => instance.getHTML(),
        setHTML: (html, cursorToEnd = true) => instance.setHTML(html, cursorToEnd),
        setMarkdown: (markdown, cursorToEnd = true) => instance.setMarkdown(markdown, cursorToEnd),
        insertText: (text) => instance.insertText(text),
        replaceSelection: (text, start, end) => instance.replaceSelection(text, start, end),
        getSelectedText: (start, end) => instance.getSelectedText(start, end),
        getSelection: () => instance.getSelection(),
        setSelection: (start, end) => instance.setSelection(start, end),
        isMarkdownMode: () => instance.isMarkdownMode(),
        isViewer: () => instance.isViewer(),
        isWysiwygMode: () => instance.isWysiwygMode(),
        blur: () => instance.blur(),
        changeMode: (mode, withoutFocus = false) => instance.changeMode(mode, withoutFocus),
        changePreviewStyle: (style) => instance.changePreviewStyle(style),
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
        deleteSelection: (start, end) => instance.deleteSelection(start, end),
        exec: (name, payload) => {
            if (payload === undefined || payload === null) {
                instance.exec(name);
                return;
            }
            instance.exec(name, payload);
        },
        focus: () => instance.focus(),
        getCurrentPreviewStyle: () => instance.getCurrentPreviewStyle(),
        getHeight: () => instance.getHeight(),
        getMinHeight: () => instance.getMinHeight(),
        getRangeInfoOfNode: (pos) => {
            if (pos === undefined || pos === null) {
                return instance.getRangeInfoOfNode();
            }
            return instance.getRangeInfoOfNode(pos);
        },
        getScrollTop: () => instance.getScrollTop(),
        hide: () => instance.hide(),
        moveCursorToEnd: (focus = true) => instance.moveCursorToEnd(focus),
        moveCursorToStart: (focus = true) => instance.moveCursorToStart(focus),
        off: (type) => instance.off(type),
        removeHook: (type) => instance.removeHook(type),
        replaceWithWidget: (start, end, text) => instance.replaceWithWidget(start, end, text),
        reset: () => instance.reset(),
        setHeight: (height) => instance.setHeight(height),
        setMinHeight: (minHeight) => instance.setMinHeight(minHeight),
        setPlaceholder: (placeholder) => instance.setPlaceholder(placeholder),
        setScrollTop: (value) => instance.setScrollTop(value),
        show: () => instance.show(),
        insertToolbarItem: (indexInfo, item) => instance.insertToolbarItem(indexInfo, item),
        removeToolbarItem: (itemName) => instance.removeToolbarItem(itemName),
        setElementStyle: (styles) => setToastUiElementStyle(editorElement, styles),
        dispose: () => disposeToastUiInstance(editorElement, editorInstances)
    };
}
