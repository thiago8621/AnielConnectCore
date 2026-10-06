function readURL(input) {
    if (input.files && input.files[0]) {

        var reader = new FileReader();

        reader.onload = function (e) {
            $('.image-upload-wrap').hide();

            $('.file-upload-image').attr('src', e.target.result);
            $('.file-upload-content').show();

            $('.image-title').html(input.files[0].name);
        };

        reader.readAsDataURL(input.files[0]);

        $('#formuploadlogo').submit();

    } else {
        removeUpload();
    }
}

function removeUpload() {
    $('.file-upload-input').replaceWith($('.file-upload-input').clone());
    $('.file-upload-content').hide();
    $('.image-upload-wrap').show();
}
$('.image-upload-wrap').bind('dragover', function () {
    $('.image-upload-wrap').addClass('image-dropping');
});
$('.image-upload-wrap').bind('dragleave', function () {
    $('.image-upload-wrap').removeClass('image-dropping');
});


function readURLMulti(input) {
    if (input.files && input.files[0]) {

        var reader = new FileReader();

        reader.onload = function (e) {
            $('.image-upload-wrap-multi').hide();

            $('.file-upload-image-multi').attr('src', e.target.result);
            $('.file-upload-content-multi').show();

            $('.image-title-multi').html(input.files[0].name);
        };

        reader.readAsDataURL(input.files[0]);

        $('#formuploadlogo').submit();

    } else {
        removeUploadMulti();
    }
}

function removeUploadMulti() {
    $('.file-upload-input-multi').replaceWith($('.file-upload-input-multi').clone());
    $('.file-upload-content-multi').hide();
    $('.image-upload-wrap-multi').show();
}
$('.image-upload-wrap-multi').bind('dragover', function () {
    $('.image-upload-wrap-multi').addClass('image-dropping');
});
$('.image-upload-wrap-multi').bind('dragleave', function () {
    $('.image-upload-wrap-multi').removeClass('image-dropping');
});

