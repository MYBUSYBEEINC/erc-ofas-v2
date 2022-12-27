var Registration = function () {
    var form = this;

    form._constructor = function () {
        form._events();
        console.log('test test test');

        $(document).ready(function () {
            $('#form-total-p-3 .alert-danger').hide();
        });
    },
    form._events = function () {
        $(document).on("click", ".actions a[href$='#next']", function (e) {

            console.log('test test');

            //var step = parseInt(localStorage.getItem('step'));
            //var otp = form.generateOTP();

            //if (step === 3) {
            //    if ($('#registration-content .mobile-number-text').val().length > 0) {
            //        $.ajax({
            //            type: "POST",
            //            url: "https://app.brandtxt.io/api/v2/SendSMS",
            //            contentType: "application/json",
            //            dataType: 'json',
            //            data: JSON.stringify({
            //                'SenderId': 'BUSYBEE',
            //                'ApiKey': '5f/BmFPxs6Palj2Qm34f+r2005s3gP5pSvcDsQdrlHo=',
            //                'ClientId': '4293fa8e-ee21-4707-a9c4-3c7416617189',
            //                'Message': `Dear Stakeholder, your verification code is ${otp}. Use this to validate your registration.`,
            //                'MobileNumbers': $('#registration-content .mobile-number-text').val()
            //            }),
            //            success: function (response) {
            //                if (response.ErrorCode === 0) {
            //                    localStorage.setItem('otp', otp);
            //                }
            //            }
            //        });
            //    }
            //}
        });

        $(document).on("click", ".actions a[href$='#finish']", function (e) {
            e.preventDefault();
            var generatedOTP = localStorage.getItem('otp');

            if (generatedOTP === $('#registration-content .otp-text').val()) {
                form.register();
            } else {
                $('#form-total-p-3 .alert-danger').show();
                $('#form-total-p-3 .alert-success').hide();
            }

     
            //$("#registration-content .table-data tr").each(function () {
            //    var files = $(this).find('input').prop("files");
            //    var formData = new FormData();

            //    console.log(files.length);

            //    for (var i = 0; i < files.length; i++) {
            //        formData.append("data", files[i]);

            //        //$.ajax({
            //        //    type: 'POST',
            //        //    url: `${baseUrl}/prefilingrequest/upload`,
            //        //    contentType: false,
            //        //    processData: false,
            //        //    cache: false,
            //        //    data: formData,
            //        //    success: function () {
            //        //        $('#pleading-content .alert-upload-document-success').show();
            //        //    },
            //        //    error: function (err) {
            //        //        console.log('There was a problem uploading the file. Please try again later.');
            //        //    },
            //        //    async: true
            //        //});
            //    }
            //});
        });
    }
}

var registration = new Registration();
registration._constructor();