// Feedback live pentru câmpurile de parolă.
// Sincronizat cu StrongPasswordAttribute.cs (server-side).
(function () {
    // ── 1. Verificare cerințe parolă (lungime + simbol) ───────────────────────
    function checkStrength(input, container) {
        const lenReq = container.querySelector('[data-req="length"]');
        const symReq = container.querySelector('[data-req="symbol"]');
        const val = input.value;

        const hasLength = val.length >= 8;
        const hasSymbol = /[^A-Za-z0-9]/.test(val);

        if (lenReq) {
            lenReq.innerHTML = (hasLength ? '✅' : '⭕') + ' Minim 8 caractere';
            lenReq.classList.toggle('text-success', hasLength);
            lenReq.classList.toggle('text-muted', !hasLength);
        }
        if (symReq) {
            symReq.innerHTML = (hasSymbol ? '✅' : '⭕') + ' Cel puțin un simbol (! @ # $ % & * ...)';
            symReq.classList.toggle('text-success', hasSymbol);
            symReq.classList.toggle('text-muted', !hasSymbol);
        }

        input.classList.remove('is-valid', 'is-invalid');
        if (val.length > 0) {
            input.classList.add(hasLength && hasSymbol ? 'is-valid' : 'is-invalid');
        }
    }

    // ── 2. Verificare match între parolă și confirmare ───────────────────────
    function findFeedback(input) {
        // Caută elementul cu clasa .password-match-feedback în cel mai apropiat
        // container .mb-3 (form-group standard Bootstrap)
        const container = input.closest('.mb-3') || input.parentElement;
        return container ? container.querySelector('.password-match-feedback') : null;
    }

    function checkMatch(confirmInput, originalInput) {
        const feedback = findFeedback(confirmInput);
        const valConfirm = confirmInput.value;
        const valOriginal = originalInput.value;

        confirmInput.classList.remove('is-valid', 'is-invalid');
        if (feedback) {
            feedback.classList.remove('text-success', 'text-danger');
            feedback.innerHTML = '';
        }

        if (valConfirm.length === 0) return;

        if (valConfirm === valOriginal) {
            confirmInput.classList.add('is-valid');
            if (feedback) {
                feedback.classList.add('text-success');
                feedback.innerHTML = '✅ Parolele coincid';
            }
        } else {
            confirmInput.classList.add('is-invalid');
            if (feedback) {
                feedback.classList.add('text-danger');
                feedback.innerHTML = '❌ Parolele nu coincid';
            }
        }
    }

    // ── 3. Inițializare la încărcarea paginii ────────────────────────────────
    document.addEventListener('DOMContentLoaded', function () {
        // Verificare cerințe
        document.querySelectorAll('[data-password-strength]').forEach(function (input) {
            const targetId = input.getAttribute('data-password-strength');
            const container = document.getElementById(targetId);
            if (!container) return;
            input.addEventListener('input', function () { checkStrength(input, container); });
            checkStrength(input, container);
        });

        // Verificare match
        document.querySelectorAll('[data-password-match]').forEach(function (confirmInput) {
            const targetId = confirmInput.getAttribute('data-password-match');
            const originalInput = document.getElementById(targetId);
            if (!originalInput) return;

            confirmInput.addEventListener('input', function () { checkMatch(confirmInput, originalInput); });
            // Re-verifică și când utilizatorul modifică parola originală
            originalInput.addEventListener('input', function () { checkMatch(confirmInput, originalInput); });
            checkMatch(confirmInput, originalInput);
        });
    });
})();
