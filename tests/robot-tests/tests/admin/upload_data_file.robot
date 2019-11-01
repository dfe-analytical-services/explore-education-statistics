*** Settings ***
Resource    ../libs/admin-common.robot

Force Tags  Admin  Local

Suite Setup       user signs in
Suite Teardown    user closes the browser

*** Test Cases ***
Verify correct data is shown when theme and topic is shown
    [Tags]  HappyPath
    user selects theme "Test theme" and topic "UI tests topic" from the admin dashboard
    user checks page contains accordion  UI tests - upload
    user opens accordion section  UI tests - upload
    user checks accordion section contains text  UI tests - upload    Methodology
    user checks accordion section contains text  UI tests - upload    Releases

User clicks edit release
    [Tags]  HappyPath
    user checks page contains details section  Tax Year, 2030 to 2031 (not Live)
    user opens details section  Tax Year, 2030 to 2031 (not Live)
    user waits until page contains element  css:[data-testid="Edit release link for UI tests - upload, Tax Year, 2030 to 2031 (not Live)"]
    user clicks element  css:[data-testid="Edit release link for UI tests - upload, Tax Year, 2030 to 2031 (not Live)"]

Validate release summary tab has correct details
    [Tags]  HappyPath
    user waits until page contains element    xpath://h2[text()="Release summary"]
    user checks summary list item "Publication title" should be "UI tests - upload"
    user checks summary list item "Time period" should be "Tax Year"
    user checks summary list item "Release period" should be "2030 to 2031"
    user checks summary list item "Lead statistician" should be "Mark Pearson"
    user checks summary list item "Scheduled release" should be "01 January 2030"
    user checks summary list item "Next release expected" should be "01 January 2031"
    user checks summary list item "Release type" should be "Ad Hoc"

Navigate to Manage data tab, File uploads
    [Tags]  HappyPath
    user clicks element  xpath://li/a[text()="Manage data"]
    user waits until page contains element   xpath://legend[text()="Add new data to release"]
    user clicks element   css:#file-upload-tab
    user waits until page contains element   xpath://legend[text()="Upload file"]
    user checks page does not contain element    xpath://*[@id="fileUploadForm"]/dl[1]
    user enters text into element   css:#fileUploadForm-name   DfE logo
    choose file  css:#fileUploadForm-file  ${CURDIR}${/}files${/}dfe-logo.jpg
    user clicks button  Upload file

Verify file uploaded details
    [Tags]  HappyPath
    user waits until page contains element    xpath://*[@id="fileUploadForm"]/dl[1]
    data file number contains xpath  1   //dt[text()="Name"]/../dd[text()="DfE logo"]
    data file number contains xpath  1   //dt[text()="File"]/../dd/a[text()="dfe-logo.jpg"]
    data file number contains xpath  1   //dt[text()="Filesize"]/../dd[.="5 Kb"]
    #data file number contains xpath  1   //dt[text()="Uploaded by"]/../dd/a[text()="EESADMIN.User1@azurehiveitco.onmicrosoft.com"]
    #${date}=  get datetime  %d/%m/%Y
    #data file number contains xpath  1   //dt[text()="Date Uploaded"]/../dd[contains(text(), "${date}")]
    #data file number contains xpath  1   //dt[text()="Status"]/../dd//strong[text()="Complete"]
    data file number contains xpath  1   //dt[text()="Actions"]/../dd/a[text()="Delete file"]

