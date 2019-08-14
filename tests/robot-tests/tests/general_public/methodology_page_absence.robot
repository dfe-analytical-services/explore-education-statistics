*** Settings ***
Resource    ../libs/library.robot

Force Tags  GeneralPublic

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Navigate to Pupil absence in schools in England methodology page
    [Tags]  HappyPath
    environment variable should be set  PUBLIC_URL
    user goes to url   %{PUBLIC_URL}/methodology
    user waits until page contains heading   Methodologies

    user opens accordion section  Pupils and schools
    user opens details dropdown   Pupil absence
    user clicks link    Pupil absence statistics: methodology
    user waits until page contains heading   Pupil absence statistics: methodology

Validate Published date, Last updated date
    [Tags]  HappyPath
    user checks element contains   css:[data-testid="published-date"]   22 March 2018
    user checks element contains   css:[data-testid="last-updated"]     26 June 2019

Validate accordion sections order
    [Tags]  HappyPath
    user checks accordion is in position  1. Overview of absence statistics         1
    user checks accordion is in position  2. National Statistics badging            2
    user checks accordion is in position  3. Methodology                            3
    user checks accordion is in position  4. Data collection                        4
    user checks accordion is in position  5. Data processing                        5
    user checks accordion is in position  6. Data quality                           6
    user checks accordion is in position  7. Contacts                               7

    user checks accordion is in position  Annex A - Calculations                    8
    user checks accordion is in position  Annex B - School attendance codes         9
    user checks accordion is in position  Annex C - Links to pupil absence national statistics and data   10
    user checks accordion is in position  Annex D - Standard breakdowns             11
    user checks accordion is in position  Annex E - Timeline                        12
    user checks accordion is in position  Annex F - Absence rates over time         13

Validate page has Print this page link
    [Tags]  HappyPath
    user checks page contains link with text and url  Print this page   \#

Validate search works
    [Tags]  HappyPath
    user clicks element   css:#pageSearchForm-input
    user presses keys    number of pupil enrolments
    user checks element contains  css:#pageSearchForm-resultsLabel   Found 1 result
    user clicks element   css:#pageSearchForm-option-0 div
