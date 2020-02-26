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

Validate Analyst1 cannot Approve a Pupil absence in schools in England release
    [Tags]  HappyPath
    sleep  10
    user opens details dropdown  Academic Year, 2016 to 2017 (Live - Latest release)
    user clicks element  css:[data-testid="Edit release link for Pupil absence in schools in England, Academic Year, 2016 to 2017 (Live - Latest release)"]
    user waits until page contains heading  Pupil absnece in schools in England
    user clicks element   xpath://a[text()="Release status"]
    user waits until page contains element  xpath://h2[text()="Release status"]
    user clicks element   xpath://button[text()="Update release status"]
    user waits until page contains element   css:#releaseStatusForm
    user checks page contains element   xpath://*[@id="releaseStatusForm-releaseStatus-draft" and not(@disabled)]
    user checks page contains element   xpath://*[@id="releaseStatusForm-releaseStatus-higher-level-review" and not(@disabled)]
    user checks page contains element   xpath://*[@id="releaseStatusForm-releaseStatus-approved" and @disabled]

This?
    [Tags]  HappyPath
    sleep  100000