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

Navigate to Manage data blocks tab
    [Tags]  HappyPath
    user clicks element  xpath://li/a[text()="Manage data blocks"]
    user waits until page contains element  css:#publicationSubjectForm

Select Subject "Absence in PRUs"
    [Tags]  HappyPath
    user selects radio    Absence in PRUs
    user clicks element   css:#publicationSubjectForm-submit
    user waits until page contains   Choose locations
    user checks previous table tool step contains  1   Subject   Absence in PRUs

Select Location LAs Barnet, Barnsley, and Bedford
    [Tags]  HappyPath
    user opens details dropdown  Local Authority
    user clicks checkbox   Barnet
    user clicks checkbox   Barnsley
    user clicks checkbox   Bedford
    user clicks element  css:#locationFiltersForm-submit
    user waits until page contains   Choose time period
    user checks previous table tool step contains  2  Local Authority   Barnet
    user checks previous table tool step contains  2  Local Authority   Barnsley
    user checks previous table tool step contains  2  Local Authority   Bedford

Select Time Period 2014/15 - 2014/15
    [Tags]  HappyPath
    user selects start date    2014/15
    user selects end date    2014/15
    user clicks element  css:#timePeriodForm-submit
    user waits until page contains   Choose your filters
    user checks previous table tool step contains  3   Start date   2014/15
    user checks previous table tool step contains  3   End date     2014/15

Select indicators
    [Tags]  HappyPath
    user clicks subheaded indicator checkbox  Absence fields   Authorised absence rate
    user clicks subheaded indicator checkbox  Absence fields   Overall absence rate
    user clicks subheaded indicator checkbox  Absence fields   Unauthorised absence rate

Create table
    [Tags]  HappyPath
    user clicks element   css:#filtersForm-submit
    user waits until results table appears

Validate table's column headings
    [Tags]  HappyPath
    user checks results table column heading contains  1  1  Pupil Referral Unit
    user checks results table column heading contains  2  1  2014/15

Validate table's row headings
    [Tags]  HappyPath
    user checks results table row heading contains   1   1   Barnet
    user checks results table row heading contains   1   2   Unauthorised absence rate
    user checks results table row heading contains   2   1   Authorised absence rate
    user checks results table row heading contains   3   1   Overall absence rate

    user checks results table row heading contains   4   1   Barnsley
    user checks results table row heading contains   4   2   Unauthorised absence rate
    user checks results table row heading contains   5   1   Authorised absence rate
    user checks results table row heading contains   6   1   Overall absence rate

    user checks results table row heading contains   7   1   Bedford
    user checks results table row heading contains   7   2   Unauthorised absence rate
    user checks results table row heading contains   8   1   Authorised absence rate
    user checks results table row heading contains   9   1   Overall absence rate

Validate table results
    [Tags]  HappyPath
    # Barnet
    user checks results table cell contains   1     1     13.2%
    user checks results table cell contains   2     1     26.9%
    user checks results table cell contains   3     1     40.1%

    # Barnsley
    user checks results table cell contains   4     1     9.4%
    user checks results table cell contains   5     1     18.3%
    user checks results table cell contains   6     1     27.8%

    # Bedford
    user checks results table cell contains   7     1     5.8%
    user checks results table cell contains   8     1     17.9%
    user checks results table cell contains   9     1     23.7%

Save data block
    [Tags]  HappyPath
    user enters text into element  css:#data-block-title       UI Test create data block title
    user enters text into element  css:#data-block-source      UI Test create data block source
    user enters text into element  css:#data-block-footnotes   UI Test create data block footnote
    user enters text into element  css:#data-block-name        UI Test create data block name
    user clicks button   Save data block
    user waits until page contains    The Data Block has been saved.

Refresh page, select new data block, verify selections
    [Tags]  HappyPath
    user reloads page
    user selects from list by label  css:#selectDataBlock   UI Test create data block title
    user waits until page contains element   xpath://h3[text()="Update data source"]
    user checks previous table tool step contains  1   Subject   Absence in PRUs
    user checks previous table tool step contains  2   Local Authority   Barnet
    user checks previous table tool step contains  2   Local Authority   Barnsley
    user checks previous table tool step contains  2   Local Authority   Bedford
    user checks previous table tool step contains  3   Start date    2014/15
    user checks previous table tool step contains  3   End date      2014/15
    user checks subheaded indicator checkbox is selected  Absence fields   Authorised absence rate
    user checks subheaded indicator checkbox is selected  Absence fields   Overall absence rate
    user checks subheaded indicator checkbox is selected  Absence fields   Unauthorised absence rate
    user checks category checkbox is selected   School type   Pupil Referral Unit

    user checks results table column heading contains  1  1  Pupil Referral Unit
    user checks results table column heading contains  2  1  2014/15
    user checks results table row heading contains   1   1   Barnet
    user checks results table row heading contains   1   2   Unauthorised absence rate
    user checks results table row heading contains   2   1   Authorised absence rate
    user checks results table row heading contains   3   1   Overall absence rate
    user checks results table cell contains   1     1     13.2%
    user checks results table cell contains   2     1     26.9%
    user checks results table cell contains   3     1     40.1%

Delete data block
    [Tags]  HappyPath
    user clicks button   Delete this data block
    user waits until page contains heading   Delete data block
    user clicks button   Confirm
    user waits until page does not contain element   xpath:h1[text()="Delete data block"]
    user checks list does not contain label   css:#selectDataBlock   UI Test create data block title