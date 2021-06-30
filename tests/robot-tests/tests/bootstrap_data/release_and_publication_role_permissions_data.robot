*** Settings ***
Resource    bootstrap-common.robot
Resource    ../libs/admin-common.robot
Library     ../libs/admin_api.py

Force Tags  BootstrapData  Local  Dev

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Variables ***
${IDENTIFIER}  UI tests - Role Permissions
${THEME_NAME}  ${IDENTIFIER} Theme
${TOPIC_NAME}  ${IDENTIFIER} Topic
${PUBLICATION_FOR_PUBLICATION_OWNER}  ${IDENTIFIER} Publication Owner
${PUBLICATION_FOR_RELEASE_VIEWER}  ${IDENTIFIER} Release Viewer
${PUBLICATION_FOR_RELEASE_CONTRIBUTOR}  ${IDENTIFIER} Release Contributor
${PUBLICATION_FOR_RELEASE_APPROVER}  ${IDENTIFIER} Release Approver
${DRAFT_RELEASE_TYPE}      Academic Year 2022/23
${HIGHER_REVIEW_RELEASE_TYPE}      Academic Year 2023/24
${APPROVED_RELEASE_TYPE}      Academic Year 2024/25
${PUBLISHED_RELEASE_TYPE}      Academic Year 2025/26

*** Test Cases ***
Create test theme and topic
    ${THEME_ID}=  user creates theme via api  ${THEME_NAME}
    ${TOPIC_ID}=  user creates topic via api  ${TOPIC_NAME}  ${THEME_ID}
    Set Suite Variable  ${TOPIC_ID}

Create new publications and published releases - for Publication Owner
    ${PUBLICATION_ID}=  user creates test publication via api   ${PUBLICATION_FOR_PUBLICATION_OWNER}  ${TOPIC_ID}
    user creates releases in all states for publication  ${PUBLICATION_ID}  ${PUBLICATION_FOR_PUBLICATION_OWNER}
    user gives analyst publication owner access  ${PUBLICATION_FOR_PUBLICATION_OWNER}

Create new publications and published releases - for Release Viewer
    ${PUBLICATION_ID}=  user creates test publication via api   ${PUBLICATION_FOR_RELEASE_VIEWER}  ${TOPIC_ID}
    user creates releases in all states for publication  ${PUBLICATION_ID}  ${PUBLICATION_FOR_RELEASE_VIEWER}
    user gives release access to all releases of publication to analyst  ${PUBLICATION_FOR_RELEASE_VIEWER}  Viewer

Create new publications and published releases - for Release Contributor
    ${PUBLICATION_ID}=  user creates test publication via api   ${PUBLICATION_FOR_RELEASE_CONTRIBUTOR}  ${TOPIC_ID}
    user creates releases in all states for publication  ${PUBLICATION_ID}  ${PUBLICATION_FOR_RELEASE_CONTRIBUTOR}
    user gives release access to all releases of publication to analyst  ${PUBLICATION_FOR_RELEASE_CONTRIBUTOR}  Viewer

Create new publications and published releases - for Release Approver
    ${PUBLICATION_ID}=  user creates test publication via api   ${PUBLICATION_FOR_RELEASE_APPROVER}  ${TOPIC_ID}
    user creates releases in all states for publication  ${PUBLICATION_ID}  ${PUBLICATION_FOR_RELEASE_APPROVER}
    user gives release access to all releases of publication to analyst  ${PUBLICATION_FOR_RELEASE_APPROVER}  Viewer

*** Keywords ***
user creates releases in all states for publication
    [Arguments]
    ...  ${PUBLICATION_ID}
    ...  ${PUBLICATION_NAME}
    user creates a fully populated draft release  ${PUBLICATION_ID}  ${PUBLICATION_NAME}  ${THEME_NAME}  ${TOPIC_NAME}
    user creates a fully populated higher review release  ${PUBLICATION_ID}  ${PUBLICATION_NAME}  ${THEME_NAME}  ${TOPIC_NAME}
    user creates a fully populated approved release  ${PUBLICATION_ID}  ${PUBLICATION_NAME}  ${THEME_NAME}  ${TOPIC_NAME}
    user creates a fully populated published release  ${PUBLICATION_ID}  ${PUBLICATION_NAME}  ${THEME_NAME}  ${TOPIC_NAME}

user gives release access to all releases of publication to analyst
    [Arguments]
    ...  ${PUBLICATION_NAME}
    ...  ${ROLE}
    user gives release access to analyst  ${PUBLICATION_NAME} - ${DRAFT_RELEASE_TYPE}  ${ROLE}
    user gives release access to analyst  ${PUBLICATION_NAME} - ${HIGHER_REVIEW_RELEASE_TYPE}  ${ROLE}
    user gives release access to analyst  ${PUBLICATION_NAME} - ${APPROVED_RELEASE_TYPE}  ${ROLE}
    user gives release access to analyst  ${PUBLICATION_NAME} - ${PUBLISHED_RELEASE_TYPE}  ${ROLE}
