import Link from '@admin/components/Link';
import PageTitle from '@admin/components/PageTitle';
import {
  PreReleaseMethodologyRouteParams,
  preReleaseMethodologyRoute,
} from '@admin/routes/preReleaseRoutes';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import publicationService from '@admin/services/publicationService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tag from '@common/components/Tag';
import TagGroup from '@common/components/TagGroup';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { generatePath, RouteComponentProps } from 'react-router';

const PreReleaseMethodologiesPage = ({
  match,
}: RouteComponentProps<ReleaseRouteParams>) => {
  const { publicationId, releaseId } = match.params;

  const { value: publication, isLoading } = useAsyncHandledRetry(
    () => publicationService.getMyPublication(publicationId),
    [publicationId],
  );

  const methodologies = publication?.methodologies.filter(
    ({ methodology }) =>
      methodology.status !== 'Draft' ||
      (methodology.status === 'Draft' && methodology.amendment),
  );

  return (
    <div className="govuk-width-container">
      <>
        <PageTitle title="Methodologies" />
        <LoadingSpinner loading={isLoading}>
          {publication && (
            <>
              {methodologies?.length === 0 &&
              !publication.externalMethodology ? (
                <WarningMessage>No Methodologies available.</WarningMessage>
              ) : (
                <ul className="govuk-list">
                  {methodologies?.map(({ methodology, owner }) => (
                    <li key={methodology.id}>
                      <>
                        <Link
                          to={generatePath<PreReleaseMethodologyRouteParams>(
                            preReleaseMethodologyRoute.path,
                            {
                              publicationId,
                              releaseId,
                              methodologyId:
                                methodology.status === 'Draft' &&
                                methodology.previousVersionId
                                  ? methodology.previousVersionId
                                  : methodology.id,
                            },
                          )}
                        >
                          {methodology.title} {owner ? '(Owned)' : '(Adopted)'}
                        </Link>
                        <TagGroup className="govuk-!-margin-left-2">
                          {methodology.status === 'Approved' &&
                            !methodology.published && <Tag>Approved</Tag>}

                          {((methodology.amendment &&
                            methodology.status === 'Draft') ||
                            methodology.published) && <Tag>Published</Tag>}

                          {methodology.amendment &&
                            methodology.status === 'Approved' && (
                              <Tag>Amendment</Tag>
                            )}
                        </TagGroup>
                      </>
                    </li>
                  ))}

                  {publication.externalMethodology && (
                    <li>
                      <Link to={publication.externalMethodology.url}>
                        {publication.externalMethodology.title} (External)
                      </Link>
                    </li>
                  )}
                </ul>
              )}
            </>
          )}
        </LoadingSpinner>
      </>
    </div>
  );
};

export default PreReleaseMethodologiesPage;
