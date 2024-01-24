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
# "SeedDataGeneration" tag from the 'Force Tags' settings as this will prevent it from
# being run against a given enironment.
#

#
# If you see a consistent error such as 'Parent 'css:body' did not contain 'link:UI tests - Publication and Release UI Permissions Publication Owner' in 1 minute 40 seconds.'
# then you will need to run this test suite against the environment you are testing against in order to seed the environment so that other tests
# that depend on this test data don't fail.
#


*** Settings ***
Library             ../libs/admin_api.py
Resource            ../libs/admin-common.robot
Resource            seed_data_constants.robot
Resource            seed_data_common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          SeedDataGeneration    Local    Dev    PreProd


*** Test Cases ***
Create test theme and topic
    ${THEME_ID}=    user creates theme via api    ${SEED_DATA_THEME_2_TITLE}
    ${TOPIC_ID}=    user creates topic via api    ${SEED_DATA_THEME_2_TOPIC_1_TITLE}    ${THEME_ID}
    user reloads page
    Set Suite Variable    ${TOPIC_ID}

Create new publications and published releases - for Publication Owner
    ${PUBLICATION_ID}=    user creates test publication via api
    ...    ${SEED_DATA_THEME_2_PUBLICATION_FOR_PUBLICATION_OWNER}
    ...    ${TOPIC_ID}

    user creates releases in all states for publication
    ...    ${PUBLICATION_ID}
    ...    ${SEED_DATA_THEME_2_PUBLICATION_FOR_PUBLICATION_OWNER}

    user gives analyst publication owner access
    ...    ${SEED_DATA_THEME_2_PUBLICATION_FOR_PUBLICATION_OWNER}

Create new publications and published releases - for Publication Approver
    ${PUBLICATION_ID}=    user creates test publication via api
    ...    ${SEED_DATA_THEME_2_PUBLICATION_FOR_PUBLICATION_APPROVER}
    ...    ${TOPIC_ID}

    user creates releases in all states for publication
    ...    ${PUBLICATION_ID}
    ...    ${SEED_DATA_THEME_2_PUBLICATION_FOR_PUBLICATION_APPROVER}

    user gives analyst publication approver access
    ...    ${SEED_DATA_THEME_2_PUBLICATION_FOR_PUBLICATION_APPROVER}

Create new publications and published releases - for Release Viewer
    ${PUBLICATION_ID}=    user creates test publication via api
    ...    ${SEED_DATA_THEME_2_PUBLICATION_FOR_RELEASE_VIEWER}
    ...    ${TOPIC_ID}

    user creates releases in all states for publication
    ...    ${PUBLICATION_ID}
    ...    ${SEED_DATA_THEME_2_PUBLICATION_FOR_RELEASE_VIEWER}

    user gives release access to all releases of publication to analyst
    ...    ${SEED_DATA_THEME_2_PUBLICATION_FOR_RELEASE_VIEWER}
    ...    Viewer

Create new publications and published releases - for Release Contributor
    ${PUBLICATION_ID}=    user creates test publication via api
    ...    ${SEED_DATA_THEME_2_PUBLICATION_FOR_RELEASE_CONTRIBUTOR}
    ...    ${TOPIC_ID}

    user creates releases in all states for publication
    ...    ${PUBLICATION_ID}
    ...    ${SEED_DATA_THEME_2_PUBLICATION_FOR_RELEASE_CONTRIBUTOR}

    user gives release access to all releases of publication to analyst
    ...    ${SEED_DATA_THEME_2_PUBLICATION_FOR_RELEASE_CONTRIBUTOR}
    ...    Contributor

Create new publications and published releases - for Release Approver
    ${PUBLICATION_ID}=    user creates test publication via api
    ...    ${SEED_DATA_THEME_2_PUBLICATION_FOR_RELEASE_APPROVER}
    ...    ${TOPIC_ID}

    user creates releases in all states for publication
    ...    ${PUBLICATION_ID}
    ...    ${SEED_DATA_THEME_2_PUBLICATION_FOR_RELEASE_APPROVER}

    user gives release access to all releases of publication to analyst
    ...    ${SEED_DATA_THEME_2_PUBLICATION_FOR_RELEASE_APPROVER}
    ...    Approver


*** Keywords ***
user creates releases in all states for publication
    [Arguments]
    ...    ${PUBLICATION_ID}
    ...    ${SEED_DATA_THEME_2_PUBLICATION_NAME}

    user creates a fully populated draft release
    ...    ${PUBLICATION_ID}
    ...    ${SEED_DATA_THEME_2_PUBLICATION_NAME}
    ...    ${SEED_DATA_THEME_2_TITLE}
    ...    ${SEED_DATA_THEME_2_TOPIC_1_TITLE}

    user creates a fully populated higher review release
    ...    ${PUBLICATION_ID}
    ...    ${SEED_DATA_THEME_2_PUBLICATION_NAME}
    ...    ${SEED_DATA_THEME_2_TITLE}
    ...    ${SEED_DATA_THEME_2_TOPIC_1_TITLE}

    user creates a fully populated approved release
    ...    ${PUBLICATION_ID}
    ...    ${SEED_DATA_THEME_2_PUBLICATION_NAME}
    ...    ${SEED_DATA_THEME_2_TITLE}
    ...    ${SEED_DATA_THEME_2_TOPIC_1_TITLE}

    user creates a fully populated published release
    ...    ${PUBLICATION_ID}
    ...    ${SEED_DATA_THEME_2_PUBLICATION_NAME}
    ...    ${SEED_DATA_THEME_2_TITLE}
    ...    ${SEED_DATA_THEME_2_TOPIC_1_TITLE}

user gives release access to all releases of publication to analyst
    [Arguments]
    ...    ${SEED_DATA_THEME_2_PUBLICATION_NAME}
    ...    ${ROLE}

    user gives release access to analyst
    ...    ${SEED_DATA_THEME_2_PUBLICATION_NAME}
    ...    ${SEED_DATA_THEME_2_DRAFT_RELEASE_TYPE}
    ...    ${ROLE}

    user gives release access to analyst
    ...    ${SEED_DATA_THEME_2_PUBLICATION_NAME}
    ...    ${SEED_DATA_THEME_2_HIGHER_REVIEW_RELEASE_TYPE}
    ...    ${ROLE}

    user gives release access to analyst
    ...    ${SEED_DATA_THEME_2_PUBLICATION_NAME}
    ...    ${SEED_DATA_THEME_2_APPROVED_RELEASE_TYPE}
    ...    ${ROLE}

    user gives release access to analyst
    ...    ${SEED_DATA_THEME_2_PUBLICATION_NAME}
    ...    ${SEED_DATA_THEME_2_PUBLISHED_RELEASE_TYPE}
    ...    ${ROLE}
