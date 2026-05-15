// Calendar interactiv programări — REQ-28..32.
// Inițializare FullCalendar cu surse multiple (programări + perioade indisponibile),
// filtre după medic/resursă și refresh automat la fiecare 30s pentru a reflecta
// modificările făcute de alți utilizatori (REQ-32).
(function () {
    'use strict';

    function init() {
        const calendarEl = document.getElementById('calendar');
        if (!calendarEl) return;

        const filtruMedic = document.getElementById('filtruMedic');
        const filtruResursa = document.getElementById('filtruResursa');
        const btnResetFiltre = document.getElementById('btnResetFiltre');
        const indicatorRefresh = document.getElementById('indicatorRefresh');

        const urlEvenimente = calendarEl.dataset.urlEvenimente;
        const urlMentenanta = calendarEl.dataset.urlMentenanta;
        const medicForcat = calendarEl.dataset.medicId;  // pentru vizualizarea Medicului

        function citesteFiltre() {
            return {
                medicId: medicForcat || (filtruMedic ? filtruMedic.value : ''),
                resursaId: filtruResursa ? filtruResursa.value : ''
            };
        }

        const sursaProgramari = {
            id: 'programari',
            url: urlEvenimente,
            method: 'GET',
            extraParams: citesteFiltre,
            failure: function () {
                console.error('Nu am putut încărca programările.');
            }
        };

        const sursaMentenanta = {
            id: 'mentenanta',
            url: urlMentenanta,
            method: 'GET',
            extraParams: function () {
                const f = citesteFiltre();
                return { resursaId: f.resursaId };
            },
            failure: function () {
                console.error('Nu am putut încărca perioadele de mentenanță.');
            }
        };

        const calendar = new FullCalendar.Calendar(calendarEl, {
            initialView: 'timeGridWeek',
            locale: 'ro',
            firstDay: 1,             // Luni
            nowIndicator: true,      // Linie roșie „acum” — vizibil utilă în zilnic/săptămânal
            navLinks: true,          // Click pe nr. zilei = sare în vizualizarea zilei
            weekNumbers: false,
            slotMinTime: '07:00:00',
            slotMaxTime: '21:00:00',
            slotDuration: '00:15:00',
            slotLabelInterval: '01:00',
            allDaySlot: true,
            allDayText: 'Mentenanță',
            height: 'auto',
            expandRows: true,
            headerToolbar: {
                left: 'prev,next today',
                center: 'title',
                right: 'dayGridMonth,timeGridWeek,timeGridDay'   // REQ-28: lunar/săptămânal/zilnic
            },
            buttonText: {
                today: 'Astăzi',
                month: 'Lună',
                week: 'Săptămână',
                day: 'Zi'
            },
            eventSources: [sursaProgramari, sursaMentenanta],
            eventDidMount: function (info) {
                // Tooltip pe hover cu detaliile programării
                const props = info.event.extendedProps || {};
                if (props.tip === 'mentenanta') {
                    info.el.title = `Mentenanță — ${props.resursa}` +
                        (props.descriere ? `\n${props.descriere}` : '');
                    return;
                }
                const linii = [
                    `Pacient: ${props.pacient}`,
                    `Medic: ${props.medic}`,
                    `Tip: ${props.tip}`,
                    `Status: ${props.status}`,
                    props.resursa && props.resursa !== '—' ? `Resursă: ${props.resursa}` : null,
                    props.motiv ? `Motiv: ${props.motiv}` : null
                ].filter(Boolean);
                info.el.title = linii.join('\n');
            },
            eventClick: function (info) {
                // Mentenanța nu e clicabilă — e doar marcaj vizual
                if ((info.event.extendedProps || {}).tip === 'mentenanta') {
                    info.jsEvent.preventDefault();
                }
            }
        });

        calendar.render();

        // Refetch la schimbarea filtrelor (REQ-29, REQ-30)
        function refetch() {
            calendar.refetchEvents();
        }

        if (filtruMedic) filtruMedic.addEventListener('change', refetch);
        if (filtruResursa) filtruResursa.addEventListener('change', refetch);
        if (btnResetFiltre) {
            btnResetFiltre.addEventListener('click', function () {
                if (filtruMedic) filtruMedic.value = '';
                if (filtruResursa) filtruResursa.value = '';
                refetch();
            });
        }

        // Auto-refresh la 30s (REQ-32) — păstrăm calendarul aliniat cu modificările
        // făcute de alți utilizatori în paralel, fără a forța reload de pagină.
        setInterval(function () {
            if (indicatorRefresh) {
                indicatorRefresh.classList.remove('text-muted');
                indicatorRefresh.classList.add('text-success');
                indicatorRefresh.textContent = '⟳ Actualizare...';
            }
            calendar.refetchEvents();
            setTimeout(function () {
                if (indicatorRefresh) {
                    indicatorRefresh.classList.remove('text-success');
                    indicatorRefresh.classList.add('text-muted');
                    indicatorRefresh.textContent = '⟳ Actualizat';
                }
            }, 800);
        }, 30000);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
