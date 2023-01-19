*** Settings ***
Resource    ./common.robot


*** Keywords ***
user waits until table is visible
    [Arguments]    ${parent}=css:body    ${wait}=${timeout}
    user waits until parent contains element    ${parent}    xpath:.//table    timeout=${wait}

user checks table column heading contains
    [Arguments]    ${row}    ${column}    ${expected}    ${parent}=css:table    ${wait}=%{WAIT_SMALL}
    user waits until parent contains element    ${parent}
    ...    xpath://thead/tr[${row}]/th[${column}][text()="${expected}"]
    ...    timeout=${wait}

user checks row contains heading
    [Arguments]    ${row_elem}    ${heading}
    user waits until parent contains element    ${row_elem}    xpath:.//th[text()="${heading}"]
    ...    error=Heading ${heading} not found for provided row element

user checks table row heading contains
    [Arguments]    ${row}    ${column}    ${expected}
    user waits until element contains    xpath://table/tbody/tr[${row}]/th[${column}]    ${expected}

user checks table cell contains
    [Arguments]    ${row}    ${column}    ${expected}    ${parent}=css:table
    user waits until parent contains element    ${parent}
    ...    xpath:.//tbody/tr[${row}]/td[${column}][contains(., "${expected}")]

user checks table cell contains element
    [Arguments]    ${row}    ${column}    ${element}    ${parent}=css:table
    user waits until parent contains element    ${parent}    xpath:.//tbody/tr[${row}]/td[${column}]
    ${cell}=    get child element    ${parent}    xpath:.//tbody/tr[${row}]/td[${column}]
    user waits until parent contains element    ${cell}    ${element}

user checks table heading in offset row contains
    [Arguments]    ${row}    ${offset}    ${column}    ${expected}    ${parent}=css:table
    ${offset_row}=    evaluate    int(${row}) + int(${offset})
    ${elem}=    get child element    ${parent}    xpath://tbody/tr[${offset_row}]/th[${column}]
    wait until element contains    ${elem}    ${expected}
    ...    error="${expected}" not found in th tag in results table tbody row ${offset_row}, column ${column}.

user checks table cell in offset row contains
    [Arguments]    ${row}    ${offset}    ${column}    ${expected}    ${parent}=css:table
    ${offset_row}=    evaluate    int(${row}) + int(${offset})
    ${elem}=    get child element    ${parent}    xpath://tbody/tr[${offset_row}]/td[${column}]
    wait until element contains    ${elem}    ${expected}
    ...    error="${expected}" not found in td tag in results table tbody row ${offset_row}, column ${column}.

user gets table row with heading
    [Arguments]    ${heading}    ${parent}=css:table
    ${elem}=    get child element    ${parent}    xpath:.//tbody/tr/th[text()="${heading}"]/..
    [Return]    ${elem}

user gets table row
    [Arguments]    ${row_cell_text}    ${parent}=css:table
    ${elem}=    get child element    ${parent}    xpath:.//tbody/tr/td[text()="${row_cell_text}"]/..
    [Return]    ${elem}

user checks table body has x rows
    [Arguments]    ${count}    ${parent}=css:table    ${wait}=${timeout}
    user waits until parent contains element    ${parent}    xpath:.//tbody/tr    timeout=${wait}    count=${count}

user checks headed table body row contains
    [Arguments]    ${row_heading}    ${content}    ${parent}=css:table    ${wait}=${timeout}
    user waits until parent contains element    ${parent}
    ...    xpath:.//tbody/tr/th[text()="${row_heading}"]/../td[contains(., "${content}")]    timeout=${wait}

user checks headed table body row cell contains
    [Arguments]    ${row_heading}    ${cell}    ${content}    ${parent}=css:table    ${wait}=${timeout}
    user waits until parent contains element    ${parent}
    ...    xpath:.//tbody/tr/th[text()="${row_heading}"]/../td[${cell}][contains(., "${content}")]    timeout=${wait}

user clicks link in table cell
    [Arguments]    ${row}    ${column}    ${link_text}    ${parent}=css:table
    user waits until parent contains element    ${parent}
    ...    xpath:.//tbody/tr[${row}]/td[${column}]/a[contains(., "${link_text}")]
    ${link}=    get child element
    ...    ${parent}
    ...    xpath:.//tbody/tr[${row}]/td[${column}]/a[contains(., "${link_text}")]
    user clicks element    ${link}

user clicks button in table cell
    [Arguments]    ${row}    ${column}    ${button_text}    ${parent}=css:table
    user waits until parent contains element    ${parent}
    ...    xpath:.//tbody/tr[${row}]/td[${column}]/button[contains(., "${button_text}")]
    ${button}=    get child element
    ...    ${parent}
    ...    xpath:.//tbody/tr[${row}]/td[${column}]/button[contains(., "${button_text}")]
    user clicks element    ${button}
