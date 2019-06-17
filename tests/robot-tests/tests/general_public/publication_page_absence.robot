*** Settings ***
Resource    ../libs/library.robot

Force Tags  GeneralPublic

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Navigate to Absence publication
    [Tags]  HappyPath
    user goes to url  ${url}
    user clicks link  Find statistics and data
    user waits until page contains  Browse to find the statistics and data you’re looking for

    user clicks element   css:[data-testid="SectionHeader Pupils and schools"] button
    element attribute value should be  css:[data-testid="SectionHeader Pupils and schools"] button   aria-expanded   true

    user clicks element containing text  Pupil absence

    user clicks element   css:[data-testid="view-stats-pupil-absence-in-schools-in-england"]
    user waits until page contains  Pupil absence data and statistics for schools in England

Validate URL
    [Documentation]  DFE-325
    [Tags]  HappyPath
    ${current_url}=  get location
    should be equal   ${current_url}   ${url}/statistics/pupil-absence-in-schools-in-england

Validate Published date
    [Tags]     HappyPath
    user checks element contains  css:[data-testid="published-date"]   22 March 2018

Validate "About these statistics" -- "For school year"
    [Documentation]  DFE-197
    [Tags]  HappyPath   Failing
    user checks element contains  css:[data-testid="release-period"]    2016 to 2017 (latest data)

    user clicks element  css:[data-testid="See previous 7 releases"] [data-testid="details--expand"]
    element attribute value should be  css:[data-testid="See previous 7 releases"] summary   aria-expanded   true

    user checks element contains  css:[data-testid="previous-releases-list"] li:nth-child(1) a    2015 to 2016
    user checks element contains  css:[data-testid="previous-releases-list"] li:nth-child(2) a    2014 to 2015
    user checks element contains  css:[data-testid="previous-releases-list"] li:nth-child(3) a    2013 to 2014
    user checks element contains  css:[data-testid="previous-releases-list"] li:nth-child(4) a    2012 to 2013
    user checks element contains  css:[data-testid="previous-releases-list"] li:nth-child(5) a    2011 to 2012
    user checks element contains  css:[data-testid="previous-releases-list"] li:nth-child(6) a    2010 to 2011
    user checks element contains  css:[data-testid="previous-releases-list"] li:nth-child(7) a    2009 to 2010

    user clicks element  css:[data-testid="See previous 7 releases"] [data-testid="details--expand"]
    element attribute value should be  css:[data-testid="See previous 7 releases"] summary   aria-expanded   false


Validate "About these statistics" -- "Last updated"
    [Tags]     HappyPath
    user checks element contains  css:[data-testid="last-updated"] time     19 April 2017

#    user clicks details   css:[data-testid="See all 2 updates"]
    user clicks element containing text   See all 2 updates
#    element attribute value should be  css:[data-testid="See all 2 updates"] summary   aria-expanded   true

    user checks element contains  css:[data-testid="last-updated-element"]:nth-child(1) time   19 April 2017
    user checks element contains  css:[data-testid="last-updated-element"]:nth-child(1) p   Underlying data file updated to include absence data

    user checks element contains  css:[data-testid="last-updated-element"]:nth-child(2) time   22 March 2017
    user checks element contains  css:[data-testid="last-updated-element"]:nth-child(2) p   First published.

#    user clicks details   css:[data-testid="See all 2 updates"]
    user clicks element containing text   See all 2 updates
#    element attribute value should be  css:[data-testid="See all 2 updates"] summary   aria-expanded   false

Validate Key Statistics data block -- Summary tab
    [Tags]  HappyPath   Failing
    user checks element contains  css:[data-testid="tile Overall absence rate       4.7%
    user checks element contains  css:[data-testid="tile Authorised absence rate    3.4%
    user checks element contains  css:[data-testid="tile Unauthorised absence rate  1.3%

    user checks element contains  css:[data-testid="Summary"] li:nth-child(1)   pupils missed on average 8.2 school days
    user checks element contains  css:[data-testid="Summary"] li:nth-child(2)   overall and unauthorised absence rates up on previous year
    user checks element contains  css:[data-testid="Summary"] li:nth-child(3)   unauthorised rise due to higher rates of unauthorised holidays
    user checks element contains  css:[data-testid="Summary"] li:nth-child(4)   10% of pupils persistently absent during 2016/17

Validate Contents section headings
    [Tags]  HappyPath
    user waits until page contains element  css:[data-testid="SectionHeader About these statistics"]
    user waits until page contains element  css:[data-testid="SectionHeader Pupil absence rates"]
    user waits until page contains element  css:[data-testid="SectionHeader Persistent absence"]
    user waits until page contains element  css:[data-testid="SectionHeader Reasons for absence"]
    user waits until page contains element  css:[data-testid="SectionHeader Distribution of absence"]
    user waits until page contains element  css:[data-testid="SectionHeader Absence by pupil characteristics"]
    user waits until page contains element  css:[data-testid="SectionHeader Absence for 4-year-olds"]
    user waits until page contains element  css:[data-testid="SectionHeader Pupil referral unit absence"]
    user waits until page contains element  css:[data-testid="SectionHeader Regional and local authority (LA) breakdown"]

Validate Extra Information section headings
    [Tags]  HappyPath
    user waits until page contains element  css:[data-testid="extra-information"]+div [data-testid="SectionHeader Where does this data come from"]
    user waits until page contains element  css:[data-testid="extra-information"]+div [data-testid="SectionHeader Feedback and questions"]
    user waits until page contains element  css:[data-testid="extra-information"]+div [data-testid="SectionHeader Contact us"]

Clicking "Go to top" move user to the top of the page
    [Tags]  HappyPath
    user clicks element  css:[data-testid="SectionHeader Contact us"]
    scroll element into view  link:Go to top
    user clicks element  link:Go to top
    user should be at top of page

Clicking "Create charts and tables" takes user to Table Tool page
    [Tags]  HappyPath   Failing
    user clicks link    Create charts and tables
    user waits until page contains element  css:[data-testid="page-title Create your own table"]

