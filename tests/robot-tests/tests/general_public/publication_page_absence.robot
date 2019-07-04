*** Settings ***
Resource    ../libs/library.robot

Force Tags  GeneralPublic

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Navigate to Absence publication
    [Tags]  HappyPath
    user goes to url  ${url}
    user waits until page contains  Select an option to find the national and regional

    user clicks link  Find statistics and data
    user waits until page contains  Browse to find the statistics and data you’re looking for

    user opens accordion section  Pupils and schools
    user opens details dropdown   Pupil absence
    user clicks element   css:[data-testid="view-stats-pupil-absence-in-schools-in-england"]
    user waits until page contains  Pupil absence data and statistics for schools in England

Validate URL
    [Documentation]  DFE-325
    [Tags]  HappyPath
    ${current_url}=  get location
    should be equal   ${current_url}   ${url}/statistics/pupil-absence-in-schools-in-england

Validate Published date and Next update date
    [Tags]     HappyPath
    user checks element contains  css:[data-testid="published-date"]   22 March 2018
    user checks element contains  css:[data-testid="next-update"]      March 2019

Validate "About these statistics" -- "For school year"
    [Documentation]  DFE-197 DFE-845
    [Tags]  HappyPath   Failing
    user checks element contains  css:[data-testid="release-name"]    2016 to 2017

    user checks number of previous releases is correct   8
    user opens details dropdown     See previous 8 releases
    user checks previous release is shown in position  2016 to 2017     1
    user checks previous release is shown in position  2015 to 2016     2
    user checks previous release is shown in position  2014 to 2015     3
    user checks previous release is shown in position  2013 to 2014     4
    user checks previous release is shown in position  2012 to 2013     5
    user checks previous release is shown in position  2011 to 2012     6
    user checks previous release is shown in position  2010 to 2011     7
    user checks previous release is shown in position  2009 to 2010     8
    user closes details dropdown   See previous 8 releases

Validate "About these statistics" -- "Last updated"
    [Tags]    HappyPath
    user checks element contains  css:[data-testid="last-updated"]     19 April 2018

    user checks number of updates is correct    2
    user opens details dropdown   See all 2 updates
    user checks update exists   19 April 2018   Underlying data file updated to include absence
    user checks update exists   22 March 2018   First published.
    user closes details dropdown   See all 2 updates

Validate Key Statistics data block -- Summary tab
    [Documentation]     DFE-915
    [Tags]  HappyPath   Failing
    user checks key stat tile contents   Overall absence rate         4.7%   Up from 4.6% in 2015/16
    user checks key stat tile contents   Authorised absence rate      3.4%   Similar to previous years
    user checks key stat tile contents   Unauthorised absence rate    1.3%   Up from 1.1% in 2015/16

    user checks key stat bullet exists   pupils missed on average 8.2 school days
    user checks key stat bullet exists   overall and unauthorised absence rates up on 2015/16
    user checks key stat bullet exists   unauthorised absence rise due to higher rates of unauthorised holidays
    user checks key stat bullet exists   10% of pupils persistently absent during 2016/17

Validate accordion sections order
    [Tags]  HappyPath
    user checks accordion is in position  About these statistics            1
    user checks accordion is in position  Pupil absence rates               2
    user checks accordion is in position  Persistent absence                3
    user checks accordion is in position  Reasons for absence               4
    user checks accordion is in position  Distribution of absence           5
    user checks accordion is in position  Absence by pupil characteristics  6
    user checks accordion is in position  Absence for 4-year-olds           7
    user checks accordion is in position  Pupil referral unit absence       8
    user checks accordion is in position  Regional and local authority (LA) breakdown  9
    user checks accordion is in position  Pupil absence data and statistics for schools in England: methodology     10
    user checks accordion is in position  National Statistics               11
    user checks accordion is in position  Contact us                        12

Clicking "Create tables" takes user to Table Tool page with absence publication selected
    [Documentation]  DFE-898
    [Tags]  HappyPath    Failing
    user clicks link    Create tables
    user waits until page contains  Create your own tables online
    user clicks button   css:#publicationForm-submit
    user waits until page contains  Choose a subject
    user checks previous table tool step contains  css:#tableTool-steps-step-1   Publication   Pupil absence in schools in England
