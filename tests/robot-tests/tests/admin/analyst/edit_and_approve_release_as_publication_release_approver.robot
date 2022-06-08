*** Settings ***
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot
Resource            ../../libs/admin/manage-content-common.robot
Resource            ../../libs/charts.robot
Resource            ../../libs/public-common.robot

Suite Setup         user signs in as analyst1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    AltersData


*** Variables ***
${PUBLICATION_NAME}=    UI tests - publication release approver %{RUN_IDENTIFIER}
${RELEASE_NAME}=        Financial Year 3000-01
${DATABLOCK_NAME}=      Dates data block name


*** Test Cases ***
Create new publication and release for "UI tests topic" topic and adds analyst1 as publication release approver
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user adds publication role to user via api
    ...    EES-test.ANALYST1@education.gov.uk
    ...    ${PUBLICATION_ID}
    ...    ReleaseApprover
    user create test release via api    ${PUBLICATION_ID}    FY    3000

Go to "Release summary" page
    user reloads page
    user navigates to editable release summary from admin dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME} (not Live)

Verify release summary
    user checks page contains element    xpath://li/a[text()="Summary" and contains(@aria-current, 'page')]
    user waits until h2 is visible    Release summary
    user checks summary list contains    Publication title    ${PUBLICATION_NAME}

Upload subject
    user uploads subject    Dates test subject    dates.csv    dates.meta.csv

Add data guidance
    user clicks link    Data guidance
    user waits until h2 is visible    Public data guidance
    user waits until page contains element    id:dataGuidanceForm-content
    user waits until page contains element    id:dataGuidance-dataFiles
    user enters text into element    id:dataGuidanceForm-content    Test data guidance content
    user enters text into data guidance data file content editor    Dates test subject
    ...    Dates test subject test data guidance content
    user clicks button    Save guidance

Add ancillary file
    user clicks link    Ancillary file uploads
    user waits until h2 is visible    Add file to release
    user enters text into element    label:Title    Test ancillary file 1
    user enters text into element    label:Summary    Test ancillary file 1 summary
    user chooses file    label:Upload file    ${FILES_DIR}test-file-1.txt
    user clicks button    Upload file
    user waits until page contains accordion section    Test ancillary file 1

Create data block table
    user creates data block for dates csv    Dates test subject    ${DATABLOCK_NAME}    Dates table title

Create chart for data block
    user waits until page contains link    Chart
    user waits until page does not contain loading spinner
    user clicks link    Chart

    user clicks button    Choose an infographic as alternative
    user chooses file    id:chartConfigurationForm-file    ${FILES_DIR}test-infographic.png
    user checks radio is checked    Use table title
    user enters text into element    id:chartConfigurationForm-alt    Sample alt text

    user clicks button    Save chart options

    user waits until page contains    Chart preview
    user checks infographic chart contains alt    id:chartBuilderPreview    Sample alt text

Create some release content
    user clicks link    Content
    user waits until h2 is visible    ${PUBLICATION_NAME}
    user waits until page contains button    Add a summary text block    %{WAIT_SMALL}

    user clicks button    Add new section
    user changes accordion section title    1    Dates data block

    user adds data block to editable accordion section    Dates data block    ${DATABLOCK_NAME}
    ...    css:#releaseMainContent
    ${datablock}=    set variable    xpath://*[@data-testid="Data block - ${DATABLOCK_NAME}"]
    user waits until page contains element    ${datablock}    %{WAIT_SMALL}
    user waits until element contains infographic chart    ${datablock}
    user checks chart title contains    ${datablock}    Dates table title
    user checks infographic chart contains alt    ${datablock}    Sample alt text

    user adds text block to editable accordion section    Dates data block    css:#releaseMainContent
    user adds content to autosaving accordion section text block    Dates data block    2    Some test text!
    ...    css:#releaseMainContent

Add public prerelease access list
    user clicks link    Pre-release access
    user creates public prerelease access list    Test public access list

Put release into higher level review
    user puts release into higher level review

Verify release status is in higher review
    user checks summary list contains    Current status    Awaiting higher review
    user checks summary list contains    Scheduled release    Not scheduled
    user checks summary list contains    Next release expected    Not set

Put release back into draft
    user puts release into draft

Approve release for scheduled release
    user approves release for scheduled release    2    12    3001

Verify release is scheduled
    user checks summary list contains    Current status    Approved
    user checks summary list contains    Scheduled release
    ...    ${PUBLISH_DATE_DAY} ${PUBLISH_DATE_MONTH_WORD} ${PUBLISH_DATE_YEAR}
    user checks summary list contains    Next release expected    December 3001

Put release back into draft again
    user puts release into draft    expected_next_release_date=December 3001

Approve release for immediate publication
    user approves original release for immediate publication
