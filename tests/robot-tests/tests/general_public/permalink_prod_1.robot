*** Settings ***
Resource            ../libs/public-common.robot

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          GeneralPublic    Prod


*** Test Cases ***
Go to Table Tool page
    user navigates to data tables page on public frontend

Go to permalink
    user navigates to public frontend    %{PUBLIC_URL}/data-tables/permalink/30999037-fcd4-409f-9ff4-d3680da7402d
    user waits until h1 is visible
    ...    'Total days missed due to fixed period exclusions' from 'Permanent and fixed-period exclusions in England'

Validate breadcrumbs
    user checks breadcrumb count should be    4
    user checks nth breadcrumb contains    1    Home
    user checks nth breadcrumb contains    2    Data tables
    user checks nth breadcrumb contains    3    Permanent link
    user checks nth breadcrumb contains    4
    ...    'Total days missed due to fixed period exclusions' from 'Permanent and fixed-period exclusions in England'

Validate miscellaneous
    user checks summary list contains    Created    7 April 2020
    user waits until element contains    testid:dataTableCaption
    ...    Number of pupil enrolments for 'Total days missed due to fixed period exclusions' for State-funded secondary in England between 2014/15 and 2016/17
    user waits until page contains button    Print this page

Validate table
    user checks table column heading contains    1    1    State-funded secondary
    user checks table column heading contains    2    1    2014/15
    user checks table column heading contains    2    2    2015/16
    user checks table column heading contains    2    3    2016/17

    user checks table row heading contains    1    1    England
    user checks table cell contains    1    1    124,995
    user checks table cell contains    1    2    135,925
    user checks table cell contains    1    3    148,820

Validate footnotes
    user checks page contains element
    ...    xpath://h3[text()="Footnotes"]/../ol/li[text()="State-funded secondary schools include city technology colleges and all secondary academies, including all-through academies and free schools."]
    user checks page contains element
    ...    xpath://h3[text()="Footnotes"]/../ol/li[text()="x - 1 or 2 pupils, or a percentage based on 1 or 2."]

Validate download files
    user checks page contains    Table in ODS format (spreadsheet, with title and footnotes)
    user checks page contains    Table in CSV format (flat file, with location codes)
    user checks page contains element    xpath://button[text()="Download table"]
    # TODO: More / Check CSV?

Use Create tables button
    user waits until h2 is visible    Create your own tables
    user clicks link    Create tables
    user waits until h1 is visible    Create your own tables
