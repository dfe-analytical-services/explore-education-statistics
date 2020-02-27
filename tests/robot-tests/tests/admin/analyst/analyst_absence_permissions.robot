*** Settings ***
Resource    ../../libs/admin-common.robot

Force Tags  Admin  Local  Dev

Suite Setup       user signs in as analyst1
Suite Teardown    user closes the browser

*** Test Cases ***
Validate Analyst1 can see correct themes and topics
    [Tags]  HappyPath
    user checks list contains x elements   css:#selectTheme   1
    user checks list contains label   css:#selectTheme   Pupils and schools
    user checks list contains x elements   css:#selectTopic   2
    user checks list contains label   css:#selectTopic   Exclusions
    user checks list contains label   css:#selectTopic   Pupil absence

Validate Analyst1 can see correct draft releases
    [Tags]  HappyPath
    user checks element should contain   css:#draft-releases-tab   View draft releases (2)
    user clicks element   css:#draft-releases-tab
    user checks draft releases tab contains publication  Pupil absence in schools in England
    user checks draft releases tab publication has release   Pupil absence in schools in England    Academic Year, 2016 to 2017 (Live - Latest release)
    user checks draft releases tab contains publication  Permanent and fixed-period exclusions in England
    user checks draft releases tab publication has release   Permanent and fixed-period exclusions in England    Academic Year, 2016 to 2017 (Live - Latest release)

Validate Analyst1 can see correct scheduled releases
    [Tags]  HappyPath
    user checks element should contain   css:#scheduled-releases-tab   View scheduled releases (0)
    user clicks element   css:#scheduled-releases-tab
    user checks element contains  css:#scheduled-releases   There are currently no scheduled releases

Validate Analyst1 cannot create a publication for Pupils and schools theme
    [Tags]  HappyPath
    user selects theme "Pupils and schools" and topic "Pupil absence" from the admin dashboard
    user selects from list by label   css:#selectTheme   Pupils and schools
    user waits until page contains accordion section  Pupil absence in schools in England
    user checks page does not contain element   xpath://a[text()="Create new publication"]

Validate Analyst1 cannot create a release for Pupil absence topic
    [Tags]  HappyPath
    user opens accordion section  Pupil absence in schools in England
    user checks page does not contain element   xpath://*[data-testid="Create new release link for Pupil absence in schools in England"]

Validate Analyst1 can see Absence release summary
    [Tags]  HappyPath
    user opens details dropdown  Academic Year, 2016 to 2017 (Live - Latest release)
    user clicks element  css:[data-testid="Edit release link for Pupil absence in schools in England, Academic Year, 2016 to 2017 (Live - Latest release)"]
    user waits until page contains heading  Pupil absence in schools in England
    user checks summary list item "Publication title" should be "Pupil absence in schools in England"
    user checks summary list item "Time period" should be "Academic Year"
    user checks summary list item "Release period" should be "2016 to 2017"
    user checks summary list item "Lead statistician" should be "Mark Pearson"
    user checks summary list item "Scheduled release" should be ""
    user checks summary list item "Next release expected" should be ""
    user checks summary list item "Release type" should be "Official Statistics"

Validate Analyst1 can see "Upload data files" button
    [Tags]  HappyPath
    user clicks element   xpath://a[text()="Manage data"]
    user waits until page contains element  xpath://legend[text()="Add new data to release"]
    user checks page contains element   css:#upload-data-files-button

Validate Analyst1 can see "Add another footnote" button
    [Tags]  HappyPath
    user clicks element  css:#footnotes-tab
    user waits until page contains element   xpath://h2[text()="Footnotes"]
    user waits until page contains element   css:#add-footnote-button

Validate Analyst1 can see "Upload file" button for ancillary files
    [Tags]  HappyPath
    user clicks element   css:#file-upload-tab
    user waits until page contains element   xpath://legend[text()="Upload file"]
    user checks page contains element   css:#upload-file-button

Validate Analyst1 can see subjects for Absence 2016/17 release
    [Tags]  HappyPath
    user clicks element   xpath://a[text()="Manage data blocks"]
    user waits until page contains element   xpath://h2[text()="Create new data block"]
    user checks element count is x   xpath://*[@id="publicationSubjectForm-subjectId"]//*[@class="govuk-radios__item"]    7

Validate Analyst1 can see Manage content page contents
    [Tags]  HappyPath
    user clicks element   xpath://a[text()="Manage content"]
    user waits until page contains element   xpath://h1[text()="Pupil absence in schools in England"]/span[text()="Academic Year 2016/17"]

    user waits until page contains element   css:#pageMode
    user checks page contains element  css:#pageMode-edit[checked]
    user checks page does not contain element   css:#pageMode-preview[checked]

    user waits until page contains key stat tile   Overall absence rate        4.6%
    user waits until page contains key stat tile   Authorised absence rate     3.5%
    user waits until page contains key stat tile   Unauthorised absence rate   1.1%
    user checks element count is x    css:[data-testid="key-stat-tile"]   3

    user checks element count is x   xpath://*[@class="govuk-accordion__section"]   11
    user checks accordion is in position  About these statistics            1
    user checks accordion is in position  Pupil absence rates               2
    user checks accordion is in position  Persistent absence                3
    user checks accordion is in position  Reasons for absence               4
    user checks accordion is in position  Distribution of absence           5
    user checks accordion is in position  Absence by pupil characteristics  6
    user checks accordion is in position  Absence for 4-year-olds           7
    user checks accordion is in position  Pupil referral unit absence       8
    user checks accordion is in position  Regional and local authority (LA) breakdown  9
    user checks accordion is in position  Methodology                       10
    user checks accordion is in position  Contact us                        11

Validate Analyst1 cannot Approve a Pupil absence in schools in England release
    [Tags]  HappyPath
    user clicks element   xpath://a[text()="Release status"]
    user waits until page contains element  xpath://h2[text()="Release Status"]
    user clicks element   xpath://button[text()="Update release status"]
    user waits until page contains element   css:#releaseStatusForm
    user checks page contains element   xpath://*[@id="releaseStatusForm-releaseStatus-draft" and not(@disabled)]
    user checks page contains element   xpath://*[@id="releaseStatusForm-releaseStatus-higher-level-review" and not(@disabled)]
    user checks page contains element   xpath://*[@id="releaseStatusForm-releaseStatus-approved" and @disabled]
