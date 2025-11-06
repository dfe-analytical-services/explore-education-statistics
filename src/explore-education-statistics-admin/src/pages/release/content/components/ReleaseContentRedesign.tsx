import Link from '@admin/components/Link';
import { useReleaseContentState } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import VisuallyHidden from '@common/components/VisuallyHidden';
import ReleaseSummaryBlock from '@common/modules/release/components/ReleaseSummaryBlock';
import React, { Fragment } from 'react';
import ReleasePageTitle from './ReleasePageTitle';

const ReleaseContent = () => {
  const { release } = useReleaseContentState();

  const { publication } = release;

  return (
    <>
      <ReleasePageTitle
        publicationSummary={publication.summary || ''}
        publicationTitle={publication.title}
        releaseTitle={release.title}
      />

      <ReleaseSummaryBlock
        lastUpdated={release.updates[0]?.on}
        releaseDate={release.published}
        releaseType={release.type}
        renderProducerLink={
          release.publishingOrganisations?.length ? (
            <span>
              {release.publishingOrganisations.map((org, index) => (
                <Fragment key={org.id}>
                  {index > 0 && ' and '}
                  <Link unvisited to={org.url}>
                    {org.title}
                  </Link>
                </Fragment>
              ))}
            </span>
          ) : (
            <Link
              unvisited
              className="govuk-link--no-underline"
              to="https://www.gov.uk/government/organisations/department-for-education"
            >
              Department for Education
            </Link>
          )
        }
        renderUpdatesLink={
          release.updates.length > 1 ? (
            <Link to="#">
              {release.updates.length} updates{' '}
              <VisuallyHidden>for {release.title}</VisuallyHidden>
            </Link>
          ) : undefined
        }
      />
    </>
  );
};

export default ReleaseContent;
