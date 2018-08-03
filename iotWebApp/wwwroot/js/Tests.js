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
        runTest("getreadings", readingPayload, "Send", null, null, showResult, false, true);
        runTest("handleevents", eventsPayload, "Retrieve", ["deviceID", "time", "reading"], ["Device", "Time", "Reading"], showResult, true, true);
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
        runTest("devicetwin", messagePayload, "Update Device Twin", null, null, showResult, false, true);

    }

    function streamAnalytics() {
        $("#results").text("");
        var readingPayload = {
            iotConnection: $('#hubConnect').val(),
            ehubConnection: $('#eventHubConnect').val(),
            interval: 250,
            iterations: 50
        };
        runTest("getreadings", readingPayload, "Send", null, null, showResult, false, true);

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
            dataType: "json",
            async: async
        }).done(function (data) {
            callBack(data, title, fieldNames, fieldTitles, showData);
        }).fail(function (error) {
            $('#results').append("<div><h3>" + title + " error</h3>" + error.statusText + "</div>");
        });
    }

    function showResult(data, title, fieldsNames, fieldTitles, showData) {
        $('#results').append("<div><h3>" + title + "</h3>" + data.message + "</div>");
        if (data.passed && showData) {
            var dataOut = "<table class='table'><thead><tr>";
            $(fieldTitles).each(function (index, fieldTitle) {
                dataOut += "<th>" + fieldTitle + "</th>";
            });
            dataOut += "</tr></thead><tbody>";
            $(data.data).each(function (index, row) {
                dataOut += "<tr>";
                $(fieldsNames).each(function (index, fieldName) {
                    dataOut += "<td>" + displayData(row,fieldName) + "</td>";
                });
                dataOut += "</tr>";
            });
            dataOut += "</tbody></table>";
            $('#results').append(dataOut);
        }

        function displayData(row, fieldName) {
            var data = row[fieldName];
            var d = new Date(data);
            if (fieldName==="time") {
                // it is a date
                return "" + d.getUTCFullYear() + "." + d.getUTCMonth() + "." + d.getUTCDate() + " " + d.getUTCHours() + ":" + d.getUTCMinutes() + ":" + d.getUTCSeconds();

            } else if (fieldName === "reading") {
                //It is a number
                return data.toFixed(2);
            } else {
                //It is a string
                return data;
            }
        }
    }
});