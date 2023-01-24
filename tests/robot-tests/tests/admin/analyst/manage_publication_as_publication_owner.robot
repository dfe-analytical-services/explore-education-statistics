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
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    Set suite variable    ${PUBLICATION_ID}

Check that no publication roles are listed yet on the Team access page
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user waits until page contains link    Team access
    user clicks link    Team access
    user waits until page contains    There are no publication owners or approvers.

Assign publication owner permissions to analyst1
    user adds publication role to user via api
    ...    EES-test.ANALYST1@education.gov.uk
    ...    ${PUBLICATION_ID}
    ...    Owner

Sign in as analyst1 and check that the Team access page now contains the Publication Owner details
    user signs in as analyst1
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user clicks link    Team access
    user waits until page contains    There are no publication approvers
    user checks table column heading contains    1    1    Name    testid:publicationRoles
    user checks table column heading contains    1    2    Email    testid:publicationRoles
    user checks table column heading contains    1    3    Publication role    testid:publicationRoles
    user checks table body has x rows    1    testid:publicationRoles
    user checks table cell contains    1    1    Analyst1 User1    testid:publicationRoles
    user checks table cell contains    1    2    ees-test.analyst1@education.gov.uk    testid:publicationRoles
    user checks table cell contains    1    3    Owner    testid:publicationRoles

Update publication contact
    user clicks link    Contact
    user waits until h2 is visible    Contact for this publication

    user clicks button    Edit contact details
    user waits until page contains element    id:publicationContactForm-teamName

    user enters text into element    id:publicationContactForm-teamName    UI test team name updated
    user enters text into element    id:publicationContactForm-teamEmail    ui_test_updated@test.com
    user enters text into element    id:publicationContactForm-contactName    UI test contact name updated
    user enters text into element    id:publicationContactForm-contactTelNo    4321 4321

    user clicks button    Update contact details
    ${modal}=    user waits until modal is visible    Confirm contact changes
    user clicks button    Confirm    ${modal}

    user checks summary list contains    Team name    UI test team name updated
    user checks summary list contains    Team email    ui_test_updated@test.com
    user checks summary list contains    Contact name    UI test contact name updated
    user checks summary list contains    Contact telephone    4321 4321

Update publication details
    user clicks link    Details
    user waits until h2 is visible    Publication details

    user clicks button    Edit publication details

    # Only BAU should see title
    user checks page does not contain element    id:publicationDetailsForm-title

    user waits until page contains element    id:publicationDetailsForm-summary

    # Only BAU should see theme and topic
    user checks page does not contain element    id:publicationDetailsForm-themeId
    user checks page does not contain element    id:publicationDetailsForm-topicId

    # Only BAU users should see supersededById
    user checks page does not contain element    id:publicationDetailsForm-supersededById

    user enters text into element    id:publicationDetailsForm-summary    UI test publication summary

    user clicks button    Update publication details
    ${modal}=    user waits until modal is visible    Confirm publication changes
    user clicks button    Confirm    ${modal}

    user checks summary list contains    Publication title    ${PUBLICATION_NAME}
    user checks summary list contains    Publication summary    UI test publication summary
    user checks summary list contains    Theme    %{TEST_THEME_NAME}
    user checks summary list contains    Topic    %{TEST_TOPIC_NAME}
    user checks summary list contains    Superseding publication    This publication is not archived
