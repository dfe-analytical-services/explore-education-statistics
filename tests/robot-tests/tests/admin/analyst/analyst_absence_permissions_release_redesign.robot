*** Settings ***
Resource            ../../libs/admin-common.robot
Resource            ../../libs/admin/manage-content-common.robot
Resource            ../../seed_data/seed_data_theme_1_constants.robot

Suite Setup         user signs in as analyst1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    ReleaseRedesign


*** Test Cases ***
Validate Analyst1 can see correct themes
    user waits until h3 is visible    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    ${ABSENCE_PUBLICATIONS}=    get webelement
    ...    xpath://*[@data-testid="theme-publications-${PUPILS_AND_SCHOOLS_THEME_TITLE}"]
    user waits until parent contains element    ${ABSENCE_PUBLICATIONS}
    ...    link:${PUPIL_ABSENCE_PUBLICATION_TITLE}

    user waits until h3 is visible    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    ${EXCLUSION_PUBLICATIONS}=    get webelement
    ...    xpath://*[@data-testid="theme-publications-${PUPILS_AND_SCHOOLS_THEME_TITLE}"]
    user waits until parent contains element    ${EXCLUSION_PUBLICATIONS}
    ...    link:${EXCLUSIONS_PUBLICATION_TITLE}

Validate Analyst1 can see correct draft, approvals and scheduled releases tabs
    user checks element should contain    id:draft-releases-tab    Draft releases
    user checks element should contain    id:scheduled-releases-tab    Approved scheduled releases
    user checks element should contain    id:approvals-tab    Your approvals

Validate Analyst1 cannot create a publication
    user checks page does not contain element    link:Create new publication

Navigate to Seed Data Theme 1 Publication 1 page
    user clicks link    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user waits until h1 is visible    ${PUPIL_ABSENCE_PUBLICATION_TITLE}

Navigate to publication release order
    user clicks link    Release order
    user waits until h2 is visible    Release order

Validate Analyst1 can see correct publication release order
    user checks table body has x rows    7    testid:release-series

    user checks table cell contains    1    1    Academic year 2016/17
    user checks table cell contains    1    2
    ...    %{PUBLIC_URL}/find-statistics/seed-publication-pupil-absence-in-schools-in-england/2016-17
    user checks table cell contains    1    3    Latest release

    user checks table cell contains    2    1    Academic year 2014/15
    user checks table cell contains    2    2
    ...    https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2014-to-2015
    user checks table cell contains    2    3    Legacy release

    user checks table cell contains    3    1    Academic year 2013/14
    user checks table cell contains    3    2
    ...    https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2013-to-2014
    user checks table cell contains    3    3    Legacy release

    user checks table cell contains    4    1    Academic year 2012/13
    user checks table cell contains    4    2
    ...    https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2012-to-2013
    user checks table cell contains    4    3    Legacy release

    user checks table cell contains    5    1    Academic year 2011/12
    user checks table cell contains    5    2
    ...    https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics
    user checks table cell contains    5    3    Legacy release

    user checks table cell contains    6    1    Academic year 2010/11
    user checks table cell contains    6    2
    ...    https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics-academic-year-2010-to-2011
    user checks table cell contains    6    3    Legacy release

    user checks table cell contains    7    1    Academic year 2009/10
    user checks table cell contains    7    2
    ...    https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics-academic-year-2009-to-2010
    user checks table cell contains    7    3    Legacy release

Check Analyst1 cannot create a legacy release
    user checks page does not contain button    Create legacy release

Check Analyst1 cannot reorder releases
    user checks page does not contain button    Reorder releases

Check Analyst1 cannot edit a legacy release
    user checks page does not contain button    Edit

Check Analyst1 cannot delete a legacy release
    user checks page does not contain button    Delete

Navigate to releases
    user clicks link    Releases
    user waits until h2 is visible    Manage releases

Validate Analyst1 cannot create a release for Seed Data Theme 1 Publication 1 publication
    user checks page does not contain    link:Create new release

Navigate to Absence release
    ${ROW}=    user gets table row    Academic year 2016/17    testid:publication-published-releases
    user clicks link containing text    View    ${ROW}

    user waits until h1 is visible    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user waits until h2 is visible    Release summary

Validate Analyst1 can see Absence release summary
    user verifies release summary    Academic year    2016/17
    ...    Official statistics

Validate Analyst1 can see 'Content' page
    user clicks link    Content
    user waits until page finishes loading
    user waits until h2 is visible    ${PUPIL_ABSENCE_PUBLICATION_TITLE}    %{WAIT_SMALL}

Validate Analyst1 can see 'Content' page key stats
    user waits until page contains element    id:releaseHeadlines    %{WAIT_LONG}
    user waits until page finishes loading
    user scrolls to element    id:releaseHeadlines
    user checks key stat contents    1    Overall absence rate    4.7%    Up from 4.6% in 2015/16    90
    user checks key stat guidance    1    What is overall absence?
    ...    Total number of all authorised and unauthorised absences from possible school sessions for all pupils.

    user checks key stat contents    2    Authorised absence rate    3.4%    Similar to previous years
    user checks key stat guidance    2    What is authorized absence rate?
    ...    Number of authorised absences as a percentage of the overall school population.

    user checks key stat contents    3    Unauthorised absence rate    1.3%    Up from 1.1% in 2015/16
    user checks key stat guidance    3    What is unauthorized absence rate?
    ...    Number of unauthorised absences as a percentage of the overall school population.

    user checks element count is x    css:[data-testid="keyStat"]    3

Validate Analyst1 can see 'Content' page sections
    user checks section is in position    About these statistics    1
    user checks section is in position    Pupil absence rates    2
    user checks section is in position    Persistent absence    3
    user checks section is in position    Reasons for absence    4
    user checks section is in position    Distribution of absence    5
    user checks section is in position    Absence by pupil characteristics    6
    user checks section is in position    Absence for 4-year-olds    7
    user checks section is in position    Pupil referral unit absence    8
    user checks section is in position    Regional and local authority (LA) breakdown    9

Validate navigation tabs are persistent
    user checks the page contains the tabs
    ...    Release home
    ...    Explore and download data
    ...    Methodology
    ...    Help and related information


*** Keywords ***
user checks the page contains the tabs
    [Arguments]    @{expected_nav_items}
    FOR    ${nav_item}    IN    @{expected_nav_items}
        ${nav_item_xpath}=    Set Variable
        ...    //div[@id="release-page-tabs"]//a[contains(text(), "${nav_item}")]
        Page Should Contain Element
        ...    xpath=${nav_item_xpath}
        ...    Page is missing "${nav_item}" nav item
    END
