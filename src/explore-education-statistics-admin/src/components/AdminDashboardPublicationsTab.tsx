import React from 'react';
import { Dictionary } from '@common/types';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import { Publication } from '@admin/services/publicationService';
import AdminDashboardPublications from './AdminDashboardPublications';
import Link from './Link';

interface AdminDashboardPublicationsTabProps {
  publicationsByThemeAndTopic: Dictionary<Publication[]>;
  noResultsMessage: string;
}

const AdminDashboardPublicationsTab = ({
  publicationsByThemeAndTopic,
  noResultsMessage,
}: AdminDashboardPublicationsTabProps) => {
  const themesAndTopics = Object.keys(publicationsByThemeAndTopic);

  if (themesAndTopics.length === 0) {
    return <div className="govuk-inset-text">{noResultsMessage}</div>;
  }

  const themesAndTopicsSections = themesAndTopics.map(themeTopic => {
    const publications = publicationsByThemeAndTopic[themeTopic];
    return (
      <>
        <AdminDashboardThemeTopic
          themeTopic={themeTopic}
          publications={publications}
        />
      </>
    );
  });

  return <div>{themesAndTopicsSections}</div>;
};

interface AdminDashboardThemeTopicProps {
  themeTopic: string;
  publications: Publication[];
}

const AdminDashboardThemeTopic = ({
  themeTopic,
  publications,
}: AdminDashboardThemeTopicProps) => {
  return (
    <>
      <h2 className="govuk-heading-l govuk-!-margin-bottom-0">{themeTopic}</h2>
      <p className="govuk-body">
        Edit an existing release or create a new release for current
        publications.
      </p>
      <Link to="/prototypes/publication-create-new" className="govuk-button">
        Create a new publication
      </Link>
      <Accordion id={themeTopic} key={themeTopic}>
        {publications.map(publication => (
          <AccordionSection
            key={publication.id}
            heading={publication.title}
            caption=""
            headingTag="h3"
          >
            <AdminDashboardPublications
              key={publication.id}
              publication={publication}
            />
          </AccordionSection>
        ))}
      </Accordion>
    </>
  );
};

export default AdminDashboardPublicationsTab;
