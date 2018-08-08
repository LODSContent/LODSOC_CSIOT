$(document).ready(function () {
    $('#sendTelemtry').click(generateData);
    $('#verifyTelemtry').click(verifyDeviceData);
    $('#messageToDevice').click(messageDevice);
    $('#deviceTwin').click(setDeviceTwin);
    $('#streamAnalytics').click(verifyStreamAnalytics);
    $('#consumerGroup').click(verifyConsumerGroups);
    $('#endpoint').click(verifyEndpoint);
    $('#cosmosDB').click(verifyCosmosDB);

    var deviceColumns = [{ ColumnName: "deviceID", Title: "Device" }, { ColumnName: "time", Title: "Time" }, { ColumnName: "reading", Title: "Reading" }];
    var analyticsColumns = [{ ColumnName: "deviceID", Title: "Device" }, { ColumnName: "time", Title: "Time" }, { ColumnName: "averageReading", Title: "Reading" }];

    function generateData() {
        showStart("Generating sample device data");
        runTest("generatedata", getPayload(30, 50), "Generate Device Data", null,  showResult, false, true);
    }

    function verifyDeviceData() {
        showStart("Verifying device data capture");
        runTest("verifyevents", getPayload(0, 0), "Verify Device Data Capture", deviceColumns, showResult, true, true);
    }

    function messageDevice() {
        showStart("Sending message to device Building001");
        runTest("messagetodevice", getPayload(0, 0), "Verify Message to Device", null, showResult, false, false);
    }

    function setDeviceTwin() {
        showStart("Configuring device Building001 via device twin.");
        runTest("devicetwin", getPayload(0, 0), "Verify Device Twin Update", null,  showResult, false, true);
    }

    function verifyStreamAnalytics() {
        $('#currentTest').text("Testing Stream Analytics");
        runTest("streamanalytics", getPayload(250, 50), "Testng Stream analytics results", analyticsColumns, showResult, true, true);
    }


    function verifyConsumerGroups() {
        showStart("Testing Consumer Groups.");
        runTest("consumergroups", getPayload(250, 50), "Test Consumer Groups", deviceColumns,  showResult, true, true);
    }

    function verifyEndpoint() {
        showStart("Testing Endpoint.");
        runTest("endpoint", getPayload(250, 50), "Test Endpoint", deviceColumns, showResult, false, true);
    }

    function verifyCosmosDB() {
        showStart("Testing Cosmos DB.");
        runTest("cosmosdb", getPayload(250, 50), "Cosmos DB", deviceColumns,  showResult, true, true);
    }

    function getPayload(interval, iterations ) {
        return {
            iotConnection: $('#hubConnect').val(),
            ehubConnection: $('#eventHubConnect').val(),
            interval: interval,
            iterations: iterations,
            eventReceiveDelay: 2
        };
    }

    function showStart(text) {
        $('#currentTest').text(text);
        $('#results').text('');
    }

    function runTest(operation, payload, title, fields, callBack, showData, async) {
        var url = "/evaluate/" + operation;// + "?" + queryString + "&encryptionKey=" + encodeURIComponent(key);
        data = JSON.stringify(payload);
        $.post({
            url: url,
            data: payload,
            dataType: "json",
            async: async
        }).done(function (data) {
            callBack(data, title, fields, showData);
        }).fail(function (error) {
            $('#results').append("<div><h3>" + title + " error</h3>" + error.statusText + "</div>");
        });
    }

    function showResult(data, title, fields, showData) {
        $('#results').append("<div><h3>" + title + "</h3>" + data.message + "</div>");
        if (data.passed && showData) {
            var dataOut = "<table class='table'><thead><tr>";
            $(fields).each(function (index, field) {
                dataOut += "<th>" + field.Title + "</th>";
            });
            dataOut += "</tr></thead><tbody>";
            $(data.data).each(function (index, row) {
                dataOut += "<tr>";
                $(fields).each(function (index, field) {
                    dataOut += "<td>" + displayData(row,field.ColumnName) + "</td>";
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