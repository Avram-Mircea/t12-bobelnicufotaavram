// Auto-formatare câmpuri data: utilizatorul tastează doar cifre,
// separatorii "/" apar singuri la pozițiile 2 și 5 (zz/ll/aaaa).
(function () {
    'use strict';

    function formateazaData(cifre) {
        cifre = cifre.replace(/\D/g, '').slice(0, 8);
        if (cifre.length >= 5) {
            return cifre.slice(0, 2) + '/' + cifre.slice(2, 4) + '/' + cifre.slice(4);
        }
        if (cifre.length >= 3) {
            return cifre.slice(0, 2) + '/' + cifre.slice(2);
        }
        return cifre;
    }

    document.addEventListener('DOMContentLoaded', function () {
        document.querySelectorAll('input.date-auto-format').forEach(function (el) {
            el.addEventListener('input', function () {
                el.value = formateazaData(el.value);
            });

            // La backspace pe separator, ștergem și cifra anterioară (UX natural).
            el.addEventListener('keydown', function (e) {
                if (e.key === 'Backspace') {
                    const pos = el.selectionStart;
                    if (pos > 0 && el.value[pos - 1] === '/') {
                        e.preventDefault();
                        el.value = el.value.slice(0, pos - 2) + el.value.slice(pos);
                        el.setSelectionRange(pos - 2, pos - 2);
                    }
                }
            });
        });

        // Acceptăm formatul românesc dd/MM/yyyy în loc de validarea default
        // jQuery Validate (care folosește new Date() — nu înțelege "/" + zz/ll).
        if (typeof window.jQuery !== 'undefined' && window.jQuery.validator) {
            window.jQuery.validator.methods.date = function (value, element) {
                if (this.optional(element)) return true;
                const m = /^(\d{2})\/(\d{2})\/(\d{4})$/.exec(value);
                if (!m) return false;
                const zi = parseInt(m[1], 10);
                const luna = parseInt(m[2], 10);
                const an = parseInt(m[3], 10);
                if (luna < 1 || luna > 12) return false;
                if (zi < 1 || zi > 31) return false;
                if (an < 1900 || an > 2100) return false;
                const d = new Date(an, luna - 1, zi);
                return d.getFullYear() === an
                    && d.getMonth() === luna - 1
                    && d.getDate() === zi;
            };
        }
    });
})();
