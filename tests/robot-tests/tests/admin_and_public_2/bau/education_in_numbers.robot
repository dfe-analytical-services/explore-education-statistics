*** Settings ***
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    AltersData


*** Test Cases ***
Remove UI test page if exists via API
    user removes ein page if exists    UI test page

Go to EiN management page
    user clicks link    Platform administration
    user waits until h1 is visible    Platform administration

    user clicks link    Manage Education in Numbers
    user waits until page contains element    testid:education-in-numbers-table

    user checks page does not contain    UI test page

Add new page "UI test page"
    user clicks link    Add new page
    user waits until h1 is visible    Create a new Education in Numbers page

    user enters text into element    css:#educationInNumbersSummaryForm-title    UI test page
    user enters text into element    css:#educationInNumbersSummaryForm-description    UI test page description

    user clicks button    Create page

    user waits until h2 is visible    Page summary

Validate page appears in EiN page table
    user clicks link    Manage Education in Numbers
    user waits until h1 is visible    Education in Numbers pages

    ${ROW}=    user gets table row    UI test page    testid:education-in-numbers-table
    user checks row cell contains text    ${ROW}    2    ui-test-page
    user checks row cell contains text    ${ROW}    3    Draft
    user checks row cell contains text    ${ROW}    4    Not yet published
    user checks row cell contains text    ${ROW}    5    0
    user checks row cell contains text    ${ROW}    6    Edit
    user checks row cell contains text    ${ROW}    6    Delete

    user clicks link    Edit    ${ROW}
    user waits until h2 is visible    Page summary

Validate page summary
    user checks summary list contains    Title    UI test page    testid:summary-list
    user checks summary list contains    Slug    ui-test-page    testid:summary-list
    user checks summary list contains    Description    UI test page description    testid:summary-list
    user checks summary list contains    Status    Draft    testid:summary-list
    user checks summary list contains    Published on    Not yet published    testid:summary-list

Edit page summary
#Validate updated page summary

#Add some content

#Validate content preview

#Publish page

#Check page appears on public site

#Amend page

#Validate amendment summary

#Validate amendment content

#Update content

#Publish amendment

#Check amendment on public site
