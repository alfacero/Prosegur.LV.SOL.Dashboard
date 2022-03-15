$.ajaxSetup({
    statusCode: {

        401: function (data) {
            location.reload();
        }

    }
});

$.validator.methods.date = function (value, element) {
    if ($(element).attr('data-val-date')) {
        if (!/^\d{1,2}\/\d{1,2}\/\d{4}$/.test(value))
            return false;

        // Parse the date parts to integers
        var parts = value.split("/");
        var day = parseInt(parts[0], 10);
        var month = parseInt(parts[1], 10);
        var year = parseInt(parts[2], 10);

        // Check the ranges of month and year
        if (year < 1000 || year > 3000 || month == 0 || month > 12)
            return false;

        var monthLength = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];

        // Adjust for leap years
        if (year % 400 == 0 || (year % 100 != 0 && year % 4 == 0))
            monthLength[1] = 29;

        // Check the range of the day
        return day > 0 && day <= monthLength[month - 1];
    }
    // use the default method
    return this.optional(element) || (value >= param[0] && value <= param[1]);
};