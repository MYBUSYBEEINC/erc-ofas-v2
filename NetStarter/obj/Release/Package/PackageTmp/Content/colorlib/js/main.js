$(function() {
    $("#form-total").steps({
        headerTag: "h2",
        bodyTag: "section",
        transitionEffect: "fade",
        enableAllSteps: false,
        autoFocus: true,
        transitionEffectSpeed: 300,
        titleTemplate: '<div class="title">#title#</div>',
        labels: {
            previous: 'Previous',
            next: 'Next Step',
            finish: 'Submit',
            current: ''
        },
        onStepChanging: function (event, currentIndex, newIndex) {
            return true;
        }
    });
});
