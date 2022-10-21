*** Settings ***
Resource            ../../libs/admin-common.robot

Suite Setup         user signs in as analyst1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev


*** Test Cases ***
Validate Analyst1 can see correct themes and topics
    user waits until h3 is visible    Pupils and schools / Pupil absence
    ${ABSENCE_PUBLICATIONS}=    get webelement
    ...    xpath://*[@data-testid="topic-publications-Pupils and schools-Pupil absence"]
    user waits until parent contains element    ${ABSENCE_PUBLICATIONS}    link:Pupil absence in schools in England

    user waits until h3 is visible    Pupils and schools / Exclusions
    ${EXCLUSION_PUBLICATIONS}=    get webelement
    ...    xpath://*[@data-testid="topic-publications-Pupils and schools-Exclusions"]
    user waits until parent contains element    ${EXCLUSION_PUBLICATIONS}
    ...    link:Permanent and fixed-period exclusions in England

Validate Analyst1 can see correct draft and scheduled releases tabs
    user checks element should contain    id:draft-releases-tab    Draft releases
    user checks element should contain    id:scheduled-releases-tab    Approved scheduled releases

Validate Analyst1 cannot create a publication
    user checks page does not contain element    link:Create new publication

Navigate to Pupil absence Publication page
    user clicks link    Pupil absence in schools in England
    user waits until h1 is visible    Pupil absence in schools in England

Validate Analyst1 cannot create a release for Pupil absence topic
    user checks page does not contain    link:Create new release

Navigate to Absence release
    ${ROW}=    user gets table row    Academic Year 2016/17    testid:publication-published-releases
    user clicks element    xpath://*[text()="View"]    ${ROW}

    user waits until h1 is visible    Pupil absence in schools in England
    user waits until h2 is visible    Release summary

Validate Analyst1 can see Absence release summary
    user verifies release summary    Pupil absence in schools in England    ${EMPTY}    Academic Year    2016/17
    ...    Data Analyst    Official statistics

Validate Analyst1 can see 'Content' page
    user clicks link    Content
    user waits for page to finish loading
    user waits until h2 is visible    Pupil absence in schools in England    %{WAIT_SMALL}

Validate Analyst1 can see 'Content' page key stats
    user waits until page contains element    id:releaseHeadlines    %{WAIT_LONG}
    user waits until page does not contain loading spinner
    user scrolls to element    id:releaseHeadlines
    user checks key stat contents    1    Overall absence rate    4.7%    Up from 4.6% in 2015/16    90
    user checks key stat definition    1    What is overall absence?
    ...    Total number of all authorised and unauthorised absences from possible school sessions for all pupils.

    user checks key stat contents    2    Authorised absence rate    3.4%    Similar to previous years
    user checks key stat definition    2    What is authorized absence rate?
    ...    Number of authorised absences as a percentage of the overall school population.

    user checks key stat contents    3    Unauthorised absence rate    1.3%    Up from 1.1% in 2015/16
    user checks key stat definition    3    What is unauthorized absence rate?
    ...    Number of unauthorised absences as a percentage of the overall school population.

    user checks element count is x    css:[data-testid="keyStat"]    3

Validate Analyst1 can see 'Content' page accordion sections
    user checks accordion is in position    About these statistics    1    id:releaseMainContent
    user checks accordion is in position    Pupil absence rates    2    id:releaseMainContent
    user checks accordion is in position    Persistent absence    3    id:releaseMainContent
    user checks accordion is in position    Reasons for absence    4    id:releaseMainContent
    user checks accordion is in position    Distribution of absence    5    id:releaseMainContent
    user checks accordion is in position    Absence by pupil characteristics    6    id:releaseMainContent
    user checks accordion is in position    Absence for 4-year-olds    7    id:releaseMainContent
    user checks accordion is in position    Pupil referral unit absence    8    id:releaseMainContent
    user checks accordion is in position    Regional and local authority (LA) breakdown    9    id:releaseMainContent
    user checks there are x accordion sections    9    id:releaseMainContent
    user checks accordion is in position    Methodology    1    id:helpAndSupport
    user checks accordion is in position    Official statistics    2    id:helpAndSupport
    user checks accordion is in position    Contact us    3    id:helpAndSupport
    user checks there are x accordion sections    3    id:helpAndSupport
