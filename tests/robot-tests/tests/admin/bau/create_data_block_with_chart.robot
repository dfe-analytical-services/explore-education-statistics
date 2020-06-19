*** Settings ***
Resource    ../../libs/admin-common.robot
Library  Collections

Force Tags  Admin  Local  Dev  AltersData

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Test Cases ***
Create Datablock test publication
    [Tags]  HappyPath
    environment variable should be set   RUN_IDENTIFIER
    user selects theme "Test theme" and topic "UI test topic %{RUN_IDENTIFIER}" from the admin dashboard
    user waits until page contains element    xpath://a[text()="Create new publication"]     60
    user clicks element   xpath://a[text()="Create new publication"]
    user creates publication  Datablock test %{RUN_IDENTIFIER}   Test methodology    Sean Gibson - (Special educational needs statistics team)

Verify Datablock test publication is created
    [Tags]  HappyPath
    user checks page contains accordion  Datablock test %{RUN_IDENTIFIER}
    user opens accordion section  Datablock test %{RUN_IDENTIFIER}
    user checks accordion section contains text  Datablock test %{RUN_IDENTIFIER}    Methodology
    user checks accordion section contains text  Datablock test %{RUN_IDENTIFIER}    Releases

Create release
    [Tags]  HappyPath
    user clicks element  css:[data-testid="Create new release link for Datablock test %{RUN_IDENTIFIER}"]
    user creates a new release for publication "Datablock test %{RUN_IDENTIFIER}" for start year "2025"
    user checks summary list item "Publication title" should be "Datablock test %{RUN_IDENTIFIER}"

Upload subject
    [Tags]  HappyPath
    user waits until page contains element    xpath://h2[text()="Release summary"]
    user checks summary list item "Publication title" should be "Datablock test %{RUN_IDENTIFIER}"
    user clicks element  xpath://li/a[text()="Manage data"]
    user enters text into element  css:#dataFileUploadForm-subjectTitle   UI test subject
    choose file   css:#dataFileUploadForm-dataFile       ${CURDIR}${/}files${/}upload-file-test.csv
    choose file   css:#dataFileUploadForm-metadataFile   ${CURDIR}${/}files${/}upload-file-test.meta.csv
    user clicks element   xpath://button[text()="Upload data files"]

    user waits until page contains element   xpath://h2[text()="Uploaded data files"]
    user waits until page contains accordion section   UI test subject
    user opens accordion section   UI test subject
    user checks page contains element   xpath://dt[text()="Subject title"]/../dd/h4[text()="UI test subject"]
    user waits until page contains element  xpath://dt[text()="Status"]/../dd//strong[text()="Complete"]     180

Navigate to Manage data blocks tab
    [Tags]  HappyPath
    scroll element into view  xpath://li/a[text()="Manage data blocks"]
    user clicks element  xpath://li/a[text()="Manage data blocks"]
    user waits until page contains element   xpath://h2[text()="Choose a subject"]

Select subject "UI test subject"
    [Tags]  HappyPath
    user waits until page contains   UI test subject
    user selects radio    UI test subject
    user clicks element   css:#publicationSubjectForm-submit
    user waits until element is visible  xpath://h2[text()="Choose locations"]     90
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
    user clicks element     css:#locationFiltersForm-submit
    user waits until element is visible  xpath://h2[text()="Choose time period"]   90

Select time period
    [Tags]   HappyPath
    ${timePeriodStartList}=   get list items  css:#timePeriodForm-start
    ${timePeriodEndList}=   get list items  css:#timePeriodForm-end
    ${expectedList}=   create list   Please select  2005  2007  2008  2009  2010  2011  2012  2016  2017  2018  2019  2020
    lists should be equal  ${timePeriodStartList}   ${expectedList}
    lists should be equal  ${timePeriodEndList}   ${expectedList}

    user selects start date    2005
    user selects end date      2020
    user clicks element     css:#timePeriodForm-submit
    user waits until element is visible  xpath://h2[text()="Choose your filters"]
    user checks previous table tool step contains  3    Start date    2005
    user checks previous table tool step contains  3    End date      2020

Select indicators
    [Tags]  HappyPath
    user clicks indicator checkbox    Admission Numbers

Create table
    [Tags]  HappyPath
    [Documentation]   EES-615
    user clicks element   css:#filtersForm-submit
    user waits until results table appears     180
    user waits until page contains element   xpath://*[@id="dataTableCaption" and text()="Table showing Admission Numbers for 'UI test subject' from 'Datablock test %{RUN_IDENTIFIER}' in Bolton 001 (E02000984), Bolton 001 (E05000364), Bolton 004 (E02000987), Bolton 004 (E05010450), Nailsea Youngwood and Syon between 2005 and 2020"]

Validate table column headings
    [Tags]  HappyPath
    user checks results table column heading contains  css:table  1  1  Admission Numbers

Validate table rows for Bolton 001 (E02000984)
    [Tags]  HappyPath
    ${row}=  user gets row number with heading  Bolton 001 (E02000984)
    user checks results table heading in offset row contains  ${row}  0  2  2019

    user checks results table cell in offset row contains  ${row}  0  1  8,533

Validate table rows for Bolton 001 (E05000364)
    [Tags]  HappyPath
    ${row}=  user gets row number with heading   Bolton 001 (E05000364)
    user checks results table heading in offset row contains  ${row}  0  2  2009
    user checks results table heading in offset row contains  ${row}  1  1  2010
    user checks results table heading in offset row contains  ${row}  2  1  2017

    user checks results table cell in offset row contains  ${row}  0  1  5,815
    user checks results table cell in offset row contains  ${row}  1  1  5,595
    user checks results table cell in offset row contains  ${row}  2  1  6,373

Validate table rows for Bolton 004 (E02000987)
    [Tags]  HappyPath
    ${row}=  user gets row number with heading   Bolton 004 (E02000987)
    user checks results table heading in offset row contains  ${row}  0  2  2020

    user checks results table cell in offset row contains  ${row}  0  1  6,031

Validate table rows for Bolton 004 (E05010450)
    [Tags]  HappyPath
    ${row}=  user gets row number with heading   Bolton 004 (E05010450)
    user checks results table heading in offset row contains  ${row}  0  2  2005
    user checks results table heading in offset row contains  ${row}  1  1  2017
    user checks results table heading in offset row contains  ${row}  2  1  2018

    user checks results table cell in offset row contains  ${row}  0  1  8,557
    user checks results table cell in offset row contains  ${row}  1  1  3,481
    user checks results table cell in offset row contains  ${row}  2  1  8,630

Validate table rows for Nailsea Youngwood
    [Tags]  HappyPath
    ${row}=  user gets row number with heading   Nailsea Youngwood
    user checks results table heading in offset row contains  ${row}  0  2  2005
    user checks results table heading in offset row contains  ${row}  1  1  2010
    user checks results table heading in offset row contains  ${row}  2  1  2011
    user checks results table heading in offset row contains  ${row}  3  1  2012
    user checks results table heading in offset row contains  ${row}  4  1  2016

    user checks results table cell in offset row contains  ${row}  0  1  3,612
    user checks results table cell in offset row contains  ${row}  1  1  9,304
    user checks results table cell in offset row contains  ${row}  2  1  9,603
    user checks results table cell in offset row contains  ${row}  3  1  8,150
    user checks results table cell in offset row contains  ${row}  4  1  4,198

Validate table rows for Syon
    [Tags]  HappyPath
    ${row}=  user gets row number with heading   Syon
    user checks results table heading in offset row contains  ${row}  0  2  2007
    user checks results table heading in offset row contains  ${row}  1  1  2008
    user checks results table heading in offset row contains  ${row}  2  1  2010
    user checks results table heading in offset row contains  ${row}  3  1  2012
    user checks results table heading in offset row contains  ${row}  4  1  2017

    user checks results table cell in offset row contains  ${row}  0  1  9,914
    user checks results table cell in offset row contains  ${row}  1  1  5,505
    user checks results table cell in offset row contains  ${row}  2  1  6,060
    user checks results table cell in offset row contains  ${row}  3  1  1,109
    user checks results table cell in offset row contains  ${row}  4  1  1,959

Save data block
    [Tags]  HappyPath
    user enters text into element  css:#data-block-name        UI Test data block name
    user clears element text   css:#data-block-title
    user enters text into element  css:#data-block-title       UI Test table title
    user enters text into element  css:#data-block-source      UI Test source
    user clicks button   Save data block
    user waits until page contains    Delete this data block

Navigate to Create chart tab
    [Tags]  HappyPath   UnderConstruction
    sleep  1000000
    user clicks element   xpath://a[text()="Configure content"]
    user clicks element   xpath://a[text()="Create chart"]
    user clicks element   css:[class^="graph-builder_chartContainer"] button:nth-child(1)  # Line chart button
    sleep   1000000

