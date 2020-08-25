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

    user checks list contains label   id:selectTheme   Pupils and schools
    user checks list contains x elements   id:selectTopic   2
    user checks list contains label   id:selectTopic   Exclusions
    user checks list contains label   id:selectTopic   Pupil absence

Validate Analyst1 can see correct draft and scheduled releases tabs
    [Tags]  HappyPath
    user checks element should contain   id:draft-releases-tab   View draft releases
    user checks element should contain   id:scheduled-releases-tab   View scheduled releases

Validate Analyst1 cannot create a publication for Pupils absence topic
    [Tags]  HappyPath
    user clicks element   id:my-publications-tab
    user waits until page contains element   id:selectTheme
    user waits until page contains heading 3  Pupil absence
    user checks page does not contain element   link:Create new publication

Validate Analyst1 cannot create a release for Pupil absence topic
    [Tags]  HappyPath
    user waits until page contains accordion section  Pupil absence in schools in England
    user opens accordion section  Pupil absence in schools in England
    user checks page does not contain element   xpath://*[data-testid="Create new release link for Pupil absence in schools in England"]

Navigate to Absence release
    [Tags]  HappyPath
    user opens details dropdown  Academic Year 2016/17 (Live - Latest release)
    user clicks element  css:[data-testid="Edit release link for Pupil absence in schools in England, Academic Year 2016/17 (Live - Latest release)"]
    user waits until page contains heading 1  Pupil absence in schools in England
    user waits until page contains heading 2  Release summary

Validate Analyst1 can see Absence release summary
    [Tags]  HappyPath   NotAgainstLocal
    user checks summary list contains  Publication title  Pupil absence in schools in England
    user checks summary list contains  Time period  Academic Year
    user checks summary list contains  Release period  2016/17
    user checks summary list contains  Lead statistician  Sean Gibson
    user checks summary list contains  Scheduled release  25 April 2018
    user checks summary list contains  Next release expected  22 March 2019
    user checks summary list contains  Release type  Official Statistics

Validate Analyst1 can see Manage content page
    [Tags]  HappyPath
    user clicks link  Manage content
    user waits until page contains heading 2   Pupil absence in schools in England

Validate Analyst1 can see 'Manage content' page key stats
    [Tags]  HappyPath
    user waits until page contains element  id:releaseHeadlines
    user scrolls to element  id:releaseHeadlines
    user waits until page contains key stat tile   Overall absence rate        4.7%    60
    user waits until page contains key stat tile   Authorised absence rate     3.4%
    user waits until page contains key stat tile   Unauthorised absence rate   1.3%
    user checks element count is x    css:[data-testid="keyStatTile-title"]   3

Validate Analyst1 can see 'Manage content' page accordion sections
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
