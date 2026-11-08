const ws = new WebSocket('ws://localhost:3055');

ws.onopen = () => {
    console.log('Connected to TalkToFigma on ws://localhost:3055!');
    // Send a message
    ws.send(JSON.stringify({ id: 1, command: 'get_document_info' }));
};

ws.onmessage = (event) => {
    console.log('Received:', event.data.slice(0, 500));
    ws.close();
    process.exit(0);
};

ws.onerror = (err) => {
    console.error('WS Error:', err);
    process.exit(1);
};

setTimeout(() => {
    console.log('Timeout waiting for response');
    ws.close();
    process.exit(0);
}, 4000);
