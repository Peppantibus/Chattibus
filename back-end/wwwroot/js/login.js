document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("loginForm");
    const msg = document.getElementById("login-msg");

    form.addEventListener("submit", async (e) => {
        e.preventDefault();

        const body = {
            username: document.getElementById("login-username").value,
            password: document.getElementById("login-password").value
        };

        try {
            const res = await fetch("/api/auth/login", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(body)
            });

            if (res.ok) {
                const data = await res.json();
                console.log(data)

                // ✅ Salvo i dati utente nel localStorage
                localStorage.setItem("jwt", data.token);
                localStorage.setItem("username", data.username);    // <-- aggiungi questo
                if (data.userId) localStorage.setItem("userId", data.userId); // se nel JSON

                msg.classList.remove("text-danger");
                msg.classList.add("text-success");
                msg.textContent = "Accesso effettuato ✅";

                //✅ Chiudo modale e vado in /chat
                setTimeout(() => {
                    const modal = bootstrap.Modal.getInstance(document.getElementById('loginModal'));
                    if (modal) modal.hide();
                    window.location.href = "/chat"; // <-- porta alla pagina chat vera
                }, 800);

            } else {
                msg.classList.remove("text-success");
                msg.classList.add("text-danger");
                msg.textContent = "Credenziali non valide";
            }
        } catch (error) {
            msg.classList.remove("text-success");
            msg.classList.add("text-danger");
            msg.textContent = "Errore di connessione al server";
        }
    });
});
