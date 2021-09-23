function changeProjectStatus(projectId, value) {
    let form = document.getElementById("projectPriorityForm");
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