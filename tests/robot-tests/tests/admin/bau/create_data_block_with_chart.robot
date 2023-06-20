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
    user waits until table tool wizard step is available    1    Select a data set

Select subject "UI test subject"
    user waits until page contains    UI test subject    %{WAIT_SMALL}
    user clicks radio    UI test subject
    user clicks element    id:publicationDataStepForm-submit
    user waits until table tool wizard step is available    2    Choose locations    %{WAIT_MEDIUM}
    user checks previous table tool step contains    1    Data set    UI test subject

Select locations
    user clicks element    testid:Expand Details Section Local authority
    user clicks checkbox    Barnsley
    user clicks checkbox    Birmingham
    user clicks checkbox    Camden
    user clicks checkbox    Greenwich

    user opens details dropdown    Ward
    user clicks checkbox    Nailsea Youngwood
    user clicks checkbox    Syon
    user clicks element    id:locationFiltersForm-submit
    user waits until table tool wizard step is available    3    Choose time period    90

Select time period
    user waits until page contains element    id:timePeriodForm-start
    ${timePeriodStartList}=    get list items    id:timePeriodForm-start
    ${timePeriodEndList}=    get list items    id:timePeriodForm-end

    ${expectedList}=    create list    Please select    2005    2007    2008    2010    2011    2012
    ...    2014    2015    2016    2017    2018
    lists should be equal    ${timePeriodStartList}    ${expectedList}
    lists should be equal    ${timePeriodEndList}    ${expectedList}

    user chooses select option    id:timePeriodForm-start    2005
    user chooses select option    id:timePeriodForm-end    2018
    user clicks element    id:timePeriodForm-submit
    user waits until table tool wizard step is available    4    Choose your filters
    user checks previous table tool step contains    3    Time period    2005 to 2018    %{WAIT_MEDIUM}

Select indicators
    user checks indicator checkbox is checked    Admission Numbers

Create table
    [Documentation]    EES-615
    user clicks element    id:filtersForm-submit
    user waits until results table appears    %{WAIT_LONG}
    user waits until element contains    testid:dataTableCaption
    ...    Admission Numbers for 'UI test subject' in Barnsley, Birmingham, Camden, Greenwich, Nailsea Youngwood and 1 other location between 2005 and 2018

Validate table rows
    user checks table column heading contains    1    1    Admission Numbers

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

    ${row}=    user gets row number with heading    Barnsley
    user checks table heading in offset row contains    ${row}    0    2    2014
    user checks table heading in offset row contains    ${row}    1    1    2015
    user checks table heading in offset row contains    ${row}    2    1    2016
    user checks table heading in offset row contains    ${row}    3    1    2017
    user checks table heading in offset row contains    ${row}    4    1    2018

    user checks table cell in offset row contains    ${row}    0    1    9,854
    user checks table cell in offset row contains    ${row}    1    1    1,134
    user checks table cell in offset row contains    ${row}    2    1    7,419
    user checks table cell in offset row contains    ${row}    3    1    5,032
    user checks table cell in offset row contains    ${row}    4    1    8,123

    ${row}=    user gets row number with heading    Birmingham
    user checks table heading in offset row contains    ${row}    0    2    2014
    user checks table heading in offset row contains    ${row}    1    1    2015
    user checks table heading in offset row contains    ${row}    2    1    2016
    user checks table heading in offset row contains    ${row}    3    1    2017
    user checks table heading in offset row contains    ${row}    4    1    2018

    user checks table cell in offset row contains    ${row}    0    1    3,708
    user checks table cell in offset row contains    ${row}    1    1    9,303
    user checks table cell in offset row contains    ${row}    2    1    8,856
    user checks table cell in offset row contains    ${row}    3    1    8,530
    user checks table cell in offset row contains    ${row}    4    1    3,962

    ${row}=    user gets row number with heading    Camden
    user checks table heading in offset row contains    ${row}    0    2    2014
    user checks table heading in offset row contains    ${row}    1    1    2015
    user checks table heading in offset row contains    ${row}    2    1    2016
    user checks table heading in offset row contains    ${row}    3    1    2017
    user checks table heading in offset row contains    ${row}    4    1    2018

    user checks table cell in offset row contains    ${row}    0    1    1,054
    user checks table cell in offset row contains    ${row}    1    1    9,790
    user checks table cell in offset row contains    ${row}    2    1    3,548
    user checks table cell in offset row contains    ${row}    3    1    4,180
    user checks table cell in offset row contains    ${row}    4    1    2,399

    ${row}=    user gets row number with heading    Greenwich
    user checks table heading in offset row contains    ${row}    0    2    2014
    user checks table heading in offset row contains    ${row}    1    1    2015
    user checks table heading in offset row contains    ${row}    2    1    2016
    user checks table heading in offset row contains    ${row}    3    1    2017
    user checks table heading in offset row contains    ${row}    4    1    2018

    user checks table cell in offset row contains    ${row}    0    1    8,247
    user checks table cell in offset row contains    ${row}    1    1    6,114
    user checks table cell in offset row contains    ${row}    2    1    8,427
    user checks table cell in offset row contains    ${row}    3    1    6,981
    user checks table cell in offset row contains    ${row}    4    1    5,669

Save data block
    user enters text into element    label:Name    ${DATABLOCK_NAME}
    user enters text into element    label:Table title    UI test table title
    user enters text into element    label:Source    UI test source

    user clicks button    Save data block
    user waits until page contains    Delete this data block

Validate data block is in list
    user clicks link    Back
    user waits until h2 is visible    Data blocks

    user waits until table is visible
    user checks table column heading contains    1    1    Name    testid:dataBlocks
    user checks table column heading contains    1    2    Has chart    testid:dataBlocks
    user checks table column heading contains    1    3    In content    testid:dataBlocks
    user checks table column heading contains    1    4    Created date    testid:dataBlocks
    user checks table column heading contains    1    5    Actions    testid:dataBlocks

    user checks table body has x rows    1    testid:dataBlocks
    user checks table cell contains    1    1    ${DATABLOCK_NAME}    testid:dataBlocks
    user checks table cell contains    1    3    No    testid:dataBlocks

Start creating a featured table
    user clicks link    Create data block
    user waits until table tool wizard step is available    1    Select a data set

Select subject "UI test subject"
    user waits until page contains    UI test subject    %{WAIT_SMALL}
    user clicks radio    UI test subject
    user clicks element    id:publicationDataStepForm-submit
    user waits until table tool wizard step is available    2    Choose locations    %{WAIT_MEDIUM}
    user checks previous table tool step contains    1    Data set    UI test subject

Select locations
    user opens details dropdown    Opportunity area
    user clicks checkbox    Bolton 001

    user clicks element    id:locationFiltersForm-submit
    user waits until table tool wizard step is available    3    Choose time period    90

Select time period
    user waits until page contains element    id:timePeriodForm-start
    user chooses select option    id:timePeriodForm-start    2009
    user chooses select option    id:timePeriodForm-end    2017
    user clicks element    id:timePeriodForm-submit
    user waits until table tool wizard step is available    4    Choose your filters
    user checks previous table tool step contains    3    Time period    2009 to 2017    %{WAIT_MEDIUM}

Select indicators
    user checks indicator checkbox is checked    Admission Numbers

Create table
    [Documentation]    EES-615
    user clicks element    id:filtersForm-submit
    user waits until results table appears    %{WAIT_LONG}
    user waits until element contains    testid:dataTableCaption
    ...    Admission Numbers for 'UI test subject' in Bolton 001 between 2009 and 2017

Save data block
    user enters text into element    label:Name    UI test featured table
    user enters text into element    label:Table title    UI test featured table title
    user enters text into element    label:Source    UI test featured table source

    user clicks checkbox    Set as a featured table for this publication
    user waits until page contains element    label:Featured table name
    user enters text into element    label:Featured table name    UI test featured table name
    user enters text into element    label:Featured table description    UI test featured table description

    user clicks button    Save data block
    user waits until page contains    Delete this data block

Validate data block is in list
    user clicks link    Back
    user waits until h2 is visible    Data blocks

    user waits until table is visible
    user checks table column heading contains    1    1    Data block name    testid:featuredTables
    user checks table column heading contains    1    2    Has chart    testid:featuredTables
    user checks table column heading contains    1    3    In content    testid:featuredTables
    user checks table column heading contains    1    4    Featured table name    testid:featuredTables
    user checks table column heading contains    1    5    Created date    testid:featuredTables
    user checks table column heading contains    1    6    Actions    testid:featuredTables

    user checks table body has x rows    1    testid:featuredTables
    user checks table cell contains    1    1    UI test featured table    testid:featuredTables
    user checks table cell contains    1    3    No    testid:featuredTables
    user checks table cell contains    1    4    UI test featured table name    testid:featuredTables

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

    ${row}=    user gets row number with heading    Barnsley    ${table}
    user checks table heading in offset row contains    ${row}    0    2    2014    ${table}
    user checks table heading in offset row contains    ${row}    1    1    2015    ${table}
    user checks table heading in offset row contains    ${row}    2    1    2016    ${table}
    user checks table heading in offset row contains    ${row}    3    1    2017    ${table}
    user checks table heading in offset row contains    ${row}    4    1    2018    ${table}

    user checks table cell in offset row contains    ${row}    0    1    9,854    ${table}
    user checks table cell in offset row contains    ${row}    1    1    1,134    ${table}
    user checks table cell in offset row contains    ${row}    2    1    7,419    ${table}
    user checks table cell in offset row contains    ${row}    3    1    5,032    ${table}
    user checks table cell in offset row contains    ${row}    4    1    8,123    ${table}

    ${row}=    user gets row number with heading    Birmingham    ${table}
    user checks table heading in offset row contains    ${row}    0    2    2014    ${table}
    user checks table heading in offset row contains    ${row}    1    1    2015    ${table}
    user checks table heading in offset row contains    ${row}    2    1    2016    ${table}
    user checks table heading in offset row contains    ${row}    3    1    2017    ${table}
    user checks table heading in offset row contains    ${row}    4    1    2018    ${table}

    user checks table cell in offset row contains    ${row}    0    1    3,708    ${table}
    user checks table cell in offset row contains    ${row}    1    1    9,303    ${table}
    user checks table cell in offset row contains    ${row}    2    1    8,856    ${table}
    user checks table cell in offset row contains    ${row}    3    1    8,530    ${table}
    user checks table cell in offset row contains    ${row}    4    1    3,962    ${table}

    ${row}=    user gets row number with heading    Camden    ${table}
    user checks table heading in offset row contains    ${row}    0    2    2014    ${table}
    user checks table heading in offset row contains    ${row}    1    1    2015    ${table}
    user checks table heading in offset row contains    ${row}    2    1    2016    ${table}
    user checks table heading in offset row contains    ${row}    3    1    2017    ${table}
    user checks table heading in offset row contains    ${row}    4    1    2018    ${table}

    user checks table cell in offset row contains    ${row}    0    1    1,054    ${table}
    user checks table cell in offset row contains    ${row}    1    1    9,790    ${table}
    user checks table cell in offset row contains    ${row}    2    1    3,548    ${table}
    user checks table cell in offset row contains    ${row}    3    1    4,180    ${table}
    user checks table cell in offset row contains    ${row}    4    1    2,399    ${table}

    ${row}=    user gets row number with heading    Greenwich    ${table}
    user checks table heading in offset row contains    ${row}    0    2    2014    ${table}
    user checks table heading in offset row contains    ${row}    1    1    2015    ${table}
    user checks table heading in offset row contains    ${row}    2    1    2016    ${table}
    user checks table heading in offset row contains    ${row}    3    1    2017    ${table}
    user checks table heading in offset row contains    ${row}    4    1    2018    ${table}

    user checks table cell in offset row contains    ${row}    0    1    8,247    ${table}
    user checks table cell in offset row contains    ${row}    1    1    6,114    ${table}
    user checks table cell in offset row contains    ${row}    2    1    8,427    ${table}
    user checks table cell in offset row contains    ${row}    3    1    6,981    ${table}
    user checks table cell in offset row contains    ${row}    4    1    5,669    ${table}

Validate marked as 'In content' on data block list
    user clicks link    Data blocks
    user waits until h2 is visible    Data blocks

    user waits until table is visible
    user checks table column heading contains    1    1    Name    testid:dataBlocks
    user checks table column heading contains    1    3    In content    testid:dataBlocks

    user checks table body has x rows    1
    user checks table cell contains    1    1    ${DATABLOCK_NAME}    testid:dataBlocks
    user checks table cell contains    1    3    Yes    testid:dataBlocks

Navigate to Chart tab
    user clicks link    Edit block    testid:dataBlocks

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
    user enters text into element    label:Enter chart title    Test chart title
    user enters text into element    label:Alt text    Test chart alt
    user enters text into element    label:Height (pixels)    400
    user enters text into element    label:Width (pixels)    900

Validate changing data sets
    user clicks link    Data sets
    user waits until h3 is visible    Data sets

    user chooses select option    id:chartDataSetsConfigurationForm-location    Nailsea Youngwood
    user clicks button    Add data set

    user chooses select option    id:chartDataSetsConfigurationForm-location    Syon
    user clicks button    Add data set

    user chooses select option    id:chartDataSetsConfigurationForm-location    Barnsley
    user clicks button    Add data set

    user checks chart legend item contains    id:chartBuilderPreview    1    Admission Numbers (Nailsea Youngwood)
    user checks chart legend item contains    id:chartBuilderPreview    2    Admission Numbers (Syon)
    user checks chart legend item contains    id:chartBuilderPreview    3    Admission Numbers (Barnsley)

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
    user checks table column heading contains    1    1    Name    testid:dataBlocks
    user checks table column heading contains    1    2    Has chart    testid:dataBlocks

    user checks table body has x rows    1
    user checks table cell contains    1    1    ${DATABLOCK_NAME}    testid:dataBlocks
    user checks table cell contains    1    2    Yes    testid:dataBlocks

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

    user clicks link    Data sets
    user waits until h3 is visible    Data sets

    user clicks button    Remove all
    user clicks button    Confirm

    user chooses select option    id:chartDataSetsConfigurationForm-location    Barnsley
    user clicks button    Add data set
    user chooses select option    id:chartDataSetsConfigurationForm-location    Birmingham
    user clicks button    Add data set
    user chooses select option    id:chartDataSetsConfigurationForm-location    Camden
    user clicks button    Add data set
    user chooses select option    id:chartDataSetsConfigurationForm-location    Greenwich
    user clicks button    Add data set

Change geographic chart legend
    user clicks link    Legend
    user waits until h3 is visible    Legend    %{WAIT_MEDIUM}

    user counts legend form item rows    5
    user checks element value should be    id:chartLegendConfigurationForm-items-0-label    Admission Numbers (2014)
    user checks element value should be    id:chartLegendConfigurationForm-items-1-label    Admission Numbers (2015)
    user checks element value should be    id:chartLegendConfigurationForm-items-2-label    Admission Numbers (2016)
    user checks element value should be    id:chartLegendConfigurationForm-items-3-label    Admission Numbers (2017)
    user checks element value should be    id:chartLegendConfigurationForm-items-4-label    Admission Numbers (2018)

    user enters text into element    id:chartLegendConfigurationForm-items-0-label    Admissions in 2014
    user enters text into element    id:chartLegendConfigurationForm-items-1-label    Admissions in 2015
    user enters text into element    id:chartLegendConfigurationForm-items-2-label    Admissions in 2016
    user enters text into element    id:chartLegendConfigurationForm-items-3-label    Admissions in 2017
    user enters text into element    id:chartLegendConfigurationForm-items-4-label    Admissions in 2018

    user waits until page does not contain loading spinner

Validate basic geographic chart preview
    user waits until element does not contain bar chart    id:chartBuilderPreview
    user waits until element contains map chart    id:chartBuilderPreview

    user checks map chart height    id:chartBuilderPreview    700
    user checks map chart width    id:chartBuilderPreview    600

    user chooses select option    id:chartBuilderPreview-map-selectedLocation    Barnsley

    user mouses over selected map feature    id:chartBuilderPreview
    user checks map tooltip label contains    id:chartBuilderPreview    Barnsley

    user checks map chart indicator tile contains    id:chartBuilderPreview    Admissions in 2014    9,854

    user checks list has x items    testid:mapBlock-legend    5
    user checks list item contains    testid:mapBlock-legend    1    1,054 to 2,813
    user checks list item contains    testid:mapBlock-legend    2    2,814 to 4,573
    user checks list item contains    testid:mapBlock-legend    3    4,574 to 6,333
    user checks list item contains    testid:mapBlock-legend    4    6,334 to 8,093
    user checks list item contains    testid:mapBlock-legend    5    8,094 to 9,854

Change geographic chart data groupings
    user clicks link    Data groupings
    user waits until h3 is visible    Data groupings

    user checks table body has x rows    5    testid:chart-data-groupings
    user checks table cell contains    1    1    Admission Numbers (All locations, 2014)    testid:chart-data-groupings
    user checks table cell contains    1    2    5 equal intervals    testid:chart-data-groupings
    user checks table cell contains    2    1    Admission Numbers (All locations, 2015)    testid:chart-data-groupings
    user checks table cell contains    2    2    5 equal intervals    testid:chart-data-groupings
    user checks table cell contains    3    1    Admission Numbers (All locations, 2016)    testid:chart-data-groupings
    user checks table cell contains    3    2    5 equal intervals    testid:chart-data-groupings
    user checks table cell contains    4    1    Admission Numbers (All locations, 2017)    testid:chart-data-groupings
    user checks table cell contains    4    2    5 equal intervals    testid:chart-data-groupings
    user checks table cell contains    5    1    Admission Numbers (All locations, 2018)    testid:chart-data-groupings
    user checks table cell contains    5    2    5 equal intervals    testid:chart-data-groupings

    user clicks button in table cell    1    3    Edit    testid:chart-data-groupings
    user clicks radio    New custom groups
    user enters text into element    label:Min    0
    user enters text into element    label:Max    3000
    user clicks button    Add group
    user enters text into element    label:Min    3001
    user enters text into element    label:Max    10000
    user clicks button    Add group
    user clicks button    Done
    user checks table cell contains    1    2    Custom    testid:chart-data-groupings

    user clicks button in table cell    2    3    Edit    testid:chart-data-groupings
    user clicks radio    Quantiles
    user enters text into element    id:chartDataGroupingForm-numberOfGroupsQuantiles    4
    user clicks button    Done
    user checks table cell contains    2    2    4 quantiles    testid:chart-data-groupings

    user clicks button in table cell    3    3    Edit    testid:chart-data-groupings
    user enters text into element    id:chartDataGroupingForm-numberOfGroups    3
    user clicks button    Done
    user checks table cell contains    3    2    3 equal intervals    testid:chart-data-groupings

    user clicks button in table cell    4    3    Edit    testid:chart-data-groupings
    user clicks radio    Copy custom groups
    user chooses select option    id:chartDataGroupingForm-copyCustomGroups    Admission Numbers (All locations, 2014)
    user clicks button    Done
    user checks table cell contains    4    2    Custom    testid:chart-data-groupings

Validate basic geographic chart preview updates correctly
    user checks list has x items    testid:mapBlock-legend    2
    user checks list item contains    testid:mapBlock-legend    1    0 to 3,000
    user checks list item contains    testid:mapBlock-legend    2    3,001 to 10,000

    user chooses select option    id:chartBuilderPreview-map-selectedDataSet    Admissions in 2015
    user checks list has x items    testid:mapBlock-legend    4
    user checks list item contains    testid:mapBlock-legend    1    1,134 to 4,869
    user checks list item contains    testid:mapBlock-legend    2    4,870 to 7,709
    user checks list item contains    testid:mapBlock-legend    3    7,710 to 9,425
    user checks list item contains    testid:mapBlock-legend    4    9,426 to 9,790

    user chooses select option    id:chartBuilderPreview-map-selectedDataSet    Admissions in 2016
    user checks list has x items    testid:mapBlock-legend    3
    user checks list item contains    testid:mapBlock-legend    1    3,548 to 5,317
    user checks list item contains    testid:mapBlock-legend    2    5,318 to 7,087
    user checks list item contains    testid:mapBlock-legend    3    7,088 to 8,856

    user chooses select option    id:chartBuilderPreview-map-selectedDataSet    Admissions in 2017
    user checks list has x items    testid:mapBlock-legend    2
    user checks list item contains    testid:mapBlock-legend    1    0 to 3,000
    user checks list item contains    testid:mapBlock-legend    2    3,001 to 10,000

Save and validate geographic chart embeds correctly
    user scrolls to the bottom of the page
    user clicks element    //*[@id="chartDataGroupingsConfigurationForm-submit"]
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

    user chooses select option    ${datablock} >> name:selectedLocation    Barnsley

    user mouses over selected map feature    ${datablock}
    user checks map tooltip label contains    ${datablock}    Barnsley

    user checks map chart indicator tile contains    ${datablock}    Admissions in 2014    9,854

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
    user waits until page does not contain button    Confirm

Delete featured table
    user clicks button    Delete block
    user waits until page does not contain loading spinner
    user clicks button    Confirm
    user waits until page does not contain button    Confirm


*** Keywords ***
user counts legend form item rows
    [Arguments]    ${number}
    user checks element count is x    css:fieldset[id^="chartLegendConfigurationForm-items"]    ${number}
