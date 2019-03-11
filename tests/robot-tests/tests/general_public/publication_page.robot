*** Settings ***
Documentation   DFE-115 Setup automated testing framework
...             DFE-98 General Public - 'Where Does The Data Come From?'
...             DFE-100 General Public - See All (4) Updates
...             DFE-101 General Public - See Previous Year/s
...             DFE-102 General Public - Getting the data
...             DFE-103 General Public - Publication Page

Resource    ../libs/library.robot

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Validate Schools page displays links to publications
    [Tags]  HappyPath   Failing
    user goes to url  ${url}
    user clicks element  css:[data-testid="home-page--themes-link"]
    user clicks element  css:[data-testid="content-item-list--schools"]

    user waits until page contains element  css:[data-testid="content-item-list--element"]
    css should match x times                css:[data-testid="content-item-list--element"]  6

Validate that Pupil Absence in Schools in England page is for latest data
    [Tags]  HappyPath   Failing
    user clicks element                 css:[data-testid="content-item-list--absence-and-exclusions"]
    user clicks element                 css:[data-testid="content-item-list--pupil-absence-in-schools-in-england"]
    user checks page contains element   css:[data-testid="publication-page--latest-data-heading"]
    user checks element should contain  css:[data-testid="publication-page--release-name"]  (latest data)

Last Updated list should display API data
    [Documentation]  DFE-100
    [Tags]  HappyPath   Failing
    user waits until element contains           css:[data-testid="publication-page--last-updated"] summary   See all 2 updates
    user checks page does not contain element   css:[data-testid="publication-page--last-updated"] details[open]

    user clicks element                 css:[data-testid="publication-page--last-updated"] [data-testid="details--expand"]
    user checks page contains element   css:[data-testid="publication-page--last-updated"] details[open]

    css should match x times        css:[data-testid="publication-page--update-element"]   2
    user checks element contains    css:[data-testid="publication-page--update-element"]:nth-child(1) time   19 April 2017
    user checks element contains    css:[data-testid="publication-page--update-element"]:nth-child(2) time   22 March 2017
    user checks element contains    css:[data-testid="publication-page--update-element"]:nth-child(2) p      First published.

Validate "Where does this data come from?" is on the page
    [Documentation]  DFE-98
    [Tags]  HappyPath  Failing
    # NOTE: the step-by-step is being changed, so cursory test only
    user checks element contains  css:[data-testid="step-by-step"]:nth-child(2) [data-testid="step-by-step--title"]   Where does this data come from?

Release Name list should display API data
    [Documentation]  DFE-101
    [Tags]  HappyPath  Failing
    user checks element contains                css:[data-testid="publication-page--release-name"] [data-testid="details--expand"]      See previous 7 releases
    user checks page does not contain element   css:[data-testid="publication-page--release-name"] details[open]

    user clicks element                 css:[data-testid="publication-page--release-name"] [data-testid="details--expand"]
    user checks page contains element   css:[data-testid="publication-page--release-name"] details[open]

    css should match x times        css:[data-testid="publication-page--release-name-list"] li   7

    user checks element contains                    css:[data-testid="publication-page--release-name-list"] li:nth-child(1)    2015 to 2016
    user checks element attribute value should be   css:[data-testid="publication-page--release-name-list"] li:nth-child(1) a  href     ${url}/themes/schools/absence-and-exclusions/pupil-absence-in-schools-in-england/2015-16

    user checks element contains                    css:[data-testid="publication-page--release-name-list"] li:nth-child(7)    2009 to 2010
    user checks element attribute value should be   css:[data-testid="publication-page--release-name-list"] li:nth-child(7) a  href    https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics-academic-year-2009-to-2010

Download CSV Zip and validate containing files
    [Documentation]  DFE-102
    [Tags]  HappyPath  Failing
    ${csv_zip_link} =   get element attribute   css:[data-testid="publication-page--download-csvs"]     href
    download file  ${csv_zip_link}  publication_page_csvs.zip
    zip should contain file  publication_page_csvs.zip    absence_geoglevels.csv
    zip should contain file  publication_page_csvs.zip    absence_lacharacteristics.csv
    zip should contain file  publication_page_csvs.zip    absence_natcharacteristics.csv

Validate 2015/2016 isn't labelled as latest data
    [Tags]  HappyPath  Failing
    user clicks link  2015 to 2016
    user checks page does not contain element   css:[data-testid="publication-page--latest-data-heading"]
    user checks element should not contain      css:[data-testid="publication-page--release-name"]  (latest data)

