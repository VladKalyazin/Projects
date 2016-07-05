﻿$(document).ready(function ($) {
    $(".clickable-row").click(function () {
        window.document.location = $(this).data("href");
    });
});

$(function () {
    $("#add_project, #add_task, #add_user").dialog
    ({
        modal: true,
        width: 600,
        height: 300,
        draggable: true,
        resizable: false,
        autoOpen: false,
        show: {
            effect: "blind",
            duration: 500
        },
        hide: {
            effect: "blind",
            duration: 500
        }
    });
    $(".ui-dialog-title").html("<i class=\"fa fa-th-list close_popup\" aria-hidden=\"true\"><span class=\"\"> Create new</span></i>");
    $(".ui-dialog-titlebar-close").append("<i class=\"fa fa-times close_popup\" aria-hidden=\"true\"></i>");
});


$(".open").click(function () {
    $("#add_task").css({ 'display': "inline" });
    $("#add_task").dialog("open");
});

$("#opener").click(function () {
    $("#add_project").css({ 'display': "inline" });
    $("#add_project").dialog("open");
});

$("#open_user").click(function () {
    $("#add_user").css({ 'display': "inline" });
    $("#add_user").dialog("open");
});


$(function () {
    $("#edit_project").dialog
    ({
        modal: true,
        width: 600,
        height: 300,
        draggable: true,
        resizable: false,
        autoOpen: false,
        show: {
            effect: "blind",
            duration: 500
        },
        hide: {
            effect: "blind",
            duration: 500
        }
    });
    $(".ui-dialog-title").html("<i class=\"fa fa-list close_popup\" aria-hidden=\"true\"><span class=\"\"> Edit project</span></i>");
    $(".ui-dialog-titlebar-close").append("<i class=\"fa fa-times close_popup\" aria-hidden=\"true\"></i>");
});

$("#open_edit").click(function () {
    $("#edit_project").css({ 'display': "inline" });
    $("#edit_project").dialog("open");
});