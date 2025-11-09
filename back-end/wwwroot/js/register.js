document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("registerForm");
    const msg = document.getElementById("register-msg");

    form.addEventListener("submit", async (e) => {
        e.preventDefault();

        const user = {
            userName: document.getElementById("reg-username").value,
            email: document.getElementById("reg-email").value,
            password: document.getElementById("reg-password").value
        };

        try {
            const res = await fetch("/api/auth/register", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(user)
            });

            if (res.ok) {
                msg.classList.remove("text-danger");
                msg.classList.add("text-success");
                msg.textContent = "Registrazione completata ✅";
                form.reset();

                // Chiudi modale dopo un breve delay e apri login
                setTimeout(() => {
                    const modal = bootstrap.Modal.getInstance(document.getElementById('registerModal'));
                    if (modal) modal.hide();
                    new bootstrap.Modal(document.getElementById('loginModal')).show();
                }, 1000);
            } else {
                const err = await res.text();
                msg.classList.remove("text-success");
                msg.classList.add("text-danger");
                msg.textContent = err || "Errore nella registrazione";
            }
        } catch (error) {
            msg.classList.remove("text-success");
            msg.classList.add("text-danger");
            msg.textContent = "Errore di connessione al server";
        }
    });
});
