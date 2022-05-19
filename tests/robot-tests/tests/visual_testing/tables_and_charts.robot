*** Settings ***
Resource    ../libs/common.robot
Resource    ../libs/charts.robot
Library     ../libs/visual.py
Library     tables_and_charts.py

*** Variables ***
${SNAPSHOT_FOLDER}=             test-results/snapshots/%{RUN_IDENTIFIER}
${KEY_STATS_FOLDER}=            key-stats
${SECONDARY_STATS_FOLDER}=      secondary-stats
${CONTENT_SECTIONS_FOLDER}=     content-sections
${FAST_TRACKS_FOLDER}=          fast-tracks

*** Keywords ***
Check release
    [Arguments]    ${release}

    Log to console    \n\n\t=====================================================================\n
    Log to console    \tProcessing release at %{PUBLIC_URL}${release.url}

    user navigates to public frontend    %{PUBLIC_URL}${release.url}

    IF    ${release.has_key_stat_blocks} is ${TRUE}

        Log to console    \n\tCapturing Key Stats:

        user waits until page contains element    xpath://h2[contains(text(), "Headline facts and figures")]
        user scrolls to element    xpath://h2[contains(text(), "Headline facts and figures")]
        user waits until page contains element    id:releaseHeadlines-summary

        FOR    ${content_block}    IN    @{release.key_stat_blocks}
            check key stats table    ${content_block}
        END
    END

    IF    ${release.has_secondary_stat_blocks} is ${TRUE}
        Log to console    \n\tCapturing Secondary Stats:

        FOR    ${content_block}    IN    @{release.secondary_stat_blocks}
            check secondary stats table    ${content_block}
        END
    END

    IF    ${release.has_content_section_blocks} is ${TRUE}
        Log to console    \n\tCapturing Content Section Data Blocks:

        FOR    ${content_block}    IN    @{release.content_section_blocks}
            IF    ${content_block.has_table_config} is ${TRUE}
                check content block table    ${content_block}
            END
        END
    END

    IF    ${release.has_fast_track_blocks} is ${TRUE}
        Log to console    \n\tCapturing Fast Tracks:

        FOR    ${content_block}    IN    @{release.fast_track_blocks}
            check fast track table    ${content_block}
        END
    END

Check Fast Track Table
    [Arguments]    ${content_block}
    user navigates to public frontend    %{PUBLIC_URL}${content_block.content_url}
    user waits until page contains element    id:tableToolWizard
    ${filepath}=    user takes screenshot of element    id:tableToolWizard
    ...    ${SNAPSHOT_FOLDER}/${content_block.release_id}/${FAST_TRACKS_FOLDER}/${content_block.content_block_id}-table.png
    user takes html snapshot of element    id:tableToolWizard
    ...    ${SNAPSHOT_FOLDER}/${content_block.release_id}/${FAST_TRACKS_FOLDER}/${content_block.content_block_id}-table.html
    log content block details    ${content_block}    Fast Track    ${filepath}

Check Content Block Table
    [Arguments]    ${content_block}
    user waits until page contains element    id:content-${content_block.content_section_position}
    ${accordion}=    user opens accordion section with id    content-${content_block.content_section_position}
    ...    id:content
    user scrolls to element    ${accordion}
    user scrolls to the top of the page
    user scrolls to the bottom of the page
    user scrolls to the top of the page
    user scrolls to the bottom of the page
    user waits until parent contains element    ${accordion}    id:dataBlock-${content_block.content_block_id}
    ${data_block}=    get child element    ${accordion}    id:dataBlock-${content_block.content_block_id}
    user waits until parent contains element    ${data_block}
    ...    id:dataBlock-${content_block.content_block_id}-tables-tab
    user scrolls to element    id:dataBlock-${content_block.content_block_id}-tables-tab
    user scrolls down    100
    ${data_block_table_tab}=    get child element    ${data_block}
    ...    id:dataBlock-${content_block.content_block_id}-tables-tab
    user clicks element    ${data_block_table_tab}
    ${table_filepath}=    user takes screenshot of element    ${data_block}
    ...    ${SNAPSHOT_FOLDER}/${content_block.release_id}/${CONTENT_SECTIONS_FOLDER}/${content_block.content_block_id}-table.png
    user takes html snapshot of element    ${data_block}
    ...    ${SNAPSHOT_FOLDER}/${content_block.release_id}/${CONTENT_SECTIONS_FOLDER}/${content_block.content_block_id}-table.html
    ${chart_filepath}=    Set Variable
    IF    ${content_block.has_chart_config} is ${TRUE}
        ${data_block_chart_tab}=    get child element    ${data_block}
        ...    id:dataBlock-${content_block.content_block_id}-charts-tab
        user clicks element    ${data_block_chart_tab}
        user waits for chart to appear    ${content_block.chart_type}    ${data_block}
        ${data_block}=    get child element    ${accordion}    id:dataBlock-${content_block.content_block_id}
        ${chart_filepath}=    user takes screenshot of element    ${data_block}
        ...    ${SNAPSHOT_FOLDER}/${content_block.release_id}/${CONTENT_SECTIONS_FOLDER}/${content_block.content_block_id}-${content_block.chart_type}-chart.png
        user takes html snapshot of element    ${data_block}
        ...    ${SNAPSHOT_FOLDER}/${content_block.release_id}/${CONTENT_SECTIONS_FOLDER}/${content_block.content_block_id}-${content_block.chart_type}-chart.html
    END
    user closes accordion section with id    content-${content_block.content_section_position}    id:content
    log content block details    ${content_block}    Content Block    ${table_filepath}    ${chart_filepath}

Check Secondary Stats Table
    [Arguments]    ${content_block}
    user scrolls to element    id:content
    user waits until page contains element    id:releaseHeadlines-tables-tab
    user scrolls to element    id:releaseHeadlines-tables-tab
    user clicks element    id:releaseHeadlines-tables-tab
    user waits until page contains element    testid:dataTableCaption
    ${filepath}=    user takes screenshot of element    id:releaseHeadlines-tables
    ...    ${SNAPSHOT_FOLDER}/${content_block.release_id}/${SECONDARY_STATS_FOLDER}/${content_block.content_block_id}-table.png
    user takes html snapshot of element    id:releaseHeadlines-tables
    ...    ${SNAPSHOT_FOLDER}/${content_block.release_id}/${SECONDARY_STATS_FOLDER}/${content_block.content_block_id}-table.html
    log content block details    ${content_block}    Secondary Stats Table    ${filepath}

Check Key Stats Table
    [Arguments]    ${content_block}
    user waits until page contains element
    ...    xpath://div[@data-testid="keyStat"][${content_block.content_block_position}]
    user waits until parent contains element
    ...    xpath://div[@data-testid="keyStat"][${content_block.content_block_position}]    testid:keyStat-value
    ${filepath}=    user takes screenshot of element
    ...    xpath://div[@data-testid="keyStat"][${content_block.content_block_position}]
    ...    ${SNAPSHOT_FOLDER}/${content_block.release_id}/${KEY_STATS_FOLDER}/${content_block.content_block_id}-table.png
    user takes html snapshot of element    xpath://div[@data-testid="keyStat"][${content_block.content_block_position}]
    ...    ${SNAPSHOT_FOLDER}/${content_block.release_id}/${KEY_STATS_FOLDER}/${content_block.content_block_id}-table.html
    log content block details    ${content_block}    Key Stats Table    ${filepath}

Log Content Block Details
    [Arguments]
    ...    ${content_block}
    ...    ${type_description}
    ...    ${table_snapshot_filepath}
    ...    ${chart_snapshot_filepath}=
    Log to console    \t\tTable snapshot taken for block ${content_block.content_block_id}
    IF    "${chart_snapshot_filepath}" != ""
        Log to console    \t\tChart snapshot taken for block ${content_block.content_block_id}
    END

user waits for chart to appear
    [Arguments]    ${chart_type}    ${data_block}
    IF    "${chart_type}" == "map"
        user waits until element contains map chart    ${data_block}
        sleep    1
    ELSE IF    "${chart_type}" == "line"
        user waits until element contains line chart    ${data_block}
    ELSE IF    "${chart_type}" == "infographic"
        user waits until element contains infographic chart    ${data_block}
    ELSE IF    "${chart_type}" == "verticalbar"
        user waits until element contains bar chart    ${data_block}
    ELSE IF    "${chart_type}" == "horizontalbar"
        user waits until element contains bar chart    ${data_block}
    ELSE
        Fail    Unhandled chart type ${chart_type}
    END
    user waits until page does not contain loading spinner
