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
    user navigates to public frontend    %{PUBLIC_URL}/data-tables/permalink/edfe9f83-d1f0-40fc-8dce-9467a250c61b
    user waits until h1 is visible
    ...    'Exclusions by characteristic' from 'Permanent and fixed-period exclusions in England'

Validate breadcrumbs
    user checks breadcrumb count should be    4
    user checks nth breadcrumb contains    1    Home
    user checks nth breadcrumb contains    2    Data tables
    user checks nth breadcrumb contains    3    Permanent link
    user checks nth breadcrumb contains    4
    ...    'Exclusions by characteristic' from 'Permanent and fixed-period exclusions in England'

Validate miscellaneous
    user checks summary list contains    Created    7 April 2020
    user waits until element contains    testid:dataTableCaption
    ...    'Exclusions by characteristic' in England between 2013/14 and 2015/16
    user waits until page contains button    Print this page

Validate table
    user checks table column heading contains    1    1    2013/14
    user checks table column heading contains    1    2    2014/15
    user checks table column heading contains    1    3    2015/16

    ${row}=    user gets row with group and indicator    England    Number of fixed period exclusions
    user checks row contains heading    ${row}    Number of fixed period exclusions
    user checks row cell contains text    ${row}    1    269,475
    user checks row cell contains text    ${row}    2    302,975
    user checks row cell contains text    ${row}    3    339,360

    ${row}=    user gets row with group and indicator    England    Number of permanent exclusions
    user checks row contains heading    ${row}    Number of permanent exclusions
    user checks row cell contains text    ${row}    1    4,950
    user checks row cell contains text    ${row}    2    5,795
    user checks row cell contains text    ${row}    3    6,685

    ${row}=    user gets row with group and indicator    England    Number of pupils
    user checks row contains heading    ${row}    Number of pupils
    user checks row cell contains text    ${row}    1    7,698,310
    user checks row cell contains text    ${row}    2    7,799,005
    user checks row cell contains text    ${row}    3    7,916,225

Validate footnotes
    user checks page contains element
    ...    xpath://h3[text()="Footnotes"]/../ol/li[text()="Includes pupils who are sole or dual main registrations. Includes boarding pupils."]
    user checks page contains element
    ...    xpath://h3[text()="Footnotes"]/../ol/li[text()="x - 1 or 2 pupils, or a percentage based on 1 or 2."]

Validate download files
    user checks page contains    Table in ODS format (spreadsheet, with title and footnotes)
    user checks page contains    Table in CSV format (flat file, with location codes)
    user checks page contains element    xpath://button[text()="Download table"]
    # TODO: More / Check CSV?

Use Create tables button
    user navigates to data tables page on public frontend
