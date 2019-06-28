import React, { Component } from 'react';
import RelatedInformation from '@common/components/RelatedInformation';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import { Dictionary } from '@common/types';
import { LoginContext } from '@admin/components/Login';
import { Authentication } from '@admin/services/PrototypeLoginService';
import DummyPublicationsData from '@admin/pages/DummyPublicationsData';
import { Publication } from '@admin/services/publicationService';
import AdminDashboardPublicationsTab from '@admin/components/AdminDashboardPublicationsTab';
import groupBy from 'lodash/groupBy';
import Link from '../components/Link';
import Page from '../components/Page';

interface State {
  myPublications: Publication[];
  inProgressPublications: Publication[];
}

class BrowseReleasesPage extends Component<{}, State> {
  public state: State = {
    myPublications: [],
    inProgressPublications: [],
  };

  public componentDidMount(): void {
    const authentication: Authentication = this.context;
    const { user } = authentication;
    const loggedInUserId = user ? user.id : null;

    const myPublications =
      loggedInUserId === null
        ? []
        : DummyPublicationsData.allPublications.filter(
            _ => _.owner.id === loggedInUserId,
          );

    const inProgressPublications =
      loggedInUserId === null
        ? []
        : DummyPublicationsData.allPublications.filter(
            _ => _.owner.id !== loggedInUserId,
          );

    this.setState({
      myPublications,
      inProgressPublications,
    });
  }

  public render() {
    const { myPublications, inProgressPublications } = this.state;

    const createThemeTopicTitleLabel = (_: Publication) =>
      `${_.topic.theme.title}, ${_.topic.title}`;

    const myPublicationsByThemeAndTopic: Dictionary<Publication[]> = groupBy(
      myPublications,
      createThemeTopicTitleLabel,
    );

    const inProgressPublicationsByThemeAndTopic: Dictionary<
      Publication[]
    > = groupBy(inProgressPublications, createThemeTopicTitleLabel);

    return (
      <Page wide breadcrumbs={[{ name: 'Administrator dashboard' }]}>
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            <LoginContext.Consumer>
              {loginContext =>
                loginContext.user && <UserGreeting user={loginContext.user} />
              }
            </LoginContext.Consumer>
          </div>
          <div className="govuk-grid-column-one-third">
            <RelatedInformation heading="Help and guidance">
              <ul className="govuk-list">
                <li>
                  <Link to="/prototypes/methodology-home">
                    Administrators' guide{' '}
                  </Link>
                </li>
              </ul>
            </RelatedInformation>
          </div>
        </div>
        <Tabs id="publicationTabs">
          <TabsSection id="my-publications" title="Publications">
            <AdminDashboardPublicationsTab
              publicationsByThemeAndTopic={myPublicationsByThemeAndTopic}
              noResultsMessage="You have not yet created any publications"
            />
          </TabsSection>
          <TabsSection id="in-progress-publications" title="In progress">
            <AdminDashboardPublicationsTab
              publicationsByThemeAndTopic={
                inProgressPublicationsByThemeAndTopic
              }
              noResultsMessage="There are currenly no releases in progress"
            />
          </TabsSection>
        </Tabs>
      </Page>
    );
  }
}

BrowseReleasesPage.contextType = LoginContext;

const UserGreeting = ({ user }: Authentication) => (
  <>
    <span className="govuk-caption-xl">Welcome</span>

    <h1 className="govuk-heading-xl">
      {user ? user.name : ''}{' '}
      <span className="govuk-body-s">
        Not you? <Link to="#">Sign out</Link>
      </span>
    </h1>
  </>
);

export default BrowseReleasesPage;
