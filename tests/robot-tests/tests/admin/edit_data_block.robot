*** Settings ***
Resource    ../libs/admin-common.robot

Force Tags  Admin  Local

Suite Setup       user signs in
Suite Teardown    user closes the browser

*** Test Cases ***
Verify correct data is shown when theme and topic is shown
    [Tags]  HappyPath
    user selects theme "Test theme" and topic "UI tests topic" from the admin dashboard
    user checks page contains accordion  UI tests - data block
    user opens accordion section  UI tests - data block
    user checks accordion section contains text  UI tests - data block    Methodology
    user checks accordion section contains text  UI tests - data block    Releases

User clicks edit release
    [Tags]  HappyPath
    user checks page contains details section  Financial Year, 2030 to 2031 (not Live)
    user opens details section  Financial Year, 2030 to 2031 (not Live)
    user waits until page contains element  css:[data-testid="Edit release link for UI tests - data block, Financial Year, 2030 to 2031 (not Live)"]
    user clicks element  css:[data-testid="Edit release link for UI tests - data block, Financial Year, 2030 to 2031 (not Live)"]

Validate release summary tab has correct details
    [Tags]  HappyPath
    user waits until page contains element    xpath://h2[text()="Release summary"]
    user checks summary list item "Publication title" should be "UI tests - data block"
    user checks summary list item "Time period" should be "Financial Year"
    user checks summary list item "Release period" should be "2030 to 2031"
    user checks summary list item "Lead statistician" should be "Mark Pearson"
    user checks summary list item "Scheduled release" should be "03 March 2030"
    user checks summary list item "Next release expected" should be "04 April 2031"
    user checks summary list item "Release type" should be "Ad Hoc"

Navigate to Manage data tab, check subjects are there
    [Tags]  HappyPath
    user clicks element  xpath://li/a[text()="Manage data"]
    user waits until page contains element   xpath://legend[text()="Add new data to release"]
    data csv number contains xpath  1   //dt[text()="Subject title"]/../dd/h4[text()="Absence in PRUs"]
    data csv number contains xpath  2   //dt[text()="Subject title"]/../dd/h4[text()="Absence rate percent bands"]

Navigate to Manage data blocks tab, check for data block
    [Tags]  HappyPath
    user clicks element  xpath://li/a[text()="Manage data blocks"]
    user waits until page contains element  css:#publicationSubjectForm

Select 'UI Test edit data block title' data block
    [Tags]  HappyPath
    user checks list contains label  css:#selectDataBlock  UI Test edit data block title
    user selects from list by label  css:#selectDataBlock  UI Test edit data block title
    user waits until page contains element   xpath://h3[text()="Update data source"]
    user checks previous table tool step contains   1    Subject    Absence rate percent bands
    user checks previous table tool step contains   2    National   England
    user checks previous table tool step contains   3    Start date   2012/13
    user checks previous table tool step contains   3    End date     2014/15
    user checks indicator checkbox is selected    Percentage of all pupil enrolments that fall into that per cent band for authorised absence
    user checks indicator checkbox is selected    Percentage of all pupil enrolments that fall into that per cent band for overall absence
    user checks indicator checkbox is selected    Percentage of all pupil enrolments that fall into that per cent band for unauthorised absence
    user checks category checkbox is selected  School type               Total
    user checks category checkbox is selected  Absence percentage band   Total
    user checks element attribute value should be   css:#data-block-title   value  UI Test edit data block title
    user checks element attribute value should be   css:#data-block-source   value  UI Test edit data block source
    user checks element attribute value should be   css:#data-block-footnotes   value   UI Test edit data block footnote
    user checks element attribute value should be   css:#data-block-name   value   UI Test edit data block name

Validate table
    [Tags]  HappyPath
    user checks results table column heading contains  1  1  2012/13
    user checks results table column heading contains  1  2  2013/14
    user checks results table column heading contains  1  3  2014/15
    user checks results table row heading contains   1   1   England
    user checks results table row heading contains   1   2   Percentage of all pupil enrolments that fall into that per cent band for overall absence
    user checks results table row heading contains   2   1   Percentage of all pupil enrolments that fall into that per cent band for authorised absence
    user checks results table row heading contains   3   1   Percentage of all pupil enrolments that fall into that per cent band for unauthorised absence
    user checks results table cell contains   1     1     100%
    user checks results table cell contains   1     2     100%
    user checks results table cell contains   1     3     100%
    user checks results table cell contains   2     1     100%
    user checks results table cell contains   2     2     100%
    user checks results table cell contains   2     3     100%
    user checks results table cell contains   3     1     100%
    user checks results table cell contains   3     2     100%
    user checks results table cell contains   3     3     100%

Update indicators
    [Tags]  HappyPath
    user waits until page contains element   xpath://legend[text()="Indicators"]
    user clicks indicator checkbox  Number of all pupil enrolments that fall into that per cent band for authorised absence
    user clicks indicator checkbox  Number of all pupil enrolments that fall into that per cent band for overall absence
    user clicks indicator checkbox  Number of all pupil enrolments that fall into that per cent band for unauthorised absence
    user clicks indicator checkbox  Percentage of all pupil enrolments that fall into that per cent band for authorised absence
    user clicks indicator checkbox  Percentage of all pupil enrolments that fall into that per cent band for overall absence
    user clicks indicator checkbox  Percentage of all pupil enrolments that fall into that per cent band for unauthorised absence
    user clicks element    css:#filtersForm-submit
    user waits until page does not contain element   [@id="filtersForm-submit and @disabled]

Validate updated table is correct
    [Tags]   HappyPath
    user checks results table column heading contains  1  1  2012/13
    user checks results table column heading contains  1  2  2013/14
    user checks results table column heading contains  1  3  2014/15
    user checks results table row heading contains   1   1   England
    user checks results table row heading contains   1   2   Number of all pupil enrolments that fall into that per cent band for overall absence
    user checks results table row heading contains   2   1   Number of all pupil enrolments that fall into that per cent band for authorised absence
    user checks results table row heading contains   3   1   Number of all pupil enrolments that fall into that per cent band for unauthorised absence
    user checks results table cell contains   1     1     6,477,725
    user checks results table cell contains   1     2     6,554,005
    user checks results table cell contains   1     3     6,642,755
    user checks results table cell contains   2     1     6,477,725
    user checks results table cell contains   2     2     6,554,005
    user checks results table cell contains   2     3     6,642,755
    user checks results table cell contains   3     1     6,477,725
    user checks results table cell contains   3     2     6,554,005
    user checks results table cell contains   3     3     6,642,755

Update data block details
    [Tags]   HappyPath
    clear element text    css:#data-block-title
    user enters text into element  css:#data-block-title    UI Test edit data block title updated
    clear element text    css:#data-block-source
    user enters text into element  css:#data-block-source    UI Test edit data block source updated
    clear element text    css:#data-block-footnotes
    user enters text into element  css:#data-block-footnotes    UI Test edit data block footnote updated
    clear element text    css:#data-block-name
    user enters text into element  css:#data-block-name    UI Test edit data block name updated
    user clicks button   Update data block
    user waits until page contains   The Data Block has been updated.

Select Create new data block, and then reselect updated data block
    [Documentation]  EES-568
    [Tags]  HappyPath   Failing
    user selects from list by label    css:#selectDataBlock   Create new data block
    user waits until page contains element   xpath://h2[text()="Create new data block"]
    user checks list does not contain label   css:#selectDataBlock   UI Test edit data block title
    user checks list contains label   css:#selectDataBlock   UI Test edit data block title updated
    user selects from list by label   css:#selectDataBlock   UI Test edit data block title updated
    user waits until page contains element   xpath://h3[text()="Update data source"]

Validate updated data blocks selections
    [Tags]  HappyPath   Failing
    user checks previous table tool step contains   1    Subject    Absence rate percent bands
    user checks previous table tool step contains   2    National   England
    user checks previous table tool step contains   3    Start date   2012/13
    user checks previous table tool step contains   3    End date     2014/15
    user checks indicator checkbox is selected    Number of all pupil enrolments that fall into that per cent band for authorised absence
    user checks indicator checkbox is selected    Number of all pupil enrolments that fall into that per cent band for overall absence
    user checks indicator checkbox is selected    Number of all pupil enrolments that fall into that per cent band for unauthorised absence
    user checks category checkbox is selected    School type               Total
    user checks category checkbox is selected    Absence percentage band   Total
    user checks element attribute value should be   css:#data-block-title      value  UI Test edit data block title updated
    user checks element attribute value should be   css:#data-block-source     value  UI Test edit data block source updated
    user checks element attribute value should be   css:#data-block-footnotes  value  UI Test edit data block footnote updated
    user checks element attribute value should be   css:#data-block-name       value  UI Test edit data block name updated