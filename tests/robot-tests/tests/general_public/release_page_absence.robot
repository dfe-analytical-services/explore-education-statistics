*** Settings ***
Resource    ../libs/public-common.robot
Resource    ../libs/charts.robot

Force Tags  GeneralPublic  Local  Dev  Test  Preprod

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Navigate to Absence publication
    [Tags]  HappyPath
    environment variable should be set  PUBLIC_URL
    user goes to url  %{PUBLIC_URL}
    user waits until page contains  Select an option to find the national and regional

    user clicks link  Find statistics and data
    user waits until page contains  Browse to find the statistics and data you’re looking for
    user waits for page to finish loading

    user opens accordion section  Pupils and schools
    user opens details dropdown   Pupil absence
    user clicks testid element  View stats link for Pupil absence in schools in England
    user waits until h1 is visible  Pupil absence in schools in England

Validate URL
    [Documentation]  DFE-325
    [Tags]  HappyPath
    user checks url contains  %{PUBLIC_URL}/find-statistics/pupil-absence-in-schools-in-england

Validate Published date
    [Tags]     HappyPath    NotAgainstPreProd    Failing
    user waits until element contains  css:[data-testid="published-date"]   25 April 2018

Validate Next update date
    [Tags]     HappyPath    NotAgainstPreProd
    user waits until element contains  css:[data-testid="next-update"]      22 March 2019

Validate Email alerts link
    [Tags]     HappyPath
    user checks page contains link with text and url  Sign up for email alerts    /subscriptions?slug=pupil-absence-in-schools-in-england

Validate "About these statistics" -- "For Academic Year:"
    [Documentation]  DFE-197 DFE-845
    [Tags]  HappyPath
    user waits until element contains  css:[data-testid="release-name"]    2016/17

Validate "About these statistics" -- Number of other releases
    [Tags]  HappyPath
    user checks number of other releases is correct   6
    user opens details dropdown     See 6 other releases
    user checks other release is shown in position  Academic Year 2014/15    1
    user checks other release is shown in position  Academic Year 2013/14    2
    user checks other release is shown in position  Academic Year 2012/13    3
    user checks other release is shown in position  Academic Year 2011/12    4
    user checks other release is shown in position  Academic Year 2010/11    5
    user checks other release is shown in position  Academic Year 2009/10    6
    user closes details dropdown   See 6 other releases

Validate "About these statistics" -- "Last updated"
    [Tags]    HappyPath
    user waits until element contains  id:releaseLastUpdated     19 April 2018

    user checks number of release updates     2
    user opens details dropdown   See all 2 updates
    user checks release update   1  19 April 2018   Underlying data file updated to include absence
    user checks release update   2  22 March 2018   First published.
    user closes details dropdown   See all 2 updates

Validate "Related guidance"
    [Tags]  HappyPath
    user checks page contains link with text and url  Pupil absence in schools in England: methodology   /methodology/pupil-absence-in-schools-in-england

Validate subject files file type and file unit style
    [Documentation]  DFE-958   DFE-562
    [Tags]  HappyPath   NotAgainstLocal   Failing
    user opens details dropdown     Download data files
    user checks page contains      Absence in PRUs (CSV, 141 Kb)
    user closes details dropdown     Download data files

Validate absence_in_prus.csv file can be downloaded
    [Documentation]  DFE-958   DFE-562
    [Tags]  HappyPath    NotAgainstLocal  Failing
    user opens details dropdown     Download data files

    download file  link:Absence in PRUs     absence_in_prus.csv
    downloaded file should have first line  absence_in_prus.csv   time_identifier,time_period,geographic_level,country_code,country_name,region_code,region_name,old_la_code,new_la_code,la_name,school_type,num_schools,enrolments,sess_possible,sess_overall,sess_authorised,sess_unauthorised,sess_overall_percent,sess_authorised_percent,sess_unauthorised_percent,enrolments_pa10_exact,enrolments_pa10_exact_percent,sess_auth_illness,sess_auth_appointments,sess_auth_religious,sess_auth_study,sess_auth_traveller,sess_auth_holiday,sess_auth_ext_holiday,sess_auth_excluded,sess_auth_other,sess_auth_totalreasons,sess_unauth_holiday,sess_unauth_late,sess_unauth_other,sess_unauth_noyet,sess_unauth_totalreasons,sess_overall_totalreasons

    user closes details dropdown     Download data files

Validate headlines -- Summary tab key stats
    [Documentation]  DFE-915   EES-806   EES-1508
    [Tags]  HappyPath
    user checks key stat contents   1  Overall absence rate         4.7%   Up from 4.6% in 2015/16
    user checks key stat contents   2  Authorised absence rate      3.4%   Similar to previous years
    user checks key stat contents   3  Unauthorised absence rate    1.3%   Up from 1.1% in 2015/16

Validate headlines -- Summary tab content
    [Documentation]    EES-718
    [Tags]  HappyPath  NotAgainstPreProd
    user checks headline summary contains   pupils missed on average 8.2 school days
    user checks headline summary contains   overall and unauthorised absence rates up on 2015/16
    user checks headline summary contains   unauthorised absence rise due to higher rates of unauthorised holidays
    user checks headline summary contains   10% of pupils persistently absent during 2016/17

Validate Key Statistics data block -- Charts tab
    [Tags]  HappyPath
    user clicks element   id:releaseHeadlines-charts-tab
    ${headline_chart}=  set variable  css:#releaseHeadlines-chart
    user waits until element contains line chart  ${headline_chart}
    user checks chart legend item contains  ${headline_chart}  1  Unauthorised absence rate (England)
    user checks chart legend item contains  ${headline_chart}  2  Authorised absence rate (England)
    user checks chart legend item contains  ${headline_chart}  3  Overall absence rate (England)

    user checks chart x axis ticks  ${headline_chart}  2012/13  2013/14  2014/15  2015/16  2016/17
    user checks chart y axis ticks  ${headline_chart}  0  2  4  6

    user mouses over line chart point  ${headline_chart}  1  1
    user checks chart tooltip label contains  ${headline_chart}  2012/13
    user checks chart tooltip item contains  ${headline_chart}  1  Overall absence rate (England): 5.3%
    user checks chart tooltip item contains  ${headline_chart}  2  Authorised absence rate (England): 4.2%
    user checks chart tooltip item contains  ${headline_chart}  3  Unauthorised absence rate (England): 1.1%

    user mouses over line chart point  ${headline_chart}  1  2
    user checks chart tooltip label contains  ${headline_chart}  2013/14
    user checks chart tooltip item contains  ${headline_chart}  1  Overall absence rate (England): 4.5%
    user checks chart tooltip item contains  ${headline_chart}  2  Authorised absence rate (England): 3.5%
    user checks chart tooltip item contains  ${headline_chart}  3  Unauthorised absence rate (England): 1.1%

    user mouses over line chart point  ${headline_chart}  1  3
    user checks chart tooltip label contains  ${headline_chart}  2014/15
    user checks chart tooltip item contains  ${headline_chart}  1  Overall absence rate (England): 4.6%
    user checks chart tooltip item contains  ${headline_chart}  2  Authorised absence rate (England): 3.5%
    user checks chart tooltip item contains  ${headline_chart}  3  Unauthorised absence rate (England): 1.1%

    user mouses over line chart point  ${headline_chart}  1  4
    user checks chart tooltip label contains  ${headline_chart}  2015/16
    user checks chart tooltip item contains  ${headline_chart}  1  Overall absence rate (England): 4.6%
    user checks chart tooltip item contains  ${headline_chart}  2  Authorised absence rate (England): 3.4%
    user checks chart tooltip item contains  ${headline_chart}  3  Unauthorised absence rate (England): 1.1%

    user mouses over line chart point  ${headline_chart}  1  5
    user checks chart tooltip label contains  ${headline_chart}  2016/17
    user checks chart tooltip item contains  ${headline_chart}  1  Overall absence rate (England): 4.7%
    user checks chart tooltip item contains  ${headline_chart}  2  Authorised absence rate (England): 3.4%
    user checks chart tooltip item contains  ${headline_chart}  3  Unauthorised absence rate (England): 1.3%

    user waits until element contains  ${headline_chart} [data-testid="footnotes"] li:nth-of-type(1)  Absence rates are the number of absence sessions expressed
    user waits until element contains  ${headline_chart} [data-testid="footnotes"] li:nth-of-type(2)  There may be discrepancies between totals and the sum of constituent parts
    user waits until element contains  ${headline_chart} [data-testid="footnotes"] li:nth-of-type(3)  x - 1 or 2 enrolments, or a percentage based on 1 or 2 enrolments.

Validate Key Statistics data block -- Data tables tab
   [Tags]  HappyPath
   user clicks element   css:#releaseHeadlines-tables-tab
   user waits until element contains   css:#dataTableCaption    Table showing 'Absence by characteristic' from 'Pupil absence in schools in England' in England between 2012/13 and 2016/17

   user checks table column heading contains  css:#releaseHeadlines-tables table  1   1   2012/13
   user checks table column heading contains  css:#releaseHeadlines-tables table  1   2   2013/14
   user checks table column heading contains  css:#releaseHeadlines-tables table  1   3   2014/15
   user checks table column heading contains  css:#releaseHeadlines-tables table  1   4   2015/16
   user checks table column heading contains  css:#releaseHeadlines-tables table  1   5   2016/17

   ${row}=  user gets row with group and indicator  css:#releaseHeadlines-tables table   England   Authorised absence rate
   user checks row contains heading  ${row}   Authorised absence rate
   user checks row cell contains text  ${row}   1    4.2
   user checks row cell contains text  ${row}   2    3.5
   user checks row cell contains text  ${row}   3    3.5
   user checks row cell contains text  ${row}   4    3.4
   user checks row cell contains text  ${row}   5    3.4

   ${row}=  user gets row with group and indicator  css:#releaseHeadlines-tables table   England   Unauthorised absence rate
   user checks row contains heading  ${row}   Unauthorised absence rate
   user checks row cell contains text  ${row}   1    1.1
   user checks row cell contains text  ${row}   2    1.1
   user checks row cell contains text  ${row}   3    1.1
   user checks row cell contains text  ${row}   4    1.1
   user checks row cell contains text  ${row}   5    1.3

   ${row}=  user gets row with group and indicator  css:#releaseHeadlines-tables table   England   Overall absence rate
   user checks row contains heading  ${row}   Overall absence rate
   user checks row cell contains text  ${row}   1    5.3
   user checks row cell contains text  ${row}   2    4.5
   user checks row cell contains text  ${row}   3    4.6
   user checks row cell contains text  ${row}   4    4.6
   user checks row cell contains text  ${row}   5    4.7

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
    user checks accordion is in position  Methodology                       10
    user checks accordion is in position  Contact us                        11

Validate Regional and local authority (LA) breakdown table
    [Tags]  HappyPath   Failing
    [Documentation]  BAU-540
    user opens accordion section  Regional and local authority (LA) breakdown
    user waits until element contains  css:#content_9_datablock-tables #dataTableCaption    Table showing 'Absence by characteristic' from 'Pupil absence in schools in England' in

    user checks table column heading contains  css:#content_9_datablock-tables table   1   1   2016/17

    ${row}=  user gets row with group and indicator  css:#content_9_datablock-tables table   Vale of White Horse   Authorised absence rate
    user checks row contains heading  ${row}   Authorised absence rate
    user checks row cell contains text  ${row}   1    3.4%
    ${row}=  user gets row with group and indicator  css:#content_9_datablock-tables table   Vale of White Horse   Overall absence rate
    user checks row contains heading  ${row}   Overall absence rate
    user checks row cell contains text  ${row}   1    4.3%
    ${row}=  user gets row with group and indicator  css:#content_9_datablock-tables table   Vale of White Horse   Unauthorised absence rate
    user checks row contains heading  ${row}   Unauthorised absence rate
    user checks row cell contains text  ${row}   1    0.9%

    ${row}=  user gets row with group and indicator  css:#content_9_datablock-tables table  Harlow   Authorised absence rate
    user checks row contains heading  ${row}   Authorised absence rate
    user checks row cell contains text  ${row}   1    3.1%
    ${row}=  user gets row with group and indicator  css:#content_9_datablock-tables table  Harlow   Overall absence rate
    user checks row contains heading  ${row}   Overall absence rate
    user checks row cell contains text  ${row}   1    4.2%
    ${row}=  user gets row with group and indicator  css:#content_9_datablock-tables table  Harlow   Unauthorised absence rate
    user checks row contains heading  ${row}   Unauthorised absence rate
    user checks row cell contains text  ${row}   1    1.1%

    ${row}=  user gets row with group and indicator  css:#content_9_datablock-tables table  Newham   Authorised absence rate
    user checks row contains heading  ${row}   Authorised absence rate
    user checks row cell contains text  ${row}   1    2.7%
    ${row}=  user gets row with group and indicator  css:#content_9_datablock-tables table  Newham   Overall absence rate
    user checks row contains heading  ${row}   Overall absence rate
    user checks row cell contains text  ${row}   1    4.4%
    ${row}=  user gets row with group and indicator  css:#content_9_datablock-tables table  Newham   Unauthorised absence rate
    user checks row contains heading  ${row}   Unauthorised absence rate
    user checks row cell contains text  ${row}   1    1.7%

Validate Regional and local authority (LA) breakdown chart
    [Tags]  HappyPath
    user opens accordion section  Regional and local authority (LA) breakdown
    user scrolls to accordion section content  Regional and local authority (LA) breakdown

    ${datablock}=  set variable  css:[data-testid="Data block - Generic data block - LA"]
    user waits until element contains map chart  ${datablock}

    user selects from list by label  ${datablock} select[name="selectedLocation"]  Vale of White Horse
    user waits until element does not contain chart tooltip  ${datablock}

    user mouses over selected map feature  ${datablock}
    user checks chart tooltip label contains  ${datablock}  Vale of White Horse
    user checks chart tooltip item contains  ${datablock}  1  Unauthorised absence rate (2016/17): 0.9%
    user checks chart tooltip item contains  ${datablock}  2  Overall absence rate (2016/17): 4.3%
    user checks chart tooltip item contains  ${datablock}  3  Authorised absence rate (2016/17): 3.4%

    user checks map chart indicator tile contains  ${datablock}  1  Unauthorised absence rate (2016/17)  0.9%
    user checks map chart indicator tile contains  ${datablock}  2  Overall absence rate (2016/17)  4.3%
    user checks map chart indicator tile contains  ${datablock}  3  Authorised absence rate (2016/17)  3.4%

    user mouses over element  ${datablock} select[name="selectedLocation"]
    user selects from list by label  ${datablock} select[name="selectedLocation"]  Harlow
    user waits until element does not contain chart tooltip  ${datablock}

    user mouses over selected map feature  ${datablock}
    user checks chart tooltip label contains  ${datablock}  Harlow
    user checks chart tooltip item contains  ${datablock}  1  Unauthorised absence rate (2016/17): 1.1%
    user checks chart tooltip item contains  ${datablock}  2  Overall absence rate (2016/17): 4.2%
    user checks chart tooltip item contains  ${datablock}  3  Authorised absence rate (2016/17): 3.1%

    user checks map chart indicator tile contains  ${datablock}  1  Unauthorised absence rate (2016/17)  1.1%
    user checks map chart indicator tile contains  ${datablock}  2  Overall absence rate (2016/17)  4.2%
    user checks map chart indicator tile contains  ${datablock}  3  Authorised absence rate (2016/17)  3.1%

    user mouses over element  ${datablock} select[name="selectedLocation"]
    user selects from list by label  ${datablock} select[name="selectedLocation"]  Newham
    user waits until element does not contain chart tooltip  ${datablock}

    user mouses over selected map feature  ${datablock}
    user checks chart tooltip label contains  ${datablock}  Newham
    user checks chart tooltip item contains  ${datablock}  1  Unauthorised absence rate (2016/17): 1.7%
    user checks chart tooltip item contains  ${datablock}  2  Overall absence rate (2016/17): 4.4%
    user checks chart tooltip item contains  ${datablock}  3  Authorised absence rate (2016/17): 2.7%

    user checks map chart indicator tile contains  ${datablock}  1  Unauthorised absence rate (2016/17)  1.7%
    user checks map chart indicator tile contains  ${datablock}  2  Overall absence rate (2016/17)  4.4%
    user checks map chart indicator tile contains  ${datablock}  3  Authorised absence rate (2016/17)  2.7%

Clicking "Create tables" takes user to Table Tool page with absence publication selected
    [Documentation]  DFE-898
    [Tags]  HappyPath
    user clicks link    Create tables
    user waits until h1 is visible  Create your own tables online   60
    user waits for page to finish loading
    user checks radio is checked  Pupil absence in schools in England
    user clicks button  Next step

    user waits until page contains  Choose a subject   60
    user checks previous table tool step contains  1   Publication   Pupil absence in schools in England
