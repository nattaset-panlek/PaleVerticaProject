const fs = require('fs');
const path = require('path');

function callFigma(command, params) {
    return new Promise((resolve, reject) => {
        const ws = new WebSocket('ws://localhost:3055');
        const reqId = 'req_' + Date.now() + '_' + Math.random().toString(36).substring(2, 7);

        const timer = setTimeout(() => {
            ws.close();
            reject(new Error(`Timeout waiting for Figma response to ${command}`));
        }, 15000);

        ws.onopen = () => {
            ws.send(JSON.stringify({ id: 'join_1', type: 'join', channel: 'flowux' }));
            setTimeout(() => {
                ws.send(JSON.stringify({
                    id: reqId,
                    type: 'message',
                    channel: 'flowux',
                    message: {
                        id: reqId,
                        command: command,
                        params: { ...params, commandId: reqId }
                    }
                }));
            }, 300);
        };

        ws.onmessage = (event) => {
            try {
                const data = JSON.parse(event.data);
                if (data.message && data.message.id === reqId) {
                    clearTimeout(timer);
                    ws.close();
                    if (data.message.error) {
                        reject(new Error(data.message.error));
                    } else {
                        resolve(data.message.result);
                    }
                }
            } catch (e) {
                // Ignore parse errors from system messages
            }
        };

        ws.onerror = (err) => {
            clearTimeout(timer);
            reject(err);
        };
    });
}

async function exportNodeImage(nodeId, format = 'PNG', scale = 1, savePath) {
    const res = await callFigma('export_node_as_image', { nodeId, format, scale });
    if (!res || !res.imageData) {
        throw new Error(`No image data returned for node ${nodeId}`);
    }
    const buf = Buffer.from(res.imageData, 'base64');
    fs.mkdirSync(path.dirname(savePath), { recursive: true });
    fs.writeFileSync(savePath, buf);
    console.log(`Exported node ${nodeId} (${format} @ ${scale}x) -> ${savePath} (${buf.length} bytes)`);
    return savePath;
}

async function getNodeInfo(nodeId) {
    return await callFigma('get_node_info', { nodeId });
}

module.exports = {
    callFigma,
    exportNodeImage,
    getNodeInfo
};

if (require.main === module) {
    const action = process.argv[2];
    const nodeId = process.argv[3];
    const outPath = process.argv[4];

    if (action === 'info' && nodeId) {
        getNodeInfo(nodeId).then(info => console.log(JSON.stringify(info, null, 2))).catch(console.error);
    } else if (action === 'export' && nodeId && outPath) {
        const fmt = process.argv[5] || 'PNG';
        const sc = parseFloat(process.argv[6] || '1');
        exportNodeImage(nodeId, fmt, sc, outPath).catch(console.error);
    } else {
        console.log('Usage:');
        console.log('  node figma_client.js info <nodeId>');
        console.log('  node figma_client.js export <nodeId> <outPath> [format=PNG] [scale=1]');
    }
}
