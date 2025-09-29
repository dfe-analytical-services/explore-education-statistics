import ButtonLink from '@admin/components/ButtonLink';
import Link from '@admin/components/Link';
import {
  releaseApiDataSetDetailsRoute,
  releaseApiDataSetPreviewRoute,
  releaseApiDataSetPreviewTokenRoute,
  ReleaseDataSetPreviewTokenRouteParams,
  ReleaseDataSetRouteParams,
} from '@admin/routes/releaseRoutes';
import previewTokenQueries from '@admin/queries/previewTokenQueries';
import apiDataSetQueries from '@admin/queries/apiDataSetQueries';
import previewTokenService from '@admin/services/previewTokenService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
import FormattedDate from '@common/components/FormattedDate';
import ButtonText from '@common/components/ButtonText';
import Tag from '@common/components/Tag';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { useQuery } from '@tanstack/react-query';
import { generatePath, useParams } from 'react-router-dom';
import React from 'react';

export default function ReleaseApiDataSetPreviewTokenLogPage() {
  const { dataSetId, releaseVersionId, publicationId } =
    useParams<ReleaseDataSetPreviewTokenRouteParams>();

  const { data: dataSet, isLoading: isLoadingDataSet } = useQuery(
    apiDataSetQueries.get(dataSetId),
  );

  const {
    data: previewTokens,
    isLoading: isLoadingPreviewTokens,
    refetch: refetchTokens,
  } = useQuery({
    ...previewTokenQueries.list(dataSet?.draftVersion?.id ?? ''),
    enabled: !!dataSet?.draftVersion,
  });

  const handleRevoke = async (id: string) => {
    await previewTokenService.revokePreviewToken(id);
    refetchTokens();
  };

  return (
    <>
      <Link
        back
        className="govuk-!-margin-bottom-6"
        to={generatePath<ReleaseDataSetRouteParams>(
          releaseApiDataSetDetailsRoute.path,
          {
            publicationId,
            releaseVersionId,
            dataSetId,
          },
        )}
      >
        Back to API data set details
      </Link>
      <LoadingSpinner loading={isLoadingDataSet || isLoadingPreviewTokens}>
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-three-quarters">
            <span className="govuk-caption-l">
              API data set preview token log
            </span>
            <h2>{dataSet?.title}</h2>
          </div>
        </div>

        {previewTokens?.length ? (
          <table>
            <caption>
              <VisuallyHidden>
                Table showing preview tokens for the API data set
              </VisuallyHidden>
            </caption>
            <thead>
              <tr>
                <th>Reference</th>
                <th>User</th>
                <th>Activates</th>
                <th>Status</th>
                <th>Expiry</th>
                <th className="govuk-!-text-align-right">Action</th>
              </tr>
            </thead>
            <tbody>
              {previewTokens.map(token => {
                return (
                  <tr key={token.id}>
                    <td>{token.label}</td>
                    <td>{token.createdByEmail}</td>
                    <td>
                      <FormattedDate format="d MMMM yyyy, HH:mm">
                        {token.activates}
                      </FormattedDate>
                    </td>
                    <td>
                      <Tag colour={getTokenStatusColour(token.status)}>
                        {token.status}
                      </Tag>
                    </td>
                    <td>
                      <FormattedDate format="d MMMM yyyy, HH:mm">
                        {token.expiry}
                      </FormattedDate>
                    </td>
                    <td className="govuk-!-text-align-right">
                      {(token.status === 'Active' ||
                        token.status === 'Pending') && (
                        <>
                          <Link
                            to={generatePath<ReleaseDataSetPreviewTokenRouteParams>(
                              releaseApiDataSetPreviewTokenRoute.path,
                              {
                                publicationId,
                                releaseVersionId,
                                dataSetId,
                                previewTokenId: token.id,
                              },
                            )}
                          >
                            View details
                            <VisuallyHidden> for {token.label}</VisuallyHidden>
                          </Link>
                          <ModalConfirm
                            title="Revoke preview token"
                            triggerButton={
                              <ButtonText className="govuk-!-margin-left-2">
                                Revoke
                                <VisuallyHidden> {token.label}</VisuallyHidden>
                              </ButtonText>
                            }
                            onConfirm={() => handleRevoke(token.id)}
                          >
                            <p>
                              Are you sure you want to revoke this preview
                              token?
                            </p>
                          </ModalConfirm>
                        </>
                      )}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        ) : (
          <p>No preview tokens have been created.</p>
        )}

        <ButtonLink
          to={generatePath<ReleaseDataSetRouteParams>(
            releaseApiDataSetPreviewRoute.path,
            {
              publicationId,
              releaseVersionId,
              dataSetId,
            },
          )}
        >
          Generate preview token
        </ButtonLink>
      </LoadingSpinner>
    </>
  );
}
function getTokenStatusColour(status: string) {
  switch (status) {
    case 'Active':
      return 'green';
    case 'Pending':
      return 'yellow';
    case 'Expired':
      return 'grey';
    default:
      return 'grey';
  }
}
