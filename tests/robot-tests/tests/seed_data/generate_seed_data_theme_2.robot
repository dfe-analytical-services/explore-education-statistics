*** Comments ***
#
# This test suite is responsible for setting up various Publications and Releases for the use of the Publication
# and Release Permissions UI tests.
#
# Releases are created in each approval state and a different role is assigned to Analyst1 for each. This way,
# there is a unique Release for each combination of Release approval status available and Release / Publication role.
#

#
# When seeding data for an environment, you will need to use the `--reseed` option of the `run_tests.py` script.
#


*** Settings ***
Library             ../libs/admin_api.py
Resource            ../libs/admin-common.robot
Resource            seed_data_theme_2_constants.robot
Resource            seed_data_common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          SeedDataGeneration    Local    Dev    PreProd


*** Test Cases ***
Create test theme
    ${THEME_ID}=    user creates theme via api    ${ROLE_PERMISSIONS_THEME_TITLE}
    user reloads page
    Set Suite Variable    ${THEME_ID}

Create new publications and published releases - for Publication Owner
    ${PUBLICATION_ID}=    user creates test publication via api
    ...    ${ROLE_PERMISSIONS_PUBLICATION_OWNER_PUBLICATION}
    ...    ${THEME_ID}

    user creates releases in all states for publication
    ...    ${PUBLICATION_ID}
    ...    ${ROLE_PERMISSIONS_PUBLICATION_OWNER_PUBLICATION}

    user gives analyst publication owner access
    ...    ${ROLE_PERMISSIONS_PUBLICATION_OWNER_PUBLICATION}

Create new publications and published releases - for Publication Approver
    ${PUBLICATION_ID}=    user creates test publication via api
    ...    ${ROLE_PERMISSIONS_PUBLICATION_APPROVER_PUBLICATION}
    ...    ${THEME_ID}

    user creates releases in all states for publication
    ...    ${PUBLICATION_ID}
    ...    ${ROLE_PERMISSIONS_PUBLICATION_APPROVER_PUBLICATION}

    user gives analyst publication approver access
    ...    ${ROLE_PERMISSIONS_PUBLICATION_APPROVER_PUBLICATION}

Create new publications and published releases - for Release Contributor
    ${PUBLICATION_ID}=    user creates test publication via api
    ...    ${ROLE_PERMISSIONS_RELEASE_CONTRIBUTOR_PUBLICATION}
    ...    ${THEME_ID}

    user creates releases in all states for publication
    ...    ${PUBLICATION_ID}
    ...    ${ROLE_PERMISSIONS_RELEASE_CONTRIBUTOR_PUBLICATION}

    user gives release access to all releases of publication to analyst
    ...    ${ROLE_PERMISSIONS_RELEASE_CONTRIBUTOR_PUBLICATION}
    ...    Contributor

Create new publications and published releases - for Release Approver
    ${PUBLICATION_ID}=    user creates test publication via api
    ...    ${ROLE_PERMISSIONS_RELEASE_APPROVER_PUBLICATION}
    ...    ${THEME_ID}

    user creates releases in all states for publication
    ...    ${PUBLICATION_ID}
    ...    ${ROLE_PERMISSIONS_RELEASE_APPROVER_PUBLICATION}

    user gives release access to all releases of publication to analyst
    ...    ${ROLE_PERMISSIONS_RELEASE_APPROVER_PUBLICATION}
    ...    Approver


*** Keywords ***
user creates releases in all states for publication
    [Arguments]
    ...    ${PUBLICATION_ID}
    ...    ${SEED_DATA_THEME_2_PUBLICATION_NAME}

    user creates a fully populated draft release
    ...    ${PUBLICATION_ID}
    ...    ${SEED_DATA_THEME_2_PUBLICATION_NAME}
    ...    ${ROLE_PERMISSIONS_THEME_TITLE}

    user creates a fully populated higher review release
    ...    ${PUBLICATION_ID}
    ...    ${SEED_DATA_THEME_2_PUBLICATION_NAME}
    ...    ${ROLE_PERMISSIONS_THEME_TITLE}

    user creates a fully populated approved release
    ...    ${PUBLICATION_ID}
    ...    ${SEED_DATA_THEME_2_PUBLICATION_NAME}
    ...    ${ROLE_PERMISSIONS_THEME_TITLE}

    user creates a fully populated published release
    ...    ${PUBLICATION_ID}
    ...    ${SEED_DATA_THEME_2_PUBLICATION_NAME}
    ...    ${ROLE_PERMISSIONS_THEME_TITLE}

user gives release access to all releases of publication to analyst
    [Arguments]
    ...    ${SEED_DATA_THEME_2_PUBLICATION_NAME}
    ...    ${ROLE}

    user gives release access to analyst
    ...    ${SEED_DATA_THEME_2_PUBLICATION_NAME}
    ...    ${ROLE_PERMISSIONS_DRAFT_RELEASE_TYPE}
    ...    ${ROLE}

    user gives release access to analyst
    ...    ${SEED_DATA_THEME_2_PUBLICATION_NAME}
    ...    ${ROLE_PERMISSIONS_HIGHER_REVIEW_RELEASE_TYPE}
    ...    ${ROLE}

    user gives release access to analyst
    ...    ${SEED_DATA_THEME_2_PUBLICATION_NAME}
    ...    ${ROLE_PERMISSIONS_APPROVED_RELEASE_TYPE}
    ...    ${ROLE}

    user gives release access to analyst
    ...    ${SEED_DATA_THEME_2_PUBLICATION_NAME}
    ...    ${ROLE_PERMISSIONS_PUBLISHED_RELEASE_TYPE}
    ...    ${ROLE}
