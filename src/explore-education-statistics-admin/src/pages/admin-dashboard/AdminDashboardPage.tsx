import ButtonLink from '@admin/components/ButtonLink';
import Link from '@admin/components/Link';
import { LoginContext } from '@admin/components/Login';
import Page from '@admin/components/Page';
import ReleasesTab from '@admin/pages/admin-dashboard/components/ReleasesByStatusTab';
import { summaryRoute } from '@admin/routes/edit-release/routes';
import { UserDetails } from '@admin/services/common/types';
import dashboardService from '@admin/services/dashboard/service';
import { AdminDashboardRelease } from '@admin/services/dashboard/types';
import loginService from '@admin/services/sign-in/service';
import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import FormFieldset from '@common/components/form/FormFieldset';
import FormSelect from '@common/components/form/FormSelect';
import RelatedInformation from '@common/components/RelatedInformation';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import { Dictionary } from '@common/types';
import React, { useContext, useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';
import MyPublicationsTab from './components/MyPublicationsTab';

interface Model {
  draftReleases: AdminDashboardRelease[];
  scheduledReleases: AdminDashboardRelease[];
  availablePreReleaseContacts: UserDetails[];
  preReleaseContactsByScheduledRelease: Dictionary<UserDetails[]>;
}

interface MatchProps {
  themeId: string;
  topicId: string;
}

const AdminDashboardPage = ({
  match,
  handleApiErrors,
}: RouteComponentProps<MatchProps> & ErrorControlProps) => {
  const { user } = useContext(LoginContext);
  const { themeId, topicId } = match.params;
  const [model, setModel] = useState<Model>();
  useEffect(() => {
    Promise.all([
      dashboardService.getDraftReleases(),
      dashboardService.getScheduledReleases(),
      dashboardService.getAvailablePreReleaseContacts(),
    ])
      .then(
        ([draftReleases, scheduledReleases, availablePreReleaseContacts]) => {
          const contactResultsByRelease = scheduledReleases.map(release =>
            dashboardService
              .getPreReleaseContactsForRelease(release.id)
              .then(contacts => ({
                releaseId: release.id,
                contacts,
              })),
          );

          return Promise.all(contactResultsByRelease).then(contactResults => {
            const preReleaseContactsByScheduledRelease: Dictionary<
              UserDetails[]
            > = {};
            contactResults.forEach(result => {
              const { releaseId, contacts } = result;
              preReleaseContactsByScheduledRelease[releaseId] = contacts;
            });
            setModel({
              draftReleases,
              scheduledReleases,
              availablePreReleaseContacts,
              preReleaseContactsByScheduledRelease,
            });
          });
        },
      )
      .catch(handleApiErrors);
  }, []);

  return (
    <>
      {model && (
        <Page wide breadcrumbs={[{ name: 'Administrator dashboard' }]}>
          <div className="govuk-grid-row">
            <div className="govuk-grid-column-two-thirds">
              <span className="govuk-caption-xl">Welcome</span>
              <h1 className="govuk-heading-xl">
                {user ? user.name : ''}{' '}
                <span className="govuk-body-s">
                  Not you?{' '}
                  <li className="govuk-header__navigation-item">
                    <Link
                      className="govuk-link"
                      to={loginService.getSignOutLink()}
                    >
                      Sign out
                    </Link>
                  </li>
                </span>
              </h1>

              <p>This is your administration dashboard - here you can:</p>

              <ul className="govuk-bullet--list govuk-!-margin-bottom-9">
                <li>
                  <Link to="/dashboard">manage publications and releases</Link>
                </li>
                <li>
                  <Link to="/methodology">manage methodology</Link>
                </li>
              </ul>
            </div>

            <div className="govuk-grid-column-one-third">
              <RelatedInformation heading="Help and guidance">
                <ul className="govuk-list">
                  <li>
                    <Link to="/documentation/using-dashboard" target="_blank">
                      Using your administration dashboard{' '}
                    </Link>
                  </li>
                  <li>
                    <Link
                      to="/documentation/create-new-release"
                      target="_blank"
                    >
                      Creating a new release{' '}
                    </Link>
                  </li>
                </ul>
              </RelatedInformation>
            </div>
          </div>

          <Tabs id="publicationTabs">
            <TabsSection
              id="my-publications"
              title="Manage publications and releases"
            >
              <MyPublicationsTab themeId={themeId} topicId={topicId} />
            </TabsSection>
            <TabsSection
              id="draft-releases"
              title={`View draft releases (${model.draftReleases.length})`}
            >
              <ReleasesTab
                releases={model.draftReleases}
                noReleasesMessage="There are currently no draft releases"
                actions={release => (
                  <ButtonLink
                    to={summaryRoute.generateLink(
                      release.publicationId,
                      release.id,
                    )}
                  >
                    View and edit release
                  </ButtonLink>
                )}
              />
            </TabsSection>
            <TabsSection
              id="scheduled-releases"
              title={`View scheduled releases (${model.scheduledReleases.length})`}
            >
              <ReleasesTab
                releases={model.scheduledReleases}
                noReleasesMessage="There are currently no scheduled releases"
                actions={release => (
                  <ButtonLink
                    to={summaryRoute.generateLink(
                      release.publicationId,
                      release.id,
                    )}
                  >
                    Preview release
                  </ButtonLink>
                )}
              >
                {release => (
                  <>
                    <FormFieldset
                      legend="Manage pre release access"
                      legendSize="s"
                      id="pre-release-selection"
                    >
                      <FormSelect
                        id="preReleaseAccessContact"
                        name="preReleaseAccessContact"
                        label="Select user"
                        options={[
                          {
                            label: 'Please select',
                            value: '',
                          },
                          ...model.availablePreReleaseContacts
                            .filter(
                              contact =>
                                !model.preReleaseContactsByScheduledRelease[
                                  release.id
                                ].find(c => c.id === contact.id),
                            )
                            .map(contact => ({
                              label: contact.name,
                              value: contact.id,
                            })),
                        ]}
                        order={[]}
                        className="govuk-!-width-one-third"
                        onChange={async event => {
                          const updatedContacts = await dashboardService
                            .addPreReleaseContactToRelease(
                              release.id,
                              event.target.value,
                            )
                            .catch(handleApiErrors);

                          setModel({
                            ...model,
                            preReleaseContactsByScheduledRelease: {
                              ...model.preReleaseContactsByScheduledRelease,
                              [release.id]: updatedContacts,
                            },
                          });
                        }}
                      />
                    </FormFieldset>

                    <SummaryList>
                      {model.preReleaseContactsByScheduledRelease[
                        release.id
                      ].map(existingContact => (
                        <SummaryListItem
                          key={existingContact.id}
                          term="Pre release access"
                          actions={
                            <Link
                              to=""
                              onClick={async _ => {
                                const updatedContacts = await dashboardService
                                  .removePreReleaseContactFromRelease(
                                    release.id,
                                    existingContact.id,
                                  )
                                  .catch(handleApiErrors);

                                setModel({
                                  ...model,
                                  preReleaseContactsByScheduledRelease: {
                                    ...model.preReleaseContactsByScheduledRelease,
                                    [release.id]: updatedContacts,
                                  },
                                });
                              }}
                            >
                              Remove
                            </Link>
                          }
                        >
                          {existingContact.name}
                        </SummaryListItem>
                      ))}
                    </SummaryList>
                  </>
                )}
              </ReleasesTab>
            </TabsSection>
          </Tabs>
        </Page>
      )}
    </>
  );
};

export default withErrorControl(AdminDashboardPage);
