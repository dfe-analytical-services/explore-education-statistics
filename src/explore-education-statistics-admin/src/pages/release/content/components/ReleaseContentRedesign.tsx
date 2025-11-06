import Link from '@admin/components/Link';
import RouteLeavingGuard from '@admin/components/RouteLeavingGuard';
import { useEditingContext } from '@admin/contexts/EditingContext';
import { useReleaseContentState } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import VisuallyHidden from '@common/components/VisuallyHidden';
import ReleaseSummaryBlock from '@common/modules/release/components/ReleaseSummaryBlock';
import React, { Fragment, useMemo } from 'react';
import ReleasePageTitle from './ReleasePageTitle';

const ReleaseContent = () => {
  const { unsavedBlocks, unsavedCommentDeletions } = useEditingContext();
  const { release } = useReleaseContentState();

  const blockRouteChange = useMemo(() => {
    if (unsavedBlocks.length > 0) {
      return true;
    }

    const blocksWithCommentDeletions = Object.entries(unsavedCommentDeletions)
      .filter(blockWithDeletions => blockWithDeletions[1].length)
      .map(blockWithDeletions => blockWithDeletions[0]);

    return blocksWithCommentDeletions.length > 0;
  }, [unsavedBlocks, unsavedCommentDeletions]);

  const { publication } = release;

  return (
    <>
      <RouteLeavingGuard
        blockRouteChange={blockRouteChange}
        title="There are unsaved changes"
      >
        <p>
          Clicking away from this tab will result in the changes being lost.
        </p>
      </RouteLeavingGuard>

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
