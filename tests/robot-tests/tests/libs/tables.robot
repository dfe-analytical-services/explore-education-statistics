*** Settings ***
Resource  ./common.robot

*** Keywords ***
user checks table column heading contains
    [Arguments]  ${table_selector}  ${row}   ${column}   ${expected}   ${wait}=30
    user waits until parent contains element  ${table_selector}
    ...  xpath://thead/tr[${row}]/th[${column}][text()="${expected}"]
    ...  timeout=${wait}

user checks row contains heading
    [Arguments]  ${row_elem}   ${heading}
    user waits until parent contains element  ${row_elem}   xpath:.//th[text()="${heading}"]
    ...  error=Heading ${heading} not found for provided row element

user checks results table row heading contains
    [Arguments]   ${row}  ${column}  ${expected}
    user waits until element contains  xpath://table/tbody/tr[${row}]/th[${column}]   ${expected}

user checks results table cell contains
    [Arguments]   ${row}  ${column}  ${expected}
    user waits until element contains  xpath://table/tbody/tr[${row}]/td[${column}]   ${expected}

user checks table heading in offset row contains
    [Arguments]  ${table_locator}   ${row}   ${offset}   ${column}   ${expected}
    ${offset_row}=  evaluate  int(${row}) + int(${offset})
    ${elem}  get child element  ${table_locator}   xpath://tbody/tr[${offset_row}]/th[${column}]
    wait until element contains  ${elem}   ${expected}
    ...  error="${expected}" not found in th tag in results table tbody row ${offset_row}, column ${column}.

user checks table cell in offset row contains
    [Arguments]  ${table_locator}   ${row}   ${offset}   ${column}   ${expected}
    ${offset_row}=  evaluate  int(${row}) + int(${offset})
    ${elem}  get child element  ${table_locator}   xpath://tbody/tr[${offset_row}]/td[${column}]
    wait until element contains  ${elem}   ${expected}
    ...  error="${expected}" not found in td tag in results table tbody row ${offset_row}, column ${column}.

user gets table row with heading
    [Arguments]   ${table_selector}  ${heading}
    ${elem}=   get child element  ${table_selector}  xpath:.//tbody/tr/th[text()="${heading}"]/..
    [Return]   ${elem}

user checks headed table body row contains
    [Arguments]  ${row_heading}  ${content}  ${parent}=css:body  ${wait}=30
    user waits until parent contains element  ${parent}  xpath:.//tbody/tr/th[text()="${row_heading}"]/../td[contains(., "${content}")]  timeout=${wait}
