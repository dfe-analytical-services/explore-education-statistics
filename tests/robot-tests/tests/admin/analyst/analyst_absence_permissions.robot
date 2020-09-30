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
    user waits until h3 is visible  Pupil absence
    user checks page does not contain element   link:Create new publication

Validate Analyst1 cannot create a release for Pupil absence topic
    [Tags]  HappyPath
    user waits until page contains accordion section  Pupil absence in schools in England
    user opens accordion section  Pupil absence in schools in England
    user checks page does not contain element   xpath://*[data-testid="Create new release link for Pupil absence in schools in England"]

Navigate to Absence release
    [Tags]  HappyPath
    user opens details dropdown  Academic Year 2016/17 (Live - Latest release)
    user clicks testid element   Edit release link for Pupil absence in schools in England, Academic Year 2016/17 (Live - Latest release)
    user waits until h1 is visible  Pupil absence in schools in England
    user waits until h2 is visible  Release summary

Validate Analyst1 can see Absence release summary
    [Tags]  HappyPath   NotAgainstLocal
    user checks summary list contains  Publication title  Pupil absence in schools in England
    user checks summary list contains  Time period  Academic Year
    user checks summary list contains  Release period  2016/17
    user checks summary list contains  Lead statistician  Sean Gibson
    user checks summary list contains  Scheduled release  25 April 2018
    user checks summary list contains  Next release expected  22 March 2019
    user checks summary list contains  Release type  Official Statistics

Validate Analyst1 can see 'Content' page
    [Tags]  HappyPath
    user clicks link  Content
    user waits until h2 is visible   Pupil absence in schools in England

Validate Analyst1 can see 'Content' page key stats
    [Tags]  HappyPath
    user waits until page contains element  id:releaseHeadlines
    user scrolls to element  id:releaseHeadlines

    user checks key stat contents    1  Overall absence rate         4.7%   Up from 4.6% in 2015/16   90
    user checks key stat definition  1  What is overall absence?  Total number of all authorised and unauthorised absences from possible school sessions for all pupils.

    user checks key stat contents    2  Authorised absence rate      3.4%   Similar to previous years
    user checks key stat definition  2  What is authorized absence rate?  Number of authorised absences as a percentage of the overall school population.

    user checks key stat contents    3  Unauthorised absence rate    1.3%   Up from 1.1% in 2015/16
    user checks key stat definition  3  What is unauthorized absence rate?  Number of unauthorised absences as a percentage of the overall school population.

    user checks element count is x    css:[data-testid="keyStat"]   3

Validate Analyst1 can see 'Content' page accordion sections
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
