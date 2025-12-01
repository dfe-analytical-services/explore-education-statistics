*** Settings ***
Resource    ./common.robot


*** Keywords ***
user waits until element contains line chart
    [Arguments]    ${locator}
    user waits until parent contains element    ${locator}    css:.recharts-line

user waits until element does not contain line chart
    [Arguments]    ${locator}
    user waits until parent does not contain element    ${locator}    css:.recharts-line

user waits until element contains bar chart
    [Arguments]    ${locator}
    user waits until parent contains element    ${locator}    css:.recharts-bar-rectangles

user waits until element does not contain bar chart
    [Arguments]    ${locator}
    user waits until parent does not contain element    ${locator}    css:.recharts-bar-rectangles

user waits until element contains map chart
    [Arguments]    ${locator}
    user waits until parent contains element    ${locator}    css:.leaflet-pane

user waits until element does not contain map chart
    [Arguments]    ${locator}
    user waits until parent does not contain element    ${locator}    css:.leaflet-pane

user waits until element contains infographic chart
    [Arguments]    ${locator}    ${wait}=${timeout}
    user waits until parent contains element    ${locator}    css:img

user waits until element does not contain infographic chart
    [Arguments]    ${locator}
    user waits until parent does not contain element    ${locator}    css:img

user waits until element contains chart tooltip
    [Arguments]    ${locator}
    user waits until parent contains element    ${locator}    testid:chartTooltip

user waits until element does not contain chart tooltip
    [Arguments]    ${locator}
    user waits until parent does not contain element    ${locator}    testid:chartTooltip

user checks chart title contains
    [Arguments]    ${locator}    ${text}
    ${element}=    get child element with retry    ${locator}    testid:chart-title
    user waits until element contains    ${element}    ${text}

user checks chart subtitle contains
    [Arguments]    ${locator}    ${text}
    ${element}=    get child element with retry    ${locator}    testid:chart-subtitle
    user waits until element contains    ${element}    ${text}

user checks chart height
    [Arguments]    ${locator}    ${height}
    user waits until parent contains element    ${locator}    css:.recharts-surface[height="${height}"]

user checks chart legend item contains
    [Arguments]    ${locator}    ${item}    ${text}
    user waits until parent contains element    ${locator}    css:.recharts-default-legend li:nth-of-type(${item})
    ${element}=    get child element    ${locator}    css:.recharts-default-legend li:nth-of-type(${item})
    user waits until element is visible    ${element}
    user waits until element contains    ${element}    ${text}

user checks chart inline legend item contains
    [Arguments]    ${locator}    ${item}    ${text}
    user waits until parent contains element    ${locator}
    ...    xpath://*[@class="recharts-layer recharts-line"][${item}]//*[@class="recharts-layer recharts-label-list"]//*[normalize-space() = "${text}"]

user checks chart y axis tick contains
    [Arguments]    ${locator}    ${tick}    ${text}
    user waits until parent contains element    ${locator}
    ...    css:.recharts-yAxis .recharts-cartesian-axis-tick:nth-of-type(${tick})
    ${element}=    get child element    ${locator}
    ...    css:.recharts-yAxis .recharts-cartesian-axis-tick:nth-of-type(${tick})
    user waits until element contains    ${element}    ${text}

user checks chart x axis tick contains
    [Arguments]    ${locator}    ${tick}    ${text}
    user waits until parent contains element    ${locator}
    ...    css:.recharts-xAxis .recharts-cartesian-axis-tick:nth-of-type(${tick})
    ${element}=    get child element    ${locator}
    ...    css:.recharts-xAxis .recharts-cartesian-axis-tick:nth-of-type(${tick})
    user waits until element contains    ${element}    ${text}

user checks chart y axis ticks
    [Arguments]    ${locator}    @{ticks}
    FOR    ${index}    ${tick}    IN ENUMERATE    @{ticks}
        user checks chart y axis tick contains    ${locator}    ${index + 1}    ${tick}
    END

user checks chart x axis ticks
    [Arguments]    ${locator}    @{ticks}
    FOR    ${index}    ${tick}    IN ENUMERATE    @{ticks}
        user checks chart x axis tick contains    ${locator}    ${index + 1}    ${tick}
    END

user mouses over line chart point
    [Arguments]    ${locator}    ${line}    ${number}
    user waits until parent contains element    ${locator}
    ...    xpath://*[@class="recharts-layer recharts-line-dots"]//*[@class="recharts-symbols"][${number}]

    ${element}=    get child element    ${locator}
    ...    css:.recharts-symbols:nth-of-type(${number})
    user waits until element is visible    ${element}
    user mouses over element    ${element}

user mouses over chart bar
    [Arguments]    ${locator}    ${number}
    user waits until parent contains element    ${locator}
    ...    css:.recharts-bar-rectangles .recharts-bar-rectangle:nth-of-type(${number})    %{WAIT_SMALL}
    ${element}=    get child element    ${locator}
    ...    css:.recharts-bar-rectangles .recharts-bar-rectangle:nth-of-type(${number})
    user waits until element is visible    ${element}
    user mouses over element    ${element}

user mouses over selected map feature
    [Arguments]    ${locator}
    # We can identify the selected geojson feature by the stroke (border) width.
    # Unselected features have stroke width 1, when selected it changes to 3.
    user waits until parent contains element    ${locator}    css:path.leaflet-interactive[stroke-width="3"]
    ${element}=    get child element    ${locator}    css:path.leaflet-interactive[stroke-width="3"]
    user waits until element is visible    ${element}
    user mouses over element    ${element}

user checks chart tooltip label contains
    [Arguments]    ${locator}    ${text}
    user waits until parent contains element    ${locator}    testid:chartTooltip-label
    ${element}=    get child element    ${locator}    testid:chartTooltip-label
    user waits until element is visible    ${element}    scroll_to_element=${False}
    user waits until element contains    ${element}    ${text}

user checks chart tooltip item contains
    [Arguments]    ${locator}    ${item}    ${text}
    user waits until parent contains element    ${locator}
    ...    css:[data-testid="chartTooltip-items"] li:nth-of-type(${item})
    ${element}=    get child element    ${locator}    css:[data-testid="chartTooltip-items"] li:nth-of-type(${item})
    user waits until element is visible    ${element}    scroll_to_element=${False}
    user waits until element contains    ${element}    ${text}

user checks map tooltip label contains
    [Arguments]    ${locator}    ${text}
    user waits until parent contains element    ${locator}    testid:chartTooltip-label
    ${element}=    get child element    ${locator}    testid:chartTooltip-label
    user waits until element is visible    ${element}    scroll_to_element=${False}
    user waits until element contains    ${element}    ${text}

user checks map tooltip item contains
    [Arguments]    ${locator}    ${text}
    user waits until parent contains element    ${locator}
    ...    testid:chartTooltip-content
    ${element}=    get child element    ${locator}    testid:chartTooltip-content
    user waits until element is visible    ${element}    scroll_to_element=${False}
    user waits until element contains    ${element}    ${text}

user checks map chart indicator tile contains
    [Arguments]    ${locator}    ${title}    ${statistic}
    user waits until parent contains element    ${locator}
    ...    testid:mapBlock-indicator
    ${indicator}=    get child element    ${locator}    testid:mapBlock-indicator

    ${indicator_title}=    get child element    ${indicator}    testid:mapBlock-indicatorTile-title
    user waits until element is visible    ${indicator_title}    scroll_to_element=${False}
    user waits until element contains    ${indicator_title}    ${title}

    ${indicator_stat}=    get child element    ${indicator}    testid:mapBlock-indicatorTile-statistic
    user waits until element is visible    ${indicator_stat}    scroll_to_element=${False}
    user waits until element contains    ${indicator_stat}    ${statistic}

user gets map boundary polygon dimensions
    [Arguments]    ${index}=1
    ${dimensions}=    Get Element Attribute    xpath://*[@class='leaflet-interactive'][${index}]    d
    [Return]    ${dimensions}

user checks infographic chart contains alt
    [Arguments]    ${locator}    ${text}
    user waits until parent contains element    ${locator}    css:img[alt="${text}"]

user configures basic chart
    [Arguments]
    ...    ${CHART_TYPE}
    ...    ${CHART_HEIGHT}
    ...    ${CHART_ALT_TEXT}=${EMPTY}
    ...    ${CHART_SUBTITLE}=${EMPTY}
    user clicks link    Chart
    user waits until page finishes loading
    ${CHART_TYPE_LIST}=    create list    Line    Horizontal bar    Vertical bar    Geographic
    should contain    ${CHART_TYPE_LIST}    ${CHART_TYPE}

    user waits until h3 is visible    Choose chart type    %{WAIT_SMALL}
    user clicks button    ${CHART_TYPE}

    user waits until page finishes loading
    user enters text into element    id:chartConfigurationForm-height    ${CHART_HEIGHT}

    IF    "${CHART_ALT_TEXT}" != "${EMPTY}"
        user enters text into element    label:Alt text    ${CHART_ALT_TEXT}
    END

    IF    "${CHART_SUBTITLE}" != "${EMPTY}"
        user enters text into element    label:Subtitle    ${CHART_SUBTITLE}
    END

    IF    "${CHART_TYPE}" == "Geographic"
        user clicks link    Boundary levels
        user waits until h3 is visible    Boundary levels    %{WAIT_MEDIUM}
        user chooses select option at index    id:chartBoundaryLevelsConfigurationForm-boundaryLevel    1
        user clicks link    Chart configuration
    END

user selects all data sets for chart
    user clicks link    Data sets
    user waits until h3 is visible    Data sets
    user clicks button    Add data set

user saves infographic configuration
    user saves chart configuration    ${True}

user saves chart configuration
    [Arguments]
    ...    ${infographic}=${False}
    # There are several "Save chart options" buttons to choose from (one per chart configuration tab).
    # We want to ensure that we click one that is visible, so we target the one on the "Chart configuration"
    # tab.
    #
    # This is true of all chart types other than "Infographic", which does not have any additional tabs.
    IF    "${infographic}" == "${False}"
        user clicks link    Chart configuration
        user waits until h3 is visible    Chart configuration
    END

    user clicks button    Save chart options
    user waits until button is enabled    Save chart options
    user waits until page contains    Chart preview
