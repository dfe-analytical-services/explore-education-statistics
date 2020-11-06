*** Settings ***
Resource    ../libs/public-common.robot

Force Tags  GeneralPublic  Prod

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Go to Table Tool page
    [Tags]  HappyPath
    environment variable should be set  PUBLIC_URL
    user goes to url  %{PUBLIC_URL}/data-tables
    user waits until h1 is visible  Create your own tables online

Go to permalink
    [Tags]  HappyPath
    user goes to url  %{PUBLIC_URL}/data-tables/permalink/30999037-fcd4-409f-9ff4-d3680da7402d
    user waits until h1 is visible   'Total days missed due to fixed period exclusions' from 'Permanent and fixed-period exclusions in England'

Validate breadcrumbs
    [Tags]  HappyPath
    user checks breadcrumb count should be  4
    user checks nth breadcrumb contains  1   Home
    user checks nth breadcrumb contains  2   Data tables
    user checks nth breadcrumb contains  3   Permanent link
    user checks nth breadcrumb contains  4   'Total days missed due to fixed period exclusions' from 'Permanent and fixed-period exclusions in England'

Validate miscellaneous
    [Tags]  HappyPath
    user checks summary list contains  Created  7 April 2020
    user waits until element contains   css:[data-testid="dataTableCaption"]   Table showing Number of pupil enrolments for 'Total days missed due to fixed period exclusions' for State-funded secondary from 'Permanent and fixed-period exclusions in England' in England between 2014/15 and 2016/17
    user waits until page contains button    Print this page

Validate table
    [Tags]  HappyPath
    user checks table column heading contains  1   1   State-funded secondary
    user checks table column heading contains  2   1   2014/15
    user checks table column heading contains  2   2   2015/16
    user checks table column heading contains  2   3   2016/17

    user checks results table row heading contains   1   1   England
    user checks results table cell contains  1    1    124,995
    user checks results table cell contains  1    2    135,925
    user checks results table cell contains  1    3    148,820

Validate footnotes
    [Tags]  HappyPath
    user checks page contains element   xpath://h3[text()="Footnotes"]/../ol/li[text()="State-funded secondary schools include city technology colleges and all secondary academies, including all-through academies and free schools."]
    user checks page contains element   xpath://h3[text()="Footnotes"]/../ol/li[text()="x - 1 or 2 pupils, or a percentage based on 1 or 2."]

Validate download files
    [Tags]  HappyPath
    user checks page contains element  xpath://button[text()="Download the underlying data of this table (CSV)"]
    user checks page contains element  xpath://button[text()="Download table as Excel spreadsheet (XLSX)"]
    # TODO: More / Check CSV?

Use Create tables button
    [Tags]  HappyPath
    user waits until h2 is visible  Create your own tables online
    user clicks link    Create tables

    user waits until h1 is visible  Create your own tables online
