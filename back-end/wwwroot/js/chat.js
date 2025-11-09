document.addEventListener("DOMContentLoaded", () => {
    // ====== Config ======
    const SAVE_VIA_REST = true; // se il WS già salva nel DB, metti false
    const WS_PATH = "/ws";      // endpoint WebSocket del tuo backend
    const API_MESSAGES = "/api/message";

    // ====== Auth ======
    const token = localStorage.getItem("jwt");
    const userName = localStorage.getItem("username");
    const userId = localStorage.getItem("userId"); // opzionale

    if (!token || !userName) {
        window.location.href = "/";
        return;
    }

    // ====== UI refs ======
    const messagesDiv = document.getElementById("messages");
    const input = document.getElementById("messageInput");
    const sendBtn = document.getElementById("sendButton");

    // evita doppio invio
    let lastSentAt = 0;

    // ====== Helpers UI ======
    function appendMessage(user, text, time) {
        const p = document.createElement("p");
        p.innerHTML = `
          <strong>${escapeHtml(user || "Anonimo")}</strong>: ${escapeHtml(text || "")}
          <span class="text-muted small">(${new Date(time || Date.now()).toLocaleTimeString()})</span>
        `;
        messagesDiv.appendChild(p);
        messagesDiv.scrollTop = messagesDiv.scrollHeight;
    }

    function escapeHtml(str) {
        return String(str)
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#39;");
    }

    // ====== REST: load history ======
    async function loadMessages() {
        try {
            const response = await fetch(`${API_MESSAGES}/user?username=${userName}`, {
                headers: {
                    "Authorization": `Bearer ${token}`,
                    "Content-Type": "application/json"
                }
            });
            if (!response.ok) {
                throw new Error('Failed to fetch messages');
            }
            const messages = await response.json();
            displayMessages(messages);
        } catch (error) {
            console.error('Error loading messages:', error);
        }
    }

    // ✅ Questa era la funzione mancante!
    function displayMessages(messages) {
        messagesDiv.innerHTML = ""; // pulisci la chat
        if (!messages || messages.length === 0) {
            const p = document.createElement("p");
            p.className = "text-muted";
            p.textContent = "Nessun messaggio trovato.";
            messagesDiv.appendChild(p);
            return;
        }

        // mostra tutti i messaggi
        messages.forEach(m => {
            const user = m.userName || m.UserName || "Anonimo";
            const text = m.text || m.Text || "";
            const time = m.time || m.Time || Date.now();
            appendMessage(user, text, time);
        });
    }

    // ====== Logout ======
    document.getElementById("logoutBtn").addEventListener("click", () => {
        localStorage.removeItem("jwt");
        localStorage.removeItem("username");
        localStorage.removeItem("userId");
        window.location.href = "/";
    });

    // ====== WS: connection with auto-retry & keepalive ======
    const protocol = window.location.protocol === "https:" ? "wss" : "ws";
    let ws;
    let reconnectAttempts = 0;
    let pingTimer = null;

    function connectWS() {
        const url = `${protocol}://${window.location.host}${WS_PATH}?token=${encodeURIComponent(token)}`;
        ws = new WebSocket(url);

        ws.onopen = async () => {
            console.log("✅ WS aperto");
            reconnectAttempts = 0;
            sendBtn.disabled = false;

            // carica cronologia appena aperto
            await loadMessages();

            // keep-alive ping ogni 25s
            clearInterval(pingTimer);
            pingTimer = setInterval(() => {
                if (ws.readyState === WebSocket.OPEN) {
                    ws.send(JSON.stringify({ type: "ping", at: Date.now() }));
                }
            }, 25000);
        };

        ws.onmessage = (event) => {
            try {
                const msg = JSON.parse(event.data);

                // ignora eventuali ping/pong
                if (msg.type === "pong" || msg.type === "ping") return;

                const user = msg.UserName ?? msg.userName ?? "Anonimo";
                const text = msg.Text ?? msg.text ?? "";
                const time = msg.Time ?? msg.time ?? Date.now();
                appendMessage(user, text, time);
            } catch (err) {
                console.error("Errore parsing messaggio:", event.data);
            }
        };

        ws.onerror = (err) => {
            console.error("⚠️ Errore WS:", err);
        };

        ws.onclose = () => {
            console.warn("❌ WS chiuso");
            sendBtn.disabled = true;
            clearInterval(pingTimer);

            // backoff esponenziale semplice
            const delay = Math.min(1000 * Math.pow(2, reconnectAttempts), 15000);
            reconnectAttempts++;
            setTimeout(connectWS, delay);
        };
    }

    connectWS();

    // ====== Send message (WS + opzionale REST) ======
    async function sendMessage() {
        const text = input.value.trim();
        if (!text) return;

        const now = Date.now();
        if (now - lastSentAt < 150) return;
        lastSentAt = now;

        const messageWS = { Text: text };
        const messageREST = { UserName: userName, Text: text };

        // 1) invia via WS
        if (ws && ws.readyState === WebSocket.OPEN) {
            try {
                ws.send(JSON.stringify(messageWS));
            } catch (e) {
                console.error("Errore invio WS:", e);
            }
        } else {
            console.warn("WS non aperto, invio solo via REST");
        }

        // 2) mostra subito
        appendMessage(userName, text, new Date());

        // 3) salva via REST (opzionale)
        if (SAVE_VIA_REST) {
            try {
                const res = await fetch(API_MESSAGES + "/add", {
                    method: "POST",
                    headers: {
                        "Authorization": `Bearer ${token}`,
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify(messageREST)
                });
                if (!res.ok) throw new Error(`HTTP ${res.status}`);
            } catch (err) {
                console.error("Errore salvataggio via REST:", err);
            }
        }

        input.value = "";
        input.focus();
    }

    // ====== UI bindings ======
    sendBtn.addEventListener("click", sendMessage);
    input.addEventListener("keydown", (e) => {
        if (e.key === "Enter" && !e.shiftKey) {
            e.preventDefault();
            sendMessage();
        }
    });
});
