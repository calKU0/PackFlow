window.sendZplToAgent = async function (printer, dataType, content, parameters) {
    try {
        const controller = new AbortController();
        const timeout = setTimeout(() => controller.abort(), 3000);

        const body = {
            PrinterName: printer,
            DataType: dataType,
            Content: content,
            Parameters: parameters
        };

        const response = await fetch("http://localhost:54321/print/", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body),
            signal: controller.signal
        });

        clearTimeout(timeout);
        return response.ok;
    } catch (err) {
        console.error("Print agent error:", err);
        return false;
    }
};

window.isAgentRunning = async () => {
    try {
        const response = await fetch("http://localhost:54321/print/", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ PrinterName: "ping", DataType: "PING", Content: "" })
        });
        return response.ok;
    } catch {
        return false;
    }
};