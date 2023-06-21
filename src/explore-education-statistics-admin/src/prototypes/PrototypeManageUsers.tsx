import PageTitle from '@admin/components/PageTitle';
import PrototypeChangeUserRole from '@admin/prototypes/components/PrototypeChangeUserRole';
import PrototypePage from '@admin/prototypes/components/PrototypePage';
import React, { useState } from 'react';
import ButtonGroup from '@common/components/ButtonGroup';
import Button from '@common/components/Button';
import { FormGroup, FormTextInput } from '@common/components/form';
import ModalConfirm from '@common/components/ModalConfirm';
import useToggle from '@common/hooks/useToggle';
import Details from '@common/components/Details';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import WarningMessage from '@common/components/WarningMessage';

const PrototypeManageUsers = () => {
  const [showDeleteUserModal, toggleDeleteUserModal] = useToggle(false);
  const [userName, setUserName] = useState('');
  const [showRoleModal, toggleRoleModal] = useToggle(false);
  const [showTeamMembers, toggleTeamMembers] = useToggle(true);

  const currentReleases = [
    'Academic year 20 / 21',
    'Academic year 19 / 20',
    'Academic year 18 / 19',
    'Academic year 17 / 18',
  ];

  const userList = [
    {
      name: 'Andrea',
      surname: 'Adams',
      releases: [
        {
          release: currentReleases[0],
          role: false,
        },
        {
          release: currentReleases[1],
          role: false,
        },
        {
          release: currentReleases[2],
          role: true,
        },
        {
          release: currentReleases[3],
          role: true,
        },
      ],
    },
    {
      name: 'Ben',
      surname: 'Browne',
      releases: [
        {
          release: currentReleases[0],
          role: false,
        },
        {
          release: currentReleases[1],
          role: true,
        },
        {
          release: currentReleases[2],
          role: true,
        },
        {
          release: currentReleases[3],
          role: true,
        },
      ],
    },
    {
      name: 'Charlie',
      surname: 'Cheeseman',
      releases: [
        {
          release: currentReleases[0],
          role: true,
        },
        {
          release: currentReleases[1],
          role: true,
        },
        {
          release: currentReleases[2],
          role: true,
        },
        {
          release: currentReleases[3],
          role: true,
        },
      ],
    },
    {
      name: 'Danielle',
      surname: 'Davids',
      releases: [
        {
          release: currentReleases[0],
          role: true,
        },
        {
          release: currentReleases[1],
          role: true,
        },
        {
          release: currentReleases[2],
          role: true,
        },
        {
          release: currentReleases[3],
          role: true,
        },
      ],
    },
  ];

  return (
    <PrototypePage
      wide
      breadcrumbs={[
        { name: 'Dashboard', link: '/dashboard' },
        { name: 'Manage users', link: '#' },
      ]}
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <PageTitle title="Manage team access" caption="Publication title" />
        </div>
      </div>

      <Tabs id="manage-release-users">
        <TabsSection title="Manage team access">
          <form className="govuk-!-margin-bottom-9">
            <fieldset className="govuk-fieldset">
              <legend className="govuk-fieldset__legend govuk-fieldset__legend--m">
                Update access for latest release (Academic year 20 / 21)
              </legend>
              {(showTeamMembers && (
                <>
                  <p className="govuk-hint">
                    Allow team members to be able to access and edit this
                    release. You can also{' '}
                    <a href="#previous-releases">
                      control access to previous releases
                    </a>
                    .
                  </p>
                  {userList.map((item, index) => (
                    <div
                      className="dfe-flex dfe-flex-wrap dfe-align-items--center dfe-justify-content--space-between dfe-flex-underline"
                      // eslint-disable-next-line react/no-array-index-key
                      key={index.toString()}
                    >
                      <h2 className="govuk-heading-s govuk-!-margin-bottom-0 dfe-flex-basis--50">{`${item.name} ${item.surname}`}</h2>

                      <PrototypeChangeUserRole
                        name={`${item.name} ${item.surname}`}
                        release={item.releases[0].release}
                        className="dfe-flex-basis--30 govuk-!-margin-top-1 govuk-!-margin-bottom-1"
                      />

                      <div className="dfe-align--right dfe-flex-basis--10">
                        <a
                          href="#"
                          onClick={() => {
                            toggleDeleteUserModal(true);
                            setUserName(`${item.name} ${item.surname}`);
                          }}
                        >
                          Remove user
                        </a>
                      </div>

                      {/* <div className="dfe-flex-basis--100">
                        <Details summary="Individual releases">
                          {item.releases.map((item2, index2) =>(
                            <>
                              <div className={`dfe-flex dfe-align-items--center ${index2 < (item.releases.length-1) ? 'dfe-flex-underline' : ''}`} key={index2.toString()}>
                                <div className="dfe-flex-basis--60 dfe-align--right govuk-!-padding-2">
                                  <PrototypeChangeUserRole 
                                    selectedRole={item2.role} 
                                    name={`${item.name} ${item.surname}`} 
                                    release={item2.release}
                                    roleId={`${index}-${index2}`} 
                                  />
                                </div>
                                <div className="dfe-flex-basis--40 dfe-align--right">
                                  <a
                                    href="#"
                                    onClick={() => {
                                      toggleDeleteUserModal(true);
                                      setUserName(`${item.name} ${item.surname}`);
                                      setReleaseName(item2.release);
                                    }}
                                  >
                                    Remove
                                  </a>
                                </div>
                              </div>
                            </>
                          ))}
                        </Details>
                      </div>*/}
                      <ModalConfirm
                        open={showDeleteUserModal}
                        title="Confirm user removal"
                        onExit={() => toggleDeleteUserModal(false)}
                        onConfirm={() => toggleDeleteUserModal(false)}
                        onCancel={() => toggleDeleteUserModal(false)}
                      >
                        <p>
                          Are you sure you want to remove{' '}
                          <strong>{userName}</strong>
                          <br />
                          from all releases in this publication?
                        </p>
                      </ModalConfirm>
                    </div>
                  ))}
                  <div className="dfe-flex dfe-flex-wrap dfe-align-items--center dfe-justify-content--flex-end govuk-!-padding-bottom-3 govuk-!-margin-top-6">
                    <div className="dfe-flex dfe-flex-basis--30">
                      <Button
                        type="button"
                        onClick={() => {
                          toggleRoleModal(true);
                        }}
                        className="dfe-flex-basis--55 govuk-!-margin-left-2"
                      >
                        Grant access to all
                      </Button>

                      <ModalConfirm
                        open={showRoleModal}
                        title="Grant access to all listed users"
                        onExit={() => toggleRoleModal(false)}
                        onConfirm={() => toggleRoleModal(false)}
                        onCancel={() => toggleRoleModal(false)}
                      >
                        <p>
                          Are you sure you want to grant access to all listed
                          users?
                        </p>
                      </ModalConfirm>
                    </div>
                  </div>
                  <a
                    href="#"
                    onClick={() => {
                      toggleTeamMembers(false);
                    }}
                  >
                    Hide team members
                  </a>
                </>
              )) || (
                <WarningMessage className="govuk-!-margin-top-9">
                  There are currently no team members associated to this
                  publication
                  <p className="govuk-!-margin-bottom-0">
                    Please{' '}
                    <a href="#manage-release-users-2">invite new users</a> to
                    join.
                  </p>
                </WarningMessage>
              )}
            </fieldset>
          </form>
          {showTeamMembers && (
            <>
              <h2 className="govuk-heading-m" id="previous-releases">
                Previous releases
              </h2>
              {currentReleases.slice(1).map((item, index) => (
                // eslint-disable-next-line react/no-array-index-key
                <div key={index.toString()}>
                  <Details summary={item}>
                    {userList.map((item2, index2) => (
                      <div
                        className="dfe-flex dfe-flex-wrap dfe-align-items--center dfe-justify-content--space-between dfe-flex-underline"
                        // eslint-disable-next-line react/no-array-index-key
                        key={index2.toString()}
                      >
                        <h2 className="govuk-heading-s govuk-!-margin-bottom-0 dfe-flex-basis--50">{`${item2.name} ${item2.surname}`}</h2>

                        <PrototypeChangeUserRole
                          selectedRole={item2.releases[index + 1].role}
                          name={`${item2.name} ${item2.surname}`}
                          release={item2.releases[index + 1].release}
                          className="dfe-flex-basis--30 govuk-!-margin-top-1 govuk-!-margin-bottom-1"
                        />
                        <div className="dfe-align--right dfe-flex-basis--10">
                          <a
                            href="#"
                            onClick={() => {
                              toggleDeleteUserModal(true);
                              setUserName(`${item2.name} ${item2.surname}`);
                            }}
                          >
                            Delete user
                          </a>
                        </div>
                      </div>
                    ))}
                  </Details>
                </div>
              ))}
            </>
          )}
        </TabsSection>
        <TabsSection title="Invite new users">
          <form>
            <fieldset className="govuk-fieldset">
              <legend className="govuk-fieldset__legend govuk-fieldset__legend--m">
                Invite new user to access current releases
              </legend>
              <p className="govuk-hint">
                The invited user must have an @education.gov.uk email address
              </p>
              <FormGroup>
                <FormTextInput
                  id="inviteEmail"
                  name="inviteEmail"
                  label="Email address"
                  className="govuk-!-width-three-quarters"
                />
              </FormGroup>

              <p className="govuk-hint">
                By default this user will have access and be able to edit to all
                releases in this publication.
              </p>

              <Details
                summary="Alternatively set roles for individual releases"
                className="govuk-!-margin-top-2"
              >
                <fieldset className="govuk-fieldset">
                  <legend className="govuk-fieldset__legend govuk-fieldset__legend--s">
                    Set user roles for individual releases
                  </legend>
                  {currentReleases.map((item, index) => (
                    <div
                      className={`dfe-flex dfe-align-items--center dfe-justify-content--space-between ${
                        index < currentReleases.length - 1
                          ? 'dfe-flex-underline'
                          : ''
                      }`}
                      // eslint-disable-next-line react/no-array-index-key
                      key={index.toString()}
                    >
                      <h2 className="govuk-heading-s govuk-!-margin-bottom-0 dfe-flex-basis--50">
                        {item}
                      </h2>
                      <div className="dfe-flex-basis--50 dfe-align--right govuk-!-padding-2">
                        <PrototypeChangeUserRole
                          selectedRole
                          name="Invited user"
                          release={item}
                        />
                      </div>
                    </div>
                  ))}
                </fieldset>
              </Details>
              <ButtonGroup>
                <Button>Send invite</Button>
              </ButtonGroup>
            </fieldset>
          </form>
        </TabsSection>
      </Tabs>
    </PrototypePage>
  );
};

export default PrototypeManageUsers;
