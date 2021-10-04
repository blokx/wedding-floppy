jQuery(document).ready(function() {
    // Handler for .ready() called.

    // https://stackoverflow.com/questions/17687619/
    $ = jQuery.noConflict(true);

    var urlParams = new URLSearchParams(window.location.search);
    var name = urlParams.get('guest');
    var code = urlParams.get('code');
    var key = urlParams.get('key');

    // alert('Hallo ' + name);

    var btnQuiz = $('#btn-quiz');
    var btnDownload = $('#btn-download');
    var quizUrl = 'https://docs.google.com/forms/d/e/1FAIpQLSfRUFQtxlINzfFrls0KuaigdLloFFNp10HVFV4qc_VeGxZPtg/viewform?usp=pp_url&entry.1168935941=' + code;

    btnQuiz.click(function () {

        if(code == null || code.length <3 || code.length > 3)
        {
            alert("Ungültige Gäste-ID");
            return;
        }

        var win = window.open(quizUrl, '_blank');
        if (win) {
            //Browser has allowed it to be opened
            win.focus();
        } else {
            //Browser has blocked it
            alert('Bitte Popups aktivieren');
        }
    });

    btnDownload.click(function () {
        window.alert('Die Bilder werden gerade von der Fotografin bearbeitet.\n' +
            'Schickt uns gern auch eure Schnappschüsse (per WhatsApp oder Email)');
    });

});