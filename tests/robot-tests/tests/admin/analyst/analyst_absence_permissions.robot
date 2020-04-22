*** Settings ***
Resource    ../../libs/admin-common.robot

Force Tags  Admin  Local  Dev

Suite Setup       user signs in as analyst1
Suite Teardown    user closes the browser

*** Test Cases ***
Validate Analyst1 can see correct themes and topics
    [Tags]  HappyPath
    user selects theme "Pupils and schools" and topic "Pupil absence" from the admin dashboard
    user waits until page contains accordion section  Pupil absence in schools in England     60

    user checks list contains label   css:#selectTheme   Pupils and schools
    user checks list contains x elements   css:#selectTopic   2
    user checks list contains label   css:#selectTopic   Exclusions
    user checks list contains label   css:#selectTopic   Pupil absence

Validate Analyst1 can see correct draft releases
    [Tags]  HappyPath
    user checks element should contain   css:#draft-releases-tab   View draft releases
    user clicks element   css:#draft-releases-tab
    user waits until draft releases tab contains publication  Pupil absence in schools in England
    user checks draft releases tab publication has release   Pupil absence in schools in England    Academic Year, 2016 to 2017 (Live - Latest release)
    user checks draft releases tab contains publication  Permanent and fixed-period exclusions in England
    user checks draft releases tab publication has release   Permanent and fixed-period exclusions in England    Academic Year, 2016 to 2017 (Live - Latest release)

Validate Analyst1 can see correct scheduled releases
    [Tags]  HappyPath
    user checks element should contain   css:#scheduled-releases-tab   View scheduled releases (0)
    user clicks element   css:#scheduled-releases-tab
    user waits until element contains  css:#scheduled-releases   There are currently no scheduled releases

Validate Analyst1 cannot create a publication for Pupils absence topic
    [Tags]  HappyPath
    user clicks element   css:#my-publications-tab
    user waits until page contains element   css:#selectTheme
    user waits until page contains element   xpath://h3[text()="Pupil absence"]
    user checks page does not contain element   xpath://a[text()="Create new publication"]

Validate Analyst1 cannot create a release for Pupil absence topic
    [Tags]  HappyPath
    user waits until page contains accordion section  Pupil absence in schools in England
    user opens accordion section  Pupil absence in schools in England
    user checks page does not contain element   xpath://*[data-testid="Create new release link for Pupil absence in schools in England"]

Validate Analyst1 can see Absence release summary
    [Tags]  HappyPath
    user opens details dropdown  Academic Year, 2016 to 2017 (Live - Latest release)
    user clicks element  css:[data-testid="Edit release link for Pupil absence in schools in England, Academic Year, 2016 to 2017 (Live - Latest release)"]
    user waits until page contains heading  Pupil absence in schools in England
    user waits until page contains element  xpath://h2[text()="Release summary"]
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
    user waits until page contains element  xpath://legend[text()="Add new data to release"]   60
    user waits until page contains element   css:#upload-data-files-button

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
    user waits until page contains element   css:#publicationSubjectForm-subjectId
    user waits until page contains element   xpath://label[text()="Absence by characteristic"]
    user checks element count is x   xpath://*[@id="publicationSubjectForm-subjectId"]//*[@class="govuk-radios__item"]    7

Validate Analyst1 can see Manage content page
    [Tags]  HappyPath
    user clicks element   xpath://a[text()="Manage content"]
    user waits until page contains element   xpath://h1[text()="Pupil absence in schools in England"]/span[text()="Academic Year 2016/17"]    120

Validate Manage content page is in Edit mode
    [Tags]  HappyPath   Failing
    user waits until page contains element   css:#pageMode
    user checks page contains element  css:#pageMode-edit[checked]
    user checks page does not contain element   css:#pageMode-preview[checked]

Validate Analyst1 can see Manage content page key stats
    [Tags]  HappyPath
    [Documentation]   EES-1508
    user waits until page contains key stat tile   Overall absence rate        4.7%    60
    user waits until page contains key stat tile   Authorised absence rate     3.4%
    user waits until page contains key stat tile   Unauthorised absence rate   1.3%
    user checks element count is x    css:[data-testid="key-stat-tile"]   3

Validate Analyst1 can see Manage content page accordion sections
    [Tags]  HappyPath
    user waits until page contains accordion section  About these statistics
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
    user checks there are x accordion sections  11

Validate Analyst1 cannot Approve a Pupil absence in schools in England release
    [Tags]  HappyPath
    user clicks element   xpath://a[text()="Release status"]
    user waits until page contains element  xpath://h2[text()="Release Status"]
    user clicks element   xpath://button[text()="Update release status"]
    user waits until page contains element   css:#releaseStatusForm
    user checks page contains element   xpath://*[@id="releaseStatusForm-releaseStatus-draft" and not(@disabled)]
    user checks page contains element   xpath://*[@id="releaseStatusForm-releaseStatus-higher-level-review" and not(@disabled)]
    user checks page contains element   xpath://*[@id="releaseStatusForm-releaseStatus-approved" and @disabled]
