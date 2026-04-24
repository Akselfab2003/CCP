window.scrollHelpers = {
    scrollToBottom: function (element) {
        if (element) element.scrollTop = element.scrollHeight;
    },
    getScrollTop: function (element) {
        return element ? element.scrollTop : 0;
    },
    getScrollHeight: function (element) {
        return element ? element.scrollHeight : 0;
    },
    preserveScrollPosition: function (element, previousScrollHeight) {
        if (element) {
            element.scrollTop += element.scrollHeight - previousScrollHeight;
        }
    }
};
