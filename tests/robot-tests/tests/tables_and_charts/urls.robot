*** Settings ***
Resource            ../libs/common.robot
Resource            ../libs/charts.robot
Library             ../libs/visual.py
Library             ../libs/tables_and_charts.py

Force Tags          GeneralPublic    Local

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

*** Test Cases ***
Test
    ${content_blocks_info}=    get content blocks

    FOR    ${content_block}    IN    @{content_blocks_info}
        IF    ${content_block.has_table_config} is ${FALSE}
            Log to console    Skipping Content Block ${content_block.content_block_id}
        ELSE IF    "${content_block.type}" == "DataBlockType.FAST_TRACK"
            Check Fast Track Table    ${content_block}
        ELSE IF    "${content_block.type}" == "DataBlockType.CONTENT_BLOCK"
            Check Content Block Table    ${content_block}
        ELSE IF    "${content_block.type}" == "DataBlockType.SECONDARY_STATS"
            Check Secondary Stats Table    ${content_block}
        ELSE IF    "${content_block.type}" == "DataBlockType.KEY_STATS"
            Check Key Stats Table    ${content_block}
        ELSE
            Fail    Unhandled Data Block Type ${content_block.type}
        END
    END

*** Keywords ***
Check Fast Track Table
    [Arguments]    ${content_block}

    user navigates to public frontend    ${content_block.content_url}
    user waits until page contains element    id:tableToolWizard
    ${filepath}=    user takes screenshot of element
    ...    id:tableToolWizard
    ...    ${content_block.content_block_id}-table.png
    log content block details    ${content_block}    Fast Track    ${filepath}

Check Content Block Table
    [Arguments]    ${content_block}

    user navigates to public frontend    ${content_block.content_url}
    ${accordion}=    user opens accordion section    ${content_block.content_section_heading}    id:content
    user scrolls to accordion section content    ${content_block.content_section_heading}    id:content
    user waits until parent contains element    ${accordion}    id:dataBlock-${content_block.content_block_id}
    ${data_block}=    get child element    ${accordion}    id:dataBlock-${content_block.content_block_id}
    user waits until parent contains element    ${data_block}
    ...    id:dataBlock-${content_block.content_block_id}-tables-tab
    ${data_block_table_tab}=    get child element    ${data_block}
    ...    id:dataBlock-${content_block.content_block_id}-tables-tab
    user clicks element    ${data_block_table_tab}
    highlight element    ${data_block}
    ${table_filepath}=    user takes screenshot of element
    ...    ${data_block}
    ...    ${content_block.content_block_id}-table.png
    ${chart_filepath}=    Set Variable
    IF    ${content_block.has_chart_config} is ${TRUE}
        ${data_block_chart_tab}=    get child element    ${data_block}
        ...    id:dataBlock-${content_block.content_block_id}-charts-tab
        user clicks element    ${data_block_chart_tab}
        IF    "${content_block.chart_type}" == "map"
            user waits until element contains map chart    ${data_block}
            user waits until page does not contain loading spinner
        END
        IF    "${content_block.chart_type}" == "line"
            user waits until element contains line chart    ${data_block}
            user waits until page does not contain loading spinner
        END
        ${data_block}=    get child element    ${accordion}    id:dataBlock-${content_block.content_block_id}
        highlight element    ${data_block}
        ${chart_filepath}=    user takes screenshot of element
        ...    ${data_block}
        ...    ${content_block.content_block_id}-${content_block.chart_type}-chart.png

    END
    log content block details    ${content_block}    Content Block    ${table_filepath}    ${chart_filepath}

Check Secondary Stats Table
    [Arguments]    ${content_block}

    user navigates to public frontend    ${content_block.content_url}
    user scrolls to element    id:content
    user waits until page contains element    id:releaseHeadlines-tables-tab
    user scrolls to element    id:releaseHeadlines-tables-tab
    user clicks element    id:releaseHeadlines-tables-tab
    user waits until page contains element    testid:dataTableCaption
    ${filepath}=    user takes screenshot of element
    ...    id:releaseHeadlines-tables
    ...    ${content_block.content_block_id}-table.png
    log content block details    ${content_block}    Secondary Stats Table    ${filepath}

Check Key Stats Table
    [Arguments]    ${content_block}
    user navigates to public frontend    ${content_block.content_url}
    user scrolls to element    xpath://h2[contains(text(), "Headline facts and figures")]
    user waits until page contains element    id:releaseHeadlines-tables-tab
    user waits until page contains element    testid:keyStat-dataBlockId-${content_block.content_block_id}

    ${filepath}=    user takes screenshot of element
    ...    testid:keyStat-dataBlockId-${content_block.content_block_id}
    ...    ${content_block.content_block_id}-table.png
    log content block details    ${content_block}    Key Stats Table    ${filepath}

Log Content Block Details
    [Arguments]
    ...    ${content_block}
    ...    ${type_description}
    ...    ${table_snapshot_filepath}
    ...    ${chart_snapshot_filepath}=
    Log to console    \n\n\n\t=====================================================================\n
    Log to console    \tId:\t\t\t ${content_block.content_block_id}
    Log to console    \tType:\t\t\t ${type_description}
    Log to console    \tURL:\t\t\t ${content_block.content_url}
    Log to console    \tTable snapshot:\t\t ${table_snapshot_filepath}
    IF    "${chart_snapshot_filepath}" != ""
        Log to console    \tChart snapshot:\t\t ${chart_snapshot_filepath}
    END
    Log to console    \n\t=====================================================================\n\n\n
