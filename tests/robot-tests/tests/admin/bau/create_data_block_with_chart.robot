*** Settings ***
Library             Collections
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot
Resource            ../../libs/charts.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser

Force Tags          Admin    Local    Dev    AltersData

*** Variables ***
${TOPIC_NAME}=              %{TEST_TOPIC_NAME}
${PUBLICATION_NAME}=        UI tests - create data block with chart %{RUN_IDENTIFIER}
${DATABLOCK_NAME}=          UI test data block
${CONTENT_SECTION_NAME}=    Test data block section

*** Test Cases ***
Create test publication and release via API
    [Tags]    HappyPath
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user create test release via api    ${PUBLICATION_ID}    AY    2025

Upload subject
    [Tags]    HappyPath
    user navigates to editable release summary from admin dashboard    ${PUBLICATION_NAME}
    ...    Academic Year 2025/26 (not Live)
    user clicks link    Data and files
    user uploads subject    UI test subject    upload-file-test.csv    upload-file-test.meta.csv

Start creating a data block
    [Tags]    HappyPath
    user clicks link    Data blocks
    user waits until h2 is visible    Data blocks

    user waits until page contains    No data blocks have been created.

    user clicks link    Create data block
    user waits until table tool wizard step is available    Choose a subject

Select subject "UI test subject"
    [Tags]    HappyPath
    user waits until page contains    UI test subject
    user clicks radio    UI test subject
    user clicks element    id:publicationSubjectForm-submit
    user waits until table tool wizard step is available    Choose locations    90
    user checks previous table tool step contains    1    Subject    UI test subject

Select locations
    [Tags]    HappyPath
    user opens details dropdown    Opportunity Area
    user clicks checkbox    Bolton 001 (E02000984)
    user clicks checkbox    Bolton 001 (E05000364)
    user clicks checkbox    Bolton 004 (E02000987)
    user clicks checkbox    Bolton 004 (E05010450)
    user opens details dropdown    Ward
    user clicks checkbox    Nailsea Youngwood
    user clicks checkbox    Syon
    user clicks element    id:locationFiltersForm-submit
    user waits until table tool wizard step is available    Choose time period    90

Select time period
    [Tags]    HappyPath
    user waits until page contains element    id:timePeriodForm-start
    ${timePeriodStartList}=    get list items    id:timePeriodForm-start
    ${timePeriodEndList}=    get list items    id:timePeriodForm-end
    ${expectedList}=    create list    Please select    2005    2007    2008    2009    2010    2011    2012    2016
    ...    2017    2018    2019    2020
    lists should be equal    ${timePeriodStartList}    ${expectedList}
    lists should be equal    ${timePeriodEndList}    ${expectedList}

    user selects from list by label    id:timePeriodForm-start    2005
    user selects from list by label    id:timePeriodForm-end    2020
    user clicks element    id:timePeriodForm-submit
    user waits until table tool wizard step is available    Choose your filters
    user checks previous table tool step contains    3    Time period    2005 to 2020

Select indicators
    [Tags]    HappyPath
    user clicks indicator checkbox    Admission Numbers

Create table
    [Documentation]    EES-615
    [Tags]    HappyPath
    user clicks element    id:filtersForm-submit
    user waits until results table appears    %{WAIT_LONG}
    user waits until element contains    css:[data-testid="dataTableCaption"]
    ...    Table showing Admission Numbers for 'UI test subject' in Bolton 001 (E02000984), Bolton 001 (E05000364), Bolton 004 (E02000987), Bolton 004 (E05010450), Nailsea Youngwood and Syon between 2005 and 2020

Validate table rows
    [Tags]    HappyPath
    user checks table column heading contains    1    1    Admission Numbers

    ${row}=    user gets row number with heading    Bolton 001 (E02000984)
    user checks table heading in offset row contains    ${row}    0    2    2019

    user checks table cell in offset row contains    ${row}    0    1    8,533

    ${row}=    user gets row number with heading    Bolton 001 (E05000364)
    user checks table heading in offset row contains    ${row}    0    2    2009
    user checks table heading in offset row contains    ${row}    1    1    2010
    user checks table heading in offset row contains    ${row}    2    1    2017

    user checks table cell in offset row contains    ${row}    0    1    5,815
    user checks table cell in offset row contains    ${row}    1    1    5,595
    user checks table cell in offset row contains    ${row}    2    1    6,373

    ${row}=    user gets row number with heading    Bolton 004 (E02000987)
    user checks table heading in offset row contains    ${row}    0    2    2020

    user checks table cell in offset row contains    ${row}    0    1    6,031

    ${row}=    user gets row number with heading    Bolton 004 (E05010450)
    user checks table heading in offset row contains    ${row}    0    2    2005
    user checks table heading in offset row contains    ${row}    1    1    2017
    user checks table heading in offset row contains    ${row}    2    1    2018

    user checks table cell in offset row contains    ${row}    0    1    8,557
    user checks table cell in offset row contains    ${row}    1    1    3,481
    user checks table cell in offset row contains    ${row}    2    1    8,630

    ${row}=    user gets row number with heading    Nailsea Youngwood
    user checks table heading in offset row contains    ${row}    0    2    2005
    user checks table heading in offset row contains    ${row}    1    1    2010
    user checks table heading in offset row contains    ${row}    2    1    2011
    user checks table heading in offset row contains    ${row}    3    1    2012
    user checks table heading in offset row contains    ${row}    4    1    2016

    user checks table cell in offset row contains    ${row}    0    1    3,612
    user checks table cell in offset row contains    ${row}    1    1    9,304
    user checks table cell in offset row contains    ${row}    2    1    9,603
    user checks table cell in offset row contains    ${row}    3    1    8,150
    user checks table cell in offset row contains    ${row}    4    1    4,198

    ${row}=    user gets row number with heading    Syon
    user checks table heading in offset row contains    ${row}    0    2    2007
    user checks table heading in offset row contains    ${row}    1    1    2008
    user checks table heading in offset row contains    ${row}    2    1    2010
    user checks table heading in offset row contains    ${row}    3    1    2012
    user checks table heading in offset row contains    ${row}    4    1    2017

    user checks table cell in offset row contains    ${row}    0    1    9,914
    user checks table cell in offset row contains    ${row}    1    1    5,505
    user checks table cell in offset row contains    ${row}    2    1    6,060
    user checks table cell in offset row contains    ${row}    3    1    1,109
    user checks table cell in offset row contains    ${row}    4    1    1,959

Save data block
    [Tags]    HappyPath
    user enters text into element    id:dataBlockDetailsForm-name    ${DATABLOCK_NAME}
    user enters text into element    id:dataBlockDetailsForm-heading    UI test table title
    user enters text into element    id:dataBlockDetailsForm-source    UI test source

    user clicks checkbox    Set as a table highlight for this publication
    user waits until page contains element    id:dataBlockDetailsForm-highlightName
    user enters text into element    id:dataBlockDetailsForm-highlightName    UI test highlight name
    user enters text into element    id:dataBlockDetailsForm-highlightDescription    UI test highlight description

    user clicks button    Save data block
    user waits until page contains    Delete this data block

Validate data block is in list
    [Tags]    HappyPath
    user clicks link    Back
    user waits until h2 is visible    Data blocks

    user waits until table is visible
    user checks table column heading contains    1    1    Name
    user checks table column heading contains    1    2    Has chart
    user checks table column heading contains    1    3    In content
    user checks table column heading contains    1    4    Highlight name
    user checks table column heading contains    1    5    Created date
    user checks table column heading contains    1    6    Actions

    user checks table body has x rows    1
    user checks results table cell contains    1    1    ${DATABLOCK_NAME}
    user checks results table cell contains    1    2    No
    user checks results table cell contains    1    3    No
    user checks results table cell contains    1    4    UI test highlight name

Embed data block into release content
    [Tags]    HappyPath
    user clicks link    Content
    user waits until h2 is visible    ${PUBLICATION_NAME}

    user creates new content section    1    ${CONTENT_SECTION_NAME}
    user clicks button    Add data block
    user selects from list by label    css:select[name="selectedDataBlock"]    ${DATABLOCK_NAME}
    user waits until element is visible    css:table
    user clicks button    Embed
    # Wait for table to update
    sleep    0.3s

Validate embedded table rows
    [Tags]    HappyPath
    ${table}=    set variable    css:[data-testid="Data block - ${DATABLOCK_NAME}"] table
    user scrolls to element    xpath://button[text()="${CONTENT_SECTION_NAME}"]
    # The below is to avoid React lazy-loading the table which causes the test to fail here
    user scrolls down    400
    user waits until page contains element    ${table}    30

    user checks table column heading contains    1    1    Admission Numbers    ${table}

    ${row}=    user gets row number with heading    Bolton 001 (E02000984)    ${table}
    user checks table heading in offset row contains    ${row}    0    2    2019    ${table}

    user checks table cell in offset row contains    ${row}    0    1    8,533    ${table}

    ${row}=    user gets row number with heading    Bolton 001 (E05000364)    ${table}
    user checks table heading in offset row contains    ${row}    0    2    2009    ${table}
    user checks table heading in offset row contains    ${row}    1    1    2010    ${table}
    user checks table heading in offset row contains    ${row}    2    1    2017    ${table}

    user checks table cell in offset row contains    ${row}    0    1    5,815    ${table}
    user checks table cell in offset row contains    ${row}    1    1    5,595    ${table}
    user checks table cell in offset row contains    ${row}    2    1    6,373    ${table}

    ${row}=    user gets row number with heading    Bolton 004 (E02000987)    ${table}
    user checks table heading in offset row contains    ${row}    0    2    2020    ${table}

    user checks table cell in offset row contains    ${row}    0    1    6,031    ${table}

    ${row}=    user gets row number with heading    Bolton 004 (E05010450)    ${table}
    user checks table heading in offset row contains    ${row}    0    2    2005    ${table}
    user checks table heading in offset row contains    ${row}    1    1    2017    ${table}
    user checks table heading in offset row contains    ${row}    2    1    2018    ${table}

    user checks table cell in offset row contains    ${row}    0    1    8,557    ${table}
    user checks table cell in offset row contains    ${row}    1    1    3,481    ${table}
    user checks table cell in offset row contains    ${row}    2    1    8,630    ${table}

    ${row}=    user gets row number with heading    Nailsea Youngwood    ${table}
    user checks table heading in offset row contains    ${row}    0    2    2005    ${table}
    user checks table heading in offset row contains    ${row}    1    1    2010    ${table}
    user checks table heading in offset row contains    ${row}    2    1    2011    ${table}
    user checks table heading in offset row contains    ${row}    3    1    2012    ${table}
    user checks table heading in offset row contains    ${row}    4    1    2016    ${table}

    user checks table cell in offset row contains    ${row}    0    1    3,612    ${table}
    user checks table cell in offset row contains    ${row}    1    1    9,304    ${table}
    user checks table cell in offset row contains    ${row}    2    1    9,603    ${table}
    user checks table cell in offset row contains    ${row}    3    1    8,150    ${table}
    user checks table cell in offset row contains    ${row}    4    1    4,198    ${table}

    ${row}=    user gets row number with heading    Syon    ${table}
    user checks table heading in offset row contains    ${row}    0    2    2007    ${table}
    user checks table heading in offset row contains    ${row}    1    1    2008    ${table}
    user checks table heading in offset row contains    ${row}    2    1    2010    ${table}
    user checks table heading in offset row contains    ${row}    3    1    2012    ${table}
    user checks table heading in offset row contains    ${row}    4    1    2017    ${table}

    user checks table cell in offset row contains    ${row}    0    1    9,914    ${table}
    user checks table cell in offset row contains    ${row}    1    1    5,505    ${table}
    user checks table cell in offset row contains    ${row}    2    1    6,060    ${table}
    user checks table cell in offset row contains    ${row}    3    1    1,109    ${table}
    user checks table cell in offset row contains    ${row}    4    1    1,959    ${table}

Validate marked as 'In content' on data block list
    [Tags]    HappyPath
    user clicks link    Data blocks
    user waits until h2 is visible    Data blocks

    user waits until table is visible
    user checks table column heading contains    1    1    Name
    user checks table column heading contains    1    3    In content

    user checks table body has x rows    1
    user checks results table cell contains    1    1    ${DATABLOCK_NAME}
    user checks results table cell contains    1    3    Yes

Navigate to Chart tab
    [Tags]    HappyPath
    user clicks link    Edit block

    user waits until h2 is visible    Edit data block
    user waits until h2 is visible    ${DATABLOCK_NAME}

    user waits until page does not contain loading spinner

    # Set url in suite variable so that we
    # can get back to this page quickly
    ${url}=    user gets url
    set suite variable    ${DATABLOCK_URL}    ${url}

    user clicks link    Chart
    user waits until table tool wizard step is available    Choose chart type

Configure basic line chart
    [Tags]    HappyPath
    user clicks button    Line
    user enters text into element    id:chartConfigurationForm-title    Test chart title
    user enters text into element    id:chartConfigurationForm-alt    Test chart alt
    user enters text into element    id:chartConfigurationForm-height    400
    user enters text into element    id:chartConfigurationForm-width    900

    user clicks link    Data sets
    user waits until h3 is visible    Data sets
    user selects from list by label    id:chartDataSetsConfigurationForm-location    Nailsea Youngwood
    user clicks button    Add data set

Validate basic line chart preview
    [Tags]    HappyPath
    ${preview}=    set variable    id:chartBuilderPreview
    user waits until element contains line chart    ${preview}

    user checks chart title contains    ${preview}    Test chart title
    user checks chart legend item contains    ${preview}    1    Admission Numbers (Nailsea Youngwood)

    user checks chart height    ${preview}    400
    user checks chart width    ${preview}    900

    user checks chart y axis ticks    ${preview}    0    2,500    5,000    7,500    10,000
    user checks chart x axis ticks    ${preview}    2005    2010    2011    2012    2016

    user mouses over line chart point    ${preview}    1    1
    user checks chart tooltip label contains    ${preview}    2005
    user checks chart tooltip item contains    ${preview}    1    Admission Numbers (Nailsea Youngwood): 3,612

    user mouses over line chart point    ${preview}    1    2
    user checks chart tooltip label contains    ${preview}    2010
    user checks chart tooltip item contains    ${preview}    1    Admission Numbers (Nailsea Youngwood): 9,304

    user mouses over line chart point    ${preview}    1    3
    user checks chart tooltip label contains    ${preview}    2011
    user checks chart tooltip item contains    ${preview}    1    Admission Numbers (Nailsea Youngwood): 9,603

    user mouses over line chart point    ${preview}    1    4
    user checks chart tooltip label contains    ${preview}    2012
    user checks chart tooltip item contains    ${preview}    1    Admission Numbers (Nailsea Youngwood): 8,150

    user mouses over line chart point    ${preview}    1    5
    user checks chart tooltip label contains    ${preview}    2016
    user checks chart tooltip item contains    ${preview}    1    Admission Numbers (Nailsea Youngwood): 4,198

Save chart and validate marked as 'Has chart' in data blocks list
    [Tags]    HappyPath
    user clicks link    Chart configuration
    user clicks button    Save chart options
    user waits until button is enabled    Save chart options

    user clicks link    Back
    user waits until h2 is visible    Data blocks

    user waits until table is visible
    user checks table column heading contains    1    1    Name
    user checks table column heading contains    1    2    Has chart

    user checks table body has x rows    1
    user checks results table cell contains    1    1    ${DATABLOCK_NAME}
    user checks results table cell contains    1    2    Yes

Validate line chart embeds correctly
    [Tags]    HappyPath
    user clicks link    Content
    user waits until h2 is visible    ${PUBLICATION_NAME}
    user opens accordion section    ${CONTENT_SECTION_NAME}    css:#releaseMainContent
    ${datablock}=    set variable    css:[data-testid="Data block - ${DATABLOCK_NAME}"]
    # The below is to avoid React lazy-loading the chart which causes the test to fail here
    user scrolls down    400
    user waits until element contains line chart    ${datablock}

    user checks chart title contains    ${datablock}    Test chart title
    user checks chart legend item contains    ${datablock}    1    Admission Numbers (Nailsea Youngwood)

    user checks chart height    ${datablock}    400
    user checks chart width    ${datablock}    900

    user checks chart y axis ticks    ${datablock}    0    2,500    5,000    7,500    10,000
    user checks chart x axis ticks    ${datablock}    2005    2010    2011    2012    2016

    user mouses over line chart point    ${datablock}    1    1
    user checks chart tooltip label contains    ${datablock}    2005
    user checks chart tooltip item contains    ${datablock}    1    Admission Numbers (Nailsea Youngwood): 3,612

    user mouses over line chart point    ${datablock}    1    2
    user checks chart tooltip label contains    ${datablock}    2010
    user checks chart tooltip item contains    ${datablock}    1    Admission Numbers (Nailsea Youngwood): 9,304

    user mouses over line chart point    ${datablock}    1    3
    user checks chart tooltip label contains    ${datablock}    2011
    user checks chart tooltip item contains    ${datablock}    1    Admission Numbers (Nailsea Youngwood): 9,603

    user mouses over line chart point    ${datablock}    1    4
    user checks chart tooltip label contains    ${datablock}    2012
    user checks chart tooltip item contains    ${datablock}    1    Admission Numbers (Nailsea Youngwood): 8,150

    user mouses over line chart point    ${datablock}    1    5
    user checks chart tooltip label contains    ${datablock}    2016
    user checks chart tooltip item contains    ${datablock}    1    Admission Numbers (Nailsea Youngwood): 4,198

Configure basic vertical bar chart
    [Tags]    HappyPath    Failing
    user goes to url    ${DATABLOCK_URL}

    user waits until h2 is visible    ${DATABLOCK_NAME}    %{WAIT_MEDIUM}
    user waits until page does not contain loading spinner

    user clicks link    Chart
    user configures basic chart    Vertical bar    500    800

Change vertical bar chart legend
    [Tags]    HappyPath    Failing
    user clicks link    Legend
    user waits until h3 is visible    Legend    60

    user counts legend form item rows    1
    user checks element value should be    id:chartLegendConfigurationForm-items-0-label
    ...    Admission Numbers (Nailsea Youngwood)    60

    user enters text into element    id:chartLegendConfigurationForm-items-0-label    Admissions

    user waits for chart preview to update

Validate basic vertical bar chart preview
    [Tags]    HappyPath    Failing
    ${preview}=    set variable    id:chartBuilderPreview
    user waits until element does not contain line chart    ${preview}
    user waits until element contains bar chart    ${preview}

    user checks chart title contains    ${preview}    Test chart title
    user checks chart legend item contains    ${preview}    1    Admissions

    user checks chart height    ${preview}    500
    user checks chart width    ${preview}    800

    user checks chart y axis ticks    ${preview}    0    2,500    5,000    7,500    10,000
    user checks chart x axis ticks    ${preview}    2005    2010    2011    2012    2016

    user mouses over chart bar    ${preview}    1
    user checks chart tooltip label contains    ${preview}    2005
    user checks chart tooltip item contains    ${preview}    1    Admissions: 3,612

    user mouses over chart bar    ${preview}    2
    user checks chart tooltip label contains    ${preview}    2010
    user checks chart tooltip item contains    ${preview}    1    Admissions: 9,304

    user mouses over chart bar    ${preview}    3
    user checks chart tooltip label contains    ${preview}    2011
    user checks chart tooltip item contains    ${preview}    1    Admissions: 9,603

    user mouses over chart bar    ${preview}    4
    user checks chart tooltip label contains    ${preview}    2012
    user checks chart tooltip item contains    ${preview}    1    Admissions: 8,150

    user mouses over chart bar    ${preview}    5
    user checks chart tooltip label contains    ${preview}    2016
    user checks chart tooltip item contains    ${preview}    1    Admissions: 4,198

Save and validate vertical bar chart embeds correctly
    [Tags]    HappyPath    Failing
    # Transient React error that happens locally & on dev sometimes: TypeError: Cannot read property '_leaflet_pos' of undefined
    user clicks link    Chart configuration
    user clicks button    Save chart options
    user waits until button is enabled    Save chart options

    user clicks link    Content
    user waits until h2 is visible    ${PUBLICATION_NAME}    60
    user opens accordion section    ${CONTENT_SECTION_NAME}    css:#releaseMainContent

    ${datablock}=    set variable    css:[data-testid="Data block - ${DATABLOCK_NAME}"]
    user waits until element does not contain line chart    ${datablock}
    # below is to prevent React lazy loading the chart
    user scrolls down    400
    user waits until element contains bar chart    ${datablock}

    user checks chart title contains    ${datablock}    Test chart title
    user checks chart legend item contains    ${datablock}    1    Admissions

    user checks chart height    ${datablock}    500
    user checks chart width    ${datablock}    800

    user checks chart y axis ticks    ${datablock}    0    2,500    5,000    7,500    10,000
    user checks chart x axis ticks    ${datablock}    2005    2010    2011    2012    2016

    user mouses over chart bar    ${datablock}    1
    user checks chart tooltip label contains    ${datablock}    2005
    user checks chart tooltip item contains    ${datablock}    1    Admissions: 3,612

    user mouses over chart bar    ${datablock}    2
    user checks chart tooltip label contains    ${datablock}    2010
    user checks chart tooltip item contains    ${datablock}    1    Admissions: 9,304

    user mouses over chart bar    ${datablock}    3
    user checks chart tooltip label contains    ${datablock}    2011
    user checks chart tooltip item contains    ${datablock}    1    Admissions: 9,603

    user mouses over chart bar    ${datablock}    4
    user checks chart tooltip label contains    ${datablock}    2012
    user checks chart tooltip item contains    ${datablock}    1    Admissions: 8,150

    user mouses over chart bar    ${datablock}    5
    user checks chart tooltip label contains    ${datablock}    2016
    user checks chart tooltip item contains    ${datablock}    1    Admissions: 4,198

Configure basic horizontal bar chart
    [Tags]    HappyPath    Failing
    user goes to url    ${DATABLOCK_URL}
    user waits until h2 is visible    ${DATABLOCK_NAME}    60
    user waits until page does not contain loading spinner

    user clicks link    Chart
    user configures basic chart    Horizontal bar    600    700

Validate basic horizontal bar chart preview
    [Tags]    HappyPath    Failing
    ${preview}=    set variable    id:chartBuilderPreview
    user waits until element contains bar chart    ${preview}

    user checks chart title contains    ${preview}    Test chart title
    user checks chart legend item contains    ${preview}    1    Admissions:

    user checks chart title contains    ${preview}    Test chart title
    user checks chart legend item contains    ${preview}    1    Admissions:

    user checks chart x axis ticks    ${preview}    0    2,500    5,000    7,500    10,000
    user checks chart y axis ticks    ${preview}    2005    2010    2011    2012    2016

    user checks chart height    ${preview}    600
    user checks chart width    ${preview}    700

    user mouses over chart bar    ${preview}    1
    user checks chart tooltip label contains    ${preview}    2005
    user checks chart tooltip item contains    ${preview}    1    Admissions: 3,612

    user mouses over chart bar    ${preview}    2
    user checks chart tooltip label contains    ${preview}    2010
    user checks chart tooltip item contains    ${preview}    1    Admissions: 9,304

    user mouses over chart bar    ${preview}    3
    user checks chart tooltip label contains    ${preview}    2011
    user checks chart tooltip item contains    ${preview}    1    Admissions: 9,603

    user mouses over chart bar    ${preview}    4
    user checks chart tooltip label contains    ${preview}    2012
    user checks chart tooltip item contains    ${preview}    1    Admissions: 8,150

    user mouses over chart bar    ${preview}    5
    user checks chart tooltip label contains    ${preview}    2016
    user checks chart tooltip item contains    ${preview}    1    Admissions: 4,198

Save and validate horizontal bar chart embeds correctly
    [Tags]    HappyPath    Failing
    user clicks link    Chart configuration
    user clicks button    Save chart options
    user waits until button is enabled    Save chart options

    user clicks link    Content
    user waits until h2 is visible    ${PUBLICATION_NAME}
    user opens accordion section    ${CONTENT_SECTION_NAME}    css:#releaseMainContent

    ${datablock}=    set variable    css:[data-testid="Data block - ${DATABLOCK_NAME}"]
    user waits until element contains bar chart    ${datablock}

    user checks chart title contains    ${datablock}    Test chart title
    user checks chart legend item contains    ${datablock}    1    Admission Numbers (Nailsea Youngwood):

    user checks chart x axis ticks    ${datablock}    0    2,500    5,000    7,500    10,000
    user checks chart y axis ticks    ${datablock}    2005    2010    2011    2012    2016

    user checks chart height    ${datablock}    600
    user checks chart width    ${datablock}    700

    user mouses over chart bar    ${datablock}    1
    user checks chart tooltip label contains    ${datablock}    2005
    user checks chart tooltip item contains    ${datablock}    1    Admissions: 3,612

    user mouses over chart bar    ${datablock}    2
    user checks chart tooltip label contains    ${datablock}    2010
    user checks chart tooltip item contains    ${datablock}    1    Admissions: 9,304

    user mouses over chart bar    ${datablock}    3
    user checks chart tooltip label contains    ${datablock}    2011
    user checks chart tooltip item contains    ${datablock}    1    Admissions: 9,603

    user mouses over chart bar    ${datablock}    4
    user checks chart tooltip label contains    ${datablock}    2012
    user checks chart tooltip item contains    ${datablock}    1    Admissions: 8,150

    user mouses over chart bar    ${datablock}    5
    user checks chart tooltip label contains    ${datablock}    2016
    user checks chart tooltip item contains    ${datablock}    1    Admissions: 4,198

Configure basic geographic chart
    [Tags]    HappyPath    Failing
    user goes to url    ${DATABLOCK_URL}
    user waits until h2 is visible    ${DATABLOCK_NAME}    60
    user waits until page does not contain loading spinner

    user clicks link    Chart
    user configures basic chart    Geographic    700    600

Change geographic chart legend
    [Tags]    HappyPath    Failing
    user clicks link    Legend
    user waits until h3 is visible    Legend    90

    user counts legend form item rows    5
    user checks element value should be    id:chartLegendConfigurationForm-items-0-label    Admission Numbers (2005)
    user checks element value should be    id:chartLegendConfigurationForm-items-1-label    Admission Numbers (2010)
    user checks element value should be    id:chartLegendConfigurationForm-items-2-label    Admission Numbers (2011)
    user checks element value should be    id:chartLegendConfigurationForm-items-3-label    Admission Numbers (2012)
    user checks element value should be    id:chartLegendConfigurationForm-items-4-label    Admission Numbers (2016)

    user enters text into element    id:chartLegendConfigurationForm-items-0-label    Admissions in 2005
    user enters text into element    id:chartLegendConfigurationForm-items-1-label    Admissions in 2010
    user enters text into element    id:chartLegendConfigurationForm-items-2-label    Admissions in 2011
    user enters text into element    id:chartLegendConfigurationForm-items-3-label    Admissions in 2012
    user enters text into element    id:chartLegendConfigurationForm-items-4-label    Admissions in 2016

    user waits for chart preview to update

Validate basic geographic chart preview
    [Tags]    HappyPath    Failing
    ${preview}=    set variable    id:chartBuilderPreview
    user waits until element does not contain bar chart    ${preview}
    user waits until element contains map chart    ${preview}

    user checks map chart height    ${preview}    700
    user checks map chart width    ${preview}    600

    user selects from list by label    ${preview}-map-selectedLocation    Nailsea Youngwood

    user mouses over selected map feature    ${preview}
    user checks chart tooltip label contains    ${preview}    Nailsea Youngwood
    user checks chart tooltip item contains    ${preview}    1    Admissions in 2005: 3,612
    user checks chart tooltip item contains    ${preview}    2    Admissions in 2010: 9,304
    user checks chart tooltip item contains    ${preview}    3    Admissions in 2011: 9,603
    user checks chart tooltip item contains    ${preview}    4    Admissions in 2012: 8,150
    user checks chart tooltip item contains    ${preview}    5    Admissions in 2016: 4,198

    user checks map chart indicator tile contains    ${preview}    1    Admissions in 2005    3,612
    user checks map chart indicator tile contains    ${preview}    2    Admissions in 2010    9,304
    user checks map chart indicator tile contains    ${preview}    3    Admissions in 2011    9,603
    user checks map chart indicator tile contains    ${preview}    4    Admissions in 2012    8,150
    user checks map chart indicator tile contains    ${preview}    5    Admissions in 2016    4,198

Save and validate geographic chart embeds correctly
    [Tags]    HappyPath    Failing    NotAgainstDev    NotAgainstLocal
    # transient React error    TypeError: Cannot read property '_leaflet_pos' of undefined

    user clicks button    Save chart options
    user waits until button is enabled    Save chart options

    user clicks link    Content
    user waits until h2 is visible    ${PUBLICATION_NAME}
    user opens accordion section    ${CONTENT_SECTION_NAME}    css:#releaseMainContent

    ${datablock}=    set variable    css:[data-testid="Data block - ${DATABLOCK_NAME}"]
    user waits until element does not contain bar chart    ${datablock}
    user waits until element contains map chart    ${datablock}

    user checks map chart height    ${datablock}    700
    user checks map chart width    ${datablock}    600

    user selects from list by label    ${datablock} select[name="selectedLocation"]    Nailsea Youngwood

    user mouses over selected map feature    ${datablock}
    user checks chart tooltip label contains    ${datablock}    Nailsea Youngwood
    user checks chart tooltip item contains    ${datablock}    1    Admissions in 2005: 3,612
    user checks chart tooltip item contains    ${datablock}    2    Admissions in 2010: 9,304
    user checks chart tooltip item contains    ${datablock}    3    Admissions in 2011: 9,603
    user checks chart tooltip item contains    ${datablock}    4    Admissions in 2012: 8,150
    user checks chart tooltip item contains    ${datablock}    5    Admissions in 2016: 4,198

    user checks map chart indicator tile contains    ${datablock}    1    Admissions in 2005    3,612
    user checks map chart indicator tile contains    ${datablock}    2    Admissions in 2010    9,304
    user checks map chart indicator tile contains    ${datablock}    3    Admissions in 2011    9,603
    user checks map chart indicator tile contains    ${datablock}    4    Admissions in 2012    8,150
    user checks map chart indicator tile contains    ${datablock}    5    Admissions in 2016    4,198

Configure basic infographic chart
    [Tags]    HappyPath
    user goes to url    ${DATABLOCK_URL}

    user waits until h2 is visible    ${DATABLOCK_NAME}
    user waits until page does not contain loading spinner

    user clicks link    Chart
    user waits until table tool wizard step is available    Choose chart type
    user clicks button    Choose an infographic as alternative
    user chooses file    id:chartConfigurationForm-file    ${FILES_DIR}test-infographic.png

Validate basic infographic chart preview
    [Tags]    HappyPath
    user checks chart title contains    id:chartBuilderPreview    Test chart title
    user waits until element contains infographic chart    id:chartBuilderPreview
    user checks infographic chart contains alt    id:chartBuilderPreview    Test chart alt

Save and validate infographic chart embeds correctly
    [Tags]    HappyPath
    user clicks button    Save chart options
    user waits until button is enabled    Save chart options

    user clicks link    Content
    user waits until h2 is visible    ${PUBLICATION_NAME}
    user opens accordion section    ${CONTENT_SECTION_NAME}    css:#releaseMainContent

    ${datablock}=    set variable    css:[data-testid="Data block - ${DATABLOCK_NAME}"]
    user checks chart title contains    ${datablock}    Test chart title
    user checks infographic chart contains alt    ${datablock}    Test chart alt

Delete embedded data block
    [Tags]    HappyPath
    user clicks button    Remove block
    user clicks button    Confirm

Delete chart from data block
    [Tags]    HappyPath
    user goes to url    ${DATABLOCK_URL}

    user waits until h2 is visible    ${DATABLOCK_NAME}
    user waits until page does not contain loading spinner

    user clicks link    Chart
    user clicks button    Delete chart
    user clicks button    Confirm
    user waits until element does not contain infographic chart    id:chartBuilderPreview

Delete data block
    [Tags]    HappyPath
    user clicks button    Delete this data block
    user waits until page does not contain loading spinner
    user clicks button    Confirm

    user waits until h2 is visible    Data blocks
    user waits until page contains    No data blocks have been created.

*** Keywords ***
user counts legend form item rows
    [Arguments]    ${number}
    user checks element count is x    css:fieldset[id^="chartLegendConfigurationForm-items"]    ${number}
