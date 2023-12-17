"use strict";
// ReSharper disable PossiblyUnassignedProperty
window.GetScrollTopAsync = function (element) {
    return element.scrollTop;
}
window.SetScrollTopAsync = function(element, scrollTop) {
    element.scrollTop = scrollTop;
}

window.GetChildAsync = function(element, index) {
    return element.children[index];
}

window.SetChildAsync = function(element, index, child) {
    element.children[index] = child;
}

window.GetSelectionStartAsync = function(element) {
    return element.selectionStart;
}

window.SetSelectionStartAsync = function(element, selectionStart) {
    element.selectionStart = selectionStart;
}

window.GetSelectionEndAsync = function(element) {
    return element.selectionEnd;
}

window.SetSelectionEndAsync = function(element, selectionEnd) {
    element.selectionEnd = selectionEnd;
}

window.GetValueAsync = function(element) {
    return element.value;
}

window.SetValueAsync = function(element, value) {
    element.value = value;
}
window.GetOpenAsync = function(element) {
    return element.open;
}

window.SetOpenAsync = function(element, open) {
    element.open = open;
}

window.GetClientXAsync = function(element) {
    return element.getBoundingClientRect().left;
}

window.GetClientYAsync = function(element) {
    return element.getBoundingClientRect().top;
}

window.ClipboardCopyAsync = async function(text) {
    await navigator.clipboard.writeText(text);
}

window.ClipboardPasteAsync = async function() {
    return await navigator.clipboard.readText();
}

window.ScrollIntoViewAsync = function (element, alignToTop) {
    element.scrollIntoView(alignToTop);
}

// ExperimentalAttribute

window.DownloadFileFromStreamAsync = async (fileName, contentStreamReference) => {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    const anchorElement = document.createElement("a");
    anchorElement.href = url;
    anchorElement.download = fileName ?? "";
    anchorElement.click();
    anchorElement.remove();
    URL.revokeObjectURL(url);
}

window.ClickAsync = async (element) => {
    element.click();
}

// ReSharper restore PossiblyUnassignedProperty
