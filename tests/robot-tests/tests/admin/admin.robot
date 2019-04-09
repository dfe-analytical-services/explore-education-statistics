*** Settings ***
Resource    ../libs/library.robot

Force Tags  Admin   UnderConstruction

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Verify Admin Page Loads
    [Tags]  HappyPath
    user goes to url  ${urlAdmin}
    user waits until page contains   Administrator dashboard

Go to edit page for current release of Pupil absence statistics
    [Tags]  HappyPath
    user clicks button  Absence and exclusions
    element attribute value should be  css:#schools-heading-1   aria-expanded   true
    user clicks element  xpath://div[@id='schools-content-1']//li[1]//a[contains(text(), 'Edit current release')]
    user waits until page contains  Edit pupil absence statistics

Validate "Click to edit" tags are appearing
    [Tags]  HappyPath
    ${count}=   get element count   xpath://*[contains(text(), "Click to edit")]
    should be equal as integers  ${count}   15

Edit headline
    [Tags]  HappyPath
    user clicks element  xpath://h1[contains(text(),"Pupil absence statistics and data for schools in England")]
    user deletes text until block is empty   xpath://h2[contains(text(),"Pupil absence statistics and data for schools in England")]
    user presses keys    abcdefghijklmopqrstuvwxyz

    # Do stuff with the toolbar...
    user clicks element  css:.ck-block-toolbar-button
#    user clicks element   css:.ck-file-dialog-button  # Upload file
#    user clicks element  css:.ck-dropdown__button:nth-child(1)  # Change text size

    user clicks element  xpath://button[contains(text(), "Save")]
    user checks element contains  css:h2  abcdefghijklmopqrstuvwxyz


