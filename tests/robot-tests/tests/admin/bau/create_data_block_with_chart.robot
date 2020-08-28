*** Settings ***
Resource    ../../libs/admin-common.robot
Resource    ../../libs/charts.robot
Library  Collections

Force Tags  Admin  Local  Dev  AltersData

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Variables ***
${TOPIC_NAME}               %{TEST_TOPIC_NAME}
${PUBLICATION_NAME}         UI tests - create data block with chart %{RUN_IDENTIFIER}
${DATABLOCK_NAME}           UI test data block
${CONTENT_SECTION_NAME}     Test data block section

*** Test Cases ***
Create test publication
    [Tags]  HappyPath
    user selects theme "Test theme" and topic "${TOPIC_NAME}" from the admin dashboard
    user waits until page contains link  Create new publication     60
    user clicks link   Create new publication
    user creates publication    ${PUBLICATION_NAME}

Verify test publication is created
    [Tags]  HappyPath
    user waits until page contains accordion section  ${PUBLICATION_NAME}
    user opens accordion section  ${PUBLICATION_NAME}
    user waits until accordion section contains text  ${PUBLICATION_NAME}    Methodology
    user waits until accordion section contains text  ${PUBLICATION_NAME}    Releases

Create release
    [Tags]  HappyPath
    user clicks testid element  Create new release link for ${PUBLICATION_NAME}
    user creates release for publication  ${PUBLICATION_NAME}  Academic Year  2025
    user checks summary list contains  Publication title  ${PUBLICATION_NAME}

Upload subject
    [Tags]  HappyPath
    user waits until h2 is visible    Release summary
    user checks summary list contains  Publication title  ${PUBLICATION_NAME}
    user clicks link  Manage data
    user enters text into element  id:dataFileUploadForm-subjectTitle   UI test subject
    user chooses file   id:dataFileUploadForm-dataFile       ${CURDIR}${/}files${/}upload-file-test.csv
    user chooses file   id:dataFileUploadForm-metadataFile   ${CURDIR}${/}files${/}upload-file-test.meta.csv
    user clicks button  Upload data files

    user waits until h2 is visible  Uploaded data files
    user waits until page contains accordion section   UI test subject
    user opens accordion section   UI test subject

    ${section}=  user gets accordion content element  UI test subject
    user checks summary list contains  Subject title    UI test subject  ${section}
    user checks summary list contains  Data file        upload-file-test.csv  ${section}
    user checks summary list contains  Metadata file    upload-file-test.meta.csv  ${section}
    user checks summary list contains  Number of rows   159  ${section}
    user checks summary list contains  Data file size   15 Kb  ${section}
    user checks summary list contains  Status           Complete  ${section}  180

Navigate to Manage data blocks tab
    [Tags]  HappyPath
    user clicks link    Manage data blocks
    user waits until h2 is visible   Choose a subject

Select subject "UI test subject"
    [Tags]  HappyPath
    user waits until page contains   UI test subject
    user clicks radio    UI test subject
    user clicks element   id:publicationSubjectForm-submit
    user waits until h2 is visible  Choose locations    90
    user checks previous table tool step contains  1    Subject     UI test subject

Select locations
    [Tags]   HappyPath
    user opens details dropdown   Opportunity Area
    user clicks checkbox   Bolton 001 (E02000984)
    user clicks checkbox   Bolton 001 (E05000364)
    user clicks checkbox   Bolton 004 (E02000987)
    user clicks checkbox   Bolton 004 (E05010450)
    user opens details dropdown   Ward
    user clicks checkbox   Nailsea Youngwood
    user clicks checkbox   Syon
    user clicks element     id:locationFiltersForm-submit
    user waits until h2 is visible  Choose time period  90

Select time period
    [Tags]   HappyPath
    ${timePeriodStartList}=   get list items  id:timePeriodForm-start
    ${timePeriodEndList}=   get list items  id:timePeriodForm-end
    ${expectedList}=   create list   Please select  2005  2007  2008  2009  2010  2011  2012  2016  2017  2018  2019  2020
    lists should be equal  ${timePeriodStartList}   ${expectedList}
    lists should be equal  ${timePeriodEndList}   ${expectedList}

    user selects from list by label  id:timePeriodForm-start  2005
    user selects from list by label  id:timePeriodForm-end  2020
    user clicks element     id:timePeriodForm-submit
    user waits until h2 is visible  Choose your filters
    user checks previous table tool step contains  3    Start date    2005
    user checks previous table tool step contains  3    End date      2020

Select indicators
    [Tags]  HappyPath
    user clicks indicator checkbox    Admission Numbers

Create table
    [Tags]  HappyPath
    [Documentation]   EES-615
    user clicks element   id:filtersForm-submit
    user waits until results table appears     180
    user waits until element contains  id:dataTableCaption  Table showing Admission Numbers for 'UI test subject' from '${PUBLICATION_NAME}' in Bolton 001 (E02000984), Bolton 001 (E05000364), Bolton 004 (E02000987), Bolton 004 (E05010450), Nailsea Youngwood and Syon between 2005 and 2020

Validate table rows
    [Tags]  HappyPath
    ${table}=  set variable  css:table
    user checks table column heading contains  ${table}  1  1  Admission Numbers

    ${row}=  user gets row number with heading  ${table}  Bolton 001 (E02000984)
    user checks table heading in offset row contains  ${table}  ${row}  0  2  2019

    user checks table cell in offset row contains  ${table}  ${row}  0  1  8,533

    ${row}=  user gets row number with heading   ${table}  Bolton 001 (E05000364)
    user checks table heading in offset row contains  ${table}  ${row}  0  2  2009
    user checks table heading in offset row contains  ${table}  ${row}  1  1  2010
    user checks table heading in offset row contains  ${table}  ${row}  2  1  2017

    user checks table cell in offset row contains  ${table}  ${row}  0  1  5,815
    user checks table cell in offset row contains  ${table}  ${row}  1  1  5,595
    user checks table cell in offset row contains  ${table}  ${row}  2  1  6,373

    ${row}=  user gets row number with heading  ${table}  Bolton 004 (E02000987)
    user checks table heading in offset row contains  ${table}  ${row}  0  2  2020

    user checks table cell in offset row contains  ${table}  ${row}  0  1  6,031

    ${row}=  user gets row number with heading  ${table}  Bolton 004 (E05010450)
    user checks table heading in offset row contains  ${table}  ${row}  0  2  2005
    user checks table heading in offset row contains  ${table}  ${row}  1  1  2017
    user checks table heading in offset row contains  ${table}  ${row}  2  1  2018

    user checks table cell in offset row contains  ${table}  ${row}  0  1  8,557
    user checks table cell in offset row contains  ${table}  ${row}  1  1  3,481
    user checks table cell in offset row contains  ${table}  ${row}  2  1  8,630

    ${row}=  user gets row number with heading  ${table}  Nailsea Youngwood
    user checks table heading in offset row contains  ${table}  ${row}  0  2  2005
    user checks table heading in offset row contains  ${table}  ${row}  1  1  2010
    user checks table heading in offset row contains  ${table}  ${row}  2  1  2011
    user checks table heading in offset row contains  ${table}  ${row}  3  1  2012
    user checks table heading in offset row contains  ${table}  ${row}  4  1  2016

    user checks table cell in offset row contains  ${table}  ${row}  0  1  3,612
    user checks table cell in offset row contains  ${table}  ${row}  1  1  9,304
    user checks table cell in offset row contains  ${table}  ${row}  2  1  9,603
    user checks table cell in offset row contains  ${table}  ${row}  3  1  8,150
    user checks table cell in offset row contains  ${table}  ${row}  4  1  4,198

    ${row}=  user gets row number with heading  ${table}  Syon
    user checks table heading in offset row contains  ${table}  ${row}  0  2  2007
    user checks table heading in offset row contains  ${table}  ${row}  1  1  2008
    user checks table heading in offset row contains  ${table}  ${row}  2  1  2010
    user checks table heading in offset row contains  ${table}  ${row}  3  1  2012
    user checks table heading in offset row contains  ${table}  ${row}  4  1  2017

    user checks table cell in offset row contains  ${table}  ${row}  0  1  9,914
    user checks table cell in offset row contains  ${table}  ${row}  1  1  5,505
    user checks table cell in offset row contains  ${table}  ${row}  2  1  6,060
    user checks table cell in offset row contains  ${table}  ${row}  3  1  1,109
    user checks table cell in offset row contains  ${table}  ${row}  4  1  1,959

Save data block
    [Tags]  HappyPath
    user enters text into element  id:dataBlockDetailsForm-name         ${DATABLOCK_NAME}
    user enters text into element  id:dataBlockDetailsForm-heading      UI test table title
    user enters text into element  id:dataBlockDetailsForm-source       UI test source
    user clicks button   Save data block
    user waits until page contains    Delete this data block

Embed data block into release content
    [Tags]  HappyPath
    user clicks link  Manage content
    user waits until h2 is visible  ${PUBLICATION_NAME}
    user clicks button  Add new section
    user changes accordion section title  1   ${CONTENT_SECTION_NAME}
    user clicks button   Add data block
    user selects from list by label  css:select[name="selectedDataBlock"]  ${DATABLOCK_NAME}
    user waits until element is visible  css:table
    user clicks button  Embed
    # Wait for table to update
    sleep  0.3s

Validate embedded table rows
    [Tags]  HappyPath
    ${table}=  set variable  css:[data-testid="Data block - ${DATABLOCK_NAME}"] table
    user scrolls to element   xpath://button[text()="${CONTENT_SECTION_NAME}"]
    user waits until page contains element  ${table}   30

    user checks table column heading contains  ${table}  1  1  Admission Numbers

    ${row}=  user gets row number with heading  ${table}  Bolton 001 (E02000984)
    user checks table heading in offset row contains  ${table}  ${row}  0  2  2019

    user checks table cell in offset row contains  ${table}  ${row}  0  1  8,533

    ${row}=  user gets row number with heading   ${table}  Bolton 001 (E05000364)
    user checks table heading in offset row contains  ${table}  ${row}  0  2  2009
    user checks table heading in offset row contains  ${table}  ${row}  1  1  2010
    user checks table heading in offset row contains  ${table}  ${row}  2  1  2017

    user checks table cell in offset row contains  ${table}  ${row}  0  1  5,815
    user checks table cell in offset row contains  ${table}  ${row}  1  1  5,595
    user checks table cell in offset row contains  ${table}  ${row}  2  1  6,373

    ${row}=  user gets row number with heading  ${table}  Bolton 004 (E02000987)
    user checks table heading in offset row contains  ${table}  ${row}  0  2  2020

    user checks table cell in offset row contains  ${table}  ${row}  0  1  6,031

    ${row}=  user gets row number with heading  ${table}  Bolton 004 (E05010450)
    user checks table heading in offset row contains  ${table}  ${row}  0  2  2005
    user checks table heading in offset row contains  ${table}  ${row}  1  1  2017
    user checks table heading in offset row contains  ${table}  ${row}  2  1  2018

    user checks table cell in offset row contains  ${table}  ${row}  0  1  8,557
    user checks table cell in offset row contains  ${table}  ${row}  1  1  3,481
    user checks table cell in offset row contains  ${table}  ${row}  2  1  8,630

    ${row}=  user gets row number with heading  ${table}  Nailsea Youngwood
    user checks table heading in offset row contains  ${table}  ${row}  0  2  2005
    user checks table heading in offset row contains  ${table}  ${row}  1  1  2010
    user checks table heading in offset row contains  ${table}  ${row}  2  1  2011
    user checks table heading in offset row contains  ${table}  ${row}  3  1  2012
    user checks table heading in offset row contains  ${table}  ${row}  4  1  2016

    user checks table cell in offset row contains  ${table}  ${row}  0  1  3,612
    user checks table cell in offset row contains  ${table}  ${row}  1  1  9,304
    user checks table cell in offset row contains  ${table}  ${row}  2  1  9,603
    user checks table cell in offset row contains  ${table}  ${row}  3  1  8,150
    user checks table cell in offset row contains  ${table}  ${row}  4  1  4,198

    ${row}=  user gets row number with heading  ${table}  Syon
    user checks table heading in offset row contains  ${table}  ${row}  0  2  2007
    user checks table heading in offset row contains  ${table}  ${row}  1  1  2008
    user checks table heading in offset row contains  ${table}  ${row}  2  1  2010
    user checks table heading in offset row contains  ${table}  ${row}  3  1  2012
    user checks table heading in offset row contains  ${table}  ${row}  4  1  2017

    user checks table cell in offset row contains  ${table}  ${row}  0  1  9,914
    user checks table cell in offset row contains  ${table}  ${row}  1  1  5,505
    user checks table cell in offset row contains  ${table}  ${row}  2  1  6,060
    user checks table cell in offset row contains  ${table}  ${row}  3  1  1,109
    user checks table cell in offset row contains  ${table}  ${row}  4  1  1,959

Navigate to Chart tab
    [Tags]  HappyPath
    user clicks link  Manage data blocks
    user selects from list by label  id:selectedDataBlock  ${DATABLOCK_NAME}
    user waits until h2 is visible  ${DATABLOCK_NAME}
    user waits until page does not contain loading spinner
    user clicks link   Chart
    user waits until h3 is visible  Choose chart type

Configure basic line chart
    [Tags]  HappyPath
    user clicks button  Line
    user enters text into element  id:chartConfigurationForm-title      Test chart title
    user enters text into element  id:chartConfigurationForm-alt        Test chart alt

    user clicks link  Data sets
    user waits until h3 is visible  Add data sets to the chart
    user selects from list by label  id:chartDataSelectorForm-location  Nailsea Youngwood
    user clicks button  Add data

Validate basic line chart preview
    [Tags]  HappyPath
    user waits until element contains line chart  id:chartBuilderPreview

    user checks chart title contains  id:chartBuilderPreview  Test chart title
    user checks chart legend item contains  id:chartBuilderPreview  1  Admission Numbers (Nailsea Youngwood)

    user checks chart y axis ticks  id:chartBuilderPreview  0     2500  5000  7500  10000
    user checks chart x axis ticks  id:chartBuilderPreview  2005  2010  2011  2012  2016

    user mouses over line chart point  id:chartBuilderPreview  1  1
    user checks chart tooltip label contains  id:chartBuilderPreview  2005
    user checks chart tooltip item contains  id:chartBuilderPreview  1  Admission Numbers (Nailsea Youngwood): 3,612

    user mouses over line chart point  id:chartBuilderPreview  1  2
    user checks chart tooltip label contains  id:chartBuilderPreview  2010
    user checks chart tooltip item contains  id:chartBuilderPreview  1  Admission Numbers (Nailsea Youngwood): 9,304

    user mouses over line chart point  id:chartBuilderPreview  1  3
    user checks chart tooltip label contains  id:chartBuilderPreview  2011
    user checks chart tooltip item contains  id:chartBuilderPreview  1  Admission Numbers (Nailsea Youngwood): 9,603

    user mouses over line chart point  id:chartBuilderPreview  1  4
    user checks chart tooltip label contains  id:chartBuilderPreview  2012
    user checks chart tooltip item contains  id:chartBuilderPreview  1  Admission Numbers (Nailsea Youngwood): 8,150

    user mouses over line chart point  id:chartBuilderPreview  1  5
    user checks chart tooltip label contains  id:chartBuilderPreview  2016
    user checks chart tooltip item contains  id:chartBuilderPreview  1  Admission Numbers (Nailsea Youngwood): 4,198

Save and validate line chart embeds correctly
    [Tags]  HappyPath
    user clicks link  Chart configuration
    user clicks button  Save chart options
    user waits until button is enabled  Save chart options

    user clicks link  Manage content
    user waits until h2 is visible  ${PUBLICATION_NAME}
    user opens accordion section  ${CONTENT_SECTION_NAME}

    ${datablock}=  set variable  css:[data-testid="Data block - ${DATABLOCK_NAME}"]
    user waits until element contains line chart  ${datablock}

    user checks chart title contains  ${datablock}  Test chart title
    user checks chart legend item contains  ${datablock}  1  Admission Numbers (Nailsea Youngwood)

    user checks chart y axis ticks  ${datablock}  0     2500  5000  7500  10000
    user checks chart x axis ticks  ${datablock}  2005  2010  2011  2012  2016

    user mouses over line chart point  ${datablock}  1  1
    user checks chart tooltip label contains  ${datablock}  2005
    user checks chart tooltip item contains  ${datablock}  1  Admission Numbers (Nailsea Youngwood): 3,612

    user mouses over line chart point  ${datablock}  1  2
    user checks chart tooltip label contains  ${datablock}  2010
    user checks chart tooltip item contains  ${datablock}  1  Admission Numbers (Nailsea Youngwood): 9,304

    user mouses over line chart point  ${datablock}  1  3
    user checks chart tooltip label contains  ${datablock}  2011
    user checks chart tooltip item contains  ${datablock}  1  Admission Numbers (Nailsea Youngwood): 9,603

    user mouses over line chart point  ${datablock}  1  4
    user checks chart tooltip label contains  ${datablock}  2012
    user checks chart tooltip item contains  ${datablock}  1  Admission Numbers (Nailsea Youngwood): 8,150

    user mouses over line chart point  ${datablock}  1  5
    user checks chart tooltip label contains  ${datablock}  2016
    user checks chart tooltip item contains  ${datablock}  1  Admission Numbers (Nailsea Youngwood): 4,198

Configure basic vertical bar chart
    [Tags]  HappyPath
    user clicks link  Manage data blocks
    user selects from list by label  id:selectedDataBlock  ${DATABLOCK_NAME}
    user waits until h2 is visible  ${DATABLOCK_NAME}
    user waits until page does not contain loading spinner
    user clicks link  Chart
    user waits until h3 is visible  Choose chart type
    user clicks button  Vertical bar

Validate basic vertical bar chart preview
    [Tags]  HappyPath
    ${preview}=  set variable  id:chartBuilderPreview
    user waits until element does not contain line chart  ${preview}
    user waits until element contains bar chart  ${preview}

    user checks chart title contains  ${preview}  Test chart title
    user checks chart legend item contains  ${preview}  1  Admission Numbers (Nailsea Youngwood)

    user checks chart y axis ticks  ${preview}  0     2500  5000  7500  10000
    user checks chart x axis ticks  ${preview}  2005  2010  2011  2012  2016

    user mouses over chart bar  ${preview}  1
    user checks chart tooltip label contains  ${preview}  2005
    user checks chart tooltip item contains  ${preview}  1  Admission Numbers (Nailsea Youngwood): 3,612

    user mouses over chart bar  ${preview}  2
    user checks chart tooltip label contains  ${preview}  2010
    user checks chart tooltip item contains  ${preview}  1  Admission Numbers (Nailsea Youngwood): 9,304

    user mouses over chart bar  ${preview}  3
    user checks chart tooltip label contains  ${preview}  2011
    user checks chart tooltip item contains  ${preview}  1  Admission Numbers (Nailsea Youngwood): 9,603

    user mouses over chart bar  ${preview}  4
    user checks chart tooltip label contains  ${preview}  2012
    user checks chart tooltip item contains  ${preview}  1  Admission Numbers (Nailsea Youngwood): 8,150

    user mouses over chart bar  ${preview}  5
    user checks chart tooltip label contains  ${preview}  2016
    user checks chart tooltip item contains  ${preview}  1  Admission Numbers (Nailsea Youngwood): 4,198

Save and validate vertical bar chart embeds correctly
    [Tags]  HappyPath
    user clicks link  Chart configuration
    user clicks button  Save chart options
    user waits until button is enabled  Save chart options

    user clicks link  Manage content
    user waits until h2 is visible  ${PUBLICATION_NAME}
    user opens accordion section  ${CONTENT_SECTION_NAME}

    ${datablock}=  set variable  css:[data-testid="Data block - ${DATABLOCK_NAME}"]
    user waits until element does not contain line chart  ${datablock}
    user waits until element contains bar chart  ${datablock}

    user checks chart title contains  ${datablock}  Test chart title
    user checks chart legend item contains  ${datablock}  1  Admission Numbers (Nailsea Youngwood)

    user checks chart y axis ticks  ${datablock}  0     2500  5000  7500  10000
    user checks chart x axis ticks  ${datablock}  2005  2010  2011  2012  2016

    user mouses over chart bar  ${datablock}  1
    user checks chart tooltip label contains  ${datablock}  2005
    user checks chart tooltip item contains  ${datablock}  1  Admission Numbers (Nailsea Youngwood): 3,612

    user mouses over chart bar  ${datablock}  2
    user checks chart tooltip label contains  ${datablock}  2010
    user checks chart tooltip item contains  ${datablock}  1  Admission Numbers (Nailsea Youngwood): 9,304

    user mouses over chart bar  ${datablock}  3
    user checks chart tooltip label contains  ${datablock}  2011
    user checks chart tooltip item contains  ${datablock}  1  Admission Numbers (Nailsea Youngwood): 9,603

    user mouses over chart bar  ${datablock}  4
    user checks chart tooltip label contains  ${datablock}  2012
    user checks chart tooltip item contains  ${datablock}  1  Admission Numbers (Nailsea Youngwood): 8,150

    user mouses over chart bar  ${datablock}  5
    user checks chart tooltip label contains  ${datablock}  2016
    user checks chart tooltip item contains  ${datablock}  1  Admission Numbers (Nailsea Youngwood): 4,198

Configure basic horizontal bar chart
    [Tags]  HappyPath
    user clicks link  Manage data blocks
    user selects from list by label  id:selectedDataBlock  ${DATABLOCK_NAME}
    user waits until h2 is visible  ${DATABLOCK_NAME}
    user waits until page does not contain loading spinner
    user clicks link  Chart
    user waits until h3 is visible  Choose chart type
    user clicks button  Horizontal bar
    # Make sure chart has updated
    sleep  0.3s

Validate basic horizontal bar chart preview
    [Tags]  HappyPath
    ${preview}=  set variable  id:chartBuilderPreview
    user waits until element contains bar chart  ${preview}

    user checks chart title contains  ${preview}  Test chart title
    user checks chart legend item contains  ${preview}  1  Admission Numbers (Nailsea Youngwood)

    user checks chart x axis ticks  ${preview}  0     2500  5000  7500  10000
    user checks chart y axis ticks  ${preview}  2005  2010  2011  2012  2016

    user mouses over chart bar  ${preview}  1
    user checks chart tooltip label contains  ${preview}  2005
    user checks chart tooltip item contains  ${preview}  1  Admission Numbers (Nailsea Youngwood): 3,612

    user mouses over chart bar  ${preview}  2
    user checks chart tooltip label contains  ${preview}  2010
    user checks chart tooltip item contains  ${preview}  1  Admission Numbers (Nailsea Youngwood): 9,304

    user mouses over chart bar  ${preview}  3
    user checks chart tooltip label contains  ${preview}  2011
    user checks chart tooltip item contains  ${preview}  1  Admission Numbers (Nailsea Youngwood): 9,603

    user mouses over chart bar  ${preview}  4
    user checks chart tooltip label contains  ${preview}  2012
    user checks chart tooltip item contains  ${preview}  1  Admission Numbers (Nailsea Youngwood): 8,150

    user mouses over chart bar  ${preview}  5
    user checks chart tooltip label contains  ${preview}  2016
    user checks chart tooltip item contains  ${preview}  1  Admission Numbers (Nailsea Youngwood): 4,198

Save and validate horizontal bar chart embeds correctly
    [Tags]  HappyPath
    user clicks link  Chart configuration
    user clicks button  Save chart options
    user waits until button is enabled  Save chart options

    user clicks link  Manage content
    user waits until h2 is visible  ${PUBLICATION_NAME}
    user opens accordion section  ${CONTENT_SECTION_NAME}

    ${datablock}=  set variable  css:[data-testid="Data block - ${DATABLOCK_NAME}"]
    user waits until element contains bar chart  ${datablock}

    user checks chart title contains  ${datablock}  Test chart title
    user checks chart legend item contains  ${datablock}  1  Admission Numbers (Nailsea Youngwood)

    user checks chart x axis ticks  ${datablock}  0     2500  5000  7500  10000
    user checks chart y axis ticks  ${datablock}  2005  2010  2011  2012  2016

    user mouses over chart bar  ${datablock}  1
    user checks chart tooltip label contains  ${datablock}  2005
    user checks chart tooltip item contains  ${datablock}  1  Admission Numbers (Nailsea Youngwood): 3,612

    user mouses over chart bar  ${datablock}  2
    user checks chart tooltip label contains  ${datablock}  2010
    user checks chart tooltip item contains  ${datablock}  1  Admission Numbers (Nailsea Youngwood): 9,304

    user mouses over chart bar  ${datablock}  3
    user checks chart tooltip label contains  ${datablock}  2011
    user checks chart tooltip item contains  ${datablock}  1  Admission Numbers (Nailsea Youngwood): 9,603

    user mouses over chart bar  ${datablock}  4
    user checks chart tooltip label contains  ${datablock}  2012
    user checks chart tooltip item contains  ${datablock}  1  Admission Numbers (Nailsea Youngwood): 8,150

    user mouses over chart bar  ${datablock}  5
    user checks chart tooltip label contains  ${datablock}  2016
    user checks chart tooltip item contains  ${datablock}  1  Admission Numbers (Nailsea Youngwood): 4,198

Configure basic geographic chart
    [Tags]  HappyPath
    user clicks link  Manage data blocks
    user selects from list by label  id:selectedDataBlock  ${DATABLOCK_NAME}
    user waits until h2 is visible  ${DATABLOCK_NAME}
    user waits until page does not contain loading spinner
    user clicks link  Chart
    user waits until h3 is visible  Choose chart type
    user clicks button  Geographic

Validate basic geographic chart preview
    [Tags]  HappyPath
    user waits until element does not contain bar chart  id:chartBuilderPreview
    user waits until element contains map chart  id:chartBuilderPreview

    user selects from list by label  id:chartBuilderPreview-map-selectedLocation  Nailsea Youngwood

    user mouses over selected map feature  id:chartBuilderPreview
    user checks chart tooltip label contains  id:chartBuilderPreview  Nailsea Youngwood
    user checks chart tooltip item contains  id:chartBuilderPreview  1  Admission Numbers (2005): 3,612
    user checks chart tooltip item contains  id:chartBuilderPreview  2  Admission Numbers (2010): 9,304
    user checks chart tooltip item contains  id:chartBuilderPreview  3  Admission Numbers (2011): 9,603
    user checks chart tooltip item contains  id:chartBuilderPreview  4  Admission Numbers (2012): 8,150
    user checks chart tooltip item contains  id:chartBuilderPreview  5  Admission Numbers (2016): 4,198

    user checks map chart indicator tile contains  id:chartBuilderPreview  1  Admission Numbers (2005)  3,612
    user checks map chart indicator tile contains  id:chartBuilderPreview  2  Admission Numbers (2010)  9,304
    user checks map chart indicator tile contains  id:chartBuilderPreview  3  Admission Numbers (2011)  9,603
    user checks map chart indicator tile contains  id:chartBuilderPreview  4  Admission Numbers (2012)  8,150
    user checks map chart indicator tile contains  id:chartBuilderPreview  5  Admission Numbers (2016)  4,198

Save and validate geographic chart embeds correctly
    [Tags]  HappyPath
    user clicks link  Chart configuration
    user clicks button  Save chart options
    user waits until button is enabled  Save chart options

    user clicks link  Manage content
    user waits until h2 is visible  ${PUBLICATION_NAME}
    user opens accordion section  ${CONTENT_SECTION_NAME}

    ${datablock}=  set variable  css:[data-testid="Data block - ${DATABLOCK_NAME}"]
    user waits until element does not contain bar chart  ${datablock}
    user waits until element contains map chart  ${datablock}

    user selects from list by label  ${datablock} select[name="selectedLocation"]  Nailsea Youngwood

    user mouses over selected map feature  ${datablock}
    user checks chart tooltip label contains  ${datablock}  Nailsea Youngwood
    user checks chart tooltip item contains  ${datablock}  1  Admission Numbers (2005): 3,612
    user checks chart tooltip item contains  ${datablock}  2  Admission Numbers (2010): 9,304
    user checks chart tooltip item contains  ${datablock}  3  Admission Numbers (2011): 9,603
    user checks chart tooltip item contains  ${datablock}  4  Admission Numbers (2012): 8,150
    user checks chart tooltip item contains  ${datablock}  5  Admission Numbers (2016): 4,198

    user checks map chart indicator tile contains  ${datablock}  1  Admission Numbers (2005)  3,612
    user checks map chart indicator tile contains  ${datablock}  2  Admission Numbers (2010)  9,304
    user checks map chart indicator tile contains  ${datablock}  3  Admission Numbers (2011)  9,603
    user checks map chart indicator tile contains  ${datablock}  4  Admission Numbers (2012)  8,150
    user checks map chart indicator tile contains  ${datablock}  5  Admission Numbers (2016)  4,198

Configure basic infographic chart
    [Tags]  HappyPath
    user clicks link  Manage data blocks
    user selects from list by label  id:selectedDataBlock  ${DATABLOCK_NAME}
    user waits until h2 is visible  ${DATABLOCK_NAME}
    user waits until page does not contain loading spinner
    user clicks link  Chart
    user waits until h3 is visible  Choose chart type
    user clicks button  Choose an infographic as alternative
    user chooses file  id:chartConfigurationForm-file  ${CURDIR}${/}files${/}test-infographic.png

Validate basic infographic chart preview
    [Tags]  HappyPath
    user checks chart title contains  id:chartBuilderPreview  Test chart title
    user waits until element contains infographic chart  id:chartBuilderPreview
    user checks infographic chart contains alt  id:chartBuilderPreview  Test chart alt

Save and validate infographic chart embeds correctly
    [Tags]  HappyPath
    user clicks button  Save chart options
    user waits until button is enabled  Save chart options

    user clicks link  Manage content
    user waits until h2 is visible  ${PUBLICATION_NAME}
    user opens accordion section  ${CONTENT_SECTION_NAME}

    ${datablock}=  set variable  css:[data-testid="Data block - ${DATABLOCK_NAME}"]
    user checks chart title contains  ${datablock}  Test chart title
    user checks infographic chart contains alt  ${datablock}  Test chart alt

Delete embedded data block
    [Tags]  HappyPath
    user clicks button  Remove block
    user clicks button  Confirm

Delete chart from data block
    [Tags]  HappyPath
    user clicks link  Manage data blocks
    user selects from list by label  id:selectedDataBlock  ${DATABLOCK_NAME}
    user waits until h2 is visible  ${DATABLOCK_NAME}
    user waits until page does not contain loading spinner
    user clicks link  Chart
    user clicks button  Delete chart
    user clicks button  Confirm
    user waits until element does not contain infographic chart  id:chartBuilderPreview

Delete data block
    [Tags]  HappyPath
    user clicks button  Delete this data block
    user waits until page does not contain loading spinner
    user clicks button  Confirm
    user waits until h2 is visible  Create new data block
