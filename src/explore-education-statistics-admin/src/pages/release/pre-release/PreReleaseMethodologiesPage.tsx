import Link from '@admin/components/Link';
import PageTitle from '@admin/components/PageTitle';
import {
  PreReleaseMethodologyRouteParams,
  preReleaseMethodologyRoute,
} from '@admin/routes/preReleaseRoutes';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import methodologyService, {
  MethodologyVersionSummary,
} from '@admin/services/methodologyService';
import publicationService, {
  ExternalMethodology,
} from '@admin/services/publicationService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tag from '@common/components/Tag';
import TagGroup from '@common/components/TagGroup';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { generatePath, RouteComponentProps } from 'react-router';

interface Model {
  externalMethodology?: ExternalMethodology;
  methodologyVersions: MethodologyVersionSummary[];
}

const PreReleaseMethodologiesPage = ({
  match,
}: RouteComponentProps<ReleaseRouteParams>) => {
  const { publicationId, releaseId } = match.params;

  const { value: model, isLoading } = useAsyncHandledRetry<Model>(async () => {
    const [externalMethodology, methodologyVersions] = await Promise.all([
      publicationService.getExternalMethodology(publicationId),
      methodologyService.listMethodologyVersions(publicationId),
    ]);

    return {
      externalMethodology,
      methodologyVersions: methodologyVersions.filter(
        methodology => methodology.status !== 'Draft' || methodology.amendment,
      ),
    };
  }, [publicationId]);

  return (
    <div className="govuk-width-container">
      <PageTitle title="Methodologies" />
      <LoadingSpinner loading={isLoading}>
        {model && (
          <>
            {model.methodologyVersions?.length === 0 &&
            !model.externalMethodology ? (
              <WarningMessage>No methodologies available.</WarningMessage>
            ) : (
              <ul className="govuk-list">
                {model.methodologyVersions.map(methodology => (
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
                        {`${methodology.title} ${
                          methodology.owned ? '(Owned)' : '(Adopted)'
                        }`}
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

                {model.externalMethodology && (
                  <li>
                    <Link to={model.externalMethodology.url}>
                      {model.externalMethodology.title} (External)
                    </Link>
                  </li>
                )}
              </ul>
            )}
          </>
        )}
      </LoadingSpinner>
    </div>
  );
};

export default PreReleaseMethodologiesPage;
