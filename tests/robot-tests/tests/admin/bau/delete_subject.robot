*** Settings ***
Resource    ../../libs/admin-common.robot

Force Tags  Admin  Local  Dev  AltersData

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Test Cases ***
Go to Create publication page for "UI tests topic" topic
    [Tags]  HappyPath
    environment variable should be set   RUN_IDENTIFIER
    user selects theme "Test theme" and topic "UI test topic %{RUN_IDENTIFIER}" from the admin dashboard
    user waits until page contains link    Create new publication
    user checks page does not contain button   UI tests - delete subject %{RUN_IDENTIFIER}
    user clicks link  Create new publication
    user waits until page contains heading    Create new publication

Enter new publication title
    [Tags]  HappyPath
    user enters text into element  id:createPublicationForm-publicationTitle   UI tests - delete subject %{RUN_IDENTIFIER}
    user checks element is not visible  id:createPublicationForm-publicationTitle-error

Select an existing methodology
    [Tags]  HappyPath
    user waits until page contains element   xpath://label[text()="Choose an existing methodology"]
    user clicks element          xpath://label[text()="Select a methodology later"]

Select contact "Tingting Shu"
    [Tags]  HappyPath
    user selects from list by label  id:createPublicationForm-selectedContactId   Tingting Shu - (Attainment statistics team)
    user checks summary list item "Team" should be "Attainment statistics team"
    user checks summary list item "Name" should be "Tingting Shu"
    user checks summary list item "Email" should be "Attainment.STATISTICS@education.gov.uk"
    user checks summary list item "Telephone" should be "0370 000 2288"

User redirects to the dashboard when clicking the Create publication button
    [Tags]  HappyPath
    user clicks button   Create publication
    user waits until page contains element   xpath://span[text()="Welcome"]

Verify that new publication has been created
    [Tags]  HappyPath
    user selects theme "Test theme" and topic "UI test topic %{RUN_IDENTIFIER}" from the admin dashboard
    user waits until page contains element   xpath://button[text()="UI tests - delete subject %{RUN_IDENTIFIER}"]
    user checks page contains accordion   UI tests - delete subject %{RUN_IDENTIFIER}
    user opens accordion section  UI tests - delete subject %{RUN_IDENTIFIER}
    user checks summary list item "Methodology" should be "No methodology assigned"
    user checks summary list item "Releases" should be "No releases created"

Create new release
    [Tags]   HappyPath
    user clicks element  css:[data-testid="Create new release link for UI tests - delete subject %{RUN_IDENTIFIER}"]
    user waits until page contains heading  Create new release

User fills in form
    [Tags]  HappyPath
    user waits until page contains element  id:releaseSummaryForm-timePeriodCoverage
    user selects from list by label  id:releaseSummaryForm-timePeriodCoverage  Tax Year
    user enters text into element  id:releaseSummaryForm-timePeriodCoverageStartYear  2020

    user clicks element   css:[data-testid="Ad Hoc"]

Click Create new release button
    [Tags]   HappyPath
    user clicks button   Create new release
    user waits until page contains element  xpath://h1/span[text()="Edit release"]
    user waits until page contains heading 1  UI tests - delete subject %{RUN_IDENTIFIER}

Verify Release summary
    [Tags]  HappyPath
    user checks page contains element   xpath://li/a[text()="Release summary" and contains(@aria-current, 'page')]
    user waits until page contains heading 2    Release summary
    user checks summary list item "Publication title" should be "UI tests - delete subject %{RUN_IDENTIFIER}"
    user checks summary list item "Time period" should be "Tax Year"
    user checks summary list item "Release period" should be "2020-21"
    user checks summary list item "Lead statistician" should be "Tingting Shu"
    user checks summary list item "Scheduled release" should be "Not scheduled"
    user checks summary list item "Next release expected" should be "Not set"
    user checks summary list item "Release type" should be "Ad Hoc"

Upload subject
    [Tags]  HappyPath
    user clicks element  xpath://li/a[text()="Manage data"]

    user enters text into element  id:dataFileUploadForm-subjectTitle   UI test subject
    choose file   id:dataFileUploadForm-dataFile       ${CURDIR}${/}files${/}upload-file-test-with-filter.csv
    choose file   id:dataFileUploadForm-metadataFile   ${CURDIR}${/}files${/}upload-file-test-with-filter.meta.csv
    user clicks button  Upload data files

    user waits until page contains element   xpath://h2[text()="Uploaded data files"]
    user waits until page contains accordion section   UI test subject
    user opens accordion section   UI test subject
    user checks page contains element   xpath://dt[text()="Subject title"]/../dd/h4[text()="UI test subject"]
    user waits until page contains element  xpath://dt[text()="Status"]/../dd//strong[text()="Complete"]     180

Create subject footnote for new subject
    [Tags]  HappyPath
    user clicks element  id:footnotes-tab
    user waits until page contains heading 2  Footnotes
    user waits until page contains element   id:add-footnote-button

    user clicks element   id:add-footnote-button
    user waits until page contains element   xpath://fieldset[@datatest-id="footnote-subject UI test subject"]
    user clicks element  xpath://fieldset[@datatest-id="footnote-subject UI test subject"]//label[text()="Select all indicators and filters for this subject"]/../input

    user clicks element   id:create-footnote-form-content
    user presses keys  UI tests subject footnote

    user clicks button   Create footnote

Create indicator footnote for new subject
    [Tags]  HappyPath
    user waits until page contains element   id:add-footnote-button

    user clicks element   id:add-footnote-button
    user waits until page contains element   xpath://fieldset[@datatest-id="footnote-subject UI test subject"]
    user opens details dropdown  Indicators
    user clicks checkbox  Admission Numbers

    user clicks element   id:create-footnote-form-content
    user presses keys  UI tests indicator Admission Numbers footnote

    user clicks button   Create footnote

Create Random Filter Total footnote for new subject
    [Tags]  HappyPath
    user waits until page contains element   id:add-footnote-button

    user clicks element   id:add-footnote-button
    user waits until page contains element   xpath://fieldset[@datatest-id="footnote-subject UI test subject"]

    user opens details dropdown   Random Filter
    user clicks checkbox   Total

    user clicks element   id:create-footnote-form-content
    user presses keys  UI tests Random Filter Total footnote

    user clicks button   Create footnote

Create Random Filter Select all footnote for new subject
    [Tags]  HappyPath
    user waits until page contains element   id:add-footnote-button

    user clicks element   id:add-footnote-button
    user waits until page contains element   xpath://fieldset[@datatest-id="footnote-subject UI test subject"]

    user opens details dropdown   Random Filter
    user clicks checkbox   Select all

    user clicks element   id:create-footnote-form-content
    user presses keys  UI tests Random Filter Select all footnote

    user clicks button   Create footnote

Delete UI test subject
    [Tags]  HappyPath
    user clicks element  id:data-upload-tab
    user waits until page contains element  xpath://legend[text()="Add new data to release"]

    user waits until page contains accordion section  UI test subject
    user clicks button   Delete files

    user waits until page contains heading   Confirm deletion of selected data files
    user checks page contains element  xpath://p[text()="4 footnotes will be removed or updated."]

    user clicks button  Confirm
    user waits until page does not contain accordion section   UI test subject
