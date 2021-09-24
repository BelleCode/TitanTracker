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
(function () {
    if ($('#ChartistOverlappingBarsOnMobile').get(0)) {
        var data = {
            labels: ['Jan', 'Feb', 'Mar', 'Apr', 'Mai', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
            series: [
                [5, 4, 3, 7, 5, 10, 3, 4, 8, 10, 6, 8],
                [3, 2, 9, 5, 4, 6, 4, 6, 7, 8, 7, 4]
            ]
        };

        var options = {
            seriesBarDistance: 10
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
})();