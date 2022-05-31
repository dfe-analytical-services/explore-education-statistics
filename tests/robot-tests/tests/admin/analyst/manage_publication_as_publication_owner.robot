*** Settings ***
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot
Resource            ../../libs/admin/manage-content-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    AltersData


*** Variables ***
${PUBLICATION_NAME}=    UI tests - manage publication as publication owner %{RUN_IDENTIFIER}


*** Test Cases ***
Create Publication as bau1
    user creates test publication via api    ${PUBLICATION_NAME}
    ${accordion}=    user opens publication on the admin dashboard    ${PUBLICATION_NAME}

Assign publication owner permissions to analyst1
    user gives analyst publication owner access    ${PUBLICATION_NAME}

Switch to analyst1
    user changes to analyst1

Go to Manage publication page
    ${accordion}=    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    user clicks link    Manage publication    ${accordion}
    user waits until page contains title    Manage publication

Update publication
    user enters text into element    id:publicationForm-teamName    UI test team name updated
    user enters text into element    id:publicationForm-teamEmail    ui_test_updated@test.com
    user enters text into element    id:publicationForm-contactName    UI test contact name updated
    user enters text into element    id:publicationForm-contactTelNo    4321 4321

    user checks page does not contain element    id:publicationForm-title    # Only BAU users should see this
    user checks page does not contain element    id:publicationForm-supersede    # Only BAU users should see this

    user clicks button    Save publication
    user clicks button    Confirm
    user waits until page does not contain button    Confirm

Check publication is updated on dashboard
    ${accordion}=    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    user waits until page contains element    testid:Team name for ${PUBLICATION_NAME}
    user checks element contains child element    ${accordion}
    ...    xpath://*[@data-testid="Team name for ${PUBLICATION_NAME}" and text()="UI test team name updated"]
    user checks element contains child element    ${accordion}
    ...    xpath://*[@data-testid="Team email for ${PUBLICATION_NAME}" and text()="ui_test_updated@test.com"]
    user checks element contains child element    ${accordion}
    ...    xpath://*[@data-testid="Contact name for ${PUBLICATION_NAME}" and text()="UI test contact name updated"]
    user checks element contains child element    ${accordion}
    ...    xpath://*[@data-testid="Contact phone number for ${PUBLICATION_NAME}" and text()="4321 4321"]
