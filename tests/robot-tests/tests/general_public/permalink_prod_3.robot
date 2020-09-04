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
    user goes to url  %{PUBLIC_URL}/data-tables/permalink/c5688b39-2630-4de0-a143-725df9e48690
    user waits until h1 is visible    'Exclusions by geographic level' from 'Permanent and fixed-period exclusions in England'

Validate breadcrumbs
    [Tags]  HappyPath
    user checks breadcrumb count should be  4
    user checks nth breadcrumb contains  1   Home
    user checks nth breadcrumb contains  2   Data tables
    user checks nth breadcrumb contains  3   Permanent link
    user checks nth breadcrumb contains  4   'Exclusions by geographic level' from 'Permanent and fixed-period exclusions in England'

Validate miscellaneous
    [Tags]  HappyPath
    user checks summary list contains  Created  7 April 2020
    user waits until element contains   css:#dataTableCaption   Table showing 'Exclusions by geographic level' from 'Permanent and fixed-period exclusions in England' in Barking and Dagenham and Barnet between 2014/15 and 2016/17
    user checks page contains element   xpath://a[text()="Print this page"]

Validate table
    [Tags]  HappyPath
    user checks table column heading contains  css:table  1   1   2014/15
    user checks table column heading contains  css:table  1   2   2015/16
    user checks table column heading contains  css:table  1   3   2016/17

    ${row}=  user gets row with group and indicator   xpath://table  Barnet   Fixed period exclusion rate
    user checks row contains heading  ${row}  	Fixed period exclusion rate
    user checks row cell contains text  ${row}    1     2.75%
    user checks row cell contains text  ${row}    2     2.83%
    user checks row cell contains text  ${row}    3     3.2%

    ${row}=  user gets row with group and indicator   xpath://table  Barnet   Number of fixed period exclusions
    user checks row contains heading  ${row}  	Number of fixed period exclusions
    user checks row cell contains text  ${row}    1     1,496
    user checks row cell contains text  ${row}    2     1,580
    user checks row cell contains text  ${row}    3     1,825

    ${row}=  user gets row with group and indicator   xpath://table  Barking and Dagenham   Fixed period exclusion rate
    user checks row contains heading  ${row}  	Fixed period exclusion rate
    user checks row cell contains text  ${row}    1     2.02%
    user checks row cell contains text  ${row}    2     1.94%
    user checks row cell contains text  ${row}    3     1.87%

    ${row}=  user gets row with group and indicator   xpath://table  Barking and Dagenham   Number of fixed period exclusions
    user checks row contains heading  ${row}  	Number of fixed period exclusions
    user checks row cell contains text  ${row}    1     805
    user checks row cell contains text  ${row}    2     810
    user checks row cell contains text  ${row}    3     798

Validate footnotes
    [Tags]  HappyPath
    user checks page contains element   xpath://h3[text()="Footnotes"]/../ol/li[text()="The number of fixed period exclusions expressed as a percentage of the number of pupils in January each year."]
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
