const { PR } = require("../vendor/datatables/extras/TableTools/pdfmake-0.1.32/pdfmake");

function changeProjectStatus(projectId, value) {
    let form = document.getElementById("projectStatusForm");
    let projectIdInput = document.getElementById("projectId");
    let projectStatusInput = document.getElementById("projectStatus");

    projectIdInput.value = parseInt(projectId);
    projectStatusInput.value = value;

    let formData = $("#projectStatusForm").serialize();

    $.ajax({
        url: $("#projectStatusForm").attr("action"),
        type: 'POST',
        dataType: 'json',
        contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
        data: formData
    });

    form.reset();
}
function changeProjectPriority(projectId, value) {
    let form = document.getElementById("projectPriorityForm");
    let projectIdInput = document.getElementById("priorityprojectId");
    let projectPriorityInput = document.getElementById("projectPriority");

    projectIdInput.value = parseInt(projectId);
    projectPriorityInput.value = value;

    let formData = $("#projectPriorityForm").serialize();

    $.ajax({
        url: $("#projectPriorityForm").attr("action"),
        type: 'POST',
        dataType: 'json',
        contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
        data: formData
    });

    form.reset();
}

/*
Chartist: Bar Chart - Overlapping On Mobile
*/
function mainCall(url1, url2) {
    priorityChart(url1);
    projectStatusChart(url2);
}

function priorityChart(url) {
    $.post(url).then(function (response) {
        if ($('#ChartistOverlappingBarsOnMobile').get(0)) {
            var data = {
                labels: response.labels,
                series: [
                    response.data,
                    response.data
                ]
            };

            var options = {
                seriesBarDistance: 10,
                reverseData: true,
                horizontalBars: true,
                axisY: {
                    offset: 70
                }
            };

            var responsiveOptions = [
                ['screen and (max-width: 640px)', {
                    seriesBarDistance: 5,
                    axisX: {
                        labelInterpolationFnc: function (value) {
                            return value[0];
                        }
                    }
                }]
            ];

            new Chartist.Bar('#ChartistOverlappingBarsOnMobile', data, options, responsiveOptions);
        }
    });
}

function projectStatusChart(url) {
    $.post(url).then(function (response) {
        if ($('#ChartistOverlappingBarsOnMobile2').get(0)) {
            var data = {
                labels: response.labels,
                series: [
                    response.data,
                    response.data
                ]
            };

            var options = {
                seriesBarDistance: 10,
                reverseData: true,
                horizontalBars: true,
                axisY: {
                    offset: 70
                }
            };

            var responsiveOptions = [
                ['screen and (max-width: 640px)', {
                    seriesBarDistance: 5,
                    axisX: {
                        labelInterpolationFnc: function (value) {
                            return value[0];
                        }
                    }
                }]
            ];

            new Chartist.Bar('#ChartistOverlappingBarsOnMobile2', data, options, responsiveOptions);
        }
    });
}