// La schimbarea medicului din form-ul de programare, refiltrăm dropdown-ul de resurse
// pentru a afișa doar pe cele compatibile cu specializarea aleasă (REQ-15).
(function () {
    'use strict';

    function refreshResurse(medicId, resursaSelect, valoareCurenta) {
        const ts = resursaSelect.tomselect;
        if (!ts) return;

        if (!medicId) {
            ts.clear(true);
            ts.clearOptions();
            ts.addOption({ value: '', text: '— Alege medicul mai întâi —' });
            ts.refreshOptions(false);
            ts.disable();
            return;
        }

        ts.enable();
        ts.clear(true);
        ts.clearOptions();
        ts.addOption({ value: '', text: '— Fără resursă —' });

        fetch(`/Programare/ResurseCompatibile?medicId=${encodeURIComponent(medicId)}`, {
            headers: { 'Accept': 'application/json' }
        })
            .then(r => r.ok ? r.json() : [])
            .then(data => {
                data.forEach(r => ts.addOption({ value: String(r.id), text: r.text }));
                ts.refreshOptions(false);
                // Restaurăm selecția dacă valoarea curentă e încă validă
                if (valoareCurenta && data.some(r => String(r.id) === String(valoareCurenta))) {
                    ts.setValue(String(valoareCurenta), true);
                }
            })
            .catch(err => console.error('Eroare la încărcare resurse:', err));
    }

    document.addEventListener('DOMContentLoaded', function () {
        const medicSelect = document.getElementById('MedicId');
        const resursaSelect = document.getElementById('ResursaId');

        if (!medicSelect || !resursaSelect) return;

        // Așteaptă inițializarea Tom Select — care se face tot în DOMContentLoaded
        function init() {
            if (!medicSelect.tomselect || !resursaSelect.tomselect) {
                setTimeout(init, 50);
                return;
            }

            const valoareInitialaResursa = resursaSelect.value;

            // Pe schimbare medic
            medicSelect.addEventListener('change', function () {
                refreshResurse(medicSelect.value, resursaSelect, null);
            });

            // La load (pt Edit, sau dacă vine cu MedicId pre-selectat)
            if (medicSelect.value) {
                refreshResurse(medicSelect.value, resursaSelect, valoareInitialaResursa);
            } else {
                refreshResurse(null, resursaSelect, null);
            }
        }
        init();
    });
})();
