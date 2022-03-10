*** Settings ***
Resource            ../libs/common.robot
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
        IF    "${content_block.type}" == "DataBlockType.FAST_TRACK"
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
    ${filepath}=    user takes screenshot of element
    ...    ${accordion}
    ...    ${content_block.content_block_id}-table.png
    log content block details    ${content_block}    Content Block    ${filepath}

Check Secondary Stats Table
    [Arguments]    ${content_block}

    user navigates to public frontend    ${content_block.content_url}
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
    Log to console    \n\n\n\t=====================================================================\n
    Log to console    \tId:\t\t\t ${content_block.content_block_id}
    Log to console    \tType:\t\t\t ${type_description}
    Log to console    \tURL:\t\t\t ${content_block.content_url}
    Log to console    \tTable snapshot:\t\t ${table_snapshot_filepath}
    Log to console    \n\t=====================================================================\n\n\n
