*** Keywords ***
Import bootstrap data roles and permissions variables
    Set suite variable    ${IDENTIFIER}    UI tests - Publication and Release UI Permissions
    Set suite variable    ${THEME_NAME}    ${IDENTIFIER} Theme
    Set suite variable    ${TOPIC_NAME}    ${IDENTIFIER} Topic
    Set suite variable    ${PUBLICATION_FOR_PUBLICATION_OWNER}    ${IDENTIFIER} Publication Owner
    Set suite variable    ${PUBLICATION_FOR_PUBLICATION_APPROVER}    ${IDENTIFIER} Publication Release Approver
    Set suite variable    ${PUBLICATION_FOR_RELEASE_VIEWER}    ${IDENTIFIER} Release Viewer
    Set suite variable    ${PUBLICATION_FOR_RELEASE_CONTRIBUTOR}    ${IDENTIFIER} Release Contributor
    Set suite variable    ${PUBLICATION_FOR_RELEASE_APPROVER}    ${IDENTIFIER} Release Approver
    Set suite variable    ${DRAFT_RELEASE_TYPE}    Academic year 2022/23
    Set suite variable    ${HIGHER_REVIEW_RELEASE_TYPE}    Academic year 2023/24
    Set suite variable    ${APPROVED_RELEASE_TYPE}    Academic year 2024/25
    Set suite variable    ${PUBLISHED_RELEASE_TYPE}    Academic year 2025/26
