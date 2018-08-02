$(document).ready(function () {
    $('#telemtry').click(getReadings);
    $('#messageToDevice').click(messageDevice);
    $('#deviceTwin').click(setDeviceTwin);
    $('#streamAnalytics').click(streamAnalytics);


    function getReadings() {
        $("#results").text("");
        var readingPayload = {
            iotConnection: $('#hubConnect').val(),
            ehubConnection: $('#eventHubConnect').val(),
            interval: 15,
            iterations: 50
        };
        var eventsPayload = {
            hubName: "-1",
            ehubConnection: $('#eventHubConnect').val(),
            iotConnection: $('#hubConnect').val(),
            eventReceiveDelay: 10

        };
        //runTest("emptyTest", "", "Web API test", null, null, showResult, false, true);
        runTest("getreadings", readingPayload, "Send", null, null, showResult,false, true);
        runTest("handleevents", eventsPayload, "Retrieve", ["DeviceID", "Time", "Reading"], ["Device", "Time", "Reading"], showResult, true, true);
    }

    function messageDevice() {
        var messagePayload = {
            iotConnection: $('#hubConnect').val(),
            ehubConnection: $('#eventHubConnect').val()
        };
        runTest("messagetodevice", messagePayload, "Message to Device", null, null, showResult, false, false);


    }

    function setDeviceTwin() {
        var messagePayload = {
            iotConnection: $('#hubConnect').val(),
            ehubConnection: $('#eventHubConnect').val()
        };
        runTest("devicetwin", messagePayload, "Update Device Twin", null, null, showResult, false,true);

    }

    function streamAnalytics() {
        $("#results").text("");
        var readingPayload = {
            iotConnection: $('#hubConnect').val(),
            ehubConnection: $('#eventHubConnect').val(),
            interval: 250,
            iterations: 50
        };
         runTest("getreadings", readingPayload, "Send", null, null, showResult, false,true);

    }

    function consumerGroups() {

    }

    function timeSeries() {

    }

    function runTest(operation, payload, title, fieldNames, fieldTitles, callBack, showData, async) {
        var url = "/evaluate/" + operation;// + "?" + queryString + "&encryptionKey=" + encodeURIComponent(key);
        data = JSON.stringify(payload);
        $.post({
            url: url,
            data: payload,
            dataType:"json",
            async: async
        }).done(function (data) {
            callBack(data, title, fieldNames, fieldTitles, showData);
        }).fail(function (error) {
            $('#results').append("<div><h3>" + title + " error</h3>" + error.statusText + "</div>");
        });
    }

    function showResult(data, title, fieldsNames, fieldTitles, showData) {
        $('#results').append("<div><h3>" + title + "</h3>" + data.Message + "</div>");
        if (data.Passed && showData) {
            var dataOut = "<table><tr>";
            $(fieldTitles).each(function (index, fieldTitle) {
                dataOut += "<th>" + fieldTitle + "</th>";
            });
            dataOut += "</tr>";
            $(data.Data).each(function (index, row) {
                dataOut += "<tr>";
                $(fieldsNames).each(function (index, fieldName) {
                    dataOut += "<td>" + row[fieldName] + "</td>";
                });
                dataOut += "</tr>";
            });
            dataOut += "</table>";
            $('#results').append(dataOut);
        }
    }
});