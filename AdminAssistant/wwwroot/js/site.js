window.clipboardHelper = {
    copyText: async function (text) {
        try {
            if (navigator.clipboard && navigator.clipboard.writeText) {
                await navigator.clipboard.writeText(text);
                return true;
            } else {
                // Fallback über ein temporäres Textfeld
                const textarea = document.createElement("textarea");
                textarea.value = text;
                textarea.style.position = "fixed";
                textarea.style.left = "-9999px";
                document.body.appendChild(textarea);
                textarea.focus();
                textarea.select();
                const ok = document.execCommand("copy");
                document.body.removeChild(textarea);
                return ok;
            }
        } catch (e) {
            console.error("Clipboard copy failed", e);
            return false;
        }
    }
};
