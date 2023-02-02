*** Comments ***
#
# This test suite is responsible for setting up various Publications and Releases for the use of the Publication
# and Release Permissions UI tests.
#
# Releases are created in each approval state and a different role is assigned to Analyst1 for each. This way,
# there is a unique Release for each combination of Release approval status available and Release / Publication role.
#

#
# When seeding data against different environments, you will need to remove the
# BootstrapData tag from the 'Force Tags' settings as this will prevent it from
# being run against a given enironment.
#

#
# If you see a consistent error such as 'Parent 'css:body' did not contain 'link:UI tests - Publication and Release UI Permissions Publication Owner' in 1 minute 40 seconds.'
# then you will need to run this test suite against the environment you are testing against in order to seed the environment so that other tests
# that depend on this test data don't fail.
#


*** Settings ***
Library             ../libs/admin_api.py
Resource            ../libs/bootstrap_data/bootstrap_common.robot
Resource            ../libs/bootstrap_data/bootstrap_data_constants.robot
Resource            ../libs/admin-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          BootstrapData    Local    Dev


*** Test Cases ***
Create test theme and topic
    Import bootstrap data roles and permissions variables
    ${THEME_ID}=    user creates theme via api    ${THEME_NAME}
    ${TOPIC_ID}=    user creates topic via api    ${TOPIC_NAME}    ${THEME_ID}
    user reloads page
    Set Suite Variable    ${TOPIC_ID}

Create new publications and published releases - for Publication Owner
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_FOR_PUBLICATION_OWNER}    ${TOPIC_ID}
    user creates releases in all states for publication    ${PUBLICATION_ID}    ${PUBLICATION_FOR_PUBLICATION_OWNER}
    user gives analyst publication owner access    ${PUBLICATION_FOR_PUBLICATION_OWNER}

Create new publications and published releases - for Publication Approver
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_FOR_PUBLICATION_APPROVER}
    ...    ${TOPIC_ID}
    user creates releases in all states for publication    ${PUBLICATION_ID}
    ...    ${PUBLICATION_FOR_PUBLICATION_APPROVER}
    user gives analyst publication approver access    ${PUBLICATION_FOR_PUBLICATION_APPROVER}

Create new publications and published releases - for Release Viewer
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_FOR_RELEASE_VIEWER}    ${TOPIC_ID}
    user creates releases in all states for publication    ${PUBLICATION_ID}    ${PUBLICATION_FOR_RELEASE_VIEWER}
    user gives release access to all releases of publication to analyst    ${PUBLICATION_FOR_RELEASE_VIEWER}    Viewer

Create new publications and published releases - for Release Contributor
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_FOR_RELEASE_CONTRIBUTOR}
    ...    ${TOPIC_ID}
    user creates releases in all states for publication    ${PUBLICATION_ID}    ${PUBLICATION_FOR_RELEASE_CONTRIBUTOR}
    user gives release access to all releases of publication to analyst    ${PUBLICATION_FOR_RELEASE_CONTRIBUTOR}
    ...    Contributor

Create new publications and published releases - for Release Approver
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_FOR_RELEASE_APPROVER}    ${TOPIC_ID}
    user creates releases in all states for publication    ${PUBLICATION_ID}    ${PUBLICATION_FOR_RELEASE_APPROVER}
    user gives release access to all releases of publication to analyst    ${PUBLICATION_FOR_RELEASE_APPROVER}
    ...    Approver


*** Keywords ***
user creates releases in all states for publication
    [Arguments]
    ...    ${PUBLICATION_ID}
    ...    ${PUBLICATION_NAME}
    user creates a fully populated draft release    ${PUBLICATION_ID}    ${PUBLICATION_NAME}    ${THEME_NAME}
    ...    ${TOPIC_NAME}

    user creates a fully populated higher review release    ${PUBLICATION_ID}    ${PUBLICATION_NAME}    ${THEME_NAME}
    ...    ${TOPIC_NAME}
    user creates a fully populated approved release    ${PUBLICATION_ID}    ${PUBLICATION_NAME}    ${THEME_NAME}
    ...    ${TOPIC_NAME}
    user creates a fully populated published release    ${PUBLICATION_ID}    ${PUBLICATION_NAME}    ${THEME_NAME}
    ...    ${TOPIC_NAME}

user gives release access to all releases of publication to analyst
    [Arguments]
    ...    ${PUBLICATION_NAME}
    ...    ${ROLE}
    user gives release access to analyst    ${PUBLICATION_NAME}    ${DRAFT_RELEASE_TYPE}    ${ROLE}
    user gives release access to analyst    ${PUBLICATION_NAME}    ${HIGHER_REVIEW_RELEASE_TYPE}    ${ROLE}
    user gives release access to analyst    ${PUBLICATION_NAME}    ${APPROVED_RELEASE_TYPE}    ${ROLE}
    user gives release access to analyst    ${PUBLICATION_NAME}    ${PUBLISHED_RELEASE_TYPE}    ${ROLE}
