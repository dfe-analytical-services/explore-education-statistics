*** Settings ***
Resource    ../libs/admin-common.robot

Force Tags  Admin  Local

Suite Setup       user signs in
Suite Teardown    user closes the browser

*** Test Cases ***
Create publication and release for upload data testing
    [Tags]  HappyPath
    user selects theme "Test theme" and topic "UI test topic %{RUN_IDENTIFIER}" from the admin dashboard
    # Create publication
    # Create release
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

Navigate to Manage data tab
    [Tags]  HappyPath
    user clicks element  xpath://li/a[text()="Manage data"]
    user waits until page contains element   xpath://legend[text()="Add new data to release"]
    user checks page does not contain element    xpath://*[@id="dataFileUploadForm"]/dl[1]
    user enters text into element   css:#dataFileUploadForm-subjectTitle   All Geographies
    choose file  css:#dataFileUploadForm-dataFile  ${CURDIR}${/}files${/}upload-file-test.csv
    choose file  css:#dataFileUploadForm-metadataFile  ${CURDIR}${/}files${/}upload-file-test.meta.csv
    user clicks button  Upload data files

# Upload data csvs
# Upload ancillary files

Verify file uploaded details
    [Tags]  HappyPath
    user waits until page contains element    xpath://*[@id="dataFileUploadForm"]/dl[1]
    data csv number contains xpath  1   //dt[text()="Subject title"]/../dd/h4[text()="All Geographies"]
    data csv number contains xpath  1   //dt[text()="Data file"]/../dd/a[text()="upload-file-test.csv"]
    data csv number contains xpath  1   //dt[text()="Filesize"]/../dd[.="15 Kb"]
    data csv number contains xpath  1   //dt[text()="Number of rows"]/../dd[text()="161"]
    data csv number contains xpath  1   //dt[text()="Metadata file"]/../dd/a[text()="upload-file-test.meta.csv"]
    data csv number contains xpath  1   //dt[text()="Uploaded by"]/../dd/a[text()="EESADMIN.User1@azurehiveitco.onmicrosoft.com"]
    ${date}=  get datetime  %d/%m/%Y
    data csv number contains xpath  1   //dt[text()="Date Uploaded"]/../dd[contains(text(), "${date}")]
    data csv number contains xpath  1   //dt[text()="Status"]/../dd//strong[text()="Complete"]
    data csv number contains xpath  1   //dt[text()="Actions"]/../dd/a[text()="Delete files"]
