*** Settings ***
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot
Resource            ../../libs/admin/manage-content-common.robot
Resource            ../../libs/charts.robot
Resource            ../../libs/public-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    AltersData


*** Variables ***
${PUBLICATION_NAME}     UI tests - maps across time %{RUN_IDENTIFIER}
${RELEASE_NAME}         Financial year 3000-01
${DATABLOCK_NAME}=      Dates data block name


*** Test Cases ***
Create new publication and release via API
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    FY    3000

Go to "Release summary" page
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}

Upload subject
    user uploads subject and waits until pending import    Dates test subject    dates.csv    dates.meta.csv
    user confirms upload to begin import    Dates test subject

Add data guidance
    user clicks link    Data guidance
    user waits until h2 is visible    Public data guidance

    user waits until page contains element    id:dataGuidanceForm-content
    user waits until page contains element    id:dataGuidance-dataFiles
    user enters text into element    id:dataGuidanceForm-content    Test data guidance content
    user waits until page contains accordion section    Dates test subject

    user enters text into data guidance data file content editor    Dates test subject
    ...    Dates test subject test data guidance content
    user clicks button    Save guidance

Create data block table
    user creates data block for dates csv    Dates test subject    ${DATABLOCK_NAME}    Dates table title

Create chart for data block
    user waits until page contains link    Chart
    user waits until page finishes loading
    user clicks link    Chart

    user configures basic chart    Geographic    600    600    map chart alt    map chart subtitle
    user selects all data sets for chart
    user waits until page contains element    id:chartBuilderPreview
    user clicks link    Boundary levels
    user chooses select option at index    id:chartBoundaryLevelsConfigurationForm-dataSetConfigs-1-boundaryLevel    2
    user saves chart configuration
    user waits until page finishes loading

Verify map polygons change when selecting a data set with different boundary data
    user waits until h3 is visible    Footnotes
    user chooses select option at index    chartBuilderPreview-map-selectedDataSet    0
    user waits until page finishes loading
    ${firstPolygons}=    user gets map boundary polygon dimensions
    user chooses select option at index    chartBuilderPreview-map-selectedDataSet    1
    user waits until page finishes loading
    ${secondPolygons}=    user gets map boundary polygon dimensions
    IF    "${firstPolygons}"=="${secondPolygons}"
        fail
    END

Verify footnotes update when selecting a data set with different boundary data
    user waits until h3 is visible    Footnotes
    user chooses select option at index    chartBuilderPreview-map-selectedDataSet    0
    user checks element contains    testId:footnotes    This map uses the boundary data Countries UK BUC 2022/12
    user chooses select option at index    chartBuilderPreview-map-selectedDataSet    1
    user waits until page finishes loading
    user checks element contains    testId:footnotes    This map uses the boundary data Countries UK BUC 2017/12
