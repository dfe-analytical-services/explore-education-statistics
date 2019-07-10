import React from 'react';
import { Dictionary } from '@common/types';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import { Publication } from '@admin/services/types/types';
import groupBy from 'lodash/groupBy';
import AdminDashboardPublications from './AdminDashboardPublications';
import Link from './Link';

interface AdminDashboardPublicationsTabProps {
  publications: Publication[];
  noResultsMessage: string;
}

const AdminDashboardPublicationsTab = ({
  publications,
  noResultsMessage,
}: AdminDashboardPublicationsTabProps) => {
  const createThemeTopicTitleLabel = (publication: Publication) =>
    `${publication.topic.theme.label}, ${publication.topic.title}`;

  const publicationsByThemeAndTopic: Dictionary<Publication[]> = groupBy(
    publications,
    createThemeTopicTitleLabel,
  );

  const themesAndTopics = Object.keys(publicationsByThemeAndTopic);

  if (themesAndTopics.length === 0) {
    return <div className="govuk-inset-text">{noResultsMessage}</div>;
  }

  const themesAndTopicsSections = themesAndTopics.map(themeTopic => (
    <React.Fragment key={themeTopic}>
      <h2 className="govuk-heading-l govuk-!-margin-bottom-0">{themeTopic}</h2>
      <p className="govuk-body">
        Edit an existing release or create a new release for current
        publications.
      </p>
      <Link to="/prototypes/publication-create-new" className="govuk-button">
        Create a new publication
      </Link>
      <Accordion id={themeTopic} key={themeTopic}>
        {publicationsByThemeAndTopic[themeTopic].map(publication => (
          <AccordionSection
            key={publication.id}
            heading={publication.title}
            headingTag="h3"
          >
            <AdminDashboardPublications publication={publication} />
          </AccordionSection>
        ))}
      </Accordion>
    </React.Fragment>
  ));

  return <section>{themesAndTopicsSections}</section>;
};

export default AdminDashboardPublicationsTab;
