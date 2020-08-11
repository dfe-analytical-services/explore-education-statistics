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
    user waits until page contains heading 1  Create your own tables online

Go to permalink
    [Tags]  HappyPath
    user goes to url  %{PUBLIC_URL}/data-tables/permalink/edfe9f83-d1f0-40fc-8dce-9467a250c61b
    user waits until page contains heading 1   'Exclusions by characteristic' from 'Permanent and fixed-period exclusions in England'

Validate breadcrumbs
    [Tags]  HappyPath
    user checks element count is x  css:[data-testid="breadcrumbs--list"] li     4
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(1)   Home
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(2)   Data tables
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(3)   Permanent link
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(4)   'Exclusions by characteristic' from 'Permanent and fixed-period exclusions in England'

Validate miscellaneous
    [Tags]  HappyPath
    user checks summary list item "Created" should be "7 April 2020"
    user checks element contains   css:#dataTableCaption   Table showing 'Exclusions by characteristic' from 'Permanent and fixed-period exclusions in England' in England between 2013/14 and 2015/16
    user checks page contains element   xpath://a[text()="Print this page"]

Validate table
    [Tags]  HappyPath
    user checks results table column heading contains  css:table  1   1   2013/14
    user checks results table column heading contains  css:table  1   2   2014/15
    user checks results table column heading contains  css:table  1   3   2015/16

    ${row}=  user gets row with group and indicator   xpath://table  England   Number of fixed period exclusions
    user checks row contains heading  ${row}  Number of fixed period exclusions
    user checks row cell contains text  ${row}    1     269,475
    user checks row cell contains text  ${row}    2     302,975
    user checks row cell contains text  ${row}    3     339,360

    ${row}=  user gets row with group and indicator   xpath://table  England   Number of permanent exclusions
    user checks row contains heading  ${row}  Number of permanent exclusions
    user checks row cell contains text  ${row}    1     4,950
    user checks row cell contains text  ${row}    2     5,795
    user checks row cell contains text  ${row}    3     6,685

    ${row}=  user gets row with group and indicator   xpath://table  England   Number of pupils
    user checks row contains heading  ${row}  Number of pupils
    user checks row cell contains text  ${row}    1     7,698,310
    user checks row cell contains text  ${row}    2     7,799,005
    user checks row cell contains text  ${row}    3     7,916,225

Validate footnotes
    [Tags]  HappyPath
    user checks page contains element   xpath://h3[text()="Footnotes"]/../ol/li[text()="Includes pupils who are sole or dual main registrations. Includes boarding pupils."]
    user checks page contains element   xpath://h3[text()="Footnotes"]/../ol/li[text()="x - 1 or 2 pupils, or a percentage based on 1 or 2."]

Validate download files
    [Tags]  HappyPath
    user checks page contains element  xpath://button[text()="Download the underlying data of this table (CSV)"]
    user checks page contains element  xpath://button[text()="Download table as Excel spreadsheet (XLSX)"]
    # TODO: More / Check CSV?

Use Create tables button
    [Tags]  HappyPath
    user waits until page contains heading 2  Create your own tables online
    user clicks link    Create tables

    user waits until page contains heading 1  Create your own tables online
