*** Settings ***
Resource    ../libs/common.robot
Resource    ../libs/charts.robot
Library     ../libs/visual.py
Library     tables_and_charts.py


*** Variables ***
${SNAPSHOT_FOLDER}=         test-results/snapshots/%{RUN_IDENTIFIER}
${PERMALINKS_FOLDER}=       permalinks


*** Keywords ***
Check permalink with id
    [Arguments]    ${permalink_id}
    ${permalink_url}=    Set Variable    %{PUBLIC_URL}/data-tables/permalink/${permalink_id}

    Log to console    \n\n\t=====================================================================\n
    Log to console    \tProcessing permalink at ${permalink_url}

    user navigates to public frontend    ${permalink_url}

    Log to console    \n\tCapturing Permalink:

    user waits until page contains element    css:figure
    ${filepath}=    user takes screenshot of element    css:figure
    ...    ${SNAPSHOT_FOLDER}/${PERMALINKS_FOLDER}/${permalink_id}-table.png
    user takes html snapshot of element    css:figure
    ...    ${SNAPSHOT_FOLDER}/${PERMALINKS_FOLDER}/${permalink_id}-table.html
    log permalink details    ${permalink_url}

Log Permalink Details
    [Arguments]
    ...    ${permalink_url}
    Log to console    \t\tPermalink snapshot taken for permalink ${permalink_url}
