*** Settings ***
Resource    ../libs/library.robot

Force Tags  Admin       UnderConstruction

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
    user clicks element child containing text  css:#schools-content-1 li:nth-child(1)    Edit current release
    user waits until page contains  Edit pupil absence statistics

Validate "Click to edit" tags are appearing
    [Tags]  HappyPath
    elements containing text should match x times  Click to edit      15

Edit headline
    [Tags]  HappyPath
    user clicks element containing text  Pupil absence statistics and data for schools in England
    user deletes text from element until block is empty  css:h2  Pupil absence statistics and data for schools in England
    user presses keys    abcdefghijklmopqrstuvwxyz
    italic x characters before cursor  5
    insert image
    sleep   10
#
#    # Do stuff with the toolbar...
#    user clicks element  css:.ck-block-toolbar-button
##    user clicks element   css:.ck-file-dialog-button  # Upload file
##    user clicks element  css:.ck-dropdown__button:nth-child(1)  # Change text size
#
#    user clicks element  xpath://button[contains(text(), "Save")]
#    user checks element contains  css:h2  abcdefghijklmopqrstuvwxyz
#

