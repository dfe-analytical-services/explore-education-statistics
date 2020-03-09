*** Settings ***
Resource    ../libs/public-common.robot

Force Tags  GeneralPublic  Local  Dev  Test

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

    user opens accordion section  Pupils and schools
    user opens details dropdown   Pupil absence
    user clicks element   css:[data-testid="view-stats-pupil-absence-in-schools-in-england"]
    user waits until page contains element   xpath://h1[text()="Pupil absence in schools in England"]

Validate URL
    [Documentation]  DFE-325
    [Tags]  HappyPath
    user checks url contains  %{PUBLIC_URL}/find-statistics/pupil-absence-in-schools-in-england

Validate Published date, Next update date, and Email alerts link
    [Tags]     HappyPath
    user checks element contains  css:[data-testid="published-date"]   22 March 2018
    user checks element contains  css:[data-testid="next-update"]      March 2019
    user checks page contains link with text and url  Sign up for email alerts    /subscriptions?slug=pupil-absence-in-schools-in-england

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

Validate Key Statistics data block -- Summary tab
    [Documentation]  DFE-915   EES-806
    [Tags]  HappyPath
    #user waits until page contains element   css:#keystats-summary    90
    user waits until page contains element   xpath://h3[text()="Overall absence rate"]    90
    user waits until page contains element   xpath://h3[text()="Authorised absence rate"]    90
    user waits until page contains element   xpath://h3[text()="Unauthorised absence rate"]    90

    user checks key stat tile contents   Overall absence rate         4.6%   Up from 4.6% in 2015/16
    user checks key stat tile contents   Authorised absence rate      3.5%   Similar to previous years
    user checks key stat tile contents   Unauthorised absence rate    1.1%   Up from 1.1% in 2015/16

    #user checks key stat bullet exists   pupils missed on average 8.2 school days
    #user checks key stat bullet exists   overall and unauthorised absence rates up on 2015/16
    #user checks key stat bullet exists   unauthorised absence rise due to higher rates of unauthorised holidays
    #user checks key stat bullet exists   10% of pupils persistently absent during 2016/17

Validate Key Statistics data block -- Data tables tab
   [Tags]  HappyPath    Failing
   user clicks element   css:#headlines-section-2-tab   # Click Table tab
   user checks element contains   css:#dataTableCaption    Table showing 'Absence by characteristic' from 'Pupil absence in schools in England' in England between 2012/13 and 2016/17

   user checks results table column heading contains  css:table  1   1   2012/13
   user checks results table column heading contains  css:table  1   2   2013/14
   user checks results table column heading contains  css:table  1   3   2014/15
   user checks results table column heading contains  css:table  1   4   2015/16
   user checks results table column heading contains  css:table  1   5   2016/17

   ${row}=  user gets row with group and indicator  css:#headlines-section-2 table   England   Authorised absence rate
   user checks row contains heading  ${row}   Authorised absence rate
   user checks row cell contains text  ${row}   1    4.2
   user checks row cell contains text  ${row}   2    3.5
   user checks row cell contains text  ${row}   3    3.5
   user checks row cell contains text  ${row}   4    3.4
   user checks row cell contains text  ${row}   5    3.4

   ${row}=  user gets row with group and indicator  css:#headlines-section-2 table   England   Unauthorised absence rateh
   user checks row contains heading  ${row}   Unauthorised absence rate
   user checks row cell contains text  ${row}   1    1.1
   user checks row cell contains text  ${row}   2    1.1
   user checks row cell contains text  ${row}   3    1.1
   user checks row cell contains text  ${row}   4    1.1
   user checks row cell contains text  ${row}   5    1.3

   ${row}=  user gets row with group and indicator  css:#headlines-section-2 table   England   Overall absence rateh
   user checks row contains heading  ${row}   Overall absence rate
   user checks row cell contains text  ${row}   1    5.3
   user checks row cell contains text  ${row}   2    4.5
   user checks row cell contains text  ${row}   3    4.6
   user checks row cell contains text  ${row}   4    4.6
   user checks row cell contains text  ${row}   5    4.7

Validate Key Statistics data block -- Charts tab
   [Tags]  HappyPath
   user clicks element   css:#headlines-section-3-tab   # Click Chart tab
   user checks element is visible  css:.recharts-responsive-container
   # TODO: Possible to verify chart?

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
    [Tags]  HappyPath
    user opens accordion section  Regional and local authority (LA) breakdown
    user checks element contains  css:#content_9_datablock-tables #dataTableCaption    Table showing 'Absence by characteristic' from 'Pupil absence in schools in England' in

    user checks results table column heading contains  css:#content_9_datablock-tables table   1   1   2016/17

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

    #TODO: More

Clicking "Create tables" takes user to Table Tool page with absence publication selected
    [Documentation]  DFE-898
    [Tags]  HappyPath
    user clicks link    Create tables
    user waits until page contains  Create your own tables online

    user clicks element   css:#publicationForm-submit

    user waits until page contains  Choose a subject
    user checks previous table tool step contains  1   Publication   Pupil absence in schools in England
