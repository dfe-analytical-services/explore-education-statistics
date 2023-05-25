*** Settings ***
Library             Collections
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot
Resource            ../../libs/admin/manage-content-common.robot
Resource            ../../libs/charts.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    AltersData


*** Variables ***
${TOPIC_NAME}=              %{TEST_TOPIC_NAME}
${PUBLICATION_NAME}=        UI tests - create data block with chart %{RUN_IDENTIFIER}
${DATABLOCK_NAME}=          UI test data block
${CONTENT_SECTION_NAME}=    Test data block section
${FOOTNOTE_1}=              Test footnote from bau
${FOOTNOTE_UPDATED}=        Updated test footnote from bau


*** Test Cases ***
Create test publication and release via API
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    AY    2025

Upload subject
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    Academic year 2025/26
    user uploads subject    UI test subject    upload-file-test.csv    upload-file-test.meta.csv

Navigate to 'Footnotes' page
    user waits for page to finish loading
    user clicks link    Footnotes
    user waits until h2 is visible    Footnotes

Create footnote
    user waits until page contains link    Create footnote
    user clicks link    Create footnote
    user waits until page does not contain loading spinner
    user clicks footnote subject radio    UI test subject    Applies to all data
    user clicks element    id:footnoteForm-content
    user enters text into element    id:footnoteForm-content    ${FOOTNOTE_1}
    user clicks radio    Applies to all data
    user clicks button    Save footnote
    user waits until h2 is visible    Footnotes    %{WAIT_SMALL}

Start creating a data block
    user clicks link    Data blocks
    user waits until h2 is visible    Data blocks
    user waits until page contains    No data blocks have been created.

    user clicks link    Create data block
    user waits until table tool wizard step is available    1    Choose a subject

Select subject "UI test subject"
    user waits until page contains    UI test subject    %{WAIT_SMALL}
    user clicks radio    UI test subject
    user clicks element    id:publicationSubjectForm-submit
    user waits until table tool wizard step is available    2    Choose locations    %{WAIT_MEDIUM}
    user checks previous table tool step contains    1    Subject    UI test subject

Select locations
    user opens details dropdown    Opportunity area
    user clicks checkbox    Bolton 001
    user clicks checkbox    Bolton 002
    user clicks checkbox    Bolton 003
    user clicks checkbox    Bolton 004

    user opens details dropdown    Ward
    user clicks checkbox    Nailsea Youngwood
    user clicks checkbox    Syon
    user clicks element    id:locationFiltersForm-submit
    user waits until table tool wizard step is available    3    Choose time period    90

Select time period
    user waits until page contains element    id:timePeriodForm-start
    ${timePeriodStartList}=    get list items    id:timePeriodForm-start
    ${timePeriodEndList}=    get list items    id:timePeriodForm-end

    ${expectedList}=    create list    Please select    2005    2006    2007    2008    2009    2010    2011    2012
    ...    2014    2016    2017    2018    2019    2020
    lists should be equal    ${timePeriodStartList}    ${expectedList}
    lists should be equal    ${timePeriodEndList}    ${expectedList}

    user chooses select option    id:timePeriodForm-start    2005
    user chooses select option    id:timePeriodForm-end    2020
    user clicks element    id:timePeriodForm-submit
    user waits until table tool wizard step is available    4    Choose your filters
    user checks previous table tool step contains    3    Time period    2005 to 2020    %{WAIT_MEDIUM}

Select indicators
    user checks indicator checkbox is checked    Admission Numbers

Create table
    [Documentation]    EES-615
    user clicks element    id:filtersForm-submit
    user waits until results table appears    %{WAIT_LONG}
    user waits until element contains    css:[data-testid="dataTableCaption"]
    ...    Admission Numbers for 'UI test subject' in Bolton 001, Bolton 002, Bolton 003, Bolton 004, Nailsea Youngwood and 1 other location between 2005 and 2020

Validate table rows
    user checks table column heading contains    1    1    Admission Numbers

    ${row}=    user gets row number with heading    Bolton 001
    user checks table heading in offset row contains    ${row}    0    2    2009

    user checks table cell in offset row contains    ${row}    0    1    5,815

    ${row}=    user gets row number with heading    Bolton 001
    user checks table heading in offset row contains    ${row}    0    2    2009
    user checks table heading in offset row contains    ${row}    1    1    2010
    user checks table heading in offset row contains    ${row}    2    1    2017

    user checks table cell in offset row contains    ${row}    1    1    5,595
    user checks table cell in offset row contains    ${row}    2    1    6,373

    ${row}=    user gets row number with heading    Bolton 004
    user checks table heading in offset row contains    ${row}    0    2    2005

    user checks table cell in offset row contains    ${row}    0    1    8,557

    ${row}=    user gets row number with heading    Bolton 004
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
    user enters text into element    id:dataBlockDetailsForm-name    ${DATABLOCK_NAME}
    user enters text into element    id:dataBlockDetailsForm-heading    UI test table title
    user enters text into element    id:dataBlockDetailsForm-source    UI test source

    user clicks checkbox    Set as a featured table for this publication
    user waits until page contains element    id:dataBlockDetailsForm-highlightName
    user enters text into element    id:dataBlockDetailsForm-highlightName    UI test highlight name
    user enters text into element    id:dataBlockDetailsForm-highlightDescription    UI test highlight description

    user clicks button    Save data block
    user waits until page contains    Delete this data block

Validate data block is in list
    user clicks link    Back
    user waits until h2 is visible    Data blocks

    user waits until table is visible
    user checks table column heading contains    1    1    Name
    user checks table column heading contains    1    2    Has chart
    user checks table column heading contains    1    3    In content
    user checks table column heading contains    1    4    Featured table name
    user checks table column heading contains    1    5    Created date
    user checks table column heading contains    1    6    Actions

    user checks table body has x rows    1
    user checks table cell contains    1    1    ${DATABLOCK_NAME}
    user checks table cell contains    1    2    No
    user checks table cell contains    1    3    No
    user checks table cell contains    1    4    UI test highlight name

Embed data block into release content
    user clicks link    Content
    user waits until h2 is visible    ${PUBLICATION_NAME}
    user closes Set Page View box
    user creates new content section    1    ${CONTENT_SECTION_NAME}
    user clicks button    Add data block
    user chooses and embeds data block    ${DATABLOCK_NAME}

Check footnote is displayed in content Tab
    user checks accordion section contains x blocks    ${CONTENT_SECTION_NAME}    1    id:releaseMainContent
    user scrolls to accordion section content    ${CONTENT_SECTION_NAME}    id:releaseMainContent

    user waits until page contains element    testid:Data block - ${DATABLOCK_NAME}
    user scrolls to element    testid:Data block - ${DATABLOCK_NAME}
    user waits until table is visible    testid:Data block - ${DATABLOCK_NAME}    10

    user scrolls to element    testid:footnotes
    user checks list has x items    testid:footnotes    1
    user checks list item contains    testid:footnotes    1    ${FOOTNOTE_1}

Update footnote
    [Documentation]    EES-3136
    user clicks link    Footnotes
    user waits until h2 is visible    Footnotes
    user clicks link    Edit footnote    testid:Footnote - ${FOOTNOTE_1}
    user waits until h2 is visible    Edit footnote
    user enters text into element    label:Footnote    ${FOOTNOTE_UPDATED}
    user clicks button    Save footnote
    user waits until page contains    ${FOOTNOTE_UPDATED}
    user checks page does not contain    ${FOOTNOTE_1}

Navigate to content tab
    user clicks link    Content
    user waits until element is visible    //*[@class="dfe-page-editing"]//h2[text()="${PUBLICATION_NAME}"]

Check updated footnote is displayed in content Tab
    [Documentation]    EES-3136
    user clicks button    Test data block section
    ${section}=    user gets accordion section content element    Test data block section
    ...    //*[@data-testid="editableAccordionSection"]

    user scrolls to element    ${section}

    user checks list has x items    testid:footnotes    1
    user checks list item contains    testid:footnotes    1    ${FOOTNOTE_UPDATED}

Validate embedded table rows
    ${datablock}=    set variable    testid:Data block - ${DATABLOCK_NAME}
    user scrolls to element    id:releaseMainContent

    user opens accordion section    ${CONTENT_SECTION_NAME}    id:releaseMainContent

    ${table}=    set variable    ${datablock} >> css:table
    user waits until page contains element    ${table}    30
    user checks table column heading contains    1    1    Admission Numbers    ${table}

    ${row}=    user gets row number with heading    Bolton 001    ${table}
    user checks table heading in offset row contains    ${row}    0    2    2009    ${table}

    user checks table cell in offset row contains    ${row}    0    1    5,815    ${table}

    ${row}=    user gets row number with heading    Bolton 001    ${table}
    user checks table heading in offset row contains    ${row}    0    2    2009    ${table}
    user checks table heading in offset row contains    ${row}    1    1    2010    ${table}
    user checks table heading in offset row contains    ${row}    2    1    2017    ${table}

    user checks table cell in offset row contains    ${row}    0    1    5,815    ${table}
    user checks table cell in offset row contains    ${row}    1    1    5,595    ${table}
    user checks table cell in offset row contains    ${row}    2    1    6,373    ${table}
    user checks table cell in offset row contains    ${row}    3    1    8,533    ${table}

    ${row}=    user gets row number with heading    Bolton 004    ${table}
    user checks table heading in offset row contains    ${row}    0    2    2005    ${table}

    user checks table cell in offset row contains    ${row}    0    1    8,557    ${table}

    ${row}=    user gets row number with heading    Bolton 004    ${table}
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
    user clicks link    Data blocks
    user waits until h2 is visible    Data blocks

    user waits until table is visible
    user checks table column heading contains    1    1    Name
    user checks table column heading contains    1    3    In content

    user checks table body has x rows    1
    user checks table cell contains    1    1    ${DATABLOCK_NAME}
    user checks table cell contains    1    3    Yes

Navigate to Chart tab
    user clicks link    Edit block

    user waits until h2 is visible    Edit data block
    user waits until h2 is visible    ${DATABLOCK_NAME}

    user waits until page does not contain loading spinner

    # Set url in suite variable so that we
    # can get back to this page quickly
    ${url}=    user gets url
    set suite variable    ${DATABLOCK_URL}    ${url}

    user clicks link    Chart
    user waits until h3 is visible    Choose chart type

Configure basic line chart
    user clicks button    Line
    user clicks radio    Set an alternative title
    user enters text into element    id:chartConfigurationForm-title    Test chart title
    user enters text into element    id:chartConfigurationForm-alt    Test chart alt
    user enters text into element    id:chartConfigurationForm-height    400
    user enters text into element    id:chartConfigurationForm-width    900

Validate changing data sets
    user clicks link    Data sets
    user waits until h3 is visible    Data sets

    user chooses select option    id:chartDataSetsConfigurationForm-location    Nailsea Youngwood
    user clicks button    Add data set

    user chooses select option    id:chartDataSetsConfigurationForm-location    Syon
    user clicks button    Add data set

    user chooses select option    id:chartDataSetsConfigurationForm-location    Bolton 001
    user clicks button    Add data set

    user checks chart legend item contains    id:chartBuilderPreview    1    Admission Numbers (Nailsea Youngwood)
    user checks chart legend item contains    id:chartBuilderPreview    2    Admission Numbers (Syon)
    user checks chart legend item contains    id:chartBuilderPreview    3    Admission Numbers (Bolton 001)

    user checks table body has x rows    3    testid:chart-data-sets

    user clicks button    Remove all
    user clicks button    Confirm

    user checks page contains    Configure the chart and add data to view a preview

    user checks table body has x rows    0    testid:chart-data-sets

Configure line chart data sets
    user chooses select option    id:chartDataSetsConfigurationForm-location    Nailsea Youngwood
    user clicks button    Add data set

    user checks chart legend item contains    id:chartBuilderPreview    1    Admission Numbers (Nailsea Youngwood)

    user clicks link    Legend
    user chooses select option    id:chartLegendConfigurationForm-items-0-symbol    Circle

Validate basic line chart preview
    user waits until element contains line chart    id:chartBuilderPreview

    user checks chart title contains    id:chartBuilderPreview    Test chart title
    user checks chart legend item contains    id:chartBuilderPreview    1    Admission Numbers (Nailsea Youngwood)

    user checks chart height    id:chartBuilderPreview    400
    user checks chart width    id:chartBuilderPreview    900

    user checks chart y axis ticks    id:chartBuilderPreview    0    2,500    5,000    7,500    10,000
    user checks chart x axis ticks    id:chartBuilderPreview    2005    2010    2011    2012    2016

    user mouses over line chart point    id:chartBuilderPreview    1    1
    user checks chart tooltip label contains    id:chartBuilderPreview    2005
    user checks chart tooltip item contains    id:chartBuilderPreview    1
    ...    Admission Numbers (Nailsea Youngwood): 3,612

    user mouses over line chart point    id:chartBuilderPreview    1    2
    user checks chart tooltip label contains    id:chartBuilderPreview    2010
    user checks chart tooltip item contains    id:chartBuilderPreview    1
    ...    Admission Numbers (Nailsea Youngwood): 9,304

    user mouses over line chart point    id:chartBuilderPreview    1    3
    user checks chart tooltip label contains    id:chartBuilderPreview    2011
    user checks chart tooltip item contains    id:chartBuilderPreview    1
    ...    Admission Numbers (Nailsea Youngwood): 9,603

    user mouses over line chart point    id:chartBuilderPreview    1    4
    user checks chart tooltip label contains    id:chartBuilderPreview    2012
    user checks chart tooltip item contains    id:chartBuilderPreview    1
    ...    Admission Numbers (Nailsea Youngwood): 8,150

    user mouses over line chart point    id:chartBuilderPreview    1    5
    user checks chart tooltip label contains    id:chartBuilderPreview    2016
    user checks chart tooltip item contains    id:chartBuilderPreview    1
    ...    Admission Numbers (Nailsea Youngwood): 4,198

Save chart and validate marked as 'Has chart' in data blocks list
    user clicks link    Chart configuration
    user clicks button    Save chart options
    user waits until button is enabled    Save chart options

    user clicks link    Back
    user waits until h2 is visible    Data blocks

    user waits until table is visible
    user checks table column heading contains    1    1    Name
    user checks table column heading contains    1    2    Has chart

    user checks table body has x rows    1
    user checks table cell contains    1    1    ${DATABLOCK_NAME}
    user checks table cell contains    1    2    Yes

Validate line chart embeds correctly
    user clicks link    Content
    user waits until h2 is visible    ${PUBLICATION_NAME}
    user opens accordion section    ${CONTENT_SECTION_NAME}    id:releaseMainContent

    ${datablock}=    set variable    testid:Data block - ${DATABLOCK_NAME}
    user waits until page contains element    ${datablock}

    # Need to scroll to block to load it
    user scrolls to element    ${datablock}
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
    user navigates to admin frontend    ${DATABLOCK_URL}

    user waits until h2 is visible    ${DATABLOCK_NAME}    %{WAIT_MEDIUM}
    user waits until page does not contain loading spinner

    user clicks link    Chart
    user configures basic chart    Vertical bar    500    800

Change vertical bar chart legend
    user clicks link    Legend
    user waits until h3 is visible    Legend    %{WAIT_SMALL}

    user counts legend form item rows    1
    user checks element value should be    id:chartLegendConfigurationForm-items-0-label
    ...    Admission Numbers (Nailsea Youngwood)    %{WAIT_SMALL}

    user enters text into element    id:chartLegendConfigurationForm-items-0-label    Admissions

Validate basic vertical bar chart preview
    user waits until element does not contain line chart    id:chartBuilderPreview
    user waits until element contains bar chart    id:chartBuilderPreview

    user checks chart title contains    id:chartBuilderPreview    Test chart title
    user checks chart legend item contains    id:chartBuilderPreview    1    Admissions

    user checks chart height    id:chartBuilderPreview    500
    user checks chart width    id:chartBuilderPreview    800

    user checks chart y axis ticks    id:chartBuilderPreview    0    2,500    5,000    7,500    10,000
    user checks chart x axis ticks    id:chartBuilderPreview    2005    2010    2011    2012    2016

    user mouses over chart bar    id:chartBuilderPreview    1
    user checks chart tooltip label contains    id:chartBuilderPreview    2005
    user checks chart tooltip item contains    id:chartBuilderPreview    1    Admissions: 3,612

    user mouses over chart bar    id:chartBuilderPreview    2
    user checks chart tooltip label contains    id:chartBuilderPreview    2010
    user checks chart tooltip item contains    id:chartBuilderPreview    1    Admissions: 9,304

    user mouses over chart bar    id:chartBuilderPreview    3
    user checks chart tooltip label contains    id:chartBuilderPreview    2011
    user checks chart tooltip item contains    id:chartBuilderPreview    1    Admissions: 9,603

    user mouses over chart bar    id:chartBuilderPreview    4
    user checks chart tooltip label contains    id:chartBuilderPreview    2012
    user checks chart tooltip item contains    id:chartBuilderPreview    1    Admissions: 8,150

    user mouses over chart bar    id:chartBuilderPreview    5
    user checks chart tooltip label contains    id:chartBuilderPreview    2016
    user checks chart tooltip item contains    id:chartBuilderPreview    1    Admissions: 4,198

Save and validate vertical bar chart embeds correctly
    # Transient React error that happens locally & on dev sometimes: TypeError: Cannot read property '_leaflet_pos' of undefined
    user clicks link    Chart configuration
    user clicks button    Save chart options
    user waits until button is enabled    Save chart options

    user clicks link    Content
    user waits until h2 is visible    ${PUBLICATION_NAME}    %{WAIT_SMALL}
    user opens accordion section    ${CONTENT_SECTION_NAME}    id:releaseMainContent

    ${datablock}=    set variable    testid:Data block - ${DATABLOCK_NAME}
    user waits until page contains element    ${datablock}

    # Need to scroll to block to load it
    user scrolls to element    ${datablock}
    user waits until element does not contain line chart    ${datablock}
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
    user navigates to admin frontend    ${DATABLOCK_URL}
    user waits until h2 is visible    ${DATABLOCK_NAME}    %{WAIT_SMALL}
    user waits until page does not contain loading spinner

    user clicks link    Chart
    user configures basic chart    Horizontal bar    600    700

Validate basic horizontal bar chart preview
    user waits until element contains bar chart    id:chartBuilderPreview

    user checks chart title contains    id:chartBuilderPreview    Test chart title
    user checks chart legend item contains    id:chartBuilderPreview    1    Admissions

    user checks chart x axis ticks    id:chartBuilderPreview    0    2,500    5,000    7,500    10,000
    user checks chart y axis ticks    id:chartBuilderPreview    2005    2010    2011    2012    2016

    user checks chart height    id:chartBuilderPreview    600
    user checks chart width    id:chartBuilderPreview    700

    user mouses over chart bar    id:chartBuilderPreview    1
    user checks chart tooltip label contains    id:chartBuilderPreview    2005
    user checks chart tooltip item contains    id:chartBuilderPreview    1    Admissions: 3,612

    user mouses over chart bar    id:chartBuilderPreview    2
    user checks chart tooltip label contains    id:chartBuilderPreview    2010
    user checks chart tooltip item contains    id:chartBuilderPreview    1    Admissions: 9,304

    user mouses over chart bar    id:chartBuilderPreview    3
    user checks chart tooltip label contains    id:chartBuilderPreview    2011
    user checks chart tooltip item contains    id:chartBuilderPreview    1    Admissions: 9,603

    user mouses over chart bar    id:chartBuilderPreview    4
    user checks chart tooltip label contains    id:chartBuilderPreview    2012
    user checks chart tooltip item contains    id:chartBuilderPreview    1    Admissions: 8,150

    user mouses over chart bar    id:chartBuilderPreview    5
    user checks chart tooltip label contains    id:chartBuilderPreview    2016
    user checks chart tooltip item contains    id:chartBuilderPreview    1    Admissions: 4,198

Save and validate horizontal bar chart embeds correctly
    user clicks link    Chart configuration
    user clicks button    Save chart options
    user waits until button is enabled    Save chart options

    user clicks link    Content
    user waits until h2 is visible    ${PUBLICATION_NAME}
    user opens accordion section    ${CONTENT_SECTION_NAME}    id:releaseMainContent

    ${datablock}=    set variable    testid:Data block - ${DATABLOCK_NAME}
    user waits until page contains element    ${datablock}

    # Need to scroll to block to load it
    user scrolls to element    ${datablock}
    user waits until element contains bar chart    ${datablock}

    user checks chart title contains    ${datablock}    Test chart title
    user checks chart legend item contains    ${datablock}    1    Admissions

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
    user navigates to admin frontend    ${DATABLOCK_URL}
    user waits until h2 is visible    ${DATABLOCK_NAME}    %{WAIT_SMALL}
    user waits until page does not contain loading spinner

    user clicks link    Chart
    user configures basic chart    Geographic    700    600

Change geographic chart legend
    user clicks link    Legend
    user waits until h3 is visible    Legend    %{WAIT_MEDIUM}

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

    user waits until page does not contain loading spinner

Validate basic geographic chart preview
    user waits until element does not contain bar chart    id:chartBuilderPreview
    user waits until element contains map chart    id:chartBuilderPreview

    user checks map chart height    id:chartBuilderPreview    700
    user checks map chart width    id:chartBuilderPreview    600

    user chooses select option    id:chartBuilderPreview-map-selectedLocation    Nailsea Youngwood

    user mouses over selected map feature    id:chartBuilderPreview
    user checks map tooltip label contains    id:chartBuilderPreview    Nailsea Youngwood

    user checks map chart indicator tile contains    id:chartBuilderPreview    Admissions in 2005    3,612

Save and validate geographic chart embeds correctly
    user scrolls to the bottom of the page
    user clicks element    //*[@id="chartLegendConfigurationForm-submit"]
    user waits until page does not contain loading spinner

    user clicks link    Content
    user waits until h2 is visible    ${PUBLICATION_NAME}
    user opens accordion section    ${CONTENT_SECTION_NAME}    id:releaseMainContent
    user waits until page does not contain loading spinner

    ${datablock}=    set variable    testid:Data block - ${DATABLOCK_NAME}
    user waits until page contains element    ${datablock}

    # Need to scroll to block to load it
    user scrolls to element    ${datablock}
    user waits until element does not contain bar chart    ${datablock}
    user waits until element contains map chart    ${datablock}

    user checks map chart height    ${datablock}    700
    user checks map chart width    ${datablock}    600

    user chooses select option    ${datablock} >> name:selectedLocation    Nailsea Youngwood

    user mouses over selected map feature    ${datablock}
    user checks map tooltip label contains    ${datablock}    Nailsea Youngwood

    user checks map chart indicator tile contains    ${datablock}    Admissions in 2005    3,612

Configure basic infographic chart
    user navigates to admin frontend    ${DATABLOCK_URL}

    user waits until h2 is visible    ${DATABLOCK_NAME}
    user waits until page does not contain loading spinner

    user clicks link    Chart
    user waits until h3 is visible    Choose chart type
    user clicks button    Choose an infographic as alternative
    user chooses file    id:chartConfigurationForm-file    ${FILES_DIR}test-infographic.png

Validate basic infographic chart preview
    user checks chart title contains    id:chartBuilderPreview    Test chart title
    user waits until element contains infographic chart    id:chartBuilderPreview
    user checks infographic chart contains alt    id:chartBuilderPreview    Test chart alt

Save and validate infographic chart embeds correctly
    user clicks button    Save chart options
    user waits until button is enabled    Save chart options

    user clicks link    Content
    user waits until h2 is visible    ${PUBLICATION_NAME}
    user opens accordion section    ${CONTENT_SECTION_NAME}    id:releaseMainContent

    ${datablock}=    set variable    testid:Data block - ${DATABLOCK_NAME}
    user waits until page contains element    ${datablock}

    # Need to scroll to block to load it
    user scrolls to element    ${datablock}
    user checks chart title contains    ${datablock}    Test chart title
    user checks infographic chart contains alt    ${datablock}    Test chart alt

Delete embedded data block
    user clicks button    Remove block
    user clicks button    Confirm

Delete chart from data block
    user navigates to admin frontend    ${DATABLOCK_URL}
    user waits until h2 is visible    ${DATABLOCK_NAME}
    user waits until page does not contain loading spinner
    user clicks link    Chart
    user clicks button    Delete chart
    user clicks button    Confirm
    user waits until element does not contain infographic chart    id:chartBuilderPreview

Delete data block
    user clicks button    Delete this data block
    user waits until page does not contain loading spinner
    user clicks button    Confirm

    user waits until h2 is visible    Data blocks
    user waits until page contains    No data blocks have been created.


*** Keywords ***
user counts legend form item rows
    [Arguments]    ${number}
    user checks element count is x    css:fieldset[id^="chartLegendConfigurationForm-items"]    ${number}
